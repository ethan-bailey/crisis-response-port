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
