# Missing .308 casing mesh GUID — evidence report

Date: 2026-10-14

## Question

Can the unresolved serialized mesh GUID `b4b69dc6c74eeb54cbd22d28e37f9a19` be recovered or safely mapped from the current recovered project?

## Search scope and result

The verified recovery artifact downloaded from GitHub Actions run 38582072502 was inspected locally. It contains 8,575 files and approximately 157 MB of uncompressed data.

- The exact GUID occurs in six serialized prefabs: the casing prefab and five Marksman-round prefabs.
- No recovered `.meta` file declares that GUID.
- The root MeshFilter in `Assets/Resources/weapons/ammunition/casings/Casing .308.prefab` references the missing GUID as mesh fileID 4300000.
- The same prefab contains a separate child `Projectile` mesh reference, so the missing root mesh is specifically the casing mesh rather than the projectile.
- The missing reference remains unresolved. The current evidence does not include the original raw Unity player asset database needed to recover the absent mesh by GUID.

## Candidate examined, not substituted

Candidate: `Assets/Mesh/308 Winchester casing hi.asset`

- Its own GUID is `cd74722033652fb409431b2947740490`, not the missing GUID.
- Serialized bounds extents are approximately 0.0060008 × 0.0060008 × 0.0255900 metres (full dimensions approximately 0.0120 × 0.0120 × 0.0512 m).
- The casing prefab's disabled BoxCollider has size approximately 0.0387774 × 0.01 × 0.01 m.
- The candidate mesh is referenced by recovered .308 bullet/UI prefabs, but that fact does not prove it is the casing's intended root mesh.
- The difference in bounds and lack of source-GUID evidence mean this candidate cannot be accepted as an identity match based on current evidence.

No GUID was rewritten, no alias was added, and no mesh was silently substituted.

## Automation added

`scripts/search_missing_mesh_guid.py` now scans the complete recovery artifact, searches exact GUID bytes, checks all `.meta` declarations, and inventories similarly named candidate assets with SHA-256 hashes. The GitHub Actions workflow runs this step and uploads `crisis-response-missing-casing-mesh-guid-search` as an artifact.

## Remaining work to resolve the GUID

1. Recover the original Unity serialized asset/database files from the original player/archive, if present, and map the missing GUID to its original mesh object.
2. If the original object cannot be recovered, parse the candidate and all sibling casing meshes into geometry, then compare dimensions, shape, materials, and prefab transform conventions. This can establish a best-effort compatibility alias, but not prove original identity.
3. Keep the gap open until the identity is verified or the user explicitly accepts a documented approximation.

## Separate browser-runtime status

The current hosted page remains an interactive viewer for recovered original scene geometry, not a playable game. The mesh-GUID investigation does not change that status. A playable vertical slice still needs original target behavior connected to scene data, plus engine services for lifecycle, rendering/materials, physics, animation, audio, UI, and touch input.
