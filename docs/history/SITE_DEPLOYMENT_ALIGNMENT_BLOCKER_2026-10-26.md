# Site v75 deployment and source alignment evidence — 2026-10-26

## Exact deployed source and result

- Site project: `appgprj_6acfef7174888191bc2bc2f269f60acc`; configured artifact repository branch `main`.
- Site source commit: `d8dbe9f3c038e59afbf0d9903d5d036c1f3169b4`.
- Saved version 75: `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_3627e642babc8191baf0ba264e96a5af`.
- Deployment: `appgdep_6adf8880cd4c8191a4d16a6789d45c6a`, `succeeded`; production URL https://crisis-response-runtime.craftysnipe13.chatgpt.site.
- Site access remains owner-private. No audience changes were made.

## Exact build and package gates

Build from the Site checkout with .NET SDK 8.0.426:

```sh
dotnet build runtime/Crisis.Runtime.csproj -c Release --no-incremental
dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release -o /tmp/cpr-site-v75-publish-d8dbe
python3 scripts/package_wasm_site.py build --repo . --published /tmp/cpr-site-v75-publish-d8dbe/wwwroot --output dist --toolchain /tmp/cpr-site-v75-toolchain-d8dbe.json --incremental-source-package /tmp/cpr-site-v73-full-source.json --incremental-profile global-ui-mission-operator
python3 scripts/package_wasm_site.py verify dist
```

Runtime Release build: 0 errors / 48 warnings. Browser/WASM publish completed; optional `wasm-tools` optimizer workload was unavailable. Package verifier returned `status=verified`, exact `sourceCommit=d8dbe9f3c038e59afbf0d9903d5d036c1f3169b4`, 2,686 files, 199 Blazor resources, 195 WASM files, and Mission profile 1,919 assets / 22,365 records / 97,767,549 compressed bytes / 205,514,345 source bytes. Source package SHA-256 `ea3592ff5bdd39534bb905cd0abac74f456578b46eb4b12cdd0cb79fe8520cf7`; `dist/build-manifest.json` SHA-256 `c1610bf34f29e38c7efc82f5f3cd6ff4cc7d8e6d1db694183850d50f2f801a52`.

Deploy archive `/tmp/cpr-site-v75-deploy-d8dbe.tar.gz`: SHA-256 `ba24d8a315e4865ccc6d68cf925423fa59fa84d57462e7320778e8dbf42de2d8`, 138,792,202 bytes. It includes `.openai/hosting.json` with matching project ID and `static.directory=dist`, plus `dist/index.html`. After one extraction, all 2,686 manifest-listed files matched their recorded length and SHA-256. Sites stores its normalized TAR as `sha256:6bc154b5299a354782a9bcc9ed7a479aca0693dea8903f3b4a3e454d20299f59`, 178,022,400 bytes / 2,688 files. These are different archive encodings and hashes.

## Sprint control fix

Approved analog movement source `docs/APPROVED_IPHONE_ANALOG_MOVEMENT.md` specifies sprint entry at 1.75 times normalized joystick radius and release at 1.5 times. The implementation had drifted to 1.42/1.24, making sprint too easy. Site source commit `d8dbe9f...` corrects the thresholds and covers below-wall, activation, hysteresis and release boundaries. Full Site Node tests passed 142/142; targeted browser input, aim, zoom and aim/fire contracts passed 33/33. This does not establish physical joystick ergonomics; owner iPhone acceptance is still required.

## Recovered actor source package audit

This command passed:

```sh
python3 scripts/verify_runtime_package.py /tmp/cpr-recovery-full-source/export/ExportedProject /tmp/cpr-site-v73-full-source.json
```

Summary: 2,060 package assets, 40,139 records, 24,856 dependency edges. Operator and Occupant actor closures each had zero unresolved GUIDs and unsupported builtins. Use the `export/ExportedProject` directory; `work/source/unpacked` is not the export root.

## Browser and device limits

The cloud browser reports WebGL disabled (`GL_VENDOR=Disabled`) and failed WebGL context creation. This is an automation-runner limitation, not a visual smoke pass or proof of production failure. The owner still needs physical iPhone Safari checks for Mission cold/warm loads and persistence, late asset progress, absolute vertical-only aim, walk/sprint feel, rapid-tap zoom, and fire/reload speaker output.

Public `develop/wasm` now contains test commit `da3fae42504557e131d255feed6a8d51b127a699`. Exact-SHA Actions run #81 (`40368479589`) succeeded: jobs `121733263824` and `121733264439`; pinned .NET 8.0.426 runtime/Release/WASM checks, all source contracts, full Mission package verification, IndexedDB full-profile and Chromium audio passed. Public input implementation SHA-256 `0f5a424597c32624f121648f51a3cf7d7bf608cd2b55ff311717e03e5caa6a78` matches Site v75; public-only test SHA `108f8d956d0a991bd9eb121d07b230fcfeee4aced7f9c72e95c931fb18e212c8` includes the new puck-clearance assertion. Full-profile result: 1,919 persisted, 11,065 ms cold, 1,919 reload cache hits, zero reload network assets, 8,568 ms warm. Package source index SHA `9d866276582031d440c316c8e60d69d252506cf67a8ae9d03dbdbe0d91980f92`, deterministic archive SHA `4e1e1d4495684bede24e7c88c1f5e5dace401fc54b43bd49115d146ee6dd42ec`, retained artifact `12672355752` digest `sha256:87586a07489cba47407980ee984ab90f725cb3822f1819a1a2293fba0b477238`. These public-run checks do not change Site v75’s source identity. The untracked recovered mesh must remain excluded from public Git and Site package.
