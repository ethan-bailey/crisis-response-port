# NEXT AGENT — Crisis Response WASM port

Observed 2026-10-21: PR #42 branch `feature/full-source-characterbody-integration` at `645652e430e20e1aa8afa6667842b96728fb7bc3`. Live `develop/wasm` observed at `2fc0a8e93be97c4d99123910fad4bd25798e71a8`. Open parallel workstreams #56 and #57 remain separate; preserve their work. Latest comparable runtime evidence remains the recorded local scan of unchanged runtime code at 17b00ce: EngineChecks 69 assertions; FoundationChecks 120 assertions; runtime contracts stopped at Material 10303. No new runtime/browser acceptance is claimed.

## Completed source acquisition — no owner computer or resource upload needed

- `cf05f6c1f242fecd42a7ba209dcde7438c21f74f`: reproducible Linux recovery tool `scripts/recover_unity_2017_2_resources.py`.
- `645652e430e20e1aa8afa6667842b96728fb7bc3`: actual official source records in `source-reference/Unity/2017.2.0f3/builtin-resource-records.json`.
- Downloaded the exact Windows Editor installer and matching built-in shader archive from official Unity release revision `46dda1414e51`. The Windows installer includes the actual `win32_nondevelopment_mono/Data/Resources/unity default resources`.
- Extracted without executing the Windows installer or launching Unity. No activation, user computer, or Actions run is needed for this recovery.
- Material fileID 10303 exists in `Editor/Data/Resources/unity_builtin_extra`: name `Default-Material`, shader pointer `{m_FileID:0,m_PathID:46}`. Shader 46 is `Standard`. The pack preserves full parsed records, raw object bytes/base64 and hashes, plus 40 exact-version shader/include sources.
- Both common and target-specific `unity default resources` contain 83 objects and neither 10303 nor 46. Their serialized headers say `2017.2.0b11`; the files are nevertheless byte-for-byte payloads from the official 2017.2.0f3 installer. Keep this distinction. The built-in-extra header is 2017.2.0f3, target -2 (Editor).
- This supersedes the earlier proposed owner-data/defer disposition and local evidence commit `dfe918a` insofar as they imply the needed source records cannot be acquired. No owner decision is now required to acquire these records. Do not repeat the old search or ask the owner to supply them.
- These are authoritative exact-release Editor records, not proof that the game's built-player shader variants or browser rendering are identical. The pack explicitly retains those unverified boundaries.

## Validation performed

```sh
python3 -m pip install UnityPy==1.25.4
python3 scripts/recover_unity_2017_2_resources.py \
  --cache /path/to/cache --extractor /path/to/7zz \
  --output source-reference/Unity/2017.2.0f3/builtin-resource-records.json \
  --download
# Repeat with --check to reproduce from SHA-pinned inputs.
```

Actual local output after regeneration with `--check`:

```json
{"material":"10303 Default-Material","packSha256":"9e2015f94ab41aa19a4304104f9f24723809e71ab4b061313b67995954751ac8","records":2,"shader":"46 Standard","shaderSources":40,"status":"pass","targetResourceRecovered":true}
```

Python byte-compilation passed. The committed GitHub resource pack was fetched back and compared exactly with the tested local pack. The earlier recovery export's missing built-in record is no longer an acquisition blocker.

## Remaining implementation list — continue without another broad scan

1. **Material/Shader runtime binding:** actionable now. Consume the recovered records from the checked pack in the existing serialized asset resolver/package pipeline. Preserve original GUID/fileIDs and material defaults; resolve its original shader pointer to 46. No guessed material or dummy shader candidate is needed. Entry points: `runtime/Engine/SerializedAssetResolver.cs`, `scripts/extract_runtime_scene.py`. Source acquisition is done; runtime resolution and rendering acceptance are not.
2. **Imported Sprite/Texture records:** unresolved runtime importer/object-ID mapping. Existing triage identifies Sprite GUID 8464cbc48daa246479808d64b86fb8c2/fileID 21300000 and Operator head texture format 12. Work these separately; do not assume new built-in records solve import identity.
3. **Original controller playback:** code/acceptance incomplete. Reference audit still reports false; the actual original controller/callback graph must execute on FullSource.CharacterBody. Composition fixtures are not playback. Use existing #57 work and source provenance; no parallel mutable actor/controller.
4. **Actual Operator/Occupant lifecycle and independent ownership:** original prefab provenance exists; complete lifecycle not passed. Preserve one authoritative FullSource.CharacterBody graph and per-actor physiology/equipment ownership.
5. **Independent two-actor gameplay:** spawn/equip/move/hit/physiology/destruction flow remains unverified.
6. **Final exact-source WASM, Chromium/WebKit gameplay and HTTP smoke:** run after local source/lifecycle/gameplay implementation; startup/input evidence does not satisfy gameplay.
7. **Required CI:** quota was exhausted. No workflow was manually dispatched in this source acquisition batch. Name the exact required workflow/check if a specific item ultimately needs Actions.

Resolved earlier: mixer parameter GUID false positives; Calibre packaging/identity contract; owner-approved scoped .308 casing root mesh replacement (approximation, not recovered identity). Preserve their existing evidence and do not reopen them without new failures.

## Preservation and boundaries

The original Codespace checkout was fast-forwarded to 7669605 and then gained local evidence-only commit dfe918a in the preceding turn. Its pre-existing untracked `runtime/Browser/wwwroot/browser-input.mjs` remains untouched; preserve it and existing stashes. New source-recovery commits above were written to the remote PR branch through the connector, so reconcile that local evidence commit non-destructively before the next Codespace update. Do not reset/clean or overwrite user work.

No preview deployment, branch integration, rebase or merge was performed. Continue on PR #42 and follow AGENT_INSTRUCTIONS.md. The user requires every remaining item to be completed or individually dispositioned; merge status is not a stopping condition.
