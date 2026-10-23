# Crisis Response parity handoff

## Current checkpoint

- Single development line: `develop/wasm`. This batch started from `92e68c43c1f5738cca16afd53605a92a18be3d10`; it adds the original-player extractor and evidence below. Recheck live HEAD before further edits.
- Open PRs: none at batch start. GitHub Actions remain disabled. No Site version was saved or deployed.
- The exact original archive is preserved by the user; archive SHA-256: `5a12ff494b26713d149f9be026619732247f817ddf8944a399b6ee1d12ea0ab3`.
- `scripts/extract_original_player_data.py` passed compilation and extraction: 411 files / 185,036,038 bytes; see `docs/evidence/ORIGINAL_PLAYER_ASSET_RECOVERY_2026-10-23.md`.
- Touch source blobs on develop at code SHA `a75983409c8aeeab76f53afeab5c0e7a41c428c8` exactly matched the focused local files. `node --test scripts/browser_input_test.mjs` passed 18/18, including stationary hold-fire, release/cancel, joystick aiming and rim sprint. This validates the input adapter only, not the integrated original actor or an iPhone session.
- The prior full selected-runtime build at `a75983409c8aeeab76f53afeab5c0e7a41c428c8` passed under .NET SDK 8.0.426 with 0 errors / 45 warnings, and runtime contracts passed while reporting `fullGameParity=false`, `fullUnityParity=false`, and two unresolved built-ins. Do not call this a full-source or parity pass. This WorkMode shell currently has no `dotnet`, Unity Editor, `7zz`, or `UnityPy`; no build was run in this batch.
- The private Sites deployment remains v31; it has not been updated or authenticated gameplay-tested.

## Gate state

1. Asset closure remains open. The original player archive contains Unity `resources.assets`, `sharedassets*.assets`, and `Resources/unity_builtin_extra`. A string scan found `.308 Winchester casing` and related names, but no serialized Mesh IDs or geometry have yet been parsed. The missing GUID `b4b69dc6c74eeb54cbd22d28e37f9a19` / fileID `4300000` remains unresolved. The approved compatibility substitution is limited to the casing root; do not substitute the five Marksman round/projectile references. Required built-ins remain unparsed.
2. Unity compatibility and complete original C# graph compilation remain incomplete. The previous SDK build covers the selected runtime graph only.
3. The source actor is not yet the sole runtime owner: the current source still includes transport `WeaponHost` / `Crisis.Runtime.Weapons.CharacterBody`. Authored Operator/Occupant loading and two-actor lifecycle with real Mission/global dependencies are unverified.
4. Input adapter tests pass; routing through the original actor and device acceptance remain open.
5–10. Original IK/physics, full combat, physiology, AI/mission progression, presentation/platform behavior, cross-scene original-vs-port traces, browser tests, and real iPhone 16 Safari acceptance remain open.

## Next bounded action

Use a Unity 2017.2-compatible reader, ideally the pinned `UnityPy==1.25.4` referenced by `scripts/recover_unity_2017_2_resources.py`, to parse the extracted original player asset containers. Record the serialized object IDs, types, names, geometry hashes, and built-in records; map only with evidence. Then rerun the dependency audit against the complete recovered scene/prefab source. If the parser cannot be made available in this environment, record that precise tooling blocker once and continue with the next source-backed runtime gate. Keep changes on `develop/wasm`, commit verified slices directly, open at most one PR when review materially helps, and do not deploy until all parity gates pass on one exact SHA.
