# iPhone input work and parity audit

Inspected base commit: `13b24c4ce296a81f4d6803cd4a3325020d13229b`.

## Result

This change improves the browser prototype's device input. It is **not a fully
working port**, does not execute PlayerController in WebAssembly, and does not
establish 1:1 behavioral parity. No new substitute gameplay systems were added.

The recovered project was downloaded from private Actions artifact 11903811738,
run 38582072502. The original input and player code were read directly. SHA-256:

| Recovered file | SHA-256 |
| --- | --- |
| InputMapping.cs | 6b7d03a5dda93c5842ae1953e5ec0bb4460c580c9d3dc09e552156df0ce6a30e |
| InputHandler.cs | 93c8cdb43482caadeffe1ee0d18e60f502398fd5f83ccb4f15332059df0a2498 |
| PlayerController.cs | 36034f2bac068358cad0912c2962fd0481f55ed5af076f6aea31b8eb4f3d30f0 |

## Device adapter

- Individual pointer ownership and capture prevent a release outside a button
  from leaving its input held.
- Releasing one finger does not clear another finger's action or keyboard input.
- Only the owning aim pointer updates an active aim gesture.
- Blur, pagehide, hidden-document transitions and resize release held input and
  aim ownership. No automatic firing resumes after returning to the page.
- Separate Kneel / Stand controls match the recovered S / W actions. Original
  Tab weapon-switch binding replaces the prototype's Q and direct 1/2 shortcuts.
- Landscape controls account for browser safe-area insets and use at least
  48 CSS pixel buttons. The prototype is explicitly labelled incomplete.

`browser-input.mjs` owns raw pressed state only. It does not implement the
original InputKey DownLong/UpShort/UpLong logic, equipment action queues, weapon
state machines, physics, AI, or other gameplay. The current prototype still
consumes this adapter in JavaScript. Future C# runtime integration must route
raw key state into the original InputHandler and retain its update ordering.

## Concrete blockers found in the existing prototype

| Area | Current implementation | Required original behavior |
| --- | --- | --- |
| Firing | Decrements a JS ammo counter and draws a Three.js ray line | PlayerController → CharacterBody.PullTrigger → weapon/bolt/projectile lifecycle, fire mode, safety and manipulation gates |
| Reload | Timeout refills a counter | Original pending equipment action, inventory/magazine state, bolt state, animation timing and reload-help conditions |
| Weapon switch | Toggles rendered prefab visibility | Original equipment state and switch action |
| Movement | Selected acceleration formula with no collision world | Rigidbody simulation, contact response, limb constraints and fixed-step ordering |
| Animation | Samples selected recovered clips | Original animation/state machine, IK, pose transitions and physics interaction |
| Aim | Partial plane/raycast/turn logic | Complete camera, interaction UI/free-aim gating, weapon aim and limb-flip behavior |
| Mission | Static recovered geometry and operator | Scene/component instantiation, AI, damage, objectives, interaction, persistence and UI |

These existing shortcuts were not validated or promoted to parity by this
change. They remain blockers to the requested deliverable. A successful browser
smoke test only establishes that the prototype runs and receives input.

## Verification

`node --test scripts/browser_input_test.mjs`: nine regression cases pass locally.
JavaScript module and inline-script syntax checks pass locally.

The Actions workflow runs the existing geometry/movement smoke test, the input
regressions, and the new Chromium/WebKit landscape integration check. The latter
checks control bounds, pointer capture, movement reaching the prototype, pagehide
reset, and Chromium multi-touch ownership. Its JSON and screenshots are workflow
artifacts. Linux WebKit emulation is not actual iPhone 16 Safari verification.

No local browser binary or .NET SDK was available in this session. Full original
game execution, original-versus-port comparisons, and physical-device tests have
not passed. A complete replacement engine cannot be inferred from these tests.
