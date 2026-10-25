# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; current source `f527059d064b6958555033c6673ce83a345f997b`.
- This restores the complete `game/index.html` from checkpoint
  `e009c00d240b63e67d29f9499d4740bbc6669f55`, then adds the verified Three.js
  import map. A previous truncated tool output had accidentally been committed
  as the first lines of the page and removed roughly half its content.
- The restored game page is 44,654 bytes, has no truncation-warning text, and
  package integrity verification passed: 769 manifest files; 199 Blazor
  resources; 195 WASM files.
- Joystick implementation commit: `f8f2391574894c967df49dadc1d712fe641f0b6d`.
  Browser/touch input 19/19, source movement 5/5, and WASM package tests 14/14
  were recorded at the preceding source checkpoint. Release WASM publish used
  pinned .NET SDK 8.0.426; optional `wasm-tools` was unavailable.

## Joystick change

The circular two-axis control keeps vertical aiming and its 16% dead zone.
Horizontal input scales walking to the walk cap by 70% travel, then holds
walking speed. Sprint requires a deliberate 1.75x horizontal over-pull; once
active it remains on until pull returns to 1.5x or less. A gold outer ring
signals sprint. The source movement/physics path and keyboard Shift sprint
are unchanged. Behavior and acceptance steps are recorded in
[approved iPhone analog movement](docs/APPROVED_IPHONE_ANALOG_MOVEMENT.md).

## Deployment

- Corrected build deployed successfully as Site version 41 at
  https://crisis-response-runtime.craftysnipe13.chatgpt.site.
- Site source commit: `b3c484a11ad7269ebbd15aacae68a6b0e9050103`; packaged game
  source: `f527059d064b6958555033c6673ce83a345f997b`.
- Corrected archive SHA-256:
  `e5196c922cc94f11f3634689cebb0f20de8e69119f1199ae86a54e6ad8a644e2`.
- Access remains custom and owner-only; no access settings were changed.
- A cloud browser opened the page and confirmed the truncation warning is gone.
  That browser has WebGL disabled and reports “Error creating WebGL context,”
  so it cannot validate the rendered game. Playwright PhysX smoke and iPhone
  Safari acceptance remain open.

## Recent iPhone memory result

The owner completed the separate Memory Check Site run: all 1,230 assets
arrived in 270,623 ms. Peak managed allocation was about 65.7 MiB; managed
heap was about 71.3 MiB. Safari exposed neither JS heap nor total device memory.
Delivery metadata and final sample byte counters disagreed; see
[phone memory evidence](docs/evidence/PHONE_SOURCE_MEMORY_CHECK_2026-10-25.md).

## Deferred audio issue

The owner reported delayed gunshot audio that later plays extremely loudly and
out of sync with firing. A fix was requested for after this deployment section;
it is not part of version 41. Investigate the weapon audio event lifecycle,
queueing, and gain/voice cleanup before making a targeted fix.

## Next work

1. Have the owner check version 41 on iPhone Safari, especially that the scene
   renders and the character appears; then assess slow walk, deliberate sprint
   engagement/release, aiming while moving, and firing.
2. Run the Playwright PhysX browser smoke when Playwright and browser binaries
   are available.
3. Draft and implement the deferred gun audio timing/volume fix, then verify
   shot-to-sound synchronization and bounded gain.
4. Continue bounded source delivery and wiring page input, physics, rendering,
   and weapon callbacks through the same actor. The normal page still uses
   browser-side movement and the transport weapon host.
