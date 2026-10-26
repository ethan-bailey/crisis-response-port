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

## Current checkpoint — full Mission profile measured; workflow fully green

On 2026-10-26, run #79 / GitHub Actions ID `40356543988` executed on exact public source SHA `4428f5f5adc06edeb19b1dc17f111e66e407d04c`. Both jobs passed: full-profile job `121690423604` and Chromium IndexedDB/audio job `121690424261`. Site v74 later deployed the exact Site-bound SHA `ae134ef153149e6b6ab262502dd978c4737087ca`; the production loader/profile files are byte-identical across those two source trees, while the develop CI run remains evidence only for its own SHA.

Exact full-profile report:

```json
{"status":"passed","browser":"Chromium","evidence":"production incremental loader over the complete verified Mission profile; Blazor bridge modeled in page","profile":"global-ui-mission-operator","assetCount":1919,"compressedAssetBytes":97767549,"sourceAssetBytes":205514345,"firstLoadMs":10879,"cachedReloadMs":8652,"firstLoadNetworkAssets":1919,"cachedReloadNetworkAssets":0,"firstLoadPersistedAssets":1919,"cachedReloadHits":1919,"firstLoadSlowestFetches":[{"path":"Assets/AudioClip/B&B Main Menu 130BPM mixed final 3.ogg","durationMs":171.4,"compressedBytes":2702558},{"path":"Assets/AudioClip/B&B MissionMusic 110BPM sample7 Mixed.ogg","durationMs":166.9,"compressedBytes":2822414},{"path":"Assets/Texture2D/wood grain normal.png","durationMs":162.5,"compressedBytes":2830061}],"firstLoadSlowestBridgeBatches":[{"path":"Assets/Texture2D/wood grain dark normal.png","durationMs":256.2,"assets":1,"compressedBytes":3027318},{"path":"Assets/Texture2D/sf_conc_floor_01_d.png","durationMs":249.9,"assets":1,"compressedBytes":3108968},{"path":"Assets/Texture2D/wood grain normal.png","durationMs":230.3,"assets":1,"compressedBytes":2830061}],"backend":"IndexedDB","boundary":"real Chromium IndexedDB and production JS loader; Blazor bridge modeled; not iPhone Safari or device storage evidence"}
```

The independent browser job passed all seven IndexedDB persistence/recovery/version tests. Its production WebAudio smoke decoded seven authored clips and played fire, magazine extract, magazine insert and bolt return once each; audio context was running, gain was 0.16, peak RMS `0.1598044873`, with zero stale/suspended/unloaded/unmapped drops. These are headless measurements; they do not establish physical iPhone audibility or the .NET bridge’s full-profile throughput.

The Site v74 full-profile package was separately built from its Site source commit and independently verified before deployment. It contains the same full-profile counts/bytes but has source-package SHA-256 `ea3592ff5bdd39534bb905cd0abac74f456578b46eb4b12cdd0cb79fe8520cf7`, distinct from the public run #79 source index. Do not substitute one package identity for the other. Site v74 archive/package identities are in `docs/evidence/SITE_DEPLOYMENT_ALIGNMENT_BLOCKER_2026-10-26.md`.
