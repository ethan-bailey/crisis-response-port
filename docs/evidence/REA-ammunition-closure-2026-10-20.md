# Ammunition dependency recovery checkpoint — 2026-10-20

## Recovery provenance

- Recovery workflow run: 38582072502; artifact ID: 11903811738.
- Recovery archive SHA-256: cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d.
- Original AssetRipper export path used in Codespaces: /tmp/cpr-recovery-artifact-11903811738/export/ExportedProject.
- The compressed package now includes 29 source asset records and 33 GUID-to-path mappings, including the ammo root, Calibre, Projectile, Casing, UI ammo model, Sprite, and their reachable packaged assets. The same original files and .meta files are retained under source-reference/Original/Assets.
- Recovery comparison check: every packaged asset byte and every mapped source file and GUID metadata file was compared against the extracted recovery export. Result: ASSETS 29 GUID_PATHS 33 ERRORS 0.
- game/assets/weapon-runtime.json.gz SHA-256: a0bcb158abc0ccf100ccea57b51a8c15f80fb81d621fdbc983d2ceeb0750b7c2.

## Verification executed

- dotnet build runtime/Checks/Crisis.Runtime.Checks.csproj --no-incremental -v:q -p:GenerateDocumentationFile=false — exit 0.
- python3 scripts/verify_full_characterbody_enums.py — passed for original WeaponMode and LimbId definitions.
- python3 scripts/verify_original_arm_controller_references.py — status pass, original archive digest matched. Its report still says controllerPlaybackImplemented: false; this is a source-reference check only.
- python3 scripts/verify_foundation_sources.py — passed: 479 source identities, pinned plugin assembly, and no recovered rule-body edits.
- Runtime contract command: dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-build — exit 134. The run first prints “PASS: source prefab record references, deferred Awake, and two independent actor clones” and “PASS: original Calibre GUID, fileID, type, path, serialized name, and prefab lifetime”. It then fails closed while loading the original Projectile fragmentMeshes because the recovered prefab references Unity built-in Material GUID 0000000000000000f000000000000000:10303, for which this runtime has no source-faithful mapping. Built-in Shader fileID 46 is also unresolved in the scanned closure. No stand-in was created.
- git diff --check — passed.

## Current limits and next gates

- The Calibre nested-prefab resolver previously returned the correct serialized calibreName but omitted the referenced asset GUID, fileID, and path. The resolver now records those identities on the exact referenced component; the focused Calibre identity contract passes.
- The full weapon loader remains blocked by the original built-in Material and Shader references. Imported Sprite object mapping is also not yet demonstrated by a successful runtime resolution.
- ScenePrefabContracts reports only source record references, deferred Awake, and independent actor clones; it does not establish full original Operator or Occupant lifecycle acceptance.
- No full two-actor gameplay flow, final WASM build, Chromium/WebKit test, or local HTTP smoke test was run in this checkpoint.
- At execution time the feature checkout base was a04f7f58f9ae9b7e202af814a4fabd85976e17da and the implementation changes were still uncommitted; do not treat that base SHA as the tested implementation SHA. The PR head remains the prior remote head until the work is committed and pushed.

## Later verification on committed source SHA 26b07b660e892b9e97025bf2b8ebe700b7753764

This later section supersedes the earlier statement above that no WASM publish, browser check, or local HTTP smoke had run. Those pre-merge checks have since run against the committed checkpoint SHA. They do not pass the missing two-actor gameplay gate.

- Source SHA: 26b07b660e892b9e97025bf2b8ebe700b7753764; commit pushed to the existing feature branch. GitHub reports the PR open, draft, unmerged, and mergeable=false. The PR-workflow lookup returned no runs for this SHA.
- Runtime checks build: exit 0. Full contract run: exit 134 after the exact Calibre and partial actor-clone checks pass; fails closed at builtin Material fileID 10303. Builtin Shader fileID 46 and Sprite imported-object mapping still unresolved.
- Browser WASM publish: dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release --no-restore -o /tmp/cpr-published-26b07b-canonical; exit 0, 194 WASM files and index.html. To match the workflow, game/browser-input.mjs was copied temporarily to runtime/Browser/wwwroot/browser-input.mjs; that destination was absent before the operation, and the temporary file was removed after publish. git status remained clean.
- The canonical output passed local HTTP smoke: index 200 (3110 bytes), blazor.webassembly.js 200 (60300 bytes), and a WASM file 200 (17173 bytes).
- Playwright 1.64.1 checks passed on Chromium 156.0.8078.4 and WebKit 27.2. Each engine loaded the WASM app (HTTP 200), reached its input-ready state, saw 8 controls and “PASS · 10 verified runtime contracts”; the LEFT control set original input left.Pressed=true while held and false after release. No page errors were reported.
- The app explicitly displays “Full gameplay is still incomplete.” These tests validate WASM startup and the browser input adapter only. They are not Chromium/WebKit gameplay acceptance; full actor spawn/equip/movement/hit/physiology/destruction flow remains unimplemented/unverified.
- Source-effect clock tests passed 3/3. Original WeaponMode/LimbId and source/controller checks passed; the controller audit still reports controllerPlaybackImplemented=false. The 479-source identity audit passed.
- Compact command/result record: docs/evidence/logs/checkpoint-26b07b-verification.txt. Raw Codespaces logs are under /tmp/cpr-*-26b07b.log for this session.

The recorded source SHA is a pre-merge implementation checkpoint. Do not merge develop/wasm or report acceptance complete until full source-faithful asset resolution, actual Operator/Occupant lifecycle, two-actor gameplay, and post-merge exact-SHA checks pass.


## Fresh Codespaces validation and asset closure — 2026-10-20

- Full suite ran from Codespaces local HEAD `5833ab50742e8103cc1e2db4c80dfb59cd2c797c` (`bash /tmp/pr42-local-scan.sh`). The implementation sources are from `26b07b660e892b9e97025bf2b8ebe700b7753764`; no runtime source files were modified in the worktree.
- Summary: 16 of 17 commands exited 0. `dependency_inventory --check`, actor/source audits, Python and JavaScript tests, 69 isolated engine assertions, 120 foundation assertions, runtime checks build, and `git diff --check` passed. The controller audit exits 0 but reports `controllerPlaybackImplemented: false`. `runtime_contracts` fails closed with exit 134 on the next Projectile after the Calibre PASS.
- Calibre output: `PASS: original Calibre GUID, fileID, type, path, serialized name, and prefab lifetime`.
- Projectile failure: `Unity built-in object 0000000000000000f000000000000000:10303 has no source-faithful runtime mapping for expected Material; refusing a synthetic substitute`.
- Per-gate logs: `/tmp/pr42-local-scan-20261020/*.log`; summary `/tmp/pr42-local-scan-20261020/summary.txt`.
- Scene audit command: `python3 scripts/scene_dependency_audit.py /tmp/cpr-recovery-artifact-11903811738/export/ExportedProject /tmp/pr42-original-scene-dependencies.md`; output: 417 assets, 708 edges, one unresolved GUID (`b4b69dc6c74eeb54cbd22d28e37f9a19`, Mesh fileID 4300000).
- Package command: `python3 scripts/extract_runtime_scene.py /tmp/cpr-recovery-artifact-11903811738/export/ExportedProject /tmp/pr42-scene-package.json`; output: 1,382 assets, 21,366 records, 14,249 object references, 508 binary assets, 19,016,011 bytes, 11 unresolved GUID references. Five missing GUID identities come from `Master mixer.mixer`; six references are to the missing .308 Mesh GUID.
- The refreshed dependency inventory contains 659 sources (up from 654) and passes its check locally. Its five new source-reference paths are `AmmunitionBase.cs`, `Calibre.cs`, `Casing.cs`, `Projectile.cs`, and `UIAmmoRenderItem.cs`; ownership ambiguity for `Calibre` and `UIAmmoRenderItem` remains visibly flagged.
- This does not establish actual Operator/Occupant lifecycle, controller playback, full two-actor gameplay, or browser gameplay. Existing browser startup/input results remain limited to the prior pre-merge build.
- Artifact provenance: workflow run 38582072502, artifact 11903811738, SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
## Continuation — exact-source build, prefab provenance, and Actions availability (2026-10-20)

- Feature branch source SHA for the local compile: 61e64b27f010204fb1cb0e29e0e27000bc11b959. The working tree was clean, and runtime, game, and source-reference/Original had no diff from implementation SHA 26b07b660e892b9e97025bf2b8ebe700b7753764.
- Exact command: dotnet build runtime/Crisis.Runtime.csproj -v:q. Result: exit 0, Build succeeded, compiler_error_count=0. Log: /tmp/pr42-runtime-build-61e64b2.log; SHA-256 c8f868835dc96319240f92812e2ef7832455120bae6c8a8d56a04ab771b97cb. This only verifies compilation of the runtime project, not WASM gameplay acceptance.
- Inventory check: python3 scripts/inventory_source_dependencies.py --check passed at 61e64b27, reporting 659 sources, 1,695 symbols, 1,329 reachable files, and 119 ambiguous names needing ownership review.
- Prefab provenance comparison: tracked Operator and Occupant prefab and .meta files were compared byte-for-byte with the original recovery export at Assets/Resources/characters. Operator prefab SHA-256 94bcda5de74f45d83f4bbf88e80874b8f77bf0339d307294187b085c03170536; meta SHA-256 8d01cf639201add925686afbe85b7b1fd8a7bd8ec6c41b6749d76b568ef63b0d; GUID 2e00922aa35d9a8438c52f1ec69254605. Occupant prefab SHA-256 9094b840c49d6c53f1fa077c274561ab5cbeedb20f85bdaae023ad67cb8c419; meta SHA-256 3e39f95ea92613c4c4eb1c2c2c82775b9ed075cdf7a925d980ef5b3eca9c5737; GUID 8363ad318a650364ebdaa8c237f65bfe. All four cmp checks passed. This confirms byte provenance and serialized preservation, not runtime lifecycle or ownership.
- Calibre contract remains a pass with exact output: PASS: original Calibre GUID, fileID, type, path, serialized name, and prefab lifetime. The full scan at local HEAD 5833ab50742e8103cc1e2db4c80dfb59cd2c797c used implementation SHA 26b07b660e892b9e97025bf2b8ebe700b7753764; 16 of 17 commands passed. Runtime contract then failed closed on Projectile's built-in Material GUID 0000000000000000f000000000000000, fileID 10303. No runtime/original-source files changed between that implementation and 61e64b27.
- Current PR #42 head 61e64b27 has no workflow runs or combined status checks in the connected GitHub view. The owner reports Actions credits will remain unavailable for roughly one month from 2026-10-20. No workflow was dispatched in this continuation. Do not delete artifacts to try to reclaim minutes: artifact deletion affects storage, not already-used Actions minutes. GitHub documents included runner minutes and artifact storage separately ([Actions billing](https://docs.github.com/en/billing/concepts/product-billing/github-actions)); deleting artifacts reclaims storage ([artifact removal](https://docs.github.com/en/actions/how-tos/manage-workflow-runs/remove-workflow-artifacts)). Do not enable paid usage or cancel unrelated workflows.
- PR remains open, draft, not mergeable. The full failing/incomplete gate list and proposed dispositions are in docs/evidence/logs/triage-2026-10-20-pr42.md; actual actor lifecycle, controller playback, independent two-actor gameplay, browser gameplay, final exact-source WASM build, and final HTTP smoke remain unverified.

## Mixer parameter GUID correction — 2026-10-21

The original ammunition closure's five unresolved GUID reports were false positives. Inspection of the pinned recovered Assets/AudioMixerController/Master mixer.mixer showed these values only as keys in m_FloatValues snapshot maps. They are mixer parameter IDs, not serialized Unity asset pointers. The extraction code now emits mixer dependency edges only from records containing both fileID and guid; the regression fixture contains all five IDs plus a genuine pointer and verifies the latter remains.

The ammunition bundle was regenerated from /tmp/cpr-recovery-artifact-11903811738/export/ExportedProject into work/pr42-session/mixer-guid-fix/scene-package.json. The strict command passed:

python3 scripts/package_ammunition_closure.py game/assets/weapon-runtime.json.gz work/pr42-session/mixer-guid-fix/scene-package.json --check

Result: 29 assets resolved, 5 source scripts, 1 built-in GUID, and no unresolved ammunition references. Supporting checks passed: Python syntax compilation and all 10 audit_tools_test.py tests. The change is pushed in 1cd7294287021c0c5e5cb7b2410932108b9a83a5.

This does not resolve the separate source-faithful Material mapping failure. The runtime contract still stops at Assets/GameObject/Projectile fragments.prefab because built-in Material 0000000000000000f000000000000000:10303 is absent from the recovered source package. The contract log is retained locally at work/pr42-session/mixer-guid-fix/full-contract.log; no material substitute was introduced.


## Owner-approved .308 casing mesh replacement — 2026-10-21

The repository owner explicitly approved using the recovered `308 Winchester casing hi.asset` as a compatibility replacement for the missing .308 casing mesh, and asked that the substitution be documented. This is a disposition decision, not evidence that the replacement has the missing asset's identity.

- Missing source mesh GUID: `b4b69dc6c74eeb54cbd22d28e37f9a19` (Mesh fileID `4300000`).
- Approved candidate: `Assets/Mesh/308 Winchester casing hi.asset`, whose own .meta GUID is `cd74722033652fb409431b2947740490` (fileID `4300000`).
- Recovered asset SHA-256: `a31736991aa6345f4d9dffa4a5532681a184ac88520867a79fa7cbef859392b6`; .meta SHA-256: `c0e0dd3b07baa024a0d07ec04bb16ffbb9a60a5d91bd173b237f8717a170c24c`.
- Candidate bounds extents: approximately `0.0060008 × 0.0060008 × 0.0255900 m`. It has 494 vertices and 494 triangles. These values describe the recovered candidate only.
- Apply the mapping only to the casing root MeshFilter in `Assets/Resources/weapons/ammunition/casings/Casing .308.prefab`. Leave the child Projectile mesh and projectile-round prefabs untouched unless their individual geometry is separately verified and approved.
- The existing `scripts/replace_missing_308_casing_mesh.py` scopes the GUID change to that root MeshFilter, verifies the candidate .meta GUID, and emits a JSON disposition record. Run it with `--write` against the recovered AssetRipper ExportedProject, then regenerate and test the runtime closure from that patched project. Retain the original export unchanged.

The earlier 2026-10-14 report correctly recorded that this candidate was then unapproved. This owner decision supersedes that pending-disposition statement. The substitution is an accepted approximation for the casing prefab, not a source-faithful identity recovery, and does not resolve the Unity built-in Material/Shader records.

## Unity 2017.2 built-in resource records — resolution path

The remaining record failure is a different issue: `Projectile fragments.prefab` refers to Unity built-in Material GUID `0000000000000000f000000000000000`, fileID `10303`, and built-in Shader fileID `46`. These records are Editor-provided and are absent from the recovered game-content export. Resolve them from the exact Unity `2017.2.0f3` Editor resources (the version recorded for the original player): recover `Resources/unity default resources` and `Resources/unity_builtin_extra`, parse the serialized object tables, and add the real Material/Shader records and dependencies to the runtime source-resource resolver. Preserve each resource's original fileID, class/type, shader pointer, defaults, and asset provenance. Then test Material 10303 and Shader 46 by exact ID and type, plus the existing projectile contract. Do not synthesize a Material or assume a modern Unity built-in has the same serialized record.


## Applied approved .308 casing mapping — 2026-10-21

- Downloaded the original recovery artifact from run 38582072502 into the existing Codespace recovery directory; left the archived ZIP and export provenance intact. Applied `scripts/replace_missing_308_casing_mesh.py --write` to the extracted working copy.
- The script changed only the casing root MeshFilter in `Assets/Resources/weapons/ammunition/casings/Casing .308.prefab`, from missing GUID `b4b69dc6c74eeb54cbd22d28e37f9a19` to recovered candidate GUID `cd74722033652fb409431b2947740490`. It verified the candidate .meta and reported `identity_verified: false`; the Projectile child was left unchanged. The script wrote `reports/308-casing-mesh-substitution.json` in the extracted working copy.
- Re-extracted the runtime scene package from the patched export: 1,382 assets, 21,366 records, 14,249 object references, 508 binary assets, 19,015,302 bytes.
- Regenerated `game/assets/weapon-runtime.json.gz` with `scripts/package_ammunition_closure.py`. Result: 29 assets, 5 source scripts, 1 built-in GUID, and zero unresolved GUID dependencies. The follow-up `--check` reports no stale script maps, no missing GUID metadata, and no unresolved dependencies.
- Updated bundle SHA-256: `d60c4d786643ac0d07faef93c6ffad9190997a5b93d09241276e8c4602e57a3`. `git diff --check` passed.
- Post-change runtime contract run: source prefab-reference/deferred-Awake/independent-clone check passed; exact Calibre identity check passed; then the contract still fails closed with `System.NotSupportedException` at Unity built-in Material GUID `0000000000000000f000000000000000`, fileID `10303`. The casing mapping removes the unresolved .308 source mesh dependency from the recovered ammunition closure; it does not provide built-in Unity resource records.

The owner-approved compatibility mapping is now applied and packaged. Do not describe the replacement mesh as an identity recovery. The separate built-in Material/Shader resolver work remains open.
