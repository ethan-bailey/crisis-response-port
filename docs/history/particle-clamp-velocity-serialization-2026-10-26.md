# Particle velocity-clamp serialization closure — 2026-10-26

## Exact source boundary

This regression uses the complete serialized `global-ui-mission-operator` inventory, not a hand-picked prefab sample.

- Inventory file: `docs/history/mission-particle-dependency-inventory-2026-10-25.json.gz`
- Decompressed inventory SHA-256: `636935d6bf545dd0bf3610008bbc8759fae1fc765553852b8110da9eacc82407`
- Source package SHA-256: `92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb`
- Scope: 27 prefabs, 51 ParticleSystems, 31 active `ClampVelocityModule` records

The added source contract walks every classID 198 record in the inventory and locks the complete active serialized parameter distribution:

| Parameter | Source inventory result |
| --- | --- |
| Active clamp records | 31 of 51 systems |
| `separateAxis` | 0 for all 31 |
| `magnitude.minMaxState` | 0 for all 31 |
| `magnitude.scalar` | 0 in 26; 5 in 5 |
| `inWorldSpace` | 0 in 25; 1 in 6 |
| `dampen` | 0.5 (2), 0.6 (1), 0.65 (3), 0.7 (1), 0.75 (1), 0.95 (1), 0.97 (1), 0.98 (11), 0.99 (1), 0.9975 (1), 0.999 (5), 0.9995 (3) |
| `drag.minMaxState` and `drag.scalar` | 0 and 0 for all 31 |
| Drag size/velocity multipliers | 1 and 1 for all 31 |
| Serialized X/Y/Z triples | 1 in 25; 0.01 in 6, on each axis |

The test file is `scripts/mission_particle_clamp_velocity_serialization_test.mjs`; it runs in the existing CI command `node --test scripts/*_test.mjs scripts/*_contracts.mjs`. The assertions protect every active record and retain the dormant X/Y/Z values without treating them as active limits while `separateAxis=0`.

## What the Unity 2017.2 API establishes

Unity's 2017.2 `LimitVelocityOverLifetimeModule` API distinguishes the overall `limit` from `limitX/Y/Z`, exposes `separateAxes`, `space`, and `dampen`, and describes `dampen` as the treatment of velocity exceeding the limit. The 2017.2 `separateAxes` page says that it enables separate limits by axis. The 2017.2 `MinMaxCurve` constructors distinguish a constant value from a scalar multiplier applied to a curve.

- [LimitVelocityOverLifetimeModule (Unity 2017.2)](https://docs.unity3d.com/2017.2/Documentation/ScriptReference/ParticleSystem.LimitVelocityOverLifetimeModule.html)
- [separateAxes (Unity 2017.2)](https://docs.unity3d.com/2017.2/Documentation/ScriptReference/ParticleSystem.LimitVelocityOverLifetimeModule-separateAxes.html)
- [MinMaxCurve constructors (Unity 2017.2)](https://docs.unity3d.com/2017.2/Documentation/ScriptReference/ParticleSystem.MinMaxCurve-ctor.html)

These references describe the scripting API. They do **not** specify the native YAML enum-to-property mapping for this recovered serialization or the exact per-frame clamp/dampen operation order. In particular, the 26 active records with `magnitude.scalar=0` must not be interpreted into a browser velocity update from field names alone. The new contract records the authored bytes and distributions; it does not claim that a browser implementation reproduces Unity's particle motion.

## Remaining engine-backed acceptance

The source serialization inventory and its regression contract are closed for this module. The engine semantic check remains necessary before implementing clamp motion:

1. Open the exact recovered prefab records in Unity 2017.2.0f3 (or a compatible Unity editor pinned to the package version).
2. Read back `enabled`, `limit.constant`, `limitX/Y/Z.constant`, `separateAxes`, `space`, `dampen`, and drag fields through `ParticleSystem.limitVelocityOverLifetime`.
3. Simulate particles with known over-limit and under-limit velocities over multiple fixed time steps; record velocity before and after each step for `dampen=0`, representative authored fractions, and the `magnitude.scalar=0/5` groups.
4. Bind those observed values/order to a browser runtime-path test, then implement the module and re-run all 31 serialized cases.

No Unity editor/runtime binary is present in this automation workspace; the exact engine-backed acceptance was not run. This is a concrete dependency for the clamp implementation only. Continue the independent module and lifecycle closures from the full particle inventory; do not pause other source-backed work for this fixture.


## Verification checkpoint

- Source-contract commit: `38c6f45792d6fba9dbd3c1f417f3c67746f72f7d`; runtime source files were unchanged.
- Focused clamp inventory regression: **2/2** locally against the exact compressed inventory. The Actions workflow ran on this SHA and completed successfully: Node **131/131**, Python **41/41**, runtime contracts, EngineChecks, FoundationChecks, and Release WASM publish/output checks.
- Actions run: [#24 — 40268907833](https://github.com/ethan-bailey/crisis-response-port/actions/runs/40268907833).
- The Actions build checked required WASM publish outputs; the independent full archive integrity verifier was not rerun on this test/documentation-only commit. The last full archive verification remains tied to runtime-equivalent commit `afc583e`.
- No Site version was created or deployed.
