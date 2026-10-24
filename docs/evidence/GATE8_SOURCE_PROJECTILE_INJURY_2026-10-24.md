# Gate 8 source projectile and injury trace — 2026-10-24

## Result

The Mission 01 headless source-runtime contract now proves that the original Operator M4 trigger and projectile path reaches an Occupant limb and updates original injury physiology.

Observed on a passing run:

- `CharacterBody.PullTrigger` and the recovered `WeaponBase` path fired one round (`ShotsFired` 0 → 1).
- The `onSpawnProjectile` event supplied a registered, active `Crisis.Runtime.FullSource.Projectile` owned by the Operator.
- The source projectile collided with the original Occupant 2 `Abdomen` capsule and the original source code consumed it on the hit.
- Original physiology changed from 0 to 1 wound, wound cavity/tissue volume from 0 to 3.9282825, and body shock from 0 to 9.895712. The Occupant remained conscious and alive in this single-shot test.
- The Mission loads 19 limbs per Occupant; 16 have readable skin textures and renderers.

## Test fixture limits

The serialized Operator spawn sits outside the authored roof edge; shots along the Occupant line hit the roof. To test the source hit path, the contract stages the initialized Operator at x=2, sets the source kneeling state, supplies the selected Occupant's abdomen aim point through the original CharacterBody aim field, and waits for the source arm pose update. This staging is test-only.

The headless runtime has no GL rasterizer. The contract disables wound decals on source limbs for this mission test instance so the original tissue and physiology calls can run without claiming that wound pixels were rendered. Particle/audio adapters record requests and do not render or play audio. Physics force sinks record requests and do not establish native PhysX parity.

## Diagnostic correction

The prior probe queried `world.Objects.OfType<Projectile>()`, which resolved to the separate transport/global projectile class instead of `Crisis.Runtime.FullSource.Projectile`. Its zero count was a type-query error, not proof that the original projectile failed to spawn. The contract now uses explicit source-type aliases and observes the original projectile through `WeaponBase.onSpawnProjectile`, including its registration, ownership, status, position, energy, and last collision.

An un-staged original shot also correctly hit a roof collider with no source `BallisticMaterial`, after which the projectile was destroyed. The headless contract stages the actor under the roof to exercise the Occupant limb branch.

## Verification

Command (using the pinned SDK 8.0.426 and recovered original scene package):

```sh
dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- --mission-ai-projectile-contract /tmp/cpr-original/scene-data/crisis-response-scenes.json
```

Result: passed with `source-projectile-hit-occupant`; abdomen wound/tissue/shock values increased as listed above.

## Remaining Gate 8 work

- Exercise the full injury path with a real GL/texture-transfer backend and verify wound pixels.
- Verify particle simulation/rendering and native physics integration.
- Continue the body-state outcome and combined actor flow checks.
- Run the exact-source WASM/browser and iPhone Safari acceptance.

Gate 8 remains open until those acceptance checks pass.

## Resolved diagnostics during this trace

- Ambiguous `Projectile` type query selected the wrong runtime type; fixed with explicit `FullSource` aliases.
- `victim.Position.x` compile error: `Position` is a scalar in this API; fixed by reading `victim.transform.position.x`.
- Anonymous object conditional needed an explicit `damageBefore=` property name; fixed.
- The first headless limb hit failed at `Graphics.Blit` because no texture-transfer driver is bound. The test now disables wound decals only in its source mission fixture and continues through the original tissue/physiology path. Visual damage remains unverified.
