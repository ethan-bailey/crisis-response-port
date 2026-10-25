# Incremental original source package store — 2026-10-25

## Delivered

`runtime/Engine/OriginalSourcePackage.cs` adds a synchronous package view with
two input modes. Existing monolithic execution packages remain supported. The
incremental mode accepts metadata without an `assets` table and an
`assetManifest` that records each original asset's UTF-8 byte length and
SHA-256. Each delivered asset is verified before it is parsed or made visible
to the source resolver. Identical retries are idempotent; a bad hash, a missing
asset, or a path outside the manifest fails closed.

`SceneLoader` and `SerializedAssetResolver` now resolve assets through this
package view. `Resources` can be configured with the same package and
`RuntimeWorld`, so loaded scene actors and original resource prefabs can share
one world. The WASM bridge exposes initialization, per-asset delivery with
count/byte progress, and scene loading on that same world.

## Verification

- Pinned .NET SDK `8.0.426` and the repository process shim were used.
- `dotnet build runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore --no-incremental -v:q` — passed, 0 errors and 48 existing warnings.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-build -- --source-package-store-contract` — passed: hash and byte-count verification, metadata preservation, duplicate retry, same-world synchronous scene load, fail-closed missing assets, and legacy package compatibility.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-build` — passed the runtime, prefab, arm, presentation, weapon, and ballistics contracts.
- `dotnet build runtime/Browser/Crisis.Runtime.Browser.csproj --no-restore --no-incremental -v:q` — passed, 0 errors and 48 existing warnings.
- `dotnet run --project runtime/EngineChecks/Crisis.Runtime.EngineChecks.csproj --no-restore` — passed 69 isolated engine assertions.
- `node --test scripts/scene_package_loader_test.mjs` — passed 3/3.
- Active `--mission-ui-contract` — passed using `/tmp/cpr-scenes-full-source.json`, SHA-256 `6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`. The original Mission started; the FullSource actor bridge snapshot, EntryTeam UI initialization, and original left press/release passed. Headless AI and audio remain outside this contract.
- `git diff --check` — passed.

## Limits and next gate

This completes the runtime store and WASM delivery boundary. It does not yet
change `game/index.html` or `runtime/Browser/wwwroot/index.html`: normal page
startup still uses browser-side movement and the transport weapon host, and
the existing opt-in scene smoke still downloads the split monolithic package.
No source asset manifest or per-asset deployment bundle was generated in this
batch. The recovered Global Context plus Mission UI plus Operator closure must
be packaged and delivered as bounded batches, then loaded and exercised by the
same FullSource actor. Playwright browser smoke was not run because the browser
toolchain is unavailable here; iPhone Safari acceptance remains open. Nothing
was deployed.
