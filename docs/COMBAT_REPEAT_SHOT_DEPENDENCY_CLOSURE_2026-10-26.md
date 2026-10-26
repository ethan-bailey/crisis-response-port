# Repeat-shot and armor dependency closure — 2026-10-26

## Scope and source identity

This is the inventory gate before extending the Mission source projectile contract beyond one shot. The target is a controlled second original M4A1 shot against a second initialized Occupant, proving that one Operator and one loaded source weapon can complete two distinct source projectile→limb→injury lifecycles. It does not change gameplay tuning or synthesize assets.

- Recovery export SHA-256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
- Exact-source full index for run #58 / SHA `17194f02ee8d6cf94d27477a595ff76be00737f1`: `9d866276582031d440c316c8e60d69d252506cf67a8ae9d03dbdbe0d91980f92`.
- Mission profile: six source roots, 1,919 assets / 22,365 records; zero unresolved refs in the bounded profile. Its binary/manifest identity and package hashes are in `NEXT_AGENT.md`.
- M4A1 5.56-FMJ ammunition graph: 29/29 packaged assets, 33 GUID-to-path mappings, zero comparison errors; five source scripts plus a builtin are identified by `docs/evidence/REA-ammunition-closure-2026-10-20.md`. The pinned `game/assets/weapon-runtime.json.gz` identity is recorded there.

## Dependency set to close before editing

1. **Actor/weapon ownership:** hash-locked Mission 01, Operator prefab (GUID `2e00922aa35d9a8438c52fec69254605`, SHA-256 `94bcda5de74f45d83f4bbf88e80874b8f77bf0339d307294187b085c03170536`), one spawned Operator with active M4A1, initialized WeaponAimer/arms, original Blackboard target, and two full Occupant bodies. Occupant prefab (GUID `8363ad318a650364ebdaa8c237f65bfe`, SHA-256 `9094b840c49d6c53ffa077c274561ab5cbeedb202f85bdaade023ad67cb8c419`): 19 limbs, 17 LimbPhysiology, 9 VitalFunctions, 4 LimbArmor, source torso/abdomen colliders.
2. **Trigger and rate:** `CharacterBody.PullTrigger` sets the original pending trigger only when `RightArm.OnTarget`; `WeaponBase.UpdateTriggerMechanism` consumes it via `Owner.QueryTrigger`; source `StrikeFiringPin → DetonateRound → CyclingBolt.DetonateRound → SpawnProjectiles` increments source `ShotsFired`. Recovered `roundsPerMinute=600` implies a 0.1-second cycle; honor source cycle and pose updates between the shots.
3. **Magazine and reload:** source WeaponContracts already exercises all 30 magazine rounds plus the chamber (31 total), bolt lock-open, original ChangeMagazine task, 30-round replacement, no trigger/no spontaneous fire, and ordered source reload audio events `audioExtractMagazine → audioInsertMagazine → audioBoltReturn`. It does not prove audible playback. Do not duplicate that test by changing ammo values.
4. **Projectile and target path:** selected source ammo is 5.56-FMJ and its Projectile prefab/mesh/material/trace closure is the 29-asset graph above. Existing Mission projectile contract stages only test actors in a clear lane, retains original colliders/layers, binds the original target/aim solution, and observes `WeaponBase.onSpawnProjectile`, ownership, registration, ballistic masks, collision and cleanup. It has proven one source shot/hit; it has not yet proven a subsequent shot after hit.
5. **Injury accounting:** use source `Limb`, `LimbPhysiology`, `VitalFunctions`, `Physiology`, and original impact callbacks; compare the second Occupant's own pre/post wound, cavity, tissue and shock values. Do not require deterministic random damage values; require one new hit and positive source-state deltas. Disable only wound decals for headless testing and keep native physics/render claims explicitly false.
6. **Armor applicability:** all four serialized `LimbArmor` components on the recovered Mission Occupant are `ArmorClass.Type.None`; there is no active front/back armor type in this Mission actor profile. A Type I–IV penetration pass/stop test cannot be claimed from this scene. Do not mutate `type` to force a branch or fabricate an armored character. Broader armor acceptance stays deferred until an exact recovered actor/equipment prefab with active armor, its ArmorClass settings, and collision/material closure is added to the selected source package.
7. **Existing contract boundaries:** `MissionProjectileContracts.FireOnce` proves one selected Occupant hit. `WeaponContracts` proves source magazine/reload state and reload audio event order. Neither proves two full Mission hit lifecycles or audible audio. Headless audio adapters remain silent.

## Runtime-discovered headless fixture dependency

Run #60 reached the first source impact, then its additional pose/cycle ticks called `DynamicLimbDecalRenderer.Update → Render → LimbRenderer.SetDynamic → Graphics.Blit` and failed because no texture-transfer driver is bound. The first test turn’s wound-decal flag alone does not suppress this later renderer path. Commit `a1ee8c34d242a2733a0209a60ed8e35d9988c5ef` disables original `DynamicLimbDecalRenderer` components after the first hit before advancing more ticks. This preserves wound/tissue/physiology callbacks while keeping pixel rendering explicitly out of scope. Exact-source run #61 (`40316946735`) is now validating that fixture and package. Do not install a no-op texture driver and call it rendered output.

## Batched acceptance plan

- Run #58 on source SHA 17194f02ee8d6cf94d27477a595ff76be00737f1 is the full passing baseline: physiology, exact Mission profile/package integrity, retained artifact and Chromium IndexedDB all passed.
- Run #59 failed compilation on an incorrect RightArm member name; source-authored field is lowercase rightArm. Commit 16c12f7 corrected it.
- Run #60 reached the first impact but later pose/cycle ticks invoked the original wound-decal renderer and required an unbound headless texture-transfer driver. Commit a1ee8c3 disables only DynamicLimbDecalRenderer after first wound in the headless fixture; it does not claim wound pixels.
- Run #61 passed both distinct source Occupant hits and their wound/tissue/shock deltas, then failed an immediate-cleanup assertion. The recovered source permits status Continue after impact.
- Run #62 passed the two-hit and damage state checks, observed one live source projectile immediately after impact and then zero after six original world ticks; it failed only because the contract still required a one-second timeout. The recovered source allows earlier destruction through Stop/Error collision result, low energy or world bounds as well as the one-second age check.
- Corrected batch: retain source hit and damage assertions; keep the live owner count at impact; advance only via original RuntimeWorld ticks until no owner-matched source projectile remains (bounded); report tick count, scaled source time and elapsed age without insisting the one-second condition occurred. Assert no force-destroy/fake cleanup and zero live projectiles after original source updates.
- Run the exact complete workflow for the corrected source SHA: full source build, profile/index verification, package hash and independent verifier, retained artifact, and Chromium IndexedDB checks. No deployment for test-only changes.

## Boundaries

The pass will prove two source-fired torso injuries and source-driven projectile removal through original runtime updates. It does not prove active armor penetration (the Mission armor types are all None), continued projectile travel's visual path, native physics, wound pixels, particles, real audio playback or device ergonomics. Keep those claims separate.
