# Mission cache quota feedback — 2026-10-25

## Scope and acceptance

CacheStorage remains an optional performance optimization: a verified network
asset must still reach the active run when the browser refuses to store it.
Previously the cache write exception was discarded, so the UI could only say
that the asset was not saved. Preserve the exact storage exception per asset
and show a recovery hint when Safari or another browser raises
`QuotaExceededError`.

## Change

- `scene-data/scene-package-loader.mjs` now returns `cacheWriteError` alongside
  `cachedLocally`. The network bytes remain usable after a cache write failure.
- `game/index.html` shows the exact error name and a storage-space recovery hint
  for quota rejection; other cache errors are also visible.
- `scripts/scene_package_loader_test.mjs` simulates a browser quota rejection
  and asserts the asset remains delivered but is not reported as locally saved.
- The manifest-scoped “Clear saved Mission data” control is already present;
  the previous `NEXT_AGENT.md` statement that it remained open was stale.

## Verification

At source worktree based on live `develop/wasm` SHA
`04dcb19e3451a41089638f28bd9e0c3b9e62967c`:

- Focused `node --test scripts/scene_package_loader_test.mjs`: **7 passed**.
- Full `node --test scripts/*_test.mjs scripts/*_contracts.mjs`: **127 passed**.
- `git diff --check`: passed.
- .NET SDK from the documented existing path: **8.0.426**.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -c Release`:
  passed, including source/package, actor, weapon, projectile and mission
  contracts; the final output explicitly reports `fullGameParity:false`.
- EngineChecks: **75 assertions passed**.
- FoundationChecks: **121 assertions passed**; output explicitly notes that
  the complete original CharacterBody gate is separate.
- Release WASM publish to `/tmp/cpr-current-publish`: passed.
- First full Node attempt failed because generated
  `game/vendor/physx-js-webidl.mjs` was absent. The documented bounded recovery
  (`npm ci --prefix game --ignore-scripts` followed by
  `npm run --prefix game vendor`) restored it; the one full-suite rerun passed.

These checks are local to the current worktree. They do not prove hosted CI,
browser storage behavior on iPhone, repeat-launch persistence, or visual/game
parity. The change has not been committed at the time this evidence draft was
prepared; the final source and remote SHAs belong in `NEXT_AGENT.md` after the
push succeeds.

## Remaining storage work

- Measure and display storage estimate/persistent-storage result where the
  browser exposes it; ensure QuotaExceededError feedback is reviewed on device.
- Evaluate/migrate the large immutable Mission chunks to IndexedDB as directed
  by the project acceptance requirements; add integrity-checked transaction,
  interruption, corrupt-entry recovery, and version invalidation coverage.
- Reproduce first-load and repeat-load transfer, timing, and memory on physical
  iPhone Safari. CacheStorage retention is browser-managed and cannot be
  described as permanent.
