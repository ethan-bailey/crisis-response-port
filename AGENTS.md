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
- **Rebase short-lived feature branches when useful.** The owner currently uses one agent at a time, so prefer rebasing an agent-owned feature branch onto current `develop/wasm` before merging when this reduces conflicts. Confirm the branch is not shared or already under concurrent modification; preserve provenance. Never rewrite `main`, `develop/wasm`, already merged commits, release tags, or immutable build evidence. If a feature branch was published, use `--force-with-lease` only after confirming no other writer and documenting the rewritten head; otherwise prefer a merge. Ordinary reviewed merges remain appropriate for integration.

## 3. No duplicate work

- Before starting a workstream, **check open PRs and recent branches for overlapping scope**. Two PRs doing the same thing (cf. the #23/#25 arm-controller overlap) waste everyone's time.
- When consolidating, close the superseded PR with a link to the surviving one so the history is traceable.

## 4. Verification standard: evidence, not claims

- A successful compile is a compile result, **not proof of browser boot or correct gameplay**. Every functional claim needs appropriate evidence: localhost smoke output, real browser boot observation (for native Unity WebGL, `createUnityInstance`), screenshots or automated browser assertions or equivalent automated browser evidence — preserved as GitHub Actions artifacts with run ID, exact tested SHA, and retention limits documented. Commit only durable, high-value reference screenshots; do not add every screenshot to Git history.
- Distinguish clearly in commit messages and docs between "it compiled," "it booted," and "it plays correctly." These are three different milestones.
- Unity work: **return the license seat after every CI run**, keep CI manual-only, and never expose `UNITY_EMAIL` / `UNITY_PASSWORD`.

## 5. Status reporting: the whole board

- When reporting status, **survey active branches and PRs for overlap and relevant milestones**, not just the one you touched last. Report tested gameplay source SHA, deployed site SHA/URL, and unfinished work separately; a current deployment does not need to include unmerged experiments. A "published build" is only current if it tracks the latest work across every lane.
- Commit messages should state **what was verified**, not just what was done. Future-you (and the next agent) will thank present-you.

## 6. Proportionate verification

- For documentation, inventory, and preserved-source-only changes, use cheap static/provenance checks. Do not rebuild .NET WASM, PhysX, browser bundles, or Unity solely to prove an unchanged runtime.
- For executable gameplay or adapter changes, run focused unit/source tests first; reserve the full .NET 8 WASM + Chromium/WebKit and local HTTP smoke pipeline for meaningful integration milestones. A public preview requires a matching passing local/browser smoke and verified package.
- Do not treat successful compilation as proof of original physics, original animation, visual fidelity, real iPhone Safari playability, or 1:1 parity.

## 7. Housekeeping

- Repository visibility and Actions policy: follow [`docs/REPOSITORY_VISIBILITY_AND_ACTIONS.md`](docs/REPOSITORY_VISIBILITY_AND_ACTIONS.md). As verified 2026-10-24, the repository is public and Actions is enabled. Keep secrets out of logs and docs.
- Heavy operations (full Unity rebuilds, IL2CPP, large uploads) get **one controlled run at a time** with cached outputs reused — never parallel redundant builds.

## 8. Protect user-approved gameplay usability enhancements

- The owner explicitly permits **beneficial, well-scoped iPhone usability improvements** even when they differ from strict original-game input behavior. This does not authorize invented substitutes for original gameplay.
- **Variable-speed analog joystick is intentional and must be retained.** Horizontal thumb travel beyond the dead zone scales the original Operator walk/sprint target speed while preserving original input actions, source-derived acceleration/PhysX and full-speed keyboard controls. Do **not** silently regress the stick to digital-only left/right controls when porting original CharacterBody or PlayerController.
- Implementation, normalization, lifecycle and test contract are documented in [the canonical WASM enhancement record](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/docs/APPROVED_IPHONE_ANALOG_MOVEMENT.md), which is also referenced in the WASM branch's AGENTS.md and NEXT_AGENT.md. Any removal or intentional behavior regression requires explicit owner approval.
