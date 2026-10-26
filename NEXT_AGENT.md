# Current handoff — 2026-10-26

## Latest verified release

- Live owner-private Site: `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.
- Site version **71** deployed successfully at `2026-10-26T03:20:08Z`: version ID `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_6c4a4c1056e081918536d1954d4048a8`; deployment ID `appgdep_6adec6d333188191a297e5ff157c996f`; status `succeeded`.
- Exact Site source commit used to build and publish: `37e6d5dcebaaba71c405d898d161b3e36e946a78` (`Front-load large mission assets during delivery`). Pushed to Site `main`. Same code/test change pushed to GitHub `develop/wasm` in commits `3c17b1115190c0530f9c4112c9d2fcc1fa1ad5a3` and `59cb7b876ad23a4aa6de5008f01fa5401d053fd6`.
- Saved Site archive record: 2,686 files, 178,012,160 stored bytes, provider archive SHA-256 `9efc3ee61c42624b6c8d078f3c4cb161b9f690656177c2fd08d7440034347040`.
- Local audited archive `/tmp/cpr-site-runtime-37e6d5d.tar.gz`: 138,805,240 bytes, SHA-256 `96aca52a65d1f7359d7205c3964b800e305581c6d9e29ed27bdc8c3baf7d1ba5`.
- Site access remains owner-only/custom; no audience setting changed.

## Version 71 changes and checks

- The bounded Mission loader now delivers the largest compressed, hash-verified assets first, with path as deterministic tie-breaker. The source manifest order and all 1,919 asset bytes/hashes are unchanged. This removes the known ordering issue where alphabetically late, multi-megabyte textures caused progress to linger near the end. It does not reduce total download bytes; device timing still needs measurement.
- Regression test uses unequal asset sizes and checks largest-first delivery while preserving base64 `.NET byte[]` transport, batch ordering, and per-asset verification.
- Node **126 passed**, Python **41 passed**. The focused scene-package-loader test passed **6/6**. Release WASM publish from `37e6d5d...` succeeded on .NET SDK 8.0.426. Optional `wasm-tools` optimization was unavailable and did not block publish.
- Exact package verifier: `status: verified`, source commit `37e6d5d...`, 2,684 payload files, 199 Blazor resources, 195 WASM files, bounded Mission profile 1,919 assets (97,767,549 compressed bytes; 205,514,345 source bytes). Build-manifest SHA-256: `ee3838eac616f4b8ab3f76644203ef3ae3ff3a778d2de0c0e1f314b6aa5e79f0`.
- Independent streaming archive audit matched every one of the **2,684/2,684** manifest files by size and SHA-256; `.openai/hosting.json` matched byte-for-byte; the only additional entry was `dist/build-manifest.json`.
- Version 70 fix remains included: the iPhone Mission-start `.NET byte[]` exception was caused by passing a `Uint8Array`; it now passes base64 text. Version 69 reload audio preload remains included, as do the version 68 absolute vertical aim and verified CacheStorage reuse.

## Remaining gates in dependency order

1. **Physical iPhone Safari acceptance of exact version 71.** Reopen the Site and start Mission. Confirm the byte-array exception is gone and Mission passes the previously reported ~1,900-asset point. Record first/repeat run duration, transfer and storage estimates; verify aim independence, sprint detent, rapid-tap zoom, and reload audio. Playwright/browser binaries and the physical iPhone are unavailable in this environment; do not claim device acceptance from server logs.
2. **Reload audio control-flow closure.** Seven original clips are preloaded before readiness and the empty-reload source events are tested. Still exercise fast/repeated reload, cancelled/interrupted reload, weapon switching, and firing synchronization against original callbacks. Preserve original clip paths and gains.
3. **Particle lifecycle parity.** Continue from `docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md` and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`. Close the remaining emitter/projectile trace, weapon smoke, blood/wound, bounce, pause/time-scale, destruction-order, active module, and Unity random-state dependencies in inventoried batches. Keep `sourceVisualParity:false` pending native comparison. The five gravity-enabled systems are outside the existing 14-prefab impact renderer.
4. **Exact missing particle texture.** Original `Spark.png` (GUID `43907091bffd53b478c985dca2762559`, SHA-256 `6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344`, 1,539 bytes) remains absent from recovered/checked-in sources. Do not substitute artwork; continue independent systems.
5. **Broader original gameplay parity.** Inventory complete dependencies, then close projectile/damage outcomes, ragdoll/joints, Mission/NPC flow, remaining animation/IK, and interaction/menu systems. A passing build alone is not parity evidence.
6. **Every release.** Push exact source to Site `main` and GitHub `develop/wasm`; build that commit; verify the bounded profile/package and archive; publish only that archive; record immutable IDs, test results, and next action here.

## Next action

Have the user retry Mission on iPhone Safari at version 71 and capture a progress report if it fails. Independently, inventory the remaining reload interruption paths from original PlayerController/ChangeMagazine/WeaponTask and add source-level checks for each path before changing audio behavior; continue particle modules that do not depend on `Spark.png`.
