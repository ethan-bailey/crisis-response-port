# Original Unity Marksman WebGL — retained private runtime test (2026-10-16)

## Why this is the next experiment

`codex/projectile-material-impacts/NEXT_AGENT.md` was reviewed. Its next bounded task is justified: an older Unity feasibility run compiled **actual** Marksman WebGL binaries but immediately discarded them, and the HTTP smoke proved file serving, not browser execution. We must validate a real Unity instance before changing the game's engine strategy.

- Earlier independently verified successful native build: [run #38800542438](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38800542438), source SHA `4ed43897973d9fac607b4e57baf2cbe4102acf3d`, private log-only artifact `12004975378`. Player files were **not** retained then.
- Selected experiment source base: **that exact known-working commit**, not the current unrelated Unity foundation head. Isolated `codex/native-marksman-webgl-runtime-validation` branch. The old Unity PR #7, browser PR #9, SDK8 packaging PR #19, scene/UI audit PR #20 and `main` are **not modified, merged or reset**.
- Original recovery input is SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d` from private release `recovery-preservation-2026-10-15`.
- Unity editor image and mode are pinned by `scripts/unity_open_spike.sh`: `unityci/editor:ubuntu-2022.3.62f1-webgl-3`, Personal activation/seat return, scoped original compatibility repairs, Marksman-only scene, single-pass IL2CPP build. This is a **Unity 2022** rebuild of exported original Unity 2017 assets; it cannot alone certify source parity.

## Added files (only on the isolated branch)

| File | Function and boundary |
| --- | --- |
| `scripts/serve_native_webgl.py` | Localhost-only HTTP server. Outputs exact `Content-Encoding: br` for `*.wasm.br`, `*.data.br`, `*.framework.js.br` with their correct underlying content MIME; no owner-public hosting. |
| `scripts/native_unity_webgl_browser_smoke.mjs` | Runs **actual** generated original Unity HTML/loader in Chromium and WebKit. Observes native `createUnityInstance` Promise resolution, nonzero canvas, browser errors and requests; dispatches genuine browser mouse/keyboard input and retains screenshots. Merely dispatching input does **not** prove in-game movement. It does not modify source Unity game files. |
| `.github/workflows/native-unity-marksman-player-verify.yml` | One workflow-only push trigger, preserving original isolated licensing/cache/status/source hash checks. Requires original compiled output integrity and license deactivation, stores built player privately for 14 days and browser screenshots/logs privately. Does not run on source/document changes. |
| `.github/workflows/native-unity-browser-only-recheck.yml` | Manual-only fallback that downloads the exact retained build artifact from run #38895140830, checks every compiled file against the original manifest's SHA256 and reruns the real browser smoke **without** Unity, an editor image or a second license activation. Only useful if the private original player artifact was successfully uploaded. |
| `docs/NATIVE_MARKSMAN_RUNTIME_VALIDATION_2026-10-16.md` | This checkpoint, test boundaries, and rollback/continuation. |

## Execution and verdict gate

[One bounded GitHub Actions run #38895140830](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38895140830), launch/input source SHA `7f4f688cba700fe96b5c68c57d71f4d3428fad0c` (workflow commit, after server and browser scripts). At initial documentation this build was **in progress**. Do not claim success until it finishes and the following separate gates are examined.

1. Verify private repo and exact preserved recovery archive SHA, original script repair guards and single Unity Personal seat deactivation.
2. Native `webgl.log`: `Build Finished, Result: Success.`, `build_exit=0`, checksum manifest has nonempty real WASM/data/loader/index, all static HTTP files responded, no evidence-only green-job misinterpretation.
3. Preserve private player artifact (not a public site or download). Match byte counts and per-file SHA256 in `webgl-output-manifest.json`.
4. `native-wasm-headers.txt` and `native-data-headers.txt`: correct Brotli and WASM MIME, not a false-positive status 200 without browser decoding.
5. `native-browser-runtime.json`: Chromium/WebKit native `createUnityInstance` **actually resolved** and generated an active-sized canvas; report genuine errors, warnings and failed requests. Screenshots before and after input are observational evidence; do **not** infer character movement, AI or game-loop parity without a measurable source in-game state/input effect.
6. Neither the older success nor this Marksman-only experiment proves Mission01, scene transitions, full original C# behavior, original shader fidelity, native particle or iPhone Safari performance. Evaluate whether a **separate** native route is worth further work based on measured runtime evidence, retaining the stable .NET/PhysX browser baseline.

**Safety:** Original code/assets and Unity Personal credentials must remain private; never publish a hosted public demo. Workflow's only automatic trigger is editing its own YAML file. Do not run it twice while queued/running. Avoid changing the Unity input/build scripts as an unmeasured attempt to fix native playback.

## Rollback and continuation

Experiment changes are five **new** files on the isolated branch, not edits to original Unity files. Reverse with new selective commits, or simply leave this unmerged feasibility branch. Do not force-push, reset, change repo visibility, rebase or alter PR #7/main. Preserve the SHA-tested game/browser branch #9 and distinct packaged SDK8 PR #19. If this native experiment fails, document the first real failing step (licensing/image/build vs MIME/decompression vs browser startup) and continue independent verified WASM development. Successful startup alone is not an instruction to switch engines.

A GitHub caveat: `workflow_dispatch` for a *new* workflow file may be unavailable unless that workflow exists on the repository default branch. Do **not** alter `main` to fix that. If a retained-player browser recheck is needed, first confirm the artifact exists, then add a one-shot `push` trigger restricted to the browser-only workflow file on this isolated branch and intentionally update only that file. Do not enable such a trigger before player upload; that would run a pointless job.\n\nRetained player artifacts automatically expire after 14 days; save private evidence before expiration if required. No owner-private live site replacement or physical iPhone test is part of this workflow.
