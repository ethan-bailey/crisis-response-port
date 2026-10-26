using System.Text.Json;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Crisis.Runtime;
using Crisis.Runtime.FullSource;
using UnityEngine;

namespace Crisis.Runtime.Checks;

/// <summary>Runs the serialized nonplayer empty-weapon switch route in its live Mission BehaviorManager.</summary>
public static class SourceAIWeaponFallbackContracts
{
    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    public static string Run(RuntimeWorld world, CharacterBody body)
    {
        Require(body != null && body.GetComponent<TeamOperator>() != null && !body.IsPlayer,
            "AI fallback requires a nonplayer Operator created by the original Mission team path");
        var tree = body.GetComponent<BehaviorTree>() ?? throw new InvalidOperationException("AI Operator has no original BehaviorTree");
        var source = tree.GetBehaviorSource();
        source.Initialize(tree);
        source.CheckForSerialization(force: true);
        var operatorTrees = world.Objects.OfType<CharacterBody>().Where(candidate => candidate.GetComponent<TeamOperator>() != null)
            .Select(candidate => new { actor = candidate.name, candidate.IsOperator, candidate.IsPlayer, tree = candidate.GetComponent<BehaviorTree>()?.enabled, root = candidate.GetComponent<BehaviorTree>()?.GetBehaviorSource().RootTask?.GetType().Name }).ToArray();
        Require(tree.GetBehaviorSource().RootTask != null,
            $"AI Operator has no deserialized source tree root (missionStarted={world.Objects.OfType<Mission>().Single().Started}, enabled={tree.enabled}, operators={JsonSerializer.Serialize(operatorTrees)})");
        var missionEnabledTree = tree.enabled;
        Require(missionEnabledTree,
            $"Original Mission.BeginMission did not enable the spawned AI Operator tree (actor={body.name}, isOperator={body.IsOperator}, isPlayer={body.IsPlayer}, trees={JsonSerializer.Serialize(operatorTrees)})");
        var conditions = tree.FindTasks<CharacterBehavior.SwitchWeaponCondition>();
        var actions = tree.FindTasks<CharacterBehavior.SwitchWeapon>();
        Require(conditions.Count == 1 && actions.Count == 1,
            $"Recovered AI tree should have one authored switch fallback pair (conditions={conditions.Count}, actions={actions.Count})");
        var condition = conditions[0];
        var action = actions[0];

        var primary = body.Equipment?.primaryWeapon ?? throw new InvalidOperationException("AI Operator has no source primary weapon");
        var secondary = body.Equipment?.secondaryWeapon ?? throw new InvalidOperationException("AI Operator has no source secondary weapon");
        Require(primary.name == "M4A1" && secondary.name == "Glock-18" && ReferenceEquals(body.ActiveWeapon, primary),
            $"AI source default loadout is not M4A1→Glock-18 (primary={primary.name}, secondary={secondary.name}, active={body.ActiveWeapon?.name})");
        Require(primary.Magazine != null && secondary.Magazine != null && secondary.Magazine.roundCount > 0,
            "AI weapon fallback lacks a source magazine on either weapon or rounds in the Glock");
        var primaryAmmoBefore = body.Equipment.PrimaryMagazines.Sum(magazine => magazine.roundCount) + body.Equipment.PrimaryRounds.count;
        var secondaryRoundsBefore = secondary.Magazine.roundCount;
        var shotsBefore = body.ShotsFired;
        world.PhysicsForceSink ??= (_, _, _, _) => { };
        world.PhysicsTorqueSink ??= (_, _, _) => { };
        foreach (var other in world.Objects.OfType<BehaviorTree>())
            if (!ReferenceEquals(other, tree)) other.enabled = false;
        for (var warmupFrame = 0; warmupFrame < 8; warmupFrame++)
            world.Tick(1f / 60f, () =>
            {
                foreach (var behaviorTree in world.Objects.OfType<BehaviorTree>())
                    if (!ReferenceEquals(behaviorTree, tree)) behaviorTree.enabled = false;
                foreach (var vocalization in world.Objects.OfType<UIVocalizationText>()) vocalization.enabled = false;
                foreach (var fog in world.Objects.OfType<FogOfWarElement>()) fog.enabled = false;
                foreach (var controller in world.Objects.OfType<PlayerController>()) controller.enabled = false;
            });
        Require(ReferenceEquals(body.ActiveWeapon, primary) && body.PendingAction == CharacterBody.CharacterAction.None && !body.armController.IsPlaying,
            $"AI switched from the still-loaded M4A1 (active={body.ActiveWeapon?.name}, pending={body.PendingAction}, playing={body.armController.IsPlaying})");

        // Stage only a real source no-ammunition state. This calls the original
        // Magazine and CyclingBolt owners; it does not set task or actor state.
        foreach (var magazine in body.Equipment.PrimaryMagazines.ToArray())
            while (magazine.RemoveOne(manualFeed: true)) { }
        if (primary.cyclingBolt.Round != null) primary.cyclingBolt.EjectCasing(forceScale: 0f);
        var otherUsableWeapon = body.Equipment.GetNonEmptyWeapon();
        Require(ReferenceEquals(body.ActiveWeapon, primary) && primary.ChamberPermanentlyEmpty && primary.Magazine.Empty &&
                !body.Equipment.HasAmmunitionForWeapon(primary) && ReferenceEquals(otherUsableWeapon, secondary) &&
                secondary.Magazine is { Empty: false },
            $"Source empty-weapon precondition failed (chamberEmpty={primary.ChamberPermanentlyEmpty}, magazineEmpty={primary.Magazine.Empty}, primaryAmmo={body.Equipment.HasAmmunitionForWeapon(primary)}, fallback={otherUsableWeapon?.name}, secondaryRounds={secondary.Magazine.roundCount})");
        Require(body.PendingAction == CharacterBody.CharacterAction.None,
            $"AI fallback fixture entered with a pending action: {body.PendingAction}");

        var activeSamples = new HashSet<string>(StringComparer.Ordinal);
        var conditionSucceeded = false;
        var actionRan = false;
        var activeWeaponChanged = false;
        var frames = 0;
        for (; frames < 600; frames++)
        {
            world.Tick(1f / 60f, () =>
            {
                foreach (var behaviorTree in world.Objects.OfType<BehaviorTree>())
                    if (!ReferenceEquals(behaviorTree, tree)) behaviorTree.enabled = false;
                foreach (var vocalization in world.Objects.OfType<UIVocalizationText>()) vocalization.enabled = false;
                foreach (var fog in world.Objects.OfType<FogOfWarElement>()) fog.enabled = false;
                foreach (var controller in world.Objects.OfType<PlayerController>()) controller.enabled = false;
            });
            var active = tree.GetActiveTasks() ?? [];
            foreach (var task in active)
            {
                activeSamples.Add(task.GetType().Name + "#" + task.ID + ":" + task.NodeData.ExecutionStatus);
                if (task is CharacterBehavior.SwitchWeaponCondition)
                    conditionSucceeded |= task.NodeData.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Success;
                if (task is CharacterBehavior.SwitchWeapon)
                {
                    actionRan |= task.NodeData.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Running;
                }
            }
            activeWeaponChanged |= ReferenceEquals(body.ActiveWeapon, secondary);
            if (activeWeaponChanged && !active.Any(task => task is CharacterBehavior.SwitchWeapon) && !body.armController.IsPlaying)
                break;
        }

        Require(frames < 600 && actionRan && activeWeaponChanged,
            $"Live AI scheduler did not execute the source fallback path (frames={frames}, condition={condition.NodeData.ExecutionStatus}, action={action.NodeData.ExecutionStatus}, actionRan={actionRan}, active={body.ActiveWeapon?.name}, tasks={JsonSerializer.Serialize(activeSamples)})");
        Require(body.PendingAction == CharacterBody.CharacterAction.None && ReferenceEquals(body.InactiveWeapon, primary) &&
                primary.IsHolstered && secondary.IsHeld && body.armController.RightSequencer.IsReady && body.armController.LeftSequencer.IsReady,
            $"Source AI swap failed to settle or clear OnEnd state (pending={body.PendingAction}, active={body.ActiveWeapon?.name}, inactive={body.InactiveWeapon?.name}, primaryHolstered={primary.IsHolstered}, secondaryHeld={secondary.IsHeld}, rightReady={body.armController.RightSequencer.IsReady}, leftReady={body.armController.LeftSequencer.IsReady})");
        Require(body.ShotsFired == shotsBefore && secondary.Magazine.roundCount == secondaryRoundsBefore,
            "AI weapon fallback changed shots or the Glock magazine");

        foreach (var magazine in body.Equipment.SecondaryMagazines.ToArray())
            while (magazine.RemoveOne(manualFeed: true)) { }
        if (secondary.cyclingBolt.Round != null) secondary.cyclingBolt.EjectCasing(forceScale: 0f);
        Require(secondary.Magazine.Empty && secondary.ChamberPermanentlyEmpty &&
                !body.Equipment.HasAmmunitionForWeapon(secondary) && body.Equipment.GetNonEmptyWeapon() == null,
            $"No-alternative negative fixture is not genuinely empty (magazineEmpty={secondary.Magazine.Empty}, chamberEmpty={secondary.ChamberPermanentlyEmpty}, hasAmmo={body.Equipment.HasAmmunitionForWeapon(secondary)}, alternative={body.Equipment.GetNonEmptyWeapon()?.name})");
        var switchedWithoutAlternative = false;
        for (var negativeFrame = 0; negativeFrame < 120; negativeFrame++)
        {
            world.Tick(1f / 60f, () =>
            {
                foreach (var behaviorTree in world.Objects.OfType<BehaviorTree>())
                    if (!ReferenceEquals(behaviorTree, tree)) behaviorTree.enabled = false;
                foreach (var vocalization in world.Objects.OfType<UIVocalizationText>()) vocalization.enabled = false;
                foreach (var fog in world.Objects.OfType<FogOfWarElement>()) fog.enabled = false;
                foreach (var controller in world.Objects.OfType<PlayerController>()) controller.enabled = false;
            });
            var active = tree.GetActiveTasks() ?? [];
            switchedWithoutAlternative |= active.Any(task => task is CharacterBehavior.SwitchWeapon && task.NodeData.ExecutionStatus == BehaviorDesigner.Runtime.Tasks.TaskStatus.Running);
        }
        Require(!switchedWithoutAlternative && ReferenceEquals(body.ActiveWeapon, secondary) &&
                body.PendingAction == CharacterBody.CharacterAction.None && !body.armController.IsPlaying,
            $"AI attempted a weapon swap with no usable alternative (switched={switchedWithoutAlternative}, active={body.ActiveWeapon?.name}, pending={body.PendingAction}, playing={body.armController.IsPlaying})");

        return JsonSerializer.Serialize(new
        {
            status = "passed",
            route = "serialized SwitchWeaponCondition → BehaviorManager SwitchWeapon → original IKArmController.SwitchWeapons → SwitchWeapon.OnEnd",
            treeEnabledByMissionBegin = missionEnabledTree,
            missionActorClassification = new { body.IsOperator, body.IsPlayer },
            condition = new { condition.ID, condition.FriendlyName, result = condition.NodeData.ExecutionStatus.ToString() },
            action = new { action.ID, action.FriendlyName, result = action.NodeData.ExecutionStatus.ToString() },
            ai = new { actor = body.name, isPlayer = body.IsPlayer },
            stagedSourceState = new { activeWeapon = primary.name, primaryAmmoBefore, primaryAmmoAfter = body.Equipment.PrimaryMagazines.Sum(magazine => magazine.roundCount) + body.Equipment.PrimaryRounds.count, chamberPermanentlyEmpty = primary.ChamberPermanentlyEmpty, otherUsableWeapon = secondary.name, secondaryRounds = secondaryRoundsBefore },
            result = new { activeWeapon = body.ActiveWeapon.name, inactiveWeapon = body.InactiveWeapon.name, frames, shots = body.ShotsFired, secondaryRounds = secondary.Magazine.roundCount, pendingAction = body.PendingAction.ToString(), sequencersReady = body.armController.RightSequencer.IsReady && body.armController.LeftSequencer.IsReady },
            activeTaskSamples = activeSamples.ToArray(),
            conditionSuccessObservedDuringActiveSnapshot = conditionSucceeded,
            sequenceProgressEvidence = "serialized direct sibling Sequence contains condition then action; source action became Running and switched the active weapon, so Sequence advanced past its condition",
            negativeCases = new[] { "M4A1 still loaded: AI stayed on M4A1", "M4A1 and Glock-18 empty: no fallback action or swap" },
            physics = "source casing ejection force sink is a headless no-op; native casing physics is not claimed"
        });
    }

}
