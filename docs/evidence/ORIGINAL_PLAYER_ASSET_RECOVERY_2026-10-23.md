# Original Windows player asset containers recovered — 2026-10-23

## Source and integrity

The user's preserved `Crisis Response.rar` archive is 67,925,884 bytes, RAR5, SHA-256 `5a12ff494b26713d149f9be026619732247f817ddf8944a399b6ee1d12ea0ab3`. This matches the existing archive inventory. The archive itself is not copied into the repository.

`scripts/extract_original_player_data.py` now safely extracts only regular files below `Crisis Response/Crisis Response_Data/`. It verifies the archive SHA before writing, rejects traversal and symlink paths, refuses to overwrite differing existing files, and writes a SHA-256 manifest. It uses the system libarchive reader; no executable from the game archive is run.

## Verification performed

- `python3 -m py_compile scripts/extract_original_player_data.py` — passed.
- Ran the extractor on the preserved archive — passed: 411 files, 185,036,038 bytes. Output manifest was generated outside the repository.
- The extracted player `resources.assets` SHA-256 is `c55bd896a6b38e43fdc85d980f80fd2f43a7c1aa484f15200a30b89079c59740`.
- The player's `Resources/unity_builtin_extra` SHA-256 is `7c8974f454eef9ed3f0ad0c21695e4cd226f4b2d33f4684d48e69315a3e9be1a`.
- Re-extracted the pinned archive to `/tmp/cpr-player-data-verified` and verified 411 files / 185,036,038 bytes. `UnityPy==1.25.4` parsed the original player containers: 9 serialized files, 19,500 objects, 163 Mesh objects, zero mesh parse errors. The checked-in [mesh inventory](ORIGINAL_PLAYER_MESH_INVENTORY_2026-10-23.json) records each container hash, object ID/type/name, mesh raw-data hash, geometry hash/counts, MeshFilter references, and the 29 built-in object IDs. It does not copy original geometry bytes into Git.
- The original player `resources.assets` resolves the `Casing .308` GameObject (pathID 2334), through MeshFilter 6231, to Mesh pathID `610`, named `.308 Winchester casing`: 42 vertices, 42 normals, 52 triangles, raw object SHA-256 `d8b2d8bd007dde96bb005abb75a5bc9fb868016fcf691029d24f515633106649`, geometry SHA-256 `26c4788b06a0823ea76ebc76247fc4a9a012f6b2540c0b5c61fa72af172378eb`.
- The player build uses a different Mesh for its .308 rounds: Mesh pathID `670`, `308 Winchester casing hi`, 494 vertices / 494 triangles, is referenced by ten .308 round/variant GameObjects. Those references were inventoried and left untouched. This supports using object 610 as the exact compiled-player casing-root geometry; it does not establish the missing source `.meta` GUID mapping.
- `Resources/unity_builtin_extra` contains 29 serialized objects, all type `Shader`; this scan found no Mesh or Material records there. The inventory records their IDs. This result does not resolve every Unity built-in dependency in the project.
- The original asset containers were extracted from `Crisis Response_Data`; no original project `.meta` files or GUID-to-player-object mapping are present in the player archive.

## Gate status and next work

The player-build mesh identification portion of Gate 1 is complete: the built `Casing .308` object points to original player Mesh 610. The source project still lacks a `.meta` record that maps GUID `b4b69dc6c74eeb54cbd22d28e37f9a19` / fileID `4300000` to player pathID 610, so do not claim source-GUID identity. Any port integration must use the exact object-610 geometry and pass the casing rendering contract; the separate high-detail Mesh 670 used by .308 rounds is not the casing-root mesh. Keep all round/projectile references unchanged.

Player built-in inventory is now available, but project built-in dependency resolution remains open: compare the source's serialized built-in pointers to the 29 actual player `unity_builtin_extra` Shader records and other runtime resources.

The owner-approved compatibility mapping is limited to the casing root MeshFilter and remains an approximation until equivalence is tested. Do not substitute the missing mesh in the five Marksman round/projectile prefab references. The recovered player strings alone do not satisfy the geometry or dependency-closure gate.

The reproducible parser is `scripts/inventory_original_player_meshes.py`; run it with the verified player-data directory and extraction manifest using `UnityPy==1.25.4`. The raw archive and extracted player binaries remain outside Git. Browser rendering and in-game casing equivalence are not established by this asset inventory.
