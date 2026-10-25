# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`.
- Joystick source change: `f8f2391574894c967df49dadc1d712fe641f0b6d`
  (`fix: make sprint joystick pull deliberate`).
- Tests at this exact source: touch/browser input 19/19; source movement 5/5.
- Pinned .NET SDK 8.0.426 Release WASM publish passed with 0 errors. The
  optional `wasm-tools` workload was unavailable.
- The source code is pushed. The handoff commit that follows records this
  tested source checkpoint.

## Joystick change

The circular two-axis control keeps vertical aiming and its 16% dead zone.
Horizontal input scales walking to the walk cap by 70% travel, then holds
walking speed. Sprint now requires a deliberate 1.75x horizontal over-pull;
once active it remains on until pull returns to 1.5x or less. A gold outer
ring signals sprint. The source movement/physics path and keyboard Shift sprint
are unchanged. Current behavior and acceptance are recorded in
[approved iPhone analog movement](docs/APPROVED_IPHONE_ANALOG_MOVEMENT.md).

## Deployment status

The existing owner-private Crisis Response Runtime Site is still live at
https://crisis-response-runtime.craftysnipe13.chatgpt.site, version 39.
Nothing was deployed, and its audience was not changed.

Two gates remain:

1. `scripts/package_wasm_site.py build` against source
   `f8f2391574894c967df49dadc1d712fe641f0b6d` stopped with
   `Original game import map missing`. The current repository
   `game/index.html` has no import map; the existing Site package input must
   be opened and reconciled before packaging. The .NET publish itself passed.
2. The Sites source helper failed opening the existing Runtime Site checkout
   with `ENOENT open .openai/hosting.json`. Its version 39 source checkpoint is
   `f81db068d09485cbb54cd7d0035065a0e5ab2222`; do not fabricate a manifest or
   switch to the separate Memory Check Site. Existing access is private/custom.

## Recent iPhone memory result

The owner completed the separate Memory Check Site run: all 1,230 assets
arrived in 270,623 ms. Peak managed allocation was about 65.7 MiB; managed
heap was about 71.3 MiB. Safari exposed neither JS heap nor total device memory.
Delivery metadata and final sample byte counters disagreed; see
[phone memory evidence](docs/evidence/PHONE_SOURCE_MEMORY_CHECK_2026-10-25.md).

## One next action

Recover the exact version 39 Runtime Site source checkout (including its
`.openai/hosting.json` and original game import map), reconcile it with
`develop/wasm` at `f8f2391574894c967df49dadc1d712fe641f0b6d`, then rebuild,
verify package integrity, and deploy privately to the same Runtime Site. Do
not publish until the exact source package passes integrity verification.
