# Arm physics closure checkpoint — 2026-10-16

This branch introduces a **read-only source-verified audit** for the next original-code WASM dependency. It does **not** claim an arm physics implementation, compile `ArmController.cs`, modify original C# scripts, or fix the missing visible hand.

## Seven-step execution gate and present evidence

1. **Recover source arm layout and joint settings.** The unmodified developer `ArmController.cs` and `ArmsControl.cs` are available; the exact serialized per-limb `Rigidbody` and `HingeJoint` frames, connected bodies, motion limits, damping, masses and kinematic configuration are **not yet captured in the executable asset path**. Must recover original prefab hierarchy and map component fileIDs to mesh/hand variants before implementation.
2. **Implement physically correct engine bridge.** `runtime/Engine/Physics.cs` currently supplies Ray, RaycastHit, BoxCollider, ray queries; it has no implemented dynamic arm rigidbodies, force-at-position API or hinge-spring backend. Add verified PhysX dynamic body and joint support with body-space/world-space conversions, Unity acceleration-at-point semantics, target angle and spring torque. Verify with deterministic numerical tests and native Unity reference before declaring complete.
3. **Compile original `ArmController` unchanged.** Only after source `CharacterBody`, `ArmsControl`, `WeaponBase`, `Hand`, `Grenade` dependencies and real physics bridge are available. Existing `CharacterBody` references numerous original AI, physiology, input, audio and UI systems; do not add empty scaffolds solely to clear compilation.
4. **Execute native and WASM controller tests.** Exercise `Initialize`, `Update`, force application, modes, kinematic transitions, joint activation, actual `Hand.SetMode` active meshes; compare physical trajectories against native evidence. Isolated `Hand.SetMode` already passed previous tests, but **does not prove in-game visible hand geometry**.
5. **Compile original IK and sequencer.** Preserve original recovered `IKArmController`, `IKArmSequencer`, `ArmIKSolver` callbacks and source prefab frame timing. Composition is already compiled; actual playback is not.
6. **One integrated full WASM/browser verification.** Only after an executable milestone; use pinned .NET 8 + PhysX + local Chromium and WebKit screenshots and assertions. Do not run a full compilation for this audit-only branch.
7. **One on-demand preview deployment.** Only when that exact tested package passes localhost smoke, is complete and checksummed; log deployed SHA separately from verified gameplay SHA. Keep Git-triggered Vercel deployment disabled.

## Original force contract to preserve

`hand.TransformPoint(0,-0.05,0)` is the target-distance origin; flatten `delta.z=0`; `num=Clamp(magnitude,minimumForce,0.5)/0.5`; `force=delta.normalized*num*forceFactor*Time.deltaTime*60`; call `lowerArmRb.AddForceAtPosition(force,hand.TransformPoint(0,-0.1,0),ForceMode.Acceleration)`. Source `forceFactor` is initially 50 and set to 30 for `HoldWeapon`. Three joints use `spring=500`, `damper=10`, `targetPosition=current joint angle` and `useSpring`; `SetKinematic` updates all three bodies. Do not substitute an IK position snap for physical forces.

## Read-only check

```sh
python3 scripts/check_original_arm_physics_closure.py
```

The script fails if the original `ArmController.cs` changes unexpectedly, and reports the existing `Physics.cs` API surface. **Passing the audit is not compilation or gameplay acceptance**. Original asset SHA: `5b98da4624c609f57fe1ec59adbe49b5daecf8d6380a7617711aa764bb641b3c`.

## Rollback

Revert this branch's script and report only. No original runtime, game assets, CI workflows, branch protection, hosting or previously verified package were changed.
