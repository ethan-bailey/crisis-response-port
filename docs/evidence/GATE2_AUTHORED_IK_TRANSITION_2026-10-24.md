# Gate 2 authored IK transition evidence — 2026-10-24

## Result

The recovered Operator prefab now completes an original `IKArmController.Unarmed`
sequence on its actor-owned right and left `ArmIKSolver` instances. Both source
sequencers transition to the authored Pelvis target, both solvers reach
`OnTarget`, both IK targets remain parented to the Operator's Pelvis, and both
sequencers return to ready. The Occupant prefab is instantiated in the same
`RuntimeWorld` and remains independently owned.

This is real recovered-prefab/headless source execution. It is not normal-page
visual acceptance: the playable page still uses the narrower
`Crisis.Runtime.Weapons.CharacterBody` transport host for weapon presentation.

## Fix discovered by the source sequence

The scene loader cast Unity 2017 serialized curve infinity value `2` directly
to this runtime's public `WrapMode` enum. In this runtime that integer means
`Loop`, so the Operator's arm interpolation repeatedly wrapped below `1` and
`ArmIKSolver.UpdateTransition` could never set `OnTarget`.

`SceneLoader.ReadAnimationCurve` now preserves the exact serialized infinity
integers and maps the recovered game's `2/2` curves to clamped runtime
evaluation, consistent with the existing ammunition and weapon curve loaders.
`ScenePrefabContracts` verifies the mapping, out-of-range evaluation, and
independent cloned curves.

## Verification

- .NET SDK 8.0.426; tracked process-stat shim used.
- Full runtime/check build: 48 existing warnings, zero errors.
- General runtime contracts: pass, including source prefab, original arm
  sequence, presentation, weapon, and ballistics contracts.
- `--prefab-lifecycle` against the tracked recovered scene package: Operator
  134 active behaviours and Occupant 132 reached Awake/OnEnable/Start; the
  authored two-arm Pelvis transition completed.
- EngineChecks: 69 assertions passed.
- FoundationChecks: 121 assertions passed.
- Release runtime build and Blazor WebAssembly publish: passed.
- Exact-source package at `e035b9fe8dd40d44dadc0ddbcc5a5f6bb1ccc95b`:
  767 files, 199 hashed Blazor resources, and 195 valid WASM files; verified
  twice by the package validator.
- Related non-browser animation, source-step, aim/fire, arm-physics, and
  particle-binding tests: passed.
- Package and audit-tool unit suites: 27 tests passed.

The optional Playwright arm-sequence smoke did not run because Playwright is not
installed in this workspace. Desktop browser automation is optional under the
standing acceptance policy, so this is not a gate blocker. No deployment was
performed.

The first package invocation passed the outer `dotnet publish --output`
directory and failed because `runtime-web/index.html` is under its `wwwroot`
child. The runbook now records the corrected `--published <DIR>/wwwroot`
command; the fresh package output passed.

## Remaining boundary

The tracked `scene-data/crisis-response-scenes.json.gz` contains 1,504 assets
and both character prefabs, but intentionally omits the Global Context and M4A1
resource prefabs needed by the full spawner/equipment contract. Do not treat it
as the private gameplay-resource package or repeatedly try the same contract
with it. The next Gate 2 normal-page step is the explicit transport-to-FullSource
actor migration; if that cannot be completed without the verified private
package, continue with an unblocked Gate 3 or Gate 7 slice.
