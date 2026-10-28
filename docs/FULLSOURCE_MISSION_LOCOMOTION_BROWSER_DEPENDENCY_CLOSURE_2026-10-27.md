# FullSource Mission locomotion browser acceptance

## Acceptance target

On the integrity-verified FullSource Mission package, verify that a fresh Left-key input reaches the original player controller after touch aim and sprint interactions. Require source input state, direction-correct position change, original source movement flags, authored locomotion selection, release to stand, finite position, and no browser exceptions. Position or velocity must not be assigned by the smoke, and the source contact synchronization must remain active.

## Dependency closure

| Layer | Source owner and dependency | Acceptance evidence |
|---|---|---|
| Mission package | `BrowserEntryTeamMission.Start`, original Operator prefab, Mission 01 scene, and verified FullSource asset manifest. The active seven-root profile contains 1,920 assets. | Packaged browser diagnostics report all profile assets and expected source/compressed bytes verified before controls run. |
| Browser input | `game/index.html` maps KeyA to left; `game/browser-input.mjs` normalizes and releases key state; `FullSourceActorBridge` forwards original `InputHandler.Keys`. | `originalInput.left.Pressed` is true while held and false after release. |
| Original movement | `PlayerController.Update` reads the original input handler and calls `CharacterBody.Move`; `FixedUpdate`, contact integration, and source synchronization retain authored ordering. | A fresh held-key sample moves left in the clear spawn-side corridor and reports source movement. No transform/velocity injection or contact bypass. |
| Authored animation | `game/source-character-step.mjs` projects original movement/facing to the recovered AnimatorController and its Body walk/backward/sprint states. | While held, require source `IsMoving` and an authored locomotion state; after release require settled velocity and `Body stand`. |
| Error handling | The FullSource actor task continues during keyboard input and the smoke captures browser page errors. | Require no page errors and finite settled position. |

## Packaged step 29 results

The earlier run #155 (`40537826305`) failed on source SHA `3ba69af948434a5aabb535cbda79173507ecb493`. Left was pressed and the original acceleration command was `[-22.499994, 0, 0]`, but `Time.deltaTime` was `0.33333334` while browser contact used `0.05`; `MovementDelta=-0.025131702` stayed below the source moving threshold `0.033333335`, leaving `Body stand`.

Run #157 (`40572335603`) tested source SHA `3a33a6df7d74681cf4074976ec9ca9d3845a56ea`, which contains both the TimeManager profile root and frame-clock alignment. Packaged step 29 **passed**: Left was true; X changed `−0.6600008 → −0.8176373`; source velocity was `−0.7484165`; `MovementDelta=−0.030167103` exceeded the aligned threshold `0.0050000004`; `IsMoving` and `IsMovingBackwards` were true; the original state was `Body walk backward`. On release, X settled at `−1.1009204`, velocity was approximately zero, and the state returned to `Body stand`. This is the first packaged pass after the frame-clock change.

Run #161 (`40694642865`) tested source SHA `630d6f71dd64c4cf71ad50cf5d12b542a458e8e8`, including the 2026-10-28 movement-after-contact commits. Packaged step 29 **passed**: Left was true; X changed `−0.6600008 → −0.8176211`; contact moved root Y `1.3901279 → 1.2841799`; source velocity was `−0.74841595`; `MovementDelta=−0.048979104`; both source movement flags were true; `Body walk backward` was selected. On release, X settled at `−1.1009043`, velocity was approximately zero, and the state returned to `Body stand`.

Compared with #157, the held and released X positions and velocity were nearly unchanged. `MovementDelta` changed from `−0.030167103` to `−0.048979104`; the contact-tick work changed that source reading while preserving the passing outcome. The earlier candidates are no longer blocking this acceptance slice.

## Independent reload acceptance

The workflow's `--reload-only` path skips locomotion controls while retaining full package loading, source actor startup, grounding checks, trusted source reload input, arm-pose comparison, original audio callbacks and playback, settling checks, zoom protection, and page-error capture. Run #161 passed this independent packaged Chromium acceptance after the isolation introduced by commit `e727b9c7`. It observed original magazine-out/in audio and a settling arm pose. The active magazine instance changed; round and reserve counters stayed at 30 and 2, so a complete ammo-count transition remains untested.

## Remaining scope

The packaged locomotion and reload-only slices passed on desktop Chromium. Rendered gait/IK and foot-contact appearance, movement reversal, sprint thresholds, full pointer/focus/zoom coverage, completed ammo-count transition, and iPhone Safari behavior remain untested. This evidence does not claim complete Gate 2, Gate 3, or overall parity.
