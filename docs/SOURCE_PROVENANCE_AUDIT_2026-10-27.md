# Recovered-source provenance audit (2026-10-27)

## Objective and verified input

The objective is to make every known difference between recovered source and browser/runtime adapters explicit before continuing downstream behavior work. The recovery input is the pinned archive SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. The public workflow first validates the downloaded recovery artifact and its extracted-file checksums; it now runs `scripts/verify_foundation_sources.py --source-root "$recovery_root"` against that exact extracted tree before generating the Mission index.

Local checks on 2026-10-27 passed both:

- `python3 scripts/verify_foundation_sources.py --archive /tmp/cpr-recovery-source.zip`
- `python3 scripts/verify_foundation_sources.py --source-root /tmp/cpr-recovery-expanded`

Each reported 484 hashed source identities and the pinned BehaviorDesigner plugin assembly. Eight decompiled BehaviorDesigner files have runtime-normalized hashes that differ from the stored decompiler-body hashes. Those eight are now enumerated with exact hashes and status in `runtime/Original/BehaviorDesigner/source-provenance.json`; the recovery artifact contains the pinned plugin assembly but not those original decompiler output bodies. The checker verifies their current runtime bytes and accepts only those exact declared delta hashes. Their decompiler lineage is therefore **explicitly unresolved**, not source-equivalent or closed.

## Recovered source versus port adapters

| Source or adapter | Recovered behavior/identity | Runtime treatment | Audit result |
|---|---|---|---|
| `FullSource/Casing.cs` | Original `Casing` body, including runtime `AddComponent<Rigidbody>()`, force/torque setup, bounce callbacks, and disable path | `Casing` is marked partial; browser sleep cleanup lives in `runtime/Engine/CasingBrowserPhysicsAdapter.cs`, separately hashed under `portAdapters` | Original body matches the pinned archive after removing the declared partial modifier |
| `FullSource/Serializer.cs` | Original serialization method | A port addition creates a missing parent directory before `File.Create`, required by the browser persistence path | Exact two-line addition is declared and stripped only for original-source hash/archive comparison |
| `FullSource/WeaponComponent.cs`, `WeaponComponentSlot.cs` | Original qualifier matching | Removed redundant early empty-string guards; `Split(..., RemoveEmptyEntries)` already returns no matches for empty input, so behavior is unchanged | Bodies now match recovered archive exactly |
| `Usage/Statistics.cs` | Original first-session usage initialization | If the client record is absent and `Load()` throws `IOException`, save the authored/default client record and continue startup | Exact first-run adapter block is declared and stripped only for source identity comparison |
| BehaviorDesigner decompiled sources | Pinned plugin DLL identity plus runtime source files | 8 source bodies differ from recorded decompiler hashes; original decompiler output text is absent from the recovery artifact | Runtime and declared delta hashes are checked; source lineage remains unresolved until the exact decompiler snapshot or reproducible decompile is recovered |

The verifier is a consistency check, not a claim of native Unity behavior. In particular, the casing adapter preserves the recovered rule body while translating browser PhysX sleep cleanup; it does not prove Unity/PhysX parity.

## Acceptance results and next actions

- `python3 -m unittest discover -s scripts -p '*test.py'` passed on the working tree.
- `node --test scripts/*_test.mjs scripts/*_contracts.mjs` passed 164/164.
- `git diff --check` passed.
- Exact archive and extracted-source verifier commands above passed.
- Exact-source Actions #117 passed Release/WASM, complete Mission package verification/retention, full-profile IndexedDB, and independent Chromium audio on `84d7ed4c8baa9a6b867d28c601ff59524374819f`. The new provenance-workflow step is part of the next pushed SHA and must pass its own Actions run before being called CI-verified.

Next, recover or reproduce the exact BehaviorDesigner decompiler outputs so the eight body deltas can be resolved; separately trace source-to-audio-event timing for transform-driven projectiles and runtime-ejected casings. Preserve explicit pending gates for iPhone Safari, exact omitted `SpatialBlend` default, native physics, and spatial output. Do not publish from this provenance batch.
