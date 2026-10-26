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

## Exact pushed commit and package evidence

- GitHub connector commit: `12d61245431b3e3a026fbd82a8199f6e983a8590`, tree
  `7dd6c19db9afcf0d02077a11c5d106744b641b74`, parent `59cb7b8...`.
- Actions run **#22**, ID `40263692936`, completed **success** for exact SHA
  `12d61245431b3e3a026fbd82a8199f6e983a8590`.
- The repo-local initial commit `6d4f45c...` had the same Git tree; package was
  rebuilt after fetching the connector commit so its manifest records exact
  remote SHA `12d612...`.
- `python3 scripts/package_wasm_site.py build --repo . --published
  /tmp/cpr-cache-fix-publish/wwwroot --output
  /tmp/cpr-cache-fix-package-remote --toolchain
  /tmp/cpr-inherit-toolchain.json --incremental-profile-package
  /tmp/cpr-v70-profile/global-ui-mission-operator` completed. Verification:
  `status: verified`, 2,684 files, 199 Blazor resources, 195 WASM files,
  1,919 assets (97,767,549 compressed; 205,514,345 source bytes),
  `sourceCommit=12d612...`.
- HTTP smoke against that package: `/`, `/game/`, `/runtime-web/` returned
  200 `text/html`; `/game/vendor/physx-js-webidl.wasm` returned 200
  `application/wasm`, 5,347,727 bytes, SHA-256
  `7d4696e6273166308ce1f0db5dd5aee96c843b6e3722d96db7e4422aa1fbee1f`.
- GitHub branch subsequently advanced to `41c4e5351242947cc988714450f508a593638013`
  with a `NEXT_AGENT.md`-only commit. The 12d code commit is its ancestor; verify
  this current tip before the next source change.

## Spark provenance check

The mounted recovered profile has `Assets/Texture2D/Spark.png` metadata with
GUID `43907091bffd53b478c985dca2762559` and source metadata declaring 1,539
bytes and SHA-256 `6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344`.
The available exported PNG at
`/tmp/cpr-recovery-b184be9/export/ExportedProject/Assets/Texture2D/Spark.png`
is 1,014 bytes and hashes to `b33ee840f7cc744655e99b138a413f72036a92bd89e237d8ed8cdf412b4a2929`.
It is not byte-identical to the declared source and is not substituted. Search
covered the mounted `/tmp` and workspace for `Spark.png`; no 1,539-byte
matching source was found. Exact raw texture recovery and any claims of
visual identity remain pending the source owner's artifact or archive.
