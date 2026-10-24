# Crisis Response Port — current handoff

## Checkpoint

- Development branch: `develop/wasm`.
- Latest pushed game source: `e91c7050f1424149aacdb61af1eba1388da2f959`.
- Pinned SDK: .NET 8.0.426 at `/workspace/.dotnet-8.0.426`; process shim at `/workspace/libprocshim.so`. Read `docs/DOTNET_BUILD_RUNBOOK.md` before .NET commands.
- Avoid Codespaces through 2026-11-01. GitHub Actions remain disabled.
- Original scene package is excluded from Git at `/tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json`.
- Current private Site deployment: version 33, `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.

## Gate status

1. **Operator lifecycle — passed.** See `docs/evidence/GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`.
2. **Occupant lifecycle and ownership — passed in source contracts.**
3. **Movement and animation — partial.** Source contracts pass; native physics and visible animation remain unverified.
4. **Equipment and firing — partial.** Original weapon, ammunition, trigger, and reload contracts pass. A shot was verified through original source on Gate 8.
5. **Hit and injury chain — partial.** The original FullSource projectile hit the selected Occupant's Torso and increased wound, tissue-damage, and shock physiology. Visual wound pixels remain unverified.
6. **Body-state outcomes — partial.** The original Mission outcome contract reaches ragdoll, success, and authored end-menu paths with headless adapters; native physical outcomes remain unverified.
7. **Mission and NPC flow — passed at the bounded headless source-runtime scope.** See `docs/evidence/GATE7_MISSION_NPC_ACCEPTANCE_2026-10-23.md`.
8. **Combined acceptance — open.** Exact-source package and private deployment pass. Interactive browser testing needs the Site owner's ChatGPT identity in the Cloud Browser. Physical iPhone Safari, native PhysX, wound decal rendering, and particle rendering/simulation are not verified.

## Latest verification

See `docs/evidence/GATE8_ACCEPTANCE_PROGRESS_2026-10-24.md` for results, scope, and error dispositions.

- Full runtime build: 0 errors, 48 warnings.
- Checks project build: 0 errors, 0 warnings.
- Mission projectile contract: selected Occupant Torso hit; wound 0→1, damaged tissue volume 0→93.34122, shock 0→271.9045. One-shot target remained conscious/alive.
- Mission outcome contract: seven rooms cleared, five Occupants spawned, all ragdoll, original success and end-menu path passed.
- Browser input tests: 19/19. WASM package tests: 13/13.
- Release WASM publish: passed with the optional `wasm-tools` optimization warning.
- Exact package verify: source commit `e91c705…`, 766 files, 199 Blazor resources, 195 valid WASM binaries.
- Sites private deployment: succeeded, version 33. Site access is owner-only.

## Errors and known limits

- First projectile probe hit the authored roof before any victim. The contract now stages a short clear lane, moves other Occupants away in the fixture, and fails unless the selected victim receives wound, tissue, and shock changes.
- Missing generated `game/vendor/physx-js-webidl.mjs` was fixed by the lockfile-pinned `npm ci --prefix game --ignore-scripts` and `npm run --prefix game vendor` commands.
- Build a changed Checks `.csproj` before running its binary with `--no-build`; otherwise old check code may run. This is documented in the .NET runbook.
- The first Sites source helper ran from the game repo without `.openai/hosting.json`; opening from `/tmp/cpr-site-open-debug` succeeded. No access policy was altered.
- Chrome reached the private Site sign-in, but the authenticated ChatGPT identity did not have owner access. Do not make the Site public or add an external viewer. The user must authorize the owner identity in the Cloud Browser before browser acceptance can continue.
- Local Playwright libraries exist, but Chromium and WebKit binaries are absent. No physical iPhone/Safari is mounted.
- Headless injury testing disables wound decals and records particle/force commands. It does not prove rendered pixels, particle simulation, or native PhysX. The separate outcome fixture does not prove that a projectile injury itself caused unconsciousness or death.
- The standard package-driven `Crisis.Runtime.Checks` suite has a separate known blocker: unresolved supported fields while importing Marksman range (`Program.cs:328`); see earlier handoff evidence. The focused Gate 8 contracts above are separate passing runs.

## Next action

Continue after the user completes the Cloud Browser handoff using the Site owner's ChatGPT identity; then run `scripts/runtime_wasm_smoke.mjs` against the version 33 private Site, capture result and console errors, and add exact-SHA evidence. Do not change the owner-only audience. Physical iPhone Safari acceptance requires an available iPhone. Gate 8 remains open until the browser and device checks pass; keep native physics and rendering limits explicit.
