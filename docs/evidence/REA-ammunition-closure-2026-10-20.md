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