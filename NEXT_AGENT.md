# Current handoff — 2026-10-25

## Exact source and shipped site checkpoint

- Tested source commit: `b5b22b95b9793e9957efe632c2bb2f6cbc812ce9` on `develop/wasm`; the following documentation commit will become the branch head. Site artifact commit: `f7d9052e454c1dfcc50ab0ada54cf485a360b86f`.
- Runtime fix: only the Blazor first-render callback starts the runtime. The duplicate inline start was removed; otherwise two deliveries wrote into the same static source store and one failed with `WASM source store rejected asset ...` while progress continued. Regression is in `scripts/runtime_memory_check_test.mjs`.
- Memory-monitor fix: sampling waits until the source store exists, samples at most once per second, and forces a final sample on success or failure. The prior per-progress-callback sampling produced a 1.47 MB report and overflowed the hosted browser's accessibility frame.
- Exact source gates: Node **88/88** (`node --test scripts/*_test.mjs`); Python **39/39** (`python3 -m unittest discover -s scripts -p '*_test.py'`); EngineChecks **69**; FoundationChecks **121**; `--browser-entryteam-session-contract` passed; Release Browser publish succeeded. Existing compiler warnings remain. Optional `wasm-tools` optimization is unavailable.
- Mission contract covers original fire input/source actor callbacks, source audio mapping/gain, absolute aim, analog movement force and PhysX position/velocity round trip. Particle and casing outputs are recorder assertions; actual browser-rendered effects and physical casing trajectories remain open.
- Package manifest SHA-256 `664c70350007a664161f60ace5e3f63f33c4f413afa8769bcdf9748d866b8cfc`; source commit recorded is `b5b22b95b9793e9957efe632c2bb2f6cbc812ce9`. It contains 2,674 manifest files, 199 Blazor resources, 195 WASM files, and the 1,919-asset `global-ui-mission-operator` profile (97,767,549 compressed asset bytes; 205,514,345 source asset bytes). Package build and independent verify passed.
- Site tests passed **36/36**. The archive `/tmp/cpr-site-runtime-b5b22b9.tar.gz` has local SHA-256 `23c59002bdf24dc4e7c39214e2921e34627bfe3a3e0c4b809985de2e09c11800`. Extraction audit matched **2,674/2,674** manifest files by length and SHA-256, with zero errors. Only expected extra files: `dist/build-manifest.json` and `.openai/hosting.json`.
- Site version **60**, `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_f3b95faad99481918b8ab4e1b7d2fbeb`, deployed successfully as `appgdep_6ade57a06b9c8191b8d7117045458229` to `https://crisis-response-runtime.craftysnipe13.chatgpt.site`. Provenance is artifact commit `f7d9052`; private deployment preserved the owner-only/custom Site. Error-only Worker query for the first 20 minutes returned zero events; this does not prove a device session.

## Live memory smoke evidence

- Version 59 Chrome/Linux smoke completed 1,919/1,919 in **82.8 sec**, with zero sample errors. Its monitor produced **4,241 samples / 1,473,134 characters**; that report size caused the CUA accessibility frame limit. Peak managed bytes were 205,217,552. This was a diagnostic/report-size problem, not a source-delivery exception.
- Version 60 Chrome 155/Linux x86_64 smoke (8 GB device-memory hint) completed 1,919/1,919 in **73.786 sec**, `error: null`, zero sample errors, **67 samples / 27,486 characters**. Peak managed bytes 226,752,560, managed heap 209,453,056, JS heap 113,379,525. The host's `measureUserAgentSpecificMemory` API was unavailable. This is a hosted Chrome smoke, not iPhone Safari acceptance and not a WebGL gameplay test.

## Dependency-ordered remaining gates

### A — iPhone Safari acceptance and remaining input bugs

1. Ask owner to open `https://crisis-response-runtime.craftysnipe13.chatgpt.site/runtime-web/?source-profile-smoke&v=60` in iPhone Safari, leave it foregrounded until **Complete**, then tap **Copy report** and paste the report. This memory-only route starts the check automatically; no movement, aiming or firing is needed. Compare its elapsed time, tail/stall, sample count, sample errors and peak managed heap with the hosted Chrome results above.
2. Separately verify the normal Mission page on iPhone: tap **Start original Mission** once; check all 1,919 assets finish, character and controls appear, then test move/sprint, absolute aim, tap/hold fire and audio timing/volume. Also check rapid taps do not trigger iOS page zoom. Safari movement, aim, firing, sound and zoom acceptance remain open.
3. Use the iPhone report's `slowestFetches` and `slowestBridgeCalls` before changing the existing six-fetch bounded concurrency. Preserve manifest-order verified adds, cancellation and fail-closed checks.

### B — browser/render gate

4. Run the normal `/game/` visual smoke on a runner with Playwright Chromium and WebGL. Playwright resolves here but Chromium installation returned an invalid zero-byte archive; the connected Chrome reports WebGL disabled. Version 60's memory route does not exercise the game renderer.

### C — FullSource parity

5. Implement browser sinks for source particle emission and casing force callbacks, preserving authored parameters, source-time lifetimes and cleanup; demonstrate visible effects and casing motion. Current source contract uses recorders only.
6. Move remaining physics/render/input/audio ownership fully through the original actor in dependency order. The Mission path already binds `FullSource.CharacterBody`, sends original controls and projects its snapshot; browser contact physics still steps/synchronizes state. Preserve accepted joystick ease-in and wall-break sprint, aim independent of horizontal movement, and hold-to-fire.
7. Close projectile hit resolution, physiology, ragdoll/joints, NPC behavior, original animation/IK, and interaction/menu parity with source contracts plus browser/device evidence for visible behavior.
8. Before any later publish, build and test one exact source SHA, verify its package, site tests and extracted deployment archive, then preserve the owner-only Site access.

## Immediate next action

Get the owner's version 60 iPhone memory report for device-specific delivery evidence. In parallel, continue with the first feasible FullSource visual effect sink (particle/casing), then rerun exact-source gates and document its remaining dependencies. Do not stop at the external-device gate.
