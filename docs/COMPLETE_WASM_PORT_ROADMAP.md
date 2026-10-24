# Complete WASM Port Roadmap

## Current authority

Follow AGENT_INSTRUCTIONS.md, AGENTS.md, and NEXT_AGENT.md for branch, environment, and evidence rules. develop/wasm is the sole development line. Historical PR #42, Codespaces-only, and old branch checkpoints are superseded. GitHub Actions remains enabled as documented by AGENT_INSTRUCTIONS.md; use configured workflows where appropriate. Use the normal Work Mode checkout; use an existing Codespace only if a required gate cannot run locally. Do not deploy intermediate work.

GAMEPLAY_COMPLETION_ROADMAP.md owns the ordered behavioral gates. This document tracks the port's source-to-WASM path and the validation needed before publication.

## Source-to-runtime path

1. Preserve original evidence. Keep recovered Unity scenes, prefabs, scripts, metadata, GUIDs, fileIDs, built-ins, Resources, and transitions attributable to the original project. Audit all six scenes and close every dependency. Do not replace unresolved assets with guesses.
2. Build the Unity compatibility surface from original usage: lifecycle and object identity/destruction; prefab remapping; transforms; time, coroutines, and events; physics and collision ordering; animation; audio; UI; Resources; and scene management.
3. Compile the selected original C# dependency graph with pinned .NET SDK 8.0.426, then compile and package the browser/WASM runtime. State which graph was compiled; a successful selected-runtime build is not proof that all recovered game sources compile.
4. Execute original actors, controls, IK, physical constraints, combat, physiology, mission logic, UI, audio, effects, saves, and mobile lifecycle as listed in the gameplay roadmap. Actor checks must use authored Operator/Occupant prefabs, all 19 limbs, real Mission/global dependencies, and two independently exercised actors.
5. Capture original behavior as traces from native references. Compare the port with justified numeric tolerances for each required scene and system; explain any differences.
6. At each major playable milestone and before release, run the exact-SHA local WASM build, HTTP smoke, source/Node checks, and package integrity checks. Have the owner test the exact build in iPhone 16 Safari for full mission play, performance, audio, and background resume; record the build/source SHA and device results. Desktop Chromium and WebKit gameplay runs are optional diagnostics and do not block acceptance.
7. Only after all gates pass on one SHA and publication is authorized, publish the existing owner-private Crisis Response Runtime Site. Preserve its access policy and verify the deployed build matches that exact SHA.

## Evidence at 2026-10-23

Current branch: develop/wasm. Current source checkpoint before this documentation update: a75983409c8aeeab76f53afeab5c0e7a41c428c8. SDK: 8.0.426.

- dotnet build runtime/Crisis.Runtime.csproj --no-incremental passed: 0 errors, 45 warnings. It compiles Engine/**/*.cs and Original/**/*.cs in the runtime project, not the full recovered Unity project.
- dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -c Release passed its contracts. The report still marks fullGameParity=false and fullUnityParity=false, with two unresolved built-ins. It does not establish scene playback or playable parity.
- scripts/scene_dependency_audit.py over the available curated source-reference/Original root found 4 assets, 3 edges, and 123 unresolved GUIDs. That root is not the complete six-scene project. The .308 casing mesh GUID b4b69dc6c74eeb54cbd22d28e37f9a19 / fileID 4300000 remains unresolved; no verified substitute is available.
- No complete scene package was available to run the scene-flow check. Rendered Marksman play and in-scene FIRE behavior remain unverified. Real Operator/Occupant lifecycle with Mission/global dependencies remains unverified.
- Current live PR state at the start of this batch: no open PRs. No Site save or deployment occurred.

## Next step

Start at the earliest unfinished gate: obtain original evidence for the missing casing mesh and built-ins and the complete six-scene source package, then run a full dependency audit and capture native scene-flow behavior. If that evidence remains unavailable, document the precise missing source input and continue with an independent source-backed gate. Do not claim closure from this partial curated source bundle, inventory, or compilation.
