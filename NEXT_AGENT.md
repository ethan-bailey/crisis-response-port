# Crisis Response Port — continuation handoff (2026-10-26)

## Current deployed checkpoint: Site v75

- Live URL: https://crisis-response-runtime.craftysnipe13.chatgpt.site
- Version 75: `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_3627e642babc8191baf0ba264e96a5af`
- Deployment `appgdep_6adf8880cd4c8191a4d16a6789d45c6a` succeeded. Owner-private access preserved.
- Exact Site source `main`: `d8dbe9f3c038e59afbf0d9903d5d036c1f3169b4`.
- Pinned .NET SDK 8.0.426. Release runtime build: 0 errors / 48 warnings. Release Browser/WASM publish succeeded; optional `wasm-tools` optimizer is not installed.
- `package_wasm_site.py build` and `verify dist` passed on that exact source: 2,686 files, 199 Blazor resources, 195 WASM files; Mission profile 1,919 assets / 22,365 records / 97,767,549 compressed bytes / 205,514,345 source bytes. Source package SHA-256 `ea3592ff5bdd39534bb905cd0abac74f456578b46eb4b12cdd0cb79fe8520cf7`; build-manifest SHA-256 `c1610bf34f29e38c7efc82f5f3cd6ff4cc7d8e6d1db694183850d50f2f801a52`.
- Deployment archive `/tmp/cpr-site-v75-deploy-d8dbe.tar.gz`: SHA-256 `ba24d8a315e4865ccc6d68cf925423fa59fa84d57462e7320778e8dbf42de2d8`, 138,792,202 bytes. Archive contains matching `.openai/hosting.json` and `dist/index.html`; independent extraction verified all 2,686 manifest file sizes and hashes. Sites normalized TAR SHA `6bc154b5299a354782a9bcc9ed7a479aca0693dea8903f3b4a3e454d20299f59`, 178,022,400 bytes / 2,688 files.
- Joystick fix: approved sprint entry/release distances are 1.75×/1.5×; implementation was corrected from 1.42×/1.24×. Tests cover 1.49× no sprint, 1.75× activation, 1.51× hysteresis, 1.5× release. Full Site Node suite 142/142 and targeted controls suite 33/33 pass.
- Recovered actor package audit passed with `python3 scripts/verify_runtime_package.py /tmp/cpr-recovery-full-source/export/ExportedProject /tmp/cpr-site-v73-full-source.json`: 2,060 assets, 40,139 records, 24,856 dependency edges; Operator/Occupant closures have zero unresolved GUIDs and unsupported builtins.
- Automated cloud browser cannot create WebGL (`GL_VENDOR=Disabled`); this is a runner limitation, not device acceptance.

## Next actions, in order

1. **Public source synchronization:** Push `game/browser-input.mjs`, `scripts/browser_input_test.mjs`, and this refreshed documentation to `develop/wasm` through the GitHub connector. Parent must be remote SHA `fe2982c1e0f30fc8617afd914586c6fee584497a`; update ref with expected-SHA compare-and-swap. Inspect exact-SHA Actions and resolve failures before building a public-branch package. Site v75 evidence belongs only to Site commit `d8dbe9f...`.
2. **Owner device acceptance:** On physical iPhone Safari, test v75 cold and warm Mission launch/persistence, late ~1,900-asset progress, vertical-only absolute aim, walk-speed control, resisted sprint wall, rapid-tap zoom, and fire/reload speaker output. Record what happens. Cloud Chromium cannot certify Safari/device behavior.
3. **Continue source parity gates regardless of device wait:** Use `docs/GATE_DEPENDENCY_INDEX.md` as the dependency-first inventory. Next source work is the original FullSource reload BehaviorTree handoff/cancel path; preserve protected `WeaponHost.cs`, and add fixture setup only in `runtime/Checks/WeaponContracts.cs`. Then progress distinct two-actor hit/injury/physiology, original IK/animation, mission/NPC, native physics/rendering and combined acceptance.

## Persistent constraints

- Keep SDK pinned to 8.0.426; use the working .NET + package workflow, not a speculative CLI/Codespaces detour.
- Do not publish unless exact source build, complete profile verifier and archive hashes pass for the same source SHA. Preserve Site owner-private access.
- Physical iPhone acceptance is a separately owned check; do not mark the software work “done” just because it is pending. Continue independent source gates and log concrete evidence.
- Keep `fullGameParity:false`, `sourceVisualParity:false`, `nativeParityVerified:false`, `originalGameComplete:false`, and `offlineComplete:false` until their required evidence exists.

## Evidence index

- Dependency order and closure requirements: `docs/GATE_DEPENDENCY_INDEX.md`.
- Exact v75 build, archive audit, actor audit and browser limitation: `docs/evidence/SITE_DEPLOYMENT_ALIGNMENT_BLOCKER_2026-10-26.md`.
- Full-profile cold/warm IndexedDB and audio evidence on public run #79: `docs/INDEXEDDB_FULLPROFILE_DEPENDENCY_CLOSURE_2026-10-26.md`.
- Publication controls: `docs/RELEASE_POLICY.md`.
