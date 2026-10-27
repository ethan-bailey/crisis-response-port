# REA: Mission startup blocked by null weapon qualifier — 2026-10-27

## Exact failure

Actions run #120 (`40439724709`) tested public `develop/wasm` SHA `79fddeb94e4ad4fe4550e0ff0f7ff5df28a7bdfd`. Chromium IndexedDB/audio job `121976902804` passed. Main job `121976902566` passed FoundationChecks, Release/WASM publication, and recovered source/index verification; its full Mission package source-entry contract failed.

Trace: `OperatorSpawner.Awake -> SpawnTeam -> CharacterBody.InitializeEquipment -> EquipmentHelper.SpawnWeapon -> WeaponBase.Deserialize -> WeaponComponent.Compatible -> WeaponComponentSlot.QualifierMatch`. The thrown `NullReferenceException` is at `qualifiers.Split(...)`. During normal EntryTeam spawn, the original M4A1 default-component compatibility check receives a null qualifier and aborts equipment initialization. This can leave a partial actor visible while the Mission is not playable. It does not alone establish why the rendered character appears airborne.

## Source behavior and bounded change

Both original qualifier matchers already use `string.IsNullOrEmpty` to define their empty/no-qualifier cases, but call `Split` on the nullable argument before those semantics can apply. Normalize null to `string.Empty` immediately before `Split`. This preserves the existing token/compatibility branches. The regression checks null, empty, matching, and nonmatching values for both source methods. The existing full Mission source-entry contract then exercises the authored Operator spawn, M4A1 loadout and Mission initialization.

## Acceptance and outstanding checks

Run the focused source contract and broader checks on the exact pushed SHA through Actions: Node/Python/runtime/engine/foundation, Release/WASM, recovered source/profile/package integrity and retention, and independent Chromium IndexedDB/audio. Keep the new package Mission contract failure visible until that exact run passes. iPhone rendered spawn position, ground contact, operator pose, and apparent gameplay start remain separate owner/device acceptance; do not claim the qualifier fix resolves the airborne visuals.


## Follow-up Actions run #121

- SHA `1390af2e7db4ca24dafa7bf37aaf843ff17e5b60`; run `40440324867`. Chromium job `121978853027` passed. Main job `121978852778` failed before FoundationChecks: the new regression expected `M4A1` to match a `M4` qualifier, but the original implementation tokenizes qualifiers and compares each exact token. The test is corrected to use exact `M4` and retains null/empty/negative cases; no product matcher change is indicated by this failure.


## Follow-up Actions run #122

- SHA 4eac44e91584d75c9dc8a5aa1e80839df7cda300; run 40440483156. Main job passed runtime, FoundationChecks, Release/WASM and publish output, then provenance/index verification failed on WeaponComponent.cs. The recovered matcher was modified by the prior attempted fix; the archive verifier correctly rejected it. Independent Chromium cache/audio job 121979382034 passed.
- Revised falsifiable cause: original method receives null from an imported MonoBehaviour compatibility field when the serialized field is omitted or explicitly null. Keep original method/source unchanged; normalize the four qualifier fields at SceneLoader.AssignSerializedFields after imported fields are applied. Regression loads actual Crisis.Runtime.FullSource.WeaponComponent and WeaponComponentSlot types from serialized component records, including an omitted and explicit-null qualifier, and checks non-throwing empty qualifier paths. The full Mission source-entry contract is still needed to verify authored Operator spawn and Mission initialization.
- Local source verification is unavailable for .NET: dotnet --version returns command not found. GitHub Actions must run the pinned .NET 8.0.426 suite on the new exact SHA. No Site deployment was made.


## Follow-up Actions run #123

- SHA 552319ed9a3a0e4f93e1622351def0e4f79fafe2; run 40441042407. Node/Python/runtime/browser-audio/engine checks and independent Chromium job 121981196135 passed. Runtime contracts include the serialized import regression for actual FullSource types. Main job 121981196682 failed FoundationChecks compilation because its reduced assembly excludes the recovered FullSource types while SceneLoader referenced them directly (CS0234). Build, provenance and Mission package steps were skipped.
- The loader adapter must not create a compile-time dependency from Engine/SceneLoader to FullSource. Use exact type full names plus reflected string fields, retaining source-file hashes. The authored Mission source-entry gate from #120 remains open.


## Exact-source Mission acceptance — Actions #124

- SHA 27ab7a17a9c18ff32a4c80e23aebd946a02afe78 passed Actions run 40441240466: main job 121981831401 and independent Chromium job 121981831528 succeeded. The pinned .NET 8.0.426 workflow passed source provenance/index, Release/WASM, the full six-root Mission package build/verify/retention, and the --mission-source-entry-contract path after the null qualifier failure observed in #120. The archived FullSource matcher blobs match the source-verified originals.
- The same exact run passed full-profile Chromium IndexedDB: 1,919/1,919 verified assets persisted, 8,075 ms cold load, 5,967 ms cached reload, 0 warm network requests. This is source/browser-loader evidence, not a rendered iPhone Safari or visual actor-pose acceptance.
- Package artifact 12706855071: 138,597,540 bytes, artifact SHA-256 c69c9134ea62da8f5b20a6e955b7f38bbc10d6654f8d73930a35ec83a6af890e, expires 2026-11-10. Deterministic package tar.gz SHA-256 aa351e1c83bdbdd78c70e34c08ba0dc4da63bac6a593ce3156b8c846ed0aa432.
- Thus the null-qualifier failure that aborted the headless source Mission path is resolved on this SHA. The airborne appearance and detached hand/gun reported in the user's screenshot are still unverified visual issues; do not infer their resolution from source-entry success.
