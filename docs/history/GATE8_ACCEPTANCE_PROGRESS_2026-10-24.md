SUPERSEDED NUMBERING — see docs/GATE_DEPENDENCY_INDEX.md for current gate status.

# Gate 8 acceptance progress — 2026-10-24

## Tested source and injury result

The tested GitHub source is `develop/wasm` at `e91c7050f1424149aacdb61af1eba1388da2f959` (`Verify projectile injury reaches selected occupant`). The headless Mission 01 projectile contract now requires a shot to hit the selected original Occupant and requires wound, damaged-tissue, and body-shock values to increase.

With SDK 8.0.426 and the original scene package at `/tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json`, the original Operator M4 fired through `CharacterBody.PullTrigger` and the original FullSource weapon/projectile path. The projectile hit the selected Occupant's original Torso capsule on layer 14. Original physiology changed from 0 to 1 wound, damaged tissue volume from 0 to `93.34122`, and body shock from 0 to `271.9045`. The Occupant remained conscious and alive after this one shot. This probe stages actors in a clear fixture lane and moves other Occupants away for the shot; it is a test setup, not Mission 01 authored positioning.

The separate `--mission-ai-outcome-contract` run passed: Mission 01 started, all seven rooms cleared through original source calls, five Occupants spawned, all five reached ragdoll, the original mission success path ran, and the authored end menu became visible. It uses silent audio and headless adapters, so it does not establish native physics, rendered effects, or audible playback.

## Exact-source WASM and private Site

- `dotnet build runtime/Crisis.Runtime.csproj --no-restore --no-incremental -v:q`: passed, 0 errors, 48 warnings.
- `dotnet build runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -v:q`: passed, 0 warnings, 0 errors.
- `node --test scripts/browser_input_test.mjs`: passed, 19/19.
- `python3 -m unittest discover -s scripts -p 'package_wasm_site_test.py'`: passed, 13/13.
- `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release`: passed under SDK 8.0.426. The optional `wasm-tools` optimization recommendation remained.
- `scripts/package_wasm_site.py build` and `verify`: passed at source commit `e91c705…`, 766 files, 199 hashed Blazor boot resources, and 195 valid WASM binaries.
- Private Crisis Response Runtime Site deployment succeeded as version 33 at `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.

The exact-source interactive browser check is still blocked. The Site remains owner-only; its ChatGPT sign-in authenticated to an account that is not on the Site's owner access list, so the runtime page was not exposed. The local Playwright package is present but its Chromium and WebKit executables are absent. Do not change Site sharing to work around this. Continue after authenticating the Cloud Browser as the Site owner.

No physical iPhone Safari device is available in this runner. Wound decal pixels, particle rendering/simulation, and native PhysX remain unverified. Gate 8 is therefore open.

## Failures and fixes

1. The first projectile contract fired but hit the authored roof before reaching an Occupant. The fixture now stages a short clear lane, moves non-target Occupants out of that lane, asserts the selected Occupant is hit, and fails unless injury physiology increases.
2. The first exact-source package attempt found missing generated files under `game/vendor/`. `npm ci --prefix game --ignore-scripts` and `npm run --prefix game vendor` regenerated the lockfile-pinned PhysX adapter; package build and verification then passed.
3. The first Sites helper invocation ran from the game repository, which has no `.openai/hosting.json`. Reopening from an empty temporary checkout succeeded; no game source or Site sharing policy was changed by the failed attempt.
4. An early `dotnet run --no-build` used the prior check executable after check source edits. Building `runtime/Checks/Crisis.Runtime.Checks.csproj` first produced the current passing contract. The .NET runbook now records this rule.

