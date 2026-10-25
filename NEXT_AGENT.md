# Crisis Response Port — current handoff

## Current source checkpoint

- Branch: `develop/wasm`. Base before this batch: `e5bf54fdf39afb77efab363c194e7cf680f5d583`.
- The main page keeps a lightweight startup and offers an explicit `Start original Mission · 98 MB` action. It then sequentially downloads the verified six-root Mission profile, initializes the authored Mission 01 EntryTeam/UI through the browser runtime, routes aim into the original WeaponPointer target, advances the original FullSource actor, synchronizes browser PhysX state, and projects that actor into the page renderer.
- The browser startup helper loads the original Global Context/UI and Mission 01, selects the EntryTeam, and validates one controlled source actor. The controlled player's movement, aim, UI press/release, and position/velocity round trip are covered by the host integrated contract.
- Source package `global-ui-mission-operator`: 1,919 assets, 22,365 records, 97,767,549 compressed bytes plus 0.85 MB metadata (about 98.6 MB total); 205,514,345 uncompressed source bytes. Mission profile is opt-in after the lightweight page is usable, not part of default first paint.

## Verification from this batch

- Incremental Mission profile integrity: passed; 1,919 assets; lazy document release verified.
- Authored Mission UI contract: passed; Mission 01 started, InteractionMenu active, original press/release and source movement/physics checks passed.
- Exact browser EntryTeam session contract: passed; Mission 01, original UI, aim/input, movement force, and position/velocity round trip.
- Runtime checks: passed. Foundation checks: 121 assertions passed. Engine checks: 69 assertions passed.
- Browser JavaScript checks: 31/31 passed; module syntax check passed.
- Browser runtime build and Release WASM publish passed with .NET SDK 8.0.426.
- iPhone Safari and Playwright acceptance were not run; keep device/browser acceptance open.

## Runtime limits still present

- Mission 01 imports unsupported or unresolved Unity built-ins and components (including Sprite, Mesh, Shader, ReflectionProbe, camera fields, audio, and several gameplay scripts). The Mission load succeeds with explicit unresolved/unsupported diagnostics; do not claim full Unity parity.
- The browser EntryTeam path disables unsupported behavior-tree/vocalization hooks, uses a silent source audio adapter, and ignores non-player rigidbody force commands. Only the controlled player's physics state is synchronized into browser PhysX. NPC/player interactions and Mission audio are not validated.
- Existing gunshot audio hardening is in source but is not yet accepted on iPhone Safari. It drops stale events (>120 ms old or >50 ms future) and reduces gain (master 0.16, fire multiplier 0.6).
- The page-level Mission source path has static checks only: no Playwright/browser binaries are available in this environment. The lightweight page and mobile behavior still need on-device acceptance.

## Release state and next action

- Source has not yet been committed/pushed for this batch. After committing this checkpoint, regenerate the Release WASM output and verified site package from that exact commit, push it to `develop/wasm`, sync the packaged static output into the existing owner-private Crisis Response Runtime Site source, save/deploy that version, and verify its production status and URL.
- The live Site remains version 43 until the new release finishes.
- User explicitly skipped the manual device-testing step. Do not block the authorized build/deploy on it; report iPhone acceptance as open and provide the deployed URL and the short manual acceptance steps.
