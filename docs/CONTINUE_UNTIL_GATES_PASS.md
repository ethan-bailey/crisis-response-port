# Persistent Crisis Response Port continuation policy

This is the repeatable engineering loop for `ethan-bailey/crisis-response-port` on `develop/wasm`. Read this file, `NEXT_AGENT.md`, and the canonical [gate dependency index](GATE_DEPENDENCY_INDEX.md) at the start of each run. `GATE_DEPENDENCY_INDEX.md` is the only gate list; do not reproduce numbered gate lists in handoffs or other documents.

## Start with the live source

1. Use the protected project checkout when present, even when the task's working directory differs. Verify repository origin, branch, local SHA/tree, live GitHub SHA/tree, worktree status, every registered worktree, and all staged/unstaged/untracked changes before editing. Preserve owner changes; do not reset, clean, overwrite, or abandon them.
2. Use the pinned .NET SDK **8.0.426** by sourcing `scripts/env/dotnet-8.0.426-preflight.sh` before .NET build/test/restore/publish commands. Confirm the browser test runner and a usable push route. Prefer the authorized GitHub connector with expected-HEAD compare-and-swap when shell Git authentication is unavailable. Avoid Codespaces while the owner's allowance is constrained. If one access or runner path fails, make one bounded alternate attempt, record the exact result, defer only dependent acceptance, and continue independent safe work.
3. Verify the current hosted CI and deployment state from the exact live source SHA. Do not infer status from stale `origin/develop/wasm` refs or from a locally cached browser package.

## Close known dependencies before implementing

Work the earliest dependency-ready gate in the index. Before changing code, inventory its original serialized assets and GUID/fileID references, source owners and components, callers/callees, initialization/update/fixed-update/teardown order, engine services, browser adapters, failure paths, and precise acceptance assertions. Use recovered original behavior and authored data as authority. Do not invent substitutes, synthetic gameplay, or unsupported asset mappings. Runtime failures can reveal hidden dependencies; record each newly discovered source edge in the owning subsystem record before continuing that batch.

## Batch, verify, and push

After each coherent source batch, run focused regressions that fail before the fix, relevant full verification, exact package-integrity checks, and hosted packaged-browser acceptance where applicable. Tie every result to the tested source SHA, runtime path, SDK/browser version, command, and output. Keep passed, failed, deferred, and untested results distinct. A pending CI job, device-only check, unavailable artifact, or open gate defers only its dependent acceptance; continue other ready work. Inspect the exact failed step/log, make one focused repair, rerun it on the next pushed SHA, and do not repeat a blocked recovery attempt without new evidence.

Commit one logical change at a time, grouping all files belonging to that change. Push directly to `develop/wasm` with an expected-HEAD fast-forward update; after push, confirm the live commit and tree. Do not write to `main`, deploy, or publish from this recurring engineering task. Publication requires a separate release request after the exact-source build and package-integrity checks pass, and must preserve the existing Site access policy.

## Keep the records useful

Update `NEXT_AGENT.md` exactly once at session end, folded into the session's final commit. Keep it limited to the current source/head facts, verified checkpoint, external blockers, and a link to the canonical gate index. Do not duplicate the gate list or keep chronological history in the handoff. Do not create dated status/checkpoint/audit/closure documents; amend the existing subsystem record. For archive work, move obsolete snapshots to `docs/history/` and update links in the same logical commit. Never create a documentation-only correction commit for a record error; correct it in the next session-end commit.

Before ending a run, reread this policy and the updated handoff, ensure each attempted action has an exact result, and state the next dependency-ready action. Do not claim overall parity while any feasible gate remains. Execution ends at the runtime's boundary; do not promise work outside that window.
