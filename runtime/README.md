# Source-preserving Crisis Response runtime

This is a reusable .NET / WebAssembly compatibility runtime under development.
It replaces the disconnected fixtures used by the older ballistics spike for the
implemented scope. It is **not a complete Unity engine or a playable full port**.

## Original code

`scripts/extract_runtime_sources.py` reads the recovered AssetRipper project.
It copies complete original DecayingObject, ObjectRemover, AudioMaterial and
BuildingController components, the original InputKey code, and selected original
InputMapping, InputHandler, AmmunitionBase and Projectile members.

The 36 selected method bodies are copied verbatim. Source-file, method-body and
generated-file SHA-256 values are recorded in `Original/source-provenance.json`.
The retained properties and field initializers are also extracted from the source.
Serialization annotations and unrelated UI/platform features are explicitly
excluded. Recovered source is decompiled C#, not the developer's original project.

Do not fill omitted gameplay dependencies with invented replacement behavior.
The partial Projectile/AmmunitionBase types expose selected original calculations;
they do not execute firing, flight, collision, fragmentation spawning or damage.

## Implemented services

- A shared object/component registry; type-aware component lookup; object identity;
  Unity-style destroyed-object null comparison; queued end-of-frame destruction.
- Parent/child transforms preserving serialized child order, local position,
  quaternion rotation and scale; world/inverse conversions; cycle checks and
  reparenting. Sheared reparenting that cannot be decomposed fails explicitly.
  RectTransform spatial records are imported, but UI anchoring/layout is not implemented.
- Supported Awake, OnEnable, Start, FixedUpdate, Update, LateUpdate, OnDisable and
  OnDestroy dispatch. Cross-script Unity execution-order parity is not verified.
- Scaled/unscaled time, fixed-step accumulation, pause, and maximum allowed timestep.
  Scene time settings are read from the original TimeManager asset.
- Raw key/axis state delivered to the recovered original InputHandler.UpdateKeys.
  Original InputKey behavior handles press/release, short/long presses, handled
  flags, suspension, universal controls and category enable/disable. Original
  binding UI, settings migration and numeric KeyCode serialization are excluded.
- Oriented and scaled BoxCollider ray queries using the imported hierarchy;
  nearest-hit selection, layer masks, trigger filtering and active/enabled state.
  This is query geometry, **not rigidbody dynamics or PhysX parity**. Equal-distance
  query ordering and collider-boundary cases still need original-engine comparison.
- IEnumerator coroutine nesting, scaled/unscaled waits, and cancellation. Other
  Unity yield instructions fail explicitly; exact Unity coroutine phase parity
  remains unverified.
- Unweighted cubic Hermite AnimationCurve evaluation using original key values and
  incoming/outgoing tangents. Weighted curves, duplicate key times and multi-key
  extrapolation are unsupported. Native infinity metadata is preserved without
  guessing its behavior. This currently limits high-deformation projectile cases.

## Original data extraction

`scripts/extract_runtime_scene.py` follows serialized references starting from
Marksman range, Mission 01, the Operator prefab, project time/layer settings, and
all 50 ammunition resource prefabs. It produces:

- `scene-package.json`: full source text, raw YAML records, parsed fields, original
  fileIDs/GUIDs, hashes and a dependency index.
- `scene-execution.json`: the same parsed fields and identifiers without duplicate
  raw text, for execution. The browser verification sends only the requested asset.
- `scene-package.report.json`: extraction counts and unresolved GUID count.

The expanded extraction contains **818 assets and 20,952 serialized records**.
All 55 seed asset source texts were checked against the recovered source bytes.
31 unresolved GUID *references* are reported; this is not 31 distinct missing assets.
Serialized dependencies outside the current executable scope remain inventoried.
All serialized reference fileIDs are decimal strings in the execution view so
large Unity prefab identifiers survive JavaScript without integer rounding.

`SceneLoader` reconstructs both scene object hierarchies and supported components:

| Scene | GameObjects / transforms | Box colliders |
| --- | ---: | ---: |
| Marksman range | 698 | 348 |
| Mission 01 | 1,526 | 629 |

These scenes currently have **zero activated original gameplay components** in the
supported import registry. Their BallisticMaterial, Target, Mission, Room and other
script records are reported as unsupported; they are not silently replaced.
The executable original components and input are verified separately in stated
runtime scenarios. The partial scene graph is not a playable scene.

## Verification and builds

`Checks/Crisis.Runtime.Checks.csproj` runs nine runtime contract groups and imports
both original scene hierarchies. It also configures the selected original Projectile
methods from original 5.56-JHP and .308-Win-BSP ammunition prefabs. Firing and
projectile initialization are a test configuration fixture, not gameplay execution.

The original 5.56-JHP energy test starts at approximately 1,806.25 J, removes 2.5%
through original ConvertEnergyToFriction, and retains approximately 1,761.09 J.
The .308 BSP test starts at approximately 3,702.73 J and retains approximately
3,610.16 J. Original penetration functions are checked as inverse calculations.
This verifies original formula execution with extracted configuration; it does
not verify the original game's collision or damage outcomes.

`.github/workflows/runtime-foundation.yml` regenerates from the private recovery
artifact, compiles the runtime, runs native checks, builds Blazor WebAssembly and
executes original input plus scene/ammunition checks in Chromium and WebKit at
852×393 CSS pixels. Build, source provenance, extraction and browser evidence are
saved as a private Actions artifact. Linux WebKit is not an actual iPhone 16 test.
The scene checks also raycast against actual imported original BoxCollider records,
retaining the tested source fileID in their reports.

## Remaining runtime gates

1. Restore full prefab/resource instantiation and reference binding for the required
   gameplay dependency closure. The current Instantiate API throws.
2. Implement and compare original rigidbody/collider/joint behavior before executing
   CharacterBody, its ragdoll and character collision world.
3. Implement renderer/material, Animator/animation, IK, audio, UI and UnityEvent
   services required by those scripts.
4. Execute complete original weapon/bolt/projectile/material and physiology systems
   together. The formula-only Projectile slice must not substitute for that chain.
5. Run original mission/AI/interaction/saving code and repeatable original-versus-port
   behavioral comparisons, followed by actual iPhone Safari testing.

No full-game parity result is claimed. The existing JavaScript prototype is not
automatically connected to this runtime by this change.
