# Current handoff — 2026-10-25

## Exact verified build checkpoint

- Tested source branch and commit: `develop/wasm` at `a308a722cb4e466447ad82d6d7b59cd0107f892e` (`Preserve authored particle module records for browser rendering`). Local and remote trees matched at test time.
- Release WebAssembly publish used .NET SDK `8.0.426` and completed successfully with **0 errors and 48 existing warnings**. Optional `wasm-tools` optimization was unavailable.
- Exact package: `/tmp/cpr-particle-site-package-a308a72`.
- `build-manifest.json` SHA-256: `3aaeea77049e88ac46591dc4b3b2054253eef9ad6ed2da253452f1e18da2327a`.
- Independent integrity verification passed: 2,679 files, 199 Blazor resources, 195 WASM files, bounded `global-ui-mission-operator` profile of 1,919 assets (97,767,549 compressed / 205,514,345 source bytes). Scene-smoke assets are excluded.
- This handoff edit is documentation-only after the tested source checkpoint. No Site version was saved or deployed.

## Completed implementation batches

1. Inventoried the full bounded particle dependency set before implementation: 51 ParticleSystems/renderers in 27 prefabs; 32 systems in the Mission scene GUID closure and 19 in 9 dynamically referenced prefabs, plus material, texture, mesh, shader, module, curve, burst and owner-reference data. See [`docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md`](docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md) and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`.
2. Wired FullSource `Emit`, `Simulate`, and `Play` callbacks from the source actor snapshot into the browser observer. Events carry source time, component GUID/file ID/path, verified containing-asset SHA-256, hierarchy, world pose, and bounded-drop accounting.
3. Closed the prefab provenance gap: `SceneLoader` resolves each prefab component's original GUID and verified asset hash; clone operations preserve it. The browser observer rejects missing or malformed identity.
4. Closed the renderer-input gap: the importer now retains complete serialized `ParticleSystem` and sibling `ParticleSystemRenderer` field records after mapping supported CPU fields. The bounded event includes both authored field maps, and the browser observer requires matching source-prefab identity and nonempty fields.

## Verification for `a308a722cb4e466447ad82d6d7b59cd0107f892e`

- FullSource Mission actor/weapon firing contract against `/tmp/cpr-incremental-mission-profile-v3`: **passed**. A real M4 shot produced one source particle event with valid source asset hash and nonempty authored module and renderer data; zero particle events were dropped. Weapon audio, casing force/torque, movement, absolute aim, and physics-state round trip also passed.
- Node: **100 passed**. Python: **43 passed**. EngineChecks: **74 passed**. FoundationChecks: **121 passed**. Incremental source-store identity contract passed.
- Release WASM publish, exact package build, and independent package verification passed for the tested source commit and manifest above.

## Remaining gates in dependency order

1. **Implement original particle simulation and drawing.** The current browser module still observes source events and explicitly reports `renderedParticles: 0` and `sourceVisualParity: false`. Use the complete inventory and newly transported source fields to implement enabled module evaluation, curves, bursts, shapes, color, lifetime, time scale/pause/cleanup, all 51 owner paths, renderer modes 0/1/4, source materials/textures/meshes, and built-in shader/mesh behavior. Add source-driven contracts per emission path before claiming visual parity.
2. **Sync the selected source to the Site-owned repository.** The Site is owner-private at version 62; its configured source is a separate `cloudflare_artifact` repository on `main`, and automatic publish is disabled. The GitHub `develop/wasm` SHA above is not that repository's head. Locate/use the supported Sites source workflow; then rebuild and verify the package against the exact Site source commit. Do not upload this archive under a mismatched source SHA.
3. **Save and deploy the checked private version.** Preserve owner-only audience. Save/deploy only after Site source HEAD, build commit, and archive source identity match and package integrity passes.
4. **Browser acceptance on that deployed version.** Verify normal Mission startup, full and bounded-memory routes, delivery completion, particle drawing, WebGL, physics/casings, input, audio and error readability with the supported browser runner.
5. **Physical iPhone Safari acceptance on that version.** Verify full-profile loading/memory, walk/sprint joystick resistance, absolute aim, weapon/audio timing and ceiling, tap-zoom resistance, and readable errors.
6. **Remaining gameplay parity.** Before each batch, inventory the complete source dependency closure for projectile/damage, ragdoll/joints, NPC behavior, remaining animation/IK, and interaction/menu systems; implement against source contracts and report measured parity and exclusions.

## Concrete blockers

- Current workspace lacks the supported Sites source-workflow helper. Sites connector exposes credential and publish operations but no source-repository push operation. Do not use a mismatched archive or handle the short-lived credential outside its supported helper.
- Playwright/browser binaries and physical iPhone Safari are unavailable in this environment. Those acceptance checks require the supported runner and the user's device.
- FullSource particle module simulation and visible particle rendering are still unimplemented. Callback transport and authored-field delivery do not constitute visual parity.

## Immediate next action

Continue on `develop/wasm` from the verified particle-field transport checkpoint. Implement the first source-driven visual particle slice using recovered authored module and renderer fields, expanding the complete 51-system dependency set in batches. Keep the Site deployment gate closed until its supported repository workflow can publish the exact tested source; after any code batch, rebuild and integrity-verify the matching package, then update this handoff with exact hashes and the next dependency-ordered action.
