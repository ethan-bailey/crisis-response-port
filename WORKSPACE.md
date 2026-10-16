# Crisis Response Port — persistent workspace

**Workspace entry point.** This file and GitHub Issues are the project workspace for the source-preserving iPhone 16 Safari port. This repository is private; do not change visibility or publicly share original game assets. A ChatGPT conversation is not a persistent build runner. Resume work from this file and `NEXT_AGENT.md` instead of attempting to recover a past chat.

## Active branches and collaboration

**Start at [CONTRIBUTING.md](CONTRIBUTING.md) and [branch policy](docs/BRANCHING_AND_COLLABORATION.md).** Since October 16, 2026: `main` is the stable repository baseline, **`develop/wasm` is the only integrated C#/.NET WASM game branch**, and new source-backed features come in short-lived `feature/*` PRs targeting `develop/wasm`. Historic PRs #8/#9 seeded this branch; they are not active development targets. Historical tooling PRs #19/#20 target `develop/wasm` for selective review. The **current** work assignment is [NEXT_AGENT.md](NEXT_AGENT.md), with [issue #22](https://github.com/ethan-bailey/crisis-response-port/issues/22) as the next original animation playback dependency. Existing shaders, particle, AI, mission, UI and device parity are unfinished.

**Latest independently verified original-source runtime milestone:** [PR #24](https://github.com/ethan-bailey/crisis-response-port/pull/24) merged into `develop/wasm` at `252337d96b845f5b2eb6dd406c32a52a423d3e37`. Actual unmodified developer `Hand.SetMode` executes in native C# and WASM Chromium/WebKit (three hand modes) at [Actions #38908722534](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38908722534), exact feature tested source `81b269dba69e79cbb325d9d92e5e45a1fd926e6b`, private site `12049028233`, evidence `12048874054`. **Actual merged `develop/wasm` source SHA `252337d96b845f5b2eb6dd406c32a52a423d3e37` independently passed the full browser/WASM pipeline** in [Actions #38909327063](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38909327063), site `12049238437`, evidence `12049348261`. Original `IKArmController`/`IKArmSequencer` playback still unimplemented; four original limb/weapon source references preserved, not compiled. Next source closure tracked in [issue #22](https://github.com/ethan-bailey/crisis-response-port/issues/22). No public or automatic Vercel deployment; full game remains incomplete.

Latest proven integrated executable build: [Actions #38901664939](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38901664939), source `1264c0b4579076b82e1b6b21955469313779fe39`, private site artifact `12046137813`, private evidence `12046012744`. Organizational/doc changes on this branch were **not** recompiled. Root `vercel.json` disables automatic deployments; only protected manual iPhone test releases.

## Current source-backed M4 magazine touch HUD on iPhone preview

**[Open latest original M4 magazine HUD preview](https://crisis-response-port-git-preview-w-6bd30d-ethanbaileymarsh-8332.vercel.app/game/)**. Verified `develop/wasm` gameplay source SHA `1e501e252379113c79722c00ed17860280964f34`, [24-step C#/WASM+browser run #38919612434](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38919612434) PASS, 726-file snapshot SHA `369cdc2c059c0030407e1496962ffe74894c478a`, deployment `dpl_DnejUSMqWQmymF41oHNG13BkumWR` READY. Original `Magazine AR 30 round` developer texture, precise live ammo count, shot feedback and drag-to-reload were validated in both Chromium and WebKit, with screenshots. The page is still owner-authenticated and actual device Safari testing is pending; physical magazine/weapon/character parity incomplete. Older previous preview URLs below are historical.

## Historical previous device preview and efficient release gate

**Test this repaired version, not the previous 404 version:** https://crisis-response-port-fb7x1neld-ethanbaileymarsh-8332.vercel.app/game/. Vercel preview `dpl_7T1JZtAUjQLRZw2mys7i5xyh8tvH` READY, private snapshot SHA `4c1b0d27014b1aa9776fd7da46b7f46630b50be2`. [Repair #38913180609](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38913180609) verified all 725 original compiled package files from previously successful WASM source SHA `252337d96b845f5b2eb6dd406c32a52a423d3e37`. Old preview had a 404 because ignored scene and PhysX files were not committed. Real protected website and iPhone Safari execution have not yet been independently confirmed.

Subsequent release process is on-demand and reusable: [manual-verified-wasm-preview.yml](https://github.com/ethan-bailey/crisis-response-port/blob/main/.github/workflows/manual-verified-wasm-preview.yml) downloads a prior tested package, checks all hashes and Git-index entries, stages reusable private snapshot `preview/wasm-current-iphone`; then one explicit protected Vercel deploy when user asks. No game recompilation for release packaging, no one-off Actions workflow, no auto Vercel deployments.

## Historical first protected device-test snapshot (broken; superseded)

**Manually published preview:** [Open partial Crisis Response WASM game](https://crisis-response-port-bpbgx2jdn-ethanbaileymarsh-8332.vercel.app/game/). The deployment is **READY with Vercel SSO protection**, built from the last proven gameplay commit `252337d96b845f5b2eb6dd406c32a52a423d3e37` and verified WASM/Chromium/WebKit [Actions #38909327063](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38909327063). Site staged as the verified static package in private `preview/wasm-hand-modes-20261016`, no automatic Vercel deploy. **No actual physical iPhone Safari acceptance yet**, and the agent connection could not access the protected page without a Vercel sign-in; see [preview identity, access and limitations](docs/PRIVATE_IPHONE_PREVIEW_2026-10-16.md).

**New source-only dependency checkpoint:** [PR #25](https://github.com/ethan-bailey/crisis-response-port/pull/25) merged original `ArmController.cs` developer source unchanged with SHA/physics provenance; does not change the above compiled game or create fake arm playback. Next: [issue #26](https://github.com/ethan-bailey/crisis-response-port/issues/26), proper original arm `Rigidbody`/`HingeJoint`/PhysX adapter, then original source controller callbacks in [issue #22](https://github.com/ethan-bailey/crisis-response-port/issues/22). No 1:1 game-parity claim.

## Historical plan (prior to branch reorganization)

## Current execution order — reviewed 2026-10-16

[NEXT_AGENT.md](NEXT_AGENT.md) is the authoritative current status and ordered next actions. Dated reports and the work board below describe evidence and scope, not competing “next” instructions. The user confirmed the original C#/.NET WASM route is the priority. Review existing package/source-audit work, compile bounded original animation/gameplay dependencies, then verify the matching WASM in the browser. Native Unity validation is separate and does not gate this work. The selected-source .NET/PhysX checkpoint remains preserved. Full animations, all maps, AI, missions and original UI are unfinished.

Native Marksman Unity compilation **succeeded** in run 38800542438, but only logs were retained and actual browser gameplay is unverified. SDK 8 package run 38849201407 is compiled/package-verified, not freshly browser-tested or deployed. UI audit run 38892941728 identifies all 24 original UI types, not UI execution. Follow the exact commits, artifacts and limits in NEXT_AGENT.md.

Latest **integrated and verified** WASM expansion: original C# arm-sequence classes and 7 source prefabs from PR #21 integrated to the PR #9 branch in `c110c738646f48f793a1e35822639676b0b21975`; the combined actual SDK 8 WASM, native C# and Chromium/WebKit browser test [#38901664939](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38901664939) **PASSED** (test input SHA `1264c0b4579076b82e1b6b21955469313779fe39`). Private complete site `12046137813`, evidence `12046012744`. No original animation controller playback yet. **Keep a single executable integration branch and short-lived source feature batches.** See [integration workflow](docs/INTEGRATION_WORKFLOW.md).

## Objective and nonnegotiable rules

Reach the closest **verified 1:1 behavior and presentation** to the original Crisis Response game in an iPhone Safari browser. Preserve the game's recovered developer systems, original source assets and original numerical behavior; keep any C#/WASM and Rust compatibility work confined to required engine adapters. **Never create stand-in gameplay, speculative visuals, fabricated effects, or imply parity from a compile alone.** Native Unity results inform route validation and reference evidence; a compile alone does not establish parity.

- **Browser code ownership (current):** `develop/wasm` is the executable integration branch; PR #9 and `codex/projectile-material-impacts` are preserved historical source/code references.
- **Independent Unity feasibility:** branch `codex/iphone-input-parity-foundation`, PR #7; keep separate from WASM. `main` accepts deliberately reviewed release promotions/governance, not normal gameplay commits.
- **Previously hosted owner-private v8:** https://crisis-response-runtime.craftysnipe13.chatgpt.site — **does not include the latest tested source glass/impact renderer updates**.
- **Latest verified browser build (2026-10-15 local):** [GitHub Actions #38846445335](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38846445335), code/workflow SHA `0933f2d4f5c5047f4e4a7f7c701fb13fd0f47e8c`, private compiled game/screenshots artifact **`12024020540`**. Native C#, .NET WASM, PhysX, Chromium and WebKit original movement/touch/camera/weapons/glass checks passed, including original particle source-manifest startup and explicit no-substitute renderer assertion. Separately, [native impact event contract #38847238826](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38847238826) **SUCCESS** at SHA `12f292657267d0c0521f00140c4ddec1976b7cd2`, artifact `12023701591`, verifies original `BallisticMaterial.SpawnEntryEffect` transport GUID/phase/time. Neither run is proof of original Unity particle visuals, live iPhone validation, or full-game parity.
- **Source-backed window audit:** [runs 38808368685 and 38808663563](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38808663563); 56 glass + 12 mirror-glass native window Quads. Exact shader parity and actual-device test are outstanding.
- **Shader recovery gap:** [original shader audit run 38812611701](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38812611701) proved both Shader Forge glass/mirror source files are AssetRipper `DummyShaderTextExporter` output, not original compiled shader behavior. Full visual parity blocked pending native reference or compiled shader recovery.
- **Original particles inventory:** [original source audit run 38812872182](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38812872182) passed, 26 ParticleSystems and 26 ParticleSystemRenderers across 14 entry/exit effect prefabs; private artifact `12008823420`. Not yet simulated/rendered.


## Full compile-to-release checklist

**Acceptance checklist (execution order is in NEXT_AGENT.md):** [docs/COMPLETE_WASM_PORT_ROADMAP.md](docs/COMPLETE_WASM_PORT_ROADMAP.md). It defines Gates 1–8, actual known-working .NET/PhysX build commands, full original game scope, native reference checks and private iPhone release criteria. **WASM compilation is already passing for the current source subset; 1:1 game completion is not.**

## Newly recovered original impact particle data

The browser branch now preserves the **exact original** 14 impact effect-prefab settings (26 ParticleSystems/26 renderers, 14 billboard + 12 mesh), `game/assets/ballistics/original-particles-source.json.gz` (SHA256 `75bbdbeadf3595f9c8fa32563a689fc119d6747abe0efb6c5bd0c347cd70cf92`), and all three source mesh assets under `game/assets/ballistics/particle-meshes/`. Mesh recovery [run #38815708747](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38815708747) succeeded at asset commit `8c713bf3797098a7988a3a142b33ce8d62b635e4`, evidence artifact `12009763698`. Three source-mesh fidelity tests passed; Splinter's packed native normals were subsequently decoded from the original sign-bitstream and passed private integrity run #38816798907. Original shader behavior and native Unity ParticleSystem execution remain incomplete. See [the full checkpoint](docs/WORKSPACE_STATUS.md), [issue #11](https://github.com/ethan-bailey/crisis-response-port/issues/11).

## Work board — scope and acceptance, not execution order

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

1. Read `WORKSPACE.md`, `NEXT_AGENT.md`, `docs/WASM_ORIGINAL_WINDOW_VISUALS_2026-10-15.md`, and the relevant linked issue. Inspect the current **branch HEAD**, active tests and CI status before modifying anything. The user reports no other active agent; still protect against changed refs.
2. Map every proposed change to a recovered original source file, serialized field, GUID, or evidence artifact. Record unresolved dependencies explicitly before implementing. Do not modify `runtime/Original/` with fabricated behavior.
3. Implement in small source-preserving commits using SHA-checked writes on the browser branch, or a documented isolated experiment branch as specified in NEXT_AGENT.md. Keep test assertions next to adapters. Do not rewrite the viewer or switch engines without verified need.
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
