using System.Text.Json;
using Crisis.Runtime.Browser;
using Crisis.Runtime.FullSource;
using UnityEngine;

namespace Crisis.Runtime.Checks;

/// <summary>
/// Exercises the recovered Operator loadout, serialized IK sequence references,
/// original PlayerController input edge, and original IKArmController callbacks.
/// It deliberately does not fabricate weapon slots or sequence frames.
/// </summary>
public static class SourceWeaponSwitchContracts
{
    static readonly (string Name, string Guid, string Path, string Sha256)[] Sequences =
    [
        ("Holster pistol", "def346a0b435e414f91d1f240c7d016e", "Assets/GameObject/Holster pistol.prefab", "2f4aa20255d361a2725879c2d6df2cd4f15ab6c7d6c2306efba40f24909c2ee3"),
        ("Unholster pistol", "afe828104b427ef47b9ac7ce4e2e1b70", "Assets/GameObject/Unholster pistol.prefab", "c0356970e9828e981b4d0d4b23bfbc9499d6544a4994b748a31012489e7c06de"),
        ("Lower weapon", "77a7fc0e3d201394cb8d8ddd031df902", "Assets/GameObject/Lower weapon.prefab", "741bad97110ccea44742705dee120bf970e6efe6cb60d93a31547edf185c6567"),
        ("Raise shoulder weapon", "95f92e749f4341449a76c5052f911111", "Assets/GameObject/Raise shoulder weapon.prefab", "6e9525f3b896c162d8290de64335a8b63056cad499d25d0701da2d8119e0645e"),
        ("Pickup grenade", "572f009943cda614aa21d5dbd0dbbbea", "Assets/GameObject/Pickup grenade.prefab", "c87b99b28d24aa2ab4af89f02a5420ef2f8374c824368010c35a3c2b9237d4e7"),
        ("Return grenade", "67a7d386af05bf346b569e4f4dfaa594", "Assets/GameObject/Return grenade.prefab", "fd9e0565ce4f977670dfef0421e5a18aa9c7271a652d45ad2d5fd8a69ecaeb89"),
        ("Apply gauze", "0b10c0f14d5ea4b41ae81ab8b177805f", "Assets/GameObject/Apply gauze.prefab", "7262845b5531f5e770b5cab1e7072f1299bc4faee8d84a1cd1b01cb37de87bfb")
    ];

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    static void VerifySequence(OriginalSourcePackage package, IKArmSequence sequence, string name, string guid, string path, string sha256)
    {
        Require(sequence != null, $"Operator IKArmController is missing serialized {name} reference");
        var sourceAsset = package.GetAsset(path);
        var originalBytesSha256 = sourceAsset.GetProperty("sha256").GetString();
        Require(sequence.OriginalAssetGuid == guid && sequence.OriginalAssetPath == path && originalBytesSha256 == sha256,
            $"Operator {name} sequence identity mismatch (guid={sequence.OriginalAssetGuid}, path={sequence.OriginalAssetPath}, sourceSha={originalBytesSha256}, expectedSourceSha={sha256})");
    }

    static object Switch(
        RuntimeWorld world,
        FullSourceActorBridge bridge,
        CharacterBody body,
        WeaponBase from,
        WeaponBase to,
        string expectedAction,
        Func<CharacterBody, WeaponBase, WeaponBase, bool> extraState,
        string[] expectedEvents)
    {
        var input = Global.Context.InputHandler;
        var controller = body.armController;
        var switchKey = input.Keys.switchWeapons;
        for (int frame = 0; frame < 120 && !switchKey.Enabled; frame++)
            bridge.Step(1f / 60f, "{}", 640f, 360f);
        Require(switchKey.Enabled, "Original InputHandler has disabled the authored switchWeapons action");
        Require(ReferenceEquals(body.ActiveWeapon, from) && ReferenceEquals(body.InactiveWeapon, to),
            $"Unexpected source weapon slots before {expectedAction} (active={body.ActiveWeapon?.name}, inactive={body.InactiveWeapon?.name})");

        var events = new List<string>();
        bool wasLowering = body.IsLoweringWeapon;
        bool wasFromHolstered = from.IsHolstered;
        bool wasToHeld = to.IsHeld;
        bool wasToInHand = ReferenceEquals(to.transform.parent, body.rightArm.hand.grabPoint);

        bridge.Step(1f / 60f, "{\"switchWeapons\":true}", 640f, 360f);
        Require(switchKey.Down, $"Original {expectedAction} input did not create a source Down edge");
        bridge.Step(1f / 60f, "{}", 640f, 360f);
        Require(controller.IsPlaying, $"Original PlayerController did not start the authored {expectedAction} IK sequence");

        const int maxFrames = 600;
        int frames = 0;
        for (; frames < maxFrames; frames++)
        {
            bridge.Step(1f / 60f, "{}", 640f, 360f);
            if (!wasLowering && body.IsLoweringWeapon) events.Add(expectedAction == "M4A1-to-Glock" ? "lower-primary" : "lower-weapon");
            if (!wasFromHolstered && from.IsHolstered) events.Add("holster-" + from.name);
            bool isToInHand = ReferenceEquals(to.transform.parent, body.rightArm.hand.grabPoint);
            if (!wasToInHand && isToInHand) events.Add("move-" + to.name + "-to-hand");
            if (ReferenceEquals(body.ActiveWeapon, to) && !wasToHeld) events.Add("activate-" + to.name);

            wasLowering = body.IsLoweringWeapon;
            wasFromHolstered = from.IsHolstered;
            wasToHeld = to.IsHeld;
            wasToInHand = isToInHand;
            if (ReferenceEquals(body.ActiveWeapon, to) && !controller.IsPlaying) break;
        }

        Require(frames < maxFrames,
            $"Original {expectedAction} sequence did not settle (active={body.ActiveWeapon?.name}, playing={controller.IsPlaying}, rightFrame={controller.RightFrame}, leftFrame={controller.LeftFrame}, rightTarget={body.rightArm.OnTarget}, leftTarget={body.leftArm.OnTarget})");
        Require(events.SequenceEqual(expectedEvents),
            $"Original {expectedAction} callbacks were out of order: {JsonSerializer.Serialize(events)}; expected {JsonSerializer.Serialize(expectedEvents)}");
        Require(ReferenceEquals(body.ActiveWeapon, to) && ReferenceEquals(body.InactiveWeapon, from) && from.IsHolstered && to.IsHeld,
            $"Original {expectedAction} did not settle the source active/inactive and held/holstered weapon states");
        Require(!controller.IsPlaying && controller.RightSequencer.IsReady && controller.LeftSequencer.IsReady,
            $"Original {expectedAction} left an IK sequencer active");
        Require(extraState(body, from, to), $"Original {expectedAction} changed source equipment or magazine state");

        return new { direction = expectedAction, action = "InputMapping.switchWeapons Down → PlayerController.Update → IKArmController.SwitchWeapons", frames, callbacks = events, activeWeapon = body.ActiveWeapon.name, inactiveWeapon = body.InactiveWeapon.name, sequencersSettled = true };
    }

    public static string Run(OriginalSourcePackage package, RuntimeWorld world, CharacterBody body, FullSourceActorBridge bridge)
    {
        world.MakeCurrent();
        var controller = body.armController ?? throw new InvalidOperationException("Original Operator has no IKArmController");
        var equipment = body.Equipment ?? throw new InvalidOperationException("Original Operator has no source Equipment");
        var primary = equipment.primaryWeapon ?? throw new InvalidOperationException("Default Operator primary M4A1 failed to load");
        var secondary = equipment.secondaryWeapon ?? throw new InvalidOperationException("Default Operator secondary Glock-18 failed to load");
        Require(primary.name == "M4A1" && secondary.name == "Glock-18", $"Unexpected source Operator loadout ({primary.name}, {secondary.name})");
        Require(ReferenceEquals(primary.Owner, body) && ReferenceEquals(secondary.Owner, body), "Source weapons are not owned by the original Operator");
        Require(primary.Magazine != null && secondary.Magazine != null, "Source M4A1/Glock magazine graph did not initialize");
        Require(primary.ammunition?.OriginalAssetPath == "Assets/Resources/weapons/ammunition/5.56-FMJ.prefab" &&
                secondary.ammunition?.OriginalAssetPath == "Assets/Resources/weapons/ammunition/9-FMJ.prefab",
            $"Source ammunition graph mismatch (M4={primary.ammunition?.OriginalAssetPath}, Glock={secondary.ammunition?.OriginalAssetPath})");

        VerifySequence(package, controller.holsterPistol, Sequences[0].Name, Sequences[0].Guid, Sequences[0].Path, Sequences[0].Sha256);
        VerifySequence(package, controller.unholsterPistol, Sequences[1].Name, Sequences[1].Guid, Sequences[1].Path, Sequences[1].Sha256);
        VerifySequence(package, controller.lowerShoulderWeapon, Sequences[2].Name, Sequences[2].Guid, Sequences[2].Path, Sequences[2].Sha256);
        VerifySequence(package, controller.returnShoulderWeapon, Sequences[3].Name, Sequences[3].Guid, Sequences[3].Path, Sequences[3].Sha256);
        VerifySequence(package, controller.pickupGrenade, Sequences[4].Name, Sequences[4].Guid, Sequences[4].Path, Sequences[4].Sha256);
        VerifySequence(package, controller.returnGrenade, Sequences[5].Name, Sequences[5].Guid, Sequences[5].Path, Sequences[5].Sha256);
        VerifySequence(package, controller.applyGauze, Sequences[6].Name, Sequences[6].Guid, Sequences[6].Path, Sequences[6].Sha256);

        Require(controller.lowerShoulderWeapon.right.Count == 2 &&
                controller.lowerShoulderWeapon.right[0].callback == IKControllerInvoke.LowerWeapon &&
                controller.lowerShoulderWeapon.right[1].callback == IKControllerInvoke.HolsterWeapon,
            "Original Lower weapon frames/callbacks were not bound from the serialized Operator prefab");
        Require(controller.unholsterPistol.right.Count == 2 &&
                controller.unholsterPistol.right[0].callback == IKControllerInvoke.MovePendingWeaponToHand &&
                controller.unholsterPistol.right[1].callback == IKControllerInvoke.ActivatePendingWeapon,
            "Original Unholster pistol frames/callbacks were not bound from the serialized Operator prefab");
        Require(controller.holsterPistol.right.Count == 2 && controller.holsterPistol.right[1].callback == IKControllerInvoke.HolsterWeapon &&
                controller.returnShoulderWeapon.right.Count == 1 && controller.returnShoulderWeapon.right[0].callback == IKControllerInvoke.ActivatePendingWeapon,
            "Original pistol holster/shoulder return callbacks were not bound from the serialized Operator prefab");

        bridge.SetAimWorldPoint(5f, 2.5f, 0f);
        for (int frame = 0; frame < 300 && (!body.rightArm.OnTarget || !body.leftArm.OnTarget); frame++)
            bridge.Step(1f / 60f, "{}", 640f, 360f);
        Require(body.rightArm.OnTarget && body.leftArm.OnTarget, "Original Operator arm solvers did not settle before switch input");
        Require(ReferenceEquals(body.ActiveWeapon, primary) && ReferenceEquals(body.InactiveWeapon, secondary), "Default source loadout did not begin M4A1 active / Glock inactive");

        int primaryRounds = primary.Magazine.roundCount;
        int secondaryRounds = secondary.Magazine.roundCount;
        int shotsBefore = body.ShotsFired;
        var forward = Switch(world, bridge, body, primary, secondary, "M4A1-to-Glock", (actor, from, to) =>
            ReferenceEquals(from.Magazine, primary.Magazine) && ReferenceEquals(to.Magazine, secondary.Magazine) &&
            primary.Magazine.roundCount == primaryRounds && secondary.Magazine.roundCount == secondaryRounds && actor.ShotsFired == shotsBefore,
            ["lower-primary", "holster-M4A1", "move-Glock-18-to-hand", "activate-Glock-18"]);
        var reverse = Switch(world, bridge, body, secondary, primary, "Glock-to-M4A1", (actor, from, to) =>
            ReferenceEquals(from.Magazine, secondary.Magazine) && ReferenceEquals(to.Magazine, primary.Magazine) &&
            primary.Magazine.roundCount == primaryRounds && secondary.Magazine.roundCount == secondaryRounds && actor.ShotsFired == shotsBefore,
            ["holster-Glock-18", "activate-M4A1"]);

        return JsonSerializer.Serialize(new
        {
            status = "passed",
            actor = body.name,
            primary = new { primary.name, asset = primary.OriginalAssetPath, ammunition = primary.ammunition.OriginalAssetPath, magazine = primary.Magazine.roundCount, capacity = primary.Magazine.capacity },
            secondary = new { secondary.name, asset = secondary.OriginalAssetPath, ammunition = secondary.ammunition.OriginalAssetPath, magazine = secondary.Magazine.roundCount, capacity = secondary.Magazine.capacity },
            authoredSequences = Sequences.Select(sequence => new { sequence.Name, sequence.Guid, sequence.Path, sequence.Sha256 }).ToArray(),
            forward,
            reverse,
            shotsUnchanged = body.ShotsFired == shotsBefore,
            scope = "original source Operator loadout, serialized IK sequence bindings, PlayerController input edges, and IKArmController callbacks; reload interruption remains a separate task-system integration gate"
        });
    }
}
