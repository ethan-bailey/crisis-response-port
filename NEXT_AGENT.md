# Crisis Response Port — handoff

## Checkpoint

- Branch: `develop/wasm`
- Verified source commit: `3bf214e` (`Restore original Behavior Designer graph execution`); pushed to `origin/develop/wasm`.
- The handoff itself is a docs-only update that will follow the verified source commit.
- Gate 1 acceptance: `docs/evidence/GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`.
- Gates 2–6 current evidence: `docs/evidence/GATES_2_TO_6_PROGRESS_2026-10-23.md`.
- Read `AGENT_INSTRUCTIONS.md` and `docs/DOTNET_BUILD_RUNBOOK.md` before .NET work.
- Preserve the excluded untracked scene package cache if present. Do not add it to Git.
- Avoid Codespaces through 2026-11-01; GitHub Actions remain disabled.

## Gate state

- **Gate 1 passed.** Original CharacterBody build, Operator and Occupant prefab lifecycle, 19 limb identities, and actor ownership checks are documented.
- **Gate 2 partial.** Original authored IK sequences and source reload/bolt-lock contracts pass. Browser composition and live projectile firing are unverified.
- **Gate 3 partial.** Source physiology, movement/posture, unconsciousness, ragdoll command dispatch, and timed hypoxic death pass. Projectile-to-limb injury remains unimplemented/unverified.
- **Gate 4 partial.** Mission 01 starts and spawns original Operators and Occupants. Native physics/render/audio and unsupported scene serialization remain.
- **Gate 5 partial.** Both original Behavior Designer graphs load with every task and embedded condition resolved. Four mission trees ran for one simulated second and remained `Running`; 153 source force commands were recorded. Navigation/perception/combat outcomes and native physics remain unverified.
- **Gate 6 partial.** Original Occupant disable/ragdoll event and `Mission.EndMission` success/UI transition pass when the fixture sets rooms checked directly. Player/AI-driven room traversal and objective progression are still missing.
- Stop at Gate 6 per owner direction; do not start Gate 7/8 work.

## Verification at source commit `3bf214e`

Pinned .NET SDK 8.0.426 with `/tmp/libprocshim.so`; no Codespaces or GitHub Actions.

- Full `runtime/Crisis.Runtime.csproj` build: passed, 0 errors / 48 warnings.
- `runtime/Checks` build: passed, 0 errors.
- Default `Crisis.Runtime.Checks`: passed, including original arm sequences, reload/bolt lock, and ballistics contracts.
- `--behavior-graph-contract`: passed; Operator 91 tasks and Occupant 66 tasks, zero unknown nodes, both embedded conditions resolved.
- `--mission-ai-contract`: passed; Mission 01, 2 Operators, 3 Occupants, 4 active trees, 60 AI ticks, 153 force commands.
- `--mission-ai-outcome-contract`: passed; source Occupant event/ragdoll, source success computation, authored PauseMenu initialization, camera dependency, and visible authored end menu.
- EngineChecks: 69 assertions passed.
- FoundationChecks: 121 assertions passed.
- `check_characterbody_ownership.py --strict`, `verify_full_characterbody_enums.py`, and `verify_characterbehavior_sources.py --archive /workspace/scratch/original-recovery-artifact.zip`: passed; 266 AI source/metadata records match the pinned archive.
- `git diff --check`: passed after trimming two trailing Markdown spaces.
- Broader `verify_foundation_sources.py --archive` remains blocked by stale unrelated provenance records; do not call it passing.

## Errors found and resolved this session

- First AI run threw `NotSupportedException` because a runtime-created `LimbAudio` source lacked an audio driver. The headless adapter now binds each new AudioSource per tick; the bounded AI run passes. Audio decode/playback remains unverified.
- FoundationChecks compile failed with `CS0246 SharedGameObject` after the original `RestartBehaviorTree` source was restored. Excluded that gameplay task from the focused FoundationChecks compile glob while retaining it in the full runtime; 121 assertions pass.
- A strengthened UI check found MissionEndMenu inactive because its MissionUI parent was inactive. The contract now reactivates the original MissionUI root before Awake and asserts `MissionEndMenu.Visible`; it passes.
- First `git diff --check` found trailing spaces in the new report header. Removed them; the final check passes.

## Next action

Build a focused contract that advances an original Operator through a room using the original room-clear/objective update path, instead of setting `Room.cleared` in the fixture. Use the recovered Mission 01 package and original source actors. Then rerun the Gate 6 outcome contract. Keep the live projectile-to-limb injury path listed as the next blocker if it cannot be completed safely within the Gate 6 stop point.
