# Current handoff — original actor prefab loading

**Observed branch:** `feature/full-source-characterbody-integration`  
**Current head:** `cff49da6172bffeaf2cec781da3f59669abf4d`  
**PR:** [#42](https://github.com/ethan-bailey/crisis-response-port/pull/42), open draft, base `develop/wasm`, mergeable=false.  
**Live base:** `develop/wasm` at `2fc0a8e93be97c4d99123910fad4bd25798e71a8`.

## Latest validated checkpoint

The latest executable source checkpoint is `5ed26d87ddb66eb6841f72cbfeed8ebcf3e6690d`. The current head adds only the regenerated dependency inventory.

- .NET SDK 10.0.401 built the net8.0 runtime at the executable checkpoint: `dotnet build runtime/Crisis.Runtime.csproj --no-incremental -v:q -p:GenerateDocumentationFile=false`, exit 0. No GitHub Actions run is attached to this SHA.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -- --prefab-only`: PASS. The fixture loads through `Resources.Load`, binds serialized GameObject/Transform/BoxCollider/Rigidbody/HingeJoint references, delays Awake until cloned references are bound, preserves hinge values, and verifies two independent cloned graphs. This uses a controlled fixture; it does not execute original CharacterBody or Limb lifecycle.
- `EngineChecks`: 60 assertions pass. `FoundationChecks`: 120 assertions pass.
- Actor migration boundary and strict CharacterBody ownership pass. Dependency inventory is fresh at 646 source files, 1,681 symbols, 1,319 reachable files and 116 ambiguous names. Original WeaponMode/LimbId identity checks pass. All eight audit-tool tests pass.
- The broader `runtime/Checks` invocation was attempted but stops in `WeaponContracts` with `AmmunitionLoader.Load: Availability`. Do not report that full suite as passing; investigate its package/asset input separately.

## Work completed

- Reviewed `morluto/rea` and recorded the bounded workflow in [`docs/REA_PARITY_EVIDENCE_WORKFLOW.md`](docs/REA_PARITY_EVIDENCE_WORKFLOW.md). REA can provide static managed-code and WASM module/interface evidence. It does not execute gameplay. No tracked Assembly-CSharp DLL or built WASM artifact is available to inspect yet.
- Added `SceneLoader.LoadPrefab` and routed original character `Resources.Load` calls through it. The loader resolves GUID-backed original MonoBehaviour classes, local object references, supported serialized fields and actor hierarchy order while keeping prefab assets outside the active scene.
- Added prefab component identities for MeshRenderer, MeshFilter, Rigidbody, HingeJoint, BoxCollider2D, BoxCollider, AudioSource, Animator, Light and CapsuleCollider. Serialized Rigidbody/hinge/collider state is retained where mapped. `Resources.Load` requires a complete graph; unsupported fields and external assets fail visibly instead of spawning a partial actor.
- Added focused source-prefab contracts and regenerated `docs/audit/source-dependencies.json` in commit `cff49da`.

## Next acceptance gate

Load the actual preserved Operator and Occupant prefab records through the generated execution package. Report and close each missing GUID/type, serialized built-in field, mesh/material/audio/animator asset reference, and prefab dependency. Then prove one complete original actor graph before wiring spawning: original CharacterBody and Limb references, body/limb/physiology ownership, and lifecycle after Mission, Masks, audio and presentation services are bound. The current synthetic clone test is not that proof.

After the actual actor load passes, continue the ordered closure in `docs/REPOSITORY_REMEDIATION_2026-10-18.md`: shared original actor ownership with PRs #56/#57, complete spawn/equipment/movement/hit/physiology/destruction tests for two actors, then one consolidated WASM build and Chromium/WebKit gameplay checks. Keep the #56 projectile and #57 IK workstreams separate during migration.

Other open lanes observed: #7 `ff2eed647e4eeb3446453b304bb4cbc4b6d31349`; #56 `fb96e92a848409014b85f6e094d995a9376fb731`; #57 `2b73044c107b875d8545d03541bf6b2d35f64654`. No merge, preview or deployment change was made. Do not claim WASM, browser boot, original gameplay, or iPhone parity from the current compile and fixture checks.
