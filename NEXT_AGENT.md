# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; current integration is committed and pushed as recorded by the latest Git history.
- Gate 1 acceptance evidence remains at `ba3196d5b1522007bf208d6f40b73fe6e344363d`; see `docs/evidence/GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`.
- Gate 2–6 status and current blockers: `docs/evidence/GATES_2_TO_6_PROGRESS_2026-10-23.md`.
- Preserve untracked `scene-data/.crisis-response-scenes.json.gz.QEOasz`; it is excluded from commits.
- Keep GitHub Actions disabled. Codespaces usage is 90% through 2026-11-01; avoid Codespaces.
- Read `AGENT_INSTRUCTIONS.md` and `docs/DOTNET_BUILD_RUNBOOK.md` before .NET work. Use the pinned SDK 8.0.426 and the documented process shim in this environment.

## Current gate state

- **Gate 1 passed**: full source CharacterBody compile, original Operator/Occupant prefab lifecycle, 19 limb identities, and independent actor source contracts.
- **Gate 2 partial**: original M4A1/Glock-18 ownership and source IK holster/activate/fire callbacks pass. Full authored arm sequencer, reload, interruption and live projectile firing remain open.
- **Gate 3 partial**: source physiology, movement/posture, unconsciousness, ragdoll command dispatch and timed hypoxic death pass. Live projectile-to-limb injury remains open.
- **Gate 4 partial**: original Mission 01 starts and source actors spawn from the recovered full scene package. Headless test suppresses AI/presentation and does not validate unsupported serialized scene fields or native physics/render/audio.
- **Gate 5 blocked**: original trees reach an authored `ConditionalEvaluator` with a null `conditionalTask` during `BehaviorManager` tick. Resolve original task graph embedded-reference deserialization; do not add substitute AI.
- **Gate 6 partial/blocked**: source mission computes success after rooms are marked checked and all four spawned Occupants are incapacitated/ragdolled. Full room interaction/player/AI path and actual UI transition are unverified; original `Mission.EndMission` throws `NullReferenceException` at uninitialized headless `MissionUI.PauseMenu`.
- Stop at Gate 6 per user direction. Gates 7 and 8 have not been started.

## Verified commands in this checkpoint

- Full runtime rebuild: 0 errors, 48 warnings.
- General `runtime/Checks`: passed.
- `--spawner-contract`: original M4A1/Glock-18 ownership, original IK callbacks, aim, movement and physiology outcomes passed.
- `--mission-outcome-contract`: mission startup, 4 source Occupants, all-rendered-safe event, ragdoll states and source-computed mission success passed; UI transition reports the blocker above.
- `runtime/EngineChecks`: 69 assertions passed.
- `runtime/FoundationChecks`: 121 assertions passed after its compile glob excludes the newly recovered CharacterBehavior task sources.
- `scripts/verify_characterbehavior_sources.py --archive /workspace/scratch/original-recovery-artifact.zip`: all 265 source/metadata records matched the pinned archive SHA `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
- The broader pre-existing `verify_foundation_sources.py --archive` has stale provenance data outside the recovered AI sources and is not a passing check; see the Gate 2–6 evidence report.

## Next work

1. Inspect serialized Behavior Designer task bytes and fix task graph reconstruction so each authored embedded condition reference resolves; add a focused regression fixture before re-enabling AI in Mission 01.
2. Complete the authored IK sequence and reload/weapon action path.
3. Close source projectile collision through the correct limb and full injury chain.
4. Initialize original MissionUI dependencies in the host and validate real objective/room interactions and end transition with AI enabled.
5. Re-run the Gate 6 acceptance before any Gate 7/8 work.

## Constraints

- Keep original source bodies, GUIDs and prefab assets intact. Record every required adapter and source archive identity.
- Use `docs/DOTNET_BUILD_RUNBOOK.md` for runtime setup and common failures; document newly discovered failure-and-fix pairs there.
- Never use Codespaces through 2026-11-01 unless the user changes that instruction. No Actions runs, public release artifacts, or presentation/device work until Gate 6 passes.
- Replace this whole handoff on each transfer; do not append dated progress to it.
