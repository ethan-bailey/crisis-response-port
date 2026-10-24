# Crisis Response — iPhone Web Port Research

> **Current collaboration setup (October 16, 2026):** `main` is the stable repository baseline, **[`develop/wasm`](https://github.com/ethan-bailey/crisis-response-port/tree/develop/wasm)** is the sole integrated C#→WebAssembly game branch, and new work uses short-lived feature PRs targeting `develop/wasm`. The game is still incomplete. The older project notes below describe historical milestones, not the current code integration branch.

## Work here

- **[Contributor onboarding](CONTRIBUTING.md)** and **[branching, PR, label and release policy](docs/BRANCHING_AND_COLLABORATION.md)** — the required conventions for human and AI contributors.
- **[Current next-agent handoff](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/NEXT_AGENT.md)** and **[workspace](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/WORKSPACE.md)** — what is integrated/tested, who owns what, and the next bounded original-code task.
- **[Issue #22: original arm-controller playback](https://github.com/ethan-bailey/crisis-response-port/issues/22)** — next priority source dependency group.
- **[Source-backed test milestone](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38901664939)** — combined original C#/WASM/Chromium/WebKit tests passed at SHA `1264c0b4579076b82e1b6b21955469313779fe39` (selected source slice only).
- **[Manual private Vercel release protocol](https://github.com/ethan-bailey/crisis-response-port/blob/develop/wasm/docs/VERCEL_MANUAL_DEPLOYMENTS.md)** — Git commits no longer trigger Vercel deployments.

The repository and its tracked source/history are public. Keep separately stored recovery archives and credentials protected. No invented gameplay, fake placeholders, public game deployment, or 1:1 parity claims without source verification.

---


This repository tracks a source-preserving, evidence-driven effort to make the original *Crisis Response* playable on iPhone 16 through a web runtime if feasible.

## Important distinction

The original archive has been extracted and inventoried in the current working environment. See [the verified archive inventory](docs/ARCHIVE_INVENTORY.md).

A separate generated file named `CrisisResponseMobile_iPhone.zip` is a from-scratch HTML canvas adaptation. It explicitly does not contain the original Windows executable or original game assets. It is not a faithful port and must not be represented as one.

## Start here

- [Non-negotiable original-game parity requirements](docs/ORIGINAL_GAME_PARITY_REQUIREMENTS.md) — original code/assets and verified behavior are mandatory; inspection demos are not accepted as a completed port.
- [Verified recovery results](docs/RECOVERY_RUN_2026-10-14.md) — the matching game assembly was decompiled and AssetRipper recovered a 7,231-file Unity project.


- [Reverse-engineering plan](docs/REVERSE_ENGINEERING_PLAN.md)
- [Verified archive inventory](docs/ARCHIVE_INVENTORY.md)
- [Subsystem parity matrix and porting status](docs/PORTING_STATUS.md)
- [WebGL compatibility audit](docs/WEBGL_COMPATIBILITY_AUDIT_2026-10-14.md) — static scan of the recovered project for likely browser build blockers.
- [WebAssembly runtime architecture decision](docs/WEBASSEMBLY_RUNTIME_ARCHITECTURE_DECISION_2026-10-14.md) — selected source-preserving C# WASM approach and implementation gates.
- [WebAssembly engine replacement plan](docs/WEBASSEMBLY_ENGINE_REPLACEMENT_PLAN_2026-10-14.md) — preserve original game logic and define parity gates.
- [Unity API compatibility matrix](docs/UNITY_API_COMPATIBILITY_MATRIX_WASM_2026-10-14.md) — verified shim coverage versus outstanding engine services.
- [Browser geometry / WASM feasibility status](docs/WASM_RUNTIME_SPIKE_2026-10-14.md) — original Unity mesh decoding and Chromium/WebGL test results.
- [Marksman range scene dependency audit](docs/MARKSMAN_RANGE_SCENE_DEPENDENCY_AUDIT_2026-10-14.md) — recursive original asset dependency graph.
- [Original C# ballistics method in browser WASM](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38593586937) — original Target.cs, BallisticMaterialProperties.cs and BallisticMaterial.cs compiled; original channel-volume, surface clamp, impenetrable-entry and basic vector/quaternion/transform tests passed with explicitly limited engine fixtures.
- [Successful original mesh browser smoke run](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38589625850) — 9,455 original triangles rendered without browser console errors.
- [Browser port strategy pivot](docs/BROWSER_PORT_STRATEGY_PIVOT_2026-10-14.md) — evaluates WINE/PROTON-in-WebAssembly and source-port approaches so Unity activation is not the only path.

## Current state

- Original archive extracted: 415 files, 204,344,165 bytes.
- Verified engine/player: Unity 2017.2.0f3, Windows x86 Mono.
- Verified gameplay assembly metadata: 1,534 types and 8,640 methods in `Assembly-CSharp.dll`.
- Original C# source and editable Unity project structure are not present in the archive.
- Cloud recovery completed successfully using a signed official mirror. The downloaded gameplay DLL exactly matches the SHA-256 recorded from the user's archive; ILSpy produced 1,333 C# files and AssetRipper exported a 7,231-file Unity project. See the recovery report for the artifact link and remaining gates.
- No verified faithful browser build exists yet. Static scene analysis reached 417 readable serialized assets and 708 dependency edges, with one unresolved serialized asset GUID in the .308 casing/projectile prefabs. A separate headless Chromium test rendered one original combined mesh; this is not a playable game or a WebAssembly engine replacement.
- The WebAssembly engine-replacement plan is now tracked in [issue #4](https://github.com/ethan-bailey/crisis-response-port/issues/4).

The goal is to preserve original logic and assets wherever possible, document unavoidable changes, and validate behavior subsystem by subsystem. Do not claim full parity without evidence and tests.
