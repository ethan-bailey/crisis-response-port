# Crisis Response Port — WorkMode handoff

## Current checkpoint

- Work directly in this persistent WorkMode checkout; do not restart Codespaces for routine edits.
- Active branch: `codex/game-scene-name-normalization-20261023`.
- Latest game-source commit packaged: `6e8148c1bb7f6b7eedab0231c2e5ea04ec188946` (`Refresh complete repository file index`). Site source commit: `d6768c2c9a211caa7a004c03e01e41b53254cfc9`.
- Site version 31 deployed successfully at `https://crisis-response-runtime.craftysnipe13.chatgpt.site`; deployment `appgdep_6adb753be4508191b0ef6cc4cf3f2b57`. The Site is owner-private. An unauthenticated browser sees “Sign in required”; the deployed game itself has not yet been manually checked in an authenticated session.
- The complete repository index is `docs/audit/repository-file-index.json`. Regenerate and check with `scripts/index_repository_files.py --site-manifest <Site dist/build-manifest.json>` after source changes. The index covers all tracked and non-ignored repository files and maps them to the latest package.
- No open PRs were present at the last GitHub check. Live `develop/wasm` was `9e234ba325d83fb1ba2f7ab38d7097e75e83fdbb`; it contains documentation/evidence commits not in this branch, with no code overlap. The current task snapshot is pushed to the active feature branch; verify its live head and PR list before any further sync. Keep at most one PR open and merge promptly when checks and gates pass.

## Latest Site changes and checks

- Restored owner-approved touch behavior from the known-good Site build: joystick vertical aim, horizontal movement with proportional travel, sprint only past the stick rim, touch guide, and iOS callout/context-menu protections.
- Fixed the animation adapter's movement predicate to use velocity. Position-delta detection falsely toggled idle on render frames between fixed PhysX steps, causing the walk/limp appearance.
- Kept the JS bridge adapter required because the deployed WASM lacks those JS-invokable methods. No raw recovered C# archive was added to the website.
- Site package contains 766 manifest-listed files from 892 source inputs. The remaining repository-only files are indexed but not copied into the browser runtime.
- Site test suite: 33/33 passed, including build integrity, WASM validation, touch input, source animation, occupant spawning, and scene package checks. Main-repository touch/animation tests and dependency inventory also passed.
- .NET SDK is unavailable in this checkout; the package reuses the compiled runtime from Site v30. No fresh C#/WASM compilation was run.
- Browser preview reached the owner-only sign-in gate. Ask the owner to sign into the Site and test joystick aiming/sprint, hold-to-fire, and walking on a touch device. Do not generate or rotate a SIWC bypass token unless explicitly requested.

## Known unrelated scene gate

Marksman scene loading still stops at missing Mesh GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`, fileID `4300000`, referenced by `Assets/Resources/weapons/ammunition/marksman/Marksman round .308 Winchester SP.prefab`. Existing evidence is in `docs/MISSING_CASING_MESH_GUID_SEARCH-2026-10-14.md` and `docs/evidence/logs/triage-2026-10-20-pr42.md`. Do not substitute the .308 casing mesh without verifying identical collider behavior. Find and verify the original round mesh or ask whether to defer the Marksman scene gate.

## Operating constraints

- Keep SDK pin `8.0.426`; a full build requires a compatible .NET SDK. Do not change project source to work around a host `/proc` issue.
- Preserve the game's original behavior and provenance. Add substitutes only where necessary and verified; avoid duplicate or less robust versions.
- Update this handoff once at the end of substantive work. Commit and push the active branch so work persists. Keep GitHub Actions disabled per owner decision.
