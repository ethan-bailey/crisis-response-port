# Crisis Response Port — current handoff

## Checkpoint

- Development branch: `develop/wasm`.
- Latest gameplay source tested and published: `4fc8a0b789b2a71a4d79e249fad403943ce6865e` (`Gate firing on converged aim after fast retarget`).
- Repository HEAD is the documentation update in this handoff, based on the tested gameplay source.
- Pinned SDK: .NET 8.0.426 at `/workspace/.dotnet-8.0.426`; process shim: `/workspace/libprocshim.so`. Read `docs/DOTNET_BUILD_RUNBOOK.md` before .NET commands.
- Repository is public and Actions is enabled. Avoid Codespaces through 2026-11-01.
- Owner-only Crisis Response Runtime Site version **37** is live at `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.
- Site version 37 uses game source `4fc8a0b789b2a71a4d79e249fad403943ce6865e`, Sites source commit `b6432a7a528d84dad5ed98071c3043bae54ccbc7`, deployment ID `appgdep_6add0cd853c081918df29c0e0eba782c`, and archive SHA-256 `4c094918a7371ecc27f55405aec8bde993ba3a74dbb14dd2ba3e081dd4a80f1c`. Owner-only access is unchanged.

## Gate 8 status

The current source build, package, and headless source contracts pass. The exact package is live for the owner's iPhone Safari weapon smoke. The result of that device smoke is pending. Gate 8 remains open for rendered wound/particle verification and native PhysX parity as well.

## Verification on gameplay source `4fc8a0b789b2a71a4d79e249fad403943ce6865e`

- `node --test scripts/aim_fire_gate_test.mjs scripts/browser_input_test.mjs scripts/aim_direction_test.mjs`: **26/26 passed**.
- `python3 -m unittest discover -s scripts -p 'package_wasm_site_test.py'`: **13/13 passed**.
- `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release --output /tmp/cpr-publish-4fc`: **passed** with SDK 8.0.426. The optional `wasm-tools` optimization workload was not installed.
- `scripts/package_wasm_site.py build` and `verify`: **passed**, 767 files, 199 Blazor resources, 195 WASM files.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore`: **passed**, including original weapon/reload contracts and source presentation, input, ballistics, scene, and prefab checks.
- Site package tests: `node --test tests/*.test.mjs` — **33/33 passed**. Deployment succeeded as version 37; saved version source commit was verified as `b6432a7a528d84dad5ed98071c3043bae54ccbc7`.
- No automated browser smoke was run: this runner has no Chromium or WebKit executables. Physical iPhone Safari testing is delegated to the owner on Site version 37.

## Next action

Collect the owner's iPhone Safari results for the rapid sky-to-floor retarget and slow aim corrections. Check whether shots pause during fast retargets until the weapon catches up, and whether slow corrections maintain sustained fire. Use any reported mismatch for one focused fix, then rebuild, verify, publish a new owner-only version, and return a fresh phone test link. After the aiming behavior passes, continue the remaining wound/particle rendering and native PhysX acceptance work. Do not change Site access settings.
