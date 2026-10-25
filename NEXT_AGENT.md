# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; source `23bbdcc4ff82f5544ea09d6ef7cb8998b82e573c`.
- Walking uses the selected Ease-in curve: after the 16% dead zone, horizontal
  speed follows a quadratic ramp to the walk cap at the joystick rim.
- Sprint is a separate virtual breakaway detent. Beyond the rim, thumb travel
  is visually damped against resistance; crossing 1.42x horizontal travel
  opens the outer ring and engages sprint. Sprint releases at 1.24x for
  hysteresis. On touch devices this is simulated resistance, not physical
  force feedback.
- Vertical joystick aim now adjusts the existing aim incrementally. Neutral
  stick input and release preserve the aim instead of resetting it to screen
  center.
- Joystick code is in `game/browser-input.mjs`; its live UI is in
  `game/index.html`. Pinned source SDK is .NET 8.0.426. This workspace did
  not have `dotnet`; the static input adapter was synced to the existing
  publish output before package integrity verification.
- WASM package integrity passed for 769 manifest files, 199 Blazor resources,
  and 195 WASM files. No automated interaction tests or device acceptance were
  run in this turn.

## Deployment

- Updated private Runtime Site deployed successfully at
  https://crisis-response-runtime.craftysnipe13.chatgpt.site (version 42).
- Site source commit: `6005704a269d34ff46d98cff7116e660d830a1cd`.
- Game source commit: `23bbdcc4ff82f5544ea09d6ef7cb8998b82e573c`.
- Owner-only access is unchanged. iPhone Safari acceptance remains open;
  Playwright PhysX smoke is also open.

## Remaining work

- Ask owner to assess Ease-in walking, the pull-through resistance/detent, and
  vertical aim preservation on iPhone Safari.
- Verify sprint release and aiming while moving on device.
- Investigate the reported delayed, extremely loud gunshot audio and implement
  the deferred timing/volume fix.
- Continue bounded source delivery and wire page input, physics, rendering,
  and weapon callbacks through the same actor. The normal page still uses
  browser-side movement and the transport weapon host.
