# Next agent — active original-game WASM development handoff

**Updated 2026-10-16.** This is the **sole current execution handoff**. [Start with the contributor guide](CONTRIBUTING.md), [branching policy](docs/BRANCHING_AND_COLLABORATION.md) and [project workspace](WORKSPACE.md). The previous stacked-PR instructions are preserved as [historical evidence](docs/history/NEXT_AGENT_BEFORE_DEVELOP_WASM_REORGANIZATION_2026-10-16.md), not live work orders.

## Branch ownership

- **`main`: stable repository baseline.** Not the current full game, not the destination for normal gameplay commits.
- **`develop/wasm`: canonical integrated executable original C#/.NET 8 WASM + pinned PhysX + browser game.** All new normal gameplay feature/fix PRs target here. Recheck its HEAD and in-progress Actions before coding. Avoid force pushes, direct history rewrites and wholesale branch replacement.
- **`feature/<issue>-<topic>`: short-lived contributor branch from the latest integration HEAD.** The first private work branch, [`feature/22-arm-controller`](https://github.com/ethan-bailey/crisis-response-port/tree/feature/22-arm-controller), is reserved for [issue #22](https://github.com/ethan-bailey/crisis-response-port/issues/22). Recheck/update its HEAD against `develop/wasm` before code changes; no original controller playback is implemented yet.
- **`research/*`, `experiment/*`: isolated provenance and engine comparisons.** The separate Unity path [PR #7](https://github.com/ethan-bailey/crisis-response-port/pull/7) is not a WASM prerequisite.
- Historic PRs #8 and #9's game/weapon/ballistics code is incorporated into `develop/wasm` as actual Git history. Original arm-sequence source batch #21 was copied and tested as documented below. PR #19 (tooling) and #20 (source/UI evidence) now target `develop/wasm` for selective review; avoid duplicate packaging tooling.

**Privacy / deployment:** This is a private recovery project. Original developer systems/assets must not be redistributed or replaced with synthesized gameplay. Vercel auto Git deployment is disabled by root `vercel.json` (`git.deploymentEnabled=false`), including on `main` and `develop/wasm`. GitHub CI validates source changes; only deploy an exact tested, protected package **manually** for physical iPhone Safari tests. See [manual release guide](docs/VERCEL_MANUAL_DEPLOYMENTS.md).

## Latest proven runnable slice

- Combined-tree selected original C# + .NET 8 WASM compiler/package + pinned PhysX + Chromium/WebKit original arm-sequence composition and existing input/source tests: **SUCCESS** [GitHub Actions #38901664939](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38901664939), **exact gameplay/test source SHA `1264c0b4579076b82e1b6b21955469313779fe39`**. Private full WASM site artifact `12046137813`, private evidence `12046012744` (expiry November 15, 2026). Source batch from closed PR #21 integrated at `c110c738646f48f793a1e35822639676b0b21975`. **Governance commits after this checkpoint are NOT a newly tested game.**
- Partial browser gameplay: original source movement, limited weapon/ballistic system, input, rendering and source glass; headless Chromium/WebKit tests exist. Complete character animation playback, full AI, physiology, mission completion, all six scenes, UI, source shader/material/particle behavior and hardware Safari validation are **not** finished.
- Original archive recovered privately: `recovery-preservation-2026-10-15`, SHA256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. Verify unchanged source SHA/GUID/fileID with each new import. **Never write stand-in controllers, fake callbacks, approximate original shaders, or overwrite `runtime/Original/` with invented game rules.**
- Source and package data beyond the verified slice: [PR #19](https://github.com/ethan-bailey/crisis-response-port/pull/19) and [PR #20](https://github.com/ethan-bailey/crisis-response-port/pull/20) contain useful tested tools/audits, not missing merged game systems. Full history and rollback: [workspace ledger](docs/WORKSPACE_STATUS.md), [complete-game gates](docs/COMPLETE_WASM_PORT_ROADMAP.md).
- **Do not claim a completion percentage or 1:1 parity.** Original ShaderForge exports include dummy shader files, native Unity ParticleSystem rendering is incomplete, and physical iPhone test is outstanding.

## Exact next bounded source task: arm-controller playback

[Issue #22 — original arm controller and IK source closure](https://github.com/ethan-bailey/crisis-response-port/issues/22)

1. Inspect current `develop/wasm`, any feature branch/PR touching `runtime/Engine/` or `runtime/Original/Animation/`, and CI status. Avoid modifying unrelated active work.
2. Recover **actual original** `IKArmSequencer`, `IKArmController`, `ArmIKSolver`, `CharacterBody` and weapon/limb callback dependencies from the pinned archive. Hash each original source and map required Unity API types/serialized prefab references. Existing six arm-sequence C# classes, seven original prefabs and 21 frames have already been imported and tested: **do not repeat or fabricate** them.
3. Find the smallest **executable** original-code slice for a representative Holster/Unholster or Raise/Lower original sequence with real timing, arm target state, and real callback. If mandatory native dependency is missing, record exact blocker; move only to an independent source-backed module rather than inserting no-op callbacks.
4. Add source-preserving Unity engine adapter behavior only as necessary, real native unit and browser tests, original-reference fixtures when available. Check hashes and cheap tests first.
5. After review, merge feature to `develop/wasm` and run **one** meaningfully scoped full .NET/PhysX/Chromium/WebKit CI check against that exact integration commit. No redundant full CI for documentation or source inventory; no automatic Vercel release.
6. Update this short handoff with exact code+tested SHA, GitHub Actions run and artifact IDs, remaining blockers and the selective rollback. Put long technical evidence in `docs/WORKSPACE_STATUS.md` and linked issue.

## Remaining program in dependency order

Original arm/character and weapons → original AI, inventory, doors, physics/physiology → objective/mission/scene flow → original UI/persistence/audio and shader/particle visual fidelity → deterministic original Unity parity comparisons → tested complete-game private package → actual iPhone Safari device acceptance and controlled release. The detailed eight gates are in `docs/COMPLETE_WASM_PORT_ROADMAP.md`.

## Rollback / parallel-work safety

Shared integration branches are append-only. Revert only the affected reviewed commit(s) with new commits after examining downstream dependencies. Preserve the private original archive and earlier CI artifacts; never force reset, change repository visibility or claim WebKit CI is physical iPhone Safari. Each PR must list explicit original source SHA/GUID, input SHA, verification evidence, gaps and rollback files.

**Session boundary:** Progress is made during an active agent/Work session only; do not imply code runs automatically after the session ends.
