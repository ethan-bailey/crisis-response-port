# Persistent continuation instructions

This file is part of the Crisis Response WASM port workflow. Read it before
ending every implementation run, including after a tool failure or context
change. The handoff in `NEXT_AGENT.md` is the current state; verify it against
the live `develop/wasm` branch before relying on its SHA or gate statuses.

## Keep each execution advancing

This work is a recurring engineering loop, not a one-batch task. During each active run, do not return after one fix, one passing CI job, a documentation change, a bounded batch, or because a gate remains open. After each batch, refresh the live branch/CI state and immediately select the next dependency-ready implementation, source-recovery, or verification batch. Target at least five independently verifiable non-documentation work items in a run when feasible. A running hosted job is not a reason to wait: use that time for another ready local/source task. After any failure, inspect its exact step/log, make one focused repair, push the repair, and rerun on that exact SHA while continuing independent work. End only at the active execution-window boundary or when no safe dependency-ready work remains; persist exact evidence and the next concrete action. Do not claim project completion or promise work outside the active run.

## Required closeout loop

1. Read `docs/GATE_DEPENDENCY_INDEX.md` and its linked system closure notes.
   List remaining parity gates in dependency order, verify the full known asset,
   script, component, lifecycle, and acceptance dependency set for the selected
   gate, and add any newly discovered dependency before implementation.
2. Pick the next feasible, dependency-ready gate or the next bounded vertical
   slice within it. Do implementation and acceptance work; a plan or an open
   gate is not a stopping condition.
3. If an exact external input is unavailable (for example, the user's iPhone,
   a private source artifact, or an authenticated runner), record the concrete
   missing input, attempted action, exact result, affected acceptance, and the
   next action in the existing evidence and handoff documents. Defer only that
   dependent acceptance and continue independent repository work.
4. Run focused checks for each slice, then the relevant complete verification
   suite when ready. Tie every result to the exact source SHA and actual runtime
   path; distinguish passed, failed, not run, and user-device acceptance.
5. Self-review provenance, ownership, behavior, and documentation. Commit and
   push each coherent verified batch to the authorized branch using an
   expected-HEAD update. Never rewrite shared history or publish a build
   without separate authorization.
6. Refresh `NEXT_AGENT.md` and the relevant REA evidence with the live branch,
   exact tested and remote SHAs, test commands/results, known failures,
   external dependencies, and one concrete next action.
7. Before returning, reread this file and the updated handoff, print the
   remaining gate list in the user-facing closeout, and continue all independent
   work available in the current run. Do not say a gate is complete without its
   acceptance evidence. Do not treat “still open,” “needs testing,” or a plan as
   a stopping reason.

An assistant cannot continue executing after a turn has ended. Persist the
unfinished work as exact next actions and resume this loop on the next run;
never promise autonomous execution outside an active run.

