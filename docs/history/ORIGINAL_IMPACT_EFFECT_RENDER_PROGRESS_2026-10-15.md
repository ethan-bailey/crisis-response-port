# Original impact effect source asset recovery and browser presentation — October 15, 2026

## Scope

Scoped **presentation adapter** on draft PR #9 (`codex/projectile-material-impacts`). This effort does **not** modify recovered C# BallisticMaterial, BallisticWindow, Projectile, BallisticHitMeshEffect, or the game rules, and does **not** touch the independent Unity WebGL workstream.

## Verified original assets

GitHub Actions [read-only audit 38796054686](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38796054686), metadata-only private artifact `source-effect-render-reference-audit`, verified the original private release ZIP SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.

From the recovered Mission 01 material properties, there are **12 distinct material-property source assets** referencing **15 distinct ballistic hit effect prefabs**:
- 7 entry effect prefabs, 7 exit effect prefabs, 1 exit-hole mesh prefab;
- 26 ParticleSystem components, with original mesh/material references resolved;
- 1 MeshFilter + 1 MeshRenderer, for the source **Wood exit hole.prefab**.
- Zero unresolved effect prefab files or linked mesh GUIDs in this inventory.

The exit-hole prefab references the exact `Assets/Mesh/Circle.asset` and `Assets/Material/Wood exit hole.mat` files. Source GUIDs:
- prefab `c0bb8365c7bd54b4597570f085f5a153`
- mesh `40980728ea312204c9b6c189d71e6dfa`
- material `ec9ffb378c2bcf945b24cd5c030325e3`

GitHub [recovery extraction run 38797133282](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38797133282) saved **only the exact decoded source** in the private branch as `game/assets/ballistics/wood-exit-hole-source.json`. The recovery ZIP and material/mesh SHA256s were checked. The source Circle mesh has **186 vertices and 100 triangles** and serialized compressed vertex positions; this mesh does not carry UVs or vertex normals, and its material has no source sampled texture. No UV coordinates or geometry were fabricated. Its original material color is `_Color=(0.6911765,0.5285467,0.28460205,1)`, metallic 0 and glossiness 0.5.

## Presentation integration

- `game/original-impact-mesh.mjs`: loads the source JSON, fails closed if GUID/hash/topology does not match, converts the original Unity mesh and world pose into THREE coordinates, and creates meshes **only** when the original C# runtime emits an `impactMesh` event with the exact exit-hole prefab GUID.
- `game/weapon-runtime.mjs`: passes existing original effect events to the dedicated adapter without changing shot, collision, energy or effect timing.
- `game/presentation.mjs`: exposes source mesh load/count diagnostics.
- `scripts/weapon_smoke.mjs`: dedicated Chromium/WebKit assertion for exact 186/100 topology, C# event pose, and disappearance when C# stops emitting that original effect instance.

The source C# `BallisticHitMeshEffect.Play` and `Update` remain authoritative for target scale and animation timing. Adapter buffers and source color are from the recovered assets, **not** substitute gameplay effects. Generated mesh normals, THREE PBR shader approximation and flattened world/local scene-parent transforms are renderer-specific boundaries, **not verified shader or original Unity visual parity**.

## Remaining work and limitations

The **26 original source particle emitters** are inventoried but not simulated or rendered in the browser. Other glass-alpha texture stamping still affects native collision logic but is not yet bound to the combined gray scene mesh. No substitute particles, glass visuals or physics/damage rules were invented.

This change has not been privately published to the existing owner-only playable Site, and has not been physically tested on iPhone. Confirm the new browser tests and preserve owner-only hosting before calling it playable.

Other porting limitations remain as documented in `IMPACT_CHECKPOINT.md` and `NEXT_AGENT.md`.
