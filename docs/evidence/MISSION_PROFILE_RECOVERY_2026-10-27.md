# Full Mission source profile recovery — 2026-10-27

## Provenance and generated profile

The archive `crisis-response-recovery-direct-v3-1.zip` passed ZIP integrity and SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. Only `export/ExportedProject` was extracted. `scripts/extract_runtime_scene.py` produced source index SHA-256 `1b196dafa059a3aadb739b294134d7fe8d9d28510f119214b35c7415059722f7`, covering 2,060 assets / 40,139 serialized records / 24,856 object-reference edges / 776 binary assets / 6 unresolved GUIDs across the entire source export.

`global-ui-mission-operator` was regenerated with the seven exact workflow roots: `ProjectSettings/TimeManager.asset`, `Assets/Resources/Global context.prefab`, `Assets/GameObject/Interior mode UI.prefab`, `Assets/Resources/ui/Interaction menu.prefab`, `Assets/Resources/characters/Operator.prefab`, `Assets/Resources/Version.prefab`, and `Assets/Scenes/Mission 01.unity`. The profile contains 1,920 assets / 22,366 records / 97,767,924 compressed descriptor bytes / 205,515,099 decoded descriptor bytes, with zero unresolved profile references. Manifest SHA-256: `6590427f38a11cfd477220a438e0dcc1ad3ba76c17f012db61258132ff26e645`. Metadata gzip SHA-256: `45ba99949095403092fc21242f6bd0cfff19619a04c41f6fe3038fa73256f29c`. All 1,920 payloads passed compressed and decoded SHA-256 and byte-count verification.

## Missing root and dependency inventory

The tracked bounded `scene-data/crisis-response-scenes.json.gz` contains 1,504 assets. It includes and source-hash matches five of the seven profile roots; `Assets/Resources/Global context.prefab` and `Assets/Resources/Version.prefab` are the two absent roots. `ProjectSettings/TimeManager.asset` exists in the bounded snapshot and matches the recovered source. Among the profile closure, 1,367 paths exist in the bounded snapshot with matching original source hashes; 553 profile paths are absent; 137 bounded snapshot paths are outside this profile closure. There are no common paths with a changed original-source hash.

The complete 553-entry list records each path, GUID, `.meta` hash, original source hash/length, profile descriptor hash/length, and every incoming serialized reference with source path, source fileID, field path, referenced fileID, and resolution. It deliberately records provenance/dependency metadata only and contains no recovered original payloads: [`MISSION_PROFILE_SOURCE_DELTA_2026-10-27.json.gz`](MISSION_PROFILE_SOURCE_DELTA_2026-10-27.json.gz).

The generated profile and recovered project remain runner-local; the normal Site package build embeds the profile and verifies every asset against its manifest. Do not add recovered original assets or the 101 MB source archive to Git. The downloaded archive is retained outside the repository for this run.
