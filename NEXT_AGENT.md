# Crisis Response Port — current handoff

## Live source and this session

- Integrated branch: `develop/wasm`; verified remote and local base HEAD `4a9c52ad522064c247d722d7b8d998e34485be27`, tree `226176a0265bf2c4055a72444d1006bbb67907c5`.
- The protected untracked file `scene-data/.crisis-response-scenes.json.gz.PSfVGg` was present in the checkout and left untouched.
- This session adds a post-contact movement bookkeeping call: after browser PhysX pose/velocity synchronization, the bridge reuses original `CharacterBody.UpdateMovement()` so the next source animation update sees displacement from the solved contact pose. `git diff --check` passed. Compilation and browser acceptance are pending.
- Local pinned-SDK preflight result: `Pinned .NET SDK 8.0.426 executable not found in known locations.` The runner could not compile this change.

## Verified runtime and open acceptance

- Latest hosted full-profile acceptance remains Actions run #157 (`40572335603`), tested source SHA `3a33a6df7d74681cf4074976ec9ca9d3845a56ea`; it predates this session's change. Packaged Mission input moved the actor and selected the original backward-walk state, but that run did not prove authored rendered animation or visual contact parity.
- Particle command transport and source dependency inventories exist. Source-material/mesh revision mismatches, generic particle rendering, and particle collision callbacks remain unresolved; do not bind mismatched assets or invent simulation output.
- Browser contact uses PhysX 5.6.1 rather than the game's Unity 2017 PhysX version. Spatial audio/reverb output and iPhone Safari acceptance also remain open. No Site deployment was made.

## Next action

Compile and run the exact-source packaged Mission movement/contact acceptance on the new branch head when the pinned runner is available. Then continue Gate 7 using the dependency order and source/hash restrictions in the [gate index](docs/GATE_DEPENDENCY_INDEX.md).
