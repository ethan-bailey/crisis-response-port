# Gates 2–6 integration progress — 2026-10-23

Branch: `develop/wasm`
Base commit before this work: `5f1a2913e98c8a8049c39b5e3943263a3c85e8fe`
Recovered source archive SHA-256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`

This checkpoint advances the original actor AI graph and mission presentation. It does not claim full gameplay parity or completion of Gate 6.

## Gate results

| Gate | Result | Evidence and remaining work |
|---|---|---|
| 1. Complete source character | Passed at prior checkpoint | Full original CharacterBody compile, Operator/Occupant prefab lifecycles, 19 limb identities, and independent actor state are documented in `GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`. |
| 2. Weapons and arm IK | Partial | Default Checks passed all 7 original arm-sequence prefabs and 21 authored frames, source IK callbacks, reload tasks, 31-round initial M4A1 state, empty-bolt lock, and 30-round reload. Browser sequence composition and live projectile firing remain open. |
| 3. Projectile injury and body outcomes | Partial | Actor-owned physiology, movement/posture, unconsciousness, ragdoll command dispatch, timed hypoxic death, and generic material ballistics pass existing contracts. The original FullSource projectile includes its limb hit and injury callbacks, but a live hit is not yet verified. The separate transport projectile partial has an explicit unsupported limb-hit stub; capsule ray queries also reject sheared transforms without native PhysX geometry. |
| 4. Mission startup and spawning | Partial | Mission 01 starts with original Operators and Occupants from the recovered full scene package. Unsupported serialized fields/resources and native physics/render/audio remain. |
| 5. Original NPC AI | Partial, graph-load blocker fixed | Original Operator and Occupant Behavior Designer graphs deserialize with 91 and 66 tasks, no unknown nodes, and both embedded conditional references resolved. A one-second original mission AI run completed 60 ticks with 4 active trees remaining `Running`; the headless force sink recorded 153 source force commands. Navigation, perception/combat outcomes, and native physics remain unverified. |
| 6. Mission and outcome | Partial | The fixture repositions an initialized original Operator through the seven active rooms, calls original `CharacterBody.UpdateMovement`, then original `TeamController.UpdateMissionState`/`Room.Clear` and `Mission.UpdateMissionState`. The original Occupant disable/ragdoll event and `Mission.EndMission` success/UI transition pass. This does not prove input-driven movement, autonomous navigation, or a complete live objective path. |

## Fixes made in this checkpoint

- Behavior Designer v1.5.7–1.5.8 serialized field keys use Mono's deterministic polynomial-31 string hash. .NET 8 randomized `string.GetHashCode()` caused embedded `[InspectTask]` references to disappear. The legacy deserializer now uses the Mono-compatible hash only for pre-1.5.9 data; newer hash formats are unchanged.
- Restored the original archived `RestartBehaviorTree.cs` and `.meta`, with the explicit .NET `Action` alias and archive provenance. This removed the last unknown task from the two actor graphs.
- Added `--behavior-graph-contract`, which deserializes both authored actor prefabs and checks task counts, unknown tasks, and embedded condition resolution.
- Added original AI mission contracts for 60 update ticks and the source mission outcome path. New audio sources are bound to the silent headless adapter each tick; this does not claim decoded or audible output.
- Replaced direct `room.cleared = true` assignments with per-room Operator relocation, original CharacterBody room update, TeamController room-clear dispatch, and Mission objective update. The contract reports seven rooms cleared through that source path.
- Reactivated the original MissionUI root before its authored Awake path and asserted that `Mission.EndMission` makes the authored end menu visible.
- Excluded the restored `RestartBehaviorTree` gameplay source from FoundationChecks' narrower compile boundary. It remains compiled by the full runtime.

## Verification

Pinned SDK: .NET 8.0.426, using `/tmp/libprocshim.so`; no Codespaces or GitHub Actions used.

- Full runtime rebuild: passed, 0 errors and 48 warnings.
- Checks project rebuild after final contract edit: passed, 0 errors.
- Default `Crisis.Runtime.Checks`: passed; original arm sequences/reload, ownership, and ballistics contract outputs all passed.
- `--behavior-graph-contract`: passed; Operator 91 tasks / Occupant 66 tasks, zero unknown tasks, one embedded condition each resolved.
- `--mission-ai-contract`: passed; mission started, 2 Operators, 3 Occupants, 4 active trees, 60 AI simulation ticks, 153 force commands.
- `--mission-ai-outcome-contract`: passed; seven active rooms cleared through the source Operator path, all spawned Occupants ragdolled, all-disabled event fired once, mission success computed, PauseMenu initialized, camera depth-of-field found, authored end menu visible, AI remained enabled through outcome.
- EngineChecks: passed, 69 assertions.
- FoundationChecks: passed, 121 assertions.
- `check_characterbody_ownership.py --strict` and `verify_full_characterbody_enums.py`: passed.
- `verify_characterbehavior_sources.py --archive /workspace/scratch/original-recovery-artifact.zip`: passed, all 266 original AI source/metadata records verified against the pinned archive.
- `git diff --check`: passed before final documentation edits; rerun before commit.
- The broader pre-existing `verify_foundation_sources.py --archive` remains blocked by stale unrelated provenance records; it is not reported as passing.

## Failures encountered and fixes

- First one-second mission AI run failed in original `LimbAudio.Update`: a runtime-created AudioSource had no playback driver. The per-tick headless adapter now binds sources created after initial scene setup; the AI run then passed.
- First room-path attempt selected a TeamOperator whose `Body` reference was not initialized in the fixture and threw `NullReferenceException`. Selecting an original TeamOperator with a bound CharacterBody resolved the fixture failure; the source room path then cleared all seven active rooms.
- FoundationChecks initially failed with `CS0246 SharedGameObject` from restored `RestartBehaviorTree.cs`. The gameplay task was excluded from FoundationChecks' source glob while retained in the complete runtime build; 121 assertions then passed.
- A strengthened UI assertion found the authored MissionEndMenu still inactive because the fixture had deactivated its MissionUI parent. Reactivating the original root before Awake fixed the transition; the end menu is now visibly active in the contract.
- The bounded original input/controller probe reached `PlayerController.Update`, then threw `NullReferenceException` at the read of `InteriorMode.UI.InteractionMenu.Visible`: the normal headless mission fixture disables the authored InteriorMode UI. Activating that full UI and calling its original `Awake` avoided the null check but caused the headless mission run to stall during actor spawning, so the probe was stopped and the experimental contract removed. This is a fixture/UI initialization blocker, not a compile error or proof that the input mapping is broken. No further UI activation retry is justified until the UI bootstrap dependency is isolated.

## Remaining blockers and next action

Gate 6 remains partial because the room contract relocates the actor inside the headless fixture rather than applying player input or demonstrating autonomous navigation. The original controller reads `InteriorMode.UI.InteractionMenu.Visible` before its normal movement action, while the headless fixture disables that UI; activating the authored UI stalled this host during spawning. Gate 7 mission/NPC flow has since passed in the bounded headless source-runtime scope documented in `GATE7_MISSION_NPC_ACCEPTANCE_2026-10-23.md`; this does not close Gates 3–6's remaining live input, physics, firing, projectile-hit, or outcome details. For projectile work, do not mistake `runtime/Engine/ProjectileFlightHost.cs`'s global transport `Projectile.HandleLimbHit` stub for the original path: `Crisis.Runtime.FullSource.Projectile.HandleLimbHit` is implemented and calls `Limb.HandleProjectile`, records the hit, and invokes the body and interaction callbacks. `Limb.HandleInterior` then applies original material interaction, trauma, physiology/vitals, shock, and wound creation. Original entry/exit tracing also requires `LimbRenderer.Awake` to find its child `MeshRenderer` and parent collider, a non-empty authored `skins` list with readable pixel data, and `MissionHelper.Instance.RenderMaterialInstance`; `LimbTextureData.Initialize` builds the trace mask from skin alpha. A live contract also needs a successful ray query; the analytic `CapsuleCollider` explicitly throws for sheared transforms that require native PhysX geometry.
