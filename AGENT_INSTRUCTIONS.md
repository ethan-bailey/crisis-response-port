# Working Instructions — PR #42 (CharacterBody integration)

For the agent working this branch. These are the owner's standing instructions; they override session habit. Last updated 2026-10-20.

## Current state (verified 2026-10-20)

- Calibre ammunition contract: PASSED.
- Remaining named blocker: a projectile's unresolved original MeshFilter / built-in Material mapping in the runtime suite.
- Known inventory fact: the pinned original-source tree contains five source files absent from the derived inventory (AmmunitionBase, Calibre, Casing, Projectile, UIAmmoRenderItem). Two duplicate class names between the reference export and the runtime source tree are separate source identities, not compiled together.

## How to work

1. **Fixes first, paperwork last.** A session's value is measured in landed code and asset changes. If a session can only produce documentation, say that plainly in one commit and stop. Do not present validation paperwork as progress.
2. **One scan, then batch.** Run the full validation suite once per work block. Capture every failing gate in a single committed list before fixing anything. Fix recoverable items in dependency order without re-running the whole suite between each fix.
3. **Bounded attempts.** For any single blocker: one fix attempt. If it fails, record blocked-with-reason plus a proposed disposition for the owner (accept as-is / rebuild the asset from original data / defer), then move on to the next item. Never spend a session re-verifying the same blocker.
4. **Genuine originals only.** Use recovered original assets and source data. No synthetic substitutes, no placeholder identities. Fail closed when original data is truly absent.
5. **Docs discipline.** Findings go in commit messages. The handoff document is updated exactly once, at the end of the session. No standalone evidence-refresh commits as a work product.
6. **Scope.** This branch and PR #42 only. Do not touch PRs #56, #57, or #7.

## Immediate next actions

1. Refresh the derived inventory once to include the five known original source files, then rerun its check once.
2. Take one bounded attempt at the MeshFilter / built-in Material mapping, using original data only.
3. Whatever the outcome, finish with the end state below.

## Required end state

- PR #42 merge-ready except for explicitly named blockers.
- For each remaining blocker: the reason it is blocked, and a proposed decision for the owner.
- State once that merging waits on GitHub Actions minutes being restored (owner's side). Do not re-litigate it in later sessions.
