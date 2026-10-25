# Crisis Response Port — current handoff

## Source checkpoint

- Work targets `develop/wasm`, with the earlier verified tree at `e5bf54fdf39afb77efab363c194e7cf680f5d583`.
- Release history includes a transient truncated upload (`9b09d5b`) and its immediate corrective revert (`ef645fa`). Do not build or deploy `9b09d5b`; it was cut off at an 8 KB upload boundary. The complete source change is rebuilt and reverified after the recovery commit.
- The lightweight page still starts first. It now offers `Start original Mission · 99 MB`, which sequentially verifies and loads the six-root original Mission profile, starts the authored EntryTeam UI in the same source world, routes aim to the original WeaponPointer, ticks the FullSource actor, synchronizes its position/velocity through browser PhysX, and projects the original actor hierarchy into the page renderer.
- Profile `global-ui-mission-operator`: 1,919 assets, 22,365 records, 97,767,549 compressed asset bytes plus metadata (about 99 MB decimal), 205,514,345 uncompressed source bytes. It is opt-in and stays off default first paint.

## Verification completed on the release candidate

- Incremental Mission profile integrity passed; lazy hydrated document release passed.
- Mission UI contract passed: Mission 01 started, original InteractionMenu active, original input press/release and source movement/physics bridge checks passed.
- Browser EntryTeam startup contract passed: original actor/UI, movement acceleration and position/velocity round trip.
- Runtime checks passed; FoundationChecks passed 121 assertions; EngineChecks passed 69 assertions.
- Browser input, source package loader, and magazine UI checks passed 31/31. Actual HTML module syntax passed.
- Browser runtime build and Release WASM publish passed with .NET SDK 8.0.426.
- No Playwright/browser or iPhone Safari run was available. The user explicitly skipped the manual device-test step; leave iPhone acceptance open.

## Known runtime limits

- Mission 01 still reports unsupported/unresolved recovered Unity components and built-ins, including Sprite, Mesh, Shader, ReflectionProbe, some Camera fields, audio components, and gameplay scripts. This is not full Unity parity.
- Browser Mission startup uses a silent source audio adapter, disables unsupported behavior-tree/vocalization hooks, and ignores non-player rigidbody force commands. Only the controlled player's physics state is integrated with browser PhysX. NPC interaction and Mission audio are not verified.
- Existing gunshot stale-event/gain hardening remains pending iPhone Safari acceptance. Source Mission gun audio is silent until the source audio bridge is implemented.

## Hosting

- Existing owner-private Site: `https://crisis-response-runtime.craftysnipe13.chatgpt.site`. Preserve its owner-only access.
- Publish only a verified static archive built from the exact final `develop/wasm` source state. Check the Site's current version/source before retrying a publish; do not create duplicate versions for an already successful deployment.
- Before closing the task, confirm `develop/wasm` is clean and current, the package integrity verifier passes, the Site version is deployed from the same release, and report the browser/device acceptance limits plainly.
