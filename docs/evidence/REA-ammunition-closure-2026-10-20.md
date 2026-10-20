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
