# Browser port strategy pivot — 2026-10-14

## Decision

Do not treat Unity Editor activation as the only path to a playable browser version. Keep the recovered Unity project as valuable reverse-engineering evidence, but investigate browser-native source-port / compatibility-runtime approaches inspired by recent game preservation projects.

Target user experience: open a URL in iPhone Safari and play the original Crisis Response, with original assets and behavior preserved wherever feasible. No user-side compilation or technical setup.

## What the example projects actually do

- [GTA3-WASM](https://github.com/Antriksh1984/GTA3-WASM) compiles the reverse-engineered re3 C++ engine to WebAssembly. It does not convert a random Windows game automatically.
- [reVC Web](https://github.com/origami-ltd/wasm-revc) ports an already decompiled/source-available C++ engine to WebAssembly, retaining the game's own logic and loading user-supplied game data.
- [Wine-Assembly](https://github.com/vgrichina/berrry-wine) runs some unmodified Windows binaries in a browser through an x86 interpreter and Win32 compatibility layer. Its documented focus is Windows 98-era programs and DirectX-era compatibility; this is not evidence that a Unity 2017 3D game will run.
- The wasm.ltd project reports a WINE/PROTON-in-WebAssembly path for Dino Crisis. This is a potentially relevant compatibility-runtime lead, but support for Crisis Response's Unity 2017, Direct3D, audio, threading, and 32-bit dependencies is unverified.

These projects suggest two distinct techniques: (A) port/rebuild a recovered engine for WebAssembly, or (B) run the original executable under a browser-hosted compatibility layer. Neither is an automatic converter.

## Crisis Response facts that constrain the choice

- The original is a 32-bit Windows x86 game using Unity 2017.2.0f3 and the Mono scripting backend.
- The recovered artifact contains the original-matching `Assembly-CSharp.dll`, 1,333 decompiled gameplay C# files, and an AssetRipper-reconstructed project with 7,231 exported files.
- The Unity project has not been compiled successfully for WebGL or tested in Safari.
- The decompiled C# scripts rely on Unity engine APIs and recovered Unity scene/prefab/asset serialization. They cannot simply be passed to Emscripten as if they were portable C++.
- The game appears to be a 3D tactical simulation with AI, physics, animation/ragdolls, ballistics, doors, audio, and mission state. Recreating all Unity engine behavior in a new runtime is a large project and risks parity loss.

## New priority order

### Track A — feasibility spike for original executable compatibility (first, time-boxed)

Evaluate the current browser WINE/PROTON projects and their actual source/build instructions. Determine whether the runtime can plausibly execute a 32-bit Unity 2017 Windows player with its graphics, audio, and threading dependencies on iPhone Safari.

Evidence required before committing to this track:
1. Confirm the runtime actually supports PE32 x86 binaries and the required Windows APIs.
2. Confirm support for the Direct3D version/backend used by the game and translation to WebGL/WebGPU.
3. Confirm Unity 2017 Mono runtime and required .NET/native DLL dependencies can load.
4. Confirm input, audio, threading, memory, and filesystem behavior are viable in Safari.
5. Build a minimal smoke test using the user's game files in a private cloud runner; do not upload proprietary game assets to a public demo or repository.
6. Record exact failures. If graphics/runtime compatibility is fundamentally missing, stop rather than sink effort into speculative glue.

This track best preserves the original executable if it works, but feasibility is uncertain and performance on iPhone may be poor.

### Track B — native WebAssembly source port / compatibility engine (in parallel as research, primary fallback)

Use the recovered C# decompilation, metadata, scenes, prefabs, and assets as behavioral evidence. Do not assume a native engine port exists. Inventory the Unity APIs and engine features used by the game; prototype a very small vertical slice before selecting a runtime.

The first vertical slice should load one recovered scene and demonstrate:
- scene/camera/rendering,
- a player character with movement and collision,
- one weapon/projectile interaction,
- one AI character,
- one door interaction,
- audio,
- a touch-friendly input scheme.

Only after that slice can run in Safari should work expand subsystem by subsystem. Preserve original assets and data formats wherever possible; document any replaced engine behavior and every known parity gap. The recovered C# code is the reference, not permission to claim behavior is preserved before testing.

Potential runtimes/approaches (evaluate, do not preselect): a source-level engine port to C/C++ with Emscripten, a carefully chosen WebAssembly-capable engine, or a thin compatibility layer around extracted game data. A Rust rewrite is not the default; use it only where a concrete subsystem justifies it.

### Track C — Unity WebGL build (fallback, not a hard gate)

Keep the current Unity WebGL workflow available as an option if a valid Unity license is obtained. Do not make Unity activation the sole project blocker, and do not misrepresent a skipped build as success.

## Immediate next actions

1. Inspect the source and documented requirements of the WINE/PROTON-in-WebAssembly lead and map them against Crisis Response's PE architecture, Unity/Mono runtime, graphics API, and native DLLs.
2. Inspect the recovered executable's player DLLs and graphics/audio dependencies to make that compatibility matrix evidence-based.
3. Create a cloud-runner smoke-test branch or workflow only after confirming the runtime can be built and its licensing permits the intended personal-use experiment.
4. If Track A fails the compatibility gate, create the smallest possible Track B browser prototype based on recovered original assets and verified behavior.
5. Publish only after a real browser build exists. Test on iPhone Safari and keep runtime/performance and behavioral-parity gates explicit.

## Success criteria

Success means Crisis Response itself launches and can be played from a browser URL on iPhone, not merely that source files were extracted, a demo UI loads, or a compiler step reports success. Keep the first goal narrow: reach a playable mission slice, then expand toward full parity.

## Current status

This document changes the project strategy; it is not evidence that either browser runtime already runs Crisis Response. The original game archive and recovered artifact remain the source of truth. No game binaries or proprietary game assets are added to the public repository.
