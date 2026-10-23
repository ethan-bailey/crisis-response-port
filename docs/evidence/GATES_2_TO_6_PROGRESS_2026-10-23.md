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
| 3. Projectile injury and body outcomes | Partial | Actor-owned physiology, movement/posture, unconsciousness, ragdoll command dispatch, timed hypoxic death, and generic material ballistics pass existing contracts. Original projectile-to-limb tissue/injury callback is not implemented by the runtime projectile host and remains open. |
| 4. Mission startup and spawning | Partial | Mission 01 starts with original Operators and Occupants from the recovered full scene package. Unsupported serialized fields/resources and native physics/render/audio remain. |
| 5. Original NPC AI | Partial, graph-load blocker fixed | Original Operator and Occupant Behavior Designer graphs deserialize with 91 and 66 tasks, no unknown nodes, and both embedded conditional references resolved. A one-second original mission AI run completed 60 ticks with 4 active trees remaining `Running`; the headless force sink recorded 153 source force commands. Navigation, perception/combat outcomes, and native physics remain unverified. |
| 6. Mission and outcome | Partial | Source Occupant unconsciousness/ragdoll and all-disabled event pass. Original `Mission.EndMission` computes success and shows the authored `MissionEndMenu` (`Visible=true`) after initializing the authored PauseMenu and camera depth-of-field dependencies. The fixture sets rooms checked directly; player/AI room traversal and the complete objective path remain unverified. |

## Fixes made in this checkpoint

- Behavior Designer v1.5.7–1.5.8 serialized field keys use Mono's deterministic polynomial-31 string hash. .NET 8 randomized `string.GetHashCode()` caused embedded `[InspectTask]` references to disappear. The legacy deserializer now uses the Mono-compatible hash only for pre-1.5.9 data; newer hash formats are unchanged.
- Restored the original archived `RestartBehaviorTree.cs` and `.meta`, with the explicit .NET `Action` alias and archive provenance. This removed the last unknown task from the two actor graphs.
- Added `--behavior-graph-contract`, which deserializes both authored actor prefabs and checks task counts, unknown tasks, and embedded condition resolution.
- Added original AI mission contracts for 60 update ticks and the source mission outcome path. New audio sources are bound to the silent headless adapter each tick; this does not claim decoded or audible output.
- Reactivated the original MissionUI root before its authored Awake path and asserted that `Mission.EndMission` makes the authored end menu visible.
- Excluded the restored `RestartBehaviorTree` gameplay source from FoundationChecks' narrower compile boundary. It remains compiled by the full runtime.

## Verification

Pinned SDK: .NET 8.0.426, using `/tmp/libprocshim.so`; no Codespaces or GitHub Actions used.

- Full runtime rebuild: passed, 0 errors and 48 warnings.
- Checks project rebuild after final contract edit: passed, 0 errors.
- Default `Crisis.Runtime.Checks`: passed; original arm sequences/reload, ownership, and ballistics contract outputs all passed.
- `--behavior-graph-contract`: passed; Operator 91 tasks / Occupant 66 tasks, zero unknown tasks, one embedded condition each resolved.
- `--mission-ai-contract`: passed; mission started, 2 Operators, 3 Occupants, 4 active trees, 60 AI simulation ticks, 153 force commands.
- `--mission-ai-outcome-contract`: passed; 4 spawned Occupants ragdolled, all-disabled event fired once, mission success computed, PauseMenu initialized, camera depth-of-field found, authored end menu visible, AI remained enabled through outcome.
- EngineChecks: passed, 69 assertions.
- FoundationChecks: passed, 121 assertions.
- `check_characterbody_ownership.py --strict` and `verify_full_characterbody_enums.py`: passed.
- `verify_characterbehavior_sources.py --archive /workspace/scratch/original-recovery-artifact.zip`: passed, all 266 original AI source/metadata records verified against the pinned archive.
- `git diff --check`: passed before final documentation edits; rerun before commit.
- The broader pre-existing `verify_foundation_sources.py --archive` remains blocked by stale unrelated provenance records; it is not reported as passing.

## Failures encountered and fixes

- First one-second mission AI run failed in original `LimbAudio.Update`: a runtime-created AudioSource had no playback driver. The per-tick headless adapter now binds sources created after initial scene setup; the AI run then passed.
- FoundationChecks initially failed with `CS0246 SharedGameObject` from restored `RestartBehaviorTree.cs`. The gameplay task was excluded from FoundationChecks' source glob while retained in the complete runtime build; 121 assertions then passed.
- A strengthened UI assertion found the authored MissionEndMenu still inactive because the fixture had deactivated its MissionUI parent. Reactivating the original root before Awake fixed the transition; the end menu is now visibly active in the contract.

## Remaining blockers and next action

Gate 6 is still partial because no contract drives an Operator/player through room traversal and source objective updates; rooms are currently marked checked by the outcome fixture. The live source projectile-to-limb injury route also remains open. Continue at the real room/objective path, then assess whether a complete original projectile limb hit can be executed without substituting gameplay. Stop before Gate 7/8 per owner direction.
