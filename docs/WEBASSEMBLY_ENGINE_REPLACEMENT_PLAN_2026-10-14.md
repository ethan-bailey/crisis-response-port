# WebAssembly engine replacement plan — 2026-10-14

## User requirement

Replace the Unity engine with a browser/WebAssembly runtime for iPhone Safari while retaining the original Crisis Response game, original assets, and 1:1 behavior wherever possible. Do not substitute a newly designed game or present a prototype as the original. Keep known parity gaps explicit.

## Current verified baseline

- Original player: 32-bit Windows x86, Unity 2017.2.0f3, Mono scripting backend.
- Recovery pipeline: original-matching `Assembly-CSharp.dll` was verified by SHA-256 against the archive; ILSpy decompilation and AssetRipper export are available in the recovery artifact.
- Reconstructed project: 7,231 files; six scene assets and six enabled scenes in build settings.
- Current CI preflight: 1,420 C# files scanned across the recovered project. The latest scan completed successfully, but it did not compile or execute the game.
- In the recovered main gameplay script directory alone, a quick inventory found 581 C# files and about 1.57 million source characters.
- Main-script API-use counts from the current static inventory (raw occurrence counts, not unique call sites): MonoBehaviour 363, GameObject 361, Transform 407, Rigidbody 125, Physics calls 47, ParticleSystem 45, AudioSource 128, Camera 58, Unity UI references 141, Input references 75, Time references 527, Resources references 40, Instantiate calls 152, Destroy calls 183.
- No verified playable browser build exists. The Unity WebGL build job is skipped because valid Unity licensing secrets are not configured.

## Important technical correction

WebAssembly is an execution target, not a drop-in game engine. The original gameplay assembly is tightly coupled to UnityEngine types, Unity scene/prefab serialization, Unity lifecycle callbacks, physics, rendering, animation, audio, UI, asset loading, and input. Compiling the decompiled C# files to WASM alone will not preserve behavior.

A replacement must provide either:
1. A compatible engine/runtime capable of loading the original Unity content and implementing the Unity APIs used by this game; or
2. A port of the gameplay layer plus replacement implementations for each required engine service.

A general-purpose x86/Windows compatibility layer is a separate experiment: it could preserve the original binary better, but it is not the same as replacing Unity with a native browser engine, and its support for this Unity 2017 player remains unverified.

## Chosen engineering approach

Use a **source-preserving, API-compatibility-first port**, with a narrow vertical-slice gate before expanding scope.

- Keep the recovered DLL, decompiled C# sources, metadata, scenes, prefabs, textures, models, audio, animation data, and original configuration as immutable reference evidence.
- Do not rewrite gameplay systems unless a specific engine dependency prevents compilation or runtime behavior. Any adapter must preserve the original public behavior and be documented.
- Prefer the highest-reuse route proven by experiments. Do not assume a Rust rewrite, a new engine, or a compatibility runtime is viable until the spike provides evidence.
- Keep the existing licensed Unity WebGL workflow as an optional comparison path, not the only path.
- Do not upload original game binaries/assets to a public demo or public repository.

## Vertical slice gate: Marksman range

Start with the recovered `Marksman range` scene, then proceed to Mission 01 only after the first slice passes.

The slice must load original scene data and original assets and demonstrate, in the browser:
1. Scene load and correct camera framing.
2. A character using the original model/material/animation data.
3. Touch movement/aim/fire controls on iPhone Safari.
4. Weapon/projectile or hitscan behavior matching the original source.
5. One target/AI behavior and damage feedback.
6. At least one original door/interactive object where present in the scene.
7. Original audio playback.
8. Scene reset and stable state over repeated attempts.

The scene name alone is not enough: inspect the scene hierarchy and dependencies first, and record any missing/reconstructed assets.

## Engine API inventory and compatibility gates

Before choosing a replacement engine, map original API usage into a versioned matrix:

- Object/lifecycle: MonoBehaviour, GameObject, Transform, Instantiate, Destroy, component lookup, scene loading.
- Simulation: Rigidbody, Collider, Physics queries, collision callbacks, time/delta-time and fixed-update ordering.
- Graphics: cameras, render pipelines/shaders/materials, lighting, particles, image effects.
- Animation: Animator/state machines, legacy Animation, ragdolls and transforms.
- Audio: AudioSource, AudioMixer and spatial playback.
- UI: Unity UI, EventSystem, raycasts, text, buttons and layout.
- Content: Resources, asset references, prefab/scene serialization and any AssetBundle dependencies.
- Input: keyboard/mouse assumptions and touch controls.
- Platform APIs: WebRequest/threading, Registry, filesystem/file browser, BinaryFormatter save/load, and any native imports.

Each matrix entry must state: original call sites, runtime implementation, test evidence, known difference, and severity. Static match counts are leads, not confirmed runtime blockers.

## Parity protocol

- Establish baseline recordings/screenshots and repeatable action sequences from the original game where feasible.
- Compare scene hierarchy and transforms, movement, collision outcomes, weapon results, AI decisions, mission state, UI transitions, audio events, save/load behavior, and restart behavior.
- Track each subsystem as Not assessed / Implemented / Tested / Parity gap. Do not mark 1:1 based only on compilation or visual resemblance.
- Preserve original assets and constants; log every conversion and any lossy import.
- Test the actual hosted build on iPhone Safari, including touch, audio-unlock rules, memory pressure, orientation, and interrupted/resumed tabs.

## Immediate work order

1. Build an evidence-based Unity API/call-site inventory from the recovered project and classify systems by vertical-slice dependency.
2. Inspect the `Marksman range` scene, prefabs, referenced scripts, materials, shaders, animation controllers, and audio dependencies.
3. Choose one minimal rendering/runtime experiment based on those dependencies; test it with original scene/assets rather than a mock scene.
4. Record whether the experiment can parse the original data, render one original object, and run one original gameplay method. If it cannot, capture the exact failure and reassess before broad implementation.
5. Do not claim a compiler log until a real compiler has run. Do not claim a playable build until it has launched and been tested in a browser.

## Current blockers and boundaries

- No local Unity Editor/compiler is available in the current environment.
- The GitHub Actions Unity build gate has no valid license secrets, so Unity compilation is currently skipped.
- A replacement engine and its runtime are not yet selected or proven.
- 1:1 parity is a target, not a guarantee. Full parity may require reimplementing a substantial part of Unity's engine behavior; the vertical slice is intended to measure that cost early.
