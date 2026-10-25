# Current handoff — 2026-10-25

## Exact pushed checkpoint

- Branch: `develop/wasm`.
- Remote implementation commit: `0fdc57e2b1e39962c0b006961cbe6f6072db36a9`; pushed through the GitHub Git Data API with an expected-head lease from `5cdfcff7a0a3504b81b2b59fe66372b5c82ad4de`.
- Local checkout is synchronized to that remote head and clean before this handoff edit. The remote implementation commit contains three ordered commits: particle callback transport, reuse of a verified bounded profile when the raw archive is incomplete, and this batch's preceding handoff.
- Package/build manifest was produced from exactly `0fdc57e2b1e39962c0b006961cbe6f6072db36a9`. Manifest SHA-256: `a16aa1832b3a11a6755ee5c27b397d265e20dbd4e1de7411de566dc58474538f`.
- No Site version was saved or deployed in this batch.

## Completed this batch

1. Completed and hash-pinned the bounded Mission particle dependency inventory: 51 ParticleSystems/renderers in 27 prefabs, 32 systems in the Mission scene GUID closure, 19 more in 9 dynamically referenced prefabs, plus the complete material, texture, mesh, shader, module, curve, burst and owner-reference records. Human and machine-readable inventories are in `docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md` and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`.
2. Bound `BrowserParticleEventQueue` before the incremental source scene loads. Source `Emit`, `Simulate` and `Play` callbacks reach FullSource actor snapshots with Unity time, source component GUID/file ID/path/hash, hierarchy, pose and bounded-drop accounting. Prefab clones retain original component identity.
3. The exact Mission source contract fired the original M4 and verified particle-command provenance and clock. It observed 1 particle event and 0 dropped requests, along with the weapon audio, casing force/torque and PhysX round trip.
4. Fixed package profile closure: the compact raw scene archive lacks `Assets/Resources/Version.prefab`, while the already integrity-checked version-62 bounded profile contains it. `package_wasm_site.py` can now reuse and re-verify the prebuilt profile instead of generating an incomplete closure from the archive.

## Verification evidence for `0fdc57e2b1e39962c0b006961cbe6f6072db36a9`

- SDK `8.0.426`; pinned SDK archive SHA-256 `bb691c98abc8797eedbd48d3a766bc734214aed746e52e2691d8bbb268c60764`; process-stat shim `/tmp/libprocshim.so` used as documented in `docs/DOTNET_BUILD_RUNBOOK.md`.
- Full runtime Release build: **0 errors, 48 existing warnings**.
- Release Browser WebAssembly publish: exit 0. The only publish note was optional `wasm-tools` optimization unavailable.
- EngineChecks: **73 passed**.
- FullSource Mission actor/weapon contract against the exact version-62 bounded profile: **passed**, including a real shot, 1 particle event with original GUID/fileID/path, zero drops, original weapon clips, casing callbacks, and physics state round trip.
- Python: **41 passed**. Node: **97 passed**. Particle inventory Node regression: **4 passed**. The exporter reproduced decompressed inventory SHA-256 `636935d6bf545dd0bf3610008bbc8759fae1fc765553852b8110da9eacc82407`.
- Release package independently verified from the pushed SHA: **2,679 files**, 199 Blazor resources, 195 WASM files, 1,919 source assets (**97,767,549 compressed / 205,514,345 uncompressed bytes**). The 6 original casing dependency hashes and all packaged files/resources passed integrity checks.

## Release/acceptance status

- Callback transport is implemented; **particle module simulation and rendering remain open**. The browser binder is still impact-only (14 prefabs / 26 systems) and reports zero rendered particles. Do not describe this batch as visual particle parity.
- Browser/WebGL PhysX smoke and physical iPhone Safari acceptance were not run. Do not claim either passed.
- The Site remains private at version 62; owner-only access was verified. Its configured source repository is a separate `cloudflare_artifact` repository on branch `main`. This pushed `develop/wasm` commit is not that branch's source head, and the Site credential response had `publish_on_push_accepted=false`. Do not save/deploy the GitHub archive under a mismatched Site source commit. Sync and build the chosen source through the Site repository workflow before a later private deployment.

## Remaining gates in dependency order

1. **Finish particle rendering.** Use the complete inventory to implement source module evaluation (curves, bursts, shape, color, lifetime, time scale, pause and cleanup), all 51 systems and 27 owner paths, renderer modes 0/1/4, materials/textures/meshes, and explicit built-in shader/mesh handling. Add contracts for each original emission path before claiming rendered parity.
2. **Synchronize and publish the exact checked build.** Push the selected code tree through the Site's own `main` source workflow, build/package from that pushed commit, verify package integrity, and deploy only that saved private version. Preserve the current owner-only audience.
3. **Browser acceptance on the exact deployed package.** Exercise normal Mission and memory route, asset delivery completion, actual particle drawing, WebGL and physics/casing behavior. Browser automation binaries were unavailable in this batch; use them if the next environment provides the supported runner.
4. **iPhone Safari acceptance.** On that exact deployed version, check the full profile, memory, movement/sprint wall, absolute aim, weapon/audio timing and ceiling, zoom resistance and error readability.
5. **Remaining gameplay parity and completion.** Before each implementation batch, inventory the complete source closure for projectile/damage, ragdoll/joints, NPC behavior, remaining animation/IK, and interaction/menu systems. Then rebuild and integrity-check the exact source package; report measured parity and exclusions.

## Immediate next action

Continue from `develop/wasm` at `0fdc57e2b1e39962c0b006961cbe6f6072db36a9`: implement source-driven particle module simulation and rendering over the completed 51-system/27-prefab dependency inventory. Keep the Site publication gate separate until its own source branch matches the tested archive.
