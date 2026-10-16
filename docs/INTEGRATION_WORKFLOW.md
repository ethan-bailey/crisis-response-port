# WASM integration workflow — canonical policy

The repository was reorganized on October 16, 2026. This file previously documented using PR #9 as a permanent integration branch; that is **superseded**. The prior version remains available in Git history.

**Read the active [branch and collaboration policy](BRANCHING_AND_COLLABORATION.md), [contributor guide](../CONTRIBUTING.md), and [current next-agent handoff](../NEXT_AGENT.md).**

## Current integration process

`main` is the stable baseline; `develop/wasm` is the only current executable game integration branch. New bounded source-feature PRs start from the latest `develop/wasm`, target `develop/wasm`, preserve original C# and asset provenance, and use regular review/merge with clear rollback. CI source/package checks are light per code PR; full .NET 8 WASM + PhysX + Chromium/WebKit checks are explicit meaningful milestones; protected Vercel releases are manual. Independent Unity feasibility does not block C# WASM progress.

Historical PRs #8/#9 seeded `develop/wasm`. Original animation source batch #21 was imported into their game branch at commit `c110c738646f48f793a1e35822639676b0b21975`. The matching combined build test was verified on [Actions #38901664939](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38901664939) at SHA `1264c0b4579076b82e1b6b21955469313779fe39`; this does not establish actual original animation-controller playback or a complete game.

The next concrete source group is [issue #22](https://github.com/ethan-bailey/crisis-response-port/issues/22): original `IKArmSequencer` / `IKArmController` / `ArmIKSolver` source closure and executable real callbacks. Use the current `develop/wasm` head when starting.

Do not revive the old instruction to open stacked PRs targeting `codex/projectile-material-impacts` or to copy unrelated bulk work from PR #19/#20. Those branches remain historical/reference evidence, not the active code base.
