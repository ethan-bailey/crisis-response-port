# Crisis Response Port — current handoff

## Checkpoint

- Development branch: `develop/wasm`.
- Latest gameplay source verified here: `4fc8a0b789b2a71a4d79e249fad403943ce6865e` (`Gate firing on converged aim after fast retarget`).
- The current handoff update is documentation-only on top of that gameplay source.
- Pinned SDK: .NET 8.0.426 at `/workspace/.dotnet-8.0.426`; process shim: `/workspace/libprocshim.so`. Read `docs/DOTNET_BUILD_RUNBOOK.md` before .NET commands.
- Repository is public and Actions is enabled. Avoid Codespaces through 2026-11-01.
- Owner-only Crisis Response Runtime Site remains on version 36, built from game source `1962efb2f99da1cc59f9ad365b0fc4bd1868f735`. Do not change its access settings or deploy without explicit authorization.

## Gate 8 status

Gate 8 remains open. The current source build, package, and headless source contracts pass. Exact-source interactive browser acceptance, the fast-retarget/slow-correction live weapon smoke, physical iPhone Safari, rendered wound decals and particles, and native PhysX parity remain unverified.

## Verification on gameplay source `4fc8a0b789b2a71a4d79e249fad403943ce6865e`

- `node --test scripts/aim_fire_gate_test.mjs scripts/browser_input_test.mjs scripts/aim_direction_test.mjs`: **26/26 passed**.
- `python3 -m unittest discover -s scripts -p 'package_wasm_site_test.py'`: **13/13 passed**.
- `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release --output /tmp/cpr-publish-4fc`: **passed** with SDK 8.0.426. The optional `wasm-tools` optimization workload was not installed.
- `scripts/package_wasm_site.py build` and `verify`: **passed**, exact source SHA `4fc8a0b789b2a71a4d79e249fad403943ce6865e`, 767 files, 199 Blazor resources, and 195 WASM files.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore`: **passed**, including original weapon/reload contracts (`initialRounds: 31`, empty bolt locks, original reload tasks, 30 reloaded rounds, no infinite ammo) and the source presentation, input, ballistics, scene, and prefab checks.
- Browser runner check: Playwright package exists, but its Chromium and WebKit executables are absent at the configured paths. No browser smoke was run.

## Current blocker and next action

Run `scripts/weapon_smoke.mjs` against the exact package in a WebGL-capable Chromium and WebKit browser, verifying that rapid sky-to-floor retargeting suppresses shots until source aim convergence and slow corrections preserve sustained fire. This runner has no browser executables, and the prior Cloud Browser was unable to create WebGL and authenticated to an account outside the owner-only Site access list. Do not alter Site access to work around that. The current Site is older than source `4fc8a0b`; do not claim its behavior verifies this fix. Once a WebGL-capable owner-authorized browser is available, run the exact-source smoke. Physical iPhone Safari and remaining visual/native parity checks are still required before Gate 8 can close.
