# Browser geometry and WebAssembly feasibility status — 2026-10-14

## Completed: original geometry in browser WebGL

Workflow run: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38589625850

The exporter read the original AssetRipper mesh asset referenced by the Marksman range scene, `Assets/Mesh/Combined Mesh (root_ scene)_0.asset`, and decoded its serialized vertex/index buffers without substituting geometry.

Validated conversion:
- 14,187 source vertices, 48-byte vertex stride.
- 28,365 triangle indices; 9,455 triangles.
- 274 submeshes.
- Position and normal data plus UV0 were present.
- Index ranges, buffer lengths, and maximum vertex index passed validation.
- Conversion to OBJ applied a Z-axis sign change and reversed triangle winding to account for coordinate handedness.

A headless Chromium run loaded that OBJ through Three.js/WebGL and reported:
- 274 rendered mesh groups.
- 28,365 expanded OBJLoader vertices.
- 9,455 triangles.
- A 1280×800 WebGL canvas.
- Zero page or console errors.

The browser screenshot and JSON smoke result are in the `crisis-response-browser-geometry-smoke` artifact from the workflow run. The OBJ and conversion report are in `crisis-response-original-mesh-export`.

## Important scope boundary

This proves one original combined mesh can be decoded and displayed in a browser. It does **not** prove the whole Unity scene loads, original materials/shaders/lightmaps are preserved, or animation, physics, input, audio, AI, UI, saves, and mission flow work. The current viewer deliberately uses a neutral material and does not execute original gameplay. This step uses WebGL and JavaScript; it is not yet a WebAssembly game engine.

## Scene manifest and unresolved asset

The static scene manifest contains 3,160 serialized records, 698 GameObjects, 302 MeshFilters, 302 MeshRenderers, and 646 MonoBehaviour records. 470 GameObjects contain at least one MonoBehaviour component according to the heuristic extraction.

The corrected recursive asset-reference audit reached 417 readable serialized assets and 708 dependency edges. It now reports one unresolved serialized asset GUID: `b4b69dc6c74eeb54cbd22d28e37f9a19`, referenced by the .308 casing/projectile prefabs. A similarly named casing mesh exists with a different GUID, so no substitution has been made.

## C# WebAssembly runtime gate: first pass passed

In [workflow run 38590382729](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38590382729), a .NET 8 Blazor WebAssembly app compiled the original decompiled `Target.cs`, ran in Chromium, and invoked its original private `Awake` method. The test passed the component attachment/GetComponent behavior and reported zero browser errors. The `CharacterBody` type was a test fixture, and the shim implemented only a tiny part of UnityEngine; this is not a playable slice or a parity claim.

## Current expansion: material properties component

A second iteration is now testing the original `BallisticMaterialProperties.cs` alongside `Target.cs`. It adds a minimal `Vector2` and Unity inspector attributes, then checks the original material-property defaults in browser WASM. The `BallisticHitEffect` and `BallisticHitMeshEffect` classes remain test fixtures. Current run: https://github.com/ethan-bailey/crisis-response-port/actions/runs/38590980350.

The next real gate after this is a behavior-bearing subsystem whose required Unity APIs can be implemented and tested without replacing the original gameplay algorithm. The complete Unity API and scene dependency surface still needs substantial implementation. Unity compilation remains blocked by missing license secrets, and no full-game browser build exists.
