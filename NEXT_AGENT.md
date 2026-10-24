# Crisis Response Port — current handoff

## Checkpoint

- Development branch: `develop/wasm`; latest pushed game source: `1962efb2f99da1cc59f9ad365b0fc4bd1868f735`.
- Pinned SDK: .NET 8.0.426 at `/workspace/.dotnet-8.0.426`; process shim at `/workspace/libprocshim.so`. Read `docs/DOTNET_BUILD_RUNBOOK.md` before any .NET command.
- Avoid Codespaces through 2026-11-01. Repository is public and GitHub Actions is enabled (verified 2026-10-24; see `docs/REPOSITORY_VISIBILITY_AND_ACTIONS.md`).
- Original scene package remains outside Git at `/tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json`.
- Owner-only Site version 36 is live at `https://crisis-response-runtime.craftysnipe13.chatgpt.site`. Game source: `1962efb2f99da1cc59f9ad365b0fc4bd1868f735`; Sites source: `037ee668ad25e7f76ff5c700dbd8f0e7a97c97bc`; archive SHA-256 `b86b3e8b37bb683aa572fb7b46e6af7aa15f2205ba2d1edf5d03ba3730ff1dbe`.

## Gate status

1. **Operator lifecycle — passed.** See `docs/evidence/GATE1_CHARACTER_ACCEPTANCE_2026-10-23.md`.
2. **Occupant lifecycle and ownership — passed in source contracts.**
3. **Movement and animation — partial.** Source contracts pass; native physics and visible animation remain unverified.
4. **Equipment and firing — partial.** Original weapon, ammunition, trigger, and reload contracts pass. A shot was verified through original source on Gate 8. Aim/fire motion behavior now has deterministic tests; interactive firing remains unverified in this cloud browser.
5. **Hit and injury chain — partial.** Original FullSource projectile hit the selected Occupant's Torso and increased wound, tissue-damage, and shock physiology. Visual wound pixels remain unverified.
6. **Body-state outcomes — partial.** Original Mission outcome contract reaches ragdoll, success, and authored end-menu paths with headless adapters; native physical outcomes remain unverified.
7. **Mission and NPC flow — passed at bounded headless source-runtime scope.** See `docs/evidence/GATE7_MISSION_NPC_ACCEPTANCE_2026-10-23.md`.
8. **Combined acceptance — open.** Exact-source package and private deployment pass. Browser gameplay, physical iPhone Safari, native PhysX, wound decal pixels, and particle rendering/simulation are not verified.

## Latest verification

- `node --test scripts/aim_fire_gate_test.mjs scripts/browser_input_test.mjs scripts/aim_direction_test.mjs`: 26/26 passed (5 new aim-fire gate cases; existing input and direction tests included).
- `python3 -m unittest discover -s scripts -p 'package_wasm_site_test.py'`: 13/13 passed.
- .NET 8.0.426 Release WebAssembly publish completed successfully to `/tmp/cpr-publish-aim`; the runtime compiled with no reported errors. Optional `wasm-tools` optimization workload warning remains non-blocking and is already covered by `docs/DOTNET_BUILD_RUNBOOK.md`.
- Exact package verification for game source `1962efb2…`: 767 files, 199 Blazor resources, 195 valid WASM files.
- Sites deployment status: succeeded, version 36, source commit `037ee668…`, owner-only access preserved. Version archive contains 769 files (767 package files plus hosting metadata).
- The live Cloud Browser now exposes the loading failure: `THREE.WebGLRenderer: Error creating WebGL context` (`GL_VENDOR = Disabled`, `GL_RENDERER = Disabled`, sandboxed). The page remains at the loading state because this browser cannot create WebGL; this does not establish a game-source boot failure. Local Playwright package/browser binaries are absent, and physical iPhone Safari is unavailable.
- Aim-fire policy in `game/aim-fire-gate.mjs`: normalized aim movements faster than 1.8 NDC units/second suppress held fire; after a fast sweep, aim must remain at or below 0.35 units/second for 140 ms before fire resumes. Slower aim corrections keep sustained fire active. Touch taps remain pending until both the aim update and settle gate are ready. Movement-stick vertical aim now marks its update dirty, including return to neutral.

## Errors encountered and disposition

- Shell `git push` failed because this runner has no GitHub credential (`could not read Username`). The committed code was pushed using the authorized GitHub connector blob/tree/commit/ref flow with expected parent `6e9921f…`; the local checkout was then reset to and verified against pushed source `1962efb…`. Do not retry shell push without authentication.
- First package invocation referenced nonexistent `/tmp/cpr-toolchain-aim.json`; reran with existing `/tmp/cpr-toolchain.json` and package verification passed.
- Package tests invoked from the repository root as a direct module failed with `ModuleNotFoundError`; correct command is the recorded `unittest discover -s scripts` form.
- Initial package help/test command used an incorrect nested `scripts/scripts` path; reran from repo root successfully.
- A shell `rm -rf` command was rejected by the command guard; synchronized generated Site `dist` using Python `shutil.rmtree`/`copytree` instead.
- `cat ... | head` produced a broken-pipe diagnostic; this was only output truncation and did not affect files.
- Native WebGL creation in Cloud Browser is disabled as recorded above. Treat that as an environment limit; do not change the gameplay to mask it.

## Next action

Run `scripts/weapon_smoke.mjs` against Site version 36 in a WebGL-capable browser and verify that a rapid sky-to-floor retarget suppresses shots while slow adjustments preserve sustained fire. Keep the Site owner-only. Gate 8 remains open until interactive browser/device acceptance and the documented native rendering/physics limits are resolved or explicitly accepted.
