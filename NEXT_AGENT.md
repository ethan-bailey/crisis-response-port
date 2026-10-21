# NEXT AGENT — Crisis Response WASM port

Updated 2026-10-21 after the owner-approved functional-substitute policy was recorded.

## Live state and finish line

- Repository: ethan-bailey/crisis-response-port. PR #42 remains open, draft, unmerged, and not ready for merge. It targets develop/wasm.
- The feature branch was refreshed from origin and observed at 030b9c384c2a7fec632baeb4653d68012d0707 after the policy commit. The current develop/wasm head was 2fc0a8e93be97c4d99123910fad4bd25798e71a8. Do not integrate develop until the gameplay and browser acceptance gates pass.
- The preserved Codespace checkout is at 17b00cecdbea65c98af6705a4ab7e389b9b7c7c7 and has one pre-existing untracked file: runtime/Browser/wwwroot/browser-input.mjs (+197 lines). It was not modified, staged, or deleted. Work for this continuation used a separate worktree at /tmp/pr42-current-ec323.
- The finish line remains: source-backed or verified behavior-equivalent dependencies, original actor and controller execution, independent two-actor gameplay, exact-source WASM gameplay, Chromium/WebKit and HTTP acceptance, then required CI. Keep the PR draft/open until those gates pass.

## Landed in this continuation

Commit 030b9c384c2a7fec632baeb4653d68012d0707 updates AGENT_INSTRUCTIONS.md and source-reference/Compatibility/Projectile-fragments-material-replacement.json.

The owner permits scoped substitutes only when tests demonstrate identical in-game behavior for the affected feature. The policy requires preserving the original pointer and provenance, recording the replacement's own identity, limiting its scope, and never claiming asset identity. If equivalence cannot be exercised or proven, the gate remains open.

The Projectile chunks material is still only a candidate. Its referenced exported shader has a dummy body, and equivalent rendering/game behavior has not been demonstrated. No runtime mapping or prefab edit was made. Do not wire this candidate in on the strength of the owner's conditional approval alone.

Validation for the documentation-only policy commit: Python JSON parsing passed for the compatibility record; git diff --check passed. No runtime code changed in that commit.

## Latest relevant runtime evidence

The consolidated Codespaces scan recorded for code head 17b00ce ran after the runtime implementation batch; later head ec323a4 changed only NEXT_AGENT.md. Thus the runtime scan applies to the unchanged runtime files at the refreshed PR head. Its relevant results:

- EngineChecks: 69 assertions passed.
- FoundationChecks: 120 assertions passed; this is not full-game parity.
- Runtime checks project built successfully.
- The runtime contract passed prefab-reference/clone checks and the exact Calibre identity check, then failed closed on Unity built-in Material 10303 in Projectile fragments.prefab.
- Original controller audit reports controllerPlaybackImplemented=false.
- No complete Operator/Occupant lifecycle, independent two-actor gameplay, final WASM gameplay, Chromium/WebKit gameplay, or final HTTP smoke has passed.
- No GitHub Actions status checks or workflow runs were available for the scanned head; no workflow was dispatched.

The full gate table and scan evidence remain in docs/evidence/logs/triage-2026-10-20-pr42.md. Ammunition closure provenance and the owner-approved .308 casing mapping are in docs/evidence/REA-ammunition-closure-2026-10-20.md. The .308 casing replacement is scoped to the casing root and is an approximation, not identity recovery. The mixer GUID false positives were already fixed by parsing serialized PPtrs only.

## Next work

1. Continue the Material 10303 gate. Search for the matching player resource record or build a narrowly scoped compatibility path using a candidate that can pass the affected feature's equivalence tests. Preserve the prefab pointer and provenance. The current dummy-shader candidate does not pass.
2. In parallel with missing asset resolution, proceed through source-backed controller playback and original Operator/Occupant lifecycle/ownership work. Keep the one authoritative FullSource.CharacterBody graph; do not add parallel transport gameplay owners.
3. After source, lifecycle, and gameplay gates pass, build and test one exact-source WASM package in Chromium and WebKit with a local HTTP smoke. Integrate the then-current develop/wasm with a normal merge commit immediately before final exact-SHA verification.

Do not merge, rebase, deploy, or alter the preview before the acceptance gates pass. Do not touch or discard the preserved untracked browser-input.mjs or existing stashes. Read AGENT_INSTRUCTIONS.md and the consolidated triage before the next implementation batch.
