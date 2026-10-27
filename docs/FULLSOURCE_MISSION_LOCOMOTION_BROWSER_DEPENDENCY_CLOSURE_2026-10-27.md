# FullSource Mission locomotion browser acceptance closure — 2026-10-27

## Acceptance target

On the integrity-verified 1,919-asset FullSource Mission package, verify that browser keyboard input reaches the original player controller after prior touch aim and sprint interactions. Capture a fresh source actor position after sprint release; send the original mapped left input through the open spawn-side corridor; verify the actual input edge, direction-correct source position delta, authored forward/backward locomotion clip selection, input release, settled stand state, finite position, and no browser exceptions.

The assertion must test movement caused by the new keyboard input. A pre-interaction position snapshot cannot serve as the post-sprint baseline. Do not assign transforms/velocity, invoke source controller methods directly, or bypass scene contact synchronization.

## Dependency closure

| Layer | Source owner and dependency | Acceptance evidence |
|---|---|---|
| Mission package | BrowserEntryTeamMission.Start with the original Operator.prefab, Mission 01 scene, and verified FullSource asset manifest; package includes 1,919 assets and serialized collider hierarchy. | Browser loads the exact workflow SHA and reports verified FullSource source delivery before controls are exercised. |
| Browser key path | game/index.html binds KeyA to left; game/browser-input.mjs normalizes keyboard events and releases them; runtime/Engine/FullSourceActorBridge.cs forwards action state to the original runtime input mapping. | Snapshot reports originalInput.left.Pressed while key is down and false after keyup. |
| Original player lifecycle | Recovered PlayerController.Update reads InputHandler.Keys.left.Pressed, sets direction −1 and calls owner CharacterBody.Move(direction). It continues handling source aim and facing in the same owner update. | Position must move left relative to a fresh, immediately pre-input position. |
| Source movement and collision | runtime/Original/CharacterBody.Full.cs: Move sets acceleration for nonzero input; FixedUpdate applies authored walk speed/force; FullSource source ticking and browser contact projection preserve scene collision. The recovered Mission contains the counter obstacle at the rightward end of the brief joystick sprint trajectory. | Use the clear spawn-side/left corridor after sprint, not the already-blocked rightward counter approach; assert a new negative position delta. Do not edit position or skip the contact step. |
| Authored animation | game/source-character-step.mjs derives moving/backwards from source velocity and facing; game/source-character-animator.mjs evaluates the recovered AnimatorController transitions and blend tree in game/assets/operator/character-animator-controller.json. Its clips include Body walk, Body walk backward, and Body sprint. | Accept an authored walk, backward-walk, or sprint blend clip while source motion is active; after release require velocity <0.05 and Body stand. |
| Error/recovery | FullSource task continues running while keyboard input is sent; browser pageerror capture remains active. | Require no page errors and a finite settled position. |

## Exact failure and repair

- **Failed**, run #146 40501400977, source SHA df4b8b882e0b7f14b6466590edc0d87142095813: all contracts, pinned .NET SDK 8.0.426 Release/WASM build, source recovery/decompiler provenance, complete Mission package build/integrity, and profile IndexedDB acceptance passed. Independent Chromium IndexedDB/audio job 122179830600 passed. Only the packaged Mission browser job failed at scripts/mission_grounding_browser_smoke.mjs:230 waiting for Body walk / Body sprint.
- Retained browser report mission-grounding.json SHA-256 d82c0e79f0d3b67b14db581f65983293626e5e934a822513c87dda4680f49d23. The browser screenshot has a separate file hash; do not reuse the JSON hash for the image. The failed-run state is grounded at source X -0.6600008, Body stand, with left/right inputs released and no page errors.
- The smoke read state once after Mission startup, before it dragged the joystick through the sprint rim. After that sprint/release it reused stale state.playerX as walkStart, pressed KeyD, and could satisfy x > start + 0.1 using prior joystick displacement. The actor had already moved from spawn X -3 to -0.66, at the right-side counter approach; the subsequent rightward walk-animation wait could remain stationary. Thus the old test did not prove the keyboard caused movement and selected a blocked scene direction.
- The source-only repair captures a fresh post-sprint baseline, walks left through the clear corridor, waits for negative movement under originalInput.left.Pressed, and accepts the recovered backward-walk clip as well as forward walk/sprint blends. It changes no gameplay code or serialized content. The exact-source rerun is pending; the repair is not counted as package/browser pass until CI completes.

## Run #147 failure diagnosis — source InputHandler snapshot not connected (2026-10-27)

- **Failed**, exact Actions run #147 `40503778240` on code SHA `37f48498d1eb6f0aa7d51b6f47902792e91f4aa4`. Main-job steps 1–28 passed, including Node/Python/runtime contracts, pinned .NET SDK 8.0.426 Release/WASM build, recovered-source and decompiler lineage checks, full Mission profile package verification/retention, and production loader IndexedDB cold/warm acceptance. The independent Chromium IndexedDB/audio job `122187786238` passed. Only packaged FullSource browser step 29 failed; retained browser artifact `12734696606` contains report `mission-grounding.json`, status failed, `pageErrors=[]`, while Mission package diagnostics show all 1,919/1,919 assets and all expected source/compressed bytes verified.
- Failure: `TimeoutError: page.waitForFunction: Timeout 30000ms exceeded` at `scripts/mission_grounding_browser_smoke.mjs:234:14`. The inline predicate waits for `window.__gameState().originalInput.left.Pressed` and new leftward position displacement at the same time. That browser `originalInputState` variable is only refreshed by the legacy `RuntimeBridge.Tick` branch. The FullSource branch instead advances `FullSourceActorBridge.Step` through `TickFullSourceActorBridge`, but did not project that bridge's original `InputHandler.Keys` into `__gameState()`. The returned value was consequently a stale/legacy input snapshot and could not be used to accept or reject actual FullSource keyboard state. No actor-movement acceptance passes from this failed run; this diagnoses the test observability gap rather than declaring movement behavior passed.
- Before the repair, the new focused regression `scripts/fullsource_input_snapshot_test.mjs` fails because `FullSourceActorBridge.Snapshot()` has no serialized original input mapping. The focused repair serializes the actual `InputKey` fields from the bridge-owned `InputHandler.Keys` after the original `RuntimeWorld.Tick`, then refreshes browser `originalInputState` from that same source actor snapshot. It preserves the original PlayerController/InputHandler lifecycle and lets the existing smoke verify source `Pressed` with direction-correct position change; it does not synthesize input, task state, or actor movement. Regression run on pre-fix source: **failed as expected**; repaired bridge/page regression: **passed**; packaged browser acceptance remains pending the next exact SHA.
- Local focused verification for the repair: `node --check scripts/mission_grounding_browser_smoke.mjs` passed; `node --test scripts/fullsource_input_snapshot_test.mjs` passed 1/1. No local repository checkout or pinned .NET SDK is available in this execution workspace, so C# compilation and complete package/browser gates must be taken from Actions on the exact pushed repair SHA.

## Next action

Run and inspect the exact SHA containing the input-snapshot repair. Require the existing source-input-plus-leftward-displacement assertion and authored locomotion acceptance to pass before recording packaged locomotion parity. If another acceptance fails, extract that exact step and retained artifact and add the newly discovered source or observability dependency here before making its focused repair. Continue to reload/IK source-arm-pose acceptance only after locomotion has passed.

## Source force-vector observability follow-up — pending exact-SHA validation

The #152 retained browser trace established input, controller eligibility,
acceleration sign/magnitude, fixed-step count and that one original force
command was emitted, but the failure artifact did not retain the command's
actual vector. Since the FullSource bridge already returns each force as
`physics.commands[{force, position, mode}]`, the focused follow-up exposes those
source-produced command records in `window.__gameState()`, the existing bounded
96-frame animation trace, and the movement/animation timeout samples. It does
not synthesize force, alter its application, or change collision solving.

The new `scripts/fullsource_movement_force_trace_test.mjs` verifies the exact
source `FixedUpdate` vector flows from `FullSourceActorBridge` through the
production page and into both browser failure paths. On base source tree
`78fdc31d5bb17954d3f193e2e9ff6d5f86d13cff` plus this local batch, focused
animation/movement checks passed **9/9**, full Node suite **183/183**, Python
unittest suite **67/67**, smoke syntax and `git diff --check` passed. These
results are local only. The batch must be pushed against current remote tip
`a686cf4f5c2d38cabb9c42a0e74f6b51af66429c`, then verified by its exact hosted
package/browser run. Actions #153 on source SHA `37bb00d` is independently
in progress; its current browser result must be inspected separately before
any behavioral movement repair.

## Follow-up observability batch — 2026-10-27

- **Failed evidence inspected:** run #148 browser report and retained failure artifact `12735084098` (SHA-256 `a0de5140820a25f2a4c65d16b41c389400ad5a290e045ea59022710061f3a434`). `InputKey.Left.Pressed` was true, but `playerX=-1.8160572`, `Body stand`, and the >0.1 displacement predicate never became true. That report did not include the original controller's early-return predicates, acceleration, fixed-step count, or actor force emissions, so it cannot distinguish a rejected source update from a movement/physics response. Do not infer that the remaining cause is the tolerance.
- **Source inventory before instrumentation:** `PlayerController.Update` owns mission-view/ragdoll eligibility, direction and `CharacterBody.Move`; `CharacterBody.Move` sets private acceleration and acceleration magnitude; `FixedUpdate` consumes the authored movement values and the bridge captures actor force commands. `FullSourceActorBridge.Step` sets mapped keys then calls normal `RuntimeWorld.Tick`; it already restores authored walk/sprint speeds in `finally`. `Snapshot` runs after the tick. The new diagnostics expose those state boundaries without writing source state or changing actor update order.
- **Changes:** `Snapshot()` now reports mission view/pause, player-controller active state, ragdoll/sprint state, acceleration and acceleration magnitude, source velocity, authored walk/sprint speeds, fixed steps, and emitted force count. `game/index.html` projects this source snapshot as `sourceMovement`. The browser smoke samples source input, source controller state, projected velocity/position and captured physics every 250 ms, retaining first and last samples in the assertion failure. This is diagnostic instrumentation only; locomotion remains **failed/unresolved** pending an exact packaged rerun.
- **Regression:** `scripts/fullsource_movement_diagnostics_test.mjs` checks that each source-owned field is serialized and exposed to the browser smoke; `scripts/fullsource_input_snapshot_test.mjs` checks the diagnostic trace. Focused tests passed 2/2; the full Node suite passed 172/172; Python tests passed 67/67; pinned SDK 8.0.426 Release runtime, Checks, EngineChecks, FoundationChecks, Browser and Blazor/WASM publish passed. These are local pre-push results, not CI or packaged Mission browser acceptance.
- **Exact local package evidence on source SHA `b3281ff5c1904e00a9eeb1980e58192112246154`:** `package_wasm_site.py build` and `verify` passed for 2,690 files (199 Blazor resources, 195 WASM files, 1,919 FullSource assets; 97,767,549 compressed / 205,514,345 source bytes). All eleven Checks executable Mission contracts passed against that package profile: browser-entryteam-session, mission-hurt-audio, mission-ui-incremental, mission-projectile, mission-physiology, mission-outcome, mission-ai-outcome, mission-weapon-switch, mission-player-reload, behavior-graph, and mission-ai-switch-fallback. Package used the integrity-verified run #148 profile with source-archive and source-index digests `cc94b34d…3a1d` and `9d866276…0f92`. A documentation-only follow-up means the package will be rebuilt and re-verified against the final commit SHA before push.
- **Next evidence dependency:** run the new smoke on an exact pushed SHA, inspect its per-frame diagnostics, and repair only the source cause shown by the trace. Browser acceptance cannot currently be replayed locally: the bounded Chromium install attempt failed with a truncated archive, and the execution image has no system Chromium. Do not repeat that install without new evidence; the hosted browser workflow is the ready path.

## Run #149 result and animation trace closure — 2026-10-27

- Actions #149 `40515805814`, exact SHA `4795aaa5b767efb64b26732599360ff999fe3ffe`, completed failed at the authored-animation wait after the test had already passed its source-left displacement check. The exact 30-second wait is for `Body walk`, `Body walk backward`, or `Body sprint`. The failure snapshot shows Left still pressed, acceleration `-1`, magnitude `1`, 16 browser fixed steps, one original force command, source speed `2.929737e-7`, final `Body stand`, and player X `-2.142769`. Thus input propagation and the test's displacement predicate passed; the visible source animation stayed idle. The deliberate HTTP 503 belongs to the first-attempt retry contract and is not the root failure. Artifact: `12738799493`, SHA-256 `b4f48fbf31507c3f5b2f17f47209f46e46026a7e2d4ac45bdd74198611140941`.
- Before changing movement formulas, the next source batch adds read-only `animationDiagnostics` and a bounded 96-frame `sourceAnimationTrace`. Each frame records the same input object passed to the recovered `SourceCharacterAnimator`, computed source movement predicate and move speed, state/transition/clip weights, source/contact positions and velocities, and the bridge movement-gate snapshot. The browser smoke attaches this trace if the authored animation wait times out. No source field, velocity, transition, threshold, serialized asset, or lifecycle is changed.
- Local animation-diagnostics batch source SHA `ee050e1d6fe23ddea720cae4ebe1fa7ae05e56b5`, tree `99b19160121c32eacdf815ae41bfb0ba5241fa4e`: package build and integrity verification passed for 2,691 files, 199 Blazor resources, 195 WASM files and 1,919 FullSource assets (97,767,549 compressed / 205,514,345 source bytes). `scripts/fullsource_animation_diagnostics_test.mjs` plus focused related tests passed 13/13; all Node tests 176/176; Python unittest suite 67/67; browser smoke syntax and `git diff --check` passed. This runner has no `dotnet` executable; it reused the prior pinned SDK 8.0.426 Blazor/WASM publish output because no C# source changed. The hosted workflow must perform the exact-SHA Release/WASM build.
- The diagnostic batch is not pushed yet. The exact remote ref is still `6041ffd28f698f03a3abb3d090d28bfd48794da4`; push with expected-HEAD update and inspect the new hosted run before any behavioral repair.
- **Next action:** inspect the #150 reload-pose browser result independently, then inspect the new animation trace from the next exact-source hosted run. Check movement predicate threshold inputs, source/contact velocity agreement, transition state, and clip weights; consult recovered `CharacterBody.UpdateAnimation` and serialized AnimatorController before any fix. Make a behavior change only when one concrete source cause is shown, then add a regression that fails on this trace before the fix.

## Run #152 source-parameter and acceptance correction (2026-10-27)

- **Failed evidence inspected:** Actions #152 `40526443453`, source SHA `6564fd6e66dd52140c0633460e64a7ddc96b55b2`, packaged browser step 29 only. Artifact `12744941866` contained the complete report. At the displacement sample Left was true and X had changed `-0.6600008` → `-0.7856593`, but source/contact velocity was `-3.9278e-7`, clip `Body stand`, and adapter `Moving=false`. Later samples show repeated ~0.025 position changes with velocity near zero, confirming cumulative position drift alone was not valid locomotion evidence. The actual contact/collision cause remains unproven and is not changed in this batch.
- **Complete in-scope source ordering inspected:** `CharacterBody.Update()` calls `UpdateAnimation()`, and `LateUpdate()` then calls `UpdateMovement()`. `IsMoving` is based on `MovementDelta` compared with `Time.deltaTime * 0.1`; `IsMovingBackwards` compares that delta sign with `Direction`. `UpdateAnimation()` stores `Moving`, `Backwards`, `MoveSpeed`, `KneelRatio`, and `CrippledRatio` on the original root `Animator`. In `game/index.html`, original world ticking and snapshot precede browser contact integration; browser physics position/velocity is synchronized afterward and the recovered clip graph is sampled. Therefore using later contact velocity to reconstruct earlier source Animator inputs can shift the state boundary.
- **Repair pending hosted verification:** `FullSourceActorBridge.Snapshot()` exports the five stored source Animator values and `MovementDelta`/source moving diagnostics. The FullSource animation path consumes those stored values; the legacy presentation path retains the existing velocity adapter. The browser smoke now waits for an authored walk/backward-walk/sprint clip while source `Moving === true`, preventing contact correction or cumulative position drift from satisfying the animation gate. A C# runtime regression synchronizes a source displacement, advances two original world ticks to respect `Update`/`LateUpdate` order, then compares the transported fields with the actual original Animator values.
- **Regression and local evidence:** new `scripts/fullsource_animation_source_parameters_test.mjs` failed before the adapter change (missing export). After implementation, focused tests passed; complete workflow Node contracts passed **182/182**, Python tests **67/67**, smoke syntax and whitespace checks passed. Runner has no `dotnet`, so the new C# runtime contract, SDK 8.0.426 build, exact package integrity, and packaged browser acceptance are untested until Actions runs on the pushed tree.
- **Next exact acceptance:** push the reviewed batch only if the live branch still equals parent `6564fd6e66dd52140c0633460e64a7ddc96b55b2`, using expected-HEAD CAS. Inspect the new C# runtime contract, then packaged-browser trace. If actual source Animator `Moving` remains false during keyboard input, continue tracing source MovementDelta and contact geometry; do not lower the animation predicate or invent movement.

## Run #153 exact-source result pending — 2026-10-27

- The run #152 source Animator ordering correction is committed/pushed as `37bb00dd21aba92f60ac51d4487140172e1b9bbc`, tree `036c3b9b7efc905d88919743fb926246e31520ba`.
- Actions #153 `40532260745` is **in progress** on that exact SHA. Main job `122287962398` has passed preflight through source-index generation currently in progress; independent Chromium cache/audio job `122287962868` passed. Package build/integrity and packaged Mission browser are not yet reported.
- Local exact-tree evidence: focused animation tests 19/19, workflow Node 182/182, Python 67/67, module/smoke syntax and diff checks passed. Runner lacks .NET, so the new C# source Animator snapshot contract and SDK 8.0.426 build remain pending hosted CI.
- Do not count the fix as packaged animation parity until #153's C# contract and browser acceptance pass. If the source Animator `Moving` flag remains false while the browser records only position drift, trace the contact geometry/MovementDelta; do not weaken the predicate. Reload pose remains later in the same browser flow and must be reported separately.

## Actions #153 exact browser result — movement remains failed

Actions #153 (`40532260745`) on source SHA
`37bb00dd21aba92f60ac51d4487140172e1b9bbc` passed contracts, pinned .NET
8.0.426 Release/WASM, recovered-source/provenance checks, the full 1,919-asset
Mission package/integrity, production IndexedDB, and the independent Chromium
cache/audio job. The only failure was packaged FullSource Mission browser step
29. Retained artifact `12748315438` has reported archive digest
`sha256:fe1f411fe9563e4852250aecc6903c5a9870243e686613b872e0798e2748af9d`;
`mission-grounding.json` SHA-256 is
`8e73199df22982ecf97f9f8ed39d1264148c7c1e1142284569fb6ccb7a68ec3d`.

The source Animator should remain stationary: Left was pressed, but original
`sourceIsMoving=false` and original Animator `Moving=false`;
`MovementDelta=-0.025131702` while source/contact X velocity was approximately
zero. Only `Body stand` and a small kneel blend were selected. The smoke's
initial 0.125 m displacement came from contact correction and did not
establish movement. No browser page errors occurred. The trace showed source
acceleration `-1`, 16–17 fixed steps, and one force command but did not include
the vector, so the force magnitude/direction and contact cause remain unknown.
Do not change animation thresholds or source force/collision behavior based on
this failure.

The follow-up source commit
`d202f150789c3c4e6c63bc5c6a67b72f26db411b` adds the actual returned force
vector to bounded frame and timeout evidence. Exact Actions #154
(`40534716763`) completed on 2026-10-27. Pinned .NET 8.0.426 contracts,
Release/WASM build/publish, recovered-source provenance, full Mission package
integrity, IndexedDB acceptance and independent Chromium cache/audio passed.
Only packaged FullSource Mission browser step 29 failed. Retained browser
artifact `12748007859` has digest
`sha256:85c7c075c068002594064ef20a78beb550697c8e0fc8dc7d67695e9d8360f0b0`;
report SHA-256 is `ad4ce53c51372ef5300b2e02f0740c788cc4ec9d569e3acb2f4f51f705d63acb`,
and screenshot SHA-256 is
`e9d7954115efdf39614a5b5aef55d9a4eeff1e832b6cfbcd13903168e3a74acf`.

The exact failure still requires authored locomotion after Left input. Unlike
#153, the report now includes the original force vector: input is true and
`CharacterBody.FixedUpdate` emits `ForceMode.Acceleration` `[-22.499994,0,0]`;
the source/contact pose is synchronized, but velocity is about `-3.93e-7`,
`MovementDelta=-0.025131702`, source `IsMoving=false`, and the Animator remains
`Body stand`. The initial 0.125m X displacement satisfied the smoke's position
threshold but was not source movement; subsequent position changes also did
not meet the original movement/Animator predicate. The page error list is
empty. Do not weaken the source predicate or alter the force/collision model.

Recovered source confirms `CharacterBody.IsMoving` is
`Mathf.Abs(MovementDelta) > Time.deltaTime * 0.1f` and `UpdateAnimation()`
samples it before `LateUpdate()` updates `MovementDelta`. The next diagnostic
batch exposes source `Time.deltaTime`, the exact computed threshold, and
`Time.fixedDeltaTime` in `movementState`, with a regression tied to this
original predicate. Compare those values against the observed delta first;
only then trace PhysX/contact or source synchronization if the time threshold
does not explain the false flag. Reload pose/audio remains untested in #154
because the browser flow stops at locomotion.


## Diagnostic clock serialization regression — local candidate

The source-clock fields added to the snapshot must describe the same frame that the original source uses. `RuntimeWorld.Tick` sets `Time.deltaTime` in `BeginUpdate`, calls source `Update` then `LateUpdate`, and returns without resetting the clock; `FullSourceActorBridge.Step` serializes its snapshot after that call. A focused C# check now compares `sourceDeltaTime` to `Time.deltaTime`, `sourceFixedDeltaTime` to `Time.fixedDeltaTime`, and `sourceMovingThreshold` to `sourceDeltaTime * 0.1f`; its Node contract guards the runtime check and the recovered predicate. This prevents a stale/incorrect diagnostic from being used to explain a browser failure.

On local commit `7ca5e0d2d63be905ccafb90f6884e5998f68db9a`, tree `145e5ecf58ca88e2a3b3e2774566ccc20b4a8ad7`, focused movement/animation tests **6/6**, full Node suite **183/183**, Python suite **67/67**, browser-smoke syntax, and whitespace checks passed. The C# check has **not** run locally because this host lacks .NET. It is not pushed: one push attempt failed exactly with `fatal: could not read Username for 'https://github.com': No such device or address`. Do not repeat absent a changed credential environment. Exact-SHA pinned SDK 8.0.426 runtime verification remains untested. The currently running #155 uses parent source SHA `3ba69af…`, not this added assertion.


## Independent packaged reload acceptance — local source candidate

The shared smoke previously placed reload/arm/audio assertions after joystick, aim, and source locomotion. The exact run can fail at authored locomotion before exercising reload, leaving that independent acceptance untested. The candidate adds `--reload-only`, which skips only the stick/aim/walk interactions and preserves full package loading, source actor startup, grounding checks, trusted touch magazine pull, active source arm-pose change beyond measured idle drift, original source audio callback plus active WebAudio playback, settled magazine/chamber/task, mobile rapid-tap zoom, and page-error checks. Workflow invokes both normal grounding and reload-only commands even if the first exits nonzero, retains both reports/logs, and fails overall if either fails.

Candidate source commit `fc431f43a2e50465bd297c6740014102ca9ec6d4`, tree `df41156602a7e8ca1641bbeb242f27c68cde69b1`, parent remote docs checkpoint `cd79299532f173c37f1a40dedf528dcb6e7c9bd8`. Local reload test 4/4, movement/animation 6/6, full Node 184/184, Python 67/67, workflow YAML parse, browser-smoke syntax, and diff check passed. Actual browser and SDK 8.0.426 verification are untested locally. Code was committed in the local isolated worktree but not pushed: one shell Git push failed because no GitHub username credential was available. Do not retry without an environment change; retain exact SHA and continue independent source review.
