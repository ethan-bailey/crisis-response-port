# Crisis Response Port — current handoff

## Durable checkpoint

- Development branch: `develop/wasm`.
- Tested source and live branch checkpoint: `dc6843cb778b843900911e258075386af6503585` (`feat: add verified incremental source package store`).
- Checkout was fetched from `origin/develop/wasm` and reset to that exact SHA; worktree was clean before this handoff update.
- No open pull requests were returned by the GitHub connector at session start.
- Site v39 remains the last owner-accepted iPhone Safari build. No deployment was performed.

## Completed major system: incremental original source package store

`runtime/Engine/OriginalSourcePackage.cs` now supports both existing monolithic
packages and incremental asset delivery. Incremental package metadata carries
an asset manifest with each asset's UTF-8 byte length and SHA-256. Assets are
validated before parsing/registration, identical retries are safe, and missing
or unmanifested assets fail closed. SceneLoader and SerializedAssetResolver
consume this shared package view. Resources can use the same `RuntimeWorld` as
the scene. The WASM RuntimeBridge exposes package initialization, verified
asset addition with progress, and scene loading on that world.

Evidence is in
`docs/evidence/INCREMENTAL_SOURCE_PACKAGE_STORE_2026-10-25.md`.

## Exact verification on the tested source

- Pinned SDK .NET `8.0.426`; process shim `/tmp/libprocshim.so`.
- Full runtime check project build: passed, 0 errors and 48 existing warnings.
- Incremental package store contract: passed hash/size verification, metadata
  preservation, idempotent retry, synchronous same-world scene load, rejected
  modified payload, fail-closed missing asset, and legacy package compatibility.
- Default runtime checks: passed runtime, prefab, arm, presentation, weapon,
  and ballistics contracts.
- WASM Browser project build: passed, 0 errors and 48 existing warnings.
- EngineChecks: passed 69 assertions.
- Scene package loader Node checks: passed 3/3.
- Active `--mission-ui-contract`: passed against
  `/tmp/cpr-scenes-full-source.json` (input SHA-256
  `6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`). The
  original Mission started; FullSource actor bridge snapshot, EntryTeam UI
  initialization, and original left press/release passed. Headless AI and audio
  are not covered by this contract.
- `git diff --check`: passed.

## Remaining integration gates

The ordinary page still uses browser-side movement and the transport weapon
host. The opt-in scene smoke still downloads the split monolithic package. The
new store and WASM bridge are not yet called by page startup, and this batch did
not produce per-asset source bundles or a bounded dependency-closure manifest.
The recovered Global Context + Mission UI + Operator closure must be packaged
for incremental delivery, loaded into one RuntimeWorld, and wired to page input,
physics, rendering, and weapon callbacks through the same FullSource actor.

Playwright browser smoke was not run because Playwright/browser binaries are
unavailable here. iPhone Safari acceptance remains open. No deployment was
performed.

## One next action

Generate a per-asset manifest and bounded dependency profile for the recovered
Global Context + Mission UI + Operator closure, connect the browser loader to
the incremental WASM bridge, then run the active Mission UI actor contract and
measure delivered bytes and peak browser memory before the first input. Keep
the exact tested SHA and iPhone Safari acceptance state attached to the result;
do not deploy before the owner-authorized release gates pass.
