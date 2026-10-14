# WebAssembly runtime architecture decision — 2026-10-14

## Decision

Use a **source-preserving C# WebAssembly runtime plus a browser rendering/input host**, rather than rewriting gameplay in Rust or converting the game into a different engine's native scene format.

- **Gameplay execution:** compile recovered original C# scripts into .NET WebAssembly. The existing CI spike now compiles original `Target.cs`, `BallisticMaterialProperties.cs`, and `BallisticMaterial.cs` and executes selected original methods in Chromium.
- **Unity compatibility layer:** implement only the UnityEngine API contracts required by original scripts, preserving original algorithms as source inputs. Component lookup/lifecycle, transforms, physics, time, resources, audio, UI, and persistence require real implementations and tests.
- **Browser host:** WebGL renderer, touch/pointer input, audio-unlock handling, browser lifecycle, and safe local persistence. Three.js is currently used only to validate original mesh decoding/rendering; it is not yet selected as the final renderer contract.
- **Content pipeline:** preserve original GUIDs/object IDs in a machine-readable scene manifest; convert original meshes/textures/materials only through explicit, reversible import steps.

## Why this route

1. A headless Chromium test successfully rendered 9,455 triangles from the original Marksman range combined mesh after decoding its Unity serialized buffers.
2. A .NET 8 browser WASM test compiled and executed selected methods from original C# source. The original `BallisticMaterial.ChannelVolume` and `EnergyToPenetrateSurface` methods passed deterministic tests; a further original impenetrable-entry state transition also passed.
3. Rewriting the gameplay in Rust or another engine would discard the recovered C# behavior as executable source and increase the risk of behavior drift.
4. The original Windows x86 Unity player imports native Windows APIs and is not a credible direct iPhone Safari runtime without a separate, unproven compatibility layer.
5. The licensed Unity WebGL build remains a comparison path, but the required Unity license secrets are not configured.

## Explicit limits

The current tests use minimal fixtures for projectile energy, collision, effects, time and some engine services. They prove that selected original C# methods can execute under WASM; they do not establish physics parity or a playable game. The renderer test uses a neutral material and one combined mesh, not the full scene.

The replacement is not "WebAssembly instead of an engine": WebAssembly is the execution target. The project still needs a complete compatibility runtime and renderer. Full Unity API behavior is not available automatically in WASM.

## Ordered implementation gates

1. **Source/build gate — partially passed:** keep the C# WASM smoke app compiling the selected original source files; do not include test fixtures as production implementations.
2. **Math/transform gate — basic math passed:** Vector3 angle, quaternion axis-angle rotation and local/world transform round-trip assertions passed in [run 38593586937](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38593586937). Parent/child hierarchy, scale, update order and full Unity semantics remain unimplemented.
3. **Scene graph gate:** use the extracted manifest to preserve GameObject IDs, component order, parent relationships, local transforms, and asset references.
4. **Asset gate:** render all required original mesh/material/texture dependencies; resolve the missing .308 casing mesh without substituting an unverified asset.
5. **Physics/ballistics gate:** compile the original projectile and ammunition sources; implement the needed Unity physics contracts; test raycasts, collision ordering, energy loss, penetration, deflection, and damage against reference cases.
6. **Input/UI gate:** map touch gestures to existing game actions, then validate menus, aiming, firing, reloading, interaction and pause.
7. **Audio/mission/persistence gate:** original audio, mission state, scoring, saves, campaign transitions and restart behavior.
8. **Parity gate:** compare original and port with repeatable action sequences, state snapshots and screenshots. Only mark a subsystem passed when there is evidence from both versions.
9. **Device gate:** test the hosted build on iPhone Safari for touch, audio unlock, memory pressure, orientation and tab suspension/resume.

## Evidence links

- [Original geometry browser test](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38589625850)
- [Original C# ballistics method and state test](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38592900397)
- [Unity API compatibility matrix](UNITY_API_COMPATIBILITY_MATRIX_WASM_2026-10-14.md)
- [Missing .308 casing asset issue](https://github.com/ethan-bailey/crisis-response-port/issues/5)
- [BallisticMaterial compatibility task](https://github.com/ethan-bailey/crisis-response-port/issues/6)
