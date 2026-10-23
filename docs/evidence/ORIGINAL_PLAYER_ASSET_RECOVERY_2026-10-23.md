# Original Windows player asset containers recovered — 2026-10-23

## Source and integrity

The user's preserved `Crisis Response.rar` archive is 67,925,884 bytes, RAR5, SHA-256 `5a12ff494b26713d149f9be026619732247f817ddf8944a399b6ee1d12ea0ab3`. This matches the existing archive inventory. The archive itself is not copied into the repository.

`scripts/extract_original_player_data.py` now safely extracts only regular files below `Crisis Response/Crisis Response_Data/`. It verifies the archive SHA before writing, rejects traversal and symlink paths, refuses to overwrite differing existing files, and writes a SHA-256 manifest. It uses the system libarchive reader; no executable from the game archive is run.

## Verification performed

- `python3 -m py_compile scripts/extract_original_player_data.py` — passed.
- Ran the extractor on the preserved archive — passed: 411 files, 185,036,038 bytes. Output manifest was generated outside the repository.
- The extracted player `resources.assets` SHA-256 is `c55bd896a6b38e43fdc85d980f80fd2f43a7c1aa484f15200a30b89079c59740`.
- The player's `Resources/unity_builtin_extra` SHA-256 is `7c8974f454eef9ed3f0ad0c21695e4cd226f4b2d33f4684d48e69315a3e9be1a`.
- A binary string scan of the original `resources.assets` found names including `.308 Winchester casing`, `Casing .308`, and `Projectile .308 FMJ`. This proves those names occur in the original player resource file; it does not identify their serialized Mesh object IDs, geometry, or GUID correspondence.
- The original asset containers were extracted from `Crisis Response_Data`; no original project `.meta` files or GUID-to-player-object mapping are present in the player archive.

## Gate status and next work

Gate 1 remains open. Parse the original Unity serialized files with a Unity 2017.2-compatible asset reader. Enumerate Mesh object IDs and geometry from `resources.assets`, `sharedassets*.assets`, and `unity_builtin_extra`; establish whether the missing GUID `b4b69dc6c74eeb54cbd22d28e37f9a19` / fileID `4300000` maps to an original player Mesh, then verify geometry and all dependent references. Also resolve built-in resource records from the actual player files rather than inferring from names.

The owner-approved compatibility mapping is limited to the casing root MeshFilter and remains an approximation until equivalence is tested. Do not substitute the missing mesh in the five Marksman round/projectile prefab references. The recovered player strings alone do not satisfy the geometry or dependency-closure gate.

The WorkMode shell used for this extraction has libarchive, but no `.NET SDK`, Unity Editor, `7zz`, or `UnityPy`. The repository's `scripts/recover_unity_2017_2_resources.py` pins `UnityPy==1.25.4`; use that compatible parser (or another verified 2017.2 parser) when available. Keep the raw archive private and do not add the recovered player binaries to Git.
