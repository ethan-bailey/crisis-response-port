# Crisis Response — iPhone Web Port Research

This repository tracks a source-preserving, evidence-driven effort to make the original *Crisis Response* playable on iPhone 16 through a web runtime if feasible.

## Active project workspace (updated October 15, 2026)

**[Open WORKSPACE.md](WORKSPACE.md)** for the private source-preserving project board and [the full compile-to-release roadmap](docs/COMPLETE_WASM_PORT_ROADMAP.md) for all remaining original game systems, dependency ordering, **eight verification gates**, and the cloud compilation commands. [Current evidence, blockers and rollback status](docs/WORKSPACE_STATUS.md) and [agent handoff](NEXT_AGENT.md) are persistent.

- Browser draft work [PR #9](https://github.com/ethan-bailey/crisis-response-port/pull/9) preserves recovered original gameplay and assets; **latest verified selected-source C#/WASM/PhysX/Chromium/WebKit build passed** at [run 38809439277](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38809439277). This is an incomplete game, not 100% parity.
- An [owner-only playable older build](https://crisis-response-runtime.craftysnipe13.chatgpt.site) exists; it **does not include** the latest tested glass renderer. The completed build artifact for PR #9 has not been privately deployed or tested on actual iPhone hardware.
- The original Shader Forge window files exported by AssetRipper are **dummy shader replacements**. Mirror/light/refraction shader parity is blocked pending actual reference; native particle module inventory is complete but particle rendering remains unfinished.
- The Unity feasibility branch and `main` are separate; the user requires no invented gameplay subsystems, no public distribution and selective non-destructive rollbacks.

### Active tasks

[Window shader parity #10](https://github.com/ethan-bailey/crisis-response-port/issues/10) · [Particles/fragments #11](https://github.com/ethan-bailey/crisis-response-port/issues/11) · [Physics/physiology #12](https://github.com/ethan-bailey/crisis-response-port/issues/12) · [Private iPhone release #13](https://github.com/ethan-bailey/crisis-response-port/issues/13) · [Full six-scene assets #14](https://github.com/ethan-bailey/crisis-response-port/issues/14) · [AI/doors/missions #15](https://github.com/ethan-bailey/crisis-response-port/issues/15) · [UI/saves #16](https://github.com/ethan-bailey/crisis-response-port/issues/16) · [Native parity harness #17](https://github.com/ethan-bailey/crisis-response-port/issues/17) · [Reproducible protected release #18](https://github.com/ethan-bailey/crisis-response-port/issues/18).

The historical reconnaissance below reflects earlier milestones and is superseded by the active workspace status.

## Important distinction

The original archive has been extracted and inventoried in the current working environment. See [the verified archive inventory](docs/ARCHIVE_INVENTORY.md).

A separate generated file named `CrisisResponseMobile_iPhone.zip` is a from-scratch HTML canvas adaptation. It explicitly does not contain the original Windows executable or original game assets. It is not a faithful port and must not be represented as one.

## Start here

- [Non-negotiable original-game parity requirements](docs/ORIGINAL_GAME_PARITY_REQUIREMENTS.md) — original code/assets and verified behavior are mandatory; inspection demos are not accepted as a completed port.
- [Verified recovery results](docs/RECOVERY_RUN_2026-10-14.md) — the matching game assembly was decompiled and AssetRipper recovered a 7,231-file Unity project.
- [Cloud WebGL build gate](.github/workflows/unity-webgl-build.yml) — downloads the verified recovery artifact and attempts a licensed WebGL build; Unity licensing and mobile parity tests remain required.
- [Unity license gate instructions](docs/UNITY_BUILD_GATE.md) — explains the missing build activation requirement and how to configure repository secrets without sharing credentials in chat.
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
- A partially playable, source-backed browser WASM build now passes scoped Chromium/WebKit tests, but the complete six-scene original game has NOT been parity-verified or tested on actual iPhone hardware. Historical static scene analysis reached 417 readable serialized assets and 708 dependency edges, with one unresolved serialized asset GUID in the .308 casing/projectile prefabs. A separate headless Chromium test rendered one original combined mesh; this is not a playable game or a WebAssembly engine replacement.
- The WebAssembly engine-replacement plan is now tracked in [issue #4](https://github.com/ethan-bailey/crisis-response-port/issues/4).

The goal is to preserve original logic and assets wherever possible, document unavoidable changes, and validate behavior subsystem by subsystem. Do not claim full parity without evidence and tests.
