# FullSource actor movement and browser PhysX bridge — 2026-10-25

## Source checkpoint

- Base branch head fetched before changes: `6cdfa15b67becaf6c466451c8a717c96e7f7f9d8`.
- Tested source commit: `44fc0600ded9d8c2a37a4ff534183287345d9810` on `develop/wasm`.
- The source commit was pushed with the connected GitHub API and matched after
  `git fetch origin develop/wasm`. No deployment was performed.

## Changes

- `FullSourceActorBridge.Step` captures movement acceleration emitted by the
  original player's Rigidbody during its source fixed updates. The response
  contains the number of fixed steps and ordered force commands. An existing
  per-actor force observer is invoked and preserved.
- The bridge exports browser PhysX's finite position and velocity back into the
  same original FullSource Rigidbody and Transform. Invalid numeric state is
  rejected. The browser endpoint is
  `SynchronizeFullSourceActorPhysics`.
- The contact-physics adapter gained a pose and velocity synchronization method;
  the browser PhysX smoke now asserts the supplied state is visible in the
  returned actor snapshot.
- The active Mission UI contract asserts a source left action emits exactly
  one negative X acceleration for its fixed step, and a PhysX result is
  reflected in the original actor's position and velocity. The original input
  press/release checks remain.

## Verification

Pinned .NET SDK 8.0.426 with `/tmp/libprocshim.so`:

- Runtime Release build: passed, 0 errors.
- Runtime Checks, EngineChecks, and FoundationChecks builds: passed.
- EngineChecks: 69 assertions passed.
- FoundationChecks: 121 assertions passed.
- Active EntryTeam Mission UI contract: passed; original interaction UI active,
  2 Operators and 3 Occupants spawned, bridge press/release observed, one
  actor-owned negative movement acceleration exported, and finite pose/velocity
  test values synchronized back to the original actor.
- Default Runtime Checks: lifecycle/reference, authored IK, presentation,
  weapon/reload, and ballistics contracts passed. Weapon contract retained the
  source 31-round chamber-plus-magazine state, source reload, and no-infinite-
  ammunition assertions.
- Browser input: 19 passed; source movement: 5; aim direction and fire gate:
  10; source animator: 4; loading: 2; magazine HUD: 8.
- Actor migration audit: 0 violations. Package tests: 13 passed. Audit tests:
  14 passed using `python3 scripts/audit_tools_test.py`.
- Release Blazor WASM publish passed. Site package built from the tested source
  commit and verified: 767 files, 195 WASM binaries, 199 hashed Blazor
  resources.

The Playwright-based browser PhysX smoke was not run: this checkout does not
have Playwright or its browser binaries. Desktop browser tests are optional.
This is source/runtime and package evidence, not browser gameplay or iPhone
Safari acceptance.

## Remaining integration work

`game/index.html` does not yet call the FullSource actor endpoints. Its current
movement, animation, and weapon presentation remain under browser-side and
transport hosts. The new force/state contract enables the physics round trip,
but the normal page still needs action/frame wiring, transform projection, and
weapon/equipment callback migration before transport authority can be removed.

Keep this source closure out of default phone startup. On the available full
package, the JSON is 251,585,827 bytes. The indexed Global Context + Mission 01
+ Operator/weapons closure has 1,918 assets and 22,361 records (122,132,020
source bytes); its reduced JSON measured 212,051,283 bytes and gzip measured
95,889,011 bytes. Current site chunking reassembles and parses the complete
package, so it is not a bounded phone load path. The actor bridge contract
does not resolve that payload or prove browser/mobile behavior.
