# Crisis Response Port — current handoff

## Live source and verified checkpoint

- Integrated branch: `develop/wasm`; latest tested remote source SHA `630d6f71dd64c4cf71ad50cf5d12b542a458e8e8`, tree `eb330d64a0981d5efeb1b7b3414f1d9da5a1338e`. The local checkout has the same tree; its local commit ID differs because source commits were replayed through the GitHub connector.
- Hosted Actions run #161 (`40694642865`) passed contracts, pinned .NET 8.0.426 Release/WASM build, exact Mission source/package integrity, full-profile Chromium cache acceptance, Mission grounding, and independent reload acceptance.
- Movement/contact ordering is accepted for the tested Mission interval: contact-solved root pose was synchronized, the following original movement sample was nonzero, original movement predicates became true, the authored state selected `Body walk backward`, and release returned to `Body stand`. The browser run marks rendered visual acceptance untested; see the movement record and canonical [gate index](docs/GATE_DEPENDENCY_INDEX.md).
- The protected untracked `scene-data/.crisis-response-scenes.json.gz.PSfVGg` remains untouched and excluded from changes.

## Remaining external dependencies

- Particle parity is blocked on reconciling the historical particle inventory with the complete-export material/texture/mesh hashes; generic wound/tracer particle commands have no renderer/collision driver. Do not map mismatched assets or synthesize results. The next dependency-ready action is recovering/re-extracting the nine historical Unity containers referenced by the inventory, then rerunning the source mesh/material crosswalk.
- Physics uses browser PhysX 5.6.1; Unity 2017 numerical/cooking parity and full dynamic/joint collision coverage are unproven.
- Audio events and source/listener metadata are transported, but browser spatialization, reverb, mixer DSP, and full voice audibility are not implemented. The serialized export omits `SpatialBlend`; no device audio acceptance exists.
- Rendered authored animation/IK and foot-contact appearance, and iPhone Safari behavior, remain untested.
- Local pinned SDK preflight still reports `Pinned .NET SDK 8.0.426 executable not found in known locations.` The hosted pinned runner passed.

Continue in dependency order from [the gate index](docs/GATE_DEPENDENCY_INDEX.md). Do not declare overall parity while these gates remain open; do not deploy or publish from this engineering task.
