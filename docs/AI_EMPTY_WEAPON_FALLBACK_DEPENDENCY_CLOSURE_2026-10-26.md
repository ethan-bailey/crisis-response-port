# AI empty-weapon fallback dependency closure — 2026-10-26

## Acceptance target

Exercise the authored AI fallback on the recovered Operator: when the current weapon is permanently empty, its magazine and all same-caliber reserves are empty, and the secondary weapon remains usable, the live source Behavior Designer graph selects `CharacterBehavior.SwitchWeaponCondition`, sets the original `PendingAction`, runs `CharacterBehavior.SwitchWeapon`, and completes `IKArmController.SwitchWeapons`. Verify one M4A1→Glock transition, cleared pending action, settled source sequencers, preserved weapon ownership, and unchanged Glock ammunition. Negative cases (still-reloadable M4A1 and no usable secondary) must not start the swap.

## Complete dependency set

| Layer | Source dependency | Acceptance evidence |
|---|---|---|
| Serialized graph | Canonical `Assets/Resources/characters/Operator.prefab`; source `BehaviorTree`, task type map, `ConditionalEvaluator`, `Sequence`, `CharacterBehavior.SwitchWeaponCondition`, `CharacterBehavior.SwitchWeapon` | Packaged graph contract must assert one reachable condition/action pair as direct children of one Sequence, no unknown task types, and exact paths. The recovered six-root Mission profile has 1,919 assets / 22,365 records. |
| Mission lifecycle | `Mission.BeginMission` → `CharacterBody.OnMissionBegin` → `Behavior.EnableBehavior` → `BehaviorManager.Tick` | Run a nonplayer Operator in the real Mission bootstrap and observe its enabled deserialized tree. Do not call the condition or action directly. |
| Operator and loadout | Operator prefab/hierarchy; `OperatorSpawnData`; `SerializableTeam`; `EquipmentHelper`; `CharacterBody.InitializeEquipment`; `Equipment.primaryWeapon/secondaryWeapon`; `ActiveWeapon/InactiveWeapon` | Use original source M4A1 + Glock-18 and their owned magazines/ammunition. The six-root profile includes both weapon graphs. No slot or hand assignment in the test. |
| Fallback condition | `SwitchWeaponCondition.OnUpdate`: `ChamberPermanentlyEmpty`, empty active magazine, no same-weapon ammunition in `Equipment`, `GetNonEmptyWeapon()!=null` | Stage a genuine source empty-weapon state for fixture setup via original `Magazine.RemoveOne(true)` and `CyclingBolt.EjectCasing(0)`. Assert the four source predicates before ticking; leave the secondary Glock loaded. No PendingAction assignment. |
| Task and lifecycle | `SwitchWeapon.OnStart` → original `IKArmController.SwitchWeapons`; `OnUpdate` returns Running while IK plays then source Failure; `OnEnd` clears PendingAction | Observe condition Success, action Running, active weapon change, action Failure/exit, and `OnEnd` cleanup via the live scheduler. Source Failure after a completed action is expected; the graph's parent determines next selection. |
| IK sequences | `IKArmController`, `IKArmSequencer`, `IKArmSequenceRuntime`, callbacks, exact `Holster pistol`, `Unholster pistol`, `Lower weapon`, `Raise shoulder weapon` assets | Reuse the source-driven switch contract's seven GUID/path/hash checks and callback/settlement assertions. For M4A1→Glock, verify lower-shoulder then unholster-pistol callbacks and both sequencers Ready. |
| Native adapters | Source Rigidbody, casing `Eject`, browser/headless physics-force sink, source silent/audio adapter | The fixture's source casing ejection requires a no-op headless force sink; this does not claim PhysX/ejection physics. Do not advance unsupported joint physics. |
| Failure boundaries | Original `SwitchWeaponCondition`, `SwitchWeapon`, `BehaviorManager` and all task `OnEnd` paths | Assert no swap if M4A1 has remaining reserve or if Glock has no nonempty magazine; compare task statuses, PendingAction, active weapon and sequencer state. |

## Workflow ordering

The static graph contract is a prerequisite for the runtime fallback assertion. The runtime slice is independent from the player `magazine` reload-interruption check but uses the same six-root package and source switch/IK implementation. After local focused checks, run Node/Python/runtime/engine/foundation, Release WASM, full source extraction/profile/package verification, and Chromium persistence/audio on the exact pushed SHA. Do not deploy until the exact source/package publisher alignment is separately resolved and explicitly authorized.

## Current checkpoint

The packaged graph now reports the exact AI route:

- `SwitchWeaponCondition`, ID 45, path `Root/Conscious[0]/Selector[1]/Computer[0]/Sequence[0]/Activities[0]/Actions[2]/Switch[0]`.
- `SwitchWeapon`, ID 46, path `Root/Conscious[0]/Selector[1]/Computer[0]/Sequence[0]/Activities[0]/Actions[2]/Switch[1]`.

The static contract asserts the direct sibling Sequence. The source runtime now uses the actual `OperatorSpawner` route: a second `SerializableEquipment` entry is consumed by `OperatorSpawnData.Team.entryMembers`, the Mission runs in Entry Team mode, and the probe selects the spawned nonplayer actor by its initialized primary weapon. The Mission lifecycle enabled its BehaviorTree (`IsOperator=true`, `IsPlayer=false`); the test does not toggle it manually.

## 2026-10-26 runtime gate passed locally

Command: `dotnet runtime/Checks/bin/Release/net8.0/Crisis.Runtime.Checks.dll --mission-ai-switch-fallback-contract /tmp/cpr-current-mission-profile-full` with pinned .NET 8 SDK 8.0.426 and the complete six-root Mission profile.

- Entry Team spawned `Operator 1` as player and `Operator 2` as a source-equipped nonplayer M4A1/Glock teammate. The scene's unconfigured `Operator` placeholder is explicitly excluded by selecting the actor with `Equipment.primaryWeapon != null`.
- Mission `BeginMission` enabled the nonplayer BehaviorTree; its 91-task serialized source graph had no unknown task types. The static graph contract resolves ID 45 `SwitchWeaponCondition` and ID 46 `SwitchWeapon` as direct siblings in one authored Sequence.
- Before depletion, the nonplayer stayed on its loaded M4A1 with no pending action or IK transition during the source-tree warmup.
- The test depleted original source magazine objects with `Magazine.RemoveOne(true)` and ejected the live chamber through `CyclingBolt.EjectCasing(0)`. It verified a permanently empty M4A1, no matching reserve ammunition, and a loaded Glock fallback before letting the BehaviorManager run.
- The active source task snapshot observed `SwitchWeapon#46:Running`; the weapon changed M4A1→Glock-18. In 46 frames the action settled with M4A1 inactive/holstered, Glock active/held, both IK sequencers ready, and `PendingAction=None`. No shot fired and the Glock held 17 rounds at swap completion.
- After the positive case, the test emptied the Glock and its source reserves and ejected its live chamber. With both weapons empty and `GetNonEmptyWeapon()==null`, 120 source ticks showed no SwitchWeapon action, no swap, no pending action and no IK transition.
- The Behavior Designer API reports transient node statuses as `Inactive` after the completed tree step; the active-task snapshot captured the action as Running. The serialized direct Sequence plus that source action execution establishes progression through its conditional. The test does not claim to capture an instantaneous condition Success or the action's post-completion Failure enum.
- Casing ejection force uses a no-op headless sink. Native PhysX casing motion, rendered output, and iPhone Safari are not claimed.

The source runtime command has been added to `.github/workflows/source-contracts.yml` after the static graph and player reload contracts. Full exact-source workflow/package verification remains required for this changed SHA.


## Exact-source pass — Actions #124

On SHA 27ab7a17a9c18ff32a4c80e23aebd946a02afe78, Actions #124 runtime contract exercised this live route and passed. Mission.BeginMission enabled nonplayer Operator 2's deserialized 91-task tree. The contract confirmed a loaded M4A1 does not trigger fallback, genuinely depleted original magazine/reserve/chamber state plus loaded Glock triggers the authored SwitchWeapon task, M4A1→Glock settles in 46 frames, both IK sequencers are Ready, PendingAction is cleared, no shot is fired, and Glock ammunition remains 17. With both source weapons empty, 120 ticks produced no action or swap. The headless casing force sink does not verify native physics. The exact full Mission package, recovered source/index provenance, and Chromium browser jobs passed the same SHA. This closes the headless source fallback gate; rendered AI/perception/behavior in the published page and physical/device acceptance remain open.
