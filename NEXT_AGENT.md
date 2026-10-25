# Current handoff — 2026-10-25

## Exact casing implementation checkpoint

- Tested and pushed source commit: `399c829078c605e8575e1b9877e113f703857174` on GitHub `develop/wasm`, with tree `3db13f5a5bb13aaf4ead17d06d5269be8692ab70`. The fully tested local source commit `908bbeedbea76d185d5ba192eb329b2c4ccf3882` has the identical Git tree. The regular HTTPS push lacked credentials; the authenticated GitHub write path published the exact tested tree. Rebuild the package under the pushed SHA before deployment.
- Python: `python3 -m unittest discover -s scripts -p '*_test.py'` — **40 passed**.
- Node: `node --test scripts/*_test.mjs` — **93 passed**.
- .NET 8.0.426 Release build — **0 errors, 48 existing warnings**. The real `--browser-entryteam-session-contract` passed with one casing force, one torque, original body ID/effective mass (0.002 kg), and finite source position/rotation/linear/angular velocity round trip. EngineChecks **69 passed**; FoundationChecks **121 passed**.
- Release Browser publish at `/tmp/cpr-publish-908bbee-2/wwwroot` succeeded from the same source tree.
- Package built and independently verified from the identical tested tree at local source `908bbeedbea76d185d5ba192eb329b2c4ccf3882`; manifest SHA-256 `5699bdf641357fcb18600912487370da5105788faca953fd4d70ca30121bee36`, **2,679 files**, 199 Blazor resources, 195 WASM files, and 1,919 incremental Mission assets (97,767,549 compressed / 205,514,345 uncompressed source bytes). This is not the final deployable package: rebuild and independently verify it against pushed source `399c829078c605e8575e1b9877e113f703857174`.
- Exact browser PhysX smoke remains unrun: vendored PhysX is compiled for WebAssembly and fails under Node; no Playwright browser binary is installed. `scripts/original_casing_physics_smoke.mjs` records the Web runner command and assertions. The adapter contract runs in Node with fakes, not real PhysX.
- No deployment has been made from this batch. The Site is owner-private, version 61. The Site's Cloudflare artifact source repository is separate from GitHub `develop/wasm`; preserve both source identities and publish only when the Site version is tied to the pushed/packaged source. Do not broaden access.

## Dependency inventory and gate order

For each gate, inventory the full source dependency closure before implementation. Record original GUIDs, prefab/object graph, serialized component fields, referenced materials/textures/audio/curves, runtime ordering/lifetime, and the executable source contract. Implement and verify the closure as a batch; use runtime failures only to identify genuinely hidden edges.

1. **Rebuild and release this casing closure.** Publish the Release Browser output from pushed GitHub SHA `399c829078c605e8575e1b9877e113f703857174`; rebuild/verify the Site package from that SHA; commit the static archive to the Site's Cloudflare source repository `main` and deploy privately. Run real-browser PhysX trajectory/collision/settle smoke when a WebAssembly/WebGL browser runner is available. Do not call the Node adapter contract a real-engine smoke.
2. **Original particle rendering.** Inventory all Mission-used particle systems from serialized prefab/component graph through modules, sub-emitter references, meshes, materials, textures, source time, emission ordering, and cleanup. Extend the source contract to cover deterministic emission parameters and lifetime, implement the complete set, then test in a browser.
3. **iPhone Safari acceptance.** Retest the normal Mission page and memory-check route on physical iPhone Safari: full bounded source download, startup completion, memory profile, joystick walk/sprint, absolute aim, weapon/fire/audio timing, error display, and repeated-tap/pinch zoom behavior. Collect a new report after the next private deployment.
4. **Browser rendering acceptance.** Run the normal `/game/` path with WebGL and WebAssembly enabled. Verify load progress through all 1,919 source assets, rendering/input/physics, and no stalled final-asset state. Current local Playwright browser archive was unavailable/invalid; do not count source-only or fake-browser contracts as acceptance.
5. **Remaining parity groups.** Before each batch inventory full source dependencies for projectile hit/resolution, physiology/damage, ragdoll/joints, NPC behavior, animation/IK transitions, and menus/interactions. Implement in dependency order, add source contract and visible smoke for every group, then rebuild/package/redeploy exact tested SHA.
6. **Completion.** Close archive extraction/audit, exact-source package integrity, private deployment, iPhone acceptance, and browser smoke. Record measured parity and residual exclusions explicitly; do not describe the port as 100% while unimplemented source groups remain.

## Immediate next action

Rebuild and independently verify the package using pushed GitHub SHA `399c829078c605e8575e1b9877e113f703857174`; then create the matching Site archive commit on its separate `main` branch and deploy privately. Continue with the full particle dependency inventory while resolving browser-runner and iPhone acceptance gates.
