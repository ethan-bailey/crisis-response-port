# Marksman range scene dependency audit — 2026-10-14

## Scope and method

Static inspection of the recovered AssetRipper project at `Assets/Scenes/Marksman range.unity`. This is a data-level inventory only; it does not execute Unity, validate references in the original editor, or prove runtime parity.

## Scene measurements

- Scene YAML size: 1,475,238 bytes.
- GameObject records: 698.
- Transform records: 586.
- MonoBehaviour records: 646.
- MeshRenderer records: 302.
- MeshFilter records: 302.
- Unique GUID references in scene text: 129, across 1,910 GUID occurrences.
- Two non-built-in GUIDs were not resolved by the direct scene scan. The recursive scan found seven unresolved non-built-in GUIDs across the scene's reachable serialized assets. These need explicit investigation before claiming the dependency closure is self-contained.

The cloud dependency workflow now recursively traverses the scene and readable serialized dependencies. The latest run reached 417 assets and 708 dependency edges, with seven unresolved non-built-in GUIDs after classifying the common Unity built-in GUIDs correctly. See [the successful workflow run](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38587439160) and its `crisis-response-marksman-range-dependencies` artifact for the generated full graph. This remains static analysis: the scene is not imported by a running engine, and the report does not establish runtime behavior.

The scene is not a tiny test map. It is a substantial, serialized Unity scene with many script components, renderable geometry, and repeated material/prefab references. A generic canvas mockup would not be a meaningful port test.

## Highest-frequency resolved dependencies

- `Scripts/Assembly-CSharp/BallisticMaterial.cs` — 253 references.
- `Mesh/Combined Mesh (root_ scene)_0.asset` — 208 references.
- `GameObject/Wood.prefab` — 102 references.
- `GameObject/Bricks.prefab` — 95 references.
- `Material/Brown.mat` — 75 references.
- `Scripts/Assembly-CSharp/BallisticWindow.cs` — 69 references.
- `GameObject/Window glass.prefab` — 69 references.
- `Texture2D/window glass.png` and `Texture2D/glass hole.png` — 69 references each.
- `Material/Window glass.mat` — 68 references.
- `Material/Brick wall red.mat` — 47 references.

These are serialized GUID occurrence counts, not runtime execution counts. In particular, the high count for `BallisticMaterial` means material/ballistics metadata is attached throughout the scene; it does not by itself tell us the cost of implementing the behavior.

## Scene script dependencies identified from GUIDs

- `GlobalContextInitializer.cs`
- `Mission.cs`
- `BallisticMaterial.cs`
- `BallisticWindow.cs`
- `DEPRECATED_LightController.cs`
- `Target.cs`
- `PaintableSurface.cs`
- `Room.cs`
- `MarksmanshipTutorialTarget.cs`
- `Marksmen.cs`
- `UIInteriorModeTextIndicator.cs`
- `AudioSourcePriority.cs`
- `PoweredLight.cs`
- `LightFlasher.cs`
- `UIClickAudio.cs`
- `MissionEndMenu.cs`
- `ScoreKeeper.cs`
- `TimeScaler.cs`
- `TutorialMode.cs`
- `UIResourceCounter.cs`
- `MissionUI.cs`
- `IntroParagraphs.cs`
- `UIScoreBreakdown.cs`
- `IndicatorArrow.cs`
- `UIHitAnnouncements.cs`
- `MarksmanshipTutorial.cs`
- `UIMissionEndOverlay.cs`
- `TextFader.cs`
- `UIMissionStatus.cs`
- `OccupantSpawner.cs`

This is a first pass from scene GUID references; prefab dependencies can add further scripts and assets.

## Consequences for an engine replacement

1. **Do not begin by rewriting the whole game.** Start with a dependency closure for this scene: scene → prefabs → scripts → materials/shaders → meshes/textures → audio → physics and UI.
2. **Preserve serialized identity.** Retain GUIDs, object IDs, original names, and references in a machine-readable intermediate representation. Avoid lossy manual conversion to a different scene format before references are resolved.
3. **Ballistics are central to this scene.** Inspect `BallisticMaterial.cs`, `BallisticWindow.cs`, `PaintableSurface.cs`, `Target.cs`, and their callers together. Rendering a room alone would not demonstrate gameplay compatibility.
4. **Renderer/physics parity must be explicit.** A WebGL/WebGPU renderer and a browser physics engine may look similar but will not automatically reproduce Unity 2017 rendering, collision ordering, raycasts, or fixed-step timing.
5. **Touch input is a separate adapter.** Preserve original action semantics; map touch controls to existing gameplay methods rather than rewriting those methods around a new control scheme.
6. **Resolve missing GUIDs first.** Determine whether each is an AssetRipper omission, an external package asset, or a built-in Unity reference before attempting runtime conversion.

## Next action

Produce a recursive dependency graph for the Marksman range scene and inspect the original ballistics/target code, then select one representative original object to drive the first browser/WASM experiment. No compilation or browser execution has been performed by this audit.
