# Arm physics closure checkpoint — 2026-10-16

Historical engine checkpoint. The source controller/sequencer are now compiled
and selected headless callbacks pass; see
[`ORIGINAL_CHARACTER_SEQUENCER_EXECUTION.md`](ORIGINAL_CHARACTER_SEQUENCER_EXECUTION.md).
The pending items below remain relevant to full source ragdoll and native
physics acceptance, but their older “IK playback is not compiled” wording has
been superseded.

PR #30 continues the requested seven-step original-code WASM plan. It now contains recovered original physics data and an isolated real PhysX engine prerequisite. **It does not fix the missing in-game hand, compile ArmController, or provide a new playable build.**

**Verified engine milestone:** [Actions #38936091879](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38936091879) PASS at exact source `2fec10a41d65fc1f5dbf1791a020420e381e09e8`: **22 numerical contracts per browser in both Chromium and WebKit**. Private artifact `12061391992`, expires 2026-11-15; ZIP SHA256 `c264e3ca1188d213c183e2e9aa42edd22dad373a89867c8be4e3e45644d7e30a`. [Fast source/package run #38936097518](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38936097518) PASS at the same SHA. These test the isolated kernel, not original controller execution or a new playable game.

## Seven-step status

1. **Recovered:** original Operator and Occupant arm/hand/physics records from recovery ZIP SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. Four arms, 12 limb bodies and joints, all native prefab hierarchy/renderer/collider records, original settings and external asset hashes. References remain decimal strings. Dataset SHA256 `e04f915f5f3a965d050313ade3fdcb6f46a70a9262f1594456cf91c3062471ab`; archive reproduction and topology checks pass locally. No original C# modified.
2. **Partial:** `game/arm-physics-kernel.mjs` runs actual pinned PhysX 5.6.1 forces, off-center moments, kinematic transitions and revolute joints. Local Chromium passed 22 numerical checks; exact-SHA CI subsequently passed both Chromium and WebKit. The source capsule/hierarchy import, Unity hinge spring semantics, exact source joint reference frames and native Unity trajectory comparison remain unimplemented. Kernel is isolated, not imported by the game. `setSpring` throws; it never silently supplies a fake spring. The numerical fixtures use boxes, not invented game characters. `scripts/arm_physics_kernel_smoke.mjs` tests Chromium/WebKit and records each result separately.
3. **Pending:** compile unchanged original `ArmController` only after real CharacterBody/ArmsControl/WeaponBase/Grenade dependencies and real physics APIs exist. Existing weapon transport views in `Crisis.Runtime.Weapons` are not the original global CharacterBody or a legitimate dependency substitution. Do not rename those views to satisfy compiler errors. Original CharacterBody.ActiveWeapon depends on real Equipment/holster state; Kinematic comes from original state transitions.
4. **Pending:** execute actual original Initialize/Update, mode transitions, force application and visible hand activation in native and WASM runtime. Numerical kernel tests are not this acceptance gate.
5. **Pending:** unchanged original IKArmController/IKArmSequencer/ArmIKSolver execution with real callbacks. Existing composition and isolated Hand.SetMode tests are not full playback.
6. **Pending:** one meaningful integrated .NET 8 WASM + PhysX + Chromium/WebKit **game** milestone after those dependencies work. The focused engine workflow is intentionally not the full compiler/package pipeline.
7. **Pending:** deliberate deployment of that matching successful, checksummed game package. Existing stable preview still points to the previously verified muzzle-flash build. No preview staging or Vercel deploy performed for this prerequisite.

## Recovered authored facts

Both prefabs' upper/lower arm masses are 4 kg; hand mass is 1 kg. All 12 arm bodies start kinematic. Original Z hinge axes, anchors, connected body IDs, auto-configure flags, gravity/damping/constraints, limit enable flags and spring settings are preserved verbatim. Upper-arm authored limit values are -80..180 degrees; lower-arm 5..145; hand -45..45. **Not every original joint enables those limits**; never turn them on simply because limits are serialized.

The 12 Hand mode references resolve to **eight distinct GameObjects**, not 12 unique meshes. Left Trigger/Fist share an authored object; right Trigger/ForwardAssist share an authored object. Original activation state is preserved, including surprising shared references; no deduplication or guessed mesh replacement. Source-data recovery alone does not prove what the browser renders.

## Verification and provenance

```sh
python3 scripts/recover_original_arm_physics.py /path/to/crisis-response-recovery.zip
python3 scripts/verify_original_arm_physics.py --archive /path/to/crisis-response-recovery.zip
npm ci --prefix game --ignore-scripts --no-audit --no-fund
node scripts/vendor_physics.mjs
# With Playwright 1.55.0 and Chromium/WebKit installed:
node scripts/arm_physics_kernel_smoke.mjs
```

Archive reproduction byte-matches the committed dataset, not just a few counters. Python/JS syntax checks passed. Focused CI workflow `.github/workflows/arm-physics-prerequisites.yml` runs on this feature workflow change and manual dispatch only; it records exact source SHA, topology and both browser results in private `arm-physics-prerequisites-evidence` (30-day retention). Do not confuse it with the full WASM package workflow. CI run/artifact results must be checked before claiming both browsers passed.

Original ArmController SHA256 remains `5b98da4624c609f57fe1ec59adbe49b5daecf8d6380a7617711aa764bb641b3c`. Its force uses `hand.TransformPoint(0,-0.05,0)` for target delta, flattens Z, clamps range, multiplies by original force factor and `Time.deltaTime*60`, then calls `lowerArmRb.AddForceAtPosition(..., hand.TransformPoint(0,-0.1,0), Acceleration)`. Three spring targets are the **current joint angles**, stiffness 500, damping 10. No IK position snaps, synthesized animations or gameplay callbacks may replace this behavior.

## Rollback and next implementation

Revert this prerequisite commit's files; no live gameplay imports or original sources changed. Source audit from earlier PR #30 commits remains independently useful. Integration HEAD when resumed: `develop/wasm` `068fc28a83a4464b24fea7168a040a691c95c3d9`; no overlapping active physics PR found. PRs #19/#20 remain packaging/inventory references, #7 independent Unity work. Continue by implementing source capsule/hinge frames and native-reference-backed spring semantics, then close real original controller dependencies. Do not call the seven-step program finished.
