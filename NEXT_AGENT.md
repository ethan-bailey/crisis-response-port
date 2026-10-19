# CURRENT HANDOFF — Crisis Response Port

**Prepared:** 2026-10-19 UTC. **Workstream:** original source actor integration, PR #42. **Repository:** private `ethan-bailey/crisis-response-port`.

## Live state and validation

- `develop/wasm`: `2fc0a8e93be97c4d99123910fad4bd25798e71a8`.
- PR #42 / `feature/full-source-characterbody-integration`: `6625789d7ede07e3be414e0bc7da4ee3df74b090` (draft, open; recorded base SHA `5d83c4ef627baa6dc9009ef9a585c0b70e0d9435`).
- Other open lanes: #7 `ff2eed647e4eeb3446453b304bb4cbc4b6d31349`, #56 `fb96e92a848409014b85f6e094d995a9376fb731`, #57 `2b73044c107b875d8545d03541bf6b2d35f64654`. Keep these ownership boundaries separate.
- The latest source checkpoint is commit `af56508d832125ce655fa705d62fda23795c1492`; `6625789` only refreshed the roadmap. The source batch changes 312 paths with zero removals, and exact blobs match preserved per-file hashes.
- Narrow source/provenance checks recorded for the checkpoint pass: 646-source inventory freshness, 477 source identities, actor migration and strict ownership scans, eight audit tests, original prefab integrity, and whitespace checks. **No full compile or Actions run is attached to the source checkpoint or current head.**
- The previous full compile at `517ce292cf2c668a9555cfa342e8d382e02b70d8` failed with 19 error occurrences / 15 missing names on a different graph, before the 312-path checkpoint. The older 144-error run `39091701268` is another graph. Neither count describes the current checkpoint; do not compare them.
- The full compile workflow is manual-only. This environment has no .NET SDK and the GitHub connector has no workflow-dispatch operation. No passing compile, browser boot or gameplay parity is claimed.

## Immediate next gate

Run the deliberate full .NET 8 compile on the current source checkpoint and preserve its exact exit code and diagnostics. Then close the resulting original dependency graph without stubs. The current runtime gap is clear: `runtime/Engine/WeaponHost.cs` constructs transport `Crisis.Runtime.Weapons.CharacterBody`; there is no runtime loader for the recovered Operator/Occupant prefab component graph and no two-actor lifecycle test.

After compilation, implement and test source prefab component/reference loading and original lifecycle on shared actor-owned components. Prove two independent actors through spawn, equipment, movement, hit callbacks, injury/physiology and destruction before expanding NPC gameplay or publishing a new preview.

## Preservation

Keep PR #42 separate from #56 projectile injury, #57 IK callbacks and #7 historical Unity/mobile work. Do not merge, deploy or alter the stable preview until exact-source compile, native/WASM/browser and required iPhone Safari gates pass. Preserve original serialized prefab/source provenance and the approved analog iPhone movement controls.
