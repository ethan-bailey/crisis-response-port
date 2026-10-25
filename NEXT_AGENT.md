# Current handoff — 2026-10-25

## Exact pushed checkpoint

- Branch: `develop/wasm`.
- Source commit: `1a87525f00a392407c3cff657169f654455185ab` (`Preserve verified prefab asset identity in particle events`); local checkout and `origin/develop/wasm` are synchronized and clean before this handoff edit.
- Release WASM output was built from that exact source commit with .NET SDK `8.0.426`.
- Exact source package: `/tmp/cpr-particle-site-package-1a87525`.
- Build manifest SHA-256: `b09ddb4091647669a3ba0db65cca17ba10cc1fce10223530561fcdbac59107ca`.
- Package integrity verification passed: 2,679 files, 199 Blazor resources, 195 WASM files, and the bounded `global-ui-mission-operator` profile (1,919 assets; 97,767,549 compressed bytes / 205,514,345 source bytes). Scene smoke data is excluded.
- No Site version was saved or deployed in this batch.

## Completed this batch

1. Completed the particle dependency inventory before rendering work: 51 ParticleSystems/renderers in 27 prefabs; 32 systems in the Mission scene GUID closure and 19 in 9 dynamically referenced prefabs; material, texture, mesh, shader, module, curve, burst, and owner-reference dependencies are recorded in [`docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md`](docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md) and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`.
2. Wired FullSource `Emit`, `Simulate`, and `Play` callbacks from the source actor snapshot into the browser particle observer. Commands carry source time, GUID, file ID, asset path, verified source-asset SHA-256, hierarchy, world pose, and bounded-drop accounting. Clones preserve source provenance.
3. Closed the hidden source-asset-hash gap. `SceneLoader` resolves the containing verified source prefab's SHA-256 and GUID for each prefab component, and clones retain that identity. The browser observer validates the source asset hash. The real Mission fire-input contract now verifies this identity on an actual M4 particle command.

## Verification evidence for `1a87525f00a392407c3cff657169f654455185ab`

- Full runtime Release build: **0 errors, 48 existing warnings**.
- Release Browser WebAssembly publish: **exit 0**. Optional `wasm-tools` optimization was unavailable; publish completed without it.
- FullSource Mission actor/weapon firing contract against `/tmp/cpr-incremental-mission-profile-v3`: **passed**. It produced one source shot and one particle event, verified the source asset SHA/GUID/file ID/path and source clock, dropped zero particle events, and passed audio, casing force/torque, movement, absolute-aim, and physics-state round-trip assertions.
- Node: **100 passed**. Python: **43 passed**. EngineChecks: **73 passed**. FoundationChecks: **121 passed**.
- Package build and independent verification passed for the exact commit and manifest above.

## Remaining gates in dependency order

1. **Original particle simulation and rendering.** The browser module currently observes commands and honestly reports `renderedParticles: 0` / `sourceVisualParity: false`. Use the completed 51-system dependency inventory to implement and contract enabled modules, curves, bursts, shape, color, lifetime, source time scale/pause/cleanup, all owner paths, billboard/mesh renderer modes (0/1/4), source materials/textures/meshes, and built-in shader/mesh behavior. Verify each original emission path before claiming visual parity. Keep unsupported source behavior explicit.
2. **Site source synchronization and exact private deployment.** The Site is owner-private at version 62 and its configured source is a separate `cloudflare_artifact` repository on branch `main`, with automatic publish disabled. The GitHub `develop/wasm` commit is not that Site branch's source commit. Locate the supported Sites source-workflow helper, synchronize the selected code into Site `main`, build/package from that resulting source commit, verify integrity, then save/deploy the matching private version. Preserve the owner-only audience.
3. **Browser acceptance on that exact deployed package.** Exercise normal Mission startup, full and bounded memory routes, asset delivery completion, particle drawing, WebGL, physics/casings, input, audio, and readable error handling with a supported browser runner.
4. **Physical iPhone Safari acceptance on the same version.** Check full profile completion and memory, joystick walk/sprint resistance, absolute aim, weapon/audio timing and ceiling, tap-zoom resistance, and error readability.
5. **Remaining gameplay parity.** Inventory each complete source dependency closure before implementation batches for projectile/damage, ragdoll/joints, NPC behavior, remaining animation/IK, and interactions/menus. Run the affected source contracts, rebuild the exact package, and record measured parity and exclusions.

## Concrete blockers and constraints

- The Site's code source is the separate Site-owned `cloudflare_artifact` repo; direct publication of the GitHub archive would have a mismatched source identity. The Site source helper is not present in the current workspace, so no Site sync or deployment was attempted.
- Automated Playwright/browser binaries and physical iPhone Safari are unavailable in this environment. Those acceptance gates need the supported browser runner and the user's device.
- Native Unity particle module rendering and full source visual parity are not implemented yet; do not treat callback transport as rendered effects.

## Immediate next action

Continue on `develop/wasm` after this handoff commit. Start with the inventoried particle simulation/rendering dependencies in gate 1. Keep Site sync/deployment blocked until its supported source workflow is available; then rebuild and verify from the exact Site-owned source commit before saving a private version. Update this file with the exact new commit, tests, package manifest digest, and next dependency-ordered action.
