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

The corrected recursive asset-reference audit reached 417 readable serialized assets and 708 dependency edges. It now reports one unresolved serialized asset GUID: `b4b69dc6c74eeb54cbd22d28e37f9a19`, referenced by the .308 casing/projectile prefabs. A similarly named casing mesh exists with a different GUID, so no substitution has been made. See [issue #5](https://github.com/ethan-bailey/crisis-response-port/issues/5).

## C# WebAssembly runtime gate: first two passes passed

In [workflow run 38590382729](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38590382729), a .NET 8 Blazor WebAssembly app compiled the original decompiled `Target.cs`, ran in Chromium, and invoked its original private `Awake` method. The test passed the component attachment/GetComponent behavior and reported zero browser errors. The `CharacterBody` type was a test fixture, and the shim implemented only a tiny part of UnityEngine; this is not a playable slice or a parity claim.

The second pass, [workflow run 38590980350](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38590980350), also compiled original `BallisticMaterialProperties.cs`, checked its default material values and ran in Chromium with zero errors. It used minimal `Vector2` and inspector-attribute shims; `CharacterBody` and ballistic effect classes were fixtures, not the original game classes.

## C# WebAssembly ballistics method gate: passed

In [workflow run 38591962729](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38591962729), the original `BallisticMaterial.cs` compiled into browser WASM without rewriting its algorithm. Chromium invoked the original static `ChannelVolume(2, 4, 6)` method and verified the expected result within tolerance. The Unity math/transform APIs and dependent game classes were minimal test fixtures; this proves execution of that one original method, not full ballistic physics.

## C# WebAssembly surface clamp gate: passed

In [workflow run 38592470882](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38592470882), the original `EnergyToPenetrateSurface` method passed a deterministic surface-thickness clamp test. The test projectile’s `EnergyToPenetrate` function is still a simplified fixture, so this validates the original wrapper/clamp logic, not the game’s true energy model.

## Current expansion: original impenetrable-entry state transition

The current run, [38592900397](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38592900397), exercises the original `BallisticMaterial.HandleProjectile` path through an impenetrable surface and checks the expected stop/zero-energy state transition. The engine math, projectile energy model, effects, and timing are still test fixtures; a pass is a limited source-method test, not full ballistic or gameplay parity.

The broader API inventory and implementation gaps are recorded in [the Unity API compatibility matrix](UNITY_API_COMPATIBILITY_MATRIX_WASM_2026-10-14.md). Unity's licensed build path remains blocked by missing license secrets. No full-game browser build exists, and no 1:1 parity claim is justified yet.
