# Current handoff — 2026-10-26

## Live repository state

- Integrated branch: develop/wasm. The live head before this workflow/documentation commit was 4bf75865dc2d8cd7e7cd7513dd583c870f2413e0. This commit is documentation and CI configuration only; query live GitHub for the new head before source work.
- No open pull request targeted develop/wasm at handoff preparation.
- The repository was public with Actions enabled. 78 branch refs existed; retain them until a separate unique-commit audit and owner-approved cleanup.
- The new source-and-WASM workflow will run on relevant source pushes and PRs, or by manual dispatch. It checks existing contracts and builds to a temporary runner directory. It does not deploy. Its result is pending at handoff preparation; record the exact run and conclusion after GitHub reports it.

## Latest playable/release checkpoint

- Release source checkpoint recorded in the prior handoff: develop/wasm commit 90cc14b2362d868fd921138a69c74092612394b6, with code parent 914a50b35c468cd1fe91fe116715549e08671653. The live branch later advanced with documentation commits; check the live branch before reusing this SHA.
- Site source commit: e03087487359e61dce2b0db0345f1dc8818bb876.
- Owner-private runtime Site version 67, deployment appgdep_6adea77b4ab481918c3aafca4051d9fc, succeeded 2026-10-26T01:06:13Z. Audience remained custom/owner-only.
- Version 67 is the current deployed acceptance candidate; physical iPhone Safari acceptance remains pending. Do not treat this older deployed source as the current GitHub head.

## Exact evidence for version 67

- .NET SDK 8.0.426 Release publish succeeded from Site source e03087487359e61dce2b0db0345f1dc8818bb876 with 0 errors. Optional wasm-tools optimization workload was unavailable; this was not a correctness failure.
- Node contracts: 122 passed, 0 failed. Python suite: 41 passed, 0 failed.
- EngineChecks: 75 passed. FoundationChecks: 121 passed.
- Package verifier: 2,684 files, 199 Blazor resources, 195 WASM files, and all 1,919 bounded source assets. Manifest SHA-256: 3b54dc10517c3a77188246dcc8726473ad54288b922aa4a8b0ef981d22e5cf8f.
- Source package SHA-256: 896bc8b990ec7554bdfd2c9c73f7ce3e7254fed1036f51c709c9ecb970d1f62d. Deployment archive SHA-256: 9c71dbe0ade190d082262c3bf4ab554a127c64f6e7a076d94427dfcbc0f5463a.
- Source profile includes 1,919 assets / 22,365 records, 97,767,549 compressed asset bytes, and 205,514,345 source asset bytes. The incremental bridge uses a flattened bounded Uint8Array and fail-closed gzip/hash verification, at most 16 assets/about 1 MiB per batch.

## Remaining gates and limitations

1. Owner acceptance for exact Site version 67 on a WebGL-capable browser and physical iPhone Safari. Capture mission completion, load/memory behavior beyond asset 1,900, movement/aim/fire/reload/audio, tap-zoom prevention, error reporting, physics/casing behavior, and particle pixels. The prior cloud browser had WebGL disabled; this blocks rendered acceptance only.
2. Complete the inventoried 51-system particle lifecycle from original serialized owners and callbacks: Emitter, ProjectileTrace, weapon smoke, blood/wound effects, bounce, pause/time-scale, destruction ordering; active collision, clamp velocity, inherit velocity, force, rotation, size, color, burst, and Unity random-state behavior. Add no invented artwork. Spark.png (GUID 43907091bffd53b478c985dca2762559; expected SHA-256 6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344) remains absent from recovered and checked-in sources.
3. Continue the broader source-backed gameplay closure for projectile/damage outcomes, ragdoll/joints, authored mission/NPC flow, remaining animation/IK, interaction/menu systems, and mobile loading/storage.

## Next action

Begin the next source-backed particle lifecycle batch from the complete 51-system inventory while version 67 awaits target-device acceptance. Work from the live develop/wasm HEAD, inventory every recoverable in-scope dependency before implementation, continue independent work past device/asset blockers, and update this handoff after the batch with exact evidence.

## Workflow-bootstrap verification

No source behavior changed in this workflow/documentation commit. This session has no local repository checkout, so no local build or test was run. The exact GitHub Actions run for the new workflow is the pending verification; append its commit SHA, run ID, and conclusion after observing it.
