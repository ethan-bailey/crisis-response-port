# Recovered-source provenance audit (2026-10-27)

## Objective and verified input

The objective is to make every known difference between recovered source and browser/runtime adapters explicit before continuing downstream behavior work. The recovery input is the pinned archive SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. The public workflow first validates the downloaded recovery artifact and its extracted-file checksums; it now runs `scripts/verify_foundation_sources.py --source-root "$recovery_root"` against that exact extracted tree before generating the Mission index.

Local checks on 2026-10-27 passed both:

- `python3 scripts/verify_foundation_sources.py --archive /tmp/cpr-recovery-source.zip`
- `python3 scripts/verify_foundation_sources.py --source-root /tmp/cpr-recovery-expanded`

The original audit reported 484 hashed source identities and the pinned BehaviorDesigner plugin assembly. The dependency is now split into eight exact runtime/decompiler deltas: four task files have matching original game-source entries in the pinned archive, while Actions #141 has reproduced the four plugin decompiler outputs from the artifact-verified assembly with pinned ILSpy 9.1.0.7988. The source-backed task files (`ConditionalEvaluator`, `EntryTask`, `RandomProbability`, and `RestartBehaviorTree`) now carry exact archive paths and explicit alias insertion metadata. The verifier reconstructs each runtime file from the archived source plus only those aliases and rejects any changed body. The archived task bodies also match their recorded decompiler-body hashes. These four source mappings are **verified**; the plugin implementation ILSpy outputs were separately reproduced from the pinned artifact. Actions #141 reproduced all four plugin implementation outputs from the artifact-verified assembly using pinned ILSpy 9.1.0.7988. Generated decompiler hashes and normalized runtime-adaptation hashes are now verified independently.

New focused checks:

- `python3 -m unittest scripts.verify_foundation_sources_test` exercises alias insertion, source identity, changed-body rejection, and ambiguous marker rejection.
- `python3 scripts/verify_foundation_sources.py --archive <verified-recovery-archive.zip>` reports 484 hashed identities, four archived game-task source mappings verified with exact aliases, and the four plugin ILSpy output hashes reproduced independently of runtime adaptations.

## Recovered source versus port adapters

| Source or adapter | Recovered behavior/identity | Runtime treatment | Audit result |
|---|---|---|---|
| `FullSource/Casing.cs` | Original `Casing` body, including runtime `AddComponent<Rigidbody>()`, force/torque setup, bounce callbacks, and disable path | `Casing` is marked partial; browser sleep cleanup lives in `runtime/Engine/CasingBrowserPhysicsAdapter.cs`, separately hashed under `portAdapters` | Original body matches the pinned archive after removing the declared partial modifier |
| `FullSource/Serializer.cs` | Original serialization method | A port addition creates a missing parent directory before `File.Create`, required by the browser persistence path | Exact two-line addition is declared and stripped only for original-source hash/archive comparison |
| `FullSource/WeaponComponent.cs`, `WeaponComponentSlot.cs` | Original qualifier matching | Removed redundant early empty-string guards; `Split(..., RemoveEmptyEntries)` already returns no matches for empty input, so behavior is unchanged | Bodies now match recovered archive exactly |
| `Usage/Statistics.cs` | Original first-session usage initialization | If the client record is absent and `Load()` throws `IOException`, save the authored/default client record and continue startup | Exact first-run adapter block is declared and stripped only for source identity comparison |
| BehaviorDesigner task sources | Original archived game-task source for the four task files plus their exact compile aliases | Four task bodies map byte-for-byte from archive to runtime after the declared aliases; archive hashes equal the recorded task/decompiler source hashes | Verified by `sourceArchiveEntry`, source hash and exact adapted-runtime comparison. The missing ILSpy output text is not claimed recovered |
| BehaviorDesigner plugin implementation | Pinned plugin DLL identity plus runtime source files | Four implementation files have runtime-normalized hashes distinct from original ILSpy bytes; the pinned output hashes are now reproduced from the artifact-verified assembly | Exact pinned ILSpy output hashes and normalized runtime-adaptation hashes are checked independently; Actions #141 reproduced all four outputs |

The verifier is a consistency check, not a claim of native Unity behavior. In particular, the casing adapter preserves the recovered rule body while translating browser PhysX sleep cleanup; it does not prove Unity/PhysX parity.

## Acceptance results and next actions

- `python3 -m unittest discover -s scripts -p '*test.py'` passed on the working tree.
- `node --test scripts/*_test.mjs scripts/*_contracts.mjs` passed 164/164.
- `git diff --check` passed.
- Exact archive and extracted-source verifier commands above passed.
- Exact-source Actions #117 passed Release/WASM, complete Mission package verification/retention, full-profile IndexedDB, and independent Chromium audio on `84d7ed4c8baa9a6b867d28c601ff59524374819f`. The new provenance-workflow step is part of the next pushed SHA and must pass its own Actions run before being called CI-verified.

Next, rerun the repaired exact decompiler gate through Mission package/browser acceptance; separately trace source-to-audio-event timing for transform-driven projectiles and runtime-ejected casings. Preserve explicit pending gates for iPhone Safari, exact omitted `SpatialBlend` default, native physics, and spatial output. Do not publish from this provenance batch.

## Reproducible decompiler check — initial verifier failure and focused repair

The canonical recovery artifact contains the exact `BehaviorDesignerRuntime.dll` (assembly SHA recorded in `runtime/Original/BehaviorDesigner/source-provenance.json`). Local probing confirmed this environment has neither `dotnet` nor `ilspycmd`, so no local decompilation result is claimed. Actions #141 installed the recorded `ilspycmd 9.1.0.7988`, decompiled the verified DLL, and reproduced the four outputs (`BinaryDeserialization.cs`, `Behavior.cs`, `SharedVariable.cs`, `TaskUtility.cs`) by comparing generated bytes against each assembly-derived `decompiledSha256`, separately from normalized runtime hashes. On mismatch, the workflow retains decompiler output for diagnosis. Actions #141 reached the step but the first comparison failed because original decompiler outputs were compared directly to compatibility-modified runtime sources. The retained artifact now passes all four expected hashes locally; the corrected verifier and exact-source hosted rerun are pending.


## Actions #141 failure diagnosis and verifier repair

The run failed at step 23 (`Require exact BehaviorDesigner decompiler lineage`) after producing artifact `12728708824` (62,573 bytes; SHA-256 `ce612a7eedb984226bebbe188cf7d273cd189373dd2c2582cfe09d5378930811`). The pinned ILSpy 9.1.0.7988 output hashes match all four `sources[*].decompiledSha256` values. The files in `runtime/Original/BehaviorDesigner` intentionally contain compatibility changes, so byte equality with generated original output is the wrong assertion. The repair validates the ILSpy output hash against assembly-derived `decompiledSha256` and separately validates the normalized runtime source against `normalizedRuntimeSha256`. Local regression tests passed 6/6 (ILSpy verifier) and 8/8 (source provenance); integration against the exact retained artifact passed 4/4 files. Hosted rerun and exact Mission package/browser checks are pending.
