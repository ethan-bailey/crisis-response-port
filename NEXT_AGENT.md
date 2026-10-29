# Crisis Response Port — current handoff

## Current source and gate status

- Work branch: `develop/wasm`. Current source HEAD before this handoff commit: `0d1b322f82aa7667494d299d313f2a3f115e932f`; checkout and live remote matched at session start. See [docs/GATE_DEPENDENCY_INDEX.md](docs/GATE_DEPENDENCY_INDEX.md) for the only numbered gate list.
- Fresh full-source runtime build at HEAD with SDK 8.0.426: **0 errors, 49 warnings** (previous baseline: 0 errors, 45 warnings). Release WASM publish and exact-source package integrity passed. Package: 1,920 assets, 199 Blazor resources, 195 WASM binaries; source index SHA-256 `38662f3e31ae4325d15c0c77d35e5542e2af403d6a89d77407b530dfd470d47`.
- Packaged Chromium step 29 passed on exact source HEAD using the Playwright-pinned Chromium 156.0.8078.4 and a temporary local mirror for the exact Three.js 0.180.0 module after jsDelivr failed in this runner. X moved `−0.6600008 → −0.817621`; source `MovementDelta=−0.048979104`; `Body walk backward` selected; release returned to `Body stand`. Page errors: none. Screenshot inspection showed substantial scene occlusion of the actor; visual gait/IK parity is not accepted.
- Reload task and original magazine-out/in audio/arm pose passed in that packaged run. It began with a full magazine and ended at 30 rounds with 30 reserve rounds; the source contract also reported 2 reserve magazines before and after. No round-depletion-to-reserve transition was exercised, so Gate 3 remains open.
- Gate 4 packaged source contracts passed for two source projectile hits into separate Occupants, torso wound/tissue/shock changes, Mission outcome, and reload task. No packaged-browser projectile-hit-to-injury/outcome sequence was run. Gates 4–8 remain open as detailed in the index; parity flags stay open pending combined acceptance and owner iPhone Safari acceptance.
- The four `source-reference/Original/NPCRecovery` Occupant files are archival source restored from retained branch #64. The runtime build did not compile those archival files, and no runtime acceptance was run against them.
- GitHub Actions run #165's six Node failures were stale inventory paths. The generated inventory is tracked at `docs/history/mission-particle-dependency-inventory-2026-10-25.json.gz`; its exporter is `scripts/export_mission_particle_dependency_inventory.py` and requires an explicit `--out`. Six test references were corrected to the archived path; all 202 Node tests pass locally.

## Environment and next action

- Checkout: `/workspace/crisis-response-port`; pinned SDK: `/root/.dotnet/dotnet` 8.0.426; Chromium: `/root/.cache/ms-playwright/chromium-1248/chrome-linux64/chrome` 156.0.8078.4.
- Combined preflight passed SDK and Chromium checks. Shell push authentication is absent by design; use the connected GitHub API for repository writes.
- Next dependency-ready work: add a packaged-browser case that spends source ammunition before reloading and asserts the magazine/reserve transition; then exercise packaged projectile-to-limb injury/outcome, authored IK output, two-actor behavior, and particle/spatial-audio/camera output. Do not close parity flags without the combined exact-source acceptance and owner iPhone Safari result.
