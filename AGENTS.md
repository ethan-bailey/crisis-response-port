# AGENTS.md — operating instructions for coding agents on this repo

> Ethan (the repo owner) asked Muse to drop this here after a 2026-10-16 workflow audit. Follow these rules on every session.

# Crisis Response Port — Agent Operating Instructions

**Current repository status:** [2026-10-17 audited operations board](docs/REPOSITORY_OPERATIONS_2026-10-17.md) tracks active PRs, tested/deployed SHAs, historic branches, and manual-build constraints. Re-read it and current GitHub state before work; older handoff sections are chronological evidence.


Derived from the 2026-10-16 workflow audit. These rules keep the port moving fast without burning quota, duplicating work, or losing results.

## 0. Mandatory live-state and checkpoint precedence gate

**Before ANY code or documentation edit**, inspect live `develop/wasm`, all open PRs (including PRs recently merged into feature branches), the target branch head, recent relevant CI and `docs/REPOSITORY_REMEDIATION_2026-10-18.md` on the current full-source branch. Never choose a task solely from an old audit, conversation, `NEXT_AGENT.md`, issue body or historical compiler count. GitHub commit-search results are not sufficient to identify the newest branch head.

Record: (a) exact target branch and inspected head, (b) newest comparable tested source SHA/run, (c) what is superseded, (d) next unmet acceptance gate. Verify the target head has not moved before writing. When checkpoints use different compiler graphs, **never infer regression or improvement from raw error totals**.

The 30-errors/21-names CharacterBody checkpoint is **superseded** by the complete-graph 144-errors/60-names checkpoint in `docs/REPOSITORY_REMEDIATION_2026-10-18.md` (run 39091701268), followed by PR #66 remediation tests (run 39093133256). Read live results again, because these markers will also age. The remediation adds source inventory, compiler baseline, workflow gating and actor-ownership guards; do not duplicate those tools or resume older tasks before checking them.

For every handoff replace `NEXT_AGENT.md` entirely, begin with the observed current branch/SHA and latest validated checkpoint, and put superseded material only in Git history or dated evidence docs. If live state cannot be established, make no source changes and report the limitation.


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

- The repo stays **private**. No public forks, no public build artifacts, no secrets in logs or docs.
- Heavy operations (full Unity rebuilds, IL2CPP, large uploads) get **one controlled run at a time** with cached outputs reused — never parallel redundant builds.

## 8. Owner-approved mobile enhancements must survive source integration

- The owner explicitly permits **beneficial, well-scoped iPhone usability enhancements** even when they differ from exact original behavior. This does not permit recreating original gameplay with invented substitutes.
- **Proportional movement-stick speed is an approved, intentional feature.** Preserve the dead zone and analog target-speed scaling through future original `CharacterBody`/`PlayerController` integrations; retain keyboard full-speed behavior and recovered original acceleration/physics. Do not silently replace this with digital-only left/right movement for a 1:1 parity claim.
- The normative implementation details, test contract and provenance are in `docs/APPROVED_IPHONE_ANALOG_MOVEMENT.md`. Future deviations need explicit written rationale and user approval, plus new regression evidence.

## 9. Mandatory clean handoff replacement

- **Every agent-to-agent handoff MUST replace the entire contents of root `NEXT_AGENT.md` with a newly written current-state handoff.** Never append a new header to old handoffs, preserve outdated paragraphs underneath, or treat the file as a chronological log. This applies after implementation, audits, housekeeping, and failed attempts whenever responsibility is transferred.
- Immediately before writing, re-read current `develop/wasm` and all active PR heads, relevant source files, CI outcomes, and build/deployment manifests. Separate what is recovered, implemented, compiled, browser-tested, deployed and still blocked. Include exact SHA evidence only where verified.
- Keep the new handoff self-contained and concise (target 1–3 pages), containing: current branches/PRs with owners and overlap; verified milestones; unverified changes and explicit blockers; next ordered actions with file paths/tests; source and iPhone UI preservation rules; deployment and rollback status. Do not insert irrelevant historical milestones or outdated future plans.
- **History stays in Git history and dated evidence documents, not in `NEXT_AGENT.md`.** Do not erase Git history, original source, or recovery provenance. There is no need to generate a duplicate archive at every handoff.
- Make the complete replacement a committed change on the appropriate branch. If the handoff is on a feature branch, clearly identify that branch and avoid implying it was merged to `develop/wasm`. For cross-project handoffs, update the canonical `develop/wasm` handoff through a reviewed maintenance PR or merge when needed; never silently overwrite concurrent updates.
- Verify the committed file's first section and final length via GitHub readback. A handoff is incomplete if `NEXT_AGENT.md` still contains an earlier handoff appended underneath.
