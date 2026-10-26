# Wound and projectile-trace dependency closure — 2026-10-26

## Scope and evidence

This closes the serialized dependency inventory for the source `Emitter` wound/blood path and the optional `ProjectileTrace` path before further rendering work. It uses the full Mission particle inventory (decompressed SHA-256 `636935d6bf545dd0bf3610008bbc8759fae1fc765553852b8110da9eacc82407`; source package SHA-256 `92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb`) and the original C# callbacks. It does not claim browser rendering parity.

The source contract is `scripts/original_particle_lifecycle_dependency_test.mjs`. It checks prefab hashes, incoming references, component file IDs, renderer and ParticleSystem fields, source call ordering, all 27 prefab renderer references, and the exact unresolved runtime asset set below. It also binds Mission muzzle-smoke and head-bounce owners to their original source callbacks. Focused command: `node --test scripts/original_particle_lifecycle_dependency_test.mjs`.

## Whole-inventory runtime asset reconciliation

The full inventory contains 15 particle materials, 12 textures, and five custom meshes. The browser's impact-only manifests contain eight materials, four textures, and three meshes. The seven extra materials, eight extra textures, and two extra meshes below are referenced by one or more of the 27 inventoried particle prefabs. The test computes the reference sets from every ParticleSystemRenderer and checks these exact deltas.

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

| Missing browser custom mesh | Original source asset | SHA-256 | Inventory metadata |
| --- | --- | --- | --- |
| `9b4443c616aa6fa429dd969b46cbca0e` | `Assets/Mesh/Chunk.asset` | `f241243a56ac693db70f6bf8276c967cb54cfe25a546fea5f7edd2ff015e8391` | 79 vertices; mesh buffers absent |
| `f62208a372e889c4f8ae2dde522cce11` | `Assets/Mesh/LowPolySphere.asset` | `68d32986c468bc15a1abf15edf3659ba168fc5225f5ec155c56c4fb332ed0e33` | 79 vertices; mesh buffers absent |

Search and hash verification in this run found none of these material, texture, or mesh source files in `source-reference/Original/Assets`; the retained inventory gives paths, source hashes, and serialized references but does not contain texture pixels or the two missing meshes' vertex/index buffers. The Spark texture's exact expected SHA is known, but its original bytes remain unavailable. Do not use generated textures, inferred mesh geometry, or parsed inventory fields as replacements for byte-exact source assets.

## Wound emitter closure

| Dependency | Source record | Recovery state |
| --- | --- | --- |
| `Assets/GameObject/Wound emitter.prefab` | SHA-256 `5331cf1fddcb0adc2a434ea0191f077925afe5ec0ab6a8d92c0f753932cf419f`; one system; one renderer | Serialized prefab and three incoming owners are in the inventory |
| `Emitter` component | fileID `114`; script GUID `7cc04c590272ba6bd67608c51dd6df33`; ParticleSystem fileID `198884547391421755` | Original `Start`, settings handlers, `Emit`, `BloodStreamBounceEmit`, and `EmitColumn` are present and source-order tested |
| `ParticleSystemRenderer` | fileID `199724804207875695`; render mode `4`; material GUID `2f18bd9e6a4de9e45b6395e6e325af12`; mesh GUID `f62208a372e889c4f8ae2dde522cce11` | Both referenced original records are in the inventory; the material and mesh hashes appear in the missing-asset tables below |
| Blood material | `Assets/Material/Blood particle new.mat`; SHA-256 `52ffba71d0fc041a2230376cddbbeb0c7884af26aa2a8e3a1f3fd7980bc113a1`; built-in shader GUID `0000000000000000f000000000000000`; no texture reference | Parsed material data is inventoried, but absent from the browser's impact-only material manifest |
| LowPolySphere | `Assets/Mesh/LowPolySphere.asset`; SHA-256 `68d32986c468bc15a1abf15edf3659ba168fc5225f5ec155c56c4fb332ed0e33`; 79 native vertices | Inventory has metadata only; it does not contain decoded vertex/index buffers. Browser mesh JSON is absent |
| Owners | Operator, Occupant, and Mission `woundEmitterPrefab` fields | All three serialized incoming references are inventory checked |

Current actor snapshots transport source `Emit`, `Simulate`, and `Play` commands with source component/renderer fields, world pose, and Rigidbody velocity. `original-particle-renderer.mjs` only draws the recovered impact `materialEffect` path; it does not draw the Wound emitter's generic source command. Adding the blood effect to the renderer before recovering the 79-vertex source mesh would either silently omit geometry or substitute an unverified shape. The inventory contains no mesh buffers from which to implement the authored mesh path. Next input for this subpath: the exact LowPolySphere `.asset` source bytes (matching the hash above) or an independently verified export of its complete vertex/index channels. Then add the Blood material to a separately scoped, source-hash-checked material package and wire the source command lifecycle.

## ProjectileTrace closure

| Dependency | Source record | Recovery state |
| --- | --- | --- |
| `Assets/GameObject/Projectile trace.prefab` | SHA-256 `be9cd9699c39ee7190012e3b9eb33a297ca3898d7f467a2307bd193d4ce58099`; ParticleSystem plus LineRenderer | Serialized trace component, fade time `0.03`, line width `0.01`, world-space setting, and hit-effect reference are inventoried |
| `ProjectileTrace` component | script GUID `691fc9e4f1bd53933eb96a1e0f9ad9d9` | Original initialization, point update, time-scale, fade, destruction, and hit callback order are source tested |
| LineRenderer material | GUID `b69974e8f5204bf45a1bc5df664959fd` | Not among the 15 inventoried ParticleSystem materials; GUID search found no matching `.mat` or `.meta` in the recovered original Assets tree |
| Tracer ammo owners | 5.56 mm, 7.62 mm, and 9 mm FMJ Tracer prefabs, each referencing `tracePrefab` | Incoming references are known, but the three owner prefab files are absent from the 27-prefab particle package and recovered source tree |
| Tracer hit effect | `Assets/GameObject/Tracer hit.prefab`; SHA-256 `7ba0c2dc98e1ecc5479f7d36ea662cbfac82bd670f7014788725619444d3ea91` | Serialized hit prefab and incoming trace reference are inventoried |

The default 5.56 mm FMJ projectile does not assign `tracePrefab`; the optional trace path is exercised only by tracer ammunition. The recovered source inventory therefore supports testing source `ProjectileTrace` ordering but cannot provide a complete runtime visual test. Next input for trace activation: all three authored tracer-ammo prefabs plus the exact LineRenderer material referenced by GUID. Keep the trace disabled for sources with a null `tracePrefab`; do not substitute a material or enable the trace on the default M4 round.

## Mission weapon-smoke and head-bounce callbacks

- `Weapon smoke emitter.prefab` (SHA-256 `a47a5d19fd2b3261c6e85d438bc464a9827fab5a7545fb743f82020bbf4437d2`) is owned by Mission's `weaponSmokeEmitterPrefab` field. `Mission.Awake` instantiates and parents it before initializing `MissionHelper`; `WeaponBase.EmitMuzzleSmoke` emits at the muzzle and, when suppressed, emits a second particle at the cycling bolt. The source ordering and owner link are checked.
- `Head bounce brain emission.prefab` (SHA-256 `59a5b8b73f9d22264aaf12295d0c9ce7277bf5d55c920244f063fd18c21c9a00`) is owned by Operator and Occupant `headBrainBounce` fields. `Limb.OnCollisionEnter` applies the head shock and then creates the linked effect at the wound position; `BallisticHitEffect.PlayBounce` stamps Unity time/lifetime, invokes `BounceEmission`, and plays the source audio path. The source ordering and both prefab owners are checked.
- These source contracts close the event/owner path only. Weapon-smoke material/texture and brain/bounce render assets remain in the whole-inventory missing set above; no visual playback claim is made.

## Gate disposition

- **Serialized closure:** PASS for the wound and trace prefabs in this bounded particle inventory; focused contract also confirms the owners outside the prefab inventory are absent.
- **Original source ordering:** PASS for the currently recovered C# methods.
- **Wound browser rendering:** BLOCKED on exact LowPolySphere geometry and a runtime binding for the recovered Blood material; no approximated mesh was introduced.
- **Tracer browser rendering:** BLOCKED on tracer owner prefabs and the exact LineRenderer material; default M4 remains unaffected because its source `tracePrefab` is null.
- **Browser/device parity:** NOT RUN. No rendering claim is made from a source-contract test.

This is a source dependency closure checkpoint. The separate impact-particle map remains unchanged. No package was published or deployed.
