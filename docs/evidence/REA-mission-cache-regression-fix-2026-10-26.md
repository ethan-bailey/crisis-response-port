# Mission cache regression fix — 2026-10-26

## Acceptance target

The Mission's largest-first bounded delivery must retain the previously verified
device cache behavior: IndexedDB primary storage, CacheStorage fallback,
manifest-bound keys, size/SHA validation of cache hits, recovery of corrupt
records, observable quota failures, storage estimate/persistence progress, and
the version-scoped clear-cache control.

## Live-source finding

The live `develop/wasm` branch was `59cb7b876ad23a4aa6de5008f01fa5401d053fd6`.
Commits `3c17b1115190c0530f9c4112c9d2fcc1fa1ad5a3` and
`59cb7b876ad23a4aa6de5008f01fa5401d053fd6` changed delivery to prioritize
large files, but also removed IndexedDB, cache integrity recovery, quota and
storage reporting, the clear-cache export, and the tests covering those paths.
`game/index.html` still imports and calls `clearIncrementalSourceCache`, so the
removed export also prevented the game module from loading. The associated test
deletions removed acceptance coverage without replacing it with a real-browser
test.

## Implementation

Restored the cache and progress implementation from the previously verified
`cc4042f7dc7fcd42a7ff3e36e7c29e3744b706e5` source tree, then retained the live
largest-first ordering. The bounded six-request fetch groups are now sorted by
compressed size descending with path as a deterministic tie-breaker; bridge
delivery follows that order and validates the complete byte totals. No original
gameplay source was changed.

## Verification

- `node --test scripts/scene_package_loader_test.mjs`: **7/7 passed**, including
  verified cache hits, corrupt-cache recovery, quota failure visibility,
  IndexedDB reuse, version-scoped clearing, and batching.
- `npm ci --prefix game --ignore-scripts && npm run --prefix game vendor`:
  regenerated the tracked-worktree-missing PhysX adapter from its lockfile.
- `node --test scripts/*_test.mjs scripts/*_contracts.mjs`: **128/128 passed**.
- `python3 -m unittest discover -s scripts -p '*test*.py' -v`: **43/43 passed**.
- `python3 scripts/inventory_source_dependencies.py --check`: passed; 810
  sources, 1,710 symbols, 1,384 reachable files, 127 ambiguous names requiring
  ownership review.
- Pinned SDK preflight with `LD_PRELOAD=/tmp/libprocshim.so`:
  `/tmp/codex-dotnet-8.0.426/dotnet --version` printed **8.0.426**.
- `dotnet restore` and `dotnet run` for EngineChecks and FoundationChecks, and
  `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release
  -o /tmp/cpr-cache-fix-publish`: completed. Release publish warns that the
  optional `wasm-tools` workload is not installed; it still emitted the Blazor
  output.
- Initial whole Node-suite attempt failed only because a fresh worktree lacked
  generated `game/vendor/physx-js-webidl.mjs`; the documented lockfile/vendor
  recovery above corrected that runner prerequisite, after which all Node tests
  passed.

## Scope and remaining acceptance

This proves loader contracts and .NET package compilation, not browser IndexedDB
transactions on Safari. Actual fresh/second browser launch, quota/eviction,
interrupted download, manifest invalidation, load timing and memory still need
Chromium/WebKit runs and owner iPhone Safari acceptance. This fix does not close
the broader source/package, authored actor lifecycle, combat, rendering or
end-to-end gates.
