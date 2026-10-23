# AGENTS.md — operating instructions for coding agents on this repo

> Ethan (the repo owner) asked Muse to drop this here after a 2026-10-16 workflow audit. Follow these rules on every session.

# Crisis Response Port — Agent Operating Instructions

## Current execution environment — owner direction, 2026-10-23

The owner has switched routine repository work from Codespaces to the Codex Work Mode checkout. Work directly in the currently provided repository workspace; do not route routine edits through a browser or restart Codespaces. This is a managed Codex workspace, not the owner's iPhone or personal computer. Scratch files may disappear between chats, so persist repository changes by committing and pushing the active branch, then verify the GitHub branch/PR head. Use the GitHub connector to inspect live branch/PR state when shell GitHub authentication is unavailable. Keep all existing build, browser, gameplay, and publishing acceptance gates unchanged.

See [`AGENT_INSTRUCTIONS.md`](AGENT_INSTRUCTIONS.md) for the session-start efficiency check, .NET runner failure handling, and handoff rules.

## 0. Mandatory live-state and checkpoint precedence gate

**Before ANY code or documentation edit**, inspect live `develop/wasm`, all open PRs (including PRs recently merged into feature branches), the target branch head, recent relevant local verification evidence, and `docs/REPOSITORY_REMEDIATION_2026-10-18.md` on the current full-source branch. GitHub Actions is disabled by owner decision, so no CI run is required or expected. Never choose a task solely from an old audit, conversation, `NEXT_AGENT.md`, issue body or historical compiler count. GitHub commit-search results are not sufficient to identify the newest branch head.

Record: (a) exact target branch and inspected head, (b) newest comparable tested source SHA/run, (c) what is superseded, (d) next unmet acceptance gate. Verify the target head has not moved before writing. When checkpoints use different compiler graphs, **never infer regression or improvement from raw error totals**.

The 30-errors/21-names CharacterBody checkpoint is **superseded** by the complete-graph 144-errors/60-names checkpoint in `docs/REPOSITORY_REMEDIATION_2026-10-18.md` (run 39091701268), followed by PR #66 remediation tests (run 39093133256). Read live results again, because these markers will also age. The remediation adds source inventory, compiler baseline, workflow gating and actor-ownership guards; do not duplicate those tools or resume older tasks before checking them.

For every handoff replace `NEXT_AGENT.md` entirely, begin with the observed current branch/SHA and latest validated checkpoint, and put superseded material only in Git history or dated evidence docs. If live state cannot be established, make no source changes and report the limitation.


Derived from the 2026-10-16 workflow audit. These rules keep the port moving fast without burning quota, duplicating work, or losing results.

## 0a. GitHub Actions disabled; local verification is the standing gate

GitHub Actions is disabled by owner decision for this project. Do not restore minutes, trigger workflows, or treat Actions checks as a gate. For landing work, run local verification on the exact candidate tree: a fresh full-source build, relevant contracts, and provenance checks. Record commands, results, and the exact tested source SHA in the handoff or evidence docs. Gameplay and browser behavior still require their applicable local/live smoke tests; a successful compile alone is not a gameplay or browser pass.

## 1. Deployments: ChatGPT Sites primary, manual Vercel fallback

- **ChatGPT Sites is the primary host** for an owner-authorized public build. Build the compiled runtime and required game assets from the exact develop/wasm source SHA, generate and verify an integrity manifest, and preserve the package byte-for-byte through Site creation.
- **Local smoke is a hard gate.** Before publishing, use a real browser to confirm the page loads, the WebAssembly runtime boots, the Marksman scene loads, and input responds. A failed or incomplete check means no publish. After publishing, repeat those checks in a fresh browser session against the live Site.
- Check that Sites accepts the verified package size and file count, serves .wasm for execution, and loads required audio and asset files. If Sites cannot host a locally passing package, allow one repackaging-only attempt; then use the manual Vercel fallback and record the exact Sites failure and resulting URL.
- **Vercel is manual fallback only.** Automatic Git deployments remain disabled by the committed vercel.json setting. Do not re-enable automatic deployment. Use the same already verified package for fallback.
- Keep the repository and raw recovered sources private. Public Sites hosting is limited to the exact package authorized by the owner; do not publish repository source or recovery archives.

## 2. Branch and PR hygiene - single-line workflow

Owner rule (2026-10-22): one agent works this repo at a time, so there is **one active workstream and one line of development** - `develop/wasm` is the project; branches are short errands off it, not parallel homes.

- **One active branch.** Do not run parallel feature branches. New ideas go on the written list (handoff or issue), not into a new branch, until the active workstream lands or the owner parks it. Open a draft PR as soon as a branch holds real work - high-value results (like runtime validation) must never sit on a branch with no merge path.
- **Short-lived by default.** Branch from current `develop/wasm`, land one verified slice, and merge back the same session or day where possible. Stockpiling verified work on an unmerged branch is how PR #42 diverged from `develop/wasm` and ended up needing repeated merge probes - treat an ageing unmerged branch as a problem to report, not a routine state.
- **Sync long-lived branches every session.** If a branch must live beyond one session, merge current `develop/wasm` into it at the start of each session and resolve the small overlaps while they are fresh. Rebase only your own unpublished, unshared commits (see CONTRIBUTING.md). Never rewrite `main`, `develop/wasm`, already merged commits, release tags, or immutable build evidence. If a published feature branch truly must be rewritten, use `--force-with-lease` only after confirming no other writer and documenting the rewritten head; otherwise prefer a merge. Ordinary reviewed merges remain appropriate for integration.
- **Close stale PRs promptly.** Don't leave dead PRs open; close with a pointer to whatever superseded them.

## 3. No duplicate work

- Before starting a workstream, **check open PRs and recent branches for overlapping scope**. Two PRs doing the same thing (cf. the #23/#25 arm-controller overlap) waste everyone's time.
- When consolidating, close the superseded PR with a link to the surviving one so the history is traceable.

## 4. Verification standard: evidence, not claims

- A successful compile is a compile result, **not proof of browser boot or correct gameplay**. Every functional claim needs appropriate evidence: local smoke output, real browser boot observation (for native Unity WebGL, `createUnityInstance`), screenshots or automated browser assertions or equivalent browser evidence. Record the commands, results, evidence references, and exact tested source SHA in the handoff or evidence docs. GitHub Actions artifacts and run IDs are unavailable because Actions is disabled by owner decision. Commit only durable, high-value reference screenshots; do not add every screenshot to Git history.
- Distinguish clearly in commit messages and docs between "it compiled," "it booted," and "it plays correctly." These are three different milestones.
- Unity work: return the license seat after each Unity run, and never expose `UNITY_EMAIL` / `UNITY_PASSWORD`. GitHub Actions stays disabled; use local verification.

## 5. Status reporting: the whole board

- When reporting status, **survey active branches and PRs for overlap and relevant milestones**, not just the one you touched last. Report tested gameplay source SHA, deployed site SHA/URL, and unfinished work separately; a current deployment does not need to include unmerged experiments. A "published build" is only current if it tracks the latest work across every lane.
- Commit messages should state **what was verified**, not just what was done. Future-you (and the next agent) will thank present-you.

## 6. Proportionate verification

- For documentation, inventory, and preserved-source-only changes, use cheap static/provenance checks. Do not rebuild .NET WASM, PhysX, browser bundles, or Unity solely to prove an unchanged runtime.
- For executable gameplay or adapter changes, run focused unit/source tests first; reserve the full .NET 8 WASM + Chromium/WebKit and local HTTP smoke pipeline for meaningful integration milestones. A public preview requires a matching passing local/browser smoke and verified package.
- Do not treat successful compilation as proof of original physics, original animation, visual fidelity, real iPhone Safari playability, or 1:1 parity.

## 7. Housekeeping

- The repository and raw recovered materials stay **private**. No public forks, raw source, recovery archives, or secrets in logs/docs. An owner-authorized compiled package may be hosted publicly on ChatGPT Sites only after the required local and live smoke gates; Vercel remains a manual fallback and automatic deployments stay disabled.
- Heavy operations (full Unity rebuilds, IL2CPP, large uploads) get **one controlled run at a time** with cached outputs reused — never parallel redundant builds.

## 8. Owner-approved mobile enhancements must survive source integration

- The owner explicitly permits **beneficial, well-scoped iPhone usability enhancements** even when they differ from exact original behavior. This does not permit recreating original gameplay with invented substitutes.
- **Proportional movement-stick speed is an approved, intentional feature.** Preserve the dead zone and analog target-speed scaling through future original `CharacterBody`/`PlayerController` integrations; retain keyboard full-speed behavior and recovered original acceleration/physics. Do not silently replace this with digital-only left/right movement for a 1:1 parity claim.
- The normative implementation details, test contract and provenance are in `docs/APPROVED_IPHONE_ANALOG_MOVEMENT.md`. Future deviations need explicit written rationale and user approval, plus new regression evidence.

## 9. Dependency closure and actor migration checkpoints

- Full original CharacterBody compilation is manual-only, after a cohesive
  subsystem batch passes narrow contracts. Do not restore push-triggered full
  builds for individual missing types. Preserve failed compiler exit status.
- Before importing source, use `docs/audit/recovery-source-index.json` and
  `scripts/inventory_source_dependencies.py`; review ambiguous type identities.
  Regenerate the source inventory with source changes and keep `--check` passing.
- `Crisis.Runtime.FullSource.CharacterBody` is the final actor authority. Do not
  add gameplay to the frozen transport owner files without a documented,
  reviewed exception in `docs/REPOSITORY_REMEDIATION_2026-10-18.md`.
- Run actor boundary/source checks, isolated EngineChecks, and locally runnable foundation
  checks before a deliberate complete compiler checkpoint. Two-actor lifecycle
  and consolidated WASM/browser evidence remain required before gameplay migration.
- Keep compiler baselines pinned to exact source/run evidence. Use diagnostic
  diffs to distinguish resolved, introduced and unchanged errors. Read the
  remediation log for rollback instructions and cross-PR integration contracts.
