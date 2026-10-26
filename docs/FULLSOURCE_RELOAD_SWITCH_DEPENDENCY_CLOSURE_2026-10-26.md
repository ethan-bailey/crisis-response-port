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
| Behavior tree scheduler | Original BehaviorTree component, deserialized source task graph, BehaviorManager.EnableBehavior/Tick, task lifecycle | Prove the Operator tree contains and enters the FullSource magazine-change action before the switch. BrowserEntryTeamMission currently disables behavior trees in its fixture, so explicitly enable the recovered tree through its normal manager, then report active task type/status and scheduler order. Do not substitute a manually constructed task. |
| FullSource reload action | runtime/Original/CharacterBehavior/CharacterBehavior/ChangeMagazine.cs (CharacterBehavior.ChangeMagazine) | Confirm serialized isPlayer/quick settings and initial state. OnStart captures self.ActiveWeapon, sets IsManipulatingEquipment, selects arm/bolt solver, and checks reserves. OnUpdate supports an owner-weapon-change exit only when isPlayer is true; it returns Success after moving the magazine arm back to the interrupted weapon. OnEnd clears PendingAction/IsManipulatingEquipment and may queue WorkBolt. |
| Reload branch selection | runtime/Original/CharacterBehavior/CharacterBehavior/Reload.cs and recovered serialized Behavior Designer graph | Reload.OnUpdate currently returns TaskStatus.Failure. Inspect the actual graph and task GUIDs to establish whether this action is intended to fail through to ChangeMagazine or whether an owner edge is missing. Do not change it based only on its class name. |
| Host boundary | runtime/Original/Weapons/ChangeMagazine.cs and runtime/Engine/WeaponHost.cs | These form the protected single-M4A1 host path used by existing reload tests. They use Crisis.Runtime.CharacterBody/WeaponTask, not the FullSource CharacterBody/Behavior Designer task. Keep the host and frozen hash unchanged; treat its tests as separate evidence. |
| Cancellation/teardown | CharacterBehavior.ChangeMagazine.OnUpdate/OnEnd, both original arm sequencers, BehaviorManager.PopTask | Switch causes the task to observe self.ActiveWeapon != weapon; verify exactly one task exit/OnEnd, recovery pose completion, both sequencers ready, correct owner weapon, no stale manipulation flag or pending action, and no unrequested magazine insertion. Determine from source bolt/magazine state whether OnEnd legitimately schedules WorkBolt. |

## Key source facts and risk checks

- PlayerController.Update handles switchWeapons.Down after its fire/reload-input branch. Its fire trigger is blocked while Body.IsManipulatingEquipment, but the switch call is not guarded by that flag.
- CharacterBehavior.ChangeMagazine.OnUpdate is the FullSource interruption candidate: when serialized isPlayer is true and active weapon differs from its captured weapon, it calls magazineArm.SetOnWeapon(weapon, 0.1f, usingRightArm) and returns Success. The scheduler must then call OnEnd; verify that order on the actor rather than only invoking methods directly.
- CharacterBehavior.ChangeMagazine.OnStart reads weapon.cyclingBolt before its null check, and OnEnd dereferences weapon.cyclingBolt; the fixture must start with a valid active source weapon.
- The same file has a separate fire-interruption path when the task reaches State9 or later. Keep it separate from the weapon-switch result.
- The FullSource Behavior Designer ChangeMagazine action is separate from the host Crisis.Runtime.Weapons.ChangeMagazine. Conflating them validates the wrong actor and lifecycle.
- Reload returning Failure is observed source behavior, not yet proof of a missing feature: Behavior Designer may route to a sibling task. The recovered graph and active-task trace decide this.

## Test matrix

1. Enable the real serialized Operator BehaviorTree and record original root/child task identities, enabled state, and tick order.
2. Cause the source reload-selection condition through the original player input/behavior path; require active task CharacterBehavior.ChangeMagazine and at least one Running update before interruption.
3. Capture exact reload state (state index, magazine ownership/counts, ammo, bolt, active weapon, arm targets) immediately before switching.
4. Deliver only the original switchWeapons Down edge; record task update, IK callbacks, task status, and scheduler OnEnd order.
5. Assert M4A1→Glock settles with its serialized callback sequence; the reload task exits once; both sequencers settle; task flags and owner state clear; magazine/ammunition match the source outcome for the captured state.
6. Repeat Glock→M4A1 and verify no stale task or pending action blocks re-entry. Separately verify fire cancellation at/before State9 if the source graph enables it.
7. Run the complete Node/Python/runtime/engine/foundation suites, Release WASM package build and independent manifest/archive verifier, then Chromium IndexedDB/audio on the exact pushed source SHA. Do not save or deploy a Site version for test-only changes.

## Open evidence boundary

The source switch contract proves authored M4A1↔Glock switching without an active reload task. The earlier M4A1 host contract proves its own Crisis.Runtime.Weapons.ChangeMagazine reload states. Neither proves that the recovered Operator BehaviorTree starts or cancels the FullSource Behavior Designer reload action during a player swap. The serialized tree/task routing, runtime scheduler activation, and exact cancellation-state ammo outcome must be verified before changing a source task or declaring this gate closed.
