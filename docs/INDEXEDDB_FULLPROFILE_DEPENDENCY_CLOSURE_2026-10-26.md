# Full-profile IndexedDB load and reload dependency closure — 2026-10-26

## Gate and observed gap

The production browser loader's IndexedDB behavior has passing Chromium checks for verified first write, persistent reload, interrupted fetch, quota fallback, corrupt-record repair, source-hash isolation and selective clear. The existing browser harness uses a single 95-byte source asset. It does not exercise the actual Mission payload associated with the reported late-load crawl or approximately 1,900-asset stall.

The verified `global-ui-mission-operator` profile is 1,919 assets, 22,365 records, 97,767,549 compressed asset bytes, and 205,514,345 uncompressed source bytes. The loader already sorts descending by compressed asset size, fetches six assets concurrently, verifies compressed hashes, writes one immutable compressed record per asset to IndexedDB, batches bridge transfer at 16 assets / 1 MiB maximum, and reports per-asset/network/bridge progress. This test closes the missing browser-scale measurement subgate; it is not a product design change.

## Complete dependency inventory before implementation

| Dependency | Source of truth | Acceptance check |
|---|---|---|
| Recovery identity | Canonical recovery archive SHA `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`, source-index SHA recorded in build provenance | Manifest/profile input is the same full-source package selected by exact-SHA workflow, not a synthetic one-asset fixture |
| Profile roots and transitive set | `manifest.json` six-root Mission profile; all 1,919 manifest entries and metadata index | Count, total bytes, metadata hash and every asset's declared byte length/hash are served from the verified profile |
| Loader ordering and concurrency | `scene-data/scene-package-loader.mjs:deliverIncrementalSourceProfile` | The actual production module is used; observed asset server requests equal 1,919 on cold load |
| Integrity checks | Manifest `compressedBytes` and `compressedSha256`; loader's SHA-256 checks | Every file is rechecked against its manifest before serving; result and loader counts match exact profile totals |
| IndexedDB namespace and transaction | Database `crisis-response-mission-assets`, object store `assets`, key prefix `sourcePackageSha256:` + absolute asset URL | Real Chromium IndexedDB is selected; all 1,919 first-load records report persisted and all 1,919 reload hits report verified cache use |
| WASM bridge shape | `InitializeIncrementalSourcePackage`, `AddIncrementalSourceGzipAssetBatch` | Browser-side bridge model validates batch base64 length, each compressed length, source path, ordered item count, cumulative source bytes and compressed bytes. It does not emulate .NET runtime execution; the existing runtime suite covers that boundary separately. |
| Failure and lifecycle cases | Existing `indexeddb_browser_smoke.mjs` one-asset tests | Keep quota, abort, corruption, source-version isolation and selective-clear cases; full-profile test adds scale and reload coverage rather than replacing them |
| Browser environment | Pinned Playwright 1.64.1 / Chromium on the workflow runner | `navigator.indexedDB` is real; local origin is a secure context; test runs on the exact workflow source SHA |
| Performance evidence | Loader profile elapsed time, per-asset fetch/hash samples, bridge batch samples, server request counters | Record cold and warm-reload duration, persisted/hit counts, and slowest fetch/bridge samples. No arbitrary wall-time threshold before baseline data exists; a stall or incomplete profile is a test failure |
| Scope and limits | Actual production JS loader + recovered profile + modeled bridge | Proves Chromium-scale delivery and local persistence only. It does not prove iPhone Safari quota, memory, offline behavior, native .NET bridge execution at full profile, or device speed |

## Execution order

1. Keep the existing exact source/package identity and one-asset browser failure-path checks.
2. Serve the exact six-root profile into a real Chromium page and run the production loader to completion.
3. Reload the browser page and run again from IndexedDB; assert no additional source-asset network requests.
4. Save the measured cold/warm durations, full asset/request counts and representative slow samples with the exact source SHA.
5. If Chromium storage quota, memory, loader progress, or bridge shape fails, identify the exact failing dependency before changing delivery behavior.

## Current checkpoint

Run #78 passed the package/build and one-asset Chromium acceptance, but the production-profile IndexedDB scale case has not yet run. A local execution attempt found no installed Chromium executable; a subsequent Playwright download produced a corrupt 0 MiB archive and could not install one. The exact profile is present at `/tmp/cpr-current-mission-profile-full`. The pinned Playwright browser installation step in GitHub Actions is the planned execution environment and succeeded in run #78. Do not claim this scale subgate closed until its full workflow result is green.
