# Contributing to Crisis Response Port

This private project preserves original Crisis Response developer code and behavior for a C#/.NET WebAssembly browser port. Do not publish proprietary source, assets, private build artifacts or credentials. **A successful compile is not proof of original-game parity.**

## Start here

Read the [audited operations board](docs/REPOSITORY_OPERATIONS_2026-10-17.md) and [branching and collaboration guide](docs/BRANCHING_AND_COLLABORATION.md), then the current [NEXT_AGENT.md](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/NEXT_AGENT.md) on `develop/wasm`, the [workspace](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/WORKSPACE.md), and the linked issue. The latest plan is on `develop/wasm`; historical notes are not active assignments.

## Standard workflow

1. Choose or open one scoped issue. Read the real `develop/wasm` HEAD and active Actions runs; coordinate overlapping source/adapter files.
2. Create a short-lived branch **from current `develop/wasm`**: `feature/<issue>-<name>`, `fix/<issue>-<name>`, `research/<topic>`, or `experiment/<topic>`.
3. Open a focused PR **targeting `develop/wasm`**, link the issue, use the PR template, and note the exact original source paths, GUIDs, SHA256, methods, adapters, test results and rollback instructions.
4. Preserve recovered original files byte-for-byte. Write only required browser/Unity engine adapters when source semantics are understood. **No placeholder gameplay, fabricated callbacks, dummy shaders or claimed 1:1 behavior without evidence.**
5. Run fast source, package and unit tests. Trigger full .NET 8 WASM/PhysX/Chromium/WebKit only for meaningful integration milestones—not every commit, documentation change or audit.
6. Review against the latest `develop/wasm` and merge one reviewed feature at a time; conventional merge commits are preferred for traceability. Record matching tested SHA, artifact IDs, remaining limitations and selective rollback in the shared handoff.
7. Deploy a matching **protected** Vercel build only when testing on a real iPhone. Keep `vercel.json` → `git.deploymentEnabled=false`.

`main` is the stable baseline; it is **not** the regular gameplay integration branch and not a claim the whole game is complete. Promotions from `develop/wasm` to `main` require explicit review and evidence. Independent Unity feasibility remains isolated.

## Agent operating checklist — 2026-10-16 audit

These rules are shared by human contributors and AI agents. They apply to **every** source-preserving WASM/Unity workstream.

- **Before writing:** survey active branches/PRs, running CI, and current integration HEAD; avoid duplicate work and coordinate ownership of overlapping engine adapters. Open a **draft PR early** when a feature branch contains valuable work; target `develop/wasm`. Close stale/superseded PRs with a traceable link.
- **Keep branches current:** refresh a contributor-owned feature branch against `develop/wasm` before integration if it has diverged. Rebase **only your own unpublished/unshared commits** with coordination; never rebase/force-reset a shared branch or another agent's active work. Prefer a normal merge commit for verified integrations.
- **Evidence for every claim:** distinguish **compiles**, **browser boots**, **executes original gameplay**, **native parity**, and **physical iPhone**. Record tested commit SHA, localhost browser smoke result, screenshots and artifact IDs; keep screenshots in private CI artifacts linked by the PR rather than unnecessarily committing huge binaries. Do not claim site functionality from a Vercel READY state alone.
- **Minimal expensive work:** use ordinary fast checks on changed source; one cached SDK8 WASM/PhysX/Chromium/WebKit milestone for a meaningful gameplay batch; no concurrent redundant full Unity/IL2CPP builds. Original Unity workflow remains manual-only, must release its license seat on completion, and must never log `UNITY_EMAIL`/`UNITY_PASSWORD`.
- **Publish on demand, after real browser boot:** a Vercel preview needs matching localhost Chromium/WebKit/game smoke evidence first. Prefer a **stable test alias** mapping to the last deliberate deploy (verify its mapping), not an unverified build or a new arbitrary URL per commit. Check complete site-package hashes and that Git tracks ignored scene/PhysX binaries. If quotas rise, note this in the branch handoff early.
- **Security and rights:** repository source and recovery archive stay private by default. User access preference can change for an authorized **compiled, asset-cleared** build, but do not make recovered proprietary source or unlicensed original assets openly redistributable just to avoid sign-in. Never include tokens, Unity license secrets, passwords, or CI credentials in logs.

**Source-specific HUD exception:** a diagnostic UI may represent original state without replacing game rules. Its visual distinction from original developer art must be documented. Only recovered actual game assets are eligible when the user requests no generated artwork.

## Evidence terms

**Recovered** = original source/assets hashed; **compiles** = selected source builds; **executes** = original methods run; **browser-tested** = matched Chromium/WebKit packaged execution; **native-parity checked** = compared with original Unity reference; **device-tested** = real iPhone Safari evidence; **released** = protected, verified and rollback-ready. Do not conflate them.

For complete-game gates see [roadmap on develop/wasm](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/docs/COMPLETE_WASM_PORT_ROADMAP.md).
