# Crisis Response Port — current handoff

## Checkpoint

- Development branch `develop/wasm` is at `72c167993280934d19c7921113eb9efe80849f89` (`Complete Mission 01 actor startup contract`). The live GitHub branch was verified at the same SHA after push; no open PRs were listed.
- The exact tested code checkpoint is `72c167993280934d19c7921113eb9efe80849f89`. A documentation-only handoff update may follow it.
- GitHub Actions remain disabled. Codespaces use is at 90% through 2026-11-01; avoid it unless the owner requests it or a required gate cannot run elsewhere.
- Read `AGENT_INSTRUCTIONS.md` and `docs/DOTNET_BUILD_RUNBOOK.md` before project work. The runbook now records the recurring .NET runner failures and Gate 7 gameplay/runtime fixes.

## Gate 7 result

Mission 01 reached `Mission.Start` and `BeginMission` with original source actors: 2 TeamOperators and 5 Occupants (8 CharacterBody instances). Blackboard property mappings resolved after the short-name type lookup fix. Gate 7 passed using the headless test adapters documented in the runbook.

The test silences audio and mixer output and suppresses scene-level MissionUI, interior canvas callbacks, FogOfWarElement, and vocalization presentation. It checks real mission and actor startup; it does not claim visual/audio parity, full AI behavior-tree execution, browser/WASM, or iPhone Safari acceptance.

## Verified checks at the exact tested code checkpoint

- `runtime/Checks --mission-contract`: passed; Mission 01 started, 2 Operators, 5 Occupants, no Blackboard property-mapping errors.
- `runtime/Checks --spawner-contract`: passed; original Occupant spawning, independent trauma/physiology ownership, movement force, and animation parameter contracts.
- `scripts/package_ammunition_closure.py game/assets/weapon-runtime.json.gz <full-package> --check`: passed; 29/29 5.56 FMJ closure assets resolved, no unresolved assets or stale script/builtin maps.
- `python3 -m unittest scripts.extract_runtime_scene_test`: 6 passed.
- `runtime/EngineChecks`: 69 assertions passed.
- `runtime/FoundationChecks`: 121 assertions passed.
- Full `runtime/Crisis.Runtime.csproj` rebuild with SDK 8.0.426 and `/tmp/libprocshim.so`: 0 errors, 45 warnings.
- `git diff --check`: passed before commit.

## Errors found and fixed in this turn

- Missing original `OperatorSpawner` source prevented Operators from appearing; restored it into the compiled source set.
- Prefab clones did not run `Awake` before the spawning callback consumed physiology; active clones now complete `Awake`/`OnEnable` before `Instantiate` returns.
- Original zero-scale UI roots and sheared arm-bone transforms failed `SetParent`; runtime transform handling now preserves local pose for singular parents and extracts closest TRS for shear.
- Missing Unity built-in class ID 120 left the original weapon reticle without its `LineRenderer`; prefab loading now supplies it.
- Behavior Designer trees omitted optional task/node metadata and stored component mappings by short type name; deserialization now uses safe metadata defaults and resolves short names across loaded assemblies.
- EngineChecks expected `Renderer.material` to return the shared source object; its assertion now reflects the Unity per-renderer copy while checking the remaining material identities.

## Remaining issues / next step

- Gate 8 combined acceptance has not run. Start it only after confirming the requested scope, then verify the complete two-actor flow, exact-source WASM/browser behavior, and iPhone Safari acceptance.
- The original Marksman `.308` round prefab still references a missing projectile mesh GUID. The already-approved `.308` casing-only mapping is not authorization to substitute that projectile mesh.
- Mission import still reports unsupported presentation/physics records and unresolved external assets in the incomplete package. Review the diagnostic inventory before claiming full asset closure.
- One known untracked corrupt package fragment remains at `scene-data/.crisis-response-scenes.json.gz.QEOasz`; it was excluded from the commit.

## Next action

Prepare Gate 8’s combined two-actor acceptance using the exact tested source checkpoint. Resolve the original Marksman projectile dependency from source provenance without substituting the separately missing mesh, then run the exact-source WASM/browser checks and request iPhone Safari acceptance only when the browser gate passes.
