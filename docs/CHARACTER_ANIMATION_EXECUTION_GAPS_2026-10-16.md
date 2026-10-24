# Character animation execution gaps — status refreshed 2026-10-24

This file began as a 2026-10-16 source audit. Its original “not compiled/not executing” claims are historical and have been replaced here with the current evidence. The current source checkpoint is `638cf4a5a470a2f514446c055b73a1a3b57cf048`; the next commit `bf21daa9320072338c5eef64823e3ecbbb3104e2` only updated the handoff.

## Current source/runtime status

| Developer source | Current runtime evidence | Remaining limit |
|---|---|---|
| `CharacterBody.cs` | Full source is compiled as `Crisis.Runtime.FullSource.CharacterBody` in `runtime/Original/CharacterBody.Full.cs`. Original actor lifecycle and combat behavior have focused headless contracts. | Not every source branch is proven; the ordinary page does not instantiate the complete original mission actor flow. Native PhysX and rendered gameplay remain separate acceptance. |
| `ArmController.cs`, `ArmsControl.cs`, `ArmIKSolver.cs` | Original arm source and solver behavior are present in the runtime; focused contracts cover selected arm/hand and physics commands. | Full ragdoll/joint response, authored rig targets, and Unity trajectory parity remain unverified. |
| `IKArmController.cs`, `IKArmSequencer.cs` | Compiled in `runtime/Original/FullSource/`; the controller has an actor-identity guard. Headless checks exercise two actors, `LowerWeapon`/`FireWeapon` callbacks, completion, and actor isolation. | Authored transition/sibling-wait callbacks and live normal-page weapon/hand visual continuity remain unverified. The browser composition bridge is diagnostic only and explicitly reports playback as false. |
| `IKArmSequence`, `IKArmSequenceRuntime`, frame/action/target/callback types | Original sequence records load from the preserved bundle; tests cover seven prefabs, 21 frames, composition and selected callback ordering. | Bundle composition is not normal gameplay playback. |
| `Hand.cs` | Original `Hand.SetMode` executes in the source contract; the preserved original renderer records are used by browser presentation code. | Exact animation/shader/device rendering parity remains unverified. |

## Safe next work

Gate 2 must wire the compiled source controller/sequencer to the real normal-page original actor lifecycle and authored mission/equipment inputs. The diagnostic bridge in `runtime/Browser/Program.cs` only composes serialized sequence data; it must not be treated as the gameplay entry point. The source path must preserve callback order, sibling waits, weapon activation/holstering, grenade/gauze ownership, and actor identity. Avoid a second JavaScript sequencer or synthesized source defaults.

The normal page's `?scene-smoke` mode loads/inspects scene records and binds Global Context. It is not mission gameplay startup. Prior `--mission-ai-*` contracts used a generated full scene package from the pinned private recovery archive and headless adapters; those tests prove only their stated source contracts. Reuse a verified package while present and regenerate only when absent or its inputs change, following `docs/DOTNET_BUILD_RUNBOOK.md`.

Gate 2 remains partial until the original systems run on the real actor path with authored targets and focused contracts cover transition completion, sibling waits, interruptions, and callback side effects. Obtain owner iPhone Safari acceptance at the next major playable milestone or before release. Desktop Chromium/WebKit are optional diagnostics.

## Evidence

- Current controller/sequencer contract: `runtime/Checks/ArmSequenceContracts.cs` and `FullSourceControllerPlaybackContracts` in the same file.
- Current source files: `runtime/Original/FullSource/IKArmController.cs`, `runtime/Original/FullSource/IKArmSequencer.cs`, and `runtime/Original/CharacterBody.Full.cs`.
- Detailed prior source combat result and limits: `docs/evidence/SOURCE_COMBAT_INTEGRATION_MILESTONE_2026-10-24.md`.
- Ordered remaining gates and first-playable dependencies: `docs/FOUNDATION_FIRST_COMPLETION_PLAN_2026-10-17.md` and `docs/evidence/GATE0_FIRST_PLAYABLE_DEPENDENCY_MATRIX_2026-10-24.md`.
