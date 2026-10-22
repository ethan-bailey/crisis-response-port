# Branches, pull requests, labels and release workflow

> **Current active branch/PR/preview audit:** [REPOSITORY_OPERATIONS_2026-10-17.md](REPOSITORY_OPERATIONS_2026-10-17.md). The branch creation facts and legacy transition below remain historical; do not infer a current SHA, open PR, or deployment from them.

**Effective October 16, 2026.** This is the authoritative, contributor-facing collaboration policy for the **private** `ethan-bailey/crisis-response-port` repository. For live code/test status read `NEXT_AGENT.md` on `develop/wasm`.

## Branch roles

| Name | Responsibility | Integration rule |
| --- | --- | --- |
| `main` | Stable **repository** baseline; incomplete game, not a full release | Deliberately promote a reviewed/tested release only; governance docs may be updated separately. |
| `develop/wasm` | Canonical integrated source-preserving C#/.NET 8 WASM game and browser adapters | All normal gameplay feature PRs target this branch. |
| `feature/<issue>-<short-name>` | One bounded original gameplay, animation or renderer dependency | Start from current `develop/wasm`, review, test, merge, retire. |
| `fix/<issue>-<short-name>` | Focused regression fix against original behavior | Start from `develop/wasm` with a failing test. |
| `research/<topic>` | Original asset, source graph, shader or parity evidence | Keep non-gameplay experiments separate until reviewed. |
| `experiment/<topic>` | Isolated native Unity/engine/platform feasibility | No automatic engine switch or behavior rewrites. |
| `release/<version>` | Optional immutable release candidate from reviewed integrated SHA | Protected, deliberate iPhone testing; promotion only with evidence. |

**Starting code:** `develop/wasm` was created at original PR #9 branch commit `2e23cd20813f6bf3e4d2077146edb8daebb06ed4`, containing its gameplay history and the integrated arm-sequence batch from former PR #21. Latest matching compiled/browser-tested *gameplay* source at restructuring is `1264c0b4579076b82e1b6b21955469313779fe39`, [Actions #38901664939](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38901664939), private site artifact `12046137813` and evidence `12046012744`. Governance commits after that are not a new test run. Controller animation playback, other original systems and physical iPhone parity are **not** complete.

## Single-agent policy (owner, effective 2026-10-22)

Work in this repository is done by one agent at a time, so branching is organised as a single line of development rather than parallel workstreams:

- There is **one active feature workstream at a time**. Additional scoped work is recorded as issues or in `NEXT_AGENT.md`, not started as parallel branches.
- Feature branches are **short-lived**: branch from current `develop/wasm`, land one reviewable slice, merge, retire.
- A branch that must live beyond one session **merges current `develop/wasm` into itself at the start of each session**, resolving overlaps while they are small - the repeated PR #42 merge probes of October 2026 are the failure mode this rule exists to prevent.
- An externally blocked merge gate (for example exhausted CI quota) does not justify stockpiling work on a long-lived branch: verified slices stay small and mergeable, with local verification as the temporary gate, and the blockage is recorded in the handoff.

## Legacy transition

- **PRs #8 and #9** contain the historical gameplay commits now reachable on `develop/wasm`; preserve those source branches and Git history as rollback/provenance references. Close them as **superseded by an equivalent new integration branch**, not falsely marked as GitHub-merged.
- **PR #21** original animation composition was source-integrated at `c110c738646f48f793a1e35822639676b0b21975`, and combined-tree tests passed [#38901664939](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38901664939). The PR was previously closed, not GitHub-merged.
- **PR #19** reproducible packaging and **PR #20** original source/UI audit now target `develop/wasm`. Review against already imported files; avoid bulk-merging duplicate scripts.
- **PR #7** remains a separate Unity feasibility effort. Do not merge Unity into the original-code WASM runtime.
- Never delete original evidence, force-reset shared history or change repository visibility.

## Standard contribution cycle

1. Choose an issue and record its scope, status and upstream original-file dependencies. **Check the actual current `develop/wasm` SHA and in-progress Actions**; don't act on an old handoff.
2. Branch from current integration. PR base = `develop/wasm`. One reviewable feature or bug fix per PR. Include source archive checksum, original developer paths and SHA/GUID/fileID, executable methods, engine-adapter changes, tests, known gaps and revert path using the PR template.
3. All original C# files and recovered assets remain unchanged unless a provably necessary source-specific compatibility patch is documented. Browser/Unity API compatibility adapters must not replace gameplay, invent animations, skip damage calculations or synthesize shaders.
4. PR-level cheap source/Node/package checks run for code changes. Reuse full .NET 8 WASM/PhysX/Chromium/WebKit **only** at significant integrated milestones. Do not rerun heavy builds for documentation changes or start duplicate runs.
5. Resolve collisions on the feature branch; never force-push/reset `develop/wasm`. Prefer conventional reviewed merge commits; exceptions require provenance in the commit/PR body. Update concise `NEXT_AGENT.md` and detailed `docs/WORKSPACE_STATUS.md` on the integration branch.
6. **Vercel Git deploys stay disabled** (`vercel.json` has `git.deploymentEnabled:false`). When a matching SHA-tested package is suitable for *real* iPhone testing, deploy intentionally to an owner-protected URL and record URL, artifact hash, access gate, real Safari evidence and rollback. Branch aliases update only on actual deploy.
7. A tested release promotion into `main` is a separate deliberate PR. **Do not call this an original-game release without completing the full acceptance gates.**

## Label vocabulary

Choose one type, at least one area, and priority when useful. Status may be absent on active work.

| Family | Approved labels |
| --- | --- |
| Type | `type:feature`, `type:research`, `type:maintenance` |
| Area | `area:wasm`, `area:animation`, `area:gameplay`, `area:assets`, `area:graphics`, `area:ui`, `area:ci-release`, `area:ios`, `area:unity` |
| Priority | `priority:P1` next critical path, `priority:P2` important later, `priority:P3` optional |
| Status | `status:blocked`, `status:reference`, `status:integrated`, `status:superseded` |

Labels represent observable status, not aspirational completeness. Historical integrations can have `status:integrated` without native shader/gameplay parity. Existing GitHub default labels may coexist.

## Server-side branch protections

**Not yet enforced:** GitHub currently reports `protected=false` for `main` and `develop/wasm`. These collaboration rules are documented and followed by agents, but they are **not a server-enforced required-review policy** yet. A repository administrator may additionally configure **Settings → Rules → Rulesets / Branch protection** (subject to the account plan) to target `main` and `develop/wasm`, prohibit deletion/force pushes, require pull requests and reviews, and require the exact fast source-check job when its status is available. Do not turn on an unavailable/mismatched required check and inadvertently block emergency rollback/release operations. Admin-only policy changes have **not** been made in this session.

## Required documentation

- **Start here:** `README.md` and `CONTRIBUTING.md`.
- **Current one-screen assignment/handoff:** `NEXT_AGENT.md` **on `develop/wasm`**.
- **Project board / overall goals:** `WORKSPACE.md` and GitHub issues.
- **Long-form source/test evidence:** `docs/WORKSPACE_STATUS.md`, `docs/COMPLETE_WASM_PORT_ROADMAP.md`, source-specific reports.
- **Releases and device testing:** `docs/VERCEL_MANUAL_DEPLOYMENTS.md`.

Do not create a competing authoritative "next agent" file per feature branch. Tests and source provenance outrank documentation assertions. The source archive and licensed game assets remain private.
