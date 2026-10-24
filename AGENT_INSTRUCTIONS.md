# Agent Working Instructions

Standing instructions for any agent working in this repository, on any branch or task. These rules describe a repeatable work loop. Follow them until the task reaches a clean end state.

## Single-line workflow (owner rule, 2026-10-22)

One agent works this repository at a time, so there is one line of development: `develop/wasm` is the project, and every branch is a short errand off it, not a second home.

- **One active workstream.** Only one feature branch is in progress at a time. New ideas and future work go on the written list (the handoff, or an issue), not into a new branch, until the active workstream lands or the owner explicitly parks it.
- **Land small, verified fixes directly on `develop/wasm`.** Do not create a branch or PR just to follow process. Use a short-lived branch and PR when review, isolation, or coordinated integration materially helps; target it at `develop/wasm`.
- **One open PR at a time; merge promptly.** Keep at most one active feature PR open. Merge it as soon as its own local build, relevant contracts, and provenance checks pass. A separate blocked acceptance gate does not hold a verified slice hostage. If work is parked or cannot be merged now, close its PR while preserving the branch; reopen it only when it is again the active, reviewable slice. Never bypass a failing gate or force a merge.
- **Short-lived branches.** Branch from the current `develop/wasm` head (record the base SHA in the handoff), do one reviewable slice, verify it, and merge it back - same session or same day where possible. A branch that keeps accumulating verified work without landing is a failure mode, not diligence: it is how PR #42 diverged from `develop/wasm` and ended up needing repeated merge probes.
- **Sync while you go.** If a branch must live beyond one session, the first work step of each following session - after the step 1 checkpoint - is to merge current `develop/wasm` into the branch and resolve the small overlaps then, while they are fresh. Never let a branch sit unmerged for days while `develop/wasm` also changes.
- **A blocked gate does not freeze landing.** When an external gate blocks a task, record it once with evidence and use local verification as the standing gate for mergeable slices; do not stockpile work while waiting for an unavailable service.

## Verification gate and GitHub Actions policy

The repository is public and GitHub Actions is enabled (verified 2026-10-24; see [`docs/REPOSITORY_VISIBILITY_AND_ACTIONS.md`](docs/REPOSITORY_VISIBILITY_AND_ACTIONS.md)). Use configured workflows when appropriate; do not disable Actions, while continuing required local verification: a fresh full-source build, relevant contracts, and provenance checks, recorded against the exact tested source SHA. Functional smoke requirements remain separate: successful compilation is not proof of browser boot or correct gameplay.

## Current execution environment: Codex Work Mode (owner direction, 2026-10-23)

The active working environment is the repository checkout provided to Codex Work Mode. It is independent of Codespaces, but it is **not** the owner's iPhone or personal computer. Use the checkout directly for repository reads, edits, local builds, and tests. Do not open or restart Codespaces for ordinary code work; use it only if the owner requests it or a required gate cannot run in the current workspace and Codespaces is the necessary available environment.

- The owner has used 90% of their Codespaces usage allowance through **2026-11-01**. Avoid Codespaces through that date whenever the task can be completed in the current checkout or through another already available tool. Use it only when a required gate cannot run here and no practical alternative is available.

- **Checkout persistence across sessions.** Treat `/workspace/scratch`, `/tmp`, and any path containing a generated workspace ID as ephemeral; a path recorded in chat or an older handoff is historical evidence, not proof that the checkout still exists. At the start of each session, inspect the current workspace and search the current scratch mount once. If no Crisis Response checkout exists, recover the live repository from authenticated GitHub `develop/wasm` at its current remote HEAD into the current workspace; do not use old ZIPs, Site build artifacts, or remembered SHAs as the working tree. Confirm the checked-out branch, HEAD SHA, and clean/dirty status against the live remote and handoff before editing. If shell Git authentication or checkout recovery fails, stop code changes and report the exact blocker; do not substitute connector-only source inspection for a buildable checkout.
- **Persistent checkpoints.** Before ending substantive work, commit and push verified changes to `develop/wasm`, then verify the remote branch points to the pushed SHA. A local commit, scratch checkout, browser session, or Sites archive alone is not a persistent project checkpoint. Record the exact branch and SHA in `NEXT_AGENT.md`; begin the next session by checking out that live SHA.
- **No archive shuttle.** Do not ask the owner to download, attach, or repeatedly transfer a full repository ZIP as a routine checkout workaround. Use the connected GitHub API for bounded file reads and edits/commits only when the required verification can run in an available environment. Connector-only inspection is not a build or test. If a complete checkout is required and no authenticated build runner is available, record that exact blocker and stop before claiming the change is verified.
- At session start, compare the current branch, HEAD, and worktree with `NEXT_AGENT.md`. Query live `develop/wasm` and open PRs through the GitHub connector when shell GitHub authentication is unavailable. If the recorded checkpoint and PR set are unchanged, do not repeat completed orientation or tests.
- The scratch checkout may not survive a new chat/session. Make repository work persistent by committing it and pushing the active branch to GitHub. Confirm the remote branch/PR head after the push. An uncommitted change is not a persistent checkpoint.
- If the current checkout is missing, follow [`docs/DOTNET_BUILD_RUNBOOK.md`](docs/DOTNET_BUILD_RUNBOOK.md#repeated-workspacecheckout-unavailability): search the current scratch mount once, use the GitHub connector only for live-state/file inspection when shell auth cannot prompt, and do not reuse stale archives as the live branch. If checkout or pinned SDK recovery fails, record the exact evidence, pause recurring work, and resume after the correct environment is restored.
- Use shell tools in this workspace for direct code work; a browser is for a genuinely browser-dependent gate, not for editing files or running routine repository commands.
- Keep the pinned .NET SDK at 8.0.426. If a fresh .NET invocation fails before compilation while reading process metadata or `/proc`, classify it as a runner/environment failure first. Do not change project source to compensate. Use a proven workspace-level workaround only if available, then record the exact invocation and result; do not spend repeated runs rediscovering the same environment failure.
- Before the first .NET command, follow [`docs/DOTNET_BUILD_RUNBOOK.md`](docs/DOTNET_BUILD_RUNBOOK.md): select the pinned SDK, apply the existing process shim when available, restore the exact project, and use the operation-specific build/publish command. Keep SDK startup, restore, compile, and optional workload warnings classified separately.
- Update `NEXT_AGENT.md` once at the end of a substantive session, replacing it entirely with the current branch/head, exact tested source SHA, checks actually run, outstanding blockers, owner decisions, and one exact next action. Keep historical detail in dated evidence files or Git history rather than accumulating stale handoff sections.

## The loop

Repeat these steps in order. Step 1 decides how much orientation a turn actually needs — do not redo work that reliable records show is already done.

1. **Checkpoint by head SHA, not by prompt.** The handoff (step 8) records the last checkpoint: branch, head SHA, open PRs, known external blockers, and the next item. At the start of any turn — a new prompt is not a new work session — make one lookup: the current branch head. If it matches the recorded SHA and the open-PR set is unchanged, orientation is already done: go straight to the recorded next item. Run the full orientation (read this file, any repo-level instructions such as AGENTS.md, the current branch/PR state, and the latest handoff or blocker list) only when the head has moved, the PR set has changed, a known external blocker's status has changed, or no checkpoint is recorded. A checkpoint is a comparison, not a deliverable: it never gets its own commit, and known external blockers are not re-checked unless something has changed.
2. **Define the finish line.** State — at least to yourself, in the handoff if one exists — what "done" means for this task: the deliverable, and what merge-ready or complete looks like.
3. **Scan once.** Run the broadest relevant checks, build, or discovery pass once, and collect EVERY failure, gap, or unknown into a single list before fixing anything. Do not discover blockers one at a time across a whole session.
4. **Triage the list.** Sort items into: (a) fixable now with material already in hand, (b) blocked on an external dependency, (c) needs an owner decision. Order fixable items by dependency and impact.
5. **Fix in batches.** Work the fixable items in order, committing actual code, asset, or configuration changes. Commit messages carry the findings — what changed, what it fixes, how it was verified. A session's value is measured in landed changes, not in time spent, commits counted, or documents produced.
6. **Bound one attempt per blocker.** If a fix attempt fails, record the blocker with its reason and evidence, propose a disposition (fix differently / accept as-is / defer), and move on to the next item. Never spend a session re-verifying the same blocker unless new information or material has appeared.
7. **Validate once after the batch.** Rerun the relevant checks once. New failures go back into the list and the loop continues at step 4. A failure identical to an already-dispositioned blocker is not new — stop on it and keep its disposition.
8. **Hand off once.** At the end of the session, update the handoff or status document exactly once: what landed, what remains blocked and why, what decisions the owner needs to make, and the checkpoint record for the next turn — branch, current head SHA, open PRs, known external blockers, and the exact next item.

## When to stop looping

Stop and report the end state when any of these is true:

- The task is done or merge-ready, apart from named external dependencies.
- Every remaining item is blocked externally or waiting on an owner decision, each with a proposed disposition.
- A full loop produced no actionable item — say so plainly in one commit or message and stop. Producing status documentation alone is not progress and is not a reason to keep going.

## Standing rules

- **Genuine material and verified substitutes.** Prefer real source data, original assets, and verified records. The owner permits a scoped substitute for missing material only when tests demonstrate identical in-game behavior for the affected feature against its original acceptance contract. Preserve the original pointer and provenance, record the substitute's own identity, and never claim source identity. If equivalence cannot be exercised or proven, leave the gate open and disposition it under step 6; do not present an unverified substitute as equivalent.
- **Evidence over claims.** Report only what was actually run, observed, or committed. Distinguish verified facts from assumptions.
- **Stay in scope.** Work only the assigned branch, PR, or task. Do not expand into unrelated work without an instruction to do so.
- **External blockers are stated once.** If progress depends on something outside the repo (quota, access, a service, the owner), name it once, clearly, and keep working on everything that does not depend on it.
- **Next turn starts from the handoff**, not from zero: compare the branch head to the recorded checkpoint SHA (step 1); if it matches, the first unfinished, actionable item on the list is the entry point.

## Resolve instruction conflicts

When project instructions conflict, resolve the conflict and continue instead of stopping at the first contradiction.

- Non-overridable platform, safety, legal, and approval requirements take precedence.
- Among project instructions, follow the latest explicit owner direction that applies to the current task. Treat older handoffs and stale status notes as historical when newer evidence or owner direction supersedes them.
- When equally current project rules still conflict, choose the narrowest reversible action that preserves source, data, and verification gates. Record the chosen rule, evidence, and effect in the handoff. Continue independent work while preserving any genuinely blocked decision.

## Owner-requested autonomous continuation

When the owner explicitly asks for continued work without further prompts, do not end the work sequence merely because one bounded gate is complete or blocked.

- Create or resume a recurring Codex automation no more often than hourly, starting from the current `NEXT_AGENT.md` checkpoint, and run its first cycle immediately.
- Each cycle takes stock of branch, HEAD, worktree, open PRs, current blockers, and the next actionable gate. It then completes one bounded batch of substantive work, verifies that batch once, and updates the handoff once.
- If a gate is externally blocked, record its exact evidence and disposition once, then continue with the next independent actionable item. Do not create repetitive status-only cycles.
- Keep GitHub Actions enabled. Follow each workflow's trigger and scope; do not dispatch unrelated workflows. Publish only when the owner has authorized the destination and the required gates pass. Do not use an unbounded shell loop or repeat a production deployment as a polling mechanism.
