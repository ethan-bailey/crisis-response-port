# Crisis Response Port — current handoff

## Durable checkpoint

- Development branch: `develop/wasm`.
- Runtime feature checkpoint: `f0f69828015ffbccb0e932e077b8972f2dd0d192`
  (`feat: report WASM source profile memory on mobile`).
- Packaging fix: `9c1551b65a8fc302e332ff9f02d52c91c6d53403`
  (`fix: align FullSource actor package manifest path`).
- The packaging fix changes the actor sidecar path to the name expected by the
  site verifier. The actual 238-asset Operator bundle and site package tests
  passed after the fix.
- A separate owner-private diagnostic Site is published as version 1:
  https://crisis-response-memory-check-20261024.craftysnipe13.chatgpt.site
  The existing Crisis Response Runtime Site remains unchanged.
- Site package source commit:
  `1d37f7bf54c5442e6066186b330cddb224c7a8ea`, a compact Site checkout based
  on development source `9c1551b65a8fc302e332ff9f02d52c91c6d53403`.

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

Evidence: [incremental source delivery](docs/evidence/INCREMENTAL_SOURCE_DELIVERY_2026-10-25.md).
Recovered input SHA-256: `6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`.

- Global Context + Interior Mode UI + Interaction Menu + Operator + Version:
  1,230 assets; 36,971,121 compressed asset bytes; 77,806,659 raw JSON bytes.
- Above plus Mission 01: 1,919 assets; 97,767,549 compressed asset bytes;
  205,514,345 raw JSON bytes.
- Incremental Mission contract passed: Mission 01 started; EntryTeam UI
  initialized; 2 Operators and 5 Occupants spawned; FullSource actor snapshot,
  left press/release, movement force, and browser-physics state round trip
  passed. This is headless; it does not run native PhysX or render pixels.

## iPhone source memory check

Instructions and exact URL: [phone source memory check](docs/evidence/PHONE_SOURCE_MEMORY_CHECK_2026-10-25.md).

Use:
https://crisis-response-memory-check-20261024.craftysnipe13.chatgpt.site/runtime-web/?source-profile-smoke

Use the `/runtime-web/` path; `/game/` opens the visual game and does not run the memory route. The route loads the 1,230-asset Global/UI/Operator/Version profile and does not
start gameplay. The user should leave the page alone until **Complete**, copy
the JSON report, and return it here. Do not aim, shoot, or move during this
memory check.

The exact Site package passed integrity verification: 2,001 files, 199 Blazor
resources, 195 valid WASM files, the bounded Operator bundle, and the
1,230-asset incremental profile. The pinned .NET 8.0.426 browser publish
completed. Python site-package tests passed 14/14. Site version 1 is
owner-private. Playwright browser smoke and physical iPhone Safari memory
acceptance have not run; both remain open.

## One next action

Run the exact private URL above on iPhone Safari and paste its complete JSON
report into the project chat. Use that report to decide whether the 36.97 MB
pre-input profile fits on the device before wiring page movement, physics,
rendering, and weapon callbacks through the FullSource actor.
