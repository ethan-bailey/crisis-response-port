# Contributing to Crisis Response Port

This private project preserves original Crisis Response developer code and behavior for a C#/.NET WebAssembly browser port. Do not publish proprietary source, assets, private build artifacts or credentials. **A successful compile is not proof of original-game parity.**

## Start here

Read [the branching and collaboration guide](docs/BRANCHING_AND_COLLABORATION.md), then the current [NEXT_AGENT.md](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/NEXT_AGENT.md) on `develop/wasm`, the [workspace](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/WORKSPACE.md), and the linked issue. The latest plan is on `develop/wasm`; historical notes are not active assignments.

## Standard workflow

1. Choose or open one scoped issue. Read the real `develop/wasm` HEAD and active Actions runs; coordinate overlapping source/adapter files.
2. Create a short-lived branch **from current `develop/wasm`**: `feature/<issue>-<name>`, `fix/<issue>-<name>`, `research/<topic>`, or `experiment/<topic>`.
3. Open a focused PR **targeting `develop/wasm`**, link the issue, use the PR template, and note the exact original source paths, GUIDs, SHA256, methods, adapters, test results and rollback instructions.
4. Preserve recovered original files byte-for-byte. Write only required browser/Unity engine adapters when source semantics are understood. **No placeholder gameplay, fabricated callbacks, dummy shaders or claimed 1:1 behavior without evidence.**
5. Run fast source, package and unit tests. Trigger full .NET 8 WASM/PhysX/Chromium/WebKit only for meaningful integration milestones—not every commit, documentation change or audit.
6. Review against the latest `develop/wasm` and merge one reviewed feature at a time; conventional merge commits are preferred for traceability. Record matching tested SHA, artifact IDs, remaining limitations and selective rollback in the shared handoff.
7. Deploy a matching **protected** Vercel build only when testing on a real iPhone. Keep `vercel.json` → `git.deploymentEnabled=false`.

`main` is the stable baseline; it is **not** the regular gameplay integration branch and not a claim the whole game is complete. Promotions from `develop/wasm` to `main` require explicit review and evidence. Independent Unity feasibility remains isolated.

## Evidence terms

**Recovered** = original source/assets hashed; **compiles** = selected source builds; **executes** = original methods run; **browser-tested** = matched Chromium/WebKit packaged execution; **native-parity checked** = compared with original Unity reference; **device-tested** = real iPhone Safari evidence; **released** = protected, verified and rollback-ready. Do not conflate them.

For complete-game gates see [roadmap on develop/wasm](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/docs/COMPLETE_WASM_PORT_ROADMAP.md).
