# Character animation and source actor execution status — refreshed 2026-10-27

## Packaged Mission startup failure — Actions #136 (2026-10-27)

Exact source SHA `55648848ce427b272708f25fc32c8226da048b05`, workflow run
`40485228497`, main job `122125219982`. Node/Python/runtime/browser-audio,
engine/FoundationChecks, pinned Release/WASM, recovered source/index provenance,
the complete 1,919-asset Mission package/integrity check, package retention and
full-profile IndexedDB loading all passed. The packaged FullSource Mission page
also transferred and verified all 1,919 assets in about 36.2 seconds, then failed
during original actor startup. The exact error was
`TypeError: audioStats.updateSourceAudioMixer is not a function` from
`game/weapon-runtime.mjs`, reached through `game/presentation.mjs` and the
Mission start handler. `createWeaponEffects` exposed only `audioStats.snapshot`
although its returned public API forwarded `updateSourceAudioMixer` and
`updateSourceAudioStates` through that facade. The recovered audio runtime does
implement both methods. The startup snapshot is consumed before the Mission
frame loop; this is an adapter wiring defect, not an absent serialized mixer
asset or transfer stall. Independent Chromium IndexedDB/audio job
`122125219552` passed. Retained browser artifact `12725391858` is 188,281 bytes,
digest `sha256:8c36d7c87df53a4195160c121dba39f10fca938e0d9b08801dfc00284dc83da2`.

The repair introduces a narrow `createWeaponAudioStats` pass-through and a
regression that asserts exact startup-mixer and per-frame-state arguments and
return values, plus production facade wiring. The previous text-only contract
checked the facade's outbound call but did not instantiate the object behind
it. No original source, serialized identity, event ordering, or physics values
are changed. Re-run the exact packaged Mission test and package gates on the
repair SHA; do not treat the local regression alone as playable Mission parity.

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

## Source hand-mode visibility and the remaining pose seam

The source `Hand.SetMode` owner activates/deactivates its fist, trigger and forward-assist GameObjects. `game/original-hand-renderers.mjs` attaches recovered hand meshes under those original nodes and initializes visibility from the serialized renderer state. The FullSource snapshot formerly carried pose TRS only, so the page could not observe later source GameObject active-state transitions. On current SHA `530c483266cf58d4ac6b9b65cb82ccb867c51643`, `FullSourceActorBridge.Snapshot` includes `activeSelf`; `projectFullSourceSnapshot` applies it as the projected source group’s visibility. The mesh remains attached under its original owner. `scripts/source_character_animator_test.mjs` asserts the original mode toggles, snapshot field, projected visibility, and hand mesh parenting. The Node contract passed in Actions #128.

This closes the source-state transport defect for hand layer visibility; it does not prove arm IK accuracy, gun grip alignment, or image output. Actions #128 also passed the complete Mission source-entry package contract and the deferred hurt-audio contract, but it did not run the normal post-download rendered page.

Static source geometry predicts a separate root/ground mismatch: authored Operator spawn Y=1.5; capsule height=1 with center offset=-0.64; Mission floor top Y=0; source foot/toe chain is near the actor root. Capsule bottom is rootY-1.14, so contact settling against the floor can leave the source visual root approximately 1.14m above the floor. This is an inferred prediction pending an observed production-path contact and rendered-root trace. Do not alter source collider values or add an offset until that diagnostic and the original authored behavior resolve the discrepancy.

## Exact-package verification — Actions #128 (2026-10-27)

Branch `develop/wasm`, exact source SHA `530c483266cf58d4ac6b9b65cb82ccb867c51643`; run `40444437888`, main job `121992691908`, independent Chromium job `121992692081`. The workflow used pinned .NET SDK 8.0.426. Both jobs passed.

The package-build job ran the source-entry session command against the exact package it created:
```sh
dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -c Release -- \
  --browser-entryteam-session-contract "$RUNNER_TEMP/cpr-site-package/scene-data/fullsource/global-ui-mission-operator"
```
Result: `status=passed`, Mission 01, Operator 1 with M4A1, source Mission actors and interaction menu initialized, one original source shot, arm ready, one identity-verified particle event, zero dropped particle events, one casing force and torque request, original input press/hold/release, horizontal-independent absolute aim, and source/browser position-velocity synchronization. Source actor count was six in this invocation and is not treated as an authored invariant.

The complete Mission contract separately reported `missionStarted=true`, `globalContextInstantiated=false`, two Operators and four Occupants for that test path. Its authored UI/outcome and AI contracts passed; the UI still uses the test harness path rather than the normal rendered Mission page. The independent full-profile browser acceptance used production incremental loading in Chromium IndexedDB; it modeled the Blazor bridge and did not render or control the Mission scene. Full details and package hashes are in `docs/evidence/GATE5_HAND_LAYER_VISIBILITY_2026-10-27.md`.

This is stronger exact-package startup evidence than the 2026-10-26 local run below; the older result is retained as history. It does not test on iPhone Safari, verify post-contact rendered root/feet, pixel-equivalent animation, native Unity Animator or native PhysX.

## Remaining Gate 2/5 acceptance

- Source-entry integration command is now in CI and passed against the exact package built on SHA `530c483` in Actions #128.
- Keep full-page pixel/pose inspection separate from source state. Verify the actual render output on an enabled WebGL device and iPhone Safari.
- Complete native/browser joint, ragdoll, casing-contact and physics acceptance; headless force sinks and round-trips do not prove native solver behavior.
- Add a production Mission-page contact diagnostic for source root, capsule center/bottom, floor top, contact-solver output and projected render root; compare its first settled frame against the authored source geometry. Verify authored AnimatorController transitions, source transition timing and two-arm visual continuity on rendered output.
- Keep parity flags false until combined Mission flow and device/render/physics evidence pass.

### Packaged Mission grounding browser acceptance closure — prepared 2026-10-27

Before implementation, the known serialized/runtime dependencies were checked against the existing Mission profile and frame owner: `Assets/Resources/characters/Operator.prefab` supplies the source capsule and spawn; `Assets/Scenes/Mission 01.unity` supplies the authored floor; the package's contact-physics export supplies capsule direction/radius/height, center offset, floor boxes, and fixed-step configuration. The normal post-download path is `InitializeIncrementalEntryTeamMission` → `TickFullSourceActorBridge` → capture source/contact-before → fixed contact steps → capture contact-after → `SynchronizeFullSourceActorPhysics` → `projectFullSourceSnapshot` → browser render. The authored Operator and Mission serialized files remain hash-locked; no substitute fixture or physics/visual offset is used.

`scripts/mission_grounding_browser_smoke.mjs` runs the **integrity-built package** in Chromium with WebGL/SwiftShader, presses the actual Start original Mission button, and verifies the visible/disabled download-progress state. The package server injects one HTTP 503 for the first request under the full Mission asset prefix; the page must show the accessible error/retry UI, re-enable Start and clear progress. The test presses the actual Retry Mission control, verifies progress resumes, and then waits for the FullSource Mission actor. This exercises the real failed-fetch recovery path without changing or substituting source assets. On success it requires the progress UI to close only after verified source transfer and actor initialization. It checks that source-before-contact, contact-before/after, synchronized-source, contact-root and projected-root samples are finite; only the stage invariants are asserted (contact root equals the final contact result; synchronized source and projected root equal the source root). It requires a finite authored capsule lower bound and retains screenshots plus machine-readable telemetry on success and failure. It then holds original `KeyD` input, checks actual source-root travel and recovered `Body walk`/`Body sprint` selection, releases input, and observes `Body stand` after the actor stops. These are packaged integration checks, not animation pixel/curve parity. The test does not assert floor contact or pixel parity: measured values and screenshots must be inspected before diagnosing a visual/physics defect. The workflow runs this after building and verifying the exact six-root Mission package and retains diagnostics even if the browser acceptance fails.

## Evidence

- EntryTeam integration contract: `runtime/Checks/Program.cs` (`--browser-entryteam-session-contract`).
- Browser source actor boundary: `runtime/Browser/Program.cs`, `runtime/Engine/FullSourceActorBridge.cs`, `runtime/Browser/BrowserEntryTeamMission.cs`, and the post-download path in `game/index.html`.
- Authored IK transition checkpoint: `docs/evidence/GATE2_AUTHORED_IK_TRANSITION_2026-10-24.md`.
- Gate order and current unverified platform boundaries: `docs/GATE_DEPENDENCY_INDEX.md`.


### Packaged Mission grounding timeout diagnosis and next-run instrumentation — 2026-10-27

Actions run #135 (`40481392427`) tested ancestor source SHA `a24165c708846a36adc7688098f2cc9008782ccc`. Its main job `122112493275` passed Node/Python/runtime/browser-audio/engine/FoundationChecks, pinned .NET 8.0.426 Release/WASM publish-output, source archive/index provenance, full Mission package build/integrity and retention, and the 1,919-asset IndexedDB test. Step 26, “Exercise FullSource Mission grounding on the packaged page,” failed after 900,000 ms at `mission_grounding_browser_smoke.mjs:111`, waiting for the retry load to produce `game.loaded` plus three synchronized-source coordinates. The independent Chromium cache/audio job `122112493569` passed. This confirms the package itself verifies, but the packaged page did not reach a playable Mission state in that attempt; no gameplay parity is accepted from this run.

The retained diagnostic artifact `12725000332` is 179,609 bytes (SHA-256 `416f133be606c82403ffc901dd92c4a556264c37bebe383d8b47b2c37a71dec8`). One authorized connector attempt to retrieve its ZIP failed because binary artifact downloads are unsupported. No repeated download attempt is planned without new access evidence. The exact runtime substage therefore remains **unknown/deferred**, not diagnosed as a specific loader or actor bug.

A follow-up browser harness is prepared locally: it caps retry-to-playable waiting at 300,000 ms, writes compact UI/source-transfer/audio/slowest-fetch/slowest-bridge/actor/grounding checkpoints to the Actions log every 15 seconds, and emits the final complete page diagnostics in both the error log and retained JSON/screenshot. It also retains explicit retry UI and clipboard assertions, original KeyD locomotion/release assertions, and rapid-touch mobile viewport zoom emulation. This is instrumentation and a stronger acceptance harness, not a runtime fix. The copied error is expected to match the complete visible HTTP 503 detail exactly. Local verification after this change: `node --check scripts/mission_grounding_browser_smoke.mjs`, Node suite **168/168**, Python suite **53/53**, workflow YAML parse and `git diff --check` passed. Local .NET is absent (`dotnet: command not found`); the next hosted run must establish behavior on the exact pushed source SHA with pinned 8.0.426.

Serialized/runtime dependency inventory remains: Global Context → Mission 01 and Interaction Menu; authored Operator prefab and original M4A1; full Mission profile/index/package; source EntryTeam initialization; retry/error/copy UI; asset loader transfer/integrity; audio preload; original FullSource actor frame, input and contact synchronization; browser grounding diagnostics; recovered AnimatorController; packaged Three.js hierarchy; keyboard locomotion; mobile viewport/touch handling. Mission/UI/prefab/source identities are not substituted. The focused next test must use the complete integrity-verified package, one intentional HTTP 503 on an authored profile asset, actual Retry, and actual Start/actor initialization. Only after the emitted checkpoint identifies the stall should a source behavior be changed.
