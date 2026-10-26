# Current handoff — 2026-10-26

## Checkpoint

- Development branch: `develop/wasm`.
- Latest source commit: `914a50b35c468cd1fe91fe116715549e08671653` (`Add source-backed stretched particle billboards`), parent `ad5e487dc67ace85cb83074f342860cab834c770`.
- Open PRs: none at the latest live query. The commit has no reported GitHub status checks.
- Keep `.NET SDK 8.0.426`, do not use GitHub CLI, do not deploy intermediate source, and preserve iPhone Safari as the owner acceptance target.
- A clone-created untracked file `source-reference/Original/Assets/Scenes/.Mission 01.unity.u4cmKY` was present at checkout and left untouched.
- An enabled hourly `Advance Crisis Response` automation already exists; do not create a duplicate.

## Batch completed at `914a50b...`

- Added `game/source-particle-render-modes.mjs` for the source camera-plane velocity direction and serialized `m_LengthScale` / `m_VelocityScale` sizing of Unity stretched billboards (mode 1). Passed the active camera through `game/presentation.mjs` and `game/weapon-runtime.mjs`; the renderer now creates a camera-facing Sprite for mode 1 instead of a mesh without geometry.
- Added regression checks against all six mode-1 records in the exact 51-system inventory. The source inventory says all six have `m_CameraVelocityScale=0` and records their exact length/velocity scales. This is an implementation slice, not visual parity or full lifecycle coverage.
- The exact `ParticleSpark.mat` serialized record is present in inventory metadata (GUID `28219ea11a6740946a759c6c6cc2b9f8`, source SHA-256 `79e89c68e5a374f7b841920ddf78eacec39d8f490953b1741b841d5dedd05607`). The raw Spark.png bytes are absent. Inventory lists GUID `43907091bffd53b478c985dca2762559`, SHA-256 `6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344`, 1,539 bytes. Searches of the checked-in source tree and repository content found no original texture bytes. Do not substitute unverified artwork.
- Verification on the committed source: Node **123/123** (`node --test scripts/*_test.mjs scripts/*_contracts.mjs`); Python **43/43** across six explicit source/test scripts; EngineChecks **75**; FoundationChecks **121**; `git diff --check` passed.
- Pinned .NET 8.0.426 Release WASM publish passed. A fresh package built and independently verified at `914a50b...`: **763 files**, **199 Blazor resources**, **195 WASM files**, `status: verified`. It has `boundedOperatorSource:false`, no incremental source profile, and no scene smoke; it is not playable Mission evidence and was **not deployed**.
- A broad `node --test scripts/*.mjs` also launched browser smoke entrypoints and failed 13 smoke files because Playwright is absent in this checkout; the 123 contract/unit tests above pass. Desktop smoke is optional under repository policy. No iPhone Safari acceptance was performed.

## Remaining gates and evidence

1. **Recover the exact bounded Mission/Operator profile before playable package verification.** The last private Site version is v64, source commit `ab0e353fd8930f8107bf9144279e7b14217a9e9`; its 1,919-asset profile used source package SHA-256 `92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb`. Site archive metadata: file `file_00000000186881f5af244d786e61e1aa`, 178,001,920 bytes, content hash `sha256:65a8d3ab9e8203a15a96b5100ab96ed0c72a3f6f084c7a7e788a7ac7ffefca0d`. The Sites connector exposed metadata but no archive materialize/download operation. Current checked-in scene data contains 1,504 indexed assets and cannot recreate that profile. Preserve the archive identifiers and continue checking for a supported source recovery path; do not synthesize a smaller profile.
2. **Continue the full 51-system particle batch from the checked-in inventory.** Mode 1 now has an isolated sizing/orientation adapter and source-bound contracts, but those six systems are not connected to a complete source lifecycle, and the missing Spark.png prevents exact rendering. Other outstanding work includes Unity random-state reproduction; active Collision, Clamp Velocity, Inherit Velocity, Force, Rotation, Size, Color and Burst behavior; source `Emitter`, `ProjectileTrace`, weapon-smoke, bounce and cleanup lifecycles; and complete material/texture/custom-mesh/shader runtime closure. Inventory each dependency before implementing; retain `sourceVisualParity:false` until native comparison.
3. **Target-device acceptance remains open.** Playable package/source recovery and the owner’s physical iPhone Safari acceptance are outstanding. The cloud browser previously reported `GL_VENDOR=Disabled` and could not create WebGL; do not treat it as iPhone evidence.
4. **Broader game parity remains open** for authored mission/NPC flow, ragdoll/joints, interactions, remaining animation/IK and full gameplay lifecycle. Build and contract passes do not establish these gates.

## Next action

Continue a source-backed module/lifecycle batch whose exact serialized records and source callbacks are available in the 51-system inventory. In parallel, retry read-only recovery of the exact Site v64 profile through any newly available supported connector. Do not substitute missing assets, claim parity, or deploy. After profile recovery, rebuild and verify the exact playable package, complete browser checks where available, then obtain the owner’s iPhone Safari result for that exact build.
