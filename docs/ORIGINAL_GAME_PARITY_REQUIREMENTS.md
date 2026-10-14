# Non-negotiable original-game parity requirement

The deliverable is a copy/port of the original *Crisis Response*, not a clone, homage, simplified substitute, or game that merely looks similar. Intended use is personal. This does not relax the fidelity requirement.

## Required preservation

1. Treat the recovered original Assembly-CSharp.dll, its ILSpy decompilation, original serialized Unity scenes/prefabs, and exported assets as source-of-truth evidence. Preserve immutable hashes and recovery artifacts.
2. Execute original decompiled gameplay code wherever feasible. Do not replace existing game rules with invented mechanics just to get a demo running.
3. Reconstruct original Unity engine services required by the source: object/component lifecycle, transforms, scene/prefab deserialization, physics and collision ordering, time, animation, rendering/materials, audio, UI, input, resources, persistence, and platform services.
4. Use original meshes, textures, materials, animation, audio, scene data, UI assets, constants, and configuration. Any conversion must be recorded and checked for losses.
5. If an engine service cannot yet be reproduced, mark the dependent system incomplete rather than substituting a superficially similar behavior.
6. Use the Marksman range scene as the first vertical slice, but only count it as playable when original scene data and dependencies load and original gameplay methods run together.
7. Verify in browser and on iPhone Safari. Compare repeatable sequences against the original where possible. Track each subsystem as Not assessed / Implemented / Tested / Parity gap.
8. Do not expose original game binaries/assets through a public demo or public repository. Keep deployment private/protected unless the owner explicitly changes that instruction.

## Evidence levels

- **Recovered:** original data/source artifact identified and integrity checked.
- **Parsed:** the original serialized data has been decoded with relationships preserved.
- **Executed:** original decompiled gameplay code runs against implemented engine services.
- **Interactive slice:** original scene, player interaction, gameplay logic, feedback, and reset work together.
- **Parity-tested:** a repeatable comparison shows the port matches the original behavior for the stated scope.

Rendering one original mesh, compiling one original C# file to WebAssembly, or passing a fixture-based unit test is useful progress but does not qualify as an interactive slice or parity-tested game.

## Current state (2026-10-14)

Recovered original code/assets and limited rendering/WASM feasibility gates exist. A faithful playable browser build has **not** yet been verified. The root index.html viewer is only an asset-inspection tool, not a game. The primary engineering task remains reconstruction of the runtime and the Marksman range vertical slice without replacing original gameplay.
