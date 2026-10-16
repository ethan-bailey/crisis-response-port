# Source arm rig import audit checkpoint — 2026-10-16

Draft [PR #34](https://github.com/ethan-bailey/crisis-response-port/pull/34) extends the isolated PhysX prerequisite branch (`feature/26-hinge-spring-capsules`) with `scripts/audit_original_arm_rig.py`. It targets `develop/wasm` as a stacked draft; **do not merge until prerequisite history and diffs are reviewed**.

The script reads the exact recovered `source-reference/Original/ArmPhysics/prefabs.json` without modifying it and exports authored transform hierarchy, original 64-bit identity strings, capsule dimensions/material references, body mass/damping/gravity/constraints, joint anchors and connected IDs, and separate serialized enable bits for hinges, limits, springs and motors. It rejects invalid ownership/topology and records `readyForControllerPlayback: false`.

Independent read-only structural assertions against the original JSON completed in this session: Operator and Occupant, four arms, twelve bodies, original local component ownership, connected rigidbody linkage and original capsule types all passed. Python script syntax and end-to-end report against the full bundle have **not** been independently run in CI. This is not a game import or gameplay animation test.

## Ordered continuation

1. Run `python3 scripts/audit_original_arm_rig.py --output /tmp/original-arm-rig-audit.json` in a repository checkout. Compare the output against retained source data; include a focused assertion that all twelve limb records preserve their authored enable bits and hierarchy IDs. This is a cheap static test, not a WASM rebuild.
2. Derive Unity-to-PhysX joint zero-angle frames from original complete world transforms and auto-connected-anchor semantics. Do not assume local joints can be created with fabricated identity rotations.
3. Recover original layer collision matrix, PhysicMaterials, and per-body solver settings before real rig instantiation.
4. Wire engine boundary to unchanged original ArmController only after CharacterBody/ArmsControl/weapon dependency closure. Preserve the original C# behavior rather than an approximate JS controller.
5. At the first meaningful executable integration milestone run targeted source tests, then one .NET 8 WASM + pinned PhysX + Chromium/WebKit game smoke; deploy on demand only when device testing is needed.

Rollback this checkpoint by reverting only `scripts/audit_original_arm_rig.py` and this document on the feature branch; the user-confirmed working preview and live gameplay are unaffected.

Known separate work: PR #33 addresses grip/wrist/chamber presentation. Avoid modifying those files in this physics lane.

## C# compile dependency gate

`python3 scripts/check_armcontroller_compile_dependencies.py` pins original `ArmController.cs` SHA256 `5b98da4624c609f57fe1ec59adbe49b5daecf8d6380a7617711aa764bb641b3c`, checks eleven concrete original-source API dependency groups, and rejects any differing runtime copy. This script is a **static prerequisite check**, not a compilation or evidence of controller execution. It deliberately does not add the original source to the broad `runtime/Crisis.Runtime.csproj` glob until real dependent types/Unity physics services compile. CI execution remains pending. The latest user request prioritizes completion of the original C# → WASM compile, with no fabricated types.
