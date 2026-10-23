# Crisis Response Port — current handoff

## Checkpoint

- Branch: `develop/wasm`; code/test source SHA is `2530764705352f98c3c42018b8aa2238ce561842`. This handoff/evidence update is a documentation-only descendant.
- The Gate 7 code checkpoint `72c167993280934d19c7921113eb9efe80849f89` and subsequent handoff update were present at session start.
- Preserve the known untracked corrupt package fragment `scene-data/.crisis-response-scenes.json.gz.QEOasz`; it is excluded from commits.
- GitHub Actions remain disabled. Codespaces use is at 90% through 2026-11-01; avoid Codespaces unless no practical alternative exists.
- Read `AGENT_INSTRUCTIONS.md` and `docs/DOTNET_BUILD_RUNBOOK.md` before project work. Recurring .NET runner failures and fixes are recorded there.
- Detailed Gate 8 attempt and error log: `docs/evidence/GATE8_ACCEPTANCE_ATTEMPT_2026-10-23.md`.

## Gate 7

Gate 7 passed at the tested source checkpoint: Mission 01 reached `Mission.Start` and `BeginMission`, producing 2 original TeamOperators and 5 Occupants (8 CharacterBody instances). The headless gate uses silent audio and suppresses presentation-only callbacks; it does not establish visual/audio parity, full behavior-tree execution, browser gameplay, or iPhone Safari acceptance.

## Gate 8 status

Gate 8 is **not complete**. Current local evidence passes the mission/spawner startup and ownership contracts, weapon source contract, ballistics contract, engine/foundation checks, Release WASM publish, and package integrity. The complete combined two-actor gameplay flow has not been proven in the real browser runtime. The browser app itself still labels full gameplay incomplete.

The browser-package build at source SHA `2530764705352f98c3c42018b8aa2238ce561842` produced and verified 767 files, 199 Blazor boot resources, and 195 WASM modules. Local Chromium installation failed because Playwright's download endpoint returned an invalid 0 MiB archive. A cloud-browser attempt to open the localhost-served private package was rejected by automatic review because it could transmit private project files to a remote browser service. Do not retry through another browser surface or indirect route. The owner must explicitly approve that specific disclosure before using the cloud browser.

Physical iPhone Safari acceptance remains unperformed; this workspace is not the owner's iPhone. Do not claim Gate 8 complete until browser gameplay and real-device Safari checks pass.

## Remaining dependency order

1. Obtain authorization for remote-browser access to the private package, or a materially safer local browser runner. Then exercise the exact-SHA published WASM package, including original scene startup and input.
2. Close the combined two-actor flow in a normally initialized mission: original Operator and Occupant ownership, movement/animation, M4 equipment and firing, limb hit/injury response, physiology/state outcome, and weapon-drop/destruction. Existing isolated contracts do not establish this complete linked sequence.
3. Fix the strict recovered-scene import failure for `Assets/Scenes/Marksman range.unity` if that scene remains in Gate 8 scope; the broad Checks run aborts on unresolved supported fields before finishing the scene loop. Record any source-faithful dispositions for missing/unsupported serialized assets.
4. Rebuild and package once from the final gameplay SHA, verify all manifest hashes, then rerun the exact-build browser smoke.
5. Perform actual iPhone Safari acceptance against that final verified build. The owner device test is required; desktop emulation is not equivalent.

## Known asset issue

GUID `b4b69dc6c74eeb54cbd22d28e37f9a19` is still absent from the recovered project metadata. Existing evidence identifies it on the `.308` casing root and Marksman-round prefabs; a separate projectile child mesh exists. The owner approved the available `.308` casing candidate as a functional substitute if verified, but no substitute was applied during this Gate 8 pass. This issue does not block the tested Mission 01 M4/5.56 startup path.

## Next action

Ask the owner for explicit approval to transmit the private packaged runtime to the remote cloud browser for Gate 8 browser testing. Continue independent two-actor integration work only after the required gameplay acceptance can be verified locally; do not publish or claim completion while the combined flow remains unverified.
