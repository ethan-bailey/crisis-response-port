# Current handoff — 2026-10-25

## Checkpoint

- Workstream: `develop/wasm`, fetched base `5cdfcff7a0a3504b81b2b59fe66372b5c82ad4de`.
- Tested implementation commit: `e260006307042521f0eadc4c3e9b571dd631a6ab` (particle event transport, component identity cloning, bounded source profile package reuse).
- Local branch is two implementation commits ahead of the fetched remote and is not pushed yet. The exact tested tree is committed locally; do not reset or discard it.
- No Site version was saved or deployed in this batch.

## Completed this batch

1. Completed and hash-pinned the bounded Mission particle dependency inventory: 51 ParticleSystems/renderers in 27 prefabs, 32 systems in the Mission scene GUID closure, 19 more in 9 dynamically referenced prefabs, plus the full material, texture, mesh, shader, module, curve, burst and owner-reference records. Human and machine-readable inventories are in `docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md` and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`.
2. Bound `BrowserParticleEventQueue` before the incremental source scene loads. Source `Emit`, `Simulate` and `Play` callbacks now reach FullSource actor snapshots with Unity time, source component GUID/file ID/path/hash, hierarchy, pose and bounded-drop accounting. Prefab clones retain original component identity.
3. Added an exact Mission source contract that fires the original M4 and verifies the particle command provenance and clock. It observed 1 particle event and 0 dropped requests, along with the already-covered weapon audio, casing force/torque and PhysX round trip.
4. Fixed packaging for the complete recovered source profile. The raw `scene-data/crisis-response-scenes.json.gz` archive lacks `Assets/Resources/Version.prefab`, while the already verified version-62 bounded profile contains that dependency. `package_wasm_site.py` can now reuse and re-verify a prebuilt profile byte-for-byte instead of generating an incomplete source closure.

## Verification evidence

- SDK `8.0.426`; pinned SDK archive SHA-256 `bb691c98abc8797eedbd48d3a766bc734214aed746e52e2691d8bbb268c60764`; process-stat shim `/tmp/libprocshim.so` used as documented in `docs/DOTNET_BUILD_RUNBOOK.md`.
- Full runtime Release build: **0 errors, 48 existing warnings**.
- Release Browser WebAssembly publish: exit 0. The only publish note was optional `wasm-tools` optimization unavailable; no compile/publish error.
- EngineChecks: **73 passed**.
- FullSource Mission actor/weapon contract against the exact version-62 bounded profile: **passed**, including real shot, one timestamped particle event with original GUID/fileID/path, 0 particle drops, original weapon clips, casing callbacks and state round trip.
- Python: **41 passed**. Node: **97 passed**. Particle inventory-specific Node tests: **4 passed**. Particle inventory exporter reproduced the pinned decompressed SHA-256 `636935d6bf545dd0bf3610008bbc8759fae1fc765553852b8110da9eacc82407`.
- Site package built and independently verified from implementation source commit `e260006307042521f0eadc4c3e9b571dd631a6ab`: **2,679 files**, 199 Blazor resources, 195 WASM files, 1,919 source assets (**97,767,549 compressed / 205,514,345 uncompressed bytes**). Build manifest SHA-256: `68f2554101457159e20e9594eea9cfebed6e067a0f28524ea0f7b17c36f9114e`.
- `git diff --check` passed before this handoff edit.

## Release/acceptance status

- Callback transport is implemented; **particle module simulation and rendering are still open**. The existing browser binder remains impact-only (14 prefabs / 26 systems) and reports zero rendered particles. Do not describe this batch as visual particle parity.
- No browser/WebGL PhysX smoke or physical iPhone Safari acceptance was performed. Do not claim either passed.
- The Sites runtime remains private at version 62. Sites access was verified owner-only. Its configured source repository is a separate `cloudflare_artifact` repository on branch `main`; the tested `develop/wasm` SHA is not that branch's current source commit. No version/deployment call was made because that would misattribute the archive to the wrong Site-source commit. The Site credential response gave no automatic publish window (`publish_on_push_accepted=false`).
- `git push origin develop/wasm` failed because this shell has no GitHub username credential. Push the committed tree through the connected GitHub Git Data API with an expected-head lease, then fetch and verify the resulting remote tree.

## Remaining gates, dependency order

1. **Finish particle rendering.** Use the complete inventory, then implement source module evaluation (curves, bursts, shape, color, lifetime, time scale, pause, cleanup), all 51 systems and 27 owner paths, renderer modes 0/1/4, materials/textures/meshes and explicit built-in shader/mesh handling. Add source emission contracts before visible rendering claims.
2. **Land and synchronize the verified source.** Push to `develop/wasm` without rewriting history. For a later Site publication, sync the chosen exact source tree to the Site's own `main` repository using the Site source workflow, then build/package from that pushed commit. Preserve private audience.
3. **Run browser acceptance on the exact package.** Exercise the normal Mission and memory route, actual particle drawing, WebGL, physics/casing and no stalled asset delivery. Browser automation binaries were unavailable in this batch; use them only if the next environment provides the supported runner.
4. **iPhone Safari acceptance.** On the exact deployed version, check full profile delivery, memory, movement/sprint wall, absolute aim, weapon/audio timing and ceiling, zoom resistance, and error readability.
5. **Close remaining gameplay parity and final release.** Inventory each dependent source graph before changing projectile/damage, ragdoll/joints, NPC behavior, remaining animation/IK and interaction/menu systems. Then rebuild and integrity-check the exact source package before any Site publish; report measured parity and explicit exclusions.

## Immediate next action

Push local commits `292c60f` and `e260006` to `develop/wasm` through GitHub Git Data API (expected base `5cdfcff7a0a3504b81b2b59fe66372b5c82ad4de`), verify the remote branch tree, then continue the source-driven particle simulation/rendering batch from the indexed closure. Do not deploy the current transport-only slice.
