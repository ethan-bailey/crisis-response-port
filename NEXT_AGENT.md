# Crisis Response Port — current handoff

## Checkpoint

- Development branch: `develop/wasm`.
- Latest gameplay source: `e009c00d240b63e67d29f9499d4740bbc6669f55` (`Keep held fire active during slow aim updates`; exact source tree `6b8011f1b30691981a9a484fe8865b545ff6d68d`).
- The owner physically played Site v37 on iPhone Safari; this is the device smoke evidence. During that test, they reported the original touch-and-hold-on-screen behavior: aiming changes stopped sustained fire. That report was initially misread as holding the Fire button. The current patch fixes the separate held-Fire-button path; it does not fix or claim to fix touch-and-hold on the play area. Per owner direction, defer the screen-hold behavior for later investigation and proceed.
- Owner-only Crisis Response Runtime Site version **39** is live at `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.
- Site v39 records game source `e009c00d240b63e67d29f9499d4740bbc6669f55`, Site source commit `f81db068d09485cbb54cd7d0035065a0e5ab2222`, deployment ID `appgdep_6add110fbaf08191bfbba54cf6e5e112`, and archive SHA-256 `0631b0e712ee11f92eb6184372e02dce0f70300ab2aec519e694c055e26e1a64`. Site access remains owner-only (custom allowlist, no external visitors).
- Site release manifest SHA-256: `75e4b73805fa0a518228b6d82d2a41d329d5b505a46518691fe9cb55d517a52a`.
- Pinned SDK: .NET 8.0.426 at `/workspace/.dotnet-8.0.426`; process shim: `/workspace/libprocshim.so`. Read `docs/DOTNET_BUILD_RUNBOOK.md` before .NET commands.

## Gate 8 status

Gate 8 build, package, Site tests, and owner-only release are complete. The owner completed physical iPhone Safari gameplay smoke on Site v37, confirming the game ran on-device and surfacing the deferred screen-hold issue. Per owner direction, accept that device smoke for this gate and move on; v39 contains a separate held-Fire-button change and was not retested on iPhone. Cloud Chrome opened v39 but could not create a WebGL context (`GL_VENDOR = Disabled`); this runner limitation blocked desktop gameplay smoke. Do not claim Chromium/WebKit verification. No Gate 9 exists in the current completion plan. Gate 7 visual wound/particle fidelity and native PhysX parity remain separate incomplete work.

## Verification on source `e009c00d240b63e67d29f9499d4740bbc6669f55`

- `node --test scripts/aim_fire_gate_test.mjs scripts/browser_input_test.mjs scripts/aim_direction_test.mjs`: **29/29 passed**, covering held Fire-button input during gradual aim changes, tap waiting, and fast-retarget blocking. These tests do not cover the deferred touch-and-hold-on-screen report.
- `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release --output /tmp/cpr-publish-e009/wwwroot`: **passed** with pinned SDK 8.0.426. The optional `wasm-tools` optimization workload was not installed.
- `scripts/package_wasm_site.py build` and `verify`: **passed**, 767 files, 199 Blazor resources, and 195 WASM files; build manifest records game source `e009c00…`.
- Site package tests: `node --test tests/*.test.mjs` — **33/33 passed**.
- Site v39 deployment returned `succeeded`; saved-version source commit and archive checksum were verified. Owner iPhone Safari gameplay smoke is confirmed on v37; v39 was not retested on device. Cloud Chrome failed to create WebGL before gameplay startup, so no desktop gameplay smoke is claimed.

## Next action

Continue to the next planned gate. Keep the touch-and-hold-on-screen behavior on the deferred issue list; the held Fire-button fix does not resolve it. Do not change Site access settings. Preserve the browser-smoke limitation and the fact that the owner tested v37, not v39. Gate 7 wound/particle rendering and native PhysX parity remain separate work.
