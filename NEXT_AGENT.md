# Crisis Response parity handoff

## Current checkpoint

- Development line: develop/wasm in ethan-bailey/crisis-response-port. Keep this as the single development branch and use normal fast-forward operations.
- Source SHA at the start of this verified batch: a75983409c8aeeab76f53afeab5c0e7a41c428c8. The batch rewrites the two stale roadmap documents and this handoff; check git log HEAD for the documentation commit SHA.
- At batch start the worktree was clean, the branch tracked origin/develop/wasm, and SDK 8.0.426 was selected.
- Live GitHub check at batch start found no open PRs. Keep Actions disabled. No Site version was saved or deployed.
- AGENT_INSTRUCTIONS.md and AGENTS.md govern priorities. The updated GAMEPLAY_COMPLETION_ROADMAP.md is the behavioral gate order; COMPLETE_WASM_PORT_ROADMAP.md describes the source-to-WASM evidence path. Historical PR #42, Codespaces-only, old checkpoint/branch, and Actions workflow directions are superseded.

## Verified evidence for source SHA a75983409c8aeeab76f53afeab5c0e7a41c428c8

- SDK: dotnet --version -> 8.0.426.
- dotnet build runtime/Crisis.Runtime.csproj --no-incremental -> passed, 0 errors and 45 warnings. This project includes Engine/**/*.cs and Original/**/*.cs; it is not the entire recovered original source tree.
- dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj -c Release -> returned PASS contract JSON. The report says fullGameParity=false, fullUnityParity=false, and unresolvedBuiltins=2. It does not prove original scene playback or playable parity.
- python3 scripts/scene_dependency_audit.py source-reference/Original /tmp/crisis-scene-audit.md -> 4 assets, 3 dependency edges, 123 unresolved GUIDs. This curated source-reference/Original directory is not the full six-scene Unity project.
- A search for a complete package/manifest under /tmp found none. The full source package needed by the scene-flow runner is unavailable in this Codespace.
- git diff --check passed for the roadmap refresh.

## Gates still open

1. Resolve original scene/asset dependencies for all six scenes. In particular, original mesh GUID b4b69dc6c74eeb54cbd22d28e37f9a19 / fileID 4300000 for the .308 casing and required built-ins remain unverified. Do not use a substitute without original evidence. Capture native scene flow.
2. Verify rendered Marksman gameplay and in-scene FIRE behavior; current contract checks do not establish either.
3. Prove two independently operating authored Operator/Occupant actors through full lifecycle with all 19 limbs and real Mission/global dependencies; prefab-only synthetic tests do not pass.
4. Complete remaining controls, IK/physics, combat, physiology, mission, presentation/platform, and original-vs-port trace/browser/mobile gates in the ordered roadmap.

## Next bounded batch

Start at the earliest unfinished gate. Seek the original .308 mesh metadata/content, required built-ins, and complete six-scene project evidence; run the dependency audit and capture native scene-flow behavior only when those inputs are present. If unavailable, record the precise missing evidence and advance one independent source-backed gate. Recheck branch/HEAD/worktree, selected SDK, and live PR state at the next run. Verify every change once at its exact SHA and replace this handoff once. Do not deploy until every required gate passes on a single SHA and owner publication instructions authorize it.
