# Crisis Response browser port — agent handoff (2026-10-15)

## Start here

- Repository: `ethan-bailey/crisis-response-port` (private).
- **Active work is draft PR #7**: https://github.com/ethan-bailey/crisis-response-port/pull/7 on `codex/iphone-input-parity-foundation`, base `main`. Do not continue from `main` alone: it does not contain the latest runtime/presentation work.
- User target: play the *original Crisis Response* as faithfully as possible in landscape Safari on **iPhone 16**, with no local computer. Preserve recovered original systems and assets. **Do not invent placeholder gameplay systems, simulate fake weapon actions, or call the current preview a 1:1 port.** No Unity license/activation should be required for the primary source-preserving WebAssembly route.
- User requested on-screen controls: touch movement, drag-to-aim, tap-to-fire once the original weapon chain is actually functional. **After a drag ends, aim must remain in the last dragged direction**; idle mouse movement and a moving camera must not steer or turn the character.

## What exists and where

- `game/index.html`: browser scene/Operator rendering, original data integration, scene contacts and presentation loop. The character is controllable, but this is an incomplete prototype.
- `game/browser-input.mjs`: browser input ownership/capture and page lifecycle adapter. It is **not** game logic.
- `game/presentation.mjs`: drives browser rendering from the scoped recovered original C# camera, head/torso and analytic arm IK through the runtime bridge.
- `game/contact-physics.mjs`: PhysX WebAssembly contact simulation using recovered collider/material data. PhysX version differs from Unity 2017; parity is **not** established.
- `runtime/Original/`: extracted original/decompiled C# methods, with provenance information in `source-provenance.json`.
- `runtime/Engine/` and `runtime/Browser/`: incomplete Unity API compatibility layer and C# WebAssembly bridge.
- `runtime/README.md`: most detailed honest summary of original method coverage, extracted data, limitations and repro instructions.
- `docs/IPHONE_INPUT_AND_PARITY_AUDIT_2026-10-14.md`: original input/PlayerController audit, recovered source hashes, scope and blockers.
- `.github/workflows/game-movement-smoke.yml` and `.github/workflows/runtime-foundation.yml`: cloud browser and runtime regression jobs.

The current draft PR previously reported native, WebAssembly, Chromium and WebKit checks passing at commit `0a676c5d02085ee47509bff84744aebb0132c86e`, with evidence linked from the PR description. **Those historical checks do not validate later commits.** Linux WebKit does not establish real iPhone Safari parity.

Private preview recorded in PR #7: https://crisis-response-runtime.craftysnipe13.chatgpt.site . Its deployment commit might lag this branch; verify the published source before interpreting visual results. Do not claim a new deployment merely because changes were pushed.

## Changes made while recovering this handoff (October 15)

The previous handoff file was not discoverable among committed changes in PR #7. This file is the durable replacement.

A source-specific input regression was identified:

- `game/browser-input.mjs` previously dispatched aim events on **unpressed mouse hover**, and failed to send an explicit end-of-aim signal when the owner released or the page was suspended.
- `game/index.html` set `hasAimPointer=true` on first aiming event and did not clear it, so the per-frame AimQuad raycast kept reprojecting the old screen pointer while the camera moved. That could make the Operator aim toward an invisible cursor even after releasing.
- The new adapter sends `onAim(null)` when the active aiming pointer ends or is cancelled/suspended; only the captured pointer produces aiming updates. The consumer clears its active flag while keeping its last character-local aim vector. No gameplay implementation was replaced.
- Debug state now exposes `aimPointerActive` and `aimLocalPoint` via `window.__gameState()`.
- `scripts/browser_input_test.mjs` adds pointer-hover/release/lifecycle regressions.
- `scripts/presentation_smoke.mjs` now uses held-pointer drags, not hover, and checks the character-local aim remains fixed after release.
- `scripts/game_movement_smoke.mjs` no longer assumes mouse hover rotates the character.

Commits (feature branch, in order): `a0c6e07`, `04d00fc`, `e9ce7d3`, `7a167b3`, `e797fa5`, `7e76389`. **The new browser regression changes must be independently run and evaluated; do not report passing without fresh CI evidence.**

Verification after those edits: all **11 input unit tests** from the committed `scripts/browser_input_test.mjs` passed in an isolated JavaScript execution of the exact committed module and test source, using Event/EventTarget stand-ins. This is not a native Node.js, GitHub Actions, Chromium, WebKit or actual iPhone test. Vercel reported a READY Git preview at commit `c7e445fa8f33653629783d8bd86ffdc42dd209c2`, but this does not establish a playable page; deployment-log inspection returned HTTP 403 for the connected Vercel scope. The PR's earlier browser test results belong to commit `0a676c5`, not these edits.

## Immediate verification gate — before more behavior changes

1. Confirm PR #7's current head and branch. Inspect changed files; do not overwrite concurrent agent work.
2. Run the source-preserving contract and browser checks in GitHub Actions, especially `node --test scripts/browser_input_test.mjs`, `node scripts/game_movement_smoke.mjs`, `node scripts/iphone_input_smoke.mjs`, and `node scripts/presentation_smoke.mjs` with the prepared cloud dependencies/site. Check full job logs, screenshots and output. Resolve failures on this feature branch and update this handoff with actual outcomes.
3. Test on real iPhone 16 Safari if device access is available. The available cloud WebKit/Chromium tests are only approximations.
4. If the user reviews a preview, verify it points to the tested feature-branch commit, not the much older `main` build.

## Next substantive source-preserving milestone

- Reconstruct and execute original C# weapon and equipment input flow, **not** a JS ammo decrement or fake reload timer: `PlayerController` -> `CharacterBody` -> weapon/bolt/magazine/projectile code. Recover component/prefab dependencies; add each necessary Unity compatibility service only with source/provenance and a behavior contract. Keep FIRE/RELOAD/SWAP marked PENDING until the real original chain is executable and tested.
- Continue original camera/animation/physics parity comparisons, especially the remaining full `CharacterBody` update order, weapon blocking, injuries, recoil, respiration, input phase order and animation state machine. The current scope is a healthy, unfired, shouldered M4 and selected original camera/IK methods.
- Only after measurable original behavior exists, validate original-versus-web traces and gameplay outcomes. No original-vs-port test or full 1:1 parity is currently established.

## Operating constraints

- This is a source-preserving research and compatibility port. Track recovered originals, SHA-256/provenance and explicitly scoped deviations; do not pass a bespoke approximation off as original.
- Prefer the active PR branch; keep `main` unchanged until tests and merge review.
- The user has **only an iPhone**. Do cloud-based build, inspection, tests, GitHub updates and deployment yourself when access permits. Present a verified clickable browser build only when it actually exists.
- If a step is incomplete, record the precise state, artifact/commit and blocker here; avoid unsupported success claims.

## Follow-up: no-Vercel local HTTP and touch fire intent (2026-10-15)

- User explicitly requested a **local server instead of Vercel**. Added `scripts/serve_local.py` (standard-library Python 3), documented in `docs/LOCAL_HTTP_SERVER.md`. Existing smoke workflow now starts this server, checks `/game/` and `/runtime-web/`, and uploads the full prepared site as the separate `crisis-response-local-http-site` artifact. This is not a publicly reachable URL and the artifact must still be built; a server inside the agent workspace cannot be accessed directly from the user's iPhone.
- Crucial touch semantic fix: `game/browser-input.mjs` now distinguishes a drag (at least 8 CSS px) from a tap (at most 350 ms). **Dragging never generates a fire press**, whereas a valid pointer-up tap triggers an `onTap` callback. Pointer cancellation/capture loss/page suspension and long presses must not fire; an unpressed cursor cannot aim. The last aimed direction is retained when the drag releases. Mouse and touch share these semantics.
- `game/tap-fire-queue.mjs` converts taps to separate C# input **key-down/key-up ticks**, correctly handling back-to-back taps and reset on page suspension. `game/index.html` sends this pulse through the *original* C# `InputHandler` bridge. This is raw input only: there is **no weapon discharge**, ammo consumption, fake bullet or other substitute original gameplay system. FIRE/RELOAD/SWAP remain disabled.
- Tests added: `scripts/browser_input_test.mjs` now has **14 tests**, all passed when the exact committed module and test bodies were executed in a V8/JavaScript EventTarget stand-in harness (not Node/browser). `scripts/tap_fire_queue_test.mjs` has **5 native Node 22 tests** which passed using a local copy of the checked-in source. A local HTTP smoke of `serve_local.py` returned HTTP 200 for both entry points and correct `.wasm`/`.mjs`/`.gz` content types. Those HTTP tests used **fixture HTML/assets, not the actual game**.
- **Not yet verified:** CI build of the current feature-branch head, complete prepared game+WASM site, full Chromium/WebKit gameplay, actual iPhone Safari, genuine firing, and original-game parity. This workspace lacks the private full repository asset tree/.NET SDK, so full recovered-game execution was not attempted here. No Vercel deployment was required for the tests listed above.
- Continue the PR branch; inspect new CI runs and packaged artifact, then extend the recovered original `PlayerController` -> `CharacterBody` -> weapon/bolt/projectile chain. Keep the no-placeholder rule. If there is any observed held-finger camera-driven aim drift, isolate the browser aim-sample lifecycle with regression evidence rather than inventing original camera logic.

## Touch fire-mode and ChatGPT Sites decision note (2026-10-15)

- The user asked how semi-automatic vs automatic weapons should work with tap-to-fire and drag-to-aim, and requested using ChatGPT **Sites** (not Spaces) for a shareable link.
- Read `docs/TOUCH_FIRE_MODES_AND_SITES_PLAN.md` for recovered input bindings, recommended dual-gesture scheme, end-to-end original weapon integration gates and private Sites requirements.
- Mode behavior must come from original weapon/selector logic: scene drag moves aim only; quick tap emits one raw fire press/release; a separate holdable FIRE button maintains raw trigger held for auto, but should cause only one round on semi via the original weapon logic. Do not add JavaScript RPM timers or fake ammo. Recovered `fireMode` binding uses T and a 0.4-second long-press threshold. The original weapon mode implementation has **not** been recovered/integrated end-to-end.
- Previous private site recorded in PR #7: `https://crisis-response-runtime.craftysnipe13.chatgpt.site`. Published source was older; current Site cannot be updated through the available GitHub tools alone. ChatGPT Sites creation/edit/publishing is via ChatGPT **Work** (or desktop Codex), `@Sites`; confirm WASM support and file sizes and publish only within licensed/private access permissions.

## iPhone touch ergonomics research (2026-10-15)

- User wants fast, low-frustration **same-spot rapid taps**, **stationary hold to auto-fire**, **drag to aim**, and optional **hold then drag while firing** using original recovered weapon behavior; read `docs/TOUCH_ERGONOMICS_RESEARCH_2026-10-15.md` before changing input.
- Research sources cited in that document: Apple WebKit responsive tapping/safe areas, MDN Pointer Events/touch-action/coalescing, Apple HIG touch sizes. Proposed values **210ms stationary hold** and **10–12 CSS px slop** are tentative tuning defaults, not measured on the user's device. A three-way tap/drag/hold ambiguity necessarily delays actual firing until the intent is known; provide immediate visual feedback without an early shot.
- **Fix committed:** `game/browser-input.mjs` now inspects `pointerup` displacement even when the final `pointermove` never arrived. It rejects a moved contact as a tap and records the final dragged aim point. Two regression tests added; **16/16 passed** with source-fetched isolated EventTarget JavaScript stand-ins (not Safari/Node browser smoke). `game/index.html` HUD/controls/help/error now respect left/right/top safe-area insets, with existing bottom inset preserved. Physical iPhone check is still due.
- **Not yet implemented:** stationary hold firing gesture, timer-free per-pointer held `fire` bridge, merging with the queued tap pulses and its provenance, relative-delta aim, diagnostics and actual gun behavior. Avoid implementing an original-weapon replacement or claiming this new hold functionality already exists. The fire-mode C# key is original and must remain authoritative.
- Prioritize gesture lifecycle and source-preserving integration, short/long-tap/cancel tests, frame-tick latency metrics, and actual iPhone verification; keep a dedicated hold-FIRE option only if measurements reveal unacceptable threshold delay or mistaps.

## 2026-10-15 continuation — one-finger hold implemented and hosting blocked

- User confirmed primary scheme is **quick repeated same-spot taps**, **stationary hold after 210ms to assert raw trigger**, **11 CSS px drag to aim without firing**, and **hold then drag to aim while trigger remains held**. This overrides an earlier proposal where a separate FIRE button was the default.
- Implemented `game/browser-input.mjs` asynchronous gesture state machine with deterministic timer injection and exact pointer ownership; no weapon simulation, original `InputHandler` decides mechanics. `game/tap-fire-queue.mjs` now holds per-source original trigger state and emits distinct up/down ticks for rapid presses, drops taps older than 500ms after delayed runtime frames, and resets on page suspension. `game/index.html` reports touch input status (`WEAPON PENDING`) and supplies held trigger state to original C# runtime; FIRE, RELOAD and SWAP remain disabled until original weapon chain exists.
- Updated `scripts/browser_input_test.mjs` (22 cases), `scripts/tap_fire_queue_test.mjs` (12), and added `scripts/touch_gesture_bridge_test.mjs` (5). **All 39 passed as isolated committed-JavaScript tests** with EventTarget stand-ins, not on GitHub Actions/real browser. Added bridge suite to workflow.
- Read `docs/TOUCH_ERGONOMICS_RESEARCH_2026-10-15.md` execution update for acceptance criteria and unfinished relative aiming. First priority now is **full original game/Blazor WASM browser build/CI** and real iPhone user test. Never claim firing works when the weapon original code is still pending.
- **Hosting:** Current Vercel team scope has deployments showing READY for the branch but share-link and deploy fetch operations all fail **403 forbidden**; per-link playability is unverified. Team/project authorization in the Vercel connection must be corrected before verified private previews can be shared. No ChatGPT Sites publish tool available in this chat; older `crisis-response-runtime.craftysnipe13.chatgpt.site` points to an obsolete commit. Avoid sending it as a test of this work.
