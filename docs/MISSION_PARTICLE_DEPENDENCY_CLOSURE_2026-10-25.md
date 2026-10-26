# Mission particle dependency closure — 2026-10-25

## Source boundary and evidence

This is the complete serialized particle dependency inventory for the bounded `global-ui-mission-operator` source package, not a particle-rendering parity claim. It was exported from the Release package built from Site source `d2787f3a6ba31318fa06d165a02a2c93632efc58` (the package used for private Site version 62).

| Evidence | Value |
| --- | --- |
| Package profile | `global-ui-mission-operator` |
| Incremental source assets | 1,919; 97,767,549 compressed / 205,514,345 uncompressed bytes |
| Source package SHA-256 | `92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb` |
| Source manifest SHA-256 | `2acba185c3c14a1b2c60d08ed72b1aeb8c2f3006b977956b26fe70ad95bbbcc2` |
| Dependency inventory SHA-256 (decompressed JSON) | `636935d6bf545dd0bf3610008bbc8759fae1fc765553852b8110da9eacc82407` |
| Source of truth | `Recovered Unity 2017.2.0f3 serialized Mission package` |

The machine-readable evidence at [mission-particle-dependency-inventory-2026-10-25.json.gz](mission-particle-dependency-inventory-2026-10-25.json.gz) retains all serialized records from the 27 particle prefabs, including every ParticleSystem and ParticleSystemRenderer, their GameObject and transform graph, attached source components, and incoming prefab references. It also records the full source material records, shader/texture references, mesh metadata, and per-file source hashes. The exporter is `scripts/export_mission_particle_dependency_inventory.py`; the regression contract is `scripts/mission_particle_dependency_inventory_test.mjs`.

The package manifest and its source records were independently read from the exact Site package staging directory. The exporter fails if any required system, renderer, prefab, material or custom mesh count changes.

## Impact-emission controller closure

The 14 impact prefabs also depend on controller scripts attached to their root and emitter GameObjects. The previously retained ParticleSystem fields did not describe how many particles to emit or which runtime speed/lifetime overrides to apply. The additional source-verified closure is exported by `scripts/export_original_particle_controller_closure.py` to `game/assets/ballistics/original-particle-controllers-source.json.gz`.

| Controller dependency | Count | Source GUID | Recovered implementation |
| --- | ---: | --- | --- |
| `BallisticHitEffect` prefab root | 14 | `f9f9f8d0f023b657c5276ea277044f46` | `runtime/Original/FullSource/BallisticHitEffect.cs` |
| `SingleEmissionEffect` emitter | 26 | `7f9bb5f5ef1d9fb3ca6dbc9296820005` | `runtime/Original/FullSource/SingleEmissionEffect.cs` |

Every controller is linked by the prefab's GameObject and component file IDs to its source ParticleSystem and ParticleSystemRenderer. The closure includes serialized controller fields (entry/exit ratios, velocity ratios, chunk flag, lifetime, random-rotation settings, audio clip references and root offset), prefab source hashes, controller source-file hashes, and the verified source-profile SHA. Export rejects partial coverage, changed hashes, missing component pairs, or script GUID changes. It does not synthesize missing controllers.

The browser evaluates the recovered Unity MinMaxCurve tangents, Gradient keys, and the original `SingleEmissionEffect` entry/exit count and speed formulas. The M4 material-effect transport includes the original projectile energy, metric and millimeter diameter, displaced-material volume, velocity, and particle-count scaler needed by those formulas. `game/original-particle-renderer.mjs` creates billboard and recovered-mesh particles using mapped source textures, geometry, material colors, and authored cone-base/circle-edge shape values for all 26 impact emitters. `ParticleSystem.Emit` callbacks now transport the live count, random seed, speed range, lifetime range, and size multiplier; the renderer joins those values by source prefab/component identity and event time. Its browser random generator is seeded from Unity's supplied seed but does not reproduce Unity's internal random algorithm. The renderer now has a camera-facing stretched-billboard path driven by the source velocity and length scales; regression checks bind it to all six mode-1 records. Their source Spark material/texture is outside the 8-material impact-only map, and the inventory-only systems are not yet connected to a complete browser lifecycle. Native shader equivalence, exact shape/random behavior, complete mode-1 system coverage and browser pixels remain unverified; `sourceVisualParity` stays false.

## Full inventory

| Source prefab | Systems | Mission scene GUID closure | Serialized owner / role |
| --- | ---: | :---: | --- |
| `Assets/GameObject/Ammunition explosion.prefab` | 2 | No | `308-Win-HE` and `7.62-HE` ammunition `explosionPrefab` fields |
| `Assets/GameObject/Frag grenade explosion.prefab` | 2 | Yes | Frag grenade detonation graph |
| `Assets/GameObject/Plastic explosive explosion.prefab` | 2 | Yes | Plastic explosive detonation graph |
| `Assets/GameObject/Projectile trace.prefab` | 1 | No | Tracer ammunition `tracePrefab`; also owns the tracer hit effect reference |
| `Assets/GameObject/Stun grenade explosion.prefab` | 3 | No | Stun grenade `detonationEffect` |
| `Assets/GameObject/Tracer hit.prefab` | 2 | No | Projectile trace `hitEffectPrefab` |
| `Assets/GameObject/Wound emitter.prefab` | 1 | Yes | Mission `woundEmitterPrefab` |
| `Assets/Resources/effects/Head bounce brain emission.prefab` | 1 | No | Operator and Occupant `headBrainBounce` fields |
| `Assets/Resources/effects/Weapon smoke emitter.prefab` | 1 | Yes | Mission `weaponSmokeEmitterPrefab` |
| `Assets/Resources/effects/ballistic hits/Carpet entry.prefab` | 1 | Yes | Ballistic material entry effect |
| `Assets/Resources/effects/ballistic hits/Carpet exit.prefab` | 2 | Yes | Ballistic material exit effect |
| `Assets/Resources/effects/ballistic hits/Concrete entry.prefab` | 2 | Yes | Ballistic material entry effect |
| `Assets/Resources/effects/ballistic hits/Concrete exit.prefab` | 2 | Yes | Ballistic material exit effect |
| `Assets/Resources/effects/ballistic hits/Glass entry.prefab` | 2 | Yes | Ballistic material entry effect |
| `Assets/Resources/effects/ballistic hits/Glass exit.prefab` | 2 | Yes | Ballistic material exit effect |
| `Assets/Resources/effects/ballistic hits/Head entry.prefab` | 2 | No | Head prefab `entryEffect` |
| `Assets/Resources/effects/ballistic hits/Head exit.prefab` | 4 | No | Head prefab `exitEffect` |
| `Assets/Resources/effects/ballistic hits/Limb entry.prefab` | 1 | No | Seven limb prefab `entryEffect` fields |
| `Assets/Resources/effects/ballistic hits/Limb exit.prefab` | 3 | No | Seven limb prefab `exitEffect` fields |
| `Assets/Resources/effects/ballistic hits/Metal entry.prefab` | 2 | Yes | Ballistic material entry effect |
| `Assets/Resources/effects/ballistic hits/Metal exit.prefab` | 2 | Yes | Ballistic material exit effect |
| `Assets/Resources/effects/ballistic hits/Plastic entry.prefab` | 2 | Yes | Ballistic material entry effect |
| `Assets/Resources/effects/ballistic hits/Plastic exit.prefab` | 2 | Yes | Ballistic material exit effect |
| `Assets/Resources/effects/ballistic hits/Wall entry.prefab` | 1 | Yes | Ballistic material entry effect |
| `Assets/Resources/effects/ballistic hits/Wall exit.prefab` | 2 | Yes | Ballistic material exit effect |
| `Assets/Resources/effects/ballistic hits/Wood entry.prefab` | 2 | Yes | Ballistic material entry effect |
| `Assets/Resources/effects/ballistic hits/Wood exit.prefab` | 2 | Yes | Ballistic material exit effect |
| **Total bounded profile** | **51** | **32 in 18 prefabs** | **19 in 9 additional dynamically referenced prefabs** |

“Mission scene GUID closure” means reachable by serialized GUID references from `Assets/Scenes/Mission 01.unity` within the bounded package. The other nine are not dead assets: each has an incoming serialized reference from a runtime owner in the package. The machine inventory preserves the exact owner asset, component fileID and field name for every incoming reference. The package root also includes Title and Transition plus UI/operator roots, so 51 is the complete bounded profile inventory; 32 is the strict Mission scene GUID closure. Both counts are retained to avoid conflating those scopes.

## Component and renderer dependency closure

- **51 ParticleSystem + 51 ParticleSystemRenderer components**, paired by their original GameObject fileID. All original component fields, module values, curve keyframes/tangents, emission bursts, renderer fields, transforms, and source file order are retained in the compressed inventory.
- **27 source prefabs**; all source prefab hashes and GUIDs are recorded.
- Renderer modes: serialized enum `0` = 26 components, `1` = 6, `4` = 19. The existing browser binding only accepts `0` and `4`; mode `1` is a concrete unsupported renderer path.
- Shape module type codes: `4` = 43, `2` = 7, `10` = 1. The Unity 2017.2 `ParticleSystemShapeType` reference identifies hemisphere emission; the seven type-2 records use radius thickness 1 and zero random-position/spherical-direction amounts. The browser now samples their authored upper-hemisphere volume. The per-particle random stream remains an approximation and visual parity is not claimed.
- All systems serialize the same 22 Unity module records. The active subset is:

| Active module | Systems |
| --- | ---: |
| Initial | 51 |
| Emission | 51 |
| Shape | 51 |
| Color | 34 |
| Inherit Velocity | 32 |
| Clamp Velocity | 31 |
| Collision | 29 |
| Size | 26 |
| Rotation | 26 |
| Force | 5 |

- Active Emission modules contain **9 burst entries** across the 51 systems. No active sub-emitter references are serialized. No module disabled in source is promoted to active by the inventory.

## Render assets and shader closure

| Dependency | Count | Coverage |
| --- | ---: | --- |
| Source Materials | 15 | Complete serialized Material records and their original hashes |
| Source Textures | 12 | All non-null material texture references have source paths and hashes |
| Custom Mesh assets | 5 | Source paths, hashes, vertex/submesh and compressed topology metadata |
| Shader dependencies | 3 | Two recovered shader files plus one Unity built-in shader |

The original built-in shader GUID is `0000000000000000f000000000000000`; its engine behavior has not been verified in the browser. One renderer references Unity's built-in mesh GUID `0000000000000000e000000000000000`; it has no project source asset. The other five unique renderer meshes are recovered package assets. These built-ins remain explicit engine compatibility dependencies, not silently treated as recovered source.

## Native lifecycle and ordering to preserve

The original C# source establishes several distinct emission paths, so a single generic “draw impact particle” callback is insufficient:

1. `SingleEmissionEffect.EntryEmission`, `ExitEmission` and `BounceEmission` modify the instance random seed, start-speed min/max, start lifetime, and sometimes start-size multiplier; then call `Simulate(Time.deltaTime, true, true, false)`, `Emit(count)`, `Play()`, and destroy the owning GameObject after its source lifetime.
2. `Emitter` handles blood and wound particles. Its `Start` applies performance maximums and subscribes to settings changes. `Emit`, `BloodStreamBounceEmit`, and `EmitColumn` mutate transform, shape radius/angle and speed before emission; the column path performs repeated source-ordered emissions along a segment.
3. `ProjectileTrace.Initialize` sets source time, particle speed from time scale, attaches a LineRenderer to the projectile, and records its first points. `Update` extends the line with projectile movement; alpha fades against source time and destroys the line renderer at the source fade time.
4. Mission weapon smoke is an ongoing system owned by the Mission resource graph; it does not share the one-shot ballistic effect lifetime.
5. Ballistic material and limb entry/exit effects are selected by source prefab references; `SingleEmissionEffect` derives count/velocity from projectile energy, diameter, displaced volume, settings, and source phase.

Relevant source code: `runtime/Original/FullSource/SingleEmissionEffect.cs`, `Emitter.cs`, `ProjectileTrace.cs`, `Mission.cs`, `Projectile.cs`, `BallisticMaterial.cs`, and `Limb.cs`. Runtime ordering, source `Time.time`/`Time.deltaTime`, settings callbacks, and cleanup must remain source-driven.

## Gate disposition

**Inventory gate: passed for the exact bounded 1,919-asset source profile.** The 26-emitter impact-only map was incomplete; this closure adds 25 previously unindexed system components and identifies the nine prefab owners that explain the scope gap.

**Source callback transport: passed locally. Impact emission-parameter evaluation: implemented and source-contract tested. Initial browser particle rendering: wired for the 14 recovered impact prefabs / 26 emitters; browser pixels and parity are unverified.** The incremental Mission world binds `BrowserParticleEventQueue` before scene load. The browser actor bridge delivers `Emit`, `Simulate`, and `Play` callbacks as bounded snapshot events carrying Unity time, original GUID/file ID/path/hash, hierarchy path, world pose, and complete serialized `ParticleSystem` and sibling renderer fields. `PrefabCloner` retains this identity. Impact events now include original projectile inputs; the browser resolves each source `SingleEmissionEffect`, evaluates the authored entry/exit formula and initial/over-lifetime curves, and creates source-textured billboard or recovered mesh objects. These objects are not yet proven against native Unity pixels. EngineChecks remain 74/74 at the last verified source checkpoint; the Mission actor/weapon contract observed one particle callback with source asset hash and zero dropped callback events.

Remaining source-render gates are bounded by the full inventory: transport and reproduce Unity's actual random seed/state; verify all 15 source materials, 12 texture references, all five custom meshes and built-in shader; handle renderer mode `1`; implement the 51 systems' active Collision, Clamp Velocity, Inherit Velocity, Force, Rotation, Size, Color, and Burst behaviors; wire source `Emitter` blood/wound, `ProjectileTrace`, weapon smoke and bounce lifecycles; add per-path source contracts and verify time ordering/cleanup. Then run WebAssembly/WebGL browser acceptance. Physical iPhone Safari acceptance requires the user's device. The browser shape sampler is an implementation slice, not a parity claim.
