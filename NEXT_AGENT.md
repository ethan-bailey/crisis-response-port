# Crisis Response Port — current handoff

## Checkpoint

- Active branch: `codex/game-scene-name-normalization-20261023`.
- Latest tested source commit: `30668ba99fb8250987b63ea81d067abf006ceabd` (`Support original SpriteRenderer and ARGB32 scene data`). This handoff update is documentation-only.
- `develop/wasm` and `origin/develop/wasm`: `adb4e6b73f67a2bafc8c5daf46dc15a6cde1e252`; the active branch was 0 behind and 9 ahead before the source commit.
- Draft PR #70 targets `develop/wasm`: https://github.com/ethan-bailey/crisis-response-port/pull/70. Other open PRs inspected: #69, #57, #56, #7. No overlapping SpriteRenderer/importer change was found.
- GitHub Actions remain disabled. No Site save, deployment, or access change was attempted.

## Completed in source commit 30668ba

- Added `SpriteRenderer.m_MaskInteraction` and `m_WasSpriteAssigned` serialized-field handling. `ScenePrefabContracts` now checks preservation of both values.
- Allowed the source-backed Unity 2017.2 `TextureImporter.textureFormat: 5` value used by `Assets/Texture2D/window glass.png`. The resolver still checks importer identity and hashes, original PNG SHA-256 `ba810cf6d18d29ac28aebfc07340188c0cd3d00f0c4fd4071f8e2a7b98d1d4b5`, GUID `bb3d703bd542afc47a1423a6c37397aa`, and Texture2D fileID `2800000`.
- Added a Marksman scene assertion for that exact texture identity. It runs after the whole scene loads, so it has not fired yet because a later dependency still blocks the scene.

## Verification at source commit 30668ba

- Fresh command `dotnet build runtime/Browser/Crisis.Runtime.Browser.csproj --configuration Release --no-incremental`: passed, 0 errors and 45 warnings.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --configuration Release`: passed the runtime, scene-prefab, presentation, weapons, and ballistics contracts. The contracts still report `fullGameParity: false`; this is not a gameplay pass.
- With the recovered `scene-data/crisis-response-scenes.json.gz` decompressed to `/tmp/crisis-response-scenes-test.json`, running the Checks project with that package loads past the SpriteRenderer field failure and the `ARGB32` window-texture resolver. It then fails closed on the next missing source asset below.
- `git diff --check` passed.

## Current blocking dependency

Full Marksman scene loading reaches:

`Assets/Scenes/Marksman range.unity` → `Marksmen` component fileID `2598` → `Assets/GameObject/Marksman mode.prefab` → `MarksmanBoltActionVisualization.roundPrefab` → `Assets/Resources/weapons/ammunition/marksman/Marksman round .308 Winchester SP.prefab` → MeshCollider fileID `64587765826080250`.

That collider references missing Mesh GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`, fileID `4300000`. The GUID is absent from the recovered package's `guidPaths` and from the recovered export/repository metadata. The evidence reports are `docs/MISSING_CASING_MESH_GUID_SEARCH-2026-10-14.md` and `docs/evidence/logs/triage-2026-10-20-pr42.md`.

The existing owner-approved substitute is narrowly limited to the root MeshFilter in `Assets/Resources/weapons/ammunition/casings/Casing .308.prefab`, using `Assets/Mesh/308 Winchester casing hi.asset`. Do not apply it to Marksman-round colliders or other projectile meshes. To continue this gate, obtain the original missing Mesh and matching metadata, or get an explicit owner disposition to defer the affected `.308` Marksman scene. Do not claim original scene flow, Marksman rendering, or in-scene FIRE as passing while this dependency is missing.

## WorkMode .NET note

This runner's PID namespace does not match its mounted `/proc`. An ordinary .NET CLI invocation previously crashed before compilation while reading the current process start time; direct MSBuild also failed while reading its process name. `Environment.ProcessId` returned a PID with no corresponding `/proc/<pid>` entry. A temporary `/tmp/libprocshim.so` that redirects current-process `/proc/<pid>` file opens to `/proc/self` enabled SDK 8.0.426 builds and checks. The shim is not tracked and may need recreation in a fresh work session; this is an environment issue, not a repository-code failure.

## Next step

Get the exact missing Mesh and matching metadata from the original Unity project/player data. If unavailable, ask the owner whether to defer the `.308` Marksman scene gate. Once that decision/source is available, resume the normalized scene import, then the original scene-flow, render, FIRE, and real Operator/Occupant lifecycle gates. Do not publish until those gameplay gates pass.
