# Original gameplay continuation — CharacterBody → IK sequencer

Integration target: `develop/wasm`. Preserve the existing executable ArmController, source IK solver, hand renderer, movement, ballistics, and magazine UI. This is a **work queue**, not a declaration that a new gameplay system has been integrated.

## Next implementation slices (in dependency order)

1. **Original CharacterBody lifecycle, not a substitute.** Recover the complete CharacterBody source from the pinned private recovery artifact, preserve its SHA256 and isolate the exact Kinematic/ragdoll transitions plus required serialized Operator references. Resolve real dependent members (Torso, Pelvis, arm solvers, active/inactive equipment, grenade flags, directional pose, and physiology) against original source. Extend the current executable source slice without disguising weapon transport views as CharacterBody.
2. **IKArmSequencer original playback.** The recovered file is `source-reference/Original/Animation/IKArmSequencer.cs`. Compile the original constructor, SetSibling, Update, Play, NewFrame, TargetTransform, Invoke, and HasFinishedFrame with real `IKArmAnimationFrame`, `IKArmAction`, `IKArmType`, `IKArmTarget`, `IKControllerInvoke`, original source transforms and `ArmIKSolver`. Maintain source frame completion order: callback on following Update, then NewFrame; sibling waits use HasFinishedFrame.
3. **IKArmController original callbacks.** Source file `source-reference/Original/Animation/IKArmController.cs`. Compile Awake/Update/Play and weapon/holster/grenade transitions after their original CharacterBody/equipment prerequisites are executable. Preserve IK_ action callbacks, especially weapon activation, physical grabPoint parenting, grenade and gauze event ownership. No fake grenade/equipment behavior.
4. **Physics, physiology and contacts.** Connect CharacterBody's original non-kinematic transitions to the already executable source ArmController and PhysX arm bodies; add original layer/material filtering and contact dispatch. Compare ragdoll/hinge traces with Unity reference when obtainable.
5. **Source-based verification.** Add focused native C# tests for sequence advance, callbacks, sibling waits, switching/holstering, real ragdoll transitions; then run one integrated .NET 8 WASM + Chromium/WebKit game milestone. Preserve tested exact SHA, package evidence and explicit unsupported behavior.
6. **Private device release only on request.** Reuse verified artifact staging and the branch's stable Vercel preview alias. Actual iPhone Safari must be separately tested.

## Keep these existing implementations

- Integrated original physical ArmController — merged PR #37. Original hand rendering/source solver — merged PR #31.
- Existing source M4 ballistics, magazine, chamber, reload and muzzle effects are already working at their documented verification scope.
- No duplicate whole-repo audits, replacement game mechanics, automatic Vercel deploy or public source/artifact release.

## Immediate bounded code entry point

Start at `source-reference/Original/Animation/IKArmSequencer.cs` and `source-reference/Original/Animation/IKArmController.cs`, and locate the real recovered CharacterBody and equipment dependencies in the pinned private recovery artifact. The in-repo `runtime/Original/Weapons/ArmTransition.cs` is **selected original ArmIKSolver behavior only**; do not mistake it for the original controller/sequencer. Source method hashes and exact adapter changes belong next to the executable code.

**Implementation status of this document:** planning only; no compile, new browser behavior, or iPhone release claimed.

## Source-confirmed step 1/2 boundary (2026-10-16)

Direct inspection of `source-reference/Original/Animation/IKArmSequencer.cs` confirms these **mandatory executable dependencies** before integration:

- `CharacterBody.WeaponAimAlignment()`, `ActiveWeapon`, `holster`, `stunGrenadeAnchor`, `chestRigIKTarget`, `pistolIKTarget`, `ApplyGauzePosition(out Vector3)`, `Head`, `Pelvis`, plus `GetComponent<TeamOperator>()` for medic targets.
- `WeaponBase.leftHandAnchor/rightHandAnchor` and controller `PreviousWeapon/PendingWeapon`.
- Original `IKArmAnimationFrame`, `IKArmAction`, `IKArmType`, `IKArmTarget`, `IKControllerInvoke` definitions and the real source `ArmIKSolver` transition methods.
- Its **source timing contract**: `Play()` enters frame 0 immediately; the first `Update()` detecting completion sets `frameDone`; the following `Update()` invokes the callback before `NewFrame(++CurrentFrame)`. Sibling waits use `HasFinishedFrame()` and not an independent timer. `Transition` waits for `solver.OnTarget` and `WaitUntilLowered` waits for alignment <= 10 degrees.

The file `runtime/Original/Weapons/ArmTransition.cs` currently contains a selected `ArmIKSolver` source slice, not a real original global `CharacterBody` substitute. The original sequencer source is **recovered but not compiled in the integrated game**; adding it before resolving these original dependencies would falsely report step 2 as complete. The full recovered CharacterBody source is now present verbatim in `source-reference/Original/CharacterBody.cs` (62,958 bytes); the private recovery artifact was retrieved. This preserves source but is not executable runtime integration. No executable gameplay or engine adapter changes have been made in this checkpoint.

## PR cleanup checkpoint

The one-shot `recover-original-character-body.yml` workflow was removed once source-reference contained the recovered file. The extraction is not required for every PR or branch push. Do not claim `CharacterBody` or `IKArmSequencer` runs in game based only on preserved source. This PR is source preservation plus dependency handoff, not a completed feature.
