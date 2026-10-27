# FullSource reload interruption dependency closure — 2026-10-26

## Acceptance target

With the recovered default Operator (M4A1 primary, Glock-18 secondary) loaded from the exact Mission source package, start the authored FullSource reload task through its real owner and interrupt it with the original PlayerController weapon-switch input. Verify that the task exits through its authored cancellation path, the M4A1-to-Glock transition completes from serialized IK sequences, and ammo, magazine, task, and pending-action state are correct for the precise reload state at interruption. Verify the reverse transition and repeat entry where the source lifecycle supports it.

This is not the WeaponHost reload contract. Do not assign weapon slots, PendingWeapon, task state, arm frames, or magazine counts directly.

## Dependency graph

| Layer | Recovered owner/input | Requirement before acceptance |
|---|---|---|
| Mission/profile | BrowserEntryTeamMission.Start, full-source Mission package and game/assets/original-arm-sequences.json provenance | Use the verified 1,919-asset Mission profile; retain serialized Operator, weapon, magazine, ammunition, behavior-tree, and IK references. |
| Actor/loadout | FullSource CharacterBody, serialized OperatorSpawnData, EquipmentHelper, M4A1 + 5.56-FMJ and Glock-18 + 9-FMJ | Load through source equipment creation. Record active/inactive weapon, ammo, and magazine counts before task start. |
| Player switch owner | FullSource.PlayerController.Update → InputHandler.Keys.switchWeapons.Down → IKArmController.SwitchWeapons() | Send a source input edge in the Mission controller path. Preserve the passing 45-frame forward and 32-frame reverse switch contract. |
| Behavior tree scheduler | Original BehaviorTree component, deserialized source task graph, BehaviorManager.EnableBehavior/Tick, task lifecycle | Mission.OnMissionBegin enables the Operator tree through the original lifecycle. BrowserEntryTeamMission normally suppresses all trees for deterministic browser sessions; its new opt-in preserves only the source player tree after normal startup while continuing to suppress nonplayer trees. Assert the tree/root and active task/status. Do not directly toggle a task or construct one. |
| FullSource reload action | runtime/Original/CharacterBehavior/CharacterBehavior/ChangeMagazine.cs (CharacterBehavior.ChangeMagazine) | Confirm serialized isPlayer/quick settings and initial state. OnStart captures self.ActiveWeapon, sets IsManipulatingEquipment, selects arm/bolt solver, and checks reserves. OnUpdate supports an owner-weapon-change exit only when isPlayer is true; it returns Success after moving the magazine arm back to the interrupted weapon. OnEnd clears PendingAction/IsManipulatingEquipment and may queue WorkBolt. |
| Reload branch selection | Original `Operator.prefab` Behavior Designer `TaskData` plus `RequestedReload.cs` | The recovered Operator graph serializes `CharacterBehavior.RequestedReload` immediately before `CharacterBehavior.ChangeMagazine` as direct children of the same Sequence. `RequestedReload.OnUpdate` succeeds only when source `CharacterBody.PendingAction` is `SwitchMagazine`; otherwise it fails. The Operator TaskData does not include a `CharacterBehavior.Reload` task type. The similarly named `Reload.cs` file is not evidence of the Operator reload route. The test contract now asserts this direct authored edge and reports both ChangeMagazine task settings. |
| Host boundary | runtime/Original/Weapons/ChangeMagazine.cs and runtime/Engine/WeaponHost.cs | These form the protected single-M4A1 host path used by existing reload tests. They use Crisis.Runtime.CharacterBody/WeaponTask, not the FullSource CharacterBody/Behavior Designer task. Keep the host and frozen hash unchanged; treat its tests as separate evidence. |
| Cancellation/teardown | CharacterBehavior.ChangeMagazine.OnUpdate/OnEnd, both original arm sequencers, BehaviorManager.PopTask | Switch causes the task to observe self.ActiveWeapon != weapon; verify exactly one task exit/OnEnd, recovery pose completion, both sequencers ready, correct owner weapon, no stale manipulation flag or pending action, and no unrequested magazine insertion. Determine from source bolt/magazine state whether OnEnd legitimately schedules WorkBolt. |

## Key source facts and risk checks

- PlayerController.Update handles switchWeapons.Down after its fire/reload-input branch. Its fire trigger is blocked while Body.IsManipulatingEquipment, but the switch call is not guarded by that flag.
- CharacterBehavior.ChangeMagazine.OnUpdate is the FullSource interruption candidate: when serialized isPlayer is true and active weapon differs from its captured weapon, it calls magazineArm.SetOnWeapon(weapon, 0.1f, usingRightArm) and returns Success. The scheduler must then call OnEnd; verify that order on the actor rather than only invoking methods directly.
- CharacterBehavior.ChangeMagazine.OnStart reads weapon.cyclingBolt before its null check, and OnEnd dereferences weapon.cyclingBolt; the fixture must start with a valid active source weapon.
- The same file has a separate fire-interruption path when the task reaches State9 or later. Keep it separate from the weapon-switch result.
- The FullSource Behavior Designer ChangeMagazine action is separate from the host Crisis.Runtime.Weapons.ChangeMagazine. Conflating them validates the wrong actor and lifecycle.
- Reload returning Failure is observed source behavior, not yet proof of a missing feature: Behavior Designer may route to a sibling task. The recovered graph and active-task trace decide this.
- The canonical recovered `Assets/Resources/characters/Operator.prefab` TaskData type table was inspected directly: its reload-selection task is `CharacterBehavior.RequestedReload`, with the `CharacterBehavior.ChangeMagazine` action as its sibling in that Sequence; it does not serialize `CharacterBehavior.Reload`. The prefab's BehaviorTree MonoBehaviour has `m_Enabled: 0`. The rebuilt graph contract now emits the serialized `isPlayer`/`quick` values, path, and enabled-component count from the packaged source object. This closes the static graph-routing uncertainty, but does not prove the source scheduler enters or ends the action.

## Test matrix

1. Use the packaged graph contract to confirm RequestedReload → ChangeMagazine and task settings.
2. Send the original `magazine` input edge; require PlayerController to set PendingAction and the real player `ChangeMagazine` action to become Running in the original BehaviorManager.
3. Let the first task entry settle via the source scheduler and verify original magazine extract/insert audio callbacks and cleanup.
4. Enter the same action again, capture its serialized state and magazine/shot snapshot, then deliver only `switchWeapons` through PlayerController.
5. Require the task's active-weapon cancellation result to be Success; verify `OnEnd` cleanup, M4A1→Glock ownership, and both authored IK sequencers settled. Record the precise ammo result, even if it reveals a source issue.
6. Run the complete Node/Python/runtime/engine/foundation suites, Release WASM package build and independent manifest/archive verifier, then Chromium IndexedDB/audio on the exact pushed source SHA. Do not save or deploy a Site version for test-only changes.

## Evidence boundary

The source switch contract proves authored M4A1↔Glock switching without an active reload task. The earlier M4A1 host contract proves its own Crisis.Runtime.Weapons.ChangeMagazine reload states. The FullSource runtime contract has now passed locally against the actual six-root Mission profile, including quick reload entry/audio/cleanup and the source PlayerController weapon-switch interruption with BehaviorManager task completion and authored IK settlement. Exact-SHA Actions/package verification for the changed code is still required. This gate remains separate from the protected `WeaponHost` path.

## 2026-10-26 runtime slice — locally passed; exact-SHA verification pending

`BrowserEntryTeamMission.Start` now accepts `enablePlayerBehavior` (default `false`, preserving existing behavior). When true, Mission startup still enables the player's serialized tree through `CharacterBody.OnMissionBegin`, then the browser fixture suppresses only nonplayer trees. `--mission-player-reload-contract` sends source `magazine` and `switchWeapons` input edges, observes the authored player task through `Behavior.GetActiveTasks()`, reports its private source state at interruption, and checks scheduler result/cleanup, source audio paths, active weapon and sequencer settlement. It does not assign PendingAction, task fields, magazines, weapon slots, or IK frames.

The workflow invokes this runtime contract and the static graph contract against the same `global-ui-mission-operator` profile. Local Release build passed with 0 errors (two pre-existing nullable warnings); Node 143/143, Python 45/45, static graph, player reload, and AI fallback commands passed against the complete six-root Mission profile. The AI probe uses the original `OperatorSpawner` and explicitly excludes the unconfigured scene placeholder; see the separate AI closure for its runtime evidence. These local checks do not substitute for the exact pushed SHA workflow/package checks. The prior full canonical recovery export and CI-equivalent profile contain 1,919 assets, 22,365 records, 97,767,549 compressed asset bytes and 205,514,345 source bytes.


## Production reload controls — source edges wired; browser acceptance pending — 2026-10-27

Current `develop/wasm` source inspection confirms that the production actor and touch-HUD paths are wired; the remaining gate is actual packaged browser acceptance:

- `runtime/Browser/Program.cs` calls `BrowserEntryTeamMission.Start(source,world,enablePlayerBehavior:true)`. `BrowserEntryTeamMission.SuppressUnboundBehavior` enables the original player-owned BehaviorTree only and suppresses null-owner/nonplayer trees on each world tick. The Operator prefab's serialized disabled bit is preserved.
- `game/index.html` forwards the real magazine action through `TickFullSourceActorBridge`; after source tick and contact synchronization it assigns `weaponState=sourceActorSnapshot.weaponState` and calls `magazineHud.render(weaponState)`. `FullSourceActorBridge.ActiveWeaponState()` provides source-owned weaponId, active rounds/capacity, chamber state, calibre-filtered reserve count, and equipment-manipulation state.
- `game/magazine-hud.mjs` accepts only a downward drag of at least 45 CSS pixels and blocks gestures while source state is unknown or already reloading. It maps the gesture to original `InputMapping.magazine`; taps and canceled gestures do not trigger reload.
- The exact-profile `--mission-player-reload-contract` passes through the original player tree, `RequestedReload` and `ChangeMagazine`, emits recovered extract/insert audio callbacks, and tests actual ammo/task/OnEnd/interruption state. This source-level contract does not substitute for the browser pointer, audio-decoder, or WebAudio gate.
- Exact packaged browser assertions are already in `scripts/mission_grounding_browser_smoke.mjs`: verified source weapon state, usable HUD, real downward drag, original source audio callback, active WebAudio playback, settled reload and no page errors. **Untested:** these assertions have not run to completion because run #148 failed earlier at movement. The source edges are present; do not re-implement them without new failing evidence.

## Packaged reload arm-pose acceptance — 2026-10-27 (next source batch)

### Newly inventoried observable dependency

The existing reload smoke checks active source weapon/ammunition, original reload callback audio, and task completion, but it does not yet retain an observable baseline for the original IK arm pose. The serialized reload task's movement must be proven through the original source actor, not inferred from the magazine HUD or a browser-only hand mesh.

- runtime/Engine/FullSourceActorBridge.cs: Snapshot() traverses the FullSource body transform tree and returns each local pose as path, position p, rotation q, scale s, and activeSelf. This includes Operator upper/lower-arm transforms and hand transforms.
- game/index.html: projectFullSourceSnapshot() maps those original transforms to the packaged Operator hierarchy before applyAnimation(dt, true). SourceCharacterAnimator samples the recovered body controller after source projection; original controller tracks intentionally omit upper-arm, lower-arm, and hand paths so body clips do not overwrite source IK.
- runtime/Original/CharacterBehavior/CharacterBehavior/ChangeMagazine.cs: the source Behavior Designer owner selects the magazine arm from the active weapon's fireFromMagazine setting, requests the serialized magazine/bolt target transitions, and its OnEnd clears task state. runtime/Original/FullSource/ArmIKSolver.cs updates its two-bone solution and upper/lower arm rotations; runtime/Original/FullSource/IKArmSequencer.cs advances the source-authored frames by solver readiness and sibling waits.
- The M4A1 reload entry must still be triggered by the actual touch magazine gesture through InputHandler, RequestedReload, and the serialized Operator behavior graph. Do not construct a task, force a pending action, move a solver frame, or set transform values in the test.

### Exact packaged assertion to add

After run #147 resolves the prior locomotion acceptance, add a package-browser observation to the same live Mission flow: capture the source arm transform baseline while the weapon is not reloading; issue the trusted touch magazine pull; while weaponState.reloading is true, require a measurable change in at least one original upper/lower-arm local rotation or position from the same source poses snapshot. Continue requiring the source reload audio callback and active WebAudio playback; then wait for source reload completion and verify M4A1/magazine/chamber state, settled input, and no browser exceptions. Retain before/during/after source arm samples in the evidence report so a failure can be attributed to the original task or solver rather than the HUD.

Use stable joint path identity and quaternion-safe comparison (absolute quaternion dot product); choose the movement threshold from the source pose values observed in the exact packaged Mission run, not an invented fixture. If the source action is too brief to sample at the current frame cadence, add bounded runtime sampling of the actual source snapshots during the task rather than bypassing their lifecycle.

### Status

- **Passed:** bridge and projection source inventory described above; exact-source headless regression asserts projected source poses precede the source Animator sample and body clips do not overwrite arm/hand paths.
- **Untested:** packaged browser observation of IK arm motion during the original active reload task. Actions #149 completed its package and IndexedDB checks but failed earlier at the authored-animation wait, so the reload section was never reached. The exact failure was a 30-second timeout waiting for `Body walk`, `Body walk backward`, or `Body sprint`; Left input and at least 0.1 m displacement had already passed. Thus no reload pose, reload audio, or mobile device acceptance is claimed from #149.

## Live source correction and pose instrumentation — 2026-10-27

The two production-edge observations above were stale relative to `develop/wasm` tree `59748c4f8221d6d42d1bbc5e83a14cda4beaaf11`:

- **Passed by source inspection:** `runtime/Browser/Program.cs` calls `BrowserEntryTeamMission.Start(source,world,enablePlayerBehavior:true)`. Its `SuppressUnboundBehavior` keeps only the original player BehaviorTree enabled and continues suppressing trees without a player owner and all nonplayer trees. The serialized Operator prefab flag is not rewritten.
- **Passed by source inspection and exact-profile source contract:** after `TickFullSourceActorBridge`, the production FullSource frame reads `sourceActorSnapshot.weaponState` and calls `magazineHud.render(weaponState)` on each frame. `FullSourceActorBridge.ActiveWeaponState()` snapshots the source weapon/magazine/chamber/reserve/manipulation owner state. The normal 45 CSS px+ `magazineTouch` drag reaches the original magazine input. The source-enabled task graph was exercised by the exact-profile player reload/interrupt contract.
- **Still untested:** packaged browser touch→reload audio/visual integration; #148 stopped at locomotion before these assertions. Do not treat the source graph, HUD snapshot, headless reload contract, or a source code read as browser or iPhone acceptance.
- The browser now exposes only the original left/right upper arm, lower arm, and hand paths from `sourceActorSnapshot.poses`. It records a stable idle trace and snapshots while the actual serialized reload task reports `weaponState.reloading`. Quaternion comparison normalizes both values and treats `q` and `-q` as the same rotation. The acceptance requires measured active reload pose change to exceed the observed idle drift and stores joint deltas and sample count. It does not set task/solver/transform state. `scripts/fullsource_reload_pose_test.mjs` covers filtering, quaternion sign equivalence, measured movement, player-tree ownership, and per-frame HUD/pose plumbing.
- **Local verification:** focused reload-pose/animator/HUD tests passed 18/18, full Node passed 175/175, and browser smoke syntax passed. Exact package and hosted browser acceptance on this observation batch remain pending.

## Exact package checkpoint — 2026-10-27

- Local source commit `683050369990b307202d9168bd6d5db92263b593`, tree `e5db3fe8b03271575886fd2adac1bbe5c4536741`: package build and `verify` **passed**, with 2,691 files, 199 Blazor resources, 195 WASM files and all 1,919 profile assets.
- Exact profile `browser-entryteam-session`, `mission-player-reload`, and `behavior-graph` contracts **passed**. Reload pose unit contract passed 3/3; full Node suite passed 174/174; Python unittest discovery passed 67/67; movement snapshot contract passed; smoke syntax and `git diff --check` passed.
- Hosted packaged reload observation is pending. Local browser execution remains unavailable after one bounded Playwright download attempt failed. Current remote HEAD before pushing this batch is `4795aaa5b767efb64b26732599360ff999fe3ffe`.
