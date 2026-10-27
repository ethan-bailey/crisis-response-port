# Recovered-source provenance audit (2026-10-27)

## Objective and verified input

The objective is to make every known difference between recovered source and browser/runtime adapters explicit before continuing downstream behavior work. The recovery input is the pinned archive SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. The public workflow first validates the downloaded recovery artifact and its extracted-file checksums; it now runs `scripts/verify_foundation_sources.py --source-root "$recovery_root"` against that exact extracted tree before generating the Mission index.

Local checks on 2026-10-27 passed both:

- `python3 scripts/verify_foundation_sources.py --archive /tmp/cpr-recovery-source.zip`
- `python3 scripts/verify_foundation_sources.py --source-root /tmp/cpr-recovery-expanded`

The original audit reported 484 hashed source identities and the pinned BehaviorDesigner plugin assembly. The dependency is now split into eight exact runtime/decompiler deltas: four task files have matching original game-source entries in the pinned archive, and four plugin implementation files still lack their decompiler output text. The source-backed task files (`ConditionalEvaluator`, `EntryTask`, `RandomProbability`, and `RestartBehaviorTree`) now carry exact archive paths and explicit alias insertion metadata. The verifier reconstructs each runtime file from the archived source plus only those aliases and rejects any changed body. The archived task bodies also match their recorded decompiler-body hashes. These four source mappings are **verified**; this does not mean the ILSpy output files were retained. The other four decompiler lineages remain **unresolved** and are accepted only by their exact declared runtime hashes.

New focused checks:

- `python3 -m unittest scripts.verify_foundation_sources_test` exercises alias insertion, source identity, changed-body rejection, and ambiguous marker rejection.
- `python3 scripts/verify_foundation_sources.py --archive <verified-recovery-archive.zip>` reports 484 hashed identities, four archived game-task source mappings verified with exact aliases, and four decompiler body hashes unresolved.

## Recovered source versus port adapters

| Source or adapter | Recovered behavior/identity | Runtime treatment | Audit result |
|---|---|---|---|
| `FullSource/Casing.cs` | Original `Casing` body, including runtime `AddComponent<Rigidbody>()`, force/torque setup, bounce callbacks, and disable path | `Casing` is marked partial; browser sleep cleanup lives in `runtime/Engine/CasingBrowserPhysicsAdapter.cs`, separately hashed under `portAdapters` | Original body matches the pinned archive after removing the declared partial modifier |
| `FullSource/Serializer.cs` | Original serialization method | A port addition creates a missing parent directory before `File.Create`, required by the browser persistence path | Exact two-line addition is declared and stripped only for original-source hash/archive comparison |
| `FullSource/WeaponComponent.cs`, `WeaponComponentSlot.cs` | Original qualifier matching | Removed redundant early empty-string guards; `Split(..., RemoveEmptyEntries)` already returns no matches for empty input, so behavior is unchanged | Bodies now match recovered archive exactly |
| `Usage/Statistics.cs` | Original first-session usage initialization | If the client record is absent and `Load()` throws `IOException`, save the authored/default client record and continue startup | Exact first-run adapter block is declared and stripped only for source identity comparison |
| BehaviorDesigner task sources | Original archived game-task source for the four task files plus their exact compile aliases | Four task bodies map byte-for-byte from archive to runtime after the declared aliases; archive hashes equal the recorded task/decompiler source hashes | Verified by `sourceArchiveEntry`, source hash and exact adapted-runtime comparison. The missing ILSpy output text is not claimed recovered |
| BehaviorDesigner plugin implementation | Pinned plugin DLL identity plus runtime source files | Four implementation files have runtime-normalized hashes different from the recorded decompiler hashes, and their original ILSpy output text is absent | Runtime and declared delta hashes are checked; exact implementation-source lineage remains unresolved pending source text or a reproducible decompile |

The verifier is a consistency check, not a claim of native Unity behavior. In particular, the casing adapter preserves the recovered rule body while translating browser PhysX sleep cleanup; it does not prove Unity/PhysX parity.

## Acceptance results and next actions

- `python3 -m unittest discover -s scripts -p '*test.py'` passed on the working tree.
- `node --test scripts/*_test.mjs scripts/*_contracts.mjs` passed 164/164.
- `git diff --check` passed.
- Exact archive and extracted-source verifier commands above passed.
- Exact-source Actions #117 passed Release/WASM, complete Mission package verification/retention, full-profile IndexedDB, and independent Chromium audio on `84d7ed4c8baa9a6b867d28c601ff59524374819f`. The new provenance-workflow step is part of the next pushed SHA and must pass its own Actions run before being called CI-verified.

Next, recover or reproduce the exact BehaviorDesigner decompiler outputs for the four still-unresolved plugin implementation files; separately trace source-to-audio-event timing for transform-driven projectiles and runtime-ejected casings. Preserve explicit pending gates for iPhone Safari, exact omitted `SpatialBlend` default, native physics, and spatial output. Do not publish from this provenance batch.
