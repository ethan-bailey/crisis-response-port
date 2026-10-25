# Crisis Response Port — current handoff

## Checkpoint

- Branch `develop/wasm`; game source `f3a5620773baaf0e1a4927b45416aadc5034faf3`.
- Audio hardening: discard audio/material events older than 120 ms or more than
  50 ms in the future; this prevents a blocked frame from replaying stale
  gunshots. Master gain is 0.16 (was 0.28); `audioFire` has an additional
  0.6 multiplier. Suspended-context and unloaded-buffer events continue to be
  dropped, never queued.
- Aim-on-stick now uses the aim position present at joystick touch-down as its
  baseline. Vertical stick motion offsets that position; returning to neutral
  and release preserve the current pointer aim.
- Joystick Ease-in, rim detent and large puck behavior from source
  `a075b24ddcf791bbfe452ea12005523106da46a8` remain in this commit.
- Browser input checks: 19/19 passed. WASM package tests: 14/14 passed.
  Package integrity passed for 769 manifest files, 199 Blazor resources and
  195 WASM files. JavaScript syntax and `git diff --check` passed.
- No .NET SDK is installed in this workspace. Browser adapter was copied to
  the existing publish output as its project content mapping specifies; no
  fresh .NET/WASM compile was run. No iPhone or Playwright run was performed.

## Steps 3–6 disposition

The complete actor integration is deferred as one dependency gate. Current
`game/index.html` does not call `InitializeFullSourceActorBridge`,
`TickFullSourceActorBridge`, or `SynchronizeFullSourceActorPhysics`. The
production page still drives movement/aim in browser code, uses the separate
`StepWeapons` transport host, and updates presentation through a distinct
host. The bridge requires an original Mission `Global.Context`, active
`PlayerController`, and authored `InteriorMode.UI.InteractionMenu` in the
same `RuntimeWorld`; it explicitly refuses to step without that UI. Wiring
around the missing authored context would create competing simulation state.
The optional scene-smoke load does not initialize this player bridge. Resolve
and verify the Mission actor/UI load path before attempting batches 3–6.

## Deployment

No new Site version was deployed. Repository instructions prohibit publishing
an intermediate build before the required integration gates pass. The existing
owner-private Runtime Site remains version 43:
https://crisis-response-runtime.craftysnipe13.chatgpt.site

## Remaining work

1. Resolve the authored Mission actor and InteractionMenu load path in one
   RuntimeWorld, then wire page input, physics synchronization, weapon actions,
   and rendering/IK to that same actor.
2. Once that gate is implemented, run the integrated mission smoke and
   exact-source package/build checks; publish only after those required gates
   pass.
3. Ask the owner to check gunshot timing/volume, aim retention, and joystick
   behavior on iPhone Safari. Audio changes have not had device acceptance.
