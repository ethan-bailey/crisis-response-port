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

## Batched acceptance plan

After the active run #58 has fully passed (Contracts/Release and Chromium IndexedDB), extend `MissionProjectileContracts` in one test-only batch:

- Preserve its existing first-shot assertion.
- Stage a second initialized Occupant in the same clear lane, move the first victim clear, update the original Blackboard target/aim, and allow the source arm pose and 0.1-second weapon cycle to complete.
- Invoke `PullTrigger` once, require exactly one increment in `ShotsFired`, observe exactly one additional source projectile owned by the same Operator, verify collision and positive injury-state deltas on the second Occupant, and assert no duplicate/ownerless projectile remains after source cleanup.
- Return the two separate victim results and per-shot traces in the contract report. Keep blood/particle/wound-pixel rendering, browser audio playback, and native PhysX out of the assertion.

If source runtime behavior reveals another timing/target/lifetime prerequisite, record the exact source callback/state and recovered dependency here before making the next implementation. Run the full pinned exact-SHA workflow, retain package hashes and artifact, and do not deploy checks-only code.
