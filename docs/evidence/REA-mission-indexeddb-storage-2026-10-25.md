# Mission chunks in IndexedDB — 2026-10-25

## Implementation

- Immutable compressed Mission assets use the `crisis-response-mission-assets`
  IndexedDB database, keyed by manifest source hash plus asset URL. The browser
  store writes only after the network asset passes compressed size and SHA-256
  verification; cache hits are rechecked against the manifest before use.
- CacheStorage remains a fallback only when IndexedDB is absent or cannot open.
  The storage preflight identifies the selected backend before transfer.
- A corrupt/unreadable IndexedDB record is deleted and that one immutable
  asset is fetched again, verified, and stored. Other verified chunks remain.
- “Clear saved Mission data” clears only entries whose key begins with the
  active source-package hash and also removes the legacy CacheStorage namespace
  for that manifest. Other manifest versions remain intact.
- IndexedDB open/write/clear errors do not replace the verified network path;
  cache write errors remain visible in per-asset progress, including quota
  rejection.

## Verification

Implementation was based on live `develop/wasm` commit
`8e8e68a3edd467d7abdac23d3dede24292a09c0a`.

- Focused Mission loader: **7 passed**.
- Full Node contracts/tests: **127 passed, 0 failed** on source tree after
  implementation, with generated PhysX vendor output restored using the
  documented `npm ci --prefix game --ignore-scripts` and
  `npm run --prefix game vendor` sequence.
- Python suite: **41 passed**.
- Regression uses a transactional IndexedDB test double in the real loader
  path to check first write, second-run reuse without asset fetch, corrupt
  entry recovery, and hash-scoped clearing. CacheStorage fallback and quota
  error reporting are separately covered.
- No actual Chromium/WebKit IndexedDB transaction or iPhone Safari storage run
  is claimed. Browser quota/eviction, interruption during a real IDB commit,
  persistent-storage behavior, and measured second-launch transfer remain
  acceptance work.

## Limits

This batch implements and unit-checks IndexedDB. It does not prove the browser
retains data, quantify browser-managed quota, measure the mission-load stall,
or establish iPhone Safari acceptance. Keep those gate items pending until
recorded on their real browser/device paths.
