# Original character/arm execution gap audit — 2026-10-16

Scope: checked `source-reference/Original/Animation/{CharacterBody,IKArmController,IKArmSequencer,ArmController,ArmIKSolver}.cs` against `runtime/Original/Animation/`, `runtime/Engine/{WeaponHost,OriginalArmPhysicsHost,OriginalArmSequences,BodyPoseHost}.cs`, and live `game/{presentation,original-arm-rig-import}.mjs`. This is an *execution audit*, not a claim of native parity.

| Developer source | Recovered in repository | Live C# WASM execution | Missing / next dependency |
|---|---|---|---|
| `CharacterBody.cs` (62,958 chars) | Full reference source | **No**: compiled `Crisis.Runtime.Weapons.CharacterBody` is a limited host model, plus `BodyPoseSlice.Methods.cs` | Source-owned `Awake/Start/Update/FixedUpdate/LateUpdate`; complete movement/pose state machine; physics activation; equipment; wounds/health/death and hit events; interactions; AI/team/audio/room references |
| `ArmController.cs` | Full source | **Yes**: `runtime/Original/Animation/ArmController.cs` invoked via `OriginalArmPhysicsHost` | Activation by original `CharacterBody.SetKinematic/SetRagdoll`, source-layer contact/material/hinge trajectory parity |
| `ArmsControl.cs` | Recovered source | **Yes**: invoked in physical-host update | Authoritative ragdoll/event transitions and original game lifecycle |
| `ArmIKSolver.cs` | Recovered/compiled original IK slice | **Partial**: original IK method bodies and hand mode calls execute | Complete `IKArmController` lifecycle, sequence-owned target changes/callback timing and native orientation validation |
| `IKArmController.cs` (12,158 chars) | Full reference source | **No**: not in runtime compilation | Needs `CharacterBody` action/equipment/weapon state, `IKArmSequencer`, callbacks, animation curve, solver and grenade/gauze/holster interfaces |
| `IKArmSequencer.cs` (5,452 chars) | Full reference source | **No**: runtime only has source sequence *data classes* and serialized composition; playback deliberately disabled | `IKArmController`, solver transitions, `CharacterBody.WeaponAimAlignment`, `TeamOperator.medicPouchTransform`, target transforms and callback dispatch |
| `IKArmSequence`, `IKArmSequenceRuntime`, `IKArmAnimationFrame`, action/target/callback enums | Source classes | **Yes**: sequence composition and source enum serialization only | Wire unchanged sequencer/controller playback to actual game frame loop |
| `Hand.cs` | Recovered original | **Yes**: source `Hand.SetMode` reaches original visual quads | Exact render shader and animator transition fidelity |

## Missing lifecycle methods — source identity

`CharacterBody` reference includes `Awake`, `Start`, `Update`, `LateUpdate`, `FixedUpdate`, `UpdateHealth`, `UpdateMovement`, `UpdateAnimation`, `SetRagdoll`, `SetKinematic`, `SetFloating`, `OnDeath`, `OnWasHit`, `OnCollisionEnter`, `OnCollisionExit`, `UpdateWeapon`, `ActivateWeapon`, `HolsterWeapon`, `ThrowGrenade`, `StabilizeTrauma` and dozens of source interaction/equipment methods. **Do not interpret similarly named host methods as original full source execution.**

`IKArmController`: `Awake`, `Update`, `Play`, `InitiateGrenadeThrow`, `UpdateHoldGrenade`, `SwitchWeapons`, `HolsterWeapon`, `ReadyGrenade`, `ReturnGrenade`, `LowerCurrentWeapon`, `ResumePreviousWeapon`, `ActivateWeapon`, `EngageSelf`, `Surrender`, `Unarmed`, `Restrained`, and 12 `IK_*` source callback methods are not executing.

`IKArmSequencer`: constructor, `SetSibling`, `Update`, `Play`, `NewFrame`, `TargetTransform`, `Invoke`, `HasFinishedFrame` are not compiled into live runtime. Sources require four timing actions: Wait, Transition, WaitUntilLowered, WaitForSiblingFrame.

## Minimal dependency-first execution order

1. Define the source `IKArmSequencer` dependency graph: `IKArmController`, `ArmIKSolver`, `CharacterBody`, `WeaponBase`, `TeamOperator`, transform targets, `Time.time`, callbacks and each serialized source sequence. Identify unresolved fields **without** substituting gameplay behavior.
2. Close real C# type/field/method dependencies through recovered source and exact source-model interface surfaces; port unchanged `IKArmSequencer` methods first, with an isolated original-source playback contract.
3. Port unchanged `IKArmController` methods and source-owned action/callback routing, then bind to normal kinematic frame lifecycle.
4. Close `CharacterBody.SetKinematic`/`SetRagdoll`, `UpdateAnimation` and injury physics event dependencies and unify real PhysX arm-world contacts. Only then exercise source dynamic arm commands in actual gameplay.
5. Perform **one** complete native C#/WASM/Chromium/WebKit milestone, then deliberately stage exact verified build for iPhone.

Do not add synthesized sequencing, damage or AI logic. This audit does not assert native Unity physics trajectory fidelity or complete game systems.
