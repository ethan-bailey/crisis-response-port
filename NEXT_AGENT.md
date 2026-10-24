# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; verified remote and local source checkpoint: `638cf4a5a470a2f514446c055b73a1a3b57cf048`.
- No deployment occurred. Site v39 remains the owner-accepted iPhone Safari build; this pass changed only isolated check boundaries and the generated source inventory, not gameplay code.
- The supported gameplay target is iPhone Safari. Desktop Chromium/WebKit automation is optional diagnostic coverage. Ask for an exact-build device check at the next major playable milestone or before a release; do not claim acceptance for features absent from the tested build.
- The previously reported play-area hold behavior can stop sustained fire during aim changes. Keep that correction documented and deferred; do not conflate it with holding the Fire button.

## Latest verified work

- Recovered pinned .NET SDK 8.0.426 from the checksum-verified archive and compiled the tracked `/proc` namespace shim. `dotnet --version` reported 8.0.426.
- Fixed two isolated-check project boundaries: EngineChecks now declares only the compile-time audio surface it needs; FoundationChecks has compile-only FullSource ammunition/calibre/projectile stubs. These do not add gameplay behavior.
- Regenerated `docs/audit/source-dependencies.json` after its freshness check found tracked-source additions; `python3 scripts/inventory_source_dependencies.py --check` now passes (806 sources, 1,704 symbols, 1,383 reachable files, 127 ambiguous names requiring ownership review).
- Passed on source SHA `638cf4a5a470a2f514446c055b73a1a3b57cf048`: Node suite 78/78; package-builder unit suite 13/13; actor migration audit with 0 violations; EngineChecks 69 assertions; FoundationChecks 121 assertions; runtime source contracts for primitives, prefab lifecycle, seven original arm sequence prefabs/21 frames, presentation, weapons, and ballistics.
- .NET runtime build and Release browser WASM publish passed (48 existing compiler warnings, 0 errors). Exact-source package build and separate verify passed: 767 files, 199 hashed Blazor resources, 195 validated WASM files. Package output is ephemeral at `/tmp/cpr-site-638cf4a` and was not deployed.
- In the clean verification worktree, ignored PhysX vendor files were restored with `npm ci --prefix game --ignore-scripts` and `npm run --prefix game vendor`; this is already documented in `docs/DOTNET_BUILD_RUNBOOK.md`. Do not expect ignored vendor files to appear in a fresh worktree.
- The original gameplay recovery scene package (`cpr-scenes-full-source.json` / `crisis-response-scenes-with-gameplay-resources.json`) was absent from the checkout and `/tmp` search at the start of this pass. Do not repeat that search without new evidence; package-dependent mission/AI contracts need the approved artifact.

## Gate state

- Gate 0: open. The regenerated lexical inventory is current, but dependency ownership and full CharacterBody closure remain incomplete; missing authored scene package prevents package-driven closure.
- Gate 1: passed within the scoped headless original-character acceptance; this is not full game parity.
- Gate 2: partial. Original source IK controller/sequencer callbacks and actor isolation pass headless contracts. Browser sequence composition playback remains explicitly unimplemented; physical/visual weapon continuity remains unverified.
- Gate 3: partial. Source projectile-to-physiology evidence exists; native PhysX, rendered wounds, and full live state/event integration remain.
- Gate 4: partial. Original mission startup/spawning/outcome evidence exists in prior exact-source reports; normal-page authored mission inputs and callbacks remain incomplete.
- Gate 5: open. Original behavior trees load and tick, but autonomous perception, navigation, decisions, and combat are not verified.
- Gate 6: partial. Source room-clear/success contracts pass headlessly; complete normal-path player/AI mission flow remains.
- Gate 7: open. Original presentation and content fidelity remain.
- Gate 8: closed for the previously owner-accepted Site v39 under the agreed manual iPhone smoke path. This pass additionally verified build and package integrity at SHA `638cf4a5a470a2f514446c055b73a1a3b57cf048`; no deploy or new visible gameplay behavior occurred.

## Exact next action

Continue in dependency order. Start Gate 2 by tracing the remaining browser sequence-composition playback gap from `runtime/Checks/ArmSequenceContracts.cs` into the real game path, and implement only if it can use original sequence/callback behavior without invented defaults. If this depends on the missing authored package, record the specific dependency once and move to the next unblocked Gate 0 or Gate 3 task. Keep using the recovered .NET SDK archive plus tracked process shim, restore ignored PhysX vendor files in any fresh worktree, use the connected GitHub API for commits/pushes, and do not deploy.
