# Crisis Response Port — WorkMode handoff

## Checkpoint

- Current working environment: the repository checkout supplied to Codex Work Mode. It is independent of Codespaces, but it is not a checkout on the owner's iPhone or personal computer. Use this checkout for code edits, builds, and local tests; use a browser only for browser acceptance gates. Scratch workspace state can disappear between chats, so persistent repository work must be committed and pushed.
- Active branch: `codex/game-scene-name-normalization-20261023`.
- Latest tested runtime source: `30668ba99fb8250987b63ea81d067abf006ceabd` (`Support original SpriteRenderer and ARGB32 scene data`). The instruction and handoff changes in this checkpoint are documentation-only and do not change that tested source.
- Live `develop/wasm` was queried through GitHub immediately before these documentation edits and is still `adb4e6b73f67a2bafc8c5daf46dc15a6cde1e252`. It matches PR #70's base. No develop merge is needed unless the live branch moves.
- PR #70 remains open and draft, targeting `develop/wasm`. The instruction/handoff update is already on the remote branch at `2dbe205632385e3daddc875dbb41c025e2312426`; GitHub readback confirmed this is also the PR head. No push is pending.
- Other open PRs inspected through GitHub: #69, #57, #56, and #7. No overlap was found with PR #70's SpriteRenderer and texture-import changes. GitHub Actions remain disabled by owner decision.

## WorkMode operating instructions

1. Begin by checking branch, HEAD, and worktree against this file. Query live `develop/wasm` and open PRs with the GitHub connector if shell GitHub authentication is unavailable. If there is no relevant source or PR movement, resume at the next action below without rerunning completed checks.
2. Work directly in the provided repository checkout. Do not reopen/restart Codespaces for routine edits or tests. Do not assume the workspace is the owner's local computer. Use a real browser only for a required browser gate.
3. Keep SDK `8.0.426`; do not change the pin. In the prior WorkMode runner, .NET failed before compilation because the process PID did not map to the mounted `/proc`. A temporary `/tmp/libprocshim.so` workaround enabled SDK builds/checks. That file is ephemeral. If the same host failure recurs, treat it as an environment issue rather than changing project source; use a proven workspace-level workaround if available and record the actual command/result. Avoid repeated attempts at the same unchanged failure.
4. Preserve work across chats by committing and pushing the active branch, then read back its GitHub head/PR head. A local scratch edit alone is not persistent.
5. Keep `NEXT_AGENT.md` concise and replace it entirely once at the end of substantive work. Record exact source SHA and checks actually run separately from documentation commits. Do not create status-only commits or repetitive cycles.

## Completed source work and verification

At source commit `30668ba`:

- Added serialized `SpriteRenderer.m_MaskInteraction` and `m_WasSpriteAssigned` handling with a regression contract.
- Allowed the source-backed Unity `TextureImporter.textureFormat: 5` value used by `Assets/Texture2D/window glass.png`, retaining GUID `bb3d703bd542afc47a1423a6c37397aa`, fileID `2800000`, original PNG bytes, SHA-256 `ba810cf6d18d29ac28aebfc07340188c0cd3d00f0c4fd4071f8e2a7b98d1d4b5`, and importer checks.
- Fresh Release Browser build passed: 0 errors, 45 warnings.
- Runtime contract suite passed (runtime, scene-prefab, presentation, weapons, ballistics). It reports `fullGameParity: false`; this is not a gameplay pass.
- Normalized Marksman scene import passed the fixed SpriteRenderer fields and original ARGB32 window texture, then stopped at the missing mesh below. The later scene texture assertion has not run because the complete scene has not loaded.
- `git diff --check` passed at the source checkpoint. The current documentation-only instruction changes also passed `git diff --check` before commit.

## Unresolved scene dependency — next action

Full Marksman scene loading reaches:

`Assets/Scenes/Marksman range.unity` → `Marksmen` component fileID `2598` → `Assets/GameObject/Marksman mode.prefab` → `MarksmanBoltActionVisualization.roundPrefab` → `Assets/Resources/weapons/ammunition/marksman/Marksman round .308 Winchester SP.prefab` → MeshCollider fileID `64587765826080250`.

The collider references missing Mesh GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`, fileID `4300000`. It is absent from the recovered package `guidPaths` and recovered export/repository metadata. Existing search/evidence: `docs/MISSING_CASING_MESH_GUID_SEARCH-2026-10-14.md` and `docs/evidence/logs/triage-2026-10-20-pr42.md`.

The owner allows substitutes only when affected behavior is identical and verified. The previously documented casing mapping (`Assets/Mesh/308 Winchester casing hi.asset`) applies to the root MeshFilter of `Assets/Resources/weapons/ammunition/casings/Casing .308.prefab`; it is not automatically an equivalent Marksman projectile collider. Do not silently reuse it. First search the available recovered/source assets for an actual .308 round mesh candidate and matching `.meta`. If one exists, preserve the missing source pointer/provenance and run a focused equivalence check against the collider's intended round behavior before using it. If there is no suitable verified candidate, request the original mesh plus matching metadata or the owner's explicit decision to defer this Marksman scene gate. Do not claim original scene flow, Marksman rendering, in-scene FIRE, or two-actor gameplay while this dependency prevents scene loading.

After a valid source/replacement decision, rerun the full scene check once. Then continue the original scene-flow, render, FIRE, and real Operator/Occupant lifecycle gates. Do not publish until the required local/browser gameplay gates pass.

## Environment and policy notes

- `docs/REPOSITORY_REMEDIATION_2026-10-18.md` was read for the mandatory repository gate. Its older CharacterBody compiler counts are historical and unrelated to this scene-import checkpoint.
- No Site save, deployment, access change, or GitHub Actions workflow was run for this work.
- Do not repeat the original .NET `/proc` diagnosis unless new runner evidence appears. Do not restore GitHub Actions.
