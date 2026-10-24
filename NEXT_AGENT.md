# Crisis Response Port — current handoff

## Checkpoint

- Development branch: `develop/wasm`.
- Latest gameplay source: `e009c00d240b63e67d29f9499d4740bbc6669f55` (`Keep held fire active during slow aim updates`; exact source tree `6b8011f1b30691981a9a484fe8865b545ff6d68d`).
- The owner tested Site v37 and reported rapid retarget was fixed, but any aim change stopped firing. Fixed the held-fire condition: held input continues during gradual aim updates, taps still wait for the current aim update, and the rapid-retarget lock still blocks firing.
- Owner-only Crisis Response Runtime Site version **39** is live at `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.
- Site v39 records game source `e009c00d240b63e67d29f9499d4740bbc6669f55`, Site source commit `f81db068d09485cbb54cd7d0035065a0e5ab2222`, deployment ID `appgdep_6add110fbaf08191bfbba54cf6e5e112`, and archive SHA-256 `0631b0e712ee11f92eb6184372e02dce0f70300ab2aec519e694c055e26e1a64`. Site access remains owner-only (custom allowlist, no external visitors).
- Site release manifest SHA-256: `75e4b73805fa0a518228b6d82d2a41d329d5b505a46518691fe9cb55d517a52a`.
- Pinned SDK: .NET 8.0.426 at `/workspace/.dotnet-8.0.426`; process shim: `/workspace/libprocshim.so`. Read `docs/DOTNET_BUILD_RUNBOOK.md` before .NET commands.

## Gate 8 status

The focused aim/input fix, pinned .NET 8 WASM publish, package verification, Site tests, owner-only release, and phone-test build are complete. Gate 8 remains open until the owner reports the physical iPhone Safari retest. This runner has no Chromium or WebKit executables, so no automated browser smoke was run. The repository’s Gate 8 plan lists Chromium and WebKit smoke as acceptance evidence; do not claim the full gate is closed without resolving that requirement or recording an owner-approved waiver. There is no Gate 9 in the current completion plan. Gate 7 visual wound/particle fidelity and native PhysX parity remain separate incomplete work.

## Verification on source `e009c00d240b63e67d29f9499d4740bbc6669f55`

- `node --test scripts/aim_fire_gate_test.mjs scripts/browser_input_test.mjs scripts/aim_direction_test.mjs`: **29/29 passed**, including held fire during gradual aim changes, tap waiting, and fast-retarget blocking.
- `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release --output /tmp/cpr-publish-e009/wwwroot`: **passed** with pinned SDK 8.0.426. The optional `wasm-tools` optimization workload was not installed.
- `scripts/package_wasm_site.py build` and `verify`: **passed**, 767 files, 199 Blazor resources, and 195 WASM files; build manifest records game source `e009c00…`.
- Site package tests: `node --test tests/*.test.mjs` — **33/33 passed**.
- Site v39 deployment returned `succeeded`; saved-version source commit and archive checksum were verified. No automated browser or new iPhone test is claimed.

## Next action

Open the Site URL above on iPhone Safari. Hold Fire while making gradual aim corrections and confirm bullets continue. Then make a fast retarget (for example, sky to floor) and confirm firing pauses until the weapon catches up, then resumes while Fire remains held. Report whether both behaviors pass. Do not change Site access settings. If they pass, close the device portion of Gate 8; address the missing Chromium/WebKit acceptance explicitly. After Gate 8, continue Gate 7 wound/particle rendering and native PhysX parity.
