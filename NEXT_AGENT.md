# NEXT AGENT — Crisis Response port completion

## Checkpoint

- Live `develop/wasm`: merge commit `f3802019e7cfdd419223dc3e02b9df87b8035289` (PR #68 merged 2026-10-23).
- PR #68 merged the Mission 01 package fix: the Site packager now generates CRG1 from tracked `game/assets/mission01.obj`, writes provenance hashes, and checks source/binary hashes before building the integrity manifest.
- Package verification on the PR head: Python package tests 12/12 passed; generated scene checks 2/2 passed. Unity source mesh export: 45,356 vertices, 100,998 indices, 33,666 triangles.
- GitHub Actions remain disabled by owner decision. Other open PRs noted before this merge: #56 and #57 target `develop/wasm`; #7 targets `main`.
- Codespaces and an interactive real browser are optional when another environment can complete the same verification.

## Remaining gates

- The current workspace did not have `.NET SDK 8.0.426`; no fresh build, publish, full Site package, or live smoke was run on merge commit `f3802019`.
- Prior browser smoke loaded the WebAssembly control panel and passed 10 runtime contracts, but no Marksman scene appeared or loaded. A full gameplay smoke still needs to prove scene loading and core controls.
- The two-actor prefab lifecycle gate remains unresolved. The original harness lacked the global/Mission scene binding (null `Global.Context`); after a temporary harness-only binding, `TeamOperator.Awake` failed at `TeamOperator.cs:110`. The temporary fixture was reverted. Do not treat a partial Operator-only run as a pass.
- Existing Sites project: `appgprj_6acfef7174888191bc2bc2f269f60acc`, version 26, source SHA `64d9c812ab01e23f37ac6c0b0b3137b71130e62d`. No new Site version was saved or deployed.

## Next viable steps

1. In an available environment, use the pinned .NET SDK 8.0.426 and run a fresh Release build and browser publish at `f3802019`; leave the SDK pin unchanged.
2. Assemble a fresh Site package from that publish output. Verify the generated Mission 01 CRG and provenance report, then validate every entry in the integrity manifest.
3. Fix or complete the scene-loading path so the browser loads the Marksman scene; rerun the browser smoke and verify scene rendering plus input behavior.
4. Repair the two-actor lifecycle harness by provisioning the actual Mission/global dependencies, then pass both Operator and Occupant lifecycle checks without temporary harness shortcuts.
5. After all local gates pass, publish the verified package to the existing Sites project and run a live smoke against served WASM and scene assets. Vercel remains a manual fallback only if Sites cannot host a locally passing package. Do not publish before local verification.



## Verification attempt at checkpoint 8ab1c2d (2026-10-23)

- Verified source SHA: `8ab1c2d49a72dd661ef750377fa5caeecbc49715`, based on PR #68 merge `f3802019e7cfdd419223dc3e02b9df87b8035289`. PR #68 is already merged.
- .NET SDK 8.0.426 was present and confirmed with `dotnet --list-sdks`. This checkpoint has no root `global.json`; the clean isolated worktree used a temporary root copy of the tracked `tools/wasm-compile/global.json` so the requested commands selected exactly 8.0.426. Tracked source at the checkpoint was not changed.
- Fresh Release build `dotnet build runtime/Crisis.Runtime.csproj --configuration Release --no-incremental`: passed, 0 errors, 45 warnings. Browser runtime publish: passed, 0 errors under SDK 8.0.426; the SDK noted the optional `wasm-tools` workload.
- Site packaging was attempted with `scripts/package_wasm_site.py`. The lockfile-pinned PhysX 2.7.3 dependency was staged with the tracked vendor script. Packaging then failed validation: `ValueError: Missing site input: runtime-web/browser-input.mjs`. `game/browser-input.mjs` exists, but the exact checkpoint's browser publish does not include its runtime-web copy. No complete package was produced, so package file count/size and full manifest verification are unavailable.
- Since no verified assembled package exists, the required real-browser smoke was not run; there is no browser console evidence. The scene-loading/Marksman/FIRE gate and two-actor Operator/Occupant lifecycle gate remain unverified. Sites and Vercel publication/live smoke were not attempted.
- Next viable steps: make a bounded host-project change that links the existing `game/browser-input.mjs` into the browser publish output; rebuild and publish at the resulting exact SHA; rerun package assembly, CRG/provenance checks, and full SHA-256 manifest verification. Then run the assembled-package browser smoke and resolve the original scene-loading path. Only after that, run Operator and Occupant lifecycle checks with real loaded-scene dependencies. Publish to the existing Sites project only if both local gates pass.
- GitHub Actions remain disabled. Do not merge PRs #56, #57, or #7. This attempt did not alter runtime source or create a deployment.
