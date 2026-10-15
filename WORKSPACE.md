# Crisis Response Port — persistent workspace

**Workspace entry point.** This file and GitHub Issues are the project workspace for the source-preserving iPhone 16 Safari port. This repository is private; do not change visibility or publicly share original game assets. A ChatGPT conversation is not a persistent build runner. Resume work from this file and `NEXT_AGENT.md` instead of attempting to recover a past chat.

## Objective and nonnegotiable rules

Reach the closest **verified 1:1 behavior and presentation** to the original Crisis Response game in an iPhone Safari browser. Preserve the game's recovered developer systems, original source assets and original numerical behavior; use C#/WASM and Rust only as explicit required engine adapters. **Never create stand-in gameplay, speculative visuals, fabricated effects, or imply parity from a compile alone.** Native Unity results remain useful reference evidence, not permission to rewrite gameplay.

- **Browser code ownership:** draft [PR #9](https://github.com/ethan-bailey/crisis-response-port/pull/9), branch `codex/projectile-material-impacts`.
- **Independent Unity feasibility:** branch `codex/iphone-input-parity-foundation`, PR #7; do not edit, merge, or reset it or `main`.
- **Previously hosted owner-private v8:** https://crisis-response-runtime.craftysnipe13.chatgpt.site — **does not include the latest tested source glass/impact renderer updates**.
- **Latest verified browser build (2026-10-15):** [GitHub Actions #38809439277](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38809439277), tested code SHA `ed3d80d89f5e4e797e0370d9152bcc828bd8becb`, private artifact `12007339085`. Both Chromium and WebKit passed original firing/touch/geometry/glass stamp tests. No physical iPhone proof.
- **Source-backed window audit:** [runs 38808368685 and 38808663563](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38808663563); 56 glass + 12 mirror-glass native window Quads. Exact shader parity and actual-device test are outstanding.
- **Shader recovery gap:** [original shader audit run 38812611701](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38812611701) proved both Shader Forge glass/mirror source files are AssetRipper `DummyShaderTextExporter` output, not original compiled shader behavior. Full visual parity blocked pending native reference or compiled shader recovery.
- **Original particles inventory:** [original source audit run 38812872182](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38812872182) passed, 26 ParticleSystems and 26 ParticleSystemRenderers across 14 entry/exit effect prefabs; private artifact `12008823420`. Not yet simulated/rendered.


## Full compile-to-release checklist

**Primary plan:** [docs/COMPLETE_WASM_PORT_ROADMAP.md](docs/COMPLETE_WASM_PORT_ROADMAP.md). It defines Gates 1–8, actual known-working .NET/PhysX build commands, full original game scope, native reference checks and private iPhone release criteria. **WASM compilation is already passing for the current source subset; 1:1 game completion is not.**

## Work board

| Order | Track | Issue | Acceptance milestone |
| --- | --- | --- | --- |
| P1 | Original window and mirror shaders | [#10](https://github.com/ethan-bailey/crisis-response-port/issues/10) | Source shader/UV/alpha/depth rules audited, original-backed WebGL behavior with regression evidence |
| P1 | Original particles and fragments | [#11](https://github.com/ethan-bailey/crisis-response-port/issues/11) | Original source emitter data/meshes/time/lifetimes used; unsupported remain explicit |
| P1 | Owner-private iPhone checkpoint | [#13](https://github.com/ethan-bailey/crisis-response-port/issues/13) | Matching tested artifact privately deployed with rollback metadata and real device feedback |
| P2 | Native engine/physics/physiology fidelity | [#12](https://github.com/ethan-bailey/crisis-response-port/issues/12) | Native equivalence fixtures for original gameplay systems; no invented simulation |
| P1 | All six original scenes and dependencies | [#14](https://github.com/ethan-bailey/crisis-response-port/issues/14) | Prefab/scene dependencies and original content integrity gated |
| P2 | AI, teams, doors, grenades, missions | [#15](https://github.com/ethan-bailey/crisis-response-port/issues/15) | Source gameplay running; native-parity scenarios |
| P2 | Original UI, saves and platform adapters | [#16](https://github.com/ethan-bailey/crisis-response-port/issues/16) | Source menu/persistence/input/platform behaviors verified |
| P1 | Native Unity versus WASM comparisons | [#17](https://github.com/ethan-bailey/crisis-response-port/issues/17) | Deterministic traces; no unmeasured required gaps |
| P1 | Reproducible private iPhone release | [#18](https://github.com/ethan-bailey/crisis-response-port/issues/18) | SHA-matched protected site plus actual-device tests |


Existing [#6](https://github.com/ethan-bailey/crisis-response-port/issues/6), [#5](https://github.com/ethan-bailey/crisis-response-port/issues/5), and [#4](https://github.com/ethan-bailey/crisis-response-port/issues/4) retain earlier context; avoid duplicate tickets.

## Required workflow for future sessions

1. Read `WORKSPACE.md`, `NEXT_AGENT.md`, `docs/WASM_ORIGINAL_WINDOW_VISUALS_2026-10-15.md`, and the relevant linked issue. Inspect the current **branch HEAD**, active tests and CI status before modifying anything. Multiple agents may edit the repo concurrently.
2. Map every proposed change to a recovered original source file, serialized field, GUID, or evidence artifact. Record unresolved dependencies explicitly before implementing. Do not modify `runtime/Original/` with fabricated behavior.
3. Implement in small source-preserving commits on the browser PR branch using SHA-checked writes. Keep test assertions next to adapters. Do not rewrite the viewer or switch engines without verified need.
4. Prefer local/static/targeted validation and user iPhone manual testing. Dispatch expensive GitHub Actions at **milestones**, not on every source push; never start duplicate pending builds.
5. Record in `docs/WORKSPACE_STATUS.md` the exact input commit, modified files, source hashes/GUIDs, commands/tests and results, artifact IDs, known limitations, next bounded action and **selective rollback** commands. Revise `NEXT_AGENT.md` only when the verified checkpoint changes. Do not overwrite someone else's work.
6. Never merge to main, change hosting visibility or republish the original game publicly without authorization. Only describe a build as *live* once owner-private deployment and artifact integrity are verified.
7. **No autonomous offline continuation:** scheduled reminders are not ongoing code execution. A future agent/Work session must explicitly run the next steps.

## Evidence and rollback

- Original geometry/material source and shader gaps: `docs/ORIGINAL_IMPACT_EFFECT_RENDER_PROGRESS_2026-10-15.md`, `docs/WASM_ORIGINAL_WINDOW_VISUALS_2026-10-15.md`.
- Exact prior WASM compiler and browser-run proof: `docs/WASM_CONTINUATION_CHECKPOINT_2026-10-15.md`.
- Material and fragment gameplay limits: `IMPACT_CHECKPOINT.md`.
- Follow-up daily status, change ledger, and run links: `docs/WORKSPACE_STATUS.md`.
- A rollback is a **new selective revert commit** after comparing file diffs and later changes; never a destructive branch reset or force push. If tests fail, record the failing run SHA and blocker rather than calling it finished.
