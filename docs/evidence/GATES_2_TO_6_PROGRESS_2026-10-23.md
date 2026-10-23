# Gates 2–6 integration progress — 2026-10-23

Branch: `develop/wasm`
Recovered source archive: `/workspace/scratch/original-recovery-artifact.zip`
Pinned archive SHA-256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`

This checkpoint reaches through Gate 6 and records the gates that remain blocked. It does not claim complete gameplay parity.

## Gate results

| Gate | Result | Evidence and remaining work |
|---|---|---|
| 1. Complete source character | Passed at prior checkpoint | See `GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`. The full original CharacterBody build, two prefab lifecycles, 19 limb IDs, and independent actor state passed. |
| 2. Weapons and arm IK | Partial | The original M4A1/Glock-18 resources load as Operator-owned primary/secondary equipment. Source IK callbacks holster, activate pending weapons, and call the body trigger; original aim alignment is exercised. The complete animation sequencer, reload, authored event interruptions, and real projectile firing remain unverified. |
| 3. Projectile injury and body outcomes | Partial | Original actor-owned physiology, movement/posture, unconsciousness, ragdoll command dispatch, and timed hypoxic death pass focused contracts. A live source projectile collision through the complete limb injury chain is still unverified. |
| 4. Mission startup and spawning | Partial | Original Mission 01 starts in `RuntimeWorld` and automatically spawns source Operators and Occupants from the recovered full scene package. The headless acceptance suppresses BehaviorTrees and presentation callbacks; unresolved serialized fields and native physics/render/audio integration remain. |
| 5. Original NPC AI | Blocked | Original authored Behavior Designer task sources and dependencies now compile. When the original prefab trees are enabled, an authored `ConditionalEvaluator` reaches `OnUpdate` with `conditionalTask == null`, causing a `NullReferenceException` during `BehaviorManager` tick. AI was deliberately suppressed in the mission contracts; no substitute AI was introduced. Resolve deserialization of the embedded conditional-task reference before claiming Gate 5. |
| 6. Complete mission and outcome | Partial, blocked at transition | The mission outcome contract marks original rooms checked, incapacitates all four spawned Occupants through original source methods, observes the all-disabled event, verifies all are ragdolled, and calls original `Mission.EndMission`. The original mission computes `missionSuccess=true`. Its UI transition then throws `NullReferenceException` because headless `MissionUI.PauseMenu` was not initialized. The contract does not exercise real room interaction or a complete objective/player/AI flow, so Gate 6 is not passed. |

## Verification

All .NET commands used the pinned local .NET SDK 8.0.426 and process shim, without Codespaces:

```sh
LD_PRELOAD=/tmp/libprocshim.so DOTNET_ROOT=/tmp/codex-dotnet-8.0.426 \
  /tmp/codex-dotnet-8.0.426/dotnet build runtime/Checks/Crisis.Runtime.Checks.csproj \
  --no-restore --no-incremental -v:q -p:GenerateDocumentationFile=false

LD_PRELOAD=/tmp/libprocshim.so DOTNET_ROOT=/tmp/codex-dotnet-8.0.426 \
  /tmp/codex-dotnet-8.0.426/dotnet runtime/Checks/bin/Debug/net8.0/Crisis.Runtime.Checks.dll \
  --spawner-contract /tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json

LD_PRELOAD=/tmp/libprocshim.so DOTNET_ROOT=/tmp/codex-dotnet-8.0.426 \
  /tmp/codex-dotnet-8.0.426/dotnet runtime/Checks/bin/Debug/net8.0/Crisis.Runtime.Checks.dll \
  --mission-outcome-contract /tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json

LD_PRELOAD=/tmp/libprocshim.so DOTNET_ROOT=/tmp/codex-dotnet-8.0.426 \
  /tmp/codex-dotnet-8.0.426/dotnet runtime/Checks/bin/Debug/net8.0/Crisis.Runtime.Checks.dll

LD_PRELOAD=/tmp/libprocshim.so DOTNET_ROOT=/tmp/codex-dotnet-8.0.426 \
  /tmp/codex-dotnet-8.0.426/dotnet run --project runtime/EngineChecks/Crisis.Runtime.EngineChecks.csproj --no-restore

LD_PRELOAD=/tmp/libprocshim.so DOTNET_ROOT=/tmp/codex-dotnet-8.0.426 \
  /tmp/codex-dotnet-8.0.426/dotnet run --project runtime/FoundationChecks/Crisis.Runtime.FoundationChecks.csproj --no-restore

python3 scripts/verify_characterbehavior_sources.py --archive /workspace/scratch/original-recovery-artifact.zip
```

Results: full runtime compile 0 errors / 48 warnings; focused M4A1/Glock-18 and original source callback contract passed; mission outcome printed `missionStarted=true`, `missionSuccess=true`, `allRenderedSafe=true`, `allDisabledEventCount=1`, `allRoomsChecked=true`, 4 Occupants ragdolled; general Checks passed; EngineChecks passed 69 assertions; FoundationChecks passed 121 assertions; all 265 restored CharacterBehavior source and metadata hashes matched the pinned archive.

The FoundationChecks compile boundary was updated to exclude the newly restored CharacterBehavior sources. Those task sources require the complete original `FullSource` and arm-IK dependency graph and do not belong in the focused FoundationChecks assembly.

## Errors encountered

- Initial original AI compile produced 251 errors: ambiguous `Action`/`TaskStatus` references, six missing typed shared variables, and missing `StaticBehavior`, `DoorReference`, Behavior Designer core tasks, plus `Collider.tag`. Added explicit aliases and original missing source/dependencies; the full runtime now compiles.
- One transient duplicate-task compile reported duplicate Behavior Designer tasks after overlapping recovery; duplicate copies were removed, leaving the authored source identity in one runtime location.
- FoundationChecks initially picked up recovered AI task sources through a broad compile glob and failed because `ArmIKSolver` was outside that focused project. Excluding `Original/CharacterBehavior/**/*.cs` restored the intended test boundary; 121 assertions now pass.
- Original AI tree execution still fails on a null embedded conditional task reference. This is not bypassed in the source or counted as a pass.
- Mission outcome succeeds in source state computation, but UI transition cannot finish in this headless host because `MissionUI.PauseMenu` is uninitialized. This remains a Gate 6 blocker.
- The pre-existing `verify_foundation_sources.py --archive` audit has stale source-provenance records outside this restored AI source set (including a `Serializer.cs` runtime adapter). Its audit is not used as a pass claim here; the new CharacterBehavior-specific archive verifier passes all 265 recovered files.

## Stop point

Stop at Gate 6 as requested. Do not begin presentation/device Gate 7/8 checks until the Gate 5 tree-reference failure, full live injury path, mission interaction path, and complete UI transition are resolved and covered by native/browser acceptance.
