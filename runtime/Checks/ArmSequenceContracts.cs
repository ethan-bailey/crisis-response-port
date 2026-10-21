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
        Check((int)IKArmType.Right == 0 && (int)IKArmType.Left == 1 && Enum.GetValues<IKArmType>().Length == 2,
            "Exact recovered original IKArmType arm-side ordering changed");
        using var handProof = JsonDocument.Parse(OriginalHandModes.Run());
        var handRoot = handProof.RootElement;
        Check(handRoot.GetProperty("originalHandMethodExecuted").GetBoolean() &&
              !handRoot.GetProperty("ikControllerPlaybackImplemented").GetBoolean(),
              "Actual original Hand.SetMode must execute without claiming IK playback");
        var handStates = handRoot.GetProperty("states").EnumerateArray().ToArray();
        Check(handStates.Length == 3, "Three original hand modes must execute");
        Check(handStates[0].GetProperty("mode").GetInt32() == 1 && handStates[0].GetProperty("fist").GetBoolean() &&
              !handStates[0].GetProperty("trigger").GetBoolean() && !handStates[0].GetProperty("forwardAssist").GetBoolean(),
              "Original fist-hand mode activation changed");
        Check(handStates[1].GetProperty("mode").GetInt32() == 2 && !handStates[1].GetProperty("fist").GetBoolean() &&
              !handStates[1].GetProperty("trigger").GetBoolean() && handStates[1].GetProperty("forwardAssist").GetBoolean(),
              "Original forward-assist mode activation changed");
        Check(handStates[2].GetProperty("mode").GetInt32() == 0 && !handStates[2].GetProperty("fist").GetBoolean() &&
              handStates[2].GetProperty("trigger").GetBoolean() && !handStates[2].GetProperty("forwardAssist").GetBoolean(),
              "Original trigger-hand mode activation changed");
        Check(ReferenceEquals(RuntimeWorld.Current, world) && Time.time == before,
              "Hand source contract must not reset current game world or time");
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
        using var controllerProof = JsonDocument.Parse(FullSourceControllerPlaybackContracts.Run(world));
        var controllerRoot = controllerProof.RootElement;
        Check(controllerRoot.GetProperty("status").GetString() == "pass" &&
              controllerRoot.GetProperty("callbackObserved").GetBoolean() &&
              controllerRoot.GetProperty("actorIsolation").GetBoolean(),
              "Original IKArmController sequencer must play on FullSource.CharacterBody and dispatch callbacks to its owning actor");
        return JsonSerializer.Serialize(new {
            status = "pass", originalClasses = 6, originalPrefabs = 7, originalFrames = 21,
            fullSourceControllerPlaybackImplemented = true,
            browserSequenceCompositionPlaybackImplemented = false,
            originalHandModeExecuted = true
        });
    }
}


public static class FullSourceControllerPlaybackContracts {
    public sealed class PlaybackBodyFixture : Crisis.Runtime.FullSource.CharacterBody {
        public PlaybackBodyFixture() { }
        public override void Awake() { }
    }

    private static (GameObject root, PlaybackBodyFixture body, Crisis.Runtime.FullSource.IKArmController controller)
        CreateActor(RuntimeWorld world, string name) {
        var root = new GameObject(name, world);
        root.SetActive(false);

        var body = root.AddComponent<PlaybackBodyFixture>();
        body.enabled = false;

        // The Wait frame exercises only the original sequencer's timing and
        // callback path. Solver instances are deliberately not scene components
        // so unrelated physical rig initialization is outside this contract.
        body.rightArm = (Crisis.Runtime.FullSource.ArmIKSolver)
            System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(
                typeof(Crisis.Runtime.FullSource.ArmIKSolver));
        body.leftArm = (Crisis.Runtime.FullSource.ArmIKSolver)
            System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(
                typeof(Crisis.Runtime.FullSource.ArmIKSolver));

        var controller = root.AddComponent<Crisis.Runtime.FullSource.IKArmController>();
        body.armController = controller;
        root.SetActive(true);
        world.Tick(0.01f);

        if (controller.RightSequencer == null || controller.LeftSequencer == null)
            throw new InvalidOperationException("Original IKArmController did not initialize both sequencers on its FullSource actor");
        return (root, body, controller);
    }

    public static string Run(RuntimeWorld world) {
        var first = CreateActor(world, "FullSource playback actor A");
        var second = CreateActor(world, "FullSource playback actor B");
        try {
            if (ReferenceEquals(first.controller, second.controller) ||
                ReferenceEquals(first.controller.RightSequencer, second.controller.RightSequencer) ||
                !ReferenceEquals(first.body.armController, first.controller) ||
                !ReferenceEquals(second.body.armController, second.controller))
                throw new InvalidOperationException("FullSource actors do not own independent original IK controllers and sequencers");

            var sequence = new IKArmSequenceRuntime();
            sequence.right.Add(new IKArmAnimationFrame {
                action = IKArmAction.Wait,
                time = 0.01f,
                callback = IKControllerInvoke.LowerWeapon
            });
            first.controller.Play(sequence);

            // The first tick completes the Wait frame; the next invokes its
            // source callback and advances the sequencer to completion.
            world.Tick(0.05f);
            world.Tick(0.05f);

            var callbackObserved = first.body.IsLoweringWeapon &&
                first.controller.RightSequencer.IsReady;
            var actorIsolation = !second.body.IsLoweringWeapon &&
                second.controller.RightSequencer.IsReady;
            if (!callbackObserved || !actorIsolation)
                throw new InvalidOperationException("Original LowerWeapon callback did not play on, and remain isolated to, its FullSource actor");

            return JsonSerializer.Serialize(new {
                status = "pass",
                authoritativeActorType = typeof(Crisis.Runtime.FullSource.CharacterBody).FullName,
                callback = nameof(IKControllerInvoke.LowerWeapon),
                callbackObserved,
                actorIsolation,
                independentControllers = true,
                sequencerAdvancedToCompletion = true
            });
        }
        finally {
            UnityEngine.Object.DestroyImmediate(first.root);
            UnityEngine.Object.DestroyImmediate(second.root);
        }
    }
}
