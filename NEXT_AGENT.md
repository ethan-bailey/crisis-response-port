# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`.
- Tested and packaged game source: `df62cd34d0f5331435bd085707dcded41f6c77f1`.
  This contains the deliberate sprint joystick change and restores the
  Three.js import map required by the bounded WASM Site packager.
- Joystick implementation commit: `f8f2391574894c967df49dadc1d712fe641f0b6d`.
- Checks recorded at the source checkpoint: browser/touch input 19/19;
  source movement 5/5; WASM Site package tests 14/14; Release WASM publish
  with pinned .NET SDK 8.0.426 passed. Optional `wasm-tools` workload was
  unavailable.
- The verified source package contains 769 files. Integrity verification
  passed before deployment.

## Joystick change

The circular two-axis control keeps vertical aiming and its 16% dead zone.
Horizontal input scales walking to the walk cap by 70% travel, then holds
walking speed. Sprint requires a deliberate 1.75x horizontal over-pull; once
active it remains on until pull returns to 1.5x or less. A gold outer ring
signals sprint. The source movement/physics path and keyboard Shift sprint
are unchanged. Behavior and acceptance steps are recorded in
[approved iPhone analog movement](docs/APPROVED_IPHONE_ANALOG_MOVEMENT.md).

## Deployment

- The existing Crisis Response Runtime Site was deployed successfully as
  version 40 at https://crisis-response-runtime.craftysnipe13.chatgpt.site.
- Site source commit: `f9120014253e87927e3107703c356704398ede56`. Its
  release manifest identifies game source `df62cd34d0f5331435bd085707dcded41f6c77f1`.
- Deployment status: succeeded. The published archive SHA-256 is
  `8cb17aab51caa50560a1b428dbbcc09e102acb0baf40465e0ba3a9b06905fbd5`.
- Access remains custom and owner-only; no access settings were changed.
- Browser Playwright PhysX smoke and iPhone Safari acceptance remain open.
  The browser smoke could not run here because Playwright and browser binaries
  were unavailable. Do not claim device acceptance from the deployment.

## Recent iPhone memory result

The owner completed the separate Memory Check Site run: all 1,230 assets
arrived in 270,623 ms. Peak managed allocation was about 65.7 MiB; managed
heap was about 71.3 MiB. Safari exposed neither JS heap nor total device memory.
Delivery metadata and final sample byte counters disagreed; see
[phone memory evidence](docs/evidence/PHONE_SOURCE_MEMORY_CHECK_2026-10-25.md).

## Deferred audio issue

The owner reported delayed gunshot audio that later plays extremely loudly and
out of sync with firing. A fix was requested for after this deployment section;
it is not part of version 40. Investigate the weapon audio event lifecycle,
queueing, and gain/voice cleanup before making a targeted fix.

## Next work

1. Have the owner check version 40 on iPhone Safari, including slow walk,
   deliberate sprint engagement/release, aiming while moving, and firing.
2. Run the Playwright PhysX browser smoke when Playwright and browser binaries
   are available.
3. Draft and implement the deferred gun audio timing/volume fix, then verify
   shot-to-sound synchronization and bounded gain.
4. Continue bounded source delivery and wiring page input, physics, rendering,
   and weapon callbacks through the same actor. The normal page still uses
   browser-side movement and the transport weapon host.
