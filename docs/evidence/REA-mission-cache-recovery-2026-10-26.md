# Mission cache recovery evidence — 2026-10-26

## Scope and source

- Change: recover a single corrupt/unreadable device-cached Mission asset from the network, then replace the cache entry only after the network response passes the existing manifest size and SHA-256 checks.
- Source: `scene-data/scene-package-loader.mjs`, helper `fetchCompressedAsset`.
- Regression: `scripts/scene_package_loader_test.mjs` corrupts one cached chunk and verifies the network fallback, cache replacement, and progress state (`wasCached:false`).
- Commit: `522b8e0063e3dbf976d6568b224bb7a72b09f7e4`, parent `088e6c2c439868b0d376207719432e13df73a219`.

## Verification

- Focused loader test: **6 passed, 0 failed**.
- Full Node suite: **126 passed, 0 failed** after running `npm ci --prefix game --ignore-scripts` and `npm run --prefix game vendor`. The first full-suite attempt failed because the vendored `game/vendor/physx-js-webidl.mjs` module was absent; the pinned dependency/vendor setup resolved that environment issue.
- `git diff --check`: passed.
- Local Node version: `v24.22.1`; this is not the workflow's pinned Node 22.
- Local .NET/WASM build: **not run**; `dotnet --version` returned `dotnet: command not found`.
- Hosted run for the pushed SHA: the connector's combined commit status returned no statuses and its commit-workflow query only exposes pull-request-triggered runs. Do not treat the hosted .NET/WASM gate as verified until its exact-SHA run is inspected.

## Limits and next dependency

This change recovers one bad immutable cached chunk without invalidating other verified chunks. It does not add IndexedDB storage, quota estimates/error UI, user clear-cache controls, or prove Safari eviction behavior. The next action is to verify the exact-SHA hosted CI result for this commit, then continue the version 70 source crosswalk and storage/mission-load gates. No Site was published.
