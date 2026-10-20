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

