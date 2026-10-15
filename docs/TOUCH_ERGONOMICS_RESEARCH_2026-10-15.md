# Crisis Response — iPhone gesture ergonomics and latency research

2026-10-15. Goal: one-surface quick taps for repeated semi-auto shots, hold in place to sustain automatic trigger, drag to aim without firing, and optional hold-then-drag to aim while firing. **Design specification, not proof that firing works.** Browser adapter only. All weapon effects remain exclusively in the recovered original game systems.

## External references

- Apple WebKit, *More Responsive Tapping on iOS*: https://webkit.org/blog/5610/more-responsive-tapping-on-ios/ — historic delayed compatibility clicks and fast taps under a properly configured viewport. Use Pointer Events directly; never await `click` or double-tap timeout for a trigger.
- MDN Pointer Events: https://developer.mozilla.org/en-US/docs/Web/API/Pointer_events — pointer identity, capture, cancellation and implicit touch capture.
- MDN `touch-action`: https://developer.mozilla.org/en-US/docs/Web/CSS/Reference/Properties/touch-action — default scrolling/pinch can generate `pointercancel`. Existing fullscreen game region already has `touch-action:none`.
- MDN pointermove: https://developer.mozilla.org/en-US/docs/Web/API/Element/pointermove_event — high event rate, pointermove not limited to pressed pointers, may coalesce. Don't raycast for unpressed hover or every hardware event.
- MDN coalesced events: https://developer.mozilla.org/en-US/docs/Web/API/PointerEvent/getCoalescedEvents — non-Baseline/secure-context feature; optional for smooth drag, always provide a normal-event fallback.
- Apple HIG Buttons: https://developer.apple.com/design/human-interface-guidelines/buttons — accessible control target at least 44x44 pt; pressed-state feedback is important.
- Apple WebKit, *Designing Websites for iPhone X*: https://webkit.org/blog/7929/designing-websites-for-iphone-x/ — `viewport-fit=cover` requires top/left/right/bottom safe-area insets, particularly in landscape.
- Nielsen Norman Group touch-target study: https://www.nngroup.com/articles/touch-target-size/ — physically generous targets reduce misses and acquisition time.

The numerical tolerances below are **proposed tuning hypotheses**, not values directly established by these articles; validate on the target iPhone 16.

## Critical ambiguity and proposed state machine

A new finger on the scene is initially `PENDING`, not firing and not steering the aim.

- On `pointerdown`: capture pointer; save origin, time, last valid aim point; give immediate subtle pressed visual feedback. Do **not** change the aim toward the finger: that would cause a jump before the gesture is classified.
- On move beyond movement slop (candidate **10–12 CSS px**, measured from initial down): enter `AIMING`. This transition wins over `PENDING` hold even if a time deadline just passed. Update aim without firing. Use final `pointerup` coordinates to catch skipped/coalesced `pointermove` events. Consider a short hysteresis/latch to avoid flicker between states; once dragging, it stays dragging until release.
- On `pointerup` while still pending and under hold threshold: emit one **discrete fire down/up** pair at the next available original input ticks. Repeated taps, including the same spot and near-misses, are independent; do **not** wait for a double-tap timer. Aim remains unchanged.
- After about **190–230 ms** of no significant movement (candidate default **210 ms**): transition from `PENDING` to `HOLD_FIRING` and hold the *raw* original trigger true. This requires a time source usable across paused animation frames (prefer monotonic timestamps+the game tick with a minimum latency check; no JavaScript cadence clock). Emit one `Down`, maintain `Pressed`, emit one `Up` on release. Original weapon mode controls shots, including safety/bolt/ammo/recoil. A semi-auto weapon must not repeat merely because pressed; automatic may repeat as allowed by original fire scheduler.
- If moving after hold began: keep `HOLD_FIRING` trigger true while updating aim. Show a distinct firing/hold UI state to prevent confusion. Never queue another tap on release.
- If cancellation/capture loss/visibility change/blur/pagehide/resize or DOM disposal: end gesture without a new fire press, release all held sources and clear queued pulses. No stuck fire, delayed queued shots, or automatic firing after resume.
- Pointer ownership: movement, sprint, and aiming surface have separate `pointerId` capture. A second finger touching the scene must not steal the active aim/fire gesture. Keyboard/mouse/touch sources must merge without one source clobbering a different held fire source.
- Optional fallback: an independent, comfortably sized hold-FIRE control for accessibility, unreliable gesture recognition, or players who find the 210 ms wait undesirable. This path should be explicit, not an alternate simulation of weapon cadence.

**Why the delay is irreducible:** On `pointerdown`, an unmoving finger might eventually release as a tap, start moving as a drag, or become a hold. Firing immediately would make any intended drag produce an accidental round. Holding the first shot until `pointerup`/hold threshold is the cost of combining three actions in one touch area. We can give immediate visual feedback, but cannot simultaneously guarantee zero onset delay and zero unintended shooting.

## Risks, current evidence, and implementable mitigations

| Priority | Risk | Current repository behavior | Mitigation |
| --- | --- | --- | --- |
| P0 | Swipe accidentally shoots if final move is missing | `bindAim` previously tested only `pointermove` | **Fixed**: release checks displacement from initial contact, classifies skipped last sample as aim, never tap |
| P0 | Current long stationary press does nothing | `bindAim` only dispatches taps on `pointerup` within 350 ms | Add explicit pending->hold transition at candidate 210 ms. **Not implemented** |
| P0 | Full-auto needs hold, semi-auto needs new trigger edges | Current `TapFireQueue` only emits short pulses | Merge per-pointer hold and discrete pulses into original `InputHandler` with down/up separation. Preserve original fire-mode selector; do not infer available modes from RPM |
| P0 | Accidental shooting while trying to aim | Current 8px drag threshold may be small for thumb jitter | Use candidate 10–12 CSS px, stable-origin displacement and dynamic classification; compare error rates on phone, use configurable slop |
| P1 | Lag from end-of-tap firing / long-press threshold | Fire pulses enter async `runtimeCall('Tick')` per frame | Immediate visual press state, no browser click/double-tap recognizer, timestamp intent and measure pointerup->original-C# down tick; avoid unnecessary await and queue age |
| P1 | Slow rapid taps create a backlog of stale shots | `TapFireQueue` bounds at 8, but no age TTL; drain is one down/up pair per two original ticks | Measure input tick latency; cap queue age or cancel stale pulses on mode/pause/weapon transition and clear when unavailable, not after long load. Reject duplicates; do not fabricate weapon firing |
| P1 | Aim jumps when drag crosses threshold | Existing `updateAim` projects **absolute screen pointer** to a plane after first significant move | Implement relative-delta aim mapping (virtual trackpad) or calibrated offset anchored at down; keep original C# camera/IK/procedural aim logic, no custom weapon state |
| P1 | Camera and walking make stationary aim drift | Existing aim rays are recomputed against camera each render frame while pointer active | Preserve stable character-local direction when no actual movement; sample pointer only when moved; use source-backed camera transforms and verify walking/turning test cases |
| P1 | iOS browser interrupts gesture/scrolls/pinches or leaves held input | Pointer capture and `touch-action:none` already present, lifecycle cleanup added | Regression tests on actual iPhone: swipe through browser edge, incoming call, orientation, tab background/foreground, Safari toolbar; ensure release/cancel propagates exactly once |
| P1 | Notch/home-indicator controls hard to touch in landscape | Existing `viewport-fit=cover`; bottom safe area respected, left/right not | **Fixed**: UI `left/right/top` safe-area insets added for HUD, controls, help and errors; visually inspect on real iPhone |
| P2 | Finger hides reticle/target and HUD overlaps game | Absolute aim uses finger coordinates, HUD overlays scene | Display crosshair from *actual original weapon* aim/ray, not raw touch coordinate; consider small visual offset/relative-delta aim; use sparse translucent HUD |
| P2 | Feels unresponsive or inconsistent at 30/60/120 Hz | Native original input tick is async per render frame and frame skips `frameBusy` | Timestamp sampling and diagnostic metrics: input timestamp->C# tick, drift distance, no-render frame count, dropped/stale pulse count, missed/ghost taps. Never invent gun cadence |
| P2 | iPhone thumb target or UI hard to reach | Existing main controls >= 54px; screenshot test uses 44px min | Keep minimum 44x44 pt and generous spacing; offer control position/mirror setting later. Ensure aim scene excludes HUD/modal and disabled button areas |

## Acceptance gates before shipping touch auto-fire

1. No fire at all when immediately dragging in 8 compass directions, including skipping a final move event. No false aim if only repeated short taps. Hold a finger stationary for 210ms, release, and confirm a single original `Down/Pressed/Up` sequence.
2. Test **20 consecutive rapid same-spot taps** at ~90ms to 220ms inter-tap intervals. Exactly one raw trigger press/release per accepted tap, with no unbounded backlog and no late shots after a mode change. Actual emitted rounds must obey the original weapon's firing mode, rate, ammunition and mechanical state.
3. Hold then drag: continued raw trigger state and normal aim updates; release ends trigger immediately. Transition must not produce an extra synthetic tap. A second finger may independently hold movement.
4. Cancel during pending, holding and dragging; focus loss/pagehide; orientation/resize; asset reloading/slow frames; system popup. Always release and never auto-fire on return.
5. Test right- and left-facing, walking/sprinting, camera tracking, stationary thumb and frame drops. Compare original procedural aim and use visual reticle, avoiding target occlusion. Cross-browser Linux WebKit is only partial evidence; actual iPhone 16 Safari is required.
6. Record target metrics in a local diagnostics overlay (off by default): **zero accidental shots** in a specified test battery, no stale shots after cancel, and low frame-to-tick input delay. Choose numerical latency targets after measuring hardware, not by assuming 60/120 Hz.
7. Build *original* PlayerController->weapon/bolt/magazine/projectile firing state before enabling live shots. Until then, show pending weapon UI. Adapter state changes alone do not constitute functioning automatic fire.

## Status of changes

- On this branch: `game/browser-input.mjs` now validates the final release displacement and updates final drag aim without misfiring; 16 isolated JS input tests passed (not full browser/iPhone).
- On this branch: `game/index.html` now honors `env(safe-area-inset-left/right/top)` for controls/HUD/errors (no physical iPhone verification).
- Pending: stationary hold recognition, per-pointer held fire source, arbitration with queued taps, latency instrumentation, relative-delta aim mapping, player-tunable thresholds, end-to-end recovered weapon execution and actual iPhone tests.

## Decision

For the user-requested no-extra-button default, try the one-surface scheme with initial **210 ms hold time and 10–12 CSS px movement slop**. Do an iPhone 16 test and tune; retain a switchable dedicated FIRE button if sustained firing onset feels slow or unintended shots remain. Avoid inventing original firing systems or claiming shot-count tests until the recovered weapon chain is present.

## Execution update — same date, later commits

**Completed on the active PR branch:** `game/browser-input.mjs` now has a per-pointer `pending → aiming` or `pending → held` gesture state machine, with default **210ms** stationary hold time, **11 CSS px** drag threshold, cancellation/reset and held-then-drag aiming. The hold timeout is used **only to recognize touch intent**, not to schedule shots or simulate the original firing rate. The adapter exposes `onTap`, `onHoldChange` and `onGesture`.

`game/tap-fire-queue.mjs` now merges discrete tap pulses and independent held-trigger input sources into one boolean sampled by the original C# `InputHandler` each `Tick`. It guarantees a release tick between successive taps or consecutive held gestures, bounds queued taps and expires stale taps (500ms by default). Pressed/released state does not imply the original weapon emitted any bullet.

`game/index.html` connects these states to the original C# input bridge and displays an immediate `TOUCH`/`AIM`/`TRIGGER HELD`/`TRIGGER TAP` hint. The UI explicitly marks all fire actions as **WEAPON PENDING**. Original weapon/projectile/ammunition/recoil systems are still absent and FIRE/RELOAD/SWAP controls remain disabled.

**Validation:** Source-fetched device adapter and pulse-queue regressions **34/34 passed** in an isolated JavaScript EventTarget stand-in harness. New `scripts/touch_gesture_bridge_test.mjs` integrated raw-input test cases **5/5 passed** using the same stand-in execution. The workflow has been updated to include this third regression suite. Native Node/GitHub Actions browser CI and physical iPhone 16 Safari validation of the new commits are **not yet confirmed**.

**Hosting blocker:** GitHub pushes trigger Vercel preview deployments reporting READY on the feature branch, but attempts to inspect `/game/`, `/runtime-web/`, and obtain temporary share links through the current Vercel connection all returned **403 forbidden** on deployment-protection/team-scoped endpoints. Therefore **none is verified to load the assembled game** and none should be presented as an iPhone-ready test link. The older ChatGPT Site `crisis-response-runtime.craftysnipe13.chatgpt.site` is explicitly at an earlier commit and cannot test these changes. Current tool inventory offers no first-party Sites publishing action; a new Site requires ChatGPT Work/Sites or another properly authorized deploy surface.

**Next priorities:** (1) inspect/run full game + Blazor WASM browser smoke on this head, (2) deploy the actual prepared site to a reachable private host with proper permissions and test a real iPhone, (3) fix absolute-pointer aim snap by implementing and validating offset/relative movement in the browser input mapping while preserving original C# camera/IK, (4) tune thresholds using measured missed-tap/false-shot data, (5) preserve original weapon system source provenance and connect projectile/bolt/magazine mechanics before enabling real gunfire.

**Supersession notice:** Early prose and the earlier `docs/TOUCH_FIRE_MODES_AND_SITES_PLAN.md` discuss a default dedicated FIRE button; the user subsequently chose **one-finger tap, hold, drag and hold-then-drag** as the primary layout. The dedicated button is now an optional accessibility fallback, not the default.
