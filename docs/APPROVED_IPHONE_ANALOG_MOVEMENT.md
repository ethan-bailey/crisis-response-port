# Approved iPhone analog movement enhancement

**Status:** Explicitly approved by the project owner on 2026-10-16 as a product improvement. This is a deliberately permitted difference from strict original-game behavior, **not a missing original system, an approximate replacement, or a feature that should be reverted in a future “restore 1:1” pass**.

## Behavior to preserve

- Drag the iPhone movement stick **left or right** to choose the same original `left`/`right` InputHandler actions; no new replacement locomotion model.
- Inside the 22% horizontal dead zone, speed intent is zero and neither direction is held.
- Outside it, displacement is linearly normalized from 0 at the dead-zone edge to 1 at full travel. The original Operator walk-speed or sprint-speed **target** is multiplied by this magnitude; source acceleration law, Unity-style ForceMode.Acceleration mapping, PhysX, contact/friction, source rotation and animations are retained.
- Keyboard/digital movement remains at 1.0 intensity, including when a stick is also held. Existing left-precedence input handling is unchanged.
- The sprint action remains available, with its existing source facing restriction and sprint-speed cap; analog displacement also scales the sprint target.
- Release, cancel, pointer-capture loss, focus loss, tab suspension, resizing and disposal must reset the stick's magnitude/visuals. An independent keyboard key remains held when a stick finger lifts.

## Enforcement for future agents

**Do not silently remove, discretize, or overwrite proportional joystick movement** when integrating more of the original `CharacterBody`, `PlayerController` or `InputHandler`. Preserve this isolated input adapter as a user-approved enhancement, explicitly documenting any future alteration and obtaining approval for intentional behavioral regression. Keep developer-authored rules and animation source faithful otherwise.

Relevant implementation: `game/browser-input.mjs` (`bindMovementStick`, `movementMagnitude`) and `game/index.html` (the original walking/sprinting target-speed calculation). The original keyboard action transport remains via `runtimeCall("Tick", ...)`.

Focused regression: `node --test scripts/browser_input_test.mjs` checks half pull, full pull, dead zone, reverse, digital keyboard coexistence, pointer cancellation and focus/cleanup. Full WASM/Chromium/WebKit test and physical iPhone evaluation occur at the next gameplay milestone; tests alone are not native fidelity.

## Product philosophy

The owner permits **deliberate, beneficial usability enhancements**, especially on iPhone, if they improve the final product. This permission does **not** authorize placeholder or invented substitutes for recovered original gameplay systems. Explicitly flag each enhancement in a permanent document and separate it from claims of exact original-game parity.
