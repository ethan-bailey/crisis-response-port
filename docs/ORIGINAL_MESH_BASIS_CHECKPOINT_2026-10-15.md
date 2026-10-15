# Original Wood exit-hole mesh coordinate-basis correction — 2026-10-15

## Original source geometry, verified by extractor inspection

Previous checkpoint incorrectly mirrored the Wood exit-hole mesh twice between the original Unity serialized geometry and the Three.js runtime.

Both original-source decode paths in `scripts/extract_original_exit_hole_mesh.py` **already** convert native Unity coordinates and winding to Three.js:

1. `unity_mesh_to_obj.py` raw source extraction outputs vertices as `(x, y, -z)`, normals as `(nx, ny, -nz)`, and faces with `(a,c,b)` winding.
2. The `compressed_mesh()` fallback also writes `vertices=[[x,y,-z] ...]` and triangle indices `(a,c,b)`.
3. The current checked-in asset `game/assets/ballistics/wood-exit-hole-source.json` explicitly says `mesh.source_format="Unity 2017 compressed mesh packed floats"`, has **186 vertices and 100 triangles**, no source UV or normal arrays, and its raw source mesh SHA is `1a6a7c334014210f536eadc6e21db582bda8f4de56be1330941bbd3ce619688e`.
4. The original *live C#* `BallisticHitMeshEffect` emits Unity `position` and `rotation`; those still require the `(x,y,-z)` world coordinate change and corresponding quaternion conversion. The **serialized local vertex buffers are already right-handed** and must not be converted again.

Old code in `game/original-impact-mesh.mjs` mirrored local source positions/normals and reversed the source triangle index order a **second time**. This was a presentation adapter mistake, **not** an original game system error. The earlier browser smoke test also compared against double-transformed source data, so it could pass while the visual handedness was wrong.

## Correction and regression, preserving the original developer data

- Commit `70c2c4e81d5ded19934c190cf069e1c7eaba8ece`: `game/original-impact-mesh.mjs` now accepts only recognized source `mesh.source_format` values, passes source extracted positions, normals and triangle indices directly into Three, and continues reflecting only live C# world transforms. No effect geometry or gameplay formula changed.
- Commit `dd44bea6993f5c0a18f453b37dfbbb8055326dda`: `scripts/weapon_smoke.mjs` now asserts direct equality to **already converted** source mesh positions, winding and normals in Chromium and WebKit, with the expected original compressed source format. It does not validate a hand-authored replacement.
- Commit `f72102ec796784b1bdf02c44856e93cc2e7002ec`: the existing full browser CI suite also runs `scripts/original_particle_source_test.mjs`, a source-only regression for the newly recovered 14 original effects and 26 particle emitters.

**Milestone CI:** [run 38814978462](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38814978462) at exact gameplay/test SHA `f72102ec796784b1bdf02c44856e93cc2e7002ec` was queued when this document was started. Check final native C#/WASM/PhysX/Chromium/WebKit results and artifact ID before claiming browser success. Do not start a duplicate run just for documentation.

## Rollback and known limitations

Selective rollback (if needed): after checking branch history and other agents, revert the specific renderer commit `70c2c4e` **together with** its browser assertion update `dd44bea`, but retain the original source asset/extractor. If the pipeline source manifest step is unrelated, do not revert it. Prefer a new explicit revert commit, never a branch reset/force-push.

Native Unity versus Three *visual* comparison remains required: accurate decoded geometry basis alone does not establish exact shader, material, lighting, depth or scene-render parity. The private owner-only site v8 is **not** updated by these branch changes. No code from recovered `runtime/Original/` was modified.
