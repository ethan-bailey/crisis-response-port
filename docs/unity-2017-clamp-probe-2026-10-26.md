# Unity 2017.2 particle clamp probe — prepared 2026-10-26

## Why this fixture exists

The source inventory contains 31 active `ClampVelocityModule` records, but YAML field names do not establish Unity's runtime mapping or clamp/dampen ordering. The authored bytes must be read through the target editor/runtime before browser particle motion is implemented. The workspace has no Unity executable, so this batch prepares the probe and defers its engine run.

## Run in the recovered project

1. Use the authorized original Unity project and open it with **Unity 2017.2.0f3**.
2. Copy `scripts/unity_2017/ParticleClampVelocityProbe.cs` into the project's `Assets/Editor/` directory.
3. Let Unity compile, then select **Crisis Response → Probe Clamp Velocity Modules**.
4. Preserve `Temp/cpr-clamp-velocity-probe.json` with the project archive SHA, editor version, and run log.

The probe enumerates all prefab assets and reports each active particle system's `enabled`, `limit.constant`, `limit.curveMultiplier`, `separateAxes`, `space`, `dampen`, drag curve and drag multipliers. It also emits one controlled particle at under-limit, at-limit, and over-limit speeds; samples after one simulation step at four step sizes and three dampen values; and records the resulting velocity components and magnitude.

## Acceptance limits

- The JSON must report the exact Unity version and all 31 expected active modules from the recovered Mission source; compare asset paths and hierarchy paths to `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`.
- Do not infer native field mapping from the synthetic cases. The output is an observed behavior fixture, not proof that the source package is complete or that browser rendering matches Unity.
- Preserve the output and compare controlled measurements across fixed-step sizes. Bind the resulting operation order and coordinate space to a browser runtime-path test before implementing module motion.
- This fixture has not been compiled or executed in Unity. The current local check is source-only: `node --test scripts/mission_particle_clamp_velocity_serialization_test.mjs` passed **2/2**, while `command -v Unity` and `command -v unity` returned no executable.

The probe uses the Unity 2017.2 ParticleSystem scripting API (`LimitVelocityOverLifetimeModule`, `MinMaxCurve`, `EmitParams.velocity`, and fixed-step `Simulate`) as the readback/measurement surface. The official API pages are linked in `docs/particle-clamp-velocity-serialization-2026-10-26.md`.
