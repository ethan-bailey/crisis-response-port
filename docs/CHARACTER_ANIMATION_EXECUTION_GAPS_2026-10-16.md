# Character animation and source actor execution status — refreshed 2026-10-26

## Current source/runtime path

The normal Mission launch now uses the recovered original actor path. After the incremental Mission profile verifies, `RuntimeBridge.InitializeIncrementalEntryTeamMission` calls `BrowserEntryTeamMission.Start`, initializes Global Context/Mission and the source EntryTeam actor, and constructs `FullSourceActorBridge`. The page then ticks original input/controller/CharacterBody/weapon owners through `TickFullSourceActorBridge`, synchronizes the browser physics state back to the source actor, and renders the source actor transform hierarchy. The older initial playable preview still uses a narrower presentation host; it is not evidence of the post-download Mission actor.

The normal-page actor transform and animation boundary is hybrid: source `RuntimeWorld` pose snapshots are applied by `projectFullSourceSnapshot`, while the page samples recovered AnimatorController/clip data through `SourceCharacterAnimator` and `stepCharacterAnimation`. This is not a Unity Animator implementation or pixel-equivalent native animation. Original arm IK, input, firing, reload, Physiology, and callbacks run in compiled C# on the Mission actor; Three.js meshes, cutout skin, hands, weapon, and physical arm view remain browser renderers.

## Dependency inventory for the normal-entry contract

| Dependency | Source owner / record | Assertion in `--browser-entryteam-session-contract` | Remaining boundary |
|---|---|---|---|
| Exact package and entrypoint | Global Context, Mission 01, UI, Operator, Version; `BrowserEntryTeamMission.Start`; `RuntimeBridge.InitializeIncrementalEntryTeamMission` | Loads the verified full Mission profile and creates the authored Mission/Interaction Menu and source Operator. Source actor snapshots varied between six and seven across the two invocations; this is not asserted as a fixed actor-count invariant. | Contract uses a locally verified profile; exact pushed workflow must execute it against the package it builds |
| Input and actor ownership | Global.Context.InputHandler/InputMapping, source PlayerController, CharacterBody, authored M4A1 | One original movement input emits source Acceleration; a 0.5 stick cap scales source walk force and restores authored speed; original arm becomes ready and source fire input increments ShotsFired | Analog input is a browser-provided magnitude and currently scales the source per-frame speed cap; native input/touch feel is iPhone acceptance |
| Aim | Source PlayerController.WeaponPointer with its screen-ray owner disabled for browser absolute aim; `SetAimWorldPoint` writes the same actor-owned pointer target | Explicit world aim remains unchanged by horizontal movement and mouse X in both directions | The post-download browser viewport/vertical absolute aim behavior still needs visual/device acceptance |
| Audio and particles | Original AudioSource, M4A1 fire/crack clips, source particle event queue and serialized renderer identity | Fire path returns source-timed fire/crack audio events; one particle event preserves component/prefab/hash fields with zero dropped events | Browser event transport is not speaker, particle pixels, or native particle simulation proof |
| Casing and actor physics | Original source Casing Rigidbody callbacks plus browser physics synchronization | Source firing emits original casing force/torque requests; player position and velocity round-trip | Browser adapter is not native PhysX parity; impact/contact trajectory remains unverified |
| Render and animation | Recovered actor hierarchy, AnimatorController graph/clip data, actor-owned IK and hand/weapon presentation | Original actor snapshot hierarchy drives render-node poses; source arm/weapon callbacks execute in the contract | Contract does not render pixels, evaluate Unity's native Animator, or verify the source animation's complete transition curve on iPhone |

## Verification on 2026-10-26

Command:

```sh
DOTNET_ROOT=/tmp/codex-dotnet-8.0.426 PATH=/tmp/codex-dotnet-8.0.426:$PATH LD_PRELOAD=/tmp/libprocshim.so \
  dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -c Release -- \
  --browser-entryteam-session-contract /tmp/cpr-site-v83-package/scene-data/fullsource/global-ui-mission-operator
```

Result: **passed twice** on source HEAD `44cf92ac552b1c65d45afed883ffec4e19f504a7`; package SHA-256 `ea3592ff5bdd39534bb905cd0abac74f456578b46eb4b12cdd0cb79fe8520cf7`. The first invocation reported six original source actors and nine source audio events; the latest rerun reported seven and ten respectively. The source Mission actor count is a runtime snapshot, not a stable authored invariant. Both runs reported Mission 01, Operator 1, one source M4A1 shot, arm readiness, enabled original fire input, one identity-verified particle event, one casing force and torque request, 0.5-magnitude source movement, horizontal-X-independent absolute aim, and player position/velocity synchronization. Latest command output is preserved in `/tmp/cpr-entryteam-44cf.log`. Runtime emitted expected disabled-effect notices for unsupported `NoiseAndGrain` and `BloomOptimized` shaders; it did not fail. Build emitted existing compiler warnings, including nullable annotations on existing contract code.

This contract was run locally in this work session and is **not yet a step in** `.github/workflows/source-contracts.yml`. It must be added after package construction so the exact CI-generated profile is exercised. Then rerun the focused command and the exact-SHA workflow before marking this source-entry slice CI-verified.

## Remaining Gate 2/5 acceptance

- Add the already-existing source-entry integration command to CI against `$RUNNER_TEMP/cpr-site-package/scene-data/fullsource/global-ui-mission-operator`.
- Keep full-page pixel/pose inspection separate from source state. Verify the actual render output on an enabled WebGL device and iPhone Safari.
- Complete native/browser joint, ragdoll, casing-contact and physics acceptance; headless force sinks and round-trips do not prove native solver behavior.
- Verify authored AnimatorController transitions, source transition timing and two-arm visual continuity on the real rendered actor. Existing authored Pelvis-transition checks are headless and do not close device presentation.
- Keep parity flags false until combined Mission flow and device/render/physics evidence pass.

## Evidence

- EntryTeam integration contract: `runtime/Checks/Program.cs` (`--browser-entryteam-session-contract`).
- Browser source actor boundary: `runtime/Browser/Program.cs`, `runtime/Engine/FullSourceActorBridge.cs`, `runtime/Browser/BrowserEntryTeamMission.cs`, and the post-download path in `game/index.html`.
- Authored IK transition checkpoint: `docs/evidence/GATE2_AUTHORED_IK_TRANSITION_2026-10-24.md`.
- Gate order and current unverified platform boundaries: `docs/GATE_DEPENDENCY_INDEX.md`.
