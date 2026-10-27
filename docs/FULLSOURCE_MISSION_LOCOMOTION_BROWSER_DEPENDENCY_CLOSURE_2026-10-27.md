# FullSource Mission locomotion browser acceptance closure — 2026-10-27

## Acceptance target

On the integrity-verified 1,919-asset FullSource Mission package, verify that browser keyboard input reaches the original player controller after prior touch aim and sprint interactions. Capture a fresh source actor position after sprint release; send the original mapped left input through the open spawn-side corridor; verify the actual input edge, direction-correct source position delta, authored forward/backward locomotion clip selection, input release, settled stand state, finite position, and no browser exceptions.

The assertion must test movement caused by the new keyboard input. A pre-interaction position snapshot cannot serve as the post-sprint baseline. Do not assign transforms/velocity, invoke source controller methods directly, or bypass scene contact synchronization.

## Dependency closure

| Layer | Source owner and dependency | Acceptance evidence |
|---|---|---|
| Mission package | BrowserEntryTeamMission.Start with the original Operator.prefab, Mission 01 scene, and verified FullSource asset manifest; package includes 1,919 assets and serialized collider hierarchy. | Browser loads the exact workflow SHA and reports verified FullSource source delivery before controls are exercised. |
| Browser key path | game/index.html binds KeyA to left; game/browser-input.mjs normalizes keyboard events and releases them; runtime/Engine/FullSourceActorBridge.cs forwards action state to the original runtime input mapping. | Snapshot reports originalInput.left.Pressed while key is down and false after keyup. |
| Original player lifecycle | Recovered PlayerController.Update reads InputHandler.Keys.left.Pressed, sets direction −1 and calls owner CharacterBody.Move(direction). It continues handling source aim and facing in the same owner update. | Position must move left relative to a fresh, immediately pre-input position. |
| Source movement and collision | runtime/Original/CharacterBody.Full.cs: Move sets acceleration for nonzero input; FixedUpdate applies authored walk speed/force; FullSource source ticking and browser contact projection preserve scene collision. The recovered Mission contains the counter obstacle at the rightward end of the brief joystick sprint trajectory. | Use the clear spawn-side/left corridor after sprint, not the already-blocked rightward counter approach; assert a new negative position delta. Do not edit position or skip the contact step. |
| Authored animation | game/source-character-step.mjs derives moving/backwards from source velocity and facing; game/source-character-animator.mjs evaluates the recovered AnimatorController transitions and blend tree in game/assets/operator/character-animator-controller.json. Its clips include Body walk, Body walk backward, and Body sprint. | Accept an authored walk, backward-walk, or sprint blend clip while source motion is active; after release require velocity <0.05 and Body stand. |
| Error/recovery | FullSource task continues running while keyboard input is sent; browser pageerror capture remains active. | Require no page errors and a finite settled position. |

## Exact failure and repair

- **Failed**, run #146 40501400977, source SHA df4b8b882e0b7f14b6466590edc0d87142095813: all contracts, pinned .NET SDK 8.0.426 Release/WASM build, source recovery/decompiler provenance, complete Mission package build/integrity, and profile IndexedDB acceptance passed. Independent Chromium IndexedDB/audio job 122179830600 passed. Only the packaged Mission browser job failed at scripts/mission_grounding_browser_smoke.mjs:230 waiting for Body walk / Body sprint.
- Retained browser report mission-grounding.json SHA-256 d82c0e79f0d3b67b14db581f65983293626e5e934a822513c87dda4680f49d23. The browser screenshot has a separate file hash; do not reuse the JSON hash for the image. The failed-run state is grounded at source X -0.6600008, Body stand, with left/right inputs released and no page errors.
- The smoke read state once after Mission startup, before it dragged the joystick through the sprint rim. After that sprint/release it reused stale state.playerX as walkStart, pressed KeyD, and could satisfy x > start + 0.1 using prior joystick displacement. The actor had already moved from spawn X -3 to -0.66, at the right-side counter approach; the subsequent rightward walk-animation wait could remain stationary. Thus the old test did not prove the keyboard caused movement and selected a blocked scene direction.
- The source-only repair captures a fresh post-sprint baseline, walks left through the clear corridor, waits for negative movement under originalInput.left.Pressed, and accepts the recovered backward-walk clip as well as forward walk/sprint blends. It changes no gameplay code or serialized content. The exact-source rerun is pending; the repair is not counted as package/browser pass until CI completes.

## Next action

Run and inspect the exact SHA containing this smoke repair. If the locomotion assertions pass, record package and browser artifacts/hashes, then continue at the next dependency-ready gate. If they fail, use the exact step log and retained timeline to add the newly observed source dependency before the next focused repair.