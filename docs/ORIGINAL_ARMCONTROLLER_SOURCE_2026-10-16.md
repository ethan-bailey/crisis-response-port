# Original ArmController source and physics dependencies — 2026-10-16

## Exact original game code

Private original archive: `recovery-preservation-2026-10-15`, SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. The original, unmodified `Assets/Scripts/Assembly-CSharp/ArmController.cs` is preserved byte-for-byte at `source-reference/Original/Animation/ArmController.cs` (6,037 bytes, 275 lines; SHA256 **`5b98da4624c609f57fe1ec59adbe49b5daecf8d6380a7617711aa764bb641b3c`**). Provenance: adjacent `armcontroller-provenance.json` and `scripts/verify_original_armcontroller_source.py`.

This is the developer's `ArmController`, **different from** the recovered `IKArmController`. Its `Mode` enum is `HoldWeapon=0`, `HoldGrenade=1`, `TargetTransform=2`, `RelativePosition=3`, `Free=4`. It connects `ArmsControl` and `CharacterBody` to real `Hand`/Unity physics. It has **not yet been selected for compilation**: compiling it before source-backed engine/character prerequisites exist would force invented gameplay.

## Source-exact behavior that cannot be stubbed

- `Initialize` resolves the actual upper arm, lower arm and hand `Rigidbody` plus `HingeJoint` references.
- `Update` handles the developer's arm mode state, `body.Kinematic`, real weapon hand anchors and grenade position, and checks `TargetReached` before changing the ragdoll's kinematic state.
- `StretchTowardTransform` and `StretchTowardRelativePosition` compute world-space hand/target separation, clamp the factor by `minimumForce`/`maxRange`, and call `lowerArmRb.AddForceAtPosition(..., ForceMode.Acceleration)` at the actual hand's transform. This **requires** an accurate Unity-to-PhysX dynamic force/constraint bridge.
- `EnableSpring` toggles the three real `HingeJoint` springs, assigns spring=500, damper=10 and each current joint angle as the target. Do **not** add inert `HingeJoint` types or fake `JointSpring` success just to compile.
- `SetHandMode` delegates directly to the unchanged developer `Hand.SetMode` that already executes under native C# and browser WASM in [run #38909327063](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38909327063).
- Verified methods: `Initialize`, `Update`, `SetTargetPosition`, `Relax`, `SetOnWeapon`, `SetOnTarget`, `SetOnRelativePosition`, `StretchTowardTransform`, `StretchTowardRelativePosition`, `SetKinematic`, `EnableSpring`, `SetHandMode` plus `HandMode`.

The source hash + exact methods/physics constants are fail-closed checks in the standard lightweight `pr-source-check.yml`. No additional full WASM build should be launched merely for the source-reference file.

## Next executable original-code closure

1. On a fresh feature branch from `develop/wasm`, inspect the **actual** existing JS PhysX backend and its current C# dynamic rigidbody/force forwarding; identify the original source-owned joints, constraints and transform frames. Do not equate compilation stubs with physics execution.
2. Recover original serialized arm `Rigidbody`/`HingeJoint` settings, `ArmsControl` and `CharacterBody` dependencies and check source unit/scale/time/force semantics. Build a real engine adapter with numerical and native-reference tests **before** importing the unchanged `ArmController` into the executable compile.
3. Execute the true original `ArmController.Update()` and its already selected source-original `Hand.SetMode` against actual character arm joint state, then original `IKArmController`/`IKArmSequencer` callbacks in dependency order. Actual native parity still required.
4. Use one explicit WASM+Chromium/WebKit integration run for a genuinely executable source milestone; private protected iPhone preview only for real device tests.

**Scope limitation:** this report adds verified immutable original developer source for future compilation; it does **not** implement arm physics, weapon switching, animation sequencer callbacks or 1:1 character parity. The game build at [run #38909327063](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38909327063) remains the latest tested gameplay source.

**Rollback:** revert this source-reference class, provenance report and fail-closed verifier/PR-workflow hook through a normal reviewed commit; preserve original archive, earlier implementation and site artifacts. No force pushes, main promotion, or automatic Vercel deploys.
