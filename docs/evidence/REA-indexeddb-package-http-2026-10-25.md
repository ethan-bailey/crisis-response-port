# Exact IndexedDB source package and HTTP smoke — 2026-10-25

## Exact source and build

- Public branch commit: `b1f8477c3a5d12bc8c83ec7bc336726165804c03`.
- Source tree: `1a0aa6e8d6c0c25eaf97101ab9fe4a934628221b`.
- SDK: .NET **8.0.426**, loaded from `/tmp/codex-dotnet-8.0.426` with
  `/tmp/libprocshim.so`.
- `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release -o
  /tmp/cpr-b1-publish`: passed. The optional `wasm-tools` optimization workload
  is absent; publish completed without that optional optimization.
- Production package was built with `scripts/package_wasm_site.py build`, using
  the hash-audited Mission profile recovered from the historical version 70
  archive. The package verifier returned `status: verified`, source commit
  `b1f8477...`, 2,684 files, 199 Blazor resources, 195 WASM files, and the
  1,919-asset Mission profile (97,767,549 compressed bytes; 205,514,345 source
  bytes).
- `build-manifest.json` SHA-256:
  `306b9fd177bd70f7a1ef4010f99f6a92cede4b66b12bf29607a4aee782120777`.
- `game/vendor/physx-js-webidl.wasm` SHA-256:
  `7d4696e6273166308ce1f0db5dd5aee96c843b6e3722d96db7e4422aa1fbee1f`.

## Verification

- Exact source Contracts/WASM build hosted workflow #19 completed successfully:
  run `40261578540`, SHA `b1f8477...`.
- Full Node suite: **127 passed, 0 failed** on source tree
  `1a0aa6e...`; Python suite on the identical tree: **41 passed**.
- The first package attempt failed closed because this fresh worktree omitted
  generated `game/vendor/physx-js-webidl.mjs`. The documented one-time recovery
  (`npm ci --prefix game --ignore-scripts`; `npm run --prefix game vendor`)
  restored it; the full Node suite passed, and packaging succeeded into a new
  output directory.
- Local HTTP smoke used the exact package, returned 200 for `/`, `/game/`,
  `/runtime-web/`; `.wasm` served as `application/wasm`. The selected 5,347,727
  byte WASM response matched the build manifest size and SHA-256.
- The first HTTP attempt requested `/dist/build-manifest.json`; the packaged
  path is `/build-manifest.json`. Correcting the test URL produced the passing
  result above. The retry consumed the full WASM response to verify its hash.

## Acceptance still pending

This local package/HTTP check does not run the interactive game in Chromium or
WebKit and does not test real IndexedDB quotas/eviction, interrupted IDB writes,
second launch, mission progress after ~1,900 assets, visual/gameplay parity, or
iPhone Safari. The exact source package remains unpublished; no Site was
deployed or preview changed.
