# Crisis Response Port — current handoff

## Checkpoint

- Branch `develop/wasm`, current source `a075b24ddcf791bbfe452ea12005523106da46a8`.
- Ease-in walk curve: quadratic speed ramp from the 16% dead zone to walk cap
  at the rim.
- Joystick is 144px on standard landscape and 128px on short landscape; thumb
  is 56px / 48px respectively.
- Sprint detent: the puck holds just inside the rim during over-pull. At
  1.42x horizontal travel it pops fully beyond the rim and opens the ring;
  it releases at 1.24x. This is visual/interaction resistance, not physical
  force feedback.
- Vertical joystick aim adjusts existing aim incrementally; neutral and release
  preserve it. Package integrity passed at 769 manifest files, 199 Blazor
  resources and 195 WASM files. No automated interaction or iPhone acceptance
  tests were run in this turn.

## Deployment

- Runtime Site version 43 deployed successfully:
  https://crisis-response-runtime.craftysnipe13.chatgpt.site
- Site source commit `803172b402dac4d8613b14ffe49cb0e2348a3661`.
- Game source commit `a075b24ddcf791bbfe452ea12005523106da46a8`.
- Site remains owner-only. iPhone Safari acceptance and Playwright PhysX
  browser smoke remain open.

## Next work

1. Ask owner to assess joystick size, visible full puck breakthrough, Ease-in
   walking, and vertical aim preservation on iPhone Safari.
2. Verify sprint release, aiming while moving, and firing on device.
3. Investigate delayed, extremely loud gunshot audio; implement targeted
   synchronization and gain fix.
4. Continue bounded source delivery and wire input, physics, rendering, and
   weapon callbacks through the same actor. The normal page still uses
   browser-side movement and the transport weapon host.
