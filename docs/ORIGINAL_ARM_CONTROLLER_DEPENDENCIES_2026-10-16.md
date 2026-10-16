# Original arm-controller playback — source-preserving dependency checkpoint

2026-10-16, [issue #22](https://github.com/ethan-bailey/crisis-response-port/issues/22), `feature/22-arm-controller`. This is a **verified source recovery and native compile prerequisite**, NOT an animation/controller playback implementation.

## Recovered from the original game, no inferred game mechanics

Source: private release `recovery-preservation-2026-10-15`, `crisis-response-recovery-direct-v3-1.zip`, exactly 101,082,158 bytes, SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. GitHub Actions verified the archive SHA, extracted untouched source directly from `export/ExportedProject/Assets/Scripts/Assembly-CSharp/`, and checked exact source hashes.

| Original class | SHA256 | Bytes | State |
| --- | --- | ---: | --- |
| `IKArmType.cs` | `bc0801f3b205f09da8a2fb1f108d34f23e25f29090c77b28fa1e86731bcb589d` | 48 | **Real unchanged original enum** added to `runtime/Original/Animation/` (selected by SDK glob). Original values `Right=0`, `Left=1`. |
| `IKArmSequencer.cs` | `29f48bdb6446fd560dae5a05f235af5701b8a484401392fb5528ec696ac59610` | 5452 | Preserved original source in `source-reference/Original/Animation/`, deliberately **not compiled** until real dependencies exist. |
| `IKArmController.cs` | `848283e9b0a3e25ce1650529b1ac57a0bebb8f5b600cada1ad33d56a3fa2ec6e` | 12158 | Preserved only, uncompiled. |
| `ArmIKSolver.cs` | `ddc4291b1ab5f73e74ddb65b8df67232bff2afddd1a591403e8aceed13a9c5dc` | 7585 | Preserved only, uncompiled. |
| `CharacterBody.cs` | `76301d45e6e94556d1a902f0d37f5f97111bc2efb89f36de7ed9acd935b55742` | 62958 | Preserved only (2,454 lines), uncompiled. |

The 4 source-reference `.cs` files are outside `runtime/Crisis.Runtime.csproj`'s `Original/**/*.cs` glob. Their full original bytes and dependency classification are tracked in `source-reference/Original/Animation/source-provenance.json`. `scripts/preserve_original_arm_controller_sources.py` re-extracts/checks original bytes; `scripts/verify_original_arm_controller_references.py` fails closed if any originals are modified, omitted, misclassified or if playback is falsely claimed. No placeholder implementations were generated.

## Genuine original call/dependency boundaries

Recovered `IKArmSequencer.Update()`, verified against original bytes:
- `Wait`: `Time.time - timestamp >= frameTime`
- `Transition`: `solver.OnTarget`
- `WaitUntilLowered`: `self.WeaponAimAlignment() <= 10f`
- `WaitForSiblingFrame`: `sibling.HasFinishedFrame(waitForSiblingFrame)`
- Its callback fires only in the **next Update** after `frameDone` becomes true. Then it advances to the next frame via `NewFrame(++CurrentFrame)`. This ordering must not be replaced by time-only tween scheduling.
- `NewFrame` resolves real `WeaponBase` hand anchors, holster, medical bag, pelvis/head and grenade anchors and calls the original `ArmIKSolver.TargetTransition`; cannot run correctly without original `CharacterBody`, weapon/limb components and actual Unity transforms.
- `IKArmController` dispatches the genuine `IK_*` weapon, gauze, grenade, restrain and firing callbacks into `CharacterBody`, `WeaponBase`, `TeamOperator`. Each is a real game action; no-op callback interfaces would change original gameplay.
- `ArmIKSolver` depends on real `Limb`, `Hand`, `Joint`, transforms, and original `InverseKinematics2D` (the latter has **already** been compiled in the existing runtime and used by presentation).
- `CharacterBody` is a 62,958-byte recovered original class with numerous AI, weapon, physics, physiology and `BehaviorDesigner.Runtime` dependencies. Its full closure requires further source-backed batches rather than importing a made-up body class.

## Checks and evidence

- Original source discovery [Actions #38905825119](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38905825119) **SUCCESS**, source-only artifact `12048026575` (expires after 14 days).
- Extended actual original arm-side enum/source review [Actions #38906143951](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38906143951) **SUCCESS**.
- Private source preservation [Actions #38906498130](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38906498130) **SUCCESS**, source preservation commit `7679a690856e53273165b8e6606fc23d6d3d99fb`, report artifact `12047449611` (14-day retention). Four untouched developer source files and a source manifest were committed, no runtime placeholder added.
- Native source/hash + selected original C# contract workflow: [Actions #38906790931](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38906790931), must check terminal state before claiming success. No full WebAssembly package/browser run initiated for this bounded prerequisite.
- Previous 41 selected original C# classes / seven prefabs / 21 frames: verified [#38901664939](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38901664939) at `1264c0b4579076b82e1b6b21955469313779fe39`. Added `IKArmType` is an additional selected original class; **do not claim 42 source files compiled in WASM or browser-tested** without subsequent matching WASM evidence.

## Next executable dependency step

1. Starting from current `develop/wasm`, compare this branch and inspect `Hand`, `Limb`, `Joint`, `WeaponBase` and `TeamOperator` original types and original scene/prefab references. Record source SHA and actual Unity API obstacles, then choose the smallest executable subset of original code.
2. Incorporate the real `CharacterBody` behavior required for weapon/hand/IK callbacks without inventing new character systems; if unbounded, prioritize an independently executable original source module and document why.
3. Only after the source closure compiles and runs, wire the original controller to a representative Holster/Unholster or Lower/Raise original prefab sequence, preserve the callback timing and verify real source behavior in native .NET/C# and WebAssembly in Chromium/WebKit. Compare against actual Unity reference where possible. No rendering/improvised tween shortcut.
4. Merge only the reviewed, verified source batch to `develop/wasm` via a PR. The four full source-reference blobs can be moved unchanged into `runtime/Original/` once all compile dependencies are genuinely satisfied. Root `vercel.json` keeps `git.deploymentEnabled=false`, with no public game deployment.

**Rollback:** selectively revert the exact `IKArmType.cs` addition, its native contract and source-reference files only after inspecting downstream dependencies. Preserve historical developer source commits and never force-reset shared `develop/wasm` or `main`.
