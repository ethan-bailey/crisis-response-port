# Gate 8 acceptance attempt — 2026-10-23

## Tested source and scope

- Tested source SHA: `2530764705352f98c3c42018b8aa2238ce561842` (`develop/wasm` before this evidence/handoff commit).
- Pinned .NET SDK: `8.0.426` from `/tmp/codex-dotnet-8.0.426`, using the documented `/tmp/libprocshim.so` runner workaround.
- Goal: continue after Gate 7 toward combined two-actor gameplay, exact-source WASM/browser acceptance, and physical iPhone Safari acceptance.

## Passing checks

- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- --mission-contract /tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json` — passed. Mission 01 reached `Mission.Start` / `BeginMission`; 2 Operators, 5 Occupants, 8 CharacterBody instances. The headless adapter suppresses/silences presentation and audio and reports many incomplete scene/presentation references; this is not full scene or browser parity.
- `dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- --spawner-contract /tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json` — passed. Original Occupant spawning, actor-owned trauma/physiology, movement force, and animation parameters; reported 0.31 ml blood loss in the targeted test.
- The broad default `runtime/Checks` pass executed the RuntimeContracts, ScenePrefabContracts, ArmSequenceContracts, PresentationContracts, WeaponContracts, and BallisticContracts successfully before its strict scene-load loop failed (see errors below).
- `dotnet run --project runtime/EngineChecks/Crisis.Runtime.EngineChecks.csproj --no-restore` — passed, 69 assertions.
- `dotnet run --project runtime/FoundationChecks/Crisis.Runtime.FoundationChecks.csproj --no-restore` — passed, 121 assertions.
- `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release` — passed with SDK 8.0.426. The SDK emitted its optional `wasm-tools` workload recommendation; publish completed.
- `python3 -m unittest discover -s scripts -p 'package_wasm_site_test.py'` — passed, 13 tests.
- `npm ci --prefix game --ignore-scripts` and `npm run --prefix game vendor` — passed; produced the pinned PhysX browser files from `physx-js-webidl@2.7.3`.
- `scripts/package_wasm_site.py build` and `scripts/package_wasm_site.py verify` — passed for source SHA `2530764705352f98c3c42018b8aa2238ce561842`: 767 package files, 199 Blazor boot resources, 195 valid WASM files. Package path: `/tmp/cpr-gate8-site-2`.

## Errors, gaps, and dispositions

1. **First package attempt lacked vendored PhysX files.** Error: `Missing site input: game/vendor/physx-js-webidl.mjs`. Applied the documented lockfile-pinned `npm ci` + `npm run vendor` steps; package build/verification then passed.
2. **First package unit-test invocation used the wrong module path.** `python3 -m unittest scripts.package_wasm_site_test` failed with `ModuleNotFoundError: No module named 'package_wasm_site'`. Corrected to unittest discovery from `scripts/`; all 13 tests passed. This was a test invocation error, not a product failure.
3. **Strict full scene-load sweep failed.** The broad `runtime/Checks` command reached source runtime, scene/prefab, arm sequence, presentation, weapon, and ballistics passes, then threw `Unresolved supported fields` at `runtime/Checks/Program.cs:265` while loading the first scene in its loop, `Assets/Scenes/Marksman range.unity`. Gate 7's Mission 01 contract still passes under its documented limited headless scope. Scene import closure remains incomplete; do not classify this as a compiler error.
4. **Chromium could not be provisioned.** Playwright attempted the official Chrome-for-Testing download; the endpoint repeatedly returned a zero-byte archive, causing `End of central directory record signature not found`. Stopped after the same failure repeated. No local browser binary is installed.
5. **Cloud browser access was rejected.** Opening `http://127.0.0.1:8765/game/` to test the localhost package was auto-review-blocked because it could transmit private project files to a remote browser service. Do not retry through another browser surface or use an indirect route. Continue only after the owner explicitly approves that specific disclosure, or use a materially safer browser runner.
6. **Physical iPhone Safari was unavailable.** This workspace cannot provide an actual iPhone Safari runtime. Desktop/browser emulation cannot be reported as iPhone acceptance.
7. **No full linked two-actor combat sequence was established.** The mission and spawner contracts prove actor startup, ownership, injury/physiology isolation, movement force, and animation parameter production. Weapon and ballistics contracts pass in separate source-backed test paths. They do not yet prove one normally initialized Mission 01 sequence connecting Operator fire to the Occupant's original limb hit, physiology outcome, and destruction/weapon-drop chain.

## Gate 8 result

Gate 8 remains open. WASM compilation and package integrity pass at the tested SHA, but exact-build browser behavior, the linked two-actor combat flow, and physical iPhone Safari acceptance remain unverified. The browser application page explicitly describes full gameplay as incomplete. Do not publish this package as a completed game or claim Gate 8 acceptance.

## Next action

Obtain the owner's explicit approval before transmitting the private package to the remote browser for testing. Then close the linked two-actor scenario and rerun one final exact-SHA package/browser acceptance. Physical iPhone Safari acceptance remains a separate required device check.
