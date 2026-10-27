# Crisis Response Port — next exact checkpoint

## Live branch and protected work

- Repository: `ethan-bailey/crisis-response-port`; branch: `develop/wasm`.
- Live HEAD is `37bb00dd21aba92f60ac51d4487140172e1b9bbc`, tree `036c3b9b7efc905d88919743fb926246e31520ba`. The code batch is committed directly to the branch by expected-HEAD update from `6564fd6e66dd52140c0633460e64a7ddc96b55b2`.
- Exact hosted Actions run #153 (`40532260745`) is **in progress** on source SHA `37bb00d`. The primary job `122287962398` is currently at “Verify recovered source and generate full source index”; the independent Chromium IndexedDB cache/audio job `122287962868` **passed**. Do not dispatch or repeat it; inspect this exact run as it advances.
- Primary checkout `/workspace/scratch/crisis-response-port-recovery` is protected: branch `develop/wasm`, local HEAD `1972f8fd2609f83ef7d4c990688ea07ed301ea77`, ahead 1 / behind 1 against stale tracking ref, protected untracked `scene-data/.crisis-response-scenes.json.gz.HtaeoS` (~73 MB). Do not clean/reset/overwrite or reconcile blindly.
- The exact-6564 worktree `/tmp/crp-github-exact-6564` contains a staged duplicate of the batch already pushed at tree `036c3b9`; it is not owner work but must not be mistaken for an uncommitted unique fix. The clean active worktree is `/tmp/crp-github-exact-37bb` at the live source SHA.
- Local runner has no .NET executable (`dotnet: command not found`); local pinned .NET 8.0.426 runtime, EngineChecks, FoundationChecks, WASM build, package integrity and packaged-browser checks are **untested on this batch**. Hosted exact-SHA evidence is required.
- No Site publish/deploy or access-policy change was made. This recurring engineering run does not publish.

## Completed source batch and evidence

- Actions #152 (`40526443453`) on parent SHA `6564fd6e66dd52140c0633460e64a7ddc96b55b2` failed only at packaged FullSource locomotion/animation. Its retained report at artifact `12744941866` showed Left pressed and X changing `-0.6600008` → `-0.7856593`, while source/contact velocity was about `-3.93e-7`, `Body stand` was selected and the adapter `Moving` value was false. Later position increments with near-zero velocity show cumulative displacement alone did not establish locomotion. The contact/collision cause remains unresolved.
- Source order was checked in original `CharacterBody.Update()` / `LateUpdate()`: `UpdateAnimation()` stores the original Animator parameters before `UpdateMovement()` refreshes `MovementDelta`; the browser performs contact integration after the FullSource tick. The old adapter independently reconstructed animation inputs from post-contact velocity.
- Commit `37bb00d` exports original `Animator` `Moving`, `Backwards`, `MoveSpeed`, `KneelRatio`, and `CrippledRatio` from `FullSourceActorBridge.Snapshot()`, plus source MovementDelta diagnostics. The FullSource render path feeds those source-owned values into the recovered AnimatorController. The packaged browser smoke now requires an authored locomotion clip while the transported source `Moving` parameter is true, so collision drift cannot count as animation/movement acceptance. A runtime contract synchronizes displacement and advances the original source ticks to assert the actual Animator snapshot values and order.
- On the exact tree that became SHA `37bb00d`: focused animation/adapter checks **19/19 passed**; complete workflow Node contracts **182/182 passed** (`node --test scripts/*_test.mjs scripts/*_contracts.mjs`); Python **67/67 passed** (`python3 -m unittest discover -s scripts -p '*test.py'`); extracted inline module syntax, browser smoke syntax, and `git diff --check` passed. These are local tests on Node v24.22.1, not the workflow Node 22 runtime. The new C# runtime assertion has not run locally because .NET is absent.
- The browser smoke already includes reload input/audio and source arm-pose capture; these remain **untested in a packaged browser** because #152 exited at locomotion before reaching them. Source reload/task contracts and their authored dependency closure are documented in `docs/FULLSOURCE_RELOAD_SWITCH_DEPENDENCY_CLOSURE_2026-10-26.md`; do not duplicate or bypass that path.

## Remaining gates in dependency order

1. Exact-SHA Release/WASM build, complete Mission package/integrity, and packaged browser locomotion/Animator acceptance (#153 in progress). If source `Moving` remains false with real input, inspect exact trace and collision/MovementDelta before changing thresholds or source behavior.
2. In that same FullSource package/browser run, verify actual reload gesture, source callback audio, original active arm-pose change beyond idle drift, reload completion and settled pose.
3. FullSource AI/NPC flow, behavior graph ownership, and mission completion under the actual packaged runtime.
4. Wound/physiology, projectile/particle lifecycle, rendered output, spatial audio and browser/native physics parity.
5. Owner iPhone Safari acceptance for load/cache, joystick/vertical absolute aim/zoom, movement, reload/audio, visuals and performance. This is deferred until device evidence arrives.
6. Combined parity declaration only after its exact-source evidence closes; keep native, pixels and iPhone flags false otherwise.

## Immediate next action

Poll/inspect Actions #153 on exact source `37bb00d`; inspect the first failed step/log if it fails. Continue independent source/acceptance work while hosted checks run. If the packaged locomotion and reload/IK assertions pass, update this handoff with each exact result and take the next dependency-ready gate. Do not publish/deploy.
