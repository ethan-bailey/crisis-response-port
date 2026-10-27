SUPERSEDED NUMBERING — see docs/GATE_DEPENDENCY_INDEX.md for current gate status.

# Gate 8 source fire discovery — 2026-10-23

## Checkpoint

- Branch: `develop/wasm`
- Tested source commit: `15d5a0b3a9c24a23b2737596dbb4621c3503e711`
- Base commit: `ba5b51b473eecaa07e22cbc31543972a50262e9e`
- Package: `/tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json`
- SDK: .NET 8.0.426 with `/tmp/libprocshim.so`

## Landed source fixes

- `Resources.Load("Weapons/Ammunition/…")` now loads the authored ammunition prefab and its serialized source component graph through `SceneLoader`. The previous field-only loader attached the transport `AmmunitionBase`, so the original `FullSource.AmmunitionBase.GetAmmunition` returned `null` and the original weapon was not ready to fire.
- The ammunition resource contract now checks the original `FullSource.AmmunitionBase`, source Calibre/projectile references, cached lookup, inactive asset lifetime, and independent clone fields.
- `PrefabCloner` no longer copies runtime delegate fields into prefab instances. The source muzzle-flash prefab clone previously failed on a copied `Rigidbody.PhysicsForceSink` delegate. The prefab contract covers this.
- `RuntimeWorld` has optional force and torque sinks for source-created rigidbodies without per-instance PhysX connections. The Mission test records force requests; it does not integrate native physics.
- Added `--mission-ai-projectile-contract` to inspect original Mission 01 actors, resolve the weapon/ammunition, and attempt the original trigger/update path.

## Verification

- `dotnet build runtime/Crisis.Runtime.csproj --no-restore --no-incremental -v:q`: passed, 0 errors, 48 existing warnings.
- `--ammunition-resource-contract … 5.56-FMJ`: passed. The original FMJ resolves with `Caliber 5.56x45mm` and `Projectile 5.56mm FMJ`; resource cache and clone checks passed.
- `--mission-ai-projectile-contract …`: Mission 01 started with original Operators and Occupants. FullSource masks were `Limbs=16384` and `BallisticTargets=201350400`; the source mask included Occupant limb layer 14. Original M4A1 was owned by the Operator, selected source ammunition and projectile, and was ready. A torso collider ray query succeeded.
- The original trigger path incremented `ShotsFired` to 1. Headless adapters recorded one particle emission and two casing force/torque requests. The projectile contract then observed zero live owner projectiles, zero Occupant limb hits, and no health/body-state change. This is **not** a Gate 8 pass.
- The standard `Crisis.Runtime.Checks` package run passed its initial runtime, prefab, arm-sequence, presentation, weapon, and ballistic contracts, then stopped at the existing `Program.cs:328` check with `Exception: Unresolved supported fields` while importing the Marksman range scene.
- `git diff --check`: passed.

## Current Gate 8 blockers

1. After the source shot is registered, no live source projectile remains observable in the Mission world. The live projectile-to-limb injury callback and body-state chain therefore remain unverified.
2. Native PhysX force/torque integration and particle simulation/rendering are not present in this headless contract.
3. Exact-source WASM/browser checks and iPhone Safari acceptance have not run.
4. The broader package-driven checks still stop on unresolved supported fields in the Marksman range scene.

## Errors encountered and dispositions

- `CS1061 Mission.scene`: fixed the diagnostic to use the original Mission GameObject name.
- Mission helper namespace/type ambiguity and global-vs-FullSource ammunition type mismatch: fixed diagnostic references with explicit FullSource types.
- `CS1061 Physiology.Unconscious` / `.Dead`: corrected the diagnostic to source members `Conscious` and `Alive`.
- `NotSupportedException` for original particle emission: test fixture now records emission requests without claiming particle simulation/rendering.
- `PrefabCloner` rejected a copied runtime `Action<…>` delegate: clone logic now excludes delegate fields, matching Unity serialization behavior.
- Missing per-instance casing force and torque sinks: source-created rigidbodies can use the RuntimeWorld-level physics bridge. The test records requests only.
- `Exception: Unresolved supported fields` in the package-driven Marksman range import remains open and is separate from the Mission ammunition/trigger work.

The next actionable step is to trace why the original projectile does not remain active after the registered shot, then verify the original projectile-to-Occupant limb callback before testing injury and body-state outcomes. Do not claim Gate 8 or publish until the exact-source WASM/browser and iPhone checks also pass.
