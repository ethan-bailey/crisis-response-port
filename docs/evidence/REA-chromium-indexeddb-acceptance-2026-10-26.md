# Chromium IndexedDB acceptance — 2026-10-26

## Exact source and hosted result

- Branch: `develop/wasm`.
- Exact tested code SHA: `60399ddda7a8aa61557bf9392e75b3817b8cab34`.
- GitHub Actions run **#30**, ID `40272757036`, completed successfully on that exact SHA: [run #30](https://github.com/ethan-bailey/crisis-response-port/actions/runs/40272757036).
- `Chromium IndexedDB cache acceptance` job `121408948007` and `Contracts and Release WASM publish` job `121408947795` both passed.
- Hosted browser: Playwright **1.64.1**, Chromium **156.0.8078.4**, Ubuntu **24.04**.
- Source job passed Node **138/138**, Python **41/41**, Runtime contracts, EngineChecks (**75**), FoundationChecks (**121**), Release WASM build/publish, and required-output checks.

## What the browser test proves

`scripts/indexeddb_browser_smoke.mjs` serves a small hash-locked fixture through the production `scene-data/scene-package-loader.mjs`. It exercises actual Chromium IndexedDB, not the Node test double. All seven assertions passed:

1. First launch fetches and integrity-checks compressed fixture bytes, then stores them in IndexedDB.
2. A full page reload reuses the verified record without another asset request.
3. An aborted in-flight request leaves no partial record; retry fetches and stores verified bytes.
4. An injected IndexedDB `QuotaExceededError` still permits delivery and does not claim the bytes were persisted.
5. A corrupted stored record is rejected, refetched and replaced only with verified bytes.
6. A changed source-package hash uses an isolated cache record.
7. Clearing one source version leaves the other intact and makes the cleared version fetch again.

The hosted step reported `status: passed` and `actualIndexedDb: true`. The quota case injects the error at the IndexedDB API boundary; it confirms graceful loader behavior but does not model Safari's physical quota limits, eviction policy, or phone storage pressure.

## Scope, limits, and next acceptance

This closes the production loader's browser-fixture persistence, recovery, version isolation and scoped-clear gate. It does not load the 99 MiB Mission package, measure the reported late-load slowdown near 1,900 assets, assess phone memory/progress, reproduce real quota/eviction, verify a fresh phone profile, or establish gameplay or iPhone Safari acceptance. The fixture bridge models the .NET call contract; it is not the full scene.

Local `playwright install chromium` was attempted once and returned a corrupt zero-byte archive (`End of central directory record signature not found`). The exact test then passed on the hosted Actions runner with its installed browser; no repeated local download was attempted.

No Site package was published or deployed. The Site connector reports live Site version **71**, but does not expose its archive. A direct unauthenticated fetch of the live fullsource manifest returned **HTTP 401**. Authorized source/archive access remains necessary to validate the complete Site package; do not bypass the access control. No deployment occurred.
