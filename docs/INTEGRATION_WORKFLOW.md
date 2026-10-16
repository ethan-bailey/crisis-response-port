# Fast integration protocol — original C# / WASM port

The user prioritizes **getting the original game playable**, not accumulating detached audits, duplicate CI runs, or early deploys. The current code integration home is the private branch `codex/projectile-material-impacts`, PR #9. This is not `main`. Read `NEXT_AGENT.md` for the latest game blockers and `docs/WORKSPACE_STATUS.md` for the historical evidence ledger.

## Keep one active executable integration branch

1. **Start on the latest verified PR #9 head.** Query the live PR head and running Actions; do not trust historical branch SHAs or previous chat notes. Do not edit PR #7 (independent Unity route) or `main`, and do not change Vercel's root `git.deploymentEnabled: false`.
2. **One feature, one bounded source dependency group.** Prefer to work directly in short-lived branches created from the CURRENT integration HEAD. New feature PRs must target PR #9, must have a short list of original developer C# classes/assets and GUID/SHA evidence, and must specify expected executable behavior. Avoid new permanent parallel versions of the same WASM game.
3. **Source preservation is a gate:** Copy unchanged recovered original C# and original assets with verifiable provenance. Only modify existing browser/engine adapters for required Unity API, physics, lifecycle or rendering semantics. Do not add placeholder gameplay, guessed shaders, fake callbacks or unconditional pass assertions.
4. **Integrate after review, rather than endlessly stacking PRs.** Compare to the **current** PR #9 base, preserve code already on that branch, and transfer only reviewed source files/adapter changes. Prefer one SHA-checked atomic tree commit so importing a batch doesn't trigger repeated Actions. Keep a merge/revert mapping to the source PR commit and original recovery SHA. The integration branch is the canonical executable snapshot; old source PRs become reference-only or close as integrated **after** the combined tree passes.
5. **One validation sequence per meaningful gameplay batch:** cheap source/hashes/native contracts first; one matching .NET 8 WASM build + pinned PhysX + Chromium/WebKit browser execution only after the integrated code changes are complete. Existing workflows are path-scoped/manual so ordinary pushes do not launch expensive CI. Do not duplicate an already active run. Source-only inventory or docs never require a complete browser rebuild. Maintain actual evidence artifact SHA and scope: composition ≠ animation playback, source event ≠ particle simulation, WebKit CI ≠ physical iPhone.
6. **Manual deployment only:** if the matching candidate passes the tested milestone and needs actual iPhone feedback, release intentionally to protected Vercel and verify deployed content/URL against tested code. Do not re-enable automatic deploys or overwrite a known-working private device build just to display progress.

## Workstream consolidation decisions

- **PR #9 — canonical WASM game integration:** original Mission 01 slice, movement, materials, weapons, glass, source effect bindings, and now recovered original arm-sequence batch from PR #21.
- **PR #21 — original arm-sequence source batch:** Source commit `256b574eca62a902e23e9ba45d2b2542d64fe445`, tested in [Actions #38896212172](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38896212172). The 18 added files plus two adapter entry-point modifications were selectively and atomically copied to the current PR #9 at commit `c110c738646f48f793a1e35822639676b0b21975`, retaining the already committed Vercel `vercel.json` policy. **This confirms content integration, not yet the combined-tree test.** Integration verification: [run #38901664939](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38901664939), exact source `1264c0b4579076b82e1b6b21955469313779fe39`; wait for terminal run status before claiming green.
- **PR #19 — SDK8 site packaging tools:** remain reference/test evidence. PR #21 already brought verified package functions to PR #9. Evaluate a specific packaging difference against the current mainline before transferring code; don't merge the entire branch to get duplicate packaging scripts.
- **PR #20 — original scene/source/UI audit:** provenance reference, not working UI code. Import only audit data/modules actually required to implement the next game subsystem.
- **PR #7 — independent Unity experiment:** no direct modifications or merge into C#/WASM development. The separate native browser experiment failed [Actions #38895140830](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38895140830); investigate separately only when native parity evidence is needed, not as a compilation prerequisite.
- **PR #8 — predecessor branch:** do not rewrite earlier weapon history or fast-forward `main` without authorization.

## Continue gameplay by executable dependency

1. **Done as bounded composition:** unchanged original `IKArmAction`, `IKArmAnimationFrame`, `IKArmSequence`, `IKArmSequenceRuntime`, `IKArmTarget`, `IKControllerInvoke`; seven original prefabs / 21 frames.
2. **Next source group:** inspect `IKArmSequencer`, `IKArmController`, `ArmIKSolver`, source weapon/CharacterBody callback graph, and serialization. Import only an executable subset after verifying Unity APIs and initialization dependencies. **Do not call composition 'animation playback'.**
3. **Playable progression:** original arm transitions + reloading and weapon actions, character states and movement, combat/AI, mission objectives and transitions, UI/save/audio, then the remaining source-verified shaders/particles.
4. **Complete-game acceptance:** all required original scenes, source gameplay, physics, behavior parity, original presentation, browser lifecycle, physical iPhone and protected release. Use `docs/COMPLETE_WASM_PORT_ROADMAP.md` for the full acceptance gate list.

## Minimal per-batch handoff

Use one short block at the TOP of `NEXT_AGENT.md`: exact current branch SHA and tested SHA, imported source classes and source SHA256, CI run/outcome/artifact, the exact first failing step if blocked, one next bounded source dependency, and selective rollback commit(s). Put detailed transcripts only in `docs/WORKSPACE_STATUS.md`. Don't create new competing "next task" documents for every commit.

**Rollback this integration batch:** compare later work, then create a selective revert of `c110c738646f48f793a1e35822639676b0b21975` and its workflow-only gate `1264c0b4579076b82e1b6b21955469313779fe39`. Never force-reset the branch and never revert unrelated original ballistics, input fixes or the Vercel opt-out.
