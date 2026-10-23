# Crisis Response parity handoff

## Current checkpoint

- Development line: `develop/wasm`. This evidence batch landed at `9c1e1f4e0f8d80bc3b3b80294545e4a075ff86a2`; this handoff/instruction cleanup is documentation-only. Query live HEAD before the next edit.
- No open PRs at the last check. The hourly parity automation is paused; its already-running cycle completed at `92e68c43c1f5738cca16afd53605a92a18be3d10`.
- No Site version was saved or deployed. The existing private Site remains v31.
- Original archive SHA-256: `5a12ff494b26713d149f9be026619732247f817ddf8944a399b6ee1d12ea0ab3`. Do not add its binaries to Git.
- `scripts/extract_original_player_data.py` passed `py_compile` and a full extraction of the original archive: 411 player-data files / 185,036,038 bytes. Asset-container hashes and the remaining parser task are recorded in `docs/evidence/ORIGINAL_PLAYER_ASSET_RECOVERY_2026-10-23.md`.
- Touch adapter tests passed 18/18 against source blobs identical to develop code SHA `a75983409c8aeeab76f53afeab5c0e7a41c428c8`. This does not prove original actor integration or device parity.
- The selected runtime project compiled at `a75983409c8aeeab76f53afeab5c0e7a41c428c8` under SDK 8.0.426 with 0 errors / 45 warnings, and its contracts ran. Their report still says `fullGameParity=false`, `fullUnityParity=false`, and two unresolved built-ins. This is not a complete original-source build or parity pass.
- The current Work Mode shell has no .NET SDK, Unity Editor, `7zz`, or `UnityPy`; no .NET build was run in this evidence batch.

## Gates 1–10

1. **Asset/scene closure — open.** The original player archive contains `resources.assets`, `sharedassets*.assets`, and `Resources/unity_builtin_extra`. String scanning found `.308 Winchester casing` and related names, but no object IDs or geometry have been parsed. Missing GUID `b4b69dc6c74eeb54cbd22d28e37f9a19` / fileID `4300000` remains unresolved. The owner-approved compatibility mapping is limited to the casing root; do not substitute the five Marksman round/projectile references. Built-in records also remain unparsed.
2. **Unity compatibility/full source compile — open.** The previous build covers the selected runtime graph only.
3. **Original actors/lifecycle — open.** Current `WeaponHost` still defines/uses transport `Crisis.Runtime.Weapons.CharacterBody`. Authored Operator/Occupant runtime loading, all 19 limbs on live actors, and two independent lifecycles with Mission/global dependencies are unverified.
4. **Controls — partial.** The adapter tests pass; integration through the original actor and physical-device checks remain open.
5. **IK and physics — open.**
6. **Weapons and combat — open.**
7. **Physiology and treatment — open.**
8. **AI, missions, and progression — open.**
9. **Presentation and platform behavior — open.**
10. **Original-vs-port traces, exact-SHA browser gameplay, and iPhone 16 Safari acceptance — open.**

## Next bounded action

Use a Unity 2017.2-compatible asset reader, ideally the pinned `UnityPy==1.25.4` referenced by `scripts/recover_unity_2017_2_resources.py`, to parse the extracted player assets. Record Mesh object IDs, types, names, geometry hashes, and built-in records; resolve the original GUID only with evidence. The extractor and evidence note are in the repository. If the parser cannot be made available, record that exact tool blocker once and continue an independent source-backed gate. Keep changes on `develop/wasm`, commit verified slices directly, and do not deploy until all gates pass on one SHA.
