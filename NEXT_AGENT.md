# Crisis Response Port — current handoff

## Durable checkpoint

- Development branch: `develop/wasm`.
- Exact tested source: `89901fee4fc2e083d029cad91f72f4eaa1ce8e72`
  (`test: load mission through incremental source profile`).
- This handoff is a docs-only follow-up on that source commit.
- No deployment was made. Site v39 remains the last owner-accepted iPhone Safari
  build. No PR was opened.

## Completed system: incremental source delivery through Mission UI and actor

The browser delivers each gzip asset as bytes into WASM. The package manifest
and metadata record raw and compressed sizes and SHA-256 values. C# verifies
both forms, enforces 32 MiB raw and 25 MiB compressed per-asset limits, stores
the verified compressed payload, and hydrates JSON on demand. Scene and prefab
loaders release parsed documents after use and can rehydrate them later.

The source profile generator includes `Assets/Resources/Version.prefab` in its
default roots. `Version.Instance` loads it through `Resources.Load("Version")`
during `GlobalContext.Awake`; this dynamic resource dependency is absent from
the serialized GUID closure. The mission contract accepts either a monolithic
JSON package or generated incremental profile directory, so the original
Mission UI and actors are exercised through the actual incremental C# store.

Evidence: [`docs/evidence/INCREMENTAL_SOURCE_DELIVERY_2026-10-25.md`](docs/evidence/INCREMENTAL_SOURCE_DELIVERY_2026-10-25.md).
Recovered input SHA-256: `6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`.

- Global Context + Interior Mode UI + Interaction Menu + Operator + Version:
  1,230 assets; 36,971,121 compressed asset bytes; 77,806,659 raw JSON bytes.
- Above plus Mission 01: 1,919 assets; 97,767,549 compressed asset bytes;
  205,514,345 raw JSON bytes.
- Incremental Mission contract passed: Mission 01 started; EntryTeam UI
  initialized; 2 Operators and 5 Occupants spawned; FullSource actor snapshot,
  left press/release, movement force, and browser-physics state round trip
  passed. This is headless; it does not run native PhysX or render pixels.

## Verification on `89901fee4fc2e083d029cad91f72f4eaa1ce8e72`

- Pinned .NET SDK 8.0.426 Checks build — passed, 0 errors and 48 existing
  warnings.
- `--mission-ui-incremental-contract /tmp/cpr-incremental-mission-profile-v3`
  — passed against all 1,919 assets in the incremental Mission profile.
- `--verify-incremental-source-profile /tmp/cpr-incremental-global-profile-v3`
  — passed all 1,230 assets and 7,598 records; parsed documents were released
  after inspection.
- `--source-package-store-contract` and default runtime contracts — passed.
- Python package tests — passed 14/14; `py_compile` passed.
- Node scene loader tests — passed 4/4; JavaScript syntax check passed.
- `git diff --check` — passed.
- Browser WASM build passed on the preceding source checkpoint; this batch
  changed the Checks harness and source profile roots, not Browser code.
- Real browser transfer, peak browser/WASM heap, iPhone Safari acceptance, and
  deployment were not performed.

## One next action

Run `?source-profile-smoke` on iPhone Safari with the 1,230-asset
Global/UI/Operator/Version profile and record transfer completion and peak
memory. If it fits, connect page movement, PhysX, rendering, and weapon
callbacks to the FullSource actor already exercised by the Mission profile
contract. Keep publication unchanged until device acceptance passes.
