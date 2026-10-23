# PR #69 post-merge publish verification — 2026-10-23

## Tested source

The local checkout was commit `c78e8ad613f1560aa6624cf259722be5610b9cda`, with Git tree `bba140f1e366411f433d1a4a7d34bce029e960b1`. That tree is identical to `develop/wasm` commit `b0c2fb852d275901dbebb667f3353f1943d5683a`, the source parent at PR #69 integration. PR #69's later integration commit changes documentation only relative to that runtime tree.

## Toolchain and commands

The workspace initially had no `dotnet` executable on `PATH`. SDK 8.0.426 was available as `/tmp/dotnet-sdk-8.0.426-linux-x64.tar.gz` (SHA-256 `bb691c98abc8797eedbd48d3a766bc734214aed746e52e2691d8bbb268c60764`). Extracted to `/tmp/codex-dotnet-8.0.426` with `tar --no-same-owner`. The existing `/tmp/libprocshim.so` was used through `LD_PRELOAD` because this runner cannot read process start metadata directly.

- SDK selection: `dotnet --list-sdks` showed 8.0.426; `dotnet --version` returned 8.0.426 with the shim.
- Fresh Release runtime build: `dotnet build runtime/Crisis.Runtime.csproj --configuration Release --no-incremental` — **0 errors, 45 warnings**.
- Clean browser build and publish: `dotnet clean runtime/Browser/Crisis.Runtime.Browser.csproj --configuration Release`, then `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj --configuration Release --output /tmp/crisis-response-pr69-publish` — **passed, 0 errors**. SDK noted the optional `wasm-tools` workload was not installed; publish completed without it.
- Published `wwwroot/browser-input.mjs` exists. SHA-256 matches tracked `game/browser-input.mjs`: `e720dee5dac385e4f4e9a0664924637860df2d82047e56a253a5d71ad0d99725`.

## Package and provenance

Ran the lockfile-pinned PhysX dependency setup (`npm ci --prefix game --ignore-scripts`, then `npm run --prefix game vendor`) and built the site package from the fresh publish output with `scripts/package_wasm_site.py`.

- Generated original Mission 01 CRG: 45,356 vertices, 33,666 triangles.
- Source OBJ SHA-256: `891dc21c94f1be328b48b6c24f8624c68a7125283b0c386230304009abe65fda`.
- Generated CRG SHA-256: `ad1b48755b368ebabb7b3e44dbc367ae01c0e4968fc3f213aac0f8c2a7924d79`.
- Full package verification passed: **750 files**, **199 Blazor boot resources**, **195 WASM files**. The package manifest verified each listed file hash and binary metadata.
- `python3 scripts/package_wasm_site_test.py`: **12/12 passed**.
- Packaged `runtime-web/browser-input.mjs` has the same SHA-256 as the tracked source and published output.

The generated package is in temporary `/tmp/crisis-response-pr69-site-verified`; it was not committed or deployed.

## Browser and gameplay status

No browser run is claimed. `npx playwright install chromium` failed after the download endpoint returned an empty archive (0 MiB; invalid ZIP). No valid Chromium executable was available. The package build manifest records `browserTestedThisBuild: false`.

Scene loading, Marksman rendering/input, two-actor lifecycle, and live deployment remain unverified. GitHub Actions were not run or re-enabled.