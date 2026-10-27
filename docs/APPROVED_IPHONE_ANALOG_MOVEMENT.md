> **Current normative behavior:** The first section below, **Approved circular iPhone joystick and icon controls (2026-10-16)**, supersedes the older horizontal 22% dead-zone/slider instructions in the second section. The joystick is circular with a **16% horizontal dead zone**, proportional walking and no touch sprint button. A later owner-requested adjustment gives walking most of the visible travel and requires a deliberate over-pull to sprint; see the updated sprint mapping below. Preserve the original developer movement/physics. See [current operations board](history/REPOSITORY_OPERATIONS_2026-10-17.md).

# Approved circular iPhone joystick and icon controls (2026-10-16)

The owner explicitly requested replacing the horizontal slider with a two-axis circular thumb joystick and removing the sprint touch button. This **supersedes the earlier 22% slider-specific dead zone and separate touch Sprint control**. Do not revert to a horizontal slider or require a sprint button during later source parity work.

- Joystick follows thumb displacement in X/Y, clamps to its circular boundary, and springs back to center on release.
- Since Crisis Response is a side-scrolling shooter, only horizontal displacement drives original left/right InputHandler movement. Vertical thumb freedom does not create a new jump or crouch action.
- 16% horizontal dead zone rescaled to normalized 0–1 magnitude. Within 16%, no movement. At full pull, target speed reaches original sprint speed.
- Horizontal pull is proportionally mapped to walking speed through 70% of the visible stick travel, then holds the original walk cap. Sprint requires a deliberate horizontal pull of at least 1.75× the calculated stick travel beyond the center; after activation it remains on until pull falls to 1.5× or less. This outer-zone hysteresis prevents accidental activation and flicker. The sprint target still uses the original CharacterBody acceleration/physics solver and source facing restriction.
- Touch sprint is automatic; original Shift-key sprint is retained for keyboard movement. Independent keyboard directional inputs remain at full intensity and override analog deflection, even when both sources are held.
- Kneel, Stand and Fire retain their original actions and accessibility labels, but use inline line icons. The magazine UI remains a functional, visual ammunition control and is not replaced with a generic reload button.
- Pointer capture, concurrent second touches, lost capture, cancellation, page background, resize, blur, and disposal reset input and visuals. Full-power ring is visual feedback only; no gameplay changes to weapons, ammunition, aim, or reloading.
- The sprint state has a distinct gold outer-ring feedback. Do not trigger sprint from merely crossing the visible rim; this was explicitly changed after the owner reported accidental sprinting and poor speed control.
- Tests: `node --test scripts/browser_input_test.mjs` plus the existing source-backed movement runtime and browser build tests. Do not claim physical iPhone evaluation until user tests.
- This is an owner-approved user-interface enhancement, not a rewritten replacement for developer gameplay systems.

---

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


## Packaged Mission size and rim acceptance — prepared 2026-10-27

The source input algorithm remains on the approved 1.75× sprint-entry / 1.5×
exit hysteresis. A dependency-ready batch based on live parent
`747cb9494758b8c2cf759e060a6e893b107f4bec` increases the stick's mobile minimum
from 144px to 176px (max 208px) in both active CSS definitions, increases the
standard thumb from 56px to 64px, and raises both compact-landscape overrides
from 128px/48px to 176px/56px. Its actual packaged FullSource browser smoke
additions measure the rendered stick, preserve aim on touch-down/release,
exercise vertical and horizontal-only movement, test below/above the sprint wall,
and require the puck to clear the rim fully. Local browser-input tests pass
20/20 and full Node passes 169/169, but these new packaged-page assertions have
not yet run in hosted CI and iPhone Safari feel/zoom remain untested. Do not
claim the enhanced physical-device experience until owner-device acceptance.
