# Current handoff — 2026-10-25

## Exact casing implementation checkpoint

- Local tested source commit: `908bbeedbea76d185d5ba192eb329b2c4ccf3882` (`Wire recovered casing into Mission PhysX scene`), parent `c8d0fe7110ec331aa8802fddc81d63248d64eaf0`. This is currently committed locally on `develop/wasm`; normal HTTPS `git push` was rejected because this environment has no GitHub CLI credentials. Push this commit before saving/publishing a Site version.
- Python: `python3 -m unittest discover -s scripts -p '*_test.py'` — **40 passed**.
- Node: `node --test scripts/*_test.mjs` — **93 passed**.
- .NET 8.0.426 Release build — **0 errors, 48 existing warnings**. The real `--browser-entryteam-session-contract` passed with one casing force, one torque, original body ID/effective mass (0.002 kg), and finite source position/rotation/linear/angular velocity round trip. EngineChecks **69 passed**; FoundationChecks **121 passed**.
- Release Browser publish at `/tmp/cpr-publish-908bbee-2/wwwroot` succeeded from the same source tree.
- Package built from source `908bbeedbea76d185d5ba192eb329b2c4ccf3882` and that exact `wwwroot`; independent `package_wasm_site.py verify` passed: manifest SHA-256 `5699bdf641357fcb18600912487370da5105788faca953fd4d70ca30121bee36`, **2,679 files**, 199 Blazor resources, 195 WASM files, and 1,919 incremental Mission assets (97,767,549 compressed / 205,514,345 uncompressed source bytes). The casing package contains the original 54-vertex/52-triangle mesh and all six mesh/prefab/material/physics-material/texture source hashes; verification checks those hashes against the source inventory.
- Exact browser PhysX smoke remains unrun: vendored PhysX is compiled for WebAssembly and fails under Node; no Playwright browser binary is installed. `scripts/original_casing_physics_smoke.mjs` records the Web runner command and assertions. The adapter contract runs in Node with fakes, not real PhysX.
- No deployment has been made from this batch. The Site is owner-private, version 61. The Site's Cloudflare artifact source repository is separate from GitHub `develop/wasm`; preserve both source identities and publish only when the Site version is tied to the pushed/packaged source. Do not broaden access.

## Dependency inventory and gate order

For each gate, inventory the full source dependency closure before implementation. Record original GUIDs, prefab/object graph, serialized component fields, referenced materials/textures/audio/curves, runtime ordering/lifetime, and the executable source contract. Implement and verify the closure as a batch; use runtime failures only to identify genuinely hidden edges.

1. **Land and rebuild this casing closure.** Push the tested source; rebuild the Release Browser output from the pushed SHA; rebuild/verify the Site package from that SHA; reconcile the Site source repository/version identity, then deploy privately. Run real-browser PhysX trajectory/collision/settle smoke when a WebAssembly/WebGL browser runner is available. Do not call the Node adapter contract a real-engine smoke.
2. **Original particle rendering.** Inventory all Mission-used particle systems from serialized prefab/component graph through modules, sub-emitter references, meshes, materials, textures, source time, emission ordering, and cleanup. Extend the source contract to cover deterministic emission parameters and lifetime, implement the complete set, then test in a browser.
3. **iPhone Safari acceptance.** Retest the normal Mission page and memory-check route on physical iPhone Safari: full bounded source download, startup completion, memory profile, joystick walk/sprint, absolute aim, weapon/fire/audio timing, error display, and repeated-tap/pinch zoom behavior. Collect a new report after the next private deployment.
4. **Browser rendering acceptance.** Run the normal `/game/` path with WebGL and WebAssembly enabled. Verify load progress through all 1,919 source assets, rendering/input/physics, and no stalled final-asset state. Current local Playwright browser archive was unavailable/invalid; do not count source-only or fake-browser contracts as acceptance.
5. **Remaining parity groups.** Before each batch inventory full source dependencies for projectile hit/resolution, physiology/damage, ragdoll/joints, NPC behavior, animation/IK transitions, and menus/interactions. Implement in dependency order, add source contract and visible smoke for every group, then rebuild/package/redeploy exact tested SHA.
6. **Completion.** Close archive extraction/audit, exact-source package integrity, private deployment, iPhone acceptance, and browser smoke. Record measured parity and residual exclusions explicitly; do not describe the port as 100% while unimplemented source groups remain.

## Immediate next action

Publish the tested `908bbeedbea76d185d5ba192eb329b2c4ccf3882` source commit to `develop/wasm` through an authenticated Git write path. Then rebuild and independently verify the package using that exact pushed SHA before any Site save/deploy. Continue with the full particle dependency inventory while resolving browser-runner and Site source-repository identity gates.
