using System.Text.Json;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Crisis.Runtime;
using Crisis.Runtime.Browser;
using Crisis.Runtime.FullSource;
using UnityEngine;

namespace Crisis.Runtime.Checks;

/// <summary>Exercises the authored Operator reload route through its real input and Behavior Designer scheduler.</summary>
public static class SourceBehaviorReloadContracts
{
    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static string Run(OriginalSourcePackage package, RuntimeWorld world, CharacterBody body, FullSourceActorBridge bridge)
    {
        Require(body.IsPlayer, "Reload task contract requires the source EntryTeam player Operator");
        var tree = body.GetComponent<BehaviorTree>() ?? throw new InvalidOperationException("Player Operator has no original BehaviorTree");
        Require(tree.enabled && tree.GetBehaviorSource().RootTask != null,
            $"Mission begin did not enable the deserialized player BehaviorTree (enabled={tree.enabled}, root={tree.GetBehaviorSource().RootTask?.GetType().Name})");
        var input = Global.Context.InputHandler;
        var magazineKey = input.Keys.magazine;
        for (var frame = 0; frame < 120 && !magazineKey.Enabled; frame++) bridge.Step(1f / 60f, "{}", 640f, 360f);
        Require(magazineKey.Enabled, "Original InputHandler did not enable the magazine action");

        var actions = tree.FindTasks<CharacterBehavior.ChangeMagazine>().ToArray();
        Require(actions.Length == 2 && actions.Count(task => task.isPlayer) == 1,
            $"Deserialized Operator reload graph differs from the recovered player/AI task pair (count={actions.Length}, player={actions.Count(task => task.isPlayer)})");
        var playerReload = actions.Single(task => task.isPlayer);
        var weapon = body.ActiveWeapon ?? throw new InvalidOperationException("Player Operator has no source active weapon");
        var magazine = weapon.Magazine ?? throw new InvalidOperationException("Player Operator active weapon has no source magazine");
        var roundsBefore = magazine.roundCount;
        var reservesBefore = body.Equipment.PrimaryMagazines.Count;
        var extractPath = weapon.audioExtractMagazine.sample?.OriginalAssetPath ?? string.Empty;
        var insertPath = weapon.audioInsertMagazine.sample?.OriginalAssetPath ?? string.Empty;
        var sourceAudioPaths = new List<string>();

        bridge.Step(1f / 60f, "{\"magazine\":true}", 640f, 360f);
        Require(magazineKey.Down, "Original magazine input did not produce its Down edge");
        Require(body.PendingAction == CharacterBody.CharacterAction.SwitchMagazine,
            $"PlayerController did not translate the original magazine key to SwitchMagazine (action={body.PendingAction})");

        var observedRunning = false;
        var activeTaskNames = new List<string>();
        for (var frame = 0; frame < 900; frame++)
        {
            using var capture = JsonDocument.Parse(JsonSerializer.Serialize(bridge.Step(1f / 60f, "{}", 640f, 360f)));
            if (capture.RootElement.TryGetProperty("audioEvents", out var audioEvents))
                sourceAudioPaths.AddRange(audioEvents.EnumerateArray().Select(item => item.GetProperty("sourcePath").GetString() ?? string.Empty));
            var active = tree.GetActiveTasks() ?? [];
            activeTaskNames.AddRange(active.Select(task => task.GetType().Name + ":" + task.NodeData.ExecutionStatus));
            observedRunning |= active.Contains(playerReload, ReferenceEqualityComparer.Instance) && playerReload.NodeData.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Running;
            if (observedRunning && !active.Contains(playerReload, ReferenceEqualityComparer.Instance)) break;
        }

        var roundsAfter = magazine.roundCount;
        var reservesAfter = body.Equipment.PrimaryMagazines.Count;
        Require(observedRunning,
            $"Real player Behavior Designer scheduler never ran the isPlayer ChangeMagazine action (active={JsonSerializer.Serialize(activeTaskNames.Distinct())}, pending={body.PendingAction}, enabled={tree.enabled})");
        Require(!body.IsManipulatingEquipment && body.PendingAction is CharacterBody.CharacterAction.None or CharacterBody.CharacterAction.WorkBolt,
            $"Original ChangeMagazine.OnEnd did not settle equipment state (manipulating={body.IsManipulatingEquipment}, pending={body.PendingAction})");
        Require(!string.IsNullOrEmpty(extractPath) && !string.IsNullOrEmpty(insertPath) &&
                sourceAudioPaths.Contains(extractPath) && sourceAudioPaths.Contains(insertPath),
            $"Original ChangeMagazine action did not emit the source extract/insert callbacks (extract={extractPath}, insert={insertPath}, events={JsonSerializer.Serialize(sourceAudioPaths)})");

        // Re-enter the same authored task, then interrupt it only through the
        // original PlayerController switch edge. The behavior manager must
        // observe the changed active weapon and deliver its normal OnEnd.
        var switchKey = input.Keys.switchWeapons;
        for (var frame = 0; frame < 120 && !switchKey.Enabled; frame++) bridge.Step(1f / 60f, "{}", 640f, 360f);
        Require(switchKey.Enabled, "Original InputHandler did not enable switchWeapons for reload interruption");
        bridge.Step(1f / 60f, "{\"magazine\":true}", 640f, 360f);
        Require(body.PendingAction == CharacterBody.CharacterAction.SwitchMagazine,
            $"Second original magazine press did not request the authored reload action (pending={body.PendingAction})");
        var secondReloadRunning = false;
        for (var frame = 0; frame < 240; frame++)
        {
            bridge.Step(1f / 60f, "{}", 640f, 360f);
            var active = tree.GetActiveTasks() ?? [];
            if (active.Contains(playerReload, ReferenceEqualityComparer.Instance) && playerReload.NodeData.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Running)
            {
                secondReloadRunning = true;
                break;
            }
        }
        Require(secondReloadRunning, "Original BehaviorManager did not re-enter the player ChangeMagazine action for interruption");
        var stateField = typeof(CharacterBehavior.ChangeMagazine).GetField("state", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new MissingFieldException(typeof(CharacterBehavior.ChangeMagazine).FullName, "state");
        var reloadStateAtInterrupt = stateField.GetValue(playerReload)?.ToString() ?? "<null>";
        var interruptMagazine = weapon.Magazine;
        var interruptMagazineRounds = interruptMagazine?.roundCount;
        var interruptShots = body.ShotsFired;
        bridge.Step(1f / 60f, "{\"switchWeapons\":true}", 640f, 360f);
        Require(switchKey.Down && body.armController.IsPlaying,
            "Original switchWeapons edge did not start the serialized IK sequence while the reload task was active");
        var switchFrames = 0;
        for (; switchFrames < 600; switchFrames++)
        {
            bridge.Step(1f / 60f, "{}", 640f, 360f);
            var active = tree.GetActiveTasks() ?? [];
            if (ReferenceEquals(body.ActiveWeapon, body.Equipment.secondaryWeapon) &&
                !active.Contains(playerReload, ReferenceEqualityComparer.Instance) && !body.armController.IsPlaying)
                break;
        }
        Require(switchFrames < 600 && ReferenceEquals(body.ActiveWeapon, body.Equipment.secondaryWeapon),
            $"Switch could not complete while the original reload task was active (frames={switchFrames}, active={body.ActiveWeapon?.name}, reloadStatus={playerReload.NodeData.ExecutionStatus})");
        Require(playerReload.NodeData.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Success,
            $"FullSource ChangeMagazine did not finish through its active-weapon cancellation result (status={playerReload.NodeData.ExecutionStatus})");
        Require(!body.IsManipulatingEquipment && body.PendingAction is CharacterBody.CharacterAction.None or CharacterBody.CharacterAction.WorkBolt,
            $"Scheduler OnEnd did not clear the interrupted task's source state (manipulating={body.IsManipulatingEquipment}, pending={body.PendingAction})");
        Require(body.armController.RightSequencer.IsReady && body.armController.LeftSequencer.IsReady &&
                ReferenceEquals(body.InactiveWeapon, weapon) && weapon.IsHolstered && body.ActiveWeapon.IsHeld,
            "Source IK switch did not settle after the reload task's scheduler cancellation");

        return JsonSerializer.Serialize(new
        {
            status = "passed",
            route = "original InputMapping.magazine Down → PlayerController.PendingAction → RequestedReload → BehaviorManager ChangeMagazine → original OnEnd",
            treeEnabledByMissionBegin = tree.enabled,
            playerAction = new { playerReload.ID, playerReload.FriendlyName, playerReload.quick, playerReload.isPlayer },
            actionRan = observedRunning,
            activeTaskSamples = activeTaskNames.Distinct().ToArray(),
            weapon = weapon.name,
            magazineRounds = new { before = roundsBefore, after = roundsAfter },
            reserveMagazines = new { before = reservesBefore, after = reservesAfter },
            activeMagazineInstanceChanged = !ReferenceEquals(magazine, weapon.Magazine),
            sourceAudioPaths = sourceAudioPaths.Distinct().ToArray(),
            settled = new { body.IsManipulatingEquipment, pendingAction = body.PendingAction.ToString() },
            interruption = new
            {
                sourceInput = "InputMapping.switchWeapons Down",
                reloadState = reloadStateAtInterrupt,
                magazineRoundsAtInterrupt = interruptMagazineRounds,
                magazineRoundsAfterInterruption = interruptMagazine?.roundCount,
                shotsBefore = interruptShots,
                shotsAfter = body.ShotsFired,
                taskResult = playerReload.NodeData.ExecutionStatus.ToString(),
                framesToSwitch = switchFrames,
                activeWeapon = body.ActiveWeapon.name,
                inactiveWeapon = body.InactiveWeapon.name,
                rightSequencerReady = body.armController.RightSequencer.IsReady,
                leftSequencerReady = body.armController.LeftSequencer.IsReady,
                equipmentFlagCleared = !body.IsManipulatingEquipment,
                pendingAction = body.PendingAction.ToString()
            },
            sideEffects = "Only original source input, mission-enabled Behavior Designer scheduler, source task and serialized IK ran; no task state, PendingAction, magazine, reserve or weapon slot was assigned by the contract"
        });
    }
}
