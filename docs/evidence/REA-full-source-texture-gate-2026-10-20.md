# REA checkpoint — full-source CharacterBody prefab/runtime gate (2026-10-20)

## State checked before edits

- Repository: `ethan-bailey/crisis-response-port`.
- Feature branch: `feature/full-source-characterbody-integration`.
- Exact source SHA tested this run: `3979faa642053d0f584d2f91982c1b88cbe0eb62`.
- Codespace worktree was clean before documentation updates; tests did not modify tracked files.
- Live `origin/develop/wasm` via `git ls-remote`: `2fc0a8e93be97c4d99123910fad4bd25798e71a8`. PR metadata currently reports base snapshot `5d83c4ef627baa6dc9009ef9a585c0b70e0d9435`; reconcile these refs again immediately before the permitted normal merge.
- PR #42 remains open and draft, targeting `develop/wasm`; not merged. Open related PRs inspected: #56 projectile/limb combat and #57 source IK callbacks. Keep these separate.
- No GitHub Actions run exists for tested SHA `3979faa...` (`github_fetch_commit_workflow_runs` returned an empty list). Therefore there is no workflow ID or Actions artifact for this checkpoint.

## Passed gates on exact source SHA

| Gate | Command / evidence | Result |
| --- | --- | --- |
| Source dependency inventory | `python3 scripts/inventory_source_dependencies.py --check` | PASS; inventory freshness check |
| Migration ownership boundary | `python3 scripts/check_actor_migration.py` | PASS; zero violations |
| CharacterBody ownership | `python3 scripts/check_characterbody_ownership.py --strict` | PASS; strict ownership scan |
| Original enum identity | `python3 scripts/verify_full_characterbody_enums.py` | PASS; exact recovered WeaponMode and LimbId definitions |
| Actor prefab asset provenance | `python3 scripts/recover_character_actor_prefabs.py --archive /tmp/crisis-recovery-release/crisis-response-recovery-direct-v3-1.zip --check` | PASS; 4 verbatim actor assets and metadata match pinned archive |
| Audit tools regressions | `python3 -m unittest discover -s scripts -p audit_tools_test.py -v` | PASS; 9 tests |
| Engine isolation contracts | `dotnet run --project runtime/EngineChecks/Crisis.Runtime.EngineChecks.csproj --no-restore -v:q` | PASS; 69 isolated assertions |
| Foundation contracts | `dotnet run --project runtime/FoundationChecks/Crisis.Runtime.FoundationChecks.csproj --no-restore -v:q` | PASS; 120 assertions |
| Runtime checks project build | `dotnet build runtime/Checks/Crisis.Runtime.Checks.csproj --no-incremental -v:q -p:GenerateDocumentationFile=false` | PASS; exit 0, 0 errors (44 warnings) |
| Actor dependency closure | `python3 scripts/verify_runtime_package.py /tmp/crisis-original-project-v3/export/ExportedProject /tmp/cpr-runtime-package/scene-package.json` | PASS; exit 0, Operator and Occupant file/meta closure has zero unresolved GUIDs |
| Whitespace | `git diff --check` | PASS |

Focused gate output is in the Codespaces session at `/tmp/cpr-oct20-texture/focused-gates.log`; .NET results at `/tmp/cpr-oct20-texture/dotnet-gates.log`; package closure at `/tmp/cpr-oct20-texture/package-closure.log`. Package source was extracted from the pinned recovery archive to `/tmp/crisis-original-project-v3/export/ExportedProject`; generated package is `/tmp/cpr-runtime-package/scene-package.json`.

## Failed / incomplete acceptance gates

- **Actual prefab lifecycle: FAIL CLOSED.** `dotnet runtime/Checks/bin/Debug/net8.0/Crisis.Runtime.Checks.dll --prefab-only /tmp/cpr-runtime-package/scene-package.json` fails while resolving the preserved Operator's Texture2D field for `Assets/Texture2D/head 5.png`. Its exact Unity importer metadata contains `textureFormat: 12`. Current CPU decoder only accepts RGBA32 PNG input and the serialized package does not have the original imported Texture2D object record for the referenced fileID. The resolver raises `NotSupportedException` rather than inventing a local ID or pretending the imported pixels/settings are known. Log: `/tmp/cpr-prefab-latest.log`.
- **Full runtime contracts: FAIL.** `dotnet runtime/Checks/bin/Debug/net8.0/Crisis.Runtime.Checks.dll` exits 134. The stack reaches `AmmunitionLoader.Load` → `WeaponHost.ctor` → `WeaponContracts.Run`; the observed exception is `System.NotSupportedException: Calibre` at `AmmunitionLoader.Load` line 46, followed by `WeaponHost.ctor` and `WeaponContracts.Run`. `Calibre` is confirmed in `runtime/Original/FullSource/Calibre.cs` as a `MonoBehaviour` class, not an enum; its source field is `public string calibreName = "Unnamed calibre";`. Compare the failing loader branch with original serialized ammunition/component data before implementing a mapping. Log: `/tmp/cpr-oct20-texture/runtime-contracts.log`.
- **Not run / not passed:** actual Operator/Occupant lifecycle, two-actor gameplay, WASM output build, Chromium gameplay, WebKit gameplay and local HTTP smoke test. No deployed preview was changed.

## Next ordered implementation/acceptance gates

1. Recover authoritative imported Texture2D content/format semantics for the referenced actor assets, preserving original GUID, serialized fileID, importer bytes/settings, pixel data and orientation. Keep unknown importer fields unsupported; do not substitute raw PNG pixels for compressed imported output without provenance.
2. Resolve the importer/object-to-serialized-fileID mapping from source-backed evidence and add tests for hashes, GUID, fileID, importer settings, dimensions, pixel values/orientation and all unsupported formats.
3. Run actual Operator prefab with complete-field/reference enforcement; verify serialized values, activation and original component ownership.
4. Run actual Occupant prefab under the same checks; prove independence of object, limb, equipment and physiology ownership.
5. Trace the `NotSupportedException: Calibre` branch in `AmmunitionLoader.Load` against original serialized ammunition and `Calibre` component references; implement source-backed coverage and rerun native contracts.
6. Complete source actor lifecycle and independent two-actor spawn/equip/movement/hit/physiology/destruction flow with original callback order.
7. Re-run focused identity/provenance/ownership/dependency gates, EngineChecks, FoundationChecks, full runtime contracts/build and package closure on a single clean recorded SHA.
8. Once implementation gates pass, re-check open PRs and live `develop/wasm`, then merge the base with a normal merge commit immediately before final verification.
9. Build WASM from the exact recorded post-merge SHA and preserve the command, workflow/run ID (if Actions runs), logs and artifact.
10. Verify gameplay in Chromium and WebKit against that exact build, run local HTTP smoke test, refresh this REA evidence/ `NEXT_AGENT.md` / PR #42 description, and keep the PR open/draft.

## Reproduction / logs

The run used the commands in the passed-gates table plus:
- `dotnet runtime/Checks/bin/Debug/net8.0/Crisis.Runtime.Checks.dll --prefab-only /tmp/cpr-runtime-package/scene-package.json`
- `dotnet runtime/Checks/bin/Debug/net8.0/Crisis.Runtime.Checks.dll`

Codespaces-session logs listed above are ephemeral; no workflow ID/artifact is claimed. The current test SHA is the code checkpoint SHA, not a claim that the blocked browser/gameplay acceptance passed.


## Follow-up branch synchronization and PR closeout (October 20, 2026)

- Codespace began clean at source-tested SHA `3979faa642053d0f584d2f91982c1b88cbe0eb62`. Fetched the feature ref and fast-forwarded only (no reset/rebase): `git merge --ff-only origin/feature/full-source-characterbody-integration` succeeded, applying the two documentation commits. New local HEAD is `5e592630131a9e81d316fb604ded2e402fc9c929`; worktree is clean and `git diff --check` passed. Runtime/source files are unchanged, so `3979faa...` remains the tested code SHA.
- Fresh `git ls-remote origin refs/heads/develop/wasm` confirmed `2fc0a8e93be97c4d99123910fad4bd25798e71a8`. PR metadata still reports base snapshot `5d83c4ef627baa6dc9009ef9a585c0b70e0d9435`; no base merge was attempted.
- Updated PR #42 description through GitHub issue metadata; result reports state open. The description now cites code SHA `3979faa...`, branch tip `5e59263...`, links the evidence/handoff, distinguishes pass/fail/not-run gates, and keeps the PR in draft (confirmed in PR metadata before update).
- Re-ran Actions lookup for source SHA `3979faa...`: empty workflow_runs list; no workflow ID/artifact.
- Codespace probe `command -v Unity || command -v unity` returned no executable. The extracted project Assets tree contains 315 `.asset` files, but the probe does not show an authoritative serialized/imported Texture2D object for the failing references. This is not evidence to synthesize imported texture data or IDs.
- No new runtime/build/browser checks were run after the documentation-only fast-forward; prior code check results above remain bound to `3979faa...`. Prefab and native contract failures remain unchanged; browser/WASM/HTTP gates remain unpassed.


## Follow-up source diagnostic checkpoint — 2026-10-20

- Source commit under test: `318fc92b87f6a2138e680e6f016ec636ceb8299a` (pushed to the PR branch). A following documentation-only commit updated `NEXT_AGENT.md`; this does not alter the tested runtime tree.
- `git diff --check` passed. `dotnet build runtime/Checks/Crisis.Runtime.Checks.csproj --no-incremental -v:q -p:GenerateDocumentationFile=false` exited successfully on net8.0 after the source diagnostic change, with zero reported errors.
- `dotnet runtime/Checks/bin/Debug/net8.0/Crisis.Runtime.Checks.dll` was rerun and exited 134. It remains a failing native contract gate. The unsupported-field diagnostic now names the original ammunition field and runtime type and preserves the serialized GUID:fileID in the error.
- Package trace: `game/assets/weapon-runtime.json.gz` has 1 asset / 1 guidPath, for 5.56-FMJ; it includes no Calibre path/asset. In the actual 5.56-FMJ prefab record, `calibre` references GUID `57cfc851e0e4734d98dbd083b39cddf`, fileID `114724804207875695`. Original source identity is `Calibre : MonoBehaviour`, with `public string calibreName = "Unnamed calibre";` in `runtime/Original/FullSource/Calibre.cs`. The missing serialized Calibre source object record is a concrete packaging/provenance gap. No behavior or object identity was synthesized.
- Diagnostic runtime log: `/tmp/cpr-oct20-texture/runtime-contracts-diagnostic.log` in Codespaces session; ephemeral. Build and source inventory check results were visible in terminal, but no durable logs or Actions artifact were produced.
- GitHub Actions lookup for `318fc92b87f6a2138e680e6f016ec636ceb8299a` returned an empty workflow_runs list; workflow ID: none; artifact: none. Planned EngineChecks/FoundationChecks rerun did not complete because the Codespace environment disconnected. The prior 69/120 assertion passes and other source/package checks remain evidence only for SHA `3979faa642053d0f584d2f91982c1b88cbe0eb62`.
- PR #42 remains open/draft. Live source branch head was `318fc92b87f6a2138e680e6f016ec636ceb8299a` before this REA-only update. No base merge, deployment, preview change or browser run occurred.
- Remaining unpassed acceptance: authoritative Calibre record/package closure; imported Texture2D object/fileID and format 12 semantics; actual Operator and Occupant prefab lifecycle; complete independent two-actor flow; final source verification; post-merge WASM build; Chromium, WebKit, local HTTP. Do not label these passed.

### Run commands

```sh
git status --short --branch
git rev-parse HEAD
git diff --check
dotnet build runtime/Checks/Crisis.Runtime.Checks.csproj --no-incremental -v:q -p:GenerateDocumentationFile=false
dotnet runtime/Checks/bin/Debug/net8.0/Crisis.Runtime.Checks.dll
```



## Calibre source-data gate — 2026-10-20

The next package edge is now identified precisely:

- Original source field type: `AmmunitionBase.calibre` is declared as `Calibre` in `runtime/Original/FullSource/AmmunitionBase.cs`. Original source defines `Calibre : MonoBehaviour` with `calibreName = "Unnamed calibre"`. This is a Unity object reference/component, not a primitive, enum, or synthetic string label.
- Exact serialized edge from the packaged 5.56-FMJ prefab: GUID `57cfc851e0e4734d98dbd083b39cddf`, fileID `114724804207875695`. The current `weapon-runtime.json.gz` has one asset and one guidPath (5.56-FMJ); neither the compressed package nor the committed `game/assets/weapon-runtime/` directory supplies the referenced Calibre object record.
- The repository's `docs/ARCHIVE_INVENTORY.md` confirms that `Crisis Response.rar` is a compiled Windows/Mono Unity build and contains no editable Unity `Assets/` project or original source files. The committed `source-reference/Original` tree has no Calibre serialized asset. This repository therefore does not provide source-authoritative component fields/object identity for that GUID:fileID.
- The earlier Codespace export location `/tmp/crisis-original-project-v3/export/ExportedProject` and source recovery archive were in the disconnected environment. A current Codespace tab is unavailable (`Tab not found in browser 1`), and this workspace contains no project checkout or archive. I have not tried to fabricate missing fields or IDs, nor altered the loader beyond its diagnostic.
- **Gate result: blocked at acquisition of the original editable asset/source record.** Available independent steps done: repository/PR/head inspection; exact type and field declaration inspection; source archive inventory inspection; package and directory path inspection. To proceed, restore access to the pinned recovery export/source archive containing the exact referenced Calibre object record, or recover another authoritative original Unity asset database/export. Then verify GUID, fileID, serialized field values, object type and all transitive refs before implementing.

The next code gate remains package closure for this Calibre edge. Do not treat the known default `calibreName` initializer as proof of the serialized value or use it to synthesize a missing prefab.


## Recovery artifact follow-up — Calibre reference verified, runtime gate still open (2026-10-20)

This section supersedes the earlier Calibre note that said the recovered export was inaccessible. The Codespaces browser session is still unavailable, but the pinned recovery artifact itself was accessible through the GitHub Actions connector.

### Recovery provenance

- GitHub Actions recovery run: [38582072502](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38582072502).
- Artifact ID: `11903811738`, downloaded as `crisis-response-recovery-original-source.zip`.
- Command after connector download: `curl --fail --location --silent --show-error <temporary artifact URL> --output /tmp/cpr-recovery-artifact.zip`.
- Checksum command: `sha256sum /tmp/cpr-recovery-artifact.zip`.
- Observed SHA-256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d` (matched the pinned hash in the recovery workflow).
- The artifact's `RECOVERY_SUMMARY.md` says the decompiled Assembly-CSharp matches the supplied archive: expected EXE MD5 `9e00a2b43b7140aaac57c828bd6566ce`; recovered DLL SHA-256 `482f8bd6ef4546965b060463377470fc2da2db996de7ec16f29eb88f56a021b3`. It contains a 7,231-file, 169 MB AssetRipper export. Treat serialized YAML and generated .meta files as recovered-export evidence; this is not proof that the compiled distribution itself included the original editable Unity project or original editor .meta files.

### Calibre identity and package edge

- `unzip -q /tmp/cpr-recovery-artifact.zip -d /tmp/cpr-recovery-authoritative` completed.
- The PR-head `game/assets/weapon-runtime.json.gz` was fetched with GitHub `fetch_file` using base64 encoding; decoded and decompressed for read-only inspection. Its compressed SHA-256 is `786e7ad6764058e59138d12e2631618da807596b510515317a1a544c35de2211`.
- The nested package has one asset (`Assets/Resources/weapons/ammunition/5.56-FMJ.prefab`) and one guidPath (AmmunitionBase script). The 5.56-FMJ component's `calibre` field references GUID `57cfc851e0e47374d98db0383b39cddf`, fileID `114724804207875695`.
- Export file `Assets/Resources/weapons/calibers/Caliber 5.56x45mm.prefab.meta` has that exact GUID. The prefab contains the component record with the exact fileID, script GUID `8e75328a0c323b110b577a2dbfddf164`, and serialized `calibreName: 5.56x45mm`. The package currently contains neither this prefab record nor a guidPath for its Calibre script.
- This validates a Calibre edge inside the recovered export and matches the serialized GUID/fileID in the runtime package. The earlier handoff miscopied the GUID; the exact value is the one recorded above.
- Four more direct non-null refs on the same ammo component are missing from the small package: Projectile `cf5bca2bd1c0369439bb8fbe00c9320d:114703437598541787`; Casing `9413de22ed594c44e8586fede9f5a355:114243736805655561`; high-poly UI model `1b3dc24381623284e82335a132ad5dad:114243736805655561`; Sprite `8464cbc48daa246479808d64b86fb8c2:21300000`. Recovered paths are `Assets/Resources/weapons/ammunition/projectiles/Projectile 5.56mm FMJ.prefab`, `Assets/Resources/weapons/ammunition/casings/Casing 5.56x45mm.prefab`, `Assets/Resources/weapons/ammunition/ui models/5.56x45mm FMJ.prefab`, and `Assets/Sprite/Icon 556 FMJ.asset`. Their transitive edges must also be followed.
- Source inspection: `runtime/Engine/AmmunitionLoader.cs` currently throws on non-primitive reference fields. `runtime/Engine/SerializedAssetResolver.cs` can load a packaged prefab component when its GUID path and records are present. The Sprite type/class mapping remains unsupported/unknown until a source-faithful record/importer object mapping is verified.

### Gate result and execution boundary

- **Passed:** recovered-artifact download/checksum; source archive report inspection; reference-to-prefab GUID/fileID match; exact Calibre script identity and serialized name match; confirmation that the current runtime package omits Calibre and the other four direct refs.
- **Not passed:** no runtime loader change, no package asset change, no Calibre native lifecycle test, no full transitive closure. No code was edited in this execution.
- Runtime source checkpoint remains `318fc92b87f6a2138e680e6f016ec636ceb8299a`; the prior check at that SHA built the checks project and passed `git diff --check`, but the full runtime contracts still exited 134 on unresolved Calibre. This run did not rerun those checks. GitHub Actions lookup for that source SHA returned an empty `workflow_runs` list; no workflow ID/artifact exists for the runtime test.
- The execution workspace `/workspace/scratch/483f9f6eb178` contains no Git repository. Browser state reports Chrome with no tabs; the previous Codespace tab cannot be reacquired here. A direct `git ls-remote https://github.com/ethan-bailey/crisis-response-port.git ...` failed because the private GitHub remote required credentials that are not present in this environment. The connected GitHub API permits inspection and file edits but does not expose a repository checkout or the base tree needed for a safe binary package commit. The correct next action is resume in an authenticated Codespaces/repo checkout; do not substitute a textual/base64 package for the existing gzip or make untested source edits.
- PR #42 remains open, draft, unmerged. Its latest source-code tests are still bound to `318fc92...`; docs-only updates do not change that test status. The next implementation gate is to add the recovered Calibre prefab record and script GUID path to the existing package, resolve Unity-object fields source-faithfully, add a focused exact-identity test, and run it. Then continue all other direct and transitive refs; do not claim the package closure is complete after Calibre alone.

### Commands used for this follow-up

```sh
sha256sum /tmp/cpr-recovery-artifact.zip
unzip -l /tmp/cpr-recovery-artifact.zip
unzip -q /tmp/cpr-recovery-artifact.zip -d /tmp/cpr-recovery-authoritative
gzip -dc /tmp/cpr-weapon-runtime.json.gz > /tmp/cpr-weapon-runtime.json
python3 - <<'PY'
# Read the nested ammoPackage; map GUIDs using exported Assets/**/*.meta;
# verify the Calibre GUID resolves to a prefab with fileID 114724804207875695;
# inspect the serialized MonoBehaviour record and enumerate other non-null refs.
PY
rg -n -C 6 'calibreName|m_Name: Caliber' '/tmp/cpr-recovery-authoritative/export/ExportedProject/Assets/Resources/weapons/calibers/Caliber 5.56x45mm.prefab'
```

Temporary logs and extracted files are local-only and not Actions artifacts. No runtime test workflow ID exists for this follow-up.

### New gate order

1. Restore authenticated checkout/Codespaces and inspect live worktree/ref state.
2. Package Calibre prefab and exact script GUID path; add field-type-safe resolver behavior.
3. Run a focused exact GUID/fileID/type/value/lifecycle test and full runtime contract to identify next missing ref.
4. Package Projectile, Casing, UI model, Sprite, and the complete recursive dependency closure; implement supported source-derived deserialization and negative tests.
5. Resolve the imported Sprite/Texture2D identity and format without synthetic mappings.
6. Pass actual Operator and Occupant prefab lifecycle and independent ownership checks.
7. Pass independent two-actor gameplay lifecycle.
8. Re-run all focused source/provenance/identity/ownership/dependency and native builds/tests on one exact SHA.
9. Only at final verification, reconcile develop/wasm and integrate it with a normal merge commit.
10. Build and test the exact merged SHA in Chromium and WebKit and via local HTTP; record logs, workflow IDs and artifacts; refresh closeout while keeping PR #42 open/draft.


## Actor prefab byte identity and recursive reference scan — 2026-10-20

### Preserved actor assets and GUID metadata

The committed `source-reference/Original/CharacterDependencies/ActorPrefabs` files were fetched from the current PR branch and compared with the checksum-verified recovered AssetRipper export.

| File | Preserved source SHA-256 | Recovered export SHA-256 | Result |
| --- | --- | --- | --- |
| `Operator.prefab` | `94bcda5de74f45d83f4bbf88e80874b8f77bf0339d307294187b085c03170536` | same | `cmp -s` exit 0 |
| `Occupant.prefab` | `9094b840c49d6c53ffa077c274561ab5cbeedb202f85dbaade023ad67cb8c419` | same | `cmp -s` exit 0 |
| `Operator.prefab.meta` | `8d01cf639201add925686afbe85b7b1fd8a7bd8ec6c41b6749d76b568ef63b0d` | same | SHA match |
| `Occupant.prefab.meta` | `3e39f95ea92613c4c4eb1c2c2c82775b9ed075cdf7a925d980ef5b3eca9c5737` | same | SHA match |

Preserved/export GUIDs: Operator `2e00922aa35d9a8438c52fec69254605`; Occupant `8363ad318a650364ebdaa8c237f65bfe`. These are byte-identity/provenance checks against the pinned recovered export; no runtime lifecycle was executed by this comparison.

Commands executed:

```sh
sha256sum /tmp/repo-Operator-fromgh.prefab /tmp/repo-Occupant-fromgh.prefab \
  '/tmp/cpr-recovery-authoritative/export/ExportedProject/Assets/Resources/characters/Operator.prefab' \
  '/tmp/cpr-recovery-authoritative/export/ExportedProject/Assets/Resources/characters/Occupant.prefab'
cmp -s /tmp/repo-Operator-fromgh.prefab \
  '/tmp/cpr-recovery-authoritative/export/ExportedProject/Assets/Resources/characters/Operator.prefab'
cmp -s /tmp/repo-Occupant-fromgh.prefab \
  '/tmp/cpr-recovery-authoritative/export/ExportedProject/Assets/Resources/characters/Occupant.prefab'
sha256sum /tmp/repo-Operator.prefab.meta-fromgh /tmp/repo-Occupant.prefab.meta-fromgh \
  '/tmp/cpr-recovery-authoritative/export/ExportedProject/Assets/Resources/characters/Operator.prefab.meta' \
  '/tmp/cpr-recovery-authoritative/export/ExportedProject/Assets/Resources/characters/Occupant.prefab.meta'
```

### Recursive reference scan (partial, not closure acceptance)

A read-only Python walk started at the recovered Operator and Occupant prefabs, indexed `Assets/**/*.meta` GUIDs, recursively followed serialized `{fileID, guid}` PPtrs, and checked IDs against YAML target record headers where available. Observed:

- 252 reachable non-script/project assets and 37 referenced C# scripts.
- 1,200 non-null serialized PPtr references.
- Zero unmapped PPtr GUIDs (the known external uGUI script IDs are accounted for by the export extraction script's explicit TypeDef map).
- Zero missing fileIDs for text Unity YAML targets.
- 452 references target audio/image binaries with no serialized Unity object record in the export; these remain unverified, including Texture2D fileID `2800000` for `head 5.png`.

The simple GUID-only scan also surfaced five GUID strings inside `Master mixer.mixer`; inspection shows these are `m_ExposedParameters` / mixer parameter identifiers rather than PPtr tuples. They are not counted as external asset references. The recursive traversal confirms file/path presence for the text-asset portion; it **does not** pass transitive asset closure because binary imported object identities/formats are unresolved and the runtime package does not include this graph.

For `head 5.png`, the recovered export provides PNG bytes (SHA-256 `f2b00675b758baa62300b9e3c9e941c891211da335aef1598d76de2c0b7f0c6b`, 128×128 RGBA) and matching meta GUID `d701c84638d2d934080508b9fb88f6ce`. Both actor prefabs refer to that GUID with fileID `2800000`; its TextureImporter has `fileIDToRecycleName: {}`, `textureFormat: 12`, `isReadable: 1`, and `textureType: 0`. These facts tighten the importer evidence but do not prove format-12 runtime pixel semantics or an authoritative imported-object mapping. Keep the existing fail-closed texture resolver behavior until those semantics are tested.

No code changes or runtime checks occurred in this follow-up. The local extracted files and comparison outputs are temporary; only this evidence record and linked Actions artifact/run are durable.
