# NEXT AGENT — Crisis Response scene package and Sites

## Checkpoint

- Live `develop/wasm`: `54f1cb18e399789d4b55a9eed750fad4485081e9` (verified during this session).
- Draft PR #68: `codex/scene-package-from-original-obj-20261022`. Tested code commit: `449bb9d4ced83f53d05b4d2c02a9537b389a159d`. This handoff update is documentation-only; read the live PR head before continuing.
- Other open PRs: #56 and #57 target `develop/wasm`; #7 targets `main`. GitHub Actions remains disabled by owner decision.
- The owner made Codespaces and an interactive real browser optional when a faster verification path exists. Do not treat those environments as prerequisites.

## Scene package gap and change

The live source tree has `game/assets/mission01.obj` but no `game/assets/mission01.crg`; Git history has no commit for that CRG path. The game page fetches the CRG and the Site packager requires it. The retained recovery artifact is run 38582072502 / artifact 11903811738 (expires 2026-10-28); it contains the original `Combined Mesh (root_ scene)_1.asset`.

PR #68 generates CRG1 from the tracked OBJ during package assembly, emits the existing hash report, and verifies source and binary hashes. It includes a failure-oriented package test with no prebuilt CRG fixture. Keep PR #68 draft until the pinned full build and remaining runtime gates pass.

## Verified in this session

- `python3 -m unittest discover -s scripts -p 'package_wasm_site_test.py' -v`: 12 passed.
- Recovered Unity mesh export: 45,356 vertices, 100,998 indices, 33,666 triangles; source buffer and index checks passed.
- `node --test scripts/game_loading_test.mjs` on the generated binary: 2 passed, including every triangle-corner position, normal, and UV check.
- The exact code files on PR head `449bb9d` match the locally tested files.

## Open gates

- This workspace has no `dotnet` executable (`dotnet --list-sdks` returned command-not-found). No build, publish, full Site package, or live smoke was run.
- Do not change the repository's .NET SDK pin; the expected version is 8.0.426.
- The full two-actor lifecycle remains a separate failed gate recorded in the prior handoff evidence.
- Existing Sites project: `appgprj_6acfef7174888191bc2bc2f269f60acc`, version 26, source SHA `64d9c812ab01e23f37ac6c0b0b3137b71130e62d`. No new Site version was saved or deployed.

## Next action

Obtain .NET SDK 8.0.426 in an available environment, then run the fresh Release build/publish and package assembly at the live PR #68 head. Verify the package manifest and the generated CRG bytes. Run the remaining automated game scene/input checks and two-actor gate. Merge only after the relevant local gates pass. Then publish the verified package to the existing Sites project and verify its served scene assets and WASM. GitHub Actions remains disabled.
