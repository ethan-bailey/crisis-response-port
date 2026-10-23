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
## Handoff update — 2026-10-23 Codespaces package and browser gate

Checkpoint: branch `develop/wasm`, HEAD `2cc11eb803d8078f0747e4baf0c733786a99ab1c`. The working tree was clean before this handoff update. The full build and package were made from this checkpoint with .NET SDK 8.0.426.

- Fresh `dotnet build runtime/Browser/Crisis.Runtime.Browser.csproj --configuration Release --no-incremental`: 0 errors, 45 warnings. Fresh publish completed.
- `scripts/package_wasm_site.py build` completed and reported `status: verified`, 750 files and 195 WASM files. `scripts/package_wasm_site.py verify /tmp/crisis-site-package` succeeded. The package contains `scene-data/crisis-response-scenes.json.gz` and `scene-data/marksman-range.json`; the scene-data entries are covered by `build-manifest.json`. The compressed scene package is ~83 MB.
- Served the assembled package from `/tmp/crisis-site-package` on port 4173 and opened it in a fresh Codespaces Chrome tab. `/runtime-web/` booted WASM and showed `PASS · 10 verified runtime contracts`; this is the input/runtime harness page, not a playable-scene pass. `/game/` reached the HUD but remained on “Loading recovered scene and original Operator prefab…” / “Loading original weapons…”, with no scene displayed.
- Browser console blocker: Three.js failed to create WebGL at `game/:100:16`: `GL_VENDOR = Disabled`, `GL_RENDERER = Disabled`, `Sandboxed = yes`, followed by `Error creating WebGL context.` No scene display/input smoke can pass in this browser. Codespaces also has no `chromium`, `chromium-browser`, or `google-chrome` binary, and no repo-local Playwright package; that was the one bounded alternate-browser check. Disposition for step 4: provably impossible in this environment.
- Step 3 remains open: `runtime/Browser/wwwroot/index.html` has not yet been wired to call `LoadScene` in the recorded MainMenu → Transition → CampaignManager build index 1 → Marksman order, and the smoke gate has not been updated for that sequence. Do not treat the 10 runtime contracts as satisfying it.
- The two-actor gate was not rerun through the real loaded scene because step 4 failed. The prior recorded attempt stopped at `CharacterBody.Awake` on `Operator(Clone)` with a `NullReferenceException`; it did not identify the null member and did not complete the Operator/Occupant lifecycle.
- No Sites feasibility probe or publish was attempted; the hard local scene/browser gate did not pass. No Vercel fallback was attempted. GitHub Actions remains disabled.

Resume only with a WebGL-capable Codespaces browser (or another authorized real browser in this session): complete the host `LoadScene` wiring and real-scene two-actor gate, then rebuild, assemble and verify the package, and repeat the full local smoke before any Sites publish.


## Checkpoint 2026-10-23  reconciled source, browser gate blocked

- Branch: develop/wasm was reconciled from the remote head 1c076c6c01f4bf1da8eafbae30629ba4de258711. The starting local head was c2a4f37d3d0674183d13e89a70ef44e6c323deb8. Backup branch: backup/develop-wasm-c2a4f37-before-reconcile-2026-10-23 (points to the original local head). No force push or GitHub Actions run.
- Local-only commit dispositions:
  - e9256f23968553621c02cd23c975960365f23256 Recover original Unity scene sources from GitHub artifact  kept/replayed.
  - ccba7d1a96763f6cbd23a8446bff93994376b16a Load original scene flow in browser host  kept/replayed.
  - b837d071e49972715cc752713835ba4504566c7e Record scene package deployment blocker  dropped as superseded: remote 449bb9d4ced83f53d05b4d2c02a9537b389a159d generates the CRG and 88b3dcfbf386ead0fbbb7d5261ab97f3e40785bf contains exact Mission 01 geometry. Original commit remains reachable from the backup.
  - 88b3dcfbf386ead0fbbb7d5261ab97f3e40785bf Recover exact Mission 01 scene geometry  kept/replayed.
  - 2cc11eb803d8078f0747e4baf0c733786a99ab1c Include browser input adapter in WASM publish  kept/replayed.
  - c2a4f37d3d0674183d13e89a70ef44e6c323deb8 Record blocked local browser publish gate  kept/replayed; original commit remains on backup.
- Remote-only commits integrated, in order: 449bb9d4ced83f53d05b4d2c02a9537b389a159d fix: generate original scene binary during packaging; ebc83e3c52e3190653480f50d403d623333e22ad docs: hand off scene package gate; f3802019e7cfdd419223d3ce02b9df87b8035289 Merge pull request #68 from ethan-bailey/codex/scene-package-from-original-obj-20261022; 8ab1c2d49a72dd661ef750377fa5caeecbc49715 docs: refresh port completion handoff after PR 68; 1c076c6c01f4bf1da8eafbae30629ba4de258711 Record SDK 8.0.426 package blocker at checkpoint 8ab1c2d.
- WebGL probe passed before gates with Chromium SwiftShader: GL_VENDOR = Google Inc. (Google); GL_RENDERER = ANGLE (Google, Vulkan 1.3.0 (SwiftShader Device (Subzero) (0x0000C0DE)), SwiftShader driver).
- Host-only boot correction commit: 7975d643687f43cf99aca685984ae7e1d4f3c62 Await Blazor startup before browser scene flow. It disables implicit autostart and waits for Blazor.start before the original scene entry point. No gameplay code was changed.
- Fresh Release build at SDK 8.0.426: PASS, 0 errors, 45 warnings. Browser publish: PASS. Exact package: /tmp/crisis-site-package-7975, source commit 7975d643687f43cf99aca685984ae7e1d4f3c62. Package verifier: verified, 750 files, 199 Blazor resources, 195 WASM files. Generated CRG and provenance match: source SHA-256 891dc21c94f1be328b48b6c24f8624c68a7125283b0c386230304009abe65fda, binary/CRG SHA-256 ad1b48755b368ebab7b3e44dbc367ae01c0e4968fc3f213aac0f8c2a7924d79; 45,356 vertices, 33,666 triangles. Manifest verification covered every entry. Build log: /tmp/build-7975.log; package is at /tmp/crisis-site-package-7975.
- Browser evidence on the assembled package at http://127.0.0.1:4178/runtime-web/: scene-flow gate FAIL / incomplete. Page boot probe reports System.InvalidOperationException: Prefab Assets/Scenes/Title.unity: script UIPreferencesMenu fileID 10017, fields [m_ObjectHideFlags, m_PrefabParentObject, m_PrefabInternal, m_GameObject, m_Enabled, m_EditorHideFlags, m_Script, m_Name, m_EditorClassIdentifier, generalPrefab, controlsPrefab, performancePrefab, graphics]. The runtime contains no UIPreferencesMenu implementation; strict serialized-field assignment rejects it while loading Title. Boot probe log: /tmp/probe_host_7975.log (console logs empty). The full scene-flow gate then ended with Chromium Target crashed before it could emit a scene result; gate output /tmp/crisis-scene-flow-7975.stdout and console log /tmp/crisis-scene-flow-console-7975.log (empty before target crash). Treat as FAIL, never as a game pass.
- Marksman render, in-scene FIRE, Operator/Occupant lifecycle: NOT RUN; original menu-to-scene flow did not complete. CharacterBody.Awake NRE was not reproduced against a loaded scene in this run, so no lifecycle fix attempt was made. Prior handoff's unloaded-scene NRE remains unresolved and does not satisfy this task's lifecycle gate.
- Sites: no save, access change, deployment, or Vercel fallback was attempted because the local scene gate failed. Existing project remains Crisis Response Runtime at https://crisis-response-runtime.craftysnipe13.chatgpt.site, previously owner-only; public access was not enabled. No live smoke was run.
- Next bounded item: restore/provide the original UIPreferencesMenu component behavior or a source-backed compatible implementation, then rebuild and rerun the complete flow and lifecycle gates against the assembled package. Do not skip the Title component or claim the menu flow passed.
- Checkpoint source SHA: 7975d643687f43cf99aca685984ae7e1d4f3c62. At handoff before this documentation update, branch tracked remote at 0 behind / 11 ahead; preserve normal fast-forward-only push semantics and keep GitHub Actions disabled.
