# Agent Working Instructions

Standing instructions for any agent working in this repository, on any branch or task. These rules describe a repeatable work loop. Follow them until the task reaches a clean end state.

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
