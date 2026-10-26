# Mission cache controls evidence — 2026-10-26

## Implementation

- Source commit: `5693194aef4d614a99764c8683c0c344a15ebe1b`.
- Added `clearIncrementalSourceCache(manifestUrl)`. It validates the current delivery manifest and deletes only `crisis-source-{sourcePackageSha256}`; unrelated CacheStorage entries are untouched.
- Added the accessible “Clear saved Mission data” control to the browser page and report success, no matching cache, unsupported storage, or the surfaced error.
- Regression coverage proves the current versioned cache name is the only entry deleted.
- The previous batch added per-asset cached/read/write outcome to Mission progress. That status is preserved.

## Verification

- Focused loader/UI tests: **8 passed, 0 failed**.
- Full Node contracts/tests: **127 passed, 0 failed**.
- `git diff --check`: passed.
- Test environment: Node `v24.22.1`; locked npm install and physics vendor step completed.
- .NET/WASM build: not run; SDK unavailable (`dotnet: command not found`).
- Hosted CI status for source SHA `5693194aef4d614a99764c8683c0c344a15ebe1b` is not exposed by the available connector. Exact-SHA CI remains unverified.

## Remaining storage work

CacheStorage remains browser-managed and can be evicted. This control clears only the active manifest version; it does not yet provide quota estimates, proactive space checks, or an IndexedDB implementation. Repeat-launch behavior still needs physical iPhone Safari measurement. No Site build was published.
