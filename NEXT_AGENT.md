# Current handoff — 2026-10-25

## Checkpoint

- Development branch: `develop/wasm`.
- Latest substantive source commit: `a4a5526adb249362126188fde3794642d9564c56` (`Add source-backed hemisphere particle sampling`), parent `135749e2b4afbc793ee6c17d582849050673df56`.
- One handoff-only commit follows this source commit; use the live GitHub branch head as the checkout target.
- Open PRs: none at the last live query.
- Keep `.NET SDK 8.0.426`, do not use GitHub CLI, do not deploy intermediate source, and preserve iPhone Safari as the owner acceptance target.
- A clone-created untracked file `source-reference/Original/Assets/Scenes/.Mission 01.unity.u4cmKY` was present at checkout and left untouched.

## Batch completed at the source checkpoint

- Implemented ParticleSystem shape code `2` from the bounded Unity 2017 source inventory: the seven enabled systems are hemispheres with `radiusThickness=1`, `randomPositionAmount=0`, and `sphericalDirectionAmount=0`. The sampler emits into the upper half-sphere and preserves source radius/arc/direction settings.
- Added deterministic shape tests and a regression check against the seven original serialized records. Unity's internal random stream is still not reproduced; this is not a browser-pixel or particle-parity claim.
- Exact-source checks on `a4a5526adb249362126188fde3794642d9564c56`: Node **119/119**, Python **41/41**, EngineChecks **75**, FoundationChecks **121**, and `git diff --check` passed.
- Pinned .NET **8.0.426** runtime build passed with **48 warnings, 0 errors**. Release Blazor WASM publish passed; optional `wasm-tools` optimization was unavailable and not required.
- `npm ci --prefix game --ignore-scripts` plus `npm run --prefix game vendor` restored the lockfile-pinned PhysX browser adapter. The first packaging attempt failed because this generated adapter was absent; after regeneration, a fresh exact-source package built and independently verified: **762 files**, **199 Blazor resources**, **195 WASM files**, source commit `a4a5526...`.
- This package has `boundedOperatorSource:false` and no incremental source profile. It is not playable Mission evidence and was **not deployed**.

## Current exact blockers and next work

1. **Recover the exact bounded Mission/Operator profile before playable package verification.** The last private Site version is v64, source commit `ab0e353fd8930f8107bf9144279e7b14217a9e9e`; its 1,919-asset profile was built from source package SHA-256 `92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb`. Site version archive metadata is available from the Sites connector: archive file `file_00000000186881f5af244d786e61e1aa`, 178,001,920 bytes, content hash `sha256:65a8d3ab9e8203a15a96b5100ab96ed0c72a3f6f084c7a7e788a7ac7ffefca0d`. The current public checkout's `scene-data/crisis-response-scenes.json.gz` is a different source package (SHA-256 `dc979ca92461159aa304b342f58bcde8e7a28c261679e188c8312786e4c2cb79`, 1,504 indexed assets), so it cannot reproduce the 1,919-asset profile. No Site archive materialization/download action is exposed by the current Sites connector; do not replace it with a smaller synthetic profile.
2. **Continue independent source-backed particle work while resolving item 1.** The complete 51-system inventory is in `docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md` and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`. Remaining batches: Unity random state, six renderer-mode-1 systems, all 51 active Collision/Clamp/Inherit/Force/Rotation/Size/Color/Burst paths, and source `Emitter`, `ProjectileTrace`, weapon-smoke, bounce and cleanup lifecycles. Inventory their dependencies as one batch before implementing; retain `sourceVisualParity:false` until native comparison.
3. **Browser/device acceptance remains open.** The available cloud browser previously reported `GL_VENDOR=Disabled` and failed WebGL context creation before Mission startup. Physical iPhone Safari acceptance is still required at a playable milestone. Do not treat desktop/WebGL-disabled runs as the iPhone result.
4. Broader game parity remains open for the source-authored mission/NPC flow, ragdoll/joints, interactions, remaining animation/IK, and full gameplay lifecycle. Build and contract passes do not establish these gates.

## Next action

Use the exact Site v64 archive reference above to attempt read-only recovery of its saved source profile through an available connector. If the connector cannot materialize it, record that limitation once and continue the independent 51-system particle batch from the checked-in source inventory. After a complete profile is available, rebuild/package the exact pushed source, run Mission/browser checks, and only then request the owner's iPhone Safari acceptance. Preserve the private Site and do not deploy this checkpoint.
