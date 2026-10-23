# Crisis Response Port — handoff

## Checkpoint

- Branch: `develop/wasm`.
- Verified source commit: `90f9f67ceb8fb6c01ed37742e346196c07c5e395` (`Exercise original operator room clear outcome path`); pushed to `origin/develop/wasm`.
- This handoff update is docs-only and follows that tested source commit.
- Gate 1 evidence: `docs/evidence/GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`.
- Gates 2–6 evidence: `docs/evidence/GATES_2_TO_6_PROGRESS_2026-10-23.md`.
- Before .NET work read `AGENT_INSTRUCTIONS.md` and `docs/DOTNET_BUILD_RUNBOOK.md`.
- Preserve any excluded untracked scene package cache; never add it to Git.
- Avoid Codespaces through 2026-11-01; GitHub Actions remain disabled.

## Gate state

- **Gate 1 passed.** Original CharacterBody compile, Operator/Occupant prefab lifecycle, 19 limb identities, and actor ownership checks are documented.
- **Gate 2 partial.** Original authored IK sequences and source reload/bolt-lock contracts pass. Browser composition and live projectile firing remain unverified.
- **Gate 3 partial.** Source physiology, movement/posture, unconsciousness, ragdoll command dispatch, timed hypoxic death, and generic material ballistics pass. The live original projectile-to-limb injury path remains unverified.
- **Gate 4 partial.** Mission 01 starts and spawns original Operators and Occupants. Native physics/render/audio and unsupported scene serialization remain.
- **Gate 5 partial.** Operator and Occupant Behavior Designer graphs load with 91 and 66 tasks, zero unknown nodes, and resolved embedded conditions. Four original mission trees ran for one simulated second and remained `Running`; 153 source force commands were observed. Native navigation, perception/combat outcomes, and physics remain unverified.
- **Gate 6 partial.** The headless outcome contract relocates an initialized original Operator across seven active rooms and calls original `CharacterBody.UpdateMovement`, `TeamController.UpdateMissionState`/`Room.Clear`, and `Mission.UpdateMissionState`. Original Occupant ragdoll/event, mission success, and visible authored end menu pass. This does not prove input-driven movement or autonomous navigation.
- Stop at Gate 6 per owner direction. Do not begin Gate 7/8 work.

## Verification at source commit `90f9f67`

Pinned SDK .NET 8.0.426 with `/tmp/libprocshim.so`; no Codespaces or GitHub Actions.

- Full runtime build: passed, 0 errors / 48 warnings.
- Checks project build after final room-path edit: passed, 0 errors.
- Default `Crisis.Runtime.Checks`: passed, including original arm sequences, reload/bolt lock, and material ballistics.
- `--behavior-graph-contract`: passed; Operator 91 tasks / Occupant 66 tasks, zero unknown tasks, both embedded conditions resolved.
- `--mission-ai-contract`: passed; Mission 01, 2 Operators, 3 Occupants, 4 active trees, 60 AI ticks, 153 force commands.
- `--mission-ai-outcome-contract`: passed; seven rooms cleared through the source Operator path, all spawned Occupants ragdolled, all-disabled event fired once, mission success computed, original PauseMenu initialized, camera dependency found, and authored end menu visible with AI enabled.
- EngineChecks: 69 assertions passed; FoundationChecks: 121 assertions passed.
- `check_characterbody_ownership.py --strict`, `verify_full_characterbody_enums.py`, and `verify_characterbehavior_sources.py --archive /workspace/scratch/original-recovery-artifact.zip`: passed; all 266 AI source/metadata records match the pinned archive.
- `git diff --check`: passed.
- `verify_foundation_sources.py --archive` remains blocked by stale unrelated provenance; do not report it as passing.

## Errors found and resolved

- Headless AI run initially threw from `LimbAudio.Update` because runtime-created AudioSources lacked a driver. Bind the silent test driver to every source each tick; decoded/audio-device playback remains unverified.
- FoundationChecks initially failed with `CS0246 SharedGameObject` from restored `RestartBehaviorTree.cs`. Exclude that gameplay task from the focused FoundationChecks compile glob; retain it in the full runtime. FoundationChecks passes 121 assertions.
- MissionEndMenu was inactive because MissionUI's parent was deactivated. Reactivate the original MissionUI before Awake and assert the authored end menu is visible.
- First room-path fixture run selected a TeamOperator whose `Body` reference was uninitialized. Select an original TeamOperator with a bound CharacterBody; all seven source room clears then passed.
- Initial `git diff --check` found trailing spaces in the report header. Removed; final check passes.

## Next action

Try one focused contract that uses the original input/controller path to move the Operator between rooms. The current headless room test relocates the source body directly and invokes original update methods, so it is not evidence of player input or autonomous movement. If that path is blocked by unsupported input/physics services, record the exact dependency and continue with a bounded inspection of the live projectile-to-limb injury route. Keep Gate 6 partial and stop before Gate 7/8.
