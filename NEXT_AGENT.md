# NEXT AGENT — Crisis Response WASM port

## Live checkpoint — 2026-10-20

- Repository: `ethan-bailey/crisis-response-port`.
- PR #42: [WIP: compile complete recovered CharacterBody and original dependency graph](https://github.com/ethan-bailey/crisis-response-port/pull/42), open, draft, unmerged; target `develop/wasm`.
- Feature branch: `feature/full-source-characterbody-integration`. The last observed PR head before this handoff refresh was `1fd5b03a876159ddf8c362c5acc73c5a8ad82f4e`; this documentation update will advance it. Do not treat docs-only commits as runtime-tested source.
- Latest runtime source checkpoint: `318fc92b87f6a2138e680e6f016ec636ceb8299a`. At that SHA, the runtime-check project build and `git diff --check` passed; the full runtime contract executable still exited 134 on missing serialized object resolution. The diagnostic improvement is not a passing gameplay or prefab gate.
- Base refs need reconciliation at the permitted final-verification merge boundary: PR base snapshot `5d83c4ef627baa6dc9009ef9a585c0b70e0d9435`; last observed live `develop/wasm` ref `2fc0a8e93be97c4d99123910fad4bd25798e71a8`. PR API reports `mergeable:false`. The 13 pre-existing changes are on the feature branch and are not integrated into `develop/wasm`.
- This execution environment has no repository checkout at `/workspace/scratch/483f9f6eb178` and the connected browser has zero tabs. A direct GitHub clone attempt could not authenticate (private repository; no Git credential prompt is available). Preserve this branch and all existing changes; do not use reset, clean, rebase, deploy, or merge now.

## Newly recovered Calibre evidence

The pinned recovery artifact was downloaded from GitHub Actions artifact **11903811738**, originating from recovery run **38582072502** ([run](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38582072502)). Verified archive SHA-256:

`cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`

Its recovery report ties the decompiled Assembly-CSharp to the supplied game archive (expected EXE MD5 `9e00a2b43b7140aaac57c828bd6566ce`; DLL SHA-256 `482f8bd6ef4546965b060463377470fc2da2db996de7ec16f29eb88f56a021b3`) and identifies an AssetRipper export. This is recovered serialized export evidence, not an assertion that the compiled archive itself contains the original editable Unity project or original editor-side .meta files.

In that export:

- `5.56-FMJ.prefab` references Calibre GUID `57cfc851e0e47374d98db0383b39cddf`, fileID `114724804207875695`.
- `Assets/Resources/weapons/calibers/Caliber 5.56x45mm.prefab.meta` resolves that exact GUID. Its prefab has a MonoBehaviour with that exact fileID, script GUID `8e75328a0c323b110b577a2dbfddf164`, and serialized `calibreName: 5.56x45mm`.
- The current `game/assets/weapon-runtime.json.gz` has a nested ammo package with only the 5.56-FMJ prefab and one script guidPath. It has no Calibre prefab record. The previous handoff's Calibre GUID was mistyped; use the value above from the packaged ammo reference and recovered export.
- The same 5.56-FMJ component has other unresolved object edges: Projectile `cf5bca2bd1c0369439bb8fbe00c9320d:114703437598541787`, Casing `9413de22ed594c44e8586fede9f5a355:114243736805655561`, UI ammo model `1b3dc24381623284e82335a132ad5dad:114243736805655561`, and Sprite `8464cbc48daa246479808d64b86fb8c2:21300000`. Recovered export paths are respectively `Assets/Resources/weapons/ammunition/projectiles/Projectile 5.56mm FMJ.prefab`, `Assets/Resources/weapons/ammunition/casings/Casing 5.56x45mm.prefab`, `Assets/Resources/weapons/ammunition/ui models/5.56x45mm FMJ.prefab`, and `Assets/Sprite/Icon 556 FMJ.asset`. Package the complete transitive closure; do not stop after adding Calibre.
- `AmmunitionLoader.Load` currently handles scalar, list, enum, and AnimationCurve fields, then fails closed for object references. `SerializedAssetResolver` can load a nested prefab component when its asset records and script guidPath are present. Sprite and all transitive records still require actual support and tests.

## Additional source and actor-asset verification — 2026-10-20

The recovered actor prefabs and metadata were compared byte-for-byte with the preserved feature-branch source-reference copies:

| Asset | SHA-256 | Export meta GUID | Result |
| --- | --- | --- | --- |
| Operator.prefab | `94bcda5de74f45d83f4bbf88e80874b8f77bf0339d307294187b085c03170536` | `2e00922aa35d9a8438c52fec69254605` | `cmp` exact match |
| Occupant.prefab | `9094b840c49d6c53ffa077c274561ab5cbeedb202f85dbaade023ad67cb8c419` | `8363ad318a650364ebdaa8c237f65bfe` | `cmp` exact match |
| Operator.prefab.meta | `8d01cf639201add925686afbe85b7b1fd8a7bd8ec6c41b6749d76b568ef63b0d` | same as above | exact SHA match |
| Occupant.prefab.meta | `3e39f95ea92613c4c4eb1c2c2c82775b9ed075cdf7a925d980ef5b3eca9c5737` | same as above | exact SHA match |

Commands: `sha256sum <preserved files> <recovered export files>` and `cmp -s <preserved prefab> <recovered export prefab>` for Operator and Occupant; both comparisons exited 0. This confirms the committed asset bytes and metadata match the checksum-pinned recovered export. It does not execute either prefab's lifecycle.

A read-only recursive GUID/PPtr traversal from the recovered Operator and Occupant exports found 252 reachable non-script/project assets plus 37 source scripts, and examined 1,200 non-null serialized object references. No PPtr GUID mapped to a missing asset/script path; no fileID mismatch was found when the target was a text Unity YAML asset. This is only partial closure evidence: 452 referenced audio/image binary objects do not expose an object record in the exported YAML and remain unverified by the walker, including textures with format-12 importer settings. Do not label the full dependency closure passed. The traversal is documented in the REA follow-up; it also distinguishes known external uGUI script IDs and AudioMixer exposed-parameter IDs from asset object PPtrs.

## Evidence and limits

- Recovery artifact acquisition and checksum passed. The repository package was fetched from PR head and decompressed for inspection; its exact compressed SHA-256 was `786e7ad6764058e59138d12e2631618da807596b510515317a1a544c35de2211`.
- The Calibre GUID-to-prefab/fileID link and the serialized `calibreName` value match between the recovered export and the ammo prefab reference. This verifies the recovered-export edge; it does not pass the project runtime loader.
- At source SHA `318fc92b87f6a2138e680e6f016ec636ceb8299a`, `dotnet build runtime/Checks/Crisis.Runtime.Checks.csproj --no-incremental -v:q -p:GenerateDocumentationFile=false` and `git diff --check` passed. The full runtime-check executable failed closed at the unresolved Calibre reference. Its test workflow lookup returned no PR workflow runs (no workflow ID or CI artifact).
- Earlier focused passes (inventory, actor ownership, enum/source identity, archive prefab provenance, audit tools, 69 EngineChecks, 120 FoundationChecks, runtime checks build, and actor package file/meta closure) are only evidenced at older source SHA `3979faa642053d0f584d2f91982c1b88cbe0eb62`; do not transfer those pass claims to newer code.
- Texture blocker remains: actual Operator prefab load previously failed closed on `Assets/Texture2D/head 5.png`, importer `textureFormat: 12`, without an authoritative runtime Texture2D mapping. The new recovery archive is now available for further texture provenance checks, but no Operator/Occupant load has yet passed against it.
- Not passed: complete transitive weapon asset closure; native weapon/runtime contracts; actual Operator and Occupant prefab lifecycle; independent ownership at runtime; full two-actor gameplay; WASM build from a single recorded tested source SHA; Chromium and WebKit gameplay against that build; local HTTP smoke test.
- No implementation source was changed in this execution. Artifact and decompressed package inspection occurred in temporary local files and are not committed evidence artifacts. Retain the Actions artifact/run identifiers above when continuing.

## Ordered next gates

1. Restore an authenticated repository checkout/Codespaces session and verify its worktree, current branch, and remote refs before edits.
2. Add the exact Calibre prefab records and script GUID mapping to the weapon package, preserving the matched GUID/fileID and serialized name; implement object-reference resolution in the loader.
3. Add a focused test proving loaded Calibre type, GUID, fileID, name, and object lifetime; run the native contract to expose the next missing edge.
4. Recover and package Projectile, Casing, UI ammo model, Sprite, and every transitive dependency from the pinned export; add explicit negative tests for missing/wrong GUID, fileID, and type.
5. Resolve Sprite's original serialized/importer identity without inventing mapping; keep unsupported formats fail-closed.
6. Run weapon and package-closure tests, then actual Operator prefab lifecycle and actual Occupant lifecycle with independent ownership checks.
7. Run full two-actor source lifecycle and gameplay flow; run the focused provenance, source identity, ownership, dependency, EngineChecks, FoundationChecks, and full native suites on one recorded source SHA.
8. Only after implementation acceptance passes, reconcile the live `develop/wasm` ref and integrate it with a normal merge commit immediately before final verification.
9. Build from the exact post-merge source SHA; run Chromium, WebKit, and local HTTP gameplay smoke checks against that build. Record exact commands, workflow IDs, logs, artifacts, and outcomes.
10. Refresh REA evidence, this handoff, and PR #42 description; confirm PR remains open and draft. Do not claim acceptance complete unless every required gate passed at the exact tested SHA.

## Latest continuation — 2026-10-20


## Post-push verification — 2026-10-20 — source SHA 26b07b660e892b9e97025bf2b8ebe700b7753764

The source commit is pushed to the feature branch. Runtime-check build, WASM publish (194 WASM files), local HTTP smoke (index, Blazor bootstrap, and WASM all HTTP 200), and Chromium/WebKit Playwright WASM input-adapter checks passed. The browser tests verify startup, 8 controls, 10 runtime contracts, and LEFT press/release state; the app explicitly says full gameplay is incomplete, so they do not pass two-actor gameplay. Full runtime contracts still fail closed on built-in Material fileID 10303. Built-in Shader 46 and imported Sprite mapping remain unresolved. Original actual-prefab lifecycle, independent two-actor gameplay, final post-merge build, and gameplay browser acceptance remain. See docs/evidence/logs/checkpoint-26b07b-verification.txt and the linked REA report. PR #42 remains open/draft and not mergeable; do not merge develop/wasm yet.
