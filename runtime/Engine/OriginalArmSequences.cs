using System.Text.Json;
using UnityEngine;

namespace Crisis.Runtime;

// Preserves recovered frame data and binds it to the unchanged original controller.
// Browser rendering/playback remains a separate, unimplemented boundary.
public static class OriginalArmSequences {
    public static Dictionary<string, IKArmSequence> Load(string json) {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.GetProperty("schema").GetInt32() != 1 ||
            root.GetProperty("scriptGuid").GetString() != "c7d54f5ebb1ade51a48c2fef96bf390f" ||
            root.GetProperty("archiveSha256").GetString() != "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d" ||
            root.GetProperty("playbackImplemented").GetBoolean())
            throw new InvalidDataException("Unsupported original arm-sequence bundle");
        // Separate world prevents diagnostic composition from resetting live game time/input.
        var world = new RuntimeWorld(makeCurrent: false, resetClock: false);
        var result = new Dictionary<string, IKArmSequence>(StringComparer.Ordinal);
        foreach (var item in root.GetProperty("sequences").EnumerateArray()) {
            var name = item.GetProperty("name").GetString();
            var sequence = new GameObject(name, world).AddComponent<IKArmSequence>();
            sequence.right = ReadFrames(item.GetProperty("right"));
            sequence.left = ReadFrames(item.GetProperty("left"));
            result.Add(name, sequence);
        }
        return result;
    }

    static T EnumValue<T>(JsonElement value) where T : struct, Enum {
        var n = value.GetInt32();
        if (!Enum.IsDefined(typeof(T), n)) throw new InvalidDataException("Unknown original arm enum");
        return (T)Enum.ToObject(typeof(T), n);
    }

    static float Number(JsonElement value) {
        var n = value.GetSingle();
        if (!float.IsFinite(n)) throw new InvalidDataException("Nonfinite original arm value");
        return n;
    }

    static List<IKArmAnimationFrame> ReadFrames(JsonElement frames) {
        var result = new List<IKArmAnimationFrame>();
        foreach (var f in frames.EnumerateArray()) {
            var offset = f.GetProperty("offset");
            result.Add(new IKArmAnimationFrame {
                name = f.GetProperty("name").GetString(),
                action = EnumValue<IKArmAction>(f.GetProperty("action")),
                transform = EnumValue<IKArmTarget>(f.GetProperty("transform")),
                offset = new Vector3(Number(offset.GetProperty("x")), Number(offset.GetProperty("y")), Number(offset.GetProperty("z"))),
                time = Number(f.GetProperty("time")),
                callback = EnumValue<IKControllerInvoke>(f.GetProperty("callback")),
                waitForSiblingFrame = f.GetProperty("waitForSiblingFrame").GetInt32()
            });
        }
        return result;
    }

    public static void BindAuthoredSequences(
        Crisis.Runtime.FullSource.IKArmController controller,
        IReadOnlyDictionary<string, IKArmSequence> sequences) {
        ArgumentNullException.ThrowIfNull(controller);
        ArgumentNullException.ThrowIfNull(sequences);
        var owner = controller.GetComponent<Crisis.Runtime.FullSource.CharacterBody>();
        Crisis.Runtime.FullSource.OriginalActorIdentity.RequireController(owner, controller);

        IKArmSequence Required(string name) => sequences.TryGetValue(name, out var sequence) && sequence != null
            ? sequence
            : throw new InvalidDataException($"Missing recovered IK sequence: {name}");

        // Resolve the full set before mutation so a partial bundle cannot leave
        // the source controller in a mixed authored/default state.
        var holsterPistol = Required("Holster pistol");
        var unholsterPistol = Required("Unholster pistol");
        var lowerShoulderWeapon = Required("Lower weapon");
        var returnShoulderWeapon = Required("Raise shoulder weapon");
        var pickupGrenade = Required("Pickup grenade");
        var returnGrenade = Required("Return grenade");
        var applyGauze = Required("Apply gauze");

        // These names map one-to-one to the source Operator's serialized fields.
        controller.holsterPistol = holsterPistol;
        controller.unholsterPistol = unholsterPistol;
        controller.lowerShoulderWeapon = lowerShoulderWeapon;
        controller.returnShoulderWeapon = returnShoulderWeapon;
        controller.pickupGrenade = pickupGrenade;
        controller.returnGrenade = returnGrenade;
        controller.applyGauze = applyGauze;
    }

    public static string Compose(string bundle, string namesJson) {
        var source = Load(bundle);
        var names = JsonSerializer.Deserialize<string[]>(namesJson);
        if (names == null || names.Length == 0) throw new ArgumentException("At least one original sequence required");
        var runtime = source[names[0]].Sequence();
        foreach (var name in names.Skip(1)) runtime.Append(source[name]);
        object Frame(IKArmAnimationFrame f) => new {
            f.name, action = (int)f.action, transform = (int)f.transform,
            offset = new[] {f.offset.x, f.offset.y, f.offset.z}, f.time,
            callback = (int)f.callback, f.waitForSiblingFrame
        };
        return JsonSerializer.Serialize(new {
            names, right = runtime.right.Select(Frame), left = runtime.left.Select(Frame),
            playbackImplemented = false,
            scope = "Original IKArmSequence and IKArmSequenceRuntime composition; no controller playback or gameplay callbacks"
        });
    }
}
