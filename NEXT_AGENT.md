# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; latest verified gameplay/build source checkpoint: `74128bc2bdf381539dc14b547c661574930c77a0`. The final handoff/index commit is documentation and repository-index metadata only.
- No deployment occurred. Site v39 remains the owner-accepted iPhone Safari build. The current pass changed audit tooling and documentation; it did not change gameplay behavior.
- Supported gameplay target: iPhone Safari. Desktop Chromium/WebKit automation is optional diagnostics. Obtain owner testing for the exact build at the next major playable milestone and before release; do not claim acceptance for features absent from the tested build.
- Keep the owner-reported play-area hold behavior distinct from holding the Fire button; it remains documented for later.

## Latest verified work

- Fixed `scripts/audit_character_animation_source_closure.py`, which still searched the obsolete `runtime/Original/Animation` path and asserted the current FullSource controller/sequencer were absent. It now checks the preserved source files against `runtime/Original/FullSource/{IKArmController,IKArmSequencer}.cs` and `runtime/Original/CharacterBody.Full.cs`, reports required method coverage and hashes, and does not claim gameplay or visual parity.
- Added a regression test for the audit path mapping. `python3 -m unittest discover -s scripts -p 'audit_tools_test.py'` passes 14/14; the audit reports no missing required methods for the controller, sequencer, or CharacterBody.
- Refreshed the current sequencer status and marked earlier 2026-10-16 dependency notes as historical/superseded. Headless tests cover selected `LowerWeapon` and `FireWeapon` callbacks and actor isolation. Browser `ComposeOriginalArmSequences` remains composition-only, explicitly reports `playbackImplemented: false`, and must not be represented as normal gameplay playback.
- Verified `python3 scripts/inventory_source_dependencies.py --check`: 806 sources, 1,704 symbols, 1,383 reachable files, 127 ambiguous names requiring ownership review.
- .NET 8.0.426 runtime build and Release WASM publish pass at source SHA `74128bc2bdf381539dc14b547c661574930c77a0` (48 existing warnings, zero errors). Exact-source package build and verify pass: 767 files, 199 hashed Blazor resources, 195 valid WASM files. Output is `/tmp/cpr-site-74128bc`; it was not deployed.
- `scripts/index_repository_files.py` regenerated and checked against that package manifest: 2,183 repository files indexed. Ignored PhysX files must be restored in a fresh worktree with `npm ci --prefix game --ignore-scripts` then `npm run --prefix game vendor`; this is already documented.
- Earlier verified runtime/source contracts on unchanged gameplay source `638cf4a5a470a2f514446c055b73a1a3b57cf048`: Node 78/78, package-builder tests 13/13, EngineChecks 69 assertions, FoundationChecks 121 assertions, and runtime contracts for prefab lifecycle, original arm sequence data, presentation, weapons, and ballistics.

## Gate state

- Gate 0: open. Dependency inventory is current and the bounded M4 source-fire dependencies are classified; normal-page original actor flow, native physics, authored visible actors, and broader source navigation/presentation remain.
- Gate 1: passed within scoped headless original-character acceptance; this is not full game parity.
- Gate 2: partial. FullSource controller/sequencer compile and selected callback/actor-isolation contracts pass. The normal page does not play composed sequences on the real authored actor/rig; source mission/prefab startup and visual/physical continuity remain.
- Gate 3: partial. Source projectile-to-physiology evidence exists; native PhysX, rendered wounds, and complete live state/event integration remain.
- Gate 4: partial. Headless source mission startup/spawning/outcome evidence exists; normal-page authored mission inputs and instantiation callbacks remain incomplete.
- Gate 5: open. Original trees load and tick; autonomous perception, navigation, decisions, and combat remain unverified.
- Gate 6: partial. Source room-clear/success contracts pass headlessly; complete normal-path player/AI mission flow remains.
- Gate 7: open. Original presentation and content fidelity remain.
- Gate 8: closed for the previously owner-accepted Site v39. Build and exact-source package integrity are additionally verified at `74128bc2bdf381539dc14b547c661574930c77a0`; no user-visible gameplay changed, no new device check is implied, and no deploy occurred.

## Exact next action

Continue Gate 2/Gate 4 at the real normal-page actor flow: trace the original scene package, authored prefab, and initialization callbacks needed to instantiate `FullSource.CharacterBody` with its original `IKArmController`, solvers, weapon targets, and equipment. Reuse an existing verified full scene package if present; do not repeatedly search/extract the recovery archive, and do not invent defaults. If required authored material is unavailable, record that specific blocker once and continue with the next unblocked Gate 3 or Gate 7 integration task. Use the checksum-verified .NET SDK 8.0.426 plus tracked process shim, restore ignored PhysX files in fresh worktrees, commit/push through the GitHub API, and never deploy.
