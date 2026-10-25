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
- Shape module type codes: `4` = 43, `2` = 7, `10` = 1. The source enum values and shape parameters are preserved verbatim; this report does not substitute guessed semantics.
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

**Source callback transport: passed locally; visual particle implementation and rendering: not passed.** The incremental Mission world binds `BrowserParticleEventQueue` before scene load. The browser actor bridge delivers `Emit`, `Simulate`, and `Play` callbacks as bounded snapshot events carrying Unity time, original GUID/file ID/path/hash, hierarchy path, world pose, and the complete serialized `ParticleSystem` and sibling `ParticleSystemRenderer` fields. The source renderer identity must match the particle system's source prefab identity. `PrefabCloner` retains this component identity and field data on runtime prefab clones. EngineChecks are 74/74; the exact Mission actor/weapon source contract observed a real M4 shot, one particle event with source asset hash and nonempty authored module/renderer fields, and zero dropped events. The separate browser binder still covers only 14 ballistic-impact prefabs / 26 systems and reports `renderedParticles: 0`. Source module execution, renderer mode `1`, the five custom meshes not in the impact-only extraction, complete 15-material/12-texture closure, built-in shader behavior, curves/bursts, all 51 source systems, source-time particle simulation, lifecycle ordering, and visible-browser verification remain outstanding.

The remaining work is therefore indexed and batchable: implement source module simulation and browser rendering over these 51 systems, all 27 prefab owner graphs, modules/curves/bursts, original render assets, source clocks and cleanup; add source contract checks for each emission path; then run a WebAssembly/WebGL browser smoke. A Playwright binary is unavailable in this environment and physical iPhone Safari acceptance requires the user's iPhone; those are acceptance constraints, not substitutes for implementing the source closure.
