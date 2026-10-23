# NEXT AGENT — Crisis Response WASM (current handoff)

## Observed Git state (2026-10-23)

- Integration branch `develop/wasm`: `1c076c6c01f4bf1da8eafbae30629ba4de258711`, verified through the GitHub connector. Always recheck the live ref before writing.
- Active focused branch: `fix/browser-publish-input-sdk-pin-20261023`, code checkpoint `4734d23664a65093199de20794182d39abd44060` before this handoff update. Draft PR [#69](https://github.com/ethan-bailey/crisis-response-port/pull/69). It is **not merged**.
- PR #69 changes: (1) `runtime/Browser/Crisis.Runtime.Browser.csproj` links the tracked original `game/browser-input.mjs` into `wwwroot/browser-input.mjs` as publish content; (2) new root `global.json` exactly matches `tools/wasm-compile/global.json` pinning .NET SDK 8.0.426.
- GitHub readback verified those two config changes and matching SDK pins. **No .NET build or published-output verification has been run for PR #69.**
- Other open PRs: #56 (original projectile/physiology integration), #57 (arm IK), and #7 (historical Unity/iPhone route). Do not merge or delete them as part of PR #69.

## Latest comparable verification and remaining gates

- Prior verified SDK 8.0.426 build at `8ab1c2d49a72dd661ef750377fa5caeecbc49715`: Release runtime build passed with 0 errors/45 warnings; browser publish passed with 0 errors. These results **do not validate PR #69**.
- Packaging previously failed because `runtime-web/browser-input.mjs` was absent despite tracked `game/browser-input.mjs` being present. PR #69 addresses the host-project source-level omission; verify actual publish output rather than assuming success.
- Mission 01 CRG generation/provenance was previously verified in PR #68, merged at `f3802019e7cfdd419223dc3e02b9df87b8035289`. Reverify against the candidate package.
- Marksman scene rendering and live FIRE/movement input remain unverified on a complete candidate package.
- Two-actor lifecycle remains unresolved: real `Global.Context` / Mission binding is required; a prior temporary fixture exposed `TeamOperator.Awake` failure at `TeamOperator.cs:110` and was reverted. No two-actor pass is claimed.
- Existing Sites deployment was reported as version 26 at source `64d9c812ab01e23f37ac6c0b0b3137b71130e62d`. No new deployment was made here.

## Environment blocker and preservation

- This execution context has GitHub repository API access and local Python/Node/Chromium, but **no .NET SDK, authenticated checkout, Codespace terminal, or complete compiled package**. Local `git ls-remote` could not resolve github.com; build and browser gates cannot run here.
- An earlier Codespace report describes divergent local-only and remote-only commits. **The Codespace checkout and its uncommitted files were not accessible here.** Do not assume this divergence is reconciled; inspect and back up unique committed AND uncommitted work before modifying that checkout.
- GitHub Actions remain disabled by owner decision. Do not run or re-enable workflows. Public Sites publishing remains gated by local package/browser and applicable actor checks.

## Exact next actionable work

1. In a .NET 8.0.426-capable environment with a full checkout, inspect its worktree and both local/remote refs; safely preserve any unique work, then reconcile divergence before using it for builds.
2. Check out the current PR #69 head and confirm `dotnet --list-sdks` plus the repository-root `global.json` select 8.0.426.
3. Build `runtime/Crisis.Runtime.csproj` in Release with `--no-incremental`, then publish `runtime/Browser/Crisis.Runtime.Browser.csproj`. Confirm published `wwwroot/browser-input.mjs` exists and its SHA-256 matches `game/browser-input.mjs`. If the MSBuild `Content` link does not publish correctly, make one bounded project-target fix, then rerun once.
4. Run `scripts/package_wasm_site.py` on that output, including source scene CRG/provenance and integrity-manifest verification.
5. Run a real local browser smoke for Marksman rendering and FIRE/movement input; resolve the scene flow using original systems. Then execute the original Operator/Occupant lifecycle gate with real scene dependencies.
6. Land PR #69 only after the applicable standing local merge gate passes; publish the verified package to the existing Sites project only after all required local gates pass. Keep PR #56/#57/#7 untouched unless separately authorized and verified.

Use `AGENTS.md`, `AGENT_INSTRUCTIONS.md`, `docs/REPOSITORY_REMEDIATION_2026-10-18.md`, and the existing recovery inventory as authoritative references. The owner-approved workflow recovery plan prioritizes actual gameplay, minimal repeat work, source preservation, a concise single handoff, and exact-SHA evidence. This document replaces obsolete chronological handoff records; Git history retains the previous text.
