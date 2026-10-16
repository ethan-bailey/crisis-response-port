using System.Text.Json;
using Crisis.Runtime;
using UnityEngine;

public static class ArmSequenceContracts {
    static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    public static string Run(string path) {
        var json = File.ReadAllText(path);
        var world = new RuntimeWorld();
        world.Tick(.1f);
        float before = Time.time;
        var sequences = OriginalArmSequences.Load(json);
        Check(ReferenceEquals(RuntimeWorld.Current, world) && Time.time == before, "Loading diagnostic sequences must not reset game clock/world");
        Check(sequences.Count == 7, "All seven original prefabs loaded");
        Check(sequences.Values.Sum(s => s.right.Count + s.left.Count) == 21, "All 21 original frames loaded");
        var holster = sequences["Holster pistol"];
        var draw = sequences["Unholster pistol"];
        var copy = holster.Sequence();
        Check(!ReferenceEquals(copy.right, holster.right) && ReferenceEquals(copy.right[0], holster.right[0]), "Original shallow frame references and independent list preserved");
        var then = holster.Then(draw);
        Check(then.right.Count == 4 && holster.right.Count == 2 && draw.right.Count == 2, "Original Then does not mutate source lists");
        Check(then.right[1].callback == IKControllerInvoke.HolsterWeapon && then.right[2].callback == IKControllerInvoke.MovePendingWeaponToHand && then.right[3].callback == IKControllerInvoke.ActivatePendingWeapon, "Original callback ordering retained");
        Check(MathF.Abs(then.right[0].offset.x + .25f) < 1e-6f && MathF.Abs(then.right.Sum(f => f.time) - .8f) < 1e-6f, "Original transform offset and sequence timings retained");
        var combined = new IKArmSequenceRuntime();
        Check(ReferenceEquals(combined.Append(sequences["Pickup grenade"].Sequence()), combined), "Runtime Append returns same original instance");
        combined.Append(sequences["Return grenade"]);
        Check(combined.right.Count == 2 && combined.left.Count == 3 && combined.left[2].callback == IKControllerInvoke.ReturnGrenade, "Both original Append overloads preserve asymmetric arm tracks");
        var gauze = sequences["Apply gauze"];
        Check(gauze.right[3].time == 3 && gauze.right[3].callback == IKControllerInvoke.GauzeFinished, "Original gauze final callback/time retained");
        using var result = JsonDocument.Parse(OriginalArmSequences.Compose(json, "[\"Lower weapon\",\"Raise shoulder weapon\"]"));
        Check(result.RootElement.GetProperty("right").GetArrayLength() == 3 && !result.RootElement.GetProperty("playbackImplemented").GetBoolean(), "Browser transport keeps scope explicit");
        try { OriginalArmSequences.Load(json.Replace("\"action\": 0", "\"action\": 999")); throw new Exception("Invalid enum accepted"); }
        catch (InvalidDataException) { }
        return JsonSerializer.Serialize(new {status = "pass", originalClasses = 6, originalPrefabs = 7, originalFrames = 21, playbackImplemented = false});
    }
}
