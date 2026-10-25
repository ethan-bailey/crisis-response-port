# Crisis Response Port — current handoff

## Checkpoint

- Active branch: `develop/wasm`; verified live source commit:
  `44fc0600ded9d8c2a37a4ff534183287345d9810`. A documentation-only checkpoint
  commit may follow it. Fetch the live branch before resuming.
- The worktree was clean after fetching the source commit. No open PRs.
- Never deploy. Site v39 remains the last owner-accepted iPhone Safari build.
  Desktop browser automation is optional. Obtain owner iPhone Safari testing at
  the next major playable milestone and before release; do not claim device
  acceptance for behavior absent from that tested build.
- Keep screen-hold aiming distinct from holding the Fire button; it remains
  documented for later.
- Batch-first applies across development. Inspect the relevant dependency
  group, fix supported related issues together in dependency order, then run
  the combined relevant checks. See `AGENT_INSTRUCTIONS.md`.

## Latest source batch

- Added a FullSource actor physics round trip. Each actor tick now returns its
  fixed-step count and source Rigidbody acceleration commands. The bridge can
  accept the browser PhysX actor's finite pose and velocity into the original
  FullSource Rigidbody and Transform. Existing actor force observers are
  preserved. Unsupported force modes and force vectors fail closed.
- Added the WASM-callable `SynchronizeFullSourceActorPhysics` endpoint and
  extended the PhysX smoke contract to check actor pose and velocity sync.
- The active EntryTeam Mission UI contract passed: original left press/release,
  one source movement acceleration, and a browser-physics-to-source pose and
  velocity round trip. Default weapon/reload/ballistics contracts also passed.
- Exact source tested: `44fc0600ded9d8c2a37a4ff534183287345d9810`.
  Verification included Release runtime and browser/WASM builds; EngineChecks
  69; FoundationChecks 121; active Mission UI actor-input/physics contract;
  default runtime, authored IK, weapon/reload, and ballistics contracts;
  browser input 19, source movement 5, aim 10, animator 4, loading 2, magazine
  HUD 8, actor migration audit, packaging tests 13, and audit tests 14. The
  corrected audit invocation was `python3 scripts/audit_tools_test.py`.
- Release site package built from this SHA and verified: 767 files, 195 WASM
  binaries, 199 hashed Blazor resources. Build output is local evidence only;
  nothing was deployed. The Playwright based PhysX browser smoke was not run
  because Playwright/browser binaries are unavailable here; desktop browser
  checks are optional and no device acceptance is claimed.
- Evidence:
  `docs/evidence/FULLSOURCE_PHYSICS_BRIDGE_2026-10-25.md`.

## Gate state and known limits

- Gate 0: open. Normal-page FullSource flow, native physics, authored actors,
  and broader navigation/presentation remain.
- Gate 1: passed within scoped headless original-character acceptance; not full
  game parity.
- Gate 2: partial. Original controller/sequencer callbacks, Operator two-arm
  IK, actor isolation, snapshots, authored UI initialization, input press and
  release, movement force export, and PhysX state import pass in test hosts.
  `game/index.html` still uses browser-side movement/animation and transport
  `WeaponHost`; it does not call the FullSource actor endpoints or project
  actor snapshots. Weapon/equipment callbacks and physical continuity remain.
- Gate 3: partial. Source projectile-to-physiology evidence exists; rendered
  wounds and complete live state/event integration remain.
- Gate 4: partial. Headless mission startup/spawning/outcome evidence exists;
  authored mission inputs and instantiation callbacks on the normal page
  remain.
- Gate 5: open. Original AI perception, navigation, decisions, and combat on
  the playable path remain unverified.
- Gate 6: partial. Headless room-clear/success contracts pass; complete normal
  path mission flow remains.
- Gate 7: open. Original presentation and content fidelity remain.
- Gate 8: closed only for the previously owner-accepted Site v39. No new
  gameplay behavior or device-acceptance claim is implied; nothing was
  deployed.
- Keep the full source closure off default phone startup. Current full package
  is 251,585,827 JSON bytes. The indexed Global Context + Mission 01 + Operator
  and weapon closure is 1,918 assets / 22,361 records / 122,132,020 source
  bytes; reduced JSON measured 212,051,283 bytes and 95,889,011 gzip bytes.
  Site chunking still requires reassembling and parsing the whole package. The
  original browser layout driver and several built-in assets/components also
  remain unsupported. Preserve these limits; do not imply a browser migration
  or phone-ready source path from the bridge contract alone.
- Use pinned .NET SDK 8.0.426 and the process shim as documented in
  `docs/DOTNET_BUILD_RUNBOOK.md`. Do not retry `dotnet-install.sh` or GitHub
  CLI login. Push through the connected GitHub API and verify with `git fetch`.

## Exact next action

Continue the actor-owned browser path in one dependency batch. First design and
verify a bounded source-package delivery or loading approach; do not put the
full closure on default iPhone startup. Then route `game/index.html` action
levels/frame ticks through `TickFullSourceActorBridge`, apply returned
acceleration commands to the contact PhysX actor, send its result through
`SynchronizeFullSourceActorPhysics`, and project the returned actor-owned
transform hierarchy into the renderer. Migrate the weapon/equipment callbacks
to that same actor before removing transport authority. Run combined source
lifecycle, UI, input, movement, IK, weapon, browser build, and package-integrity
checks once after the batch. Request owner iPhone Safari acceptance only when
the resulting build is a major playable milestone. Commit and push verified
changes through GitHub API, verify with Git fetch, and never deploy.
