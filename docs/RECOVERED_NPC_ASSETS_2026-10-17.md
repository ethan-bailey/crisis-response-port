# Recovered original NPC visuals, spawn definitions and cardboard boxes — 2026-10-17

## Verified source archive; do not mistake missing Git files for deletions

The **complete original recovered Unity project** is in the repository's GitHub Actions artifact [run 38582072502](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38582072502), artifact ID `11903811738`, name `crisis-response-recovery-direct-v3-1`. Its **unmodified ZIP SHA-256** is `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`; ZIP integrity test found zero damaged files. ZIP contains **8,575 entries**, including report/decompiled-source entries and the recovered Unity project (7,231 files, about 169 MiB). The **temporary Actions copy** is scheduled to expire **October 28, 2026 at 14:38:58 UTC**, but an identical **permanent private GitHub release copy already existed from October 15**: [recovery-preservation-2026-10-15](https://github.com/ethan-bailey/crisis-response-port/releases/tag/recovery-preservation-2026-10-15) (101,082,158 bytes, matching ZIP SHA-256). The separate user's original `.rar` has SHA-256 `5a12ff494b26713d149f9be026619732247f817ddf8944a399b6ee1d12ea0ab3`.

The original installer and the full AssetRipper Unity export **were deliberately not committed** into ordinary Git. Their absence from `main`, `develop/wasm` or PR #56 is not evidence of a deletion. Comparison of the **26** `game/assets/` files present at original gameplay commit `ec7a1e1ec` against `main`, `develop/wasm`, and PR #56 found **zero missing paths**. The recovered Operator animation JSON was later updated, not deleted.

A [one-time private preservation run #39041401478](https://github.com/ethan-bailey/crisis-response-port/actions/runs/39041401478) **SUCCEEDED**. It verified the exact recovery ZIP hash, copied original scene/prefab/spawn C# and Unity `.meta` source to `source-reference/Original/NPCRecovery/Assets/`, copied 19 original NPC PNGs to `game/assets/occupant/skins/`, and committed them on the feature branch at [`65b8b6a`](https://github.com/ethan-bailey/crisis-response-port/commit/65b8b6ae1950fccc036e9d93e80432337269b497). The preservation manifest `source-reference/Original/NPCRecovery/MANIFEST.json` records SHA-256 hashes of **67** source/visual files. A separate private draft release was also created with the exact recovered ZIP; the existing 2026-10-15 private release remains the canonical permanent backup.

## Exact original NPC visual and behavior sources (path relative to AssetRipper project)

- **True physical/visual NPC prefab:** `Assets/Resources/characters/Occupant.prefab` (267,833 bytes, SHA-256 `9094b840c49d6c53ffa077c274561ab5cbeedb202f85dbaade023ad67cb8c419`). It has **56 GameObjects**, **20 MeshRenderers**, **16 LimbRenderer dynamic skin components**, and **20 CapsuleColliders**. Also retain its `.meta` serialized GUID file.
- **Distinct console command descriptor:** `Assets/GameObject/Occupant.prefab` (2,066 bytes, SHA-256 `c41a1f6c4e04ffd9de8947f55f57de61323ccdf916970c5069e86f044a177da8`). This is **not** the NPC body prefab; it points at the above via prefab GUID `8363ad318a650364ebdaa8c237f65bfe`.
- **Original skin PNGs in `Assets/Texture2D/`:** `black pelvis.png`, `black abdomen.png`, `torso_0.png`, `neck protector.png`, `head 1.png` through `head 5.png`, `upperarm.png`, `lowerarm.png`, `black upper leg.png`, `black lower leg.png`, `boot.png`, `boot toes.png`, and original hand textures `hand.png`, `hand safe.png`, `hand trigger.png`, `hand front rifle.png`. **19 retained PNGs total** (346,247 bytes). Their `.meta` GUIDs resolve the prefab's authentic material/renderer references; one occupant limb skin palette has five original head variants.
- **Original materials:** `Assets/Material/Dynamic occupant torso.mat`, `Assets/Material/Static occupant torso.mat` and other referenced shared limb materials.
- **Original C# spawn and AI**: `Assets/Scripts/Assembly-CSharp/OccupantSpawner.cs`, `Occupants.cs`, `OccupantSpawnData.cs`, `SpawnArea.cs`, `CharacterTemplate.cs`, and `SerializableOccupantSpawn*.cs` classes. Spawning varies by tutorial/campaign/sandbox modes: `OccupantSpawner.Start` initializes original spawn areas and calls `Spawn`; sandbox uses enabled specific/random groups from `Global.Context.OccupantSpawnData.data`; campaign uses mission character templates. Do not mistake the `debugSpawn` group for always-spawned mission NPCs.

## Authentic serialized scene spawn definitions

The original `OccupantSpawner` MonoBehaviour has script GUID `b6775fa88f191b1a6dbb5cea21f8f7f4`, found in **both** original scenes. These exact YAML fields, not guessed NPC coordinates, must drive subsequent integration:

| Original scene | File SHA-256 | Source spawner record | Configuration |
|---|---|---|---|
| `Assets/Scenes/Mission 01.unity` | `c43b36c415b4df14ef6223eaced686a8ddcead6c46f7504346aecb1a03c5743e` | fileID `7142`, GameObject `380` | `spawnLegacyGroup=1`; start/end **1–39.5**; one **Default** area with x 0.5–40, y 1, z -1.525–-1.475, walkable -9.5–40; original area `probability=0` |
| `Assets/Scenes/Marksman range.unity` | `cc64f48f2087a62346fdc77ee85b874e7b09c727e1fb246f86007b9415b3d16b` | fileID `3161`, GameObject `272` | `spawnLegacyGroup=1`; start/end **-6–12**; seven named areas; collision increment 0.2, clear radius 0.5, max tries 20 |

The seven Marksman range areas and original probabilities are **Front ground (0.2), Center wing inside (0.2), Right wing inside (0.15), Left wing inside (0.15), Center wing roof (0.1), Left wing roof (0.1), Right wing roof (0.1)**. Each area's original x/y/z and walkability ranges live in its scene YAML. Original debug spawn has `bodyPrefabs: [Occupant]`, `minCount=1`, `maxCount=1`, `minCoord=28`, `maxCoord=31`; it is a **debug command default**, not a mission placement schedule. `Mission 01` has a serialized default area with probability zero; `spawnLegacyGroup` and source logic must be respected rather than forcing the zero-probability area to spawn.

## Box prefabs and placements (the literal boxes, not NPCs)

- `Assets/GameObject/Big cardboard box.prefab` (SHA-256 `013be4bc5a43ad71b0e7f3381c665f196968eb5b771ce77540afae6f4cd64f1d`).
- `Assets/GameObject/Small cardboard box.prefab` (SHA-256 `52630db32cd6c33fa2427258789f83220ccfd2a692b032c7e78d7d4e685b9f56`).
- Original `Mission 01.unity` has named scene instances such as `Cardboard box (3)`, `Big cardboard box`, and `Small cardboard box`, with their original transforms, prefab links and materials. Do not count UI GameObjects labeled `Box (n)` as physical cardboard props.

## Port integration safeguard

The production browser currently imports one Operator. **Recovered NPC skins and scene/spawner source are now present in Git on the feature branch, but production NPC instantiation, AI, material binding and visual rendering still require execution wiring.** Keep exact shader/material/skin GUID associations and spawn-mode rules when consuming this recovered data; **presence and preservation of recovered assets are not proof of NPC gameplay or rendering parity**. Never delete or overwrite the 97 MiB source archive, source GUID maps, or existing `game/assets/operator/` binaries.
