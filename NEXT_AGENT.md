# Current handoff — 2026-10-26

## SDK recovery and exact-SHA verification

- Correction: `dotnet` was not on the default `PATH`, but the pinned SDK was present at `/tmp/codex-dotnet-8.0.426` with `/tmp/libprocshim.so`. After applying the runbook environment, `dotnet --version` reported **8.0.426**. The runbook now requires checking these known paths before recording an SDK blocker.
- Clean detached worktree at source SHA `5693194aef4d614a99764c8683c0c344a15ebe1b`: runtime checks passed; EngineChecks 75 passed; FoundationChecks passed; Release build passed (48 warnings, 0 errors); Release WASM publish and output assertions passed. Optional wasm-tools optimization was unavailable. Evidence: [pinned SDK recovery](docs/evidence/REA-pinned-sdk-recovery-2026-10-26.md).
- Local Node suite passed 127/127 against the equivalent source tree. The GitHub connector returns no hosted check status for the push SHA. Physical device acceptance and visual/runtime parity are not implied by build success.

## Latest implementation batch

- Source commit `5693194aef4d614a99764c8683c0c344a15ebe1b` adds a clear control scoped to the currently verified Mission manifest cache. It reports clear success, absence, unsupported storage, or errors; regression coverage checks that the versioned Mission cache is the only cache deleted. Evidence: [REA mission cache controls](docs/evidence/REA-mission-cache-controls-2026-10-26.md).
- Focused loader/UI tests passed 8/8; full Node suite passed 127/127. Local .NET/WASM build and hosted exact-SHA status are not verified. CacheStorage eviction, quota handling, IndexedDB, iPhone Safari repeat-load measurement, and Site version 70 crosswalk remain open. No Site was published.
- Next action: run/inspect exact-SHA CI for `5693194aef4d614a99764c8683c0c344a15ebe1b`, then close quota-aware storage and crosswalk work before any release.

## Latest implementation batch

- Source commit `a56a898232b2624c1613d72ed5bf5192be231bd4` reports whether each Mission asset was read from the verified device cache, fetched and successfully cached locally, or fetched without a successful local cache write. The regression suite covers these states plus corrupt-cache recovery. Evidence: [REA mission device-cache status](docs/evidence/REA-mission-device-cache-status-2026-10-26.md).
- Local focused tests passed 7/7 and the full Node suite passed 126/126 after the locked npm install/vendor step. Local .NET/WASM build was not run (`dotnet` unavailable); exact-SHA hosted check status is not exposed by the available connector. No Site was published.
- Next action: inspect the hosted CI run for `a56a898232b2624c1613d72ed5bf5192be231bd4`; continue with quota-aware storage/clear-cache behavior and the version 70 source crosswalk. Physical iPhone acceptance remains pending.

## Latest implementation batch

- Commit `522b8e0063e3dbf976d6568b224bb7a72b09f7e4` recovers a corrupt cached Mission asset by deleting that entry and fetching only that asset again; the replacement is cached only after manifest size/SHA verification. The regression and local checks are recorded in [REA mission cache recovery evidence](docs/evidence/REA-mission-cache-recovery-2026-10-26.md). Full Node suite passed 126/126 after the pinned npm dependency/vendor setup. Local .NET/WASM build was not run because `dotnet` is unavailable. The GitHub connector did not expose a check status for the push SHA, so hosted verification remains unconfirmed.

## Latest verified release

- Live owner-private Site: `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.
- Site version **70** deployed successfully at `2026-10-26T02:08:28Z`: version ID `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_7def6122c1488191a5f0235918cd425d`; deployment ID `appgdep_6adeb613e1648191a121453d2476eae4`; status `succeeded`.
- Exact Site source commit used to build and publish: `d65cdb2fbd1c50f55fbd5c7ab96eb12a242ca684` (`Encode mission asset batches for .NET byte interop`). Pushed to Site `main` before build. GitHub `develop/wasm` contains the same code/test change in commits `dc5b4f1ef666d6186ace580df0ce2349b892bf5c` and `df7aad51045b1e03537ab7a3`; the handoff sync is documentation-only.
- Site saved archive record: 2,686 files, 177,981,440 stored bytes; provider archive SHA-256 `6035973fcd57e39d61a14d58c7133ab9b25a56db953c7424125fcb2666e03293`.
- Local audited archive `/tmp/cpr-site-runtime-d65cdb2.tar.gz`: 138,782,955 bytes, SHA-256 `a8603142e5591fa6c1b767aa03a1992391d74441e382dda45df159f9b89c7fda`.
- Site preserved owner-only/custom access. No audience settings changed.

## Version 70 fix and exact verification

- The iPhone Mission-start exception was traced to `AddIncrementalSourceGzipAssetBatch`: JavaScript passed a `Uint8Array` as the `byte[]` Blazor argument, while the .NET 8 JS interop converter expects base64 text. The batch path now calls the existing bounded `bytesToBase64` helper. Regression tests assert the argument is a string and decodes to the exact compressed bytes, for one-asset and multi-asset batches. This targets the `ByteArrayJsonConverter.Read` exception from the iPhone screenshots.
- Release WASM publish from exact Site commit `d65cdb2...` succeeded with .NET SDK 8.0.426. Optional `wasm-tools` optimization workload is unavailable; publish completed without it.
- Node: **126 passed, 0 failed** (`node --test scripts/*_test.mjs scripts/*_contracts.mjs`). Python: **41 passed** (`python3 -m unittest discover -s scripts -p '*test.py'`). EngineChecks: **75 passed**. FoundationChecks: **121 passed**. Runtime/Checks passed original source/prefab, actor, presentation, weapon/reload, and ballistic contracts.
- Exact package verifier: `status: verified`; 2,684 payload files, 199 Blazor resources, 195 WASM files, bounded Mission profile 1,919 assets (97,767,549 compressed bytes; 205,514,345 source bytes). Build-manifest SHA-256: `0def3084593dbe5cae4064530c3d0296eecbe68223e6c6c26efd1b378d53f1d4`.
- Independent streaming archive audit matched sizes and SHA-256 for all **2,684/2,684** manifest files, matched `.openai/hosting.json` byte-for-byte, and found only the expected `dist/build-manifest.json` extra.

## Included previous release changes

- Version 69 (`Preload original weapon and reload audio before play`, source `0542735282607dee06504a8887cc8a8d8486ebe6`) awaits loading/decoding of the seven original weapon clips before readiness. The empty M4 reload contract verifies magazine-extract, magazine-insert, and bolt-return events; automated coverage verifies source clip mappings and readiness ordering. Version 69 test counts were Node 126, Python 41, EngineChecks 75, FoundationChecks 121, plus Runtime/Checks.
- Version 68 made joystick vertical aim absolute and independent of horizontal movement; Mission source delivery now reuses compressed assets from device-local CacheStorage with manifest hash/length checks and network fallback.
- Mobile zoom guard and the larger rim-break sprint joystick are present in the current source. Device behavior still requires iPhone Safari acceptance.

## Remaining gates in dependency order

1. **Physical iPhone Safari acceptance of exact version 70.** Confirm Start Mission no longer throws the byte-array interop exception and the load gets beyond the earlier ~1,900-asset hang. Record first-run/repeat-run transfer and timing plus storage estimates; verify cached reload, absolute vertical aim, horizontal movement independence, walk/sprint detent, rapid-tap zoom behavior, and reload audio timing. Playwright/browser binaries and a physical iPhone are unavailable in this environment; do not mark device acceptance passed without it.
2. **Reload audio control-flow closure.** Preloading and empty-reload callbacks are covered. Verify fast/repeated reload, cancelled reload, empty reload, weapon switch/interruption, and synchronization under rapid firing against original callbacks. Preserve original clip paths/gains; do not add guessed sounds.
3. **Particle lifecycle parity.** Continue from `docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md` and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`. Inventory each remaining implementation slice and close owner/callback dependencies for emitter/projectile trace, weapon smoke, blood/wounds, bounce, pause/time-scale, and destruction ordering; then active Collision, Clamp Velocity, Inherit Velocity, Force, Rotation, Size, Color, Burst, and Unity random-state behavior. Keep `sourceVisualParity:false` pending native comparison. The five gravity-enabled systems are outside the current 14-prefab impact renderer; do not claim them handled by adding gravity to the impact-only path.
4. **Exact missing particle texture.** Original `Spark.png` (GUID `43907091bffd53b478c985dca2762559`, SHA-256 `6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344`, 1,539 bytes) is absent from recovered and checked-in sources. Do not substitute artwork; continue independent particle modules and record exact source/package location if recovered.
5. **Broader original gameplay parity.** Inventory complete dependencies before each batch, then close projectile/damage outcomes, ragdoll/joints, authored Mission/NPC flow, remaining animation/IK, and interaction/menu systems. Runtime/build success is not itself parity evidence.
6. **Every release.** Push exact source to Site `main` and GitHub `develop/wasm`; build from that exact Site commit; regenerate or reuse only the exact verified bounded profile; run package and archive integrity checks; publish only the audited archive; update this handoff with immutable IDs, tests, and the next concrete action.

## Next action

Create the file-level source-to-Site crosswalk for deployed version 70 against public develop/wasm SHA df7aad51045b1e03537ab7a3a49334122eb40831. Record the Site SHA, GitHub SHA, exact file hashes, and archive/manifest evidence before another Site release. Then continue the remaining reload event-timing and particle-lifecycle batches; keep physical iPhone acceptance of version 70 explicitly pending.
