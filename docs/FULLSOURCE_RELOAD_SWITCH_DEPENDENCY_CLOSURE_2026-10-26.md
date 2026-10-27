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


## Production Mission entry disables the recovered player task tree — 2026-10-27

A packaged-page trace for the user-visible magazine/reload path found that the
browser does not currently enable the original player Behavior Designer tree:

- `runtime/Browser/Program.cs` calls
  `BrowserEntryTeamMission.Start(source, world)` without the optional flag.
- `runtime/Browser/BrowserEntryTeamMission.cs` defaults
  `enablePlayerBehavior=false`. Its `SuppressUnboundBehavior` disables every
  BehaviorTree when that flag is false, including the player tree, and the
  suppression is repeated before each normal `RuntimeWorld.Tick`.
- `game/index.html` forwards the real magazine action through
  `TickFullSourceActorBridge`; `FullSourceActorBridge.Step` applies InputHandler
  levels and runs the original world lifecycle. But the player controller's
  `PendingAction=SwitchMagazine` needs the serialized player tree to select
  `RequestedReload` and schedule `CharacterBehavior.ChangeMagazine`. With the
  tree disabled, the original extract/insert callbacks cannot be reached from
  the production browser input path.
- The source contract `SourceBehaviorReloadContracts` calls
  `BrowserEntryTeamMission.Start(..., enablePlayerBehavior:true)`; it verifies
  the real player scheduler enters the authored reload task, emits the recovered
  extract/insert audio source paths, completes a reload, then receives a real
  switch input and exits through the task's original cancellation/OnEnd path.
  That passing contract therefore does not cover the production default call.

The browser separately retrieves and decodes original mission audio clips,
constructs the production `createSourceWeaponAudioDispatcher`, calls
`playSourceAudioEvents` with each source bridge snapshot, and has a trusted
pointer gesture path to resume WebAudio. The dependency set is present; the
missing edge is player-tree activation at Mission startup.

Repair requirement: the production EntryTeam initialization must enable only
the source player tree while keeping all nonplayer trees suppressed. Preserve
the original prefab's disabled serialized BehaviorTree bit and the normal
`BrowserEntryTeamMission` default for callers that require deterministic
suppression; change only the actual playable-page caller. Extend the exact
integrity-verified packaged-page smoke to perform the original magazine
down-drag and observe source reload playback through the real audio runtime
after the browser gesture. Keep the source contract for actual task, ammo,
callbacks and interruption as a separate assertion. Do not synthesize reload
events, call the task directly, or count clip preloading alone as reload sound.
This production integration is **untested** until an exact-SHA package/browser
run passes.


## Packaged touch HUD does not receive FullSource magazine state — 2026-10-27

A second caller/callee trace found that the normal touchscreen reload gesture is
also disconnected from the active FullSource weapon:

- `game/magazine-hud.mjs` sets its private `ready` flag only when
  `render(state)` receives a recognized `weaponId` and valid magazine
  capacity. `bindMagazineDrag` calls `onReload` only when
  `canDrop() === ready && !reloading`; a drag gesture cannot start a reload
  while the HUD has no source state.
- The control is initially rendered with `null`. In the legacy branch,
  `game/index.html` updates the HUD from `weaponState` after
  `upperBody.update(...)`. The FullSource branch instead returns from
  `frame()` after `TickFullSourceActorBridge`/projection and never calls
  `magazineHud.render` or updates the HUD-owned `weaponState`.
- `runtime/Engine/FullSourceActorBridge.cs` currently snapshots only the active
  weapon's name; the original live state is available through
  `CharacterBody.ActiveWeapon`, `WeaponBase.Magazine.roundCount/capacity`,
  `WeaponBase.ChamberStatus`, `CharacterBody.Equipment` and
  `CharacterBody.IsManipulatingEquipment`. The source enum defines chambered
  round as `WeaponBase.ChamberState.Live`. Primary/secondary reserve magazines
  are owned by `Equipment.PrimaryMagazines` / `SecondaryMagazines` and are
  filtered by `Magazine.calibre`.
- The actual touchscreen control only accepts a downward drag of at least 45
  CSS pixels (then checks its ready/reloading state); tapping it is intentionally
  not a reload. The browser input bridge forwards that action as the source
  `magazine` key. The current source behavior-tree default further prevents the
  original reload task from running, as documented immediately above.

Implementation dependency: snapshot the source-owned active magazine values
needed by `magazinePresentation` (`weaponId`, `magazine`,
`magazineCapacity`, `chamberLive`), reserve count for the HUD/status, and
`reloading` from source state; feed this snapshot to `magazineHud.render` on
every FullSource frame. Enable only the player behavior tree at the production
EntryTeam startup call; keep nonplayer trees suppressed and do not mutate the
prefab's serialized BehaviorTree flag. The exact packaged browser regression
must see a ready HUD, perform the production 45px+ touch drag, observe the
source task's audio event and actual WebAudio playback after the trusted
gesture, and check that the control returns from reloading. The existing
source scheduler contract remains responsible for task/OnEnd, ammo, magazine
identity and interruption details. This production touch/audio integration is
**untested** until the exact-SHA packaged page passes.
