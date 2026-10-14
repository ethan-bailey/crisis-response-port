# Unity API compatibility matrix — WebAssembly replacement

This matrix records actual evidence from the source-preserving browser runtime experiments. An API is not marked implemented merely because it appears in source or has a similarly named type in another engine.

## Verified by a browser WASM test

| API / behavior | Evidence | Scope / limitation |
|---|---|---|
| `MonoBehaviour` base type | Original `Target.cs` compiled in .NET 8 Blazor WebAssembly | Minimal test shim only; no Unity lifecycle scheduler |
| `GetComponent<T>()` | Original private `Target.Awake()` was invoked in Chromium and cached the attached test component | Single-component fixture; no real scene/component registry |
| `TooltipAttribute`, `HeaderAttribute` | Original `Target.cs` and `BallisticMaterialProperties.cs` compile against shim attributes | Inspector metadata only; no Unity Inspector behavior |
| `Vector2` | Original `BallisticMaterialProperties.cs` compiled and its default vector was checked | Minimal two-float struct; no complete Unity math API |
| Original `Target.active` default and `BallisticMaterialProperties` defaults | Browser WASM test assertions | Does not execute ballistic physics or target damage |

First source-execution proof: [workflow run 38590382729](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38590382729). Expanded test: [workflow run 38590980350](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38590980350).

## Required engine-service implementations — not yet implemented

| Service | Evidence in original scene/project | Required parity work |
|---|---|---|
| Object graph and lifecycle | 698 GameObjects, 586 Transform records, 646 MonoBehaviour records in Marksman range | Preserve serialized IDs/references; Awake/OnEnable/Start/Update/FixedUpdate/OnDisable/OnDestroy order; instantiate/destroy semantics |
| Transform/math | Parent/child transform hierarchy, world/local conversions and rotations throughout source | Vector/quaternion/matrix math, parent propagation, coordinate conventions and numerical comparison |
| Physics and collision | 348 BoxCollider and 22 Rigidbody records in the scene; source uses raycasts and collision callbacks | Collider geometry, broadphase, raycast order, trigger/collision callbacks, rigidbody integration, fixed-step timing |
| Meshes and materials | 302 MeshFilters and 302 MeshRenderers; static combined mesh conversion passed | Preserve mesh/submesh/material mapping, original shaders, texture color space, transparency, normals, lightmaps and render ordering |
| Ballistics and damage | 253 direct scene references to `BallisticMaterial.cs`, 69 to `BallisticWindow.cs`; original projectile logic depends on physics, transforms and material data | Run original algorithm unchanged where possible; implement required Unity services and test penetration, deflection, hit events and damage outcomes |
| Animation and ragdolls | Animator/animation references across project | State-machine behavior, bone transforms, blend timing, ragdoll physics and animation events |
| Audio | AudioSource references and original mixer asset | Mixer routing, spatial attenuation, timing, audio unlock policy and mobile lifecycle |
| UI and events | 112 RectTransform records, 77 CanvasGroup records, Unity UI package references | Layout, EventSystem/raycast semantics, input focus, text, menus and touch event ordering |
| Input | Keyboard/mouse polling in original scripts; no verified touch adapter over gameplay actions | Map touch controls to existing action methods, preserve aim/fire/reload/interaction semantics |
| Time/coroutines | Time/delta-time usage throughout source | Scaled/unscaled time, fixed update, coroutine yield semantics and deterministic ordering |
| Content/persistence | Resources, prefab references, BinaryFormatter, filesystem paths and registry usage | Original resource IDs and scene loading; safe browser-compatible saves/import/export without changing game state semantics |
| Network/platform services | Legacy WebRequest/threaded calls, Windows Registry/filesystem assumptions | Isolate ancillary calls, avoid mixed-content/CORS issues, remove desktop-only dependencies behind adapters |

## Asset integrity gate

The recursive dependency audit reached 417 readable serialized assets and 708 dependency edges. One unresolved asset GUID remains: `b4b69dc6c74eeb54cbd22d28e37f9a19`, used by .308 casing/projectile prefabs. Do not silently map it to a similarly named mesh. Track it in [issue #5](https://github.com/ethan-bailey/crisis-response-port/issues/5).

## Next gate

Expand from test fixtures to a behavior-bearing original subsystem only when its dependency closure is explicit. Keep original algorithms as source inputs, and test each shim contract against original behavior. The static mesh viewer and the tiny C# WASM fixture are feasibility gates, not a playable game. No full-game browser build or 1:1 parity claim exists yet.
