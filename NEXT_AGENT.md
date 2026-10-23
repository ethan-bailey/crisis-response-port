# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; current HEAD `76760e3c3c3e4ad09786849aa588e2fa699bdc39`, confirmed equal to `origin/develop/wasm`. Code/test source SHA remains `2530764705352f98c3c42018b8aa2238ce561842`; later commits only record evidence and handoff.
- No source edits were made during this continuation.
- Preserve the known untracked corrupt package fragment `scene-data/.crisis-response-scenes.json.gz.QEOasz`; it is excluded from commits.
- GitHub Actions remain disabled. Codespaces use is at 90% through 2026-11-01; avoid Codespaces unless no practical alternative exists.
- Read `AGENT_INSTRUCTIONS.md` and `docs/DOTNET_BUILD_RUNBOOK.md` before project work. Recurring .NET runner failures and fixes are recorded there.
- Detailed Gate 8 checks and errors: `docs/evidence/GATE8_ACCEPTANCE_ATTEMPT_2026-10-23.md`.

## Gate 7

Gate 7 passed at the tested source checkpoint: Mission 01 reached `Mission.Start` and `BeginMission`, producing 2 original TeamOperators and 5 Occupants (8 CharacterBody instances). The headless gate uses silent audio and suppresses presentation-only callbacks; it does not establish visual/audio parity, full behavior-tree execution, browser gameplay, or iPhone Safari acceptance.

## Gate 8 status

Gate 8 is **not complete**. Current local evidence passes the mission/spawner startup and ownership contracts, weapon source contract, ballistics contract, engine/foundation checks, Release WASM publish, and package integrity. The complete combined two-actor gameplay flow has not been proven in the real browser runtime. The browser app itself still labels full gameplay incomplete.

The browser-package build at source SHA `2530764705352f98c3c42018b8aa2238ce561842` produced and verified 767 files, 199 Blazor boot resources, and 195 WASM modules. Local Chromium installation failed because Playwright's download endpoint returned an invalid 0 MiB archive; no local browser binary is installed. The owner explicitly approved transmitting the package to ChatGPT Cloud Chrome, but the cloud browser cannot reach the workspace's `127.0.0.1:8765` server (`ERR_CONNECTION_REFUSED`), both before and after restarting the server. No remote/public deployment or alternate file transfer was attempted.

Physical iPhone Safari acceptance remains unperformed; this workspace is not the owner's iPhone. Do not claim Gate 8 complete until browser gameplay and real-device Safari checks pass.

## Remaining dependency order

1. Provide a permitted, private route that Cloud Chrome can reach for the exact-SHA package, or restore a local browser runner. The package transmission itself is authorized; localhost routing is the current blocker. Then exercise the package, original scene startup, and input.
2. Close the combined two-actor flow in a normally initialized mission: original Operator and Occupant ownership, movement/animation, M4 equipment and firing, limb hit/injury response, physiology/state outcome, and weapon-drop/destruction. Existing isolated contracts do not establish this complete linked sequence.
3. Fix the strict recovered-scene import failure for `Assets/Scenes/Marksman range.unity` if that scene remains in Gate 8 scope; the broad Checks run aborts on unresolved supported fields before finishing the scene loop. Record any source-faithful dispositions for missing/unsupported serialized assets.
4. Rebuild and package once from the final gameplay SHA, verify all manifest hashes, then rerun the exact-build browser smoke.
5. Perform actual iPhone Safari acceptance against that final verified build. The owner device test is required; desktop emulation is not equivalent.

## Known asset issue

GUID `b4b69dc6c74eeb54cbd22d28e37f9a19` is still absent from the recovered project metadata. Existing evidence identifies it on the `.308` casing root and Marksman-round prefabs; a separate projectile child mesh exists. The owner approved the available `.308` casing candidate as a functional substitute if verified, but no substitute was applied during this Gate 8 pass. This issue does not block the tested Mission 01 M4/5.56 startup path.

## Next action

Resolve the isolated-browser access path without publicly exposing the private runtime, then run the exact-SHA browser smoke. Gate 8 remains open pending the linked two-actor combat chain, browser gameplay, and owner-device iPhone Safari acceptance.
