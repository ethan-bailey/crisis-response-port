# Original Character Systems — Executable Integration Boundary

This milestone uses the original recovered Unity gameplay source and AnimatorController rather than inventing replacement mechanics. **Do not misrepresent these executable pieces as full original-game parity.**

## Evidence and provenance

The pinned private recovery archive is GitHub Actions artifact `11903811738`, archive SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. The recovered developer source, exact seven Unity AnimatorController/AnimationClip YAML assets, individual SHA-256 values, and archive paths are in `source-reference/Original/CharacterExecution/provenance.json`. The complete original `CharacterBody.cs`, `Physiology.cs`, `Limb.cs`, `LimbPhysiology.cs`, `LimbMuscle.cs`, `Wound.cs`, `IKArmController.cs`, and `IKArmSequencer.cs` remain unmodified reference material.

## Executable physiology / wounds

- `runtime/Original/CharacterLimbInjury.cs` executes source limb trauma clamping, internal wound-cavity pressure accumulation, direct blood-volume loss, hemorrhage, hemorrhage stopping, and wound-flow arithmetic. It handles wound counts and preserves the source diameter/pressure relationship.
- `runtime/Original/CharacterPhysiologyConditions.cs` and `CharacterVitalDynamics.cs` execute source blood pressure, pulse, breathing rates, inhalation oxygen, work oxygen consumption, shock decay, hypoxia brain damage, brain/lung trauma, and consciousness thresholds.
- The source blood stream / particle systems, neurological reactions (`MuscleControl`, `LimbMuscle`), ballistic contact-to-injury wiring, original concussion curve and mission-death state transitions are **not yet completely connected**.
- PR #51 integrates source **VitalFunctions.HitTest** using nine real component groups, fifteen original serialized organ records, their probabilities/Unity Hermite curves, source Head/Neck/Spine/Bone/Lung/Heart behavior, and original physical capsule Y-normalization. Original projectile and LimbRenderer source references were recovered SHA-backed to `source-reference/Original/ProjectileInteractions`. Native source tests pass [Actions #38989202829](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38989202829). **No live shot-to-limb collision/wound claim:** original `Projectile.HandleLimbHit`, the skin texture `LimbRenderer.FindEntryExit` trace, ballistic-source `Limb.HandleInterior`, and vital projectile dispatch must still be integrated. Do not fake the surface trace with a simple capsule penetration length. The wound renderer boundary refuses particle mode without a real sink. This is not a substitute for the full original `Physiology.Update`.

## Executable limb / PhysX

- `runtime/Original/Weapons/CharacterLimbRagdollState.cs` adds original `Limb.SetKinematic`, `Limb.SetFloating`, original impulse buffering and `Limb.AddTrauma` / `CharacterBody.SetKinematic` / `SetFloating`.
- `runtime/Engine/PresentationHost.cs` retains real C# Rigidbody state, propagating source force, kinematic, gravity and drag operations through the existing native PhysX sink.
- `game/arm-physics-kernel.mjs`, `game/original-arm-rig-import.mjs` and `runtime/Engine/OriginalArmPhysicsHost.cs` consume original dynamic gravity/damping commands for already-imported authored arm rigidbodies.
- PR #50 merged the full original **21 source Rigidbody records / 18 HingeJoint** Operator graph into the live PhysX importer, retaining the separate already simulated contact-physics root and 20 source limb/helmet actors. All **19 Limb** components are now attached to the actual C# player alongside **17 LimbPhysiology**, **16 LimbMuscle**, and original serialized numerical settings. Verified in Chromium and WebKit on [Actions #38985858499](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38985858499).
- Remaining: source native contact callbacks, author-exact full ragdoll contact injury propagation and Unity physical trajectory parity; **a PhysX actor/joint count passing tests does not establish complete native ragdoll parity**.

## Executable AnimatorController and live pose

- `scripts/export_original_character_animator.py` parses the **recovered** `Character animator.controller` YAML at SHA-256 `d1ef99a73f2f600c09be9f099e6f88b10aff8328099621c45ce19a6c5635803b` into `game/assets/operator/character-animator-controller.json`. It asserts 4 states, 11 nonempty-condition transitions, authored fixed transition durations and blend trees.
- `game/source-character-animator.mjs` advances these conditions, state times, transition fades and authored kneel/walk-sprint blend thresholds. `game/index.html` uses the controller to blend authentic source clip curves per frame, replacing an older simplified manual locomotion selection.
- `runtime/Browser/Program.cs` connects the existing real `WeaponHost.Character` to its original `CharacterBody.UpdateAnimation` C# equations. The animation parameters therefore come from a persistent source character, not manually reimplemented browser heuristics.
- Remaining: exact Unity clip blending details, native AnimatorController interruption/footstep events under complex transitions, and exact Unity Animator.StringToHash integers (the existing parameter adapter uses stable non-native IDs). Device visual alignment requires inspection.

## Executable IK sequencer frame machine

- `runtime/Original/OriginalIKArmPlayback.cs` ports exact original `IKArmSequencer` wait, target-transition, lowered-alignment, sibling-frame gating and deferred callback order, with an adapter to the already compiled original `ArmIKSolver.TargetTransition` and `StartWait`.
- Unbound gameplay callbacks require the recovered original `IKArmController`, `Equipment`, `Grenade`, `Gauze`, wound target and weapon-holster source implementation. Do not invent callback success or enable partially bound sequences against live reload. Current verification covers real source frame semantics; **complete live IKArmController/IKArmSequencer lifecycle is still pending**.

## Validation and merge criteria

Use the one-shot `.github/workflows/character-systems-verified.yml` native source math/IK contracts and source AnimatorController Node tests. At a gameplay milestone run `.github/workflows/wasm-arm-sequences.yml` once for exact native/.NET 8 WASM package integrity plus Chromium and WebKit gameplay. Only merge passing commits; do not automatically deploy Vercel. The owner's approved proportional movement joystick must survive character-body/physics integration.

The complete class compiler graph in draft [PR #42](https://github.com/ethan-bailey/crisis-response-port/pull/42) is **not** fixed simply by compiling the methods above. Missing dependencies, e.g. real `Physiology` and `Limb` component graph, arms callbacks, mission interaction, brain effects, leg physics, still need integration before full original `CharacterBody` execution can be claimed.
