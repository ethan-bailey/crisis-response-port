SUPERSEDED NUMBERING — see docs/GATE_DEPENDENCY_INDEX.md for current gate status.

# Gate 7 — Mission and NPC flow acceptance — 2026-10-23

## Result

**Gate 7 passed for original Mission 01 startup and in-mission NPC system
execution in the headless RuntimeWorld harness.** The contract loaded the
recovered mission and original Global Context, spawned original Operator and
Occupant actors, ran the original Behavior Designer trees, exercised the
source mission room-clear and Occupant-disable paths, and reached the authored
mission-success menu.

This result does not claim native PhysX movement, autonomous navigation,
perception/combat success, full presentation, decoded audio, or browser/iPhone
acceptance. Those remain separate gates or platform dependencies.

## Tested state

- Branch: `develop/wasm`.
- Runtime source SHA: `90f9f67ceb8fb6c01ed37742e346196c07c5e395`.
- Documentation checkpoint at run: `937300033c6ad585dfd71107ddb5564c78bcb78e`.
- SDK: .NET 8.0.426, pinned `global.json`; runner shim `/tmp/libprocshim.so`.
- Scene package: `/tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json`.
- No Codespaces or GitHub Actions used.

The source SHA is unchanged from the tested Gate 2–6 source commit; the
intervening commits only updated evidence and handoff documentation.

## Verification

Commands run from the repository root:

```sh
DOTNET_ROOT=/tmp/codex-dotnet-8.0.426 PATH="/tmp/codex-dotnet-8.0.426:$PATH" LD_PRELOAD=/tmp/libprocshim.so \
  dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- \
  --mission-ai-outcome-contract /tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json

DOTNET_ROOT=/tmp/codex-dotnet-8.0.426 PATH="/tmp/codex-dotnet-8.0.426:$PATH" LD_PRELOAD=/tmp/libprocshim.so \
  dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- \
  --mission-ai-contract /tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json
```

Both commands completed successfully. The checks project invocation rebuilt
the full runtime dependency with the pinned SDK; there were no compile errors
(the existing warning set remains).

Mission AI contract output:

- Mission 01 started through original `Mission.Start` / `BeginMission`.
- Original mission objects: 1,526; original components: 1,054.
- Actors: 2 Operators, 5 Occupants, 8 CharacterBody instances.
- Six active original behavior trees had deserialized `ConditionalEvaluator`
  roots. They remained `Running` for 60 simulation ticks (one simulated
  second).
- The original force-command sink recorded 240 commands. This measures source
  force requests; the harness does not integrate them with native PhysX.

Mission outcome contract output:

- Seven active rooms cleared through original Operator room query,
  `TeamController.UpdateMissionState` / `Room.Clear`, and
  `Mission.UpdateMissionState`.
- All five spawned Occupants entered the original ragdoll path; the
  all-disabled event fired once and `AllRenderedSafe` became true.
- Original `Mission.EndMission` computed success and the authored end menu was
  visible. Original MissionUI initialized its PauseMenu; the interior camera's
  depth-of-field component was found.
- The original behavior trees remained enabled through the outcome path.

The diagnostic `globalContextInstantiated` field was `false`: this compares
`Global.Context.gameObject` with `SceneLoadResult.prefabRoot`, which is the
same active loaded original Global Context root in this fixture. The mission
still initialized, spawned the original actors, and completed the above source
paths; the field does not indicate a failed scene startup.

## Scope and remaining limits

The headless adapter suppresses selected presentation callbacks and uses
silent audio. The mission import reports unsupported presentation/physics
components and unresolved built-in camera, mesh, shader, and resource fields.
The outcome contract positions an original Operator at each room and disables
Occupants through source APIs; it does not demonstrate autonomous traversal or
combat. The AI trees remained `Running`, and force commands were recorded
without a physics solve. Native navigation/perception/combat, the live
projectile-to-limb chain, exact-source WASM browser play, and physical iPhone
Safari remain unverified.

## Errors

- Compile errors: none.
- Runtime contract failures: none in these two runs.
- Existing compiler warnings were emitted during the checks project build;
  they did not prevent either contract from passing.
