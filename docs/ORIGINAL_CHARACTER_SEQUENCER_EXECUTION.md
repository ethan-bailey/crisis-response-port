# Original CharacterBody and IK sequencer — current execution status

## Verified checkpoint

The current source checkpoint is `638cf4a5a470a2f514446c055b73a1a3b57cf048` (followed by documentation-only handoff commit `bf21daa9320072338c5eef64823e3ecbbb3104e2`). Do not use the older 2026-10-16 planning status below as current evidence.

- `runtime/Original/CharacterBody.Full.cs` supplies the original `Crisis.Runtime.FullSource.CharacterBody` implementation in the full runtime assembly. It compiles; its presence does not prove every source branch is exercised by the normal browser game.
- `runtime/Original/FullSource/IKArmSequencer.cs` and `IKArmController.cs` are compiled into the full runtime. Their method bodies match the preserved source-reference files, apart from namespace adaptation and the actor-identity guard in `IKArmController.Play`.
- The headless `FullSourceControllerPlaybackContracts` fixture verifies actor-owned controller/sequencer instances, source callback ordering for `LowerWeapon` and `FireWeapon`, completion, sibling-frame waiting, and isolation between two fixture actors. The fixture does not exercise a complete authored character rig, authored transition targets, or rendered/physical continuity.
- The source arm-sequence bundle preserves seven original prefabs and 21 authored frames. Runtime contracts verify source list composition, frame data, callback ordering, and bounds. The browser `ComposeOriginalArmSequences` bridge is a diagnostic composition/serialization endpoint and deliberately returns `playbackImplemented: false`; it is not a browser gameplay scheduler.
- The current full runtime build and Release WASM publish passed at the gameplay-source checkpoint above. These builds establish compilation only.

## Gate 2 — remaining acceptance

Gate 2 remains partial. The normal `game/index.html` path does not load a complete original mission package or instantiate the original Operator through the complete source lifecycle. Its optional `?scene-smoke` path only loads/inspects scenes and initializes Global Context. The separate browser sequence bridge composes source records but does not play them. Do not implement a second JavaScript sequencer or invent missing actor/equipment defaults to make that diagnostic report playback.

The next safe implementation requires the real authored mission/prefab inputs and a normal game-path owner for the original `FullSource.CharacterBody`, `IKArmController`, `IKArmSequencer`, solvers, weapon/hand targets, and callbacks. Then add focused contracts for transition completion, sibling waits, switch/holster interruption, and authored callback side effects, followed by an exact-build iPhone Safari milestone check. Desktop browser automation is optional under the owner policy.

The original scene package used by prior headless mission contracts is generated from the pinned recovery archive; it is not the compact tracked site scene package. Reuse an existing verified package while it exists. Regenerate it only when it is absent or inputs change, using `docs/DOTNET_BUILD_RUNBOOK.md`. Do not repeatedly extract the archive just to rerun unrelated checks.

## Preserved source timing contract

`IKArmSequencer.Play` enters frame 0 immediately. Once an action completes, the following `Update` invokes its callback before advancing with `NewFrame`. `WaitForSiblingFrame` uses `HasFinishedFrame`; `Transition` waits for the source solver's `OnTarget`; `WaitUntilLowered` uses source aim alignment. Preserve these semantics when wiring the real actor lifecycle. No synthetic timing, grenade, gauze, equipment, or physics behavior is accepted as source parity.

## Evidence pointers

- `runtime/Checks/ArmSequenceContracts.cs` — composition and controller callback contracts.
- `runtime/Original/FullSource/IKArmSequencer.cs` and `IKArmController.cs` — compiled source implementation.
- `docs/evidence/SOURCE_COMBAT_INTEGRATION_MILESTONE_2026-10-24.md` — exact scene-package command and prior headless mission/combat contract results; those results do not prove browser or native-physics parity.
- `docs/history/GATE0_FIRST_PLAYABLE_DEPENDENCY_MATRIX_2026-10-24.md` — first-playable dependency classification and normal-page startup limits.
