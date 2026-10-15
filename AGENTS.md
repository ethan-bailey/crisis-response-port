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
