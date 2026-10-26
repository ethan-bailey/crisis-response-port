# Wound and projectile-trace dependency closure — 2026-10-26

## Scope and evidence

This closes the serialized dependency inventory for the source `Emitter` wound/blood path and optional `ProjectileTrace` path before further rendering work. It combines the bounded historical Mission particle inventory (decompressed SHA-256 `636935d6bf545dd0bf3610008bbc8759fae1fc765553852b8110da9eacc82407`) with the complete recovery archive SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d` and its generated source index SHA-256 `657b93c8a385f5f94e337eb9d49f9ee10d7b581f080bf43c13c25cbe04343c88`. The artifact supplies the serialized files and binary buffers used below. It does not yet establish that this export is the exact same project revision as the historical particle inventory or deployed Site source, and it does not claim browser rendering parity.

The source contract is `scripts/original_particle_lifecycle_dependency_test.mjs`. It checks prefab hashes, incoming references, component file IDs, renderer and ParticleSystem fields, source call ordering, all 27 prefab renderer references, and the exact unresolved runtime asset set below. It also binds Mission muzzle-smoke and head-bounce owners to their original source callbacks. Focused command: `node --test scripts/original_particle_lifecycle_dependency_test.mjs`.

## Whole-inventory runtime asset reconciliation

The historical 27-prefab inventory contains 15 particle materials, 12 textures, and five custom meshes. The current impact-only browser maps bind eight materials and four textures. Seven materials and eight textures are not yet in those maps. Five source-mesh JSON files are now present; three are the previously recovered historical meshes, while `Chunk` and `LowPolySphere` come from the hash-mismatched complete export documented below. The test computes the reference sets from every ParticleSystemRenderer and checks these exact deltas.

| Missing browser material | Original source asset | SHA-256 |
| --- | --- | --- |
| `02cb3c12ddabcf842bd3a305d976b73b` | `Assets/Material/Stun grenade smoke.mat` | `cc751bc69d50ab0d0ec0505952b3d0a80040e4b85eb75386b8cf06f3cc4c33ce` |
| `166803ee771bf814cbffdb9dd564b612` | `Assets/Material/Flesh chunk.mat` | `15afa0e3ee3b3862748458d988afdb963d3a416719a8578b878b613ba9a044c6` |
| `28219ea11a6740946a759c6c6cc2b9f8` | `Assets/Material/ParticleSpark.mat` | `79e89c68e5a374f7b841920ddf78eacec39d8f490953b1741b841d5dedd05607` |
| `2f18bd9e6a4de9e45b6395e6e325af12` | `Assets/Material/Blood particle new.mat` | `52ffba71d0fc041a2230376cddbbeb0c7884af26aa2a8e3a1f3fd7980bc113a1` |
| `3ef6f72747746c04094e0dac76918064` | `Assets/Material/Hydro mist.mat` | `cfcf7b554f75b69a291604585154054ee5392695817439e303801ead28b27b2f` |
| `78bd66f8e354a564287553acff2ab912` | `Assets/Material/Gun smoke.mat` | `8c571454798d1b9a6cd6c4f33a1a2483eb423aa6fca94d216dfe296bef717c4c` |
| `f88e26bf467496741b4c28316201b48b` | `Assets/Material/Brain.mat` | `f6863c70a59a35a1adcdf995ce28c596e50f827dbf65a7250aae57d34052d5ba` |

| Missing browser texture | Original source asset | SHA-256 |
| --- | --- | --- |
| `0698d19480d8616439566fcc492a6141` | `Assets/Texture2D/brain albedo.png` | `4009ad0dbed33e3d7cc8d178e143e7c14e6ed456e0498f0e4b8e5323dcdd0122` |
| `3f5678afea752fb4aa55ec3a6bb5ee0d` | `Assets/Texture2D/brain normal.png` | `9445fb9349a8a1803d2d2eace75ab1e9e1a8d6a1d40ae8420aeb25a06fa557e7` |
| `43907091bffd53b478c985dca2762559` | `Assets/Texture2D/Spark.png` | `6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344` |
| `4ae3dad7f7820a14daaff571c7763543` | `Assets/Texture2D/flesh_0.png` | `a3ea7fc3ded54f4fdc5febdcbba003c63f18fa2ab82f88a75a951c465e0ab5c7` |
| `8d74c3abf30ef9342aae5c8a0fad8388` | `Assets/Texture2D/brain specular.png` | `83120ac44793d783acd7545f91dafbb383c3f38a8d94f155349e8c853b00d6ed` |
| `c3d5701bb477a2c47a733d4dd0c3cce3` | `Assets/Texture2D/hydro mist puff.png` | `374cb7f1d327fbff7fa5441f37ef1ba0f309d5e6e4dfb4b0a5f9ed8c8eacaba4` |
| `f0b75ecf96d6104489b28b07451a3db8` | `Assets/Texture2D/crumpled paper normal.png` | `041a589e610d9e523f874f4c5e60277c4106275d6e940bf937c9e10dfee0edcd` |
| `f8daee0cf4e406b4986d4843c0aa9d17` | `Assets/Texture2D/gun smoke.png` | `7349d61d80a35a67027ed88b6d167d6227889a7a987b1d7429f0dbfad33f2d40` |

| Mesh not in historical impact-renderer map | Original source asset | Historical inventory SHA-256 | Historical inventory metadata |
| --- | --- | --- | --- |
| `9b4443c616aa6fa429dd969b46cbca0e` | `Assets/Mesh/Chunk.asset` | `f241243a56ac693db70f6bf8276c967cb54cfe25a546fea5f7edd2ff015e8391` | Historical inventory metadata only; complete-export geometry recovered at a different hash |
| `f62208a372e889c4f8ae2dde522cce11` | `Assets/Mesh/LowPolySphere.asset` | `68d32986c468bc15a1abf15edf3659ba168fc5225f5ec155c56c4fb332ed0e33` | Historical inventory metadata only; complete-export geometry recovered at a different hash |

### Complete-export reconciliation (2026-10-26)

The canonical full-project artifact contains all **17** files listed above at their expected paths. Each `.meta` GUID matches the serialized dependency inventory. However, the SHA-256 of **every one** differs from the older inventory hash in the tables: the complete export is a different content revision for these files. Therefore these source records are recovered and hash-verified against the full archive, but the old particle inventory's particular asset bytes are not byte-exactly reconciled. Keep the old and full-export hashes side by side; do not replace old records or promote the new variants into a runtime material/texture map until a source-version crosswalk establishes which revision belongs to the target Site/gameplay build.

| Original source path | Historical inventory SHA-256 | Complete-export SHA-256 | GUID |
| --- | --- | --- | --- |
| `Assets/Material/Stun grenade smoke.mat` | `cc751bc69d50ab0d0ec0505952b3d0a80040e4b85eb75386b8cf06f3cc4c33ce` | `745284658c11a82e616b8ed9d06ba1635ca79059be5e84ffa4489f1e683b8f1c` | `02cb3c12ddabcf842bd3a305d976b73b` |
| `Assets/Material/Flesh chunk.mat` | `15afa0e3ee3b3862748458d988afdb963d3a416719a8578b878b613ba9a044c6` | `8f872385fd9db02d3a0018d7c1acedd14dfad1df9c33dee33cb1d54484e55b71` | `166803ee771bf814cbffdb9dd564b612` |
| `Assets/Material/ParticleSpark.mat` | `79e89c68e5a374f7b841920ddf78eacec39d8f490953b1741b841d5dedd05607` | `7796856c15d631d5b8dca607ce8236f9c50f73cac98c949e31fb56857c70869a` | `28219ea11a6740946a759c6c6cc2b9f8` |
| `Assets/Material/Blood particle new.mat` | `52ffba71d0fc041a2230376cddbbeb0c7884af26aa2a8e3a1f3fd7980bc113a1` | `c255dba9c7a89b21ced32c78bc6697adc7101d40b41f9fe6646f397e1f6cf8e2` | `2f18bd9e6a4de9e45b6395e6e325af12` |
| `Assets/Material/Hydro mist.mat` | `cfcf7b554f75b69a291604585154054ee5392695817439e303801ead28b27b2f` | `2c547c3739075e74aa4c589fa972295b49ff779a9b5ba982154cbd410811f57b` | `3ef6f72747746c04094e0dac76918064` |
| `Assets/Material/Gun smoke.mat` | `8c571454798d1b9a6cd6c4f33a1a2483eb423aa6fca94d216dfe296bef717c4c` | `94dfe83a827dd212959fb161820282b2c73e28c18fab910e3e5092c41edc9963` | `78bd66f8e354a564287553acff2ab912` |
| `Assets/Material/Brain.mat` | `f6863c70a59a35a1adcdf995ce28c596e50f827dbf65a7250aae57d34052d5ba` | `8ca71943561b211ada72538606e1573b4806e8c1d278f6f1c8905077413952a7` | `f88e26bf467496741b4c28316201b48b` |
| `Assets/Texture2D/brain albedo.png` | `4009ad0dbed33e3d7cc8d178e143e7c14e6ed456e0498f0e4b8e5323dcdd0122` | `69a57af4451cb85a422dda58418414b84887038f139aa28d382451d567e21781` | `0698d19480d8616439566fcc492a6141` |
| `Assets/Texture2D/brain normal.png` | `9445fb9349a8a1803d2d2eace75ab1e9e1a8d6a1d40ae8420aeb25a06fa557e7` | `842fbd2a2ada9eb48b19d7dc32e27fb1cfc908c04c5cffc2e5a35c7106557df0` | `3f5678afea752fb4aa55ec3a6bb5ee0d` |
| `Assets/Texture2D/Spark.png` | `6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344` | `b33ee840f7cc744655e99b138a413f72036a92bd89e237d8ed8cdf412b4a2929` | `43907091bffd53b478c985dca2762559` |
| `Assets/Texture2D/flesh_0.png` | `a3ea7fc3ded54f4fdc5febdcbba003c63f18fa2ab82f88a75a951c465e0ab5c7` | `ca2723bb62af35e598ba440097345d842204e844779e1a8f54362fafdb13337a` | `4ae3dad7f7820a14daaff571c7763543` |
| `Assets/Texture2D/brain specular.png` | `83120ac44793d783acd7545f91dafbb383c3f38a8d94f155349e8c853b00d6ed` | `ac6fb17a7ae734abf17600feebdb519eaed83382801ce347b707153adb9cf6a9` | `8d74c3abf30ef9342aae5c8a0fad8388` |
| `Assets/Texture2D/hydro mist puff.png` | `374cb7f1d327fbff7fa5441f37ef1ba0f309d5e6e4dfb4b0a5f9ed8c8eacaba4` | `83fce7b90d6ab4db85624a25b86ff740facbab964f844b952c6f550855f18b8d` | `c3d5701bb477a2c47a733d4dd0c3cce3` |
| `Assets/Texture2D/crumpled paper normal.png` | `041a589e610d9e523f874f4c5e60277c4106275d6e940bf937c9e10dfee0edcd` | `cee4c1ececd6b660a2aabd86f46b34ecbe3fb4ef017f61e873285d2a85bbb030` | `f0b75ecf96d6104489b28b07451a3db8` |
| `Assets/Texture2D/gun smoke.png` | `7349d61d80a35a67027ed88b6d167d6227889a7a987b1d7429f0dbfad33f2d40` | `30a3ca49021d3db01c70dd6a4735646c683b2bc654e2652e75d06699d37614f3` | `f8daee0cf4e406b4986d4843c0aa9d17` |
| `Assets/Mesh/Chunk.asset` | `f241243a56ac693db70f6bf8276c967cb54cfe25a546fea5f7edd2ff015e8391` | `16c78ce02019fd56c4e904fe6fc6095a582601ad06564c1d316e97605913f25d` | `9b4443c616aa6fa429dd969b46cbca0e` |
| `Assets/Mesh/LowPolySphere.asset` | `68d32986c468bc15a1abf15edf3659ba168fc5225f5ec155c56c4fb332ed0e33` | `6b07b7570773145d9e5676a32ee293274cd0dc4dca41fc767680615f7a0cad9a` | `f62208a372e889c4f8ae2dde522cce11` |

The recovered complete-export mesh files contain usable source channels. `LowPolySphere.asset` has 79 raw vertices and 336 indices (112 triangles); `Chunk.asset` has 28 packed vertices and 48 indices (16 triangles). `scripts/extract_original_particle_meshes.py` now decodes these two in addition to the three previously recovered particle meshes, checking each complete-export SHA before decoding, preserving source normals/UVs when present, converting coordinate handedness, and reversing triangle winding. Generated JSON was checked by `original_particle_mesh_source_test.mjs`: 5/5 source meshes, finite/non-degenerate faces, index bounds, source normal/winding agreement, and no invented UVs. This closes geometry availability for the **complete-export variant** only; the hash mismatch above still blocks calling it the exact historical particle-inventory mesh.

## Wound emitter closure

| Dependency | Source record | Recovery state |
| --- | --- | --- |
| `Assets/GameObject/Wound emitter.prefab` | SHA-256 `5331cf1fddcb0adc2a434ea0191f077925afe5ec0ab6a8d92c0f753932cf419f`; one system; one renderer | Serialized prefab and three incoming owners are in the inventory |
| `Emitter` component | fileID `114`; script GUID `7cc04c590272ba6bd67608c51dd6df33`; ParticleSystem fileID `198884547391421755` | Original `Start`, settings handlers, `Emit`, `BloodStreamBounceEmit`, and `EmitColumn` are present and source-order tested |
| `ParticleSystemRenderer` | fileID `199724804207875695`; render mode `4`; material GUID `2f18bd9e6a4de9e45b6395e6e325af12`; mesh GUID `f62208a372e889c4f8ae2dde522cce11` | Both referenced original records are in the inventory; the material and mesh hashes appear in the missing-asset tables below |
| Blood material | `Assets/Material/Blood particle new.mat`; SHA-256 `52ffba71d0fc041a2230376cddbbeb0c7884af26aa2a8e3a1f3fd7980bc113a1`; built-in shader GUID `0000000000000000f000000000000000`; no texture reference | Parsed material data is inventoried, but absent from the browser's impact-only material manifest |
| LowPolySphere | Historical inventory SHA-256 `68d32986c468bc15a1abf15edf3659ba168fc5225f5ec155c56c4fb332ed0e33`; complete-export SHA-256 `6b07b7570773145d9e5676a32ee293274cd0dc4dca41fc767680615f7a0cad9a`; 79 native vertices | Complete-export geometry is decoded to browser JSON; asset hash differs from the historical inventory, so same-revision fidelity remains unproven |
| Owners | Operator, Occupant, and Mission `woundEmitterPrefab` fields | All three serialized incoming references are inventory checked |

Current actor snapshots transport source `Emit`, `Simulate`, and `Play` commands with source component/renderer fields, world pose, and Rigidbody velocity. `original-particle-renderer.mjs` only draws the recovered impact `materialEffect` path; it does not draw the Wound emitter's generic source command. The complete-export snapshot now supplies and decodes LowPolySphere's 79 vertices and 112 triangles, but its asset SHA differs from the historical inventory. The renderer also lacks a runtime binding for the Blood material and the source command lifecycle. Do not wire this variant into visuals until its source revision is reconciled and its material dependency is mapped from the same revision.

## ProjectileTrace closure

| Dependency | Source record | Recovery state |
| --- | --- | --- |
| `Assets/GameObject/Projectile trace.prefab` | SHA-256 `be9cd9699c39ee7190012e3b9eb33a297ca3898d7f467a2307bd193d4ce58099`; ParticleSystem plus LineRenderer | Serialized trace component, fade time `0.03`, line width `0.01`, world-space setting, and hit-effect reference are inventoried |
| `ProjectileTrace` component | script GUID `691fc9e4f1bd53933eb96a1e0f9ad9d9` | Original initialization, point update, time-scale, fade, destruction, and hit callback order are source tested |
| LineRenderer material | `Assets/Material/Projectile trace.mat`, GUID `b69974e8f5204bf45a1bc5df664959fd` | Recovered from the complete export; SHA-256 `f905ef224a6584190b21b129e3834a1781a201116c685643fa5a057aa0db6e58`. It uses Unity's built-in Standard shader (`0000000000000000f000000000000000`) and has no external texture references. This closes the material-file dependency for that export, but no Site/historical version crosswalk exists. |
| Tracer ammo owners | 5.56 mm, 7.62 mm, and 9 mm FMJ Tracer prefabs, each referencing `tracePrefab` | Outside the 27-prefab particle inventory, but the complete six-root Mission profile includes the tracer/projectile owners; use that profile for source-level closure |
| Tracer hit effect | `Assets/GameObject/Tracer hit.prefab`; SHA-256 `7ba0c2dc98e1ecc5479f7d36ea662cbfac82bd670f7014788725619444d3ea91` | Serialized hit prefab and incoming trace reference are inventoried |

The default 5.56 mm FMJ projectile does not assign `tracePrefab`; the optional trace path is exercised only by tracer ammunition. The material dependency has now been found in the complete export: its serialized color is `(1, 0.9841784, 0.42647058, 1)`, tint is `(1, 0.96044624, 0.42647058, 0.5)`, emission is black, and both saved texture slots are null. The export's LineRenderer width multiplier is `0.01`; source `ProjectileTrace.Initialize` overrides both ends with the projectile's metric diameter, then `Update` appends a point after one meter of travel, scales particle speed by `Time.timeScale`, and fades/destroys the line using `traceFadeTime` (serialized as `0.03` in the prefab). The complete-export material has no historical hash in the bounded 27-prefab inventory because that inventory omitted LineRenderer materials. Its source revision therefore remains un-crosswalked; do not bind this recovered variant into runtime visuals until the target Site/gameplay revision is identified. Keep the trace disabled for sources with a null `tracePrefab`; do not substitute a material or enable the trace on the default M4 round.

## Impact InheritVelocity missing-command recovery — 2026-10-26

The reported `Original particle InheritVelocity requires its source Rigidbody command: 198243736805655561` exception occurred when the impact renderer had no matching source Emit command and the `materialEffect` value also lacked `sourceRigidbodyVelocity`. That component fileID is used by the recovered impact emitters with InheritVelocity enabled at multiplier 1. The renderer now selects a finite attached-Rigidbody velocity from the source Emit command first, then the material-effect event. If both transports omit it, it uses zero for this visual-only term and increments `sourceVelocityFallbacks` in its snapshot instead of throwing through `spawnImpact` and aborting the frame. This prevents a presentation crash while keeping the approximation observable; the zero-fallback path does not claim source velocity parity. Node particle binding/value checks passed 21/21 after the change.

Complete-export records used for this dependency expansion: `Projectile trace.mat` SHA-256 `f905ef224a6584190b21b129e3834a1781a201116c685643fa5a057aa0db6e58`; `Projectile trace.prefab` SHA-256 `6591172c2aad05c0fa64d2d9aad6888c23068f7ab0ad7cb7239745ce29afe358`; decompiled `ProjectileTrace.cs` SHA-256 `05dee1e4e58a785a1ebd223d86b2ffa08e91d28daf2d0d01d0e0749f7b5e7859`. The project export's script and checked-in namespaced runtime source differ only by the runtime namespace and `Object` alias wrapper on direct diff; that wrapper is not a source-version crosswalk for the serialized prefab/material.

## Mission weapon-smoke and head-bounce callbacks

- `Weapon smoke emitter.prefab` (SHA-256 `a47a5d19fd2b3261c6e85d438bc464a9827fab5a7545fb743f82020bbf4437d2`) is owned by Mission's `weaponSmokeEmitterPrefab` field. `Mission.Awake` instantiates and parents it before initializing `MissionHelper`; `WeaponBase.EmitMuzzleSmoke` emits at the muzzle and, when suppressed, emits a second particle at the cycling bolt. The source ordering and owner link are checked.
- `Head bounce brain emission.prefab` (SHA-256 `59a5b8b73f9d22264aaf12295d0c9ce7277bf5d55c920244f063fd18c21c9a00`) is owned by Operator and Occupant `headBrainBounce` fields. `Limb.OnCollisionEnter` applies the head shock and then creates the linked effect at the wound position; `BallisticHitEffect.PlayBounce` stamps Unity time/lifetime, invokes `BounceEmission`, and plays the source audio path. The source ordering and both prefab owners are checked.
- These source contracts close the event/owner path only. Weapon-smoke material/texture and brain/bounce assets remain unbound from the historical browser maps; no visual playback claim is made.

## Gate disposition

- **Serialized closure:** PASS for the wound and trace prefabs in this bounded particle inventory. The complete six-root source profile includes all three tracer/projectile owner paths and the workflow checks the required source tree before packaging.
- **Original source ordering:** PASS for the recovered C# methods.
- **Wound browser rendering:** BLOCKED on resolving the complete-export versus historical inventory hash mismatch and binding the same-revision Blood material and generic source command lifecycle; no replacement visuals were introduced.
- **Tracer browser rendering:** BLOCKED on identifying the target source revision and connecting the recovered same-export LineRenderer material and trace lifecycle to browser visuals. The material's file/shader/texture closure is now inventoried for the complete export; default M4 remains unaffected because its source `tracePrefab` is null.
- **Browser/device parity:** NOT RUN. No rendering claim is made from a source-contract test.

This is a source dependency closure checkpoint. The separate impact-particle map remains unchanged. No package was published or deployed.
