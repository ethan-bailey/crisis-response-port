# Mission device-cache status evidence — 2026-10-26

## Scope

The Mission loader already stores immutable, compressed source assets in a CacheStorage namespace keyed by the source package hash and verifies every cache hit against manifest byte length and SHA-256. This batch makes the result observable: progress now distinguishes assets loaded from cache, fetched and cached locally, and fetched but not stored because browser storage was unavailable or rejected the write.

Changed files:

- `scene-data/scene-package-loader.mjs`: return `cachedLocally` only after CacheStorage.put succeeds; preserve network fallback if storage is unavailable.
- `game/index.html`: show cache reuse/write outcome alongside per-asset Mission progress.
- `scripts/scene_package_loader_test.mjs`: assert cache hit, successful local cache write, and corrupt-entry recovery replacement.

Source commit: `a56a898232b2624c1613d72ed5bf5192be231bd4` (parent `28a310e61ae8fec3dc170a8f7d8eeab0ad8f6acb`).

## Verification

- Focused loader and UI error-panel tests: **7 passed, 0 failed**.
- Full Node contracts/tests: **126 passed, 0 failed**, after `npm ci --prefix game --ignore-scripts` and `npm run --prefix game vendor`.
- `git diff --check`: passed.
- Local runtime: Node `v24.22.1`; the GitHub workflow uses Node 22.
- Local .NET/WASM build: not run; `dotnet` is not installed in this runner.
- The GitHub connector has not exposed a check status for the pushed source SHA. Do not mark hosted .NET/WASM verification complete until the exact-SHA run is inspected.

## Limits and next work

This is CacheStorage with browser-managed retention, not a guarantee that the browser will preserve data indefinitely. IndexedDB migration, quota estimates and user-facing quota/error handling, and a clear-cache control remain open. No real iPhone Safari run or Site publication was performed. Continue with exact-SHA hosted verification, then the version 70 crosswalk and remaining mission/storage gates.

## Superseding status — 2026-10-26

The paragraph above is a historical checkpoint. The loader now uses IndexedDB with CacheStorage fallback, storage estimate/progress, quota-safe delivery, and a version-scoped clear control. Actions run #30 passed the seven real-Chromium fixture assertions (including reload reuse, interruption/retry, corruption repair, quota-error handling, source isolation, and clear); run #33 repeated source/build and browser acceptance. Site version 71 is deployed. Full 98 MiB phone acceptance and source crosswalk to the Site's recorded source identifier remain open; see `NEXT_AGENT.md`.
