# Mission storage estimate preflight — 2026-10-25

## Scope

Before the large Mission transfer, query the browser's available storage estimate
and request persistent storage when supported. Show both results to the player.
These browser APIs are advisory: storage refusal, unavailable estimates, or a
missing API must not prevent verified network delivery.

## Implementation and pass criteria

- `getStorageStatus` calls `navigator.storage.persist()` and `estimate()` only
  when the corresponding APIs exist; exceptions are converted to unknown
  status rather than breaking Mission delivery.
- The loader emits a storage preflight progress event before downloads. It
  normalizes finite usage/quota values and computes nonnegative estimated
  available bytes.
- The Mission progress panel reports estimated available MiB, whether
  persistent storage was granted/declined/unavailable, and the compressed
  Mission size. It says delivery continues when local storage is declined.
- The Node contract supplies a browser storage mock, verifies the computed
  estimate and persistent result, and confirms the preflight stage is ordered
  before fetching/adding assets.

## Verification

Source is based on remote `develop/wasm` SHA
`f1f66a6a29a1e292b42e67613e857357af16f6ce`.

- Focused `node --test scripts/scene_package_loader_test.mjs`: **7 passed**.
- Full `node --test scripts/*_test.mjs scripts/*_contracts.mjs`: **127 passed**
  after the known generated PhysX vendor recovery (`npm ci --prefix game
  --ignore-scripts`; `npm run --prefix game vendor`). The first attempt failed
  only because the generated vendor file was absent in the fresh checkout.
- `git diff --check`: passed.
- Browser/device storage was mocked in Node; no live browser quota estimate,
  storage persistence, eviction, or iPhone Safari repeat-launch claim is made.
- This slice does not implement the outstanding IndexedDB transaction store.

Record the final tested source SHA and remote SHA in `NEXT_AGENT.md` after
updating the authorized branch.
