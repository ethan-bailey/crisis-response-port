# AGENTS.md — operating instructions for coding agents on this repo

> Ethan (the repo owner) asked Muse to drop this here after a 2026-10-16 workflow audit. Follow these rules on every session.

# Crisis Response Port — Agent Operating Instructions

Derived from the 2026-10-16 workflow audit. These rules keep the port moving fast without burning quota, duplicating work, or losing results.

## 1. Deployments: on-demand only

- **No automatic Vercel deployments on push.** Validate on localhost in CI; deploy to Vercel only when you need a public, device-testable URL (e.g., real iPhone Safari testing — localhost can't reach the phone).
- **Gate every public deploy on a passing localhost smoke test.** Never deploy a commit that hasn't booted locally.
- **Use each branch's stable preview alias** for testing, not per-deployment URLs. One URL per branch, always pointing at its latest deploy.
- If usage starts climbing again, say so in the branch docs before it becomes a problem — don't wait to be asked.

## 2. Branch and PR hygiene

- **One branch per workstream.** Open a draft PR as soon as a branch holds real work — high-value results (like runtime validation) must never sit on a branch with no merge path.
- **Close stale PRs promptly.** Don't leave dead PRs open; close with a pointer to whatever superseded them.
- **Rebase long-lived branches before merging.** 30+ commits of divergence turns merges into projects — rebase early and often against the target branch.

## 3. No duplicate work

- Before starting a workstream, **check open PRs and recent branches for overlapping scope**. Two PRs doing the same thing (cf. the #23/#25 arm-controller overlap) waste everyone's time.
- When consolidating, close the superseded PR with a link to the surviving one so the history is traceable.

## 4. Verification standard: evidence, not claims

- A build succeeding is not a result. **Every functional claim needs evidence**: localhost smoke output, boot observation (`createUnityInstance` in a real browser), screenshots — committed to the branch, not just described.
- Distinguish clearly in commit messages and docs between "it compiled," "it booted," and "it plays correctly." These are three different milestones.
- Unity work: **return the license seat after every CI run**, keep CI manual-only, and never expose `UNITY_EMAIL` / `UNITY_PASSWORD`.

## 5. Status reporting: the whole board

- When reporting status, **survey all active branches and PRs**, not just the one you touched last. A "published build" is only current if it tracks the latest work across every lane.
- Commit messages should state **what was verified**, not just what was done. Future-you (and the next agent) will thank present-you.

## 6. Housekeeping

- The repo stays **private**. No public forks, no public build artifacts, no secrets in logs or docs.
- Heavy operations (full Unity rebuilds, IL2CPP, large uploads) get **one controlled run at a time** with cached outputs reused — never parallel redundant builds.
