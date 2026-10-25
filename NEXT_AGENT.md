# Current handoff — 2026-10-25

## Latest committed source checkpoint

- Branch: `develop/wasm`.
- Exact GitHub source commit: `bebd8d374d05c49699a44a76f3050cd92d8e758e` (`Transport live particle emission state and seed`). Local HEAD and the live branch matched after the push.
- This source commit has not been built into a new WASM package, saved as a Site version, or deployed.
- The exact last verified WASM build remains commit `a308a722cb4e466447ad82d6d7b59cd0107f892e`; its package is `/tmp/cpr-particle-site-package-a308a72`, with `build-manifest.json` SHA-256 `3aaeea77049e88ac46591dc4b3b2054253eef9ad6ed2da253452f1e18da2327a`. That evidence predates the current particle-controller and rendering changes and must not be reused as current-build evidence.

## Work completed at `0c07e395`

1. Preserved the bounded 1,919-asset `global-ui-mission-operator` dependency inventory and the complete particle asset, material, texture, mesh, shader, module, curve, burst, and owner-reference closure. See `docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md` and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`.
2. Added a source-verified controller closure for all 14 ballistic impact prefabs: 14 `BallisticHitEffect` roots and 26 `SingleEmissionEffect` components, each paired by original file ID to its particle system and renderer. The exported gzip SHA-256 is `0dec7b3eb1f7a20d95aa5ee58befbf0dd8226232ce4a0a795d41f5dc579382ce`; its source profile SHA-256 is `92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb`.
3. Added tested Unity MinMaxCurve/Gradient evaluation, source-authored entry/exit counts, speed ranges, lifetime and chunk sizing, plus source field delivery through `materialEffect` events.
4. Added WebGL drawing for the 26 impact emitters using source textures/materials/meshes and the authored cone-base shape (type 4) and circle-edge shape (type 10). Emit callbacks now include the actual count, random seed, speed range, lifetime range, and size multiplier; the browser correlates these to the impact prefab/component and source time. `sourceVisualParity` remains false because the browser PRNG does not reproduce Unity's internal random algorithm, and renderer mode 1, other shape paths, native shader equivalence, the broader 51-system lifecycle and browser pixels are not complete.

## Verification actually completed for current source

- `node --test scripts/*_test.mjs`: **116 passed, 0 failed**.
- `python3 -m unittest discover -s scripts -p '*test.py'`: **41 passed, 0 failed**.
- `node --check` passed for `game/original-particle-renderer.mjs`, `game/weapon-runtime.mjs`, and `game/original-particle-event-binding.mjs`.
- `git diff --check`: passed before commit.
- Re-exported the controller closure from `/tmp/cpr-incremental-mission-profile-v3`; output hash exactly matched the committed gzip above.
- Current environment has no `dotnet` executable (`dotnet --version` returns command not found). Therefore EngineChecks, FoundationChecks, Release WASM publish, and a current-source package build/integrity verification were not run. The existing `/tmp/libprocshim.so` does not provide the missing SDK/compiler.

## Dependency-ordered gates and concrete next actions

1. **Build and verify the exact current source.** Use a runner with pinned .NET SDK `8.0.426` and the documented process shim, run EngineChecks and FoundationChecks, publish `runtime/Browser/Crisis.Runtime.Browser.csproj` in Release, package its `wwwroot` into a new unique output directory, and run `package_wasm_site.py verify`. Record the full package path, source SHA, and manifest SHA. Do not use any older `/tmp/cpr-particle-site-package-*` directory as evidence for this commit. If no such runner is available, provision the SDK in this execution environment before continuing; the local blocker is specifically the absent `dotnet` binary.
2. **Close the rest of particle parity against the already complete dependency inventory.** Extend runtime contracts beyond the 26 one-shot impact emitters to all 51 systems and all owners; reproduce Unity's internal PRNG from the transported seed; implement shape type 2, mode-1 rendering, and source active modules (bursts, color/size/rotation, force, inherited/clamped velocity, and collision); then close blood/wound emitters, tracer, weapon-smoke, bounce, pause/time-scale, and destruction ordering. Add source-backed per-path regression cases before claiming parity.
3. **Sync only the selected, verified source to the private Sites source repository.** Site ID `appgprj_6acfef7174888191bc2bc2f269f60acc`, owner-only URL `https://crisis-response-runtime.craftysnipe13.chatgpt.site`, Site source branch `main`; its history differs from GitHub `develop/wasm`. The supported Sites write-credential plus `site-workflow.mjs` helper is available. Push a full source overlay while preserving the Site `.git` and `.openai/hosting.json`; verify the remote HEAD, build from that exact checkout, and integrity-check the resulting archive before saving anything.
4. **Save and deploy that exact verified private archive.** Automatic publish-on-push is disabled. Use the private Sites save-and-deploy operation only after Site source HEAD, tested build commit, and archive all identify the same source; preserve owner-only access.
5. **Run browser acceptance on that deployed version.** Verify Mission startup, full and bounded-memory delivery, end-of-load behavior, particle drawing, WebGL, physics/casings, input/aim/movement, weapon/audio timing, tap-zoom resistance, and readable errors using an available browser runner.
6. **Run physical iPhone Safari acceptance.** Use the user's device to check long-session memory, joystick/sprint resistance, absolute aim, weapons/audio, zoom behavior, and error visibility. The physical device is not available to this runner.
7. **Continue toward gameplay parity.** Inventory complete source dependencies before each batch for projectile/damage, ragdoll/joints, NPC behavior, remaining animation/IK, and interaction/menu systems. Implement and verify each batch, then rebuild and integrity-check the exact resulting source before publishing.

## Conditions that must not be blurred

- The current source changes are committed and pushed to GitHub, but have not been compiled into WASM or packaged.
- The prior `a308a72` package is valid evidence only for `a308a72`; it is not the current build.
- Browser and physical iPhone checks are unavailable here. Continue independent source implementation and tests while arranging those runners; record results by exact source and deployed-version IDs.
- Never save or deploy a Site version before exact-source Release build and package integrity verification pass.
