# Chromium IndexedDB acceptance — 2026-10-26

## Exact source and hosted result

- Branch: `develop/wasm`.
- Exact tested source: `627314c2566df076d901e4170f18ba5c8fc6ab50`.
- GitHub Actions run **#28**, ID `40271922220`, completed successfully on that exact SHA.
- The `Chromium IndexedDB cache acceptance` job and the `Contracts and Release WASM publish` job both passed.
- Hosted browser: Playwright **1.64.1**, Chromium **156.0.8078.4**, Ubuntu **24.04**.
- The regular source job passed Node **138/138**, Python **41/41**, runtime contracts, EngineChecks (**75**), FoundationChecks (**121**), source build, Release WASM publish, and required-output checks.

## What the browser test proves

`scripts/indexeddb_browser_smoke.mjs` serves a tiny, hash-locked fixture through the production `scene-data/scene-package-loader.mjs`. It runs in real Chromium with the browser's actual IndexedDB implementation, not the Node test double. The run verified:

1. First launch fetches and integrity-checks the compressed fixture, then stores it in IndexedDB.
2. A full page reload reuses the verified stored asset without another asset request.
3. A corrupted stored record is rejected, fetched from the network, and replaced only with verified bytes.
4. A changed source-package hash gets an isolated cache record.
5. Clearing one source version leaves the other version intact and makes the cleared version fetch again.

The step output reported `status: passed`, `actualIndexedDb: true`, and four asset requests across first load, recovery, changed manifest, and after clearing. The post-reload cache hit did not add an asset request.

## Scope and next acceptance

This closes the production loader's basic real-browser IndexedDB persistence, recovery, and versioned-clear gate. It does not load the 99 MiB Mission package, measure the reported ~1,900-asset slowdown or phone memory, reproduce quota/eviction pressure, verify a fresh browser profile, or stand in for iPhone Safari acceptance. The fixture's bridge models the .NET call contract; this is not a full gameplay/browser scene run.

The workspace runner's one local `playwright install chromium` attempt returned a zero-byte/corrupt archive (`End of central directory record signature not found`). This limitation was resolved for the actual check by installing Chromium on the hosted Actions runner; no repeated local download was attempted. No Site package was published or deployed.

## Follow-on interruption case

A follow-on browser case now aborts an intercepted in-flight asset request, inspects IndexedDB for the source-version key, then retries and requires a fresh integrity-checked store. It also simulates an IndexedDB `QuotaExceededError` on `put` and requires delivery to complete without reporting the asset as persisted. The quota error is injected at the browser API boundary; it is not a test of Safari's physical quota policy or eviction behavior.

These follow-on assertions are **not yet accepted** in this evidence revision. They are in code commit `60399ddda7a8aa61557bf9392e75b3817b8cab34` on `develop/wasm`; the runner has no Playwright Chromium executable. Static syntax, full Node/Python contracts and the dependency inventory pass locally, but the revised browser test awaits the hosted Actions result for that exact pushed SHA. The available GitHub connector does not list push-triggered Actions runs, so the result is unknown and must be checked from the [branch Actions page](https://github.com/ethan-bailey/crisis-response-port/actions?query=branch%3Adevelop%2Fwasm). The prior local installer attempt returned a zero-byte/corrupt archive, so do not repeat it.
