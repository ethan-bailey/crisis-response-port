# Step 2: exact source dependency closure for original IKArmSequencer

This is a dependency checklist for executing **unmodified** `source-reference/Original/Animation/IKArmSequencer.cs` and `IKArmController.cs`. Do not add an alternative animation state machine.

## Source entrypoints and temporal behavior

- `IKArmController.Awake`: construct right/left `IKArmSequencer` bound to `self.rightArm` and `self.leftArm`; cross-link sibling.
- `IKArmController.Update`: run right then left sequencer, followed by `UpdateHoldGrenade`. The ordering matters for `WaitForSiblingFrame`.
- `IKArmSequencer.Play/NewFrame`: attach actual `IKArmSequenceRuntime` frame arrays, set timestamps via source `Time.time`, call source `solver.TargetTransition` for Transition else `solver.StartWait`.
- `IKArmSequencer.Update`: evaluate exact source conditions `Time.time - timestamp >= frameTime`, `solver.OnTarget`, `self.WeaponAimAlignment() <= 10f`, `sibling.HasFinishedFrame(waitForSiblingFrame)`. Callback is deferred until the following frame after `frameDone`.
- `TargetTransform`: CurrentWeapon, PreviousWeapon, PendingWeapon, ThighHolster, GrenadeAnchor, MagazinePocket, PistolIK, Wound, MedicPouch, Head, Pelvis; use original `Transform.TransformVector`, no inferred attachment points.
- `Invoke`: route only source-defined enum cases to `IKArmController.IK_*` handlers; no standalone scripted effects.

## Required type/field coverage

| Source dependency | Present now | Missing to execute source unchanged |
|---|---|---|
| `IKArmAction/Target/Type/ControllerInvoke` enums and sequence classes | All recovered and compiled | None for data representation |
| `Time.time` and `Time.deltaTime` | Host available | Source frame scheduling and sequence lifecycle must own clock |
| `ArmIKSolver.OnTarget/StartWait/TargetTransition` | Recovered IK host slice | Verify exact method signatures, source transition semantics and original `OnTarget` cadence |
| `CharacterBody.ActiveWeapon` and original `WeaponBase.leftHandAnchor/rightHandAnchor` | Partial WeaponHost implementation | Weapon swapping, previous/pending weapon ownership, original `WeaponBase` lifecycle |
| `CharacterBody.holster/stunGrenadeAnchor/chestRigIKTarget/pistolIKTarget` | Chest/pistol targets partly mapped | Full original holster/grenade transforms from Operator prefab |
| `CharacterBody.Head/Pelvis` | C# and browser transform graph partly mapped | Source `Head` binding and coordinate parity |
| `CharacterBody.ApplyGauzePosition(out Vector3)` | Source method only | Original wound target, injuries/physiology and gauze selection |
| `TeamOperator.medicPouchTransform` | Not compiled in active arm host | Recovered source `TeamOperator` behavior/field |
| `CharacterBody.WeaponAimAlignment()` | Original source method not invoked | Pose/aim state inputs, no constant substitutes |
| `IKArmController.IK_ActivatePendingWeapon/IK_HolsterWeapon/IK_FireWeapon/IK_Restrain/` etc | Source methods in reference file | Original character equipment/events/actions; targeted no-op must **not** be used |

## Initial practical integration slice

Prioritize source Wait and Transition frames with **real** weapon and pelvis/head anchors, then sibling synchronization. Keep grenade, gauze, restraint, and death/damage paths blocked until their true source dependencies resolve. Nonexisting target/field dependencies must fail explicitly in development; do not silently return invented behavior.

The 2026-10-16 combined playable preview already verifies original arm controller execution but does not verify sequencer source playback, dynamic ragdoll activation or original Unity physics solver parity.
