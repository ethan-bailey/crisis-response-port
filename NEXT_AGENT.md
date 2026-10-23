# Crisis Response Port — current handoff

## Checkpoint

- Development line: `develop/wasm`, live and local head `f335a065f7012cb71c12cf49ced0e86c10d05226` at the last verification. The remote push succeeded; worktree was clean and matched `origin/develop/wasm`.
- No open PRs at the last GitHub connector check. GitHub Actions remain disabled. Continue directly on `develop/wasm`; do not create a PR for this workstream.
- Current private Site: v31. No build or deployment was made in this session.
- Codespaces use is at 90% through 2026-11-01; stay in this Work Mode checkout unless a required gate cannot run here.

## Verified this session

- Pinned .NET SDK **8.0.426** selected with `/tmp/libprocshim.so` at merge checkpoint `789d46bc4c2d3fc740635e0add338ddd3f87365d`.
- `EngineChecks`: **69 assertions pass**. `FoundationChecks`: **121 assertions pass**. Full `runtime/Crisis.Runtime.csproj` build: **0 errors, 45 warnings**. This compiles the selected runtime project, not the full original CharacterBody graph or complete game parity.
- Browser adapter/scene movement tests: **26 pass**; audit Python suite: **12 pass**; Site packager tests: **13 pass**. Actor migration and CharacterBody ownership scans are clear; source inventory check passes (662 sources, 1,704 symbols, 1,332 reachable files, 124 ambiguous names still need review).
- Re-extracted the pinned `Crisis Response.rar` (SHA-256 `5a12ff494b26713d149f9be026619732247f817ddf8944a399b6ee1d12ea0ab3`): 411 files / 185,036,038 bytes. `UnityPy==1.25.4` inventory on 9 player containers passed: 19,500 serialized objects, 163 meshes, no mesh parse errors, and 29 `unity_builtin_extra` objects (all Shader). Reproduce with `scripts/inventory_original_player_meshes.py`; raw player files remain outside Git.
- Original player `resources.assets` maps GameObject `Casing .308` → MeshFilter 6231 → Mesh pathID 610 (`.308 Winchester casing`, 42 vertices, 52 triangles). The ten .308 round/variant GameObjects use different Mesh pathID 670 (`308 Winchester casing hi`, 494 vertices / 494 triangles). Hashes and all references are in `docs/evidence/ORIGINAL_PLAYER_MESH_INVENTORY_2026-10-23.json`.
- Source GUID `b4b69dc6c74eeb54cbd22d28e37f9a19` / fileID `4300000` still has no project `.meta` mapping. The player binary establishes the compiled casing-root mesh object, not source GUID identity. Keep round/projectile refs unchanged. Casing rendering equivalence in the port remains untested.
- Commits pushed: `789d46b` reconciled the local branch with current develop; `b30b59f` added the source-backed mesh inventory; `f335a06` recorded results and refreshed the repository file index.

## Remaining gates

- Determine whether the current scene/package path needs exact player Mesh 610 geometry in the casing-root asset path; if so, integrate it and verify actual in-game rendering. Do not substitute Mesh 670 into the casing root or alter round/projectile refs.
- Resolve the source's built-in dependency pointers against recovered player records; the 29 Shader IDs alone do not close this gate.
- Original actor lifecycle, two-actor combat/physiology/ragdoll, scene flow, real browser gameplay smoke, and physical iPhone Safari acceptance remain open. A successful selected-project build is not parity evidence.
- Do not publish until required exact-SHA local browser/game gates pass and the owner-authorized destination is confirmed.

## Errors and environment notes

- An initial repository-index check omitted required `--site-manifest`; reran with `/tmp/crisis-response-site-20bfc1d/build-manifest.json` and it passed.
- A temporary scanner pointed at the wrong extraction directory; corrected it and reran successfully against a fresh archive-verified extraction.
- `UnityPy` was absent initially; installed pinned `UnityPy==1.25.4` into `/tmp/unitypy-1.25.4-env`. The .NET SDK and process shim were available; no .NET compiler errors occurred.

## Next action

On `develop/wasm`, use the verified Mesh 610 record to determine whether the current scene/package path needs an exact casing-root geometry import. If needed, add that source-backed asset and a focused render contract, without changing round/projectile references; otherwise mark the casing mesh dependency resolved for the player runtime and move to built-in resource reference closure. Run the applicable checks once, update this handoff once, regenerate the repository file index, commit, push, and verify the remote head.
