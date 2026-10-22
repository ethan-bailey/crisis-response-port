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

## Current continuation checkpoint — 2026-10-21 (supersedes the older remaining list above)

This is the active handoff for PR #42. Current code head before this handoff note: `6cb600b1695cca3bef59b3c0a0a810b4a6d33a39`. PR #42 remains a draft. The PR description now reflects the published state and exact verification gates.

- Do not reacquire Unity data. The verified Windows x86 Mono resource cache is already retained at `/workspace/scratch/c3ca869b77d9/unity-cache/Unity-2017.2.0f3-Windows-x86-Mono-resource-cache.zip`, SHA-256 `15deb1638c08b92d8b9f505486ce421d31e0a3df32f5fe359206ece3d8e23101`; the same canonical cache is preserved in the persistent Library.
- Completed on the branch: Unity Material 10303/Shader 46 package and resolver mapping; Sprite class ID 213 hydration mapping; arm-physics fixture joins `original-arm-physics.json`; original IKArmController/sequencer playback and two-actor callback-isolation contract; Operator/Occupant lifecycle component-ownership assertions. The local scratch snapshot at 7669605 is older than the active branch and must not be treated as the current checkout.
- Latest native run evidence predates the arm-physics fixture merge and controller playback contract. That earlier run compiled and passed deferred prefab Awake/two-clone and Calibre identity, then stopped at missing fixture `armPhysics`. The fixture now joins the recovered source object, but this exact head has not been rerun.
- Exact native check still required: workflow **Execute recovered original code with runtime services**, step **Compile and test runtime against original components and scenes**. Exact full-source compile checks: workflow **Compile recovered full CharacterBody dependency closure**, jobs **foundation-contracts** and **compile-closure**. Actions quota remains unavailable; do not dispatch these workflows.
- Exact browser acceptance still required: workflow **Execute recovered original code with runtime services**, step **Execute original methods and original scene graph in WebAssembly**, plus its Chromium/WebKit install and localhost HTTP steps. Do not treat startup or input smoke as gameplay acceptance.
- Next implementation target is production actor/spawner integration. `runtime/Browser/Program.cs` still initializes `Crisis.Runtime.Weapons.WeaponHost`; that host constructs the transport `Crisis.Runtime.Weapons.CharacterBody`. The original `Crisis.Runtime.FullSource.OccupantSpawner` exists, but its mission/global-context equipment flow is not connected to the browser runtime's authoritative source actor. Continue with the source-owned actor path and test spawn, equip, movement, hit/physiology isolation, and destruction as separate behaviors.
## PR #42 gate checkpoint — 2026-10-22

- Checkpoint before this handoff-only commit: PR #42 head `a61cbfe0a079924c9d13c85d21b2f925d08fe2c3`; open PR set was #7, #42, #56, and #57. The handoff commit follows this source commit and does not change runtime source.
- PR #42 remains draft and is not merge-ready. The source commit `a61cbfe` was pushed to the existing feature branch. The full-source build `dotnet build runtime/Crisis.Runtime.csproj --no-incremental` exited 0 on that source commit: 45 warnings, 0 errors (15.65 s). The full-source result on prior exact head `9479dc6d5140799b39780900c1d4add617cf9a7a` was also 45 warnings, 0 errors. The separate 0 warnings/0 errors report was an incremental no-op, not a source recompile; both diagnostic counts refer to `runtime/Crisis.Runtime.csproj`, not a runtime-only subset.
- Local ammunition closure packaging passed with 1,382 package objects and no unresolved ammunition package fields. Official Unity 2017.2.0f3 resource recovery supplied the Mesh 10210 data and was included in `a61cbfe`.

### SceneLoader blockers and individual dispositions

| Scene | Reference | Disposition |
| --- | --- | --- |
| Marksman Range | Mission component fileID 2518, field `secureWeaponMarker` points to `Assets/Resources/markers/Marker secure.prefab`; nested unsupported built-in `SpriteRenderer` field. Runtime has no `SpriteRenderer` implementation. | Needs owner decision whether to expand this PR with renderer support. |
| Mission 01 | GameObject fileID 6932, `m_Sprite`, zero GUID, built-in Sprite fileID 10907. No matching entry is in recovered pinned Unity resource records. | Needs owner decision on exact-source recovery versus an approved visual substitute. |
| Mission 01 | GameObject fileID 6957, `m_Sprite`, zero GUID, built-in Sprite fileID 10913. No matching entry is in recovered pinned Unity resource records. | Needs owner decision on exact-source recovery versus an approved visual substitute. |

The native scene contract stops on the Marksman Range reference. The Mission 01 rows are independently unresolved in the extracted package. Native original-scene, spawner-driven scene, and exact-source browser smoke have not passed and remain blocked by these dispositions. The Actions-only Runtime Foundation check is `needs CI`; Actions minutes have been exhausted since 2026-10-19. Do not wait on Actions.

### Merge-readiness blocker

A non-mutating merge-tree check against local `origin/develop/wasm` showed conflicts in `.gitignore`, `AGENTS.md`, `NEXT_AGENT.md`, `runtime/Checks/Program.cs`, and `runtime/Engine/PresentationHost.cs`. One bounded resolution attempt was made. The combined source then failed full compile with 14 duplicate-member errors involving original Animator parameter accessors (`StringToHash`, `SetBool`, `SetFloat`, `GetBool`, `GetFloat`), `CapsuleCollider.Raycast`, and `ArmorClass.PenetrationTest`, among duplicate declarations. The merge was aborted; the isolated tree is restored to source commit `a61cbfe`. Nothing from the uncompilable merge tree was pushed.

Disposition: **needs owner decision** to reconcile the overlapping engine API and native-check changes between the PR history and `develop/wasm`. Do not mark PR #42 ready or merge it until this is resolved. Sites/hosting work has not started because the merge-ready trigger is not met.

### Preserved Codespaces checkout items

- Local commit `dfe918a6bd3be3727355acd0033a41a91a5329c7` is patch-equivalent to the corresponding remote commit (`git cherry` returned no unique patch). Disposition: duplicate; no cherry-pick or push needed.
- `runtime/Browser/wwwroot/browser-input.mjs` is a generated workflow output copied from tracked `game/browser-input.mjs`. The preserved local copy has a different SHA-256 and is stale; regenerate it from the tracked source for checks. It is not an independent verification input.


## Session 1 owner-decision work (2026-10-22)

Checkpoint: PR #42 source head was 3a22f709c70be577c881a7300c1b6926559c0a21. The implementation commit is fcfe355d6928934a0331faceb1ed26e000bc128e.

The fresh full-source compile ran as `dotnet build runtime/Crisis.Runtime.csproj --no-incremental` on fcfe355d6928934a0331faceb1ed26e000bc128e: 45 warnings, 0 errors, exit 0 (15.76 seconds). This is the full-source runtime project compile. The 0-warning/0-error report on 9479dc6 was an incremental no-op, not a full-source compile; the recorded full-source result on 9479dc6 is 45 warnings, 0 errors.

The generated scene execution package was built from work/recovery/export/ExportedProject to /tmp/cr42-session1-scene-package.json. Summary: 1,382 assets, 21,366 records, 6 unresolved GUIDs, 14,249 object references, 508 binary assets, 190,490,359 bytes. The recovered built-in resource record pack SHA-256 is ad613f8d50a4d58be86781e0b332f8410727c8b2a77306d100ee3bc162e5272e.

Validation command: `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -- /tmp/cr42-session1-scene-package.json`. RuntimeContracts, ScenePrefabContracts, ArmSequenceContracts, PresentationContracts, WeaponContracts, and BallisticContracts ran before scene loading without throwing; WeaponContracts includes the original Calibre identity/lifetime contract. Scene loading then failed at Marksman Range fileID 2518 (secureWeaponMarker -> Assets/Resources/markers/Marker secure.prefab). The strict prefab loader reported unsupported SpriteRenderer.m_WasSpriteAssigned and SpriteRenderer.m_MaskInteraction. The bounded m_MaskInteraction support attempt added the SpriteRenderer.maskInteraction property, but the loader still rejects that serialized field. No general renderer work was attempted. ResourcePrefabContracts and SpawnerGameplayContracts were not reached after the scene-load exception. Mission 01 was not reached by this run.

Individual reference dispositions:
- Marksman Range fileID 2518, Marker secure.prefab, m_MaskInteraction: needs owner decision for any further attempt; the one bounded attempt did not make the loader accept the serialized member.
- Marksman Range fileID 2518, Marker secure.prefab, m_WasSpriteAssigned: needs owner decision; outside the one-field scope and still rejected by the strict prefab loader.
- Mission 01 fileID 6932, m_Sprite -> built-in Sprite 10907: exact Sprite Background recovered from the pinned Unity 2017.2.0f3 Editor unity_builtin_extra using the existing recovery pipeline; raw SHA-256 7e986d3388d894a5bec0e2240d18c99114607491cb58cdcf05e982cccd501855. Disposition: recoverable mapping; package mapping added, scene load not reached.
- Mission 01 fileID 6957, m_Sprite -> built-in Sprite 10913: exact Sprite Knob recovered from the same source; raw SHA-256 a44cbb0e083d3e89f9b26062a07b4d5e49af3854a91ed794021ecb8a5a0284f. Disposition: recoverable mapping; package mapping added, scene load not reached.

Preserved Codespaces checkout dispositions: local commit dfe918a is a duplicate of its remote commit, so no push or cherry-pick is needed. browser-input.mjs is generated from tracked source and is not needed for this verification. The untracked global.json remains untouched.

Session 1 has no develop/wasm merge. The scene-load result above is a verified failure disposition; start Session 2 only after this head and handoff update are pushed.
