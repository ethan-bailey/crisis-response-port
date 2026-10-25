# Current handoff — 2026-10-25

## Exact shipped checkpoint

- Runtime source branch `develop/wasm` is at `f8a4be5234e294c7fb58fc5f14e037941d70014d` (verified against the live GitHub branch head). Site artifact source is `b7b1e2318e264b1fe861e3163a9913c3e8ae1781`.
- The source startup race is fixed: `App.razor` owns the single first-render `startOriginalRuntime()` call; the duplicate inline startup call was removed. `scripts/runtime_memory_check_test.mjs` guards this contract. Before this fix, two concurrent source-profile deliveries wrote the same static asset store; the UI advanced while the second `AddIncrementalSourceGzipAsset` saw an inconsistent received count (`WASM source store rejected asset ...`).
- Exact source gates: Node **88/88** (`node --test scripts/*_test.mjs`); Python **39/39** (`python3 -m unittest discover -s scripts -p '*_test.py'`); EngineChecks **69**; FoundationChecks **121**; Release browser publish succeeded; `--browser-entryteam-session-contract` passed at this SHA. Compiler reports existing warnings; no errors. `wasm-tools` optimization is optional and unavailable here.
- Mission contract evidence covers the original fire input/source actor path, source shot and authored event callbacks, source audio mapping and bounded gain, absolute aim, analog movement, and position/velocity round trip. This does not prove native browser particle/casing presentation or full gameplay parity.
- Exact package `sourceCommit` is `f8a4be5234e294c7fb58fc5f14e037941d70014d`; `build-manifest.json` SHA-256 `8b8cecbb15af5226d11f82ce9371e5d0e2675bda2f024e75b865918b2b4d3114`. It contains 2,674 manifest files, 199 Blazor resources, 195 WASM files, and the 1,919-asset `global-ui-mission-operator` profile (97,767,549 compressed bytes; 205,514,345 source bytes). `package_wasm_site.py build` and independent `verify` both passed.
- Site repository tests passed **36/36**. Site artifact commit `b7b1e2318e264b1fe861e3163a9913c3e8ae1781`; deployment archive `/tmp/cpr-site-runtime-f8a4be.tar.gz`, local SHA-256 `4e4136b1185693f692e1a1fe067648046c4179501379ae9c6505a9ec667cff14`. Archive extraction independently matched **2,674/2,674** manifest files by length and SHA-256 with zero mismatches; only expected extras were `dist/build-manifest.json` and `dist/.openai/hosting.json`. The hosting descriptor names project `appgprj_6acfef7174888191bc2bc2f269f60acc` and static directory `dist`.
- Site version **59**, `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_65e4632e53e8819182bf3cfb91836450`, deployed successfully as `appgdep_6ade54d6735c8191a3526b133d2271e3` to `https://crisis-response-runtime.craftysnipe13.chatgpt.site`. Version provenance reports artifact commit `b7b1e23`; the site remains owner-only/custom. Worker error query for the first 15 minutes after deployment returned zero events (not proof of a successful browser or iPhone session).
- The local archive's SHA is the hash of the `.tar.gz`; Sites reports normalized stored archive metadata separately (`sha256:9d5c3d1e4bc9ec71b8c0ad69ab87ba21151ca34ee1093b7ef4acdc8327e82342`, 2,676 files). Do not treat these different archive representations as a mismatch.

## Dependency-ordered remaining gates

### A — Device acceptance and delivery telemetry

1. Owner iPhone Safari retest of version 59. Open `https://crisis-response-runtime.craftysnipe13.chatgpt.site/runtime-web/?source-profile-smoke`, keep the tab foregrounded until the panel says **Complete**, tap **Copy report**, then paste the report back. No movement/aim/fire is needed for this memory-only pass. Capture the final asset count, elapsed time, sample errors, and any long tail/stall. The memory check now targets the shipped Mission profile, waits for source-store initialization before sampling, and starts delivery once.
2. Separately verify Mission usability and the delayed/too-loud gunshot on the normal page after the memory pass. Record a short screen capture and whether shot audio is immediate and bounded. Safari does not expose total process memory through every API; preserve the report's distinction between unavailable browser-wide memory and measured managed heap.
3. Use returned device telemetry to tune bounded asset fetch/concurrency/progress only if measurements identify a bottleneck. The loader already fetches in batches of six, adds verified assets in manifest order, checks both compressed and source hashes, and reports slow fetch/bridge calls. Do not raise concurrency or claim the long tail fixed without iPhone evidence.

### B — Automated visual/runtime acceptance

4. Run browser smoke on a machine with Playwright Chromium and WebGL. In this runner, the connected browser cannot create a WebGL context and Playwright's browser install returned an invalid zero-byte archive. The source/profile smoke has not been re-run in a clean browser after version 59 because the current CUA session exceeded its native message frame limit. These are concrete runner blockers; continue source work independently.

### C — FullSource parity implementation

5. Add browser presentation sinks for original particle emission and casing force callbacks; keep authored parameters, source-time lifetime, deterministic cleanup, and validate rendered output. Current Mission tests use recorders and do not prove visible effects or physical casing motion.
6. Move normal-page input, movement, physics, aim, rendering, weapon and audio authority to the same FullSource actor in dependency order. Preserve the accepted analog walk curve, rim-breakthrough sprint, absolute aim independent of joystick X, and hold-to-fire behavior; retire browser/transport duplicates only after actor contracts pass.
7. Close projectile impact, hit resolution, physiology/ragdoll and joints, NPC behavior, original animation/IK, and interaction/menu state against original source with browser/device evidence for presented effects. Keep each gate indexed with exact commit/test artifacts; do not label compile/inventory/fixture results as playability.
8. Only publish a later build after its exact source Release build, source contracts, package verification, site tests, and archive extraction audit pass on one SHA. Preserve the existing owner-only Site audience.

## Immediate next action

Wait for the owner to return the version 59 iPhone Safari memory report using the URL in A1; meanwhile implement and contract-test the first feasible FullSource presentation sink (particles/casings), then re-run all exact-source gates and update this handoff. Do not stop work at the external-device gate.
