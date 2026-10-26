# Current handoff — 2026-10-26

## Live checkpoint

- Repository `ethan-bailey/crisis-response-port`; authorized branch `develop/wasm`.
- Latest code commit: `12d61245431b3e3a026fbd82a8199f6e983a8590`, tree `7dd6c19db9afcf0d02077a11c5d106744b641b74`. Remote HEAD was `7d5874caf7743797964bac3a719d19e0b0e587d3` immediately before this documentation-only update; refresh before edits.
- Actions run #22 (`40263692936`) passed on exact code commit `12d612...`.
- Release WASM package rebuilt from that exact remote SHA and verified: 2,684 files, 199 Blazor resources, 195 WASM files, and 1,919 Mission source assets (97,767,549 compressed bytes; 205,514,345 source bytes). HTTP smoke returned correct entry pages and WASM MIME/hash. Evidence: `docs/evidence/REA-mission-cache-regression-fix-2026-10-26.md`.
- No usable browser interaction or physical iPhone Safari result is available. The cloud Chrome surface returned `net::ERR_CONNECTION_REFUSED` for a local HTTP server that answered from this workspace; no Chrome/Firefox/WebKit binary is installed in the runner. Keep `fullGameParity:false` and visual parity unclaimed.
- .NET SDK pin is 8.0.426. On this runner use `/tmp/codex-dotnet-8.0.426/dotnet` with `LD_PRELOAD=/tmp/libprocshim.so` as documented in `docs/DOTNET_BUILD_RUNBOOK.md`; do not reinstall or retarget.
- A prior handoff reports owner-private Site version 71 published from separate source before this run. This run made no Site/preview change. Current user instruction prohibits publishing/deployment; keep later publication separate and unauthorized absent a new explicit instruction.
- Preserve the unrelated untracked user artifact `source-reference/Original/Assets/Scenes/.Mission 01.unity.u4cmKY` if it appears in the primary checkout.
- Read `docs/CONTINUE_UNTIL_GATES_PASS.md` before returning; list gates and continue independent work after deferring exact external dependencies.

## Latest verified implementation

Live commits `3c17b11` and `59cb7b8` changed Mission asset delivery to largest-first, but also removed IndexedDB, integrity-checked cache recovery, quota/storage progress, the clear-cache export, and corresponding tests while `game/index.html` still imported the removed export. Commit `12d6124` restored the prior verified cache path and tests, then kept deterministic largest-first six-way fetch and bounded .NET bridge batches.

For the implementation tree:

- Focused loader/cache contracts: **7/7**.
- Full Node contracts: **129/129**. Fresh worktrees need the documented `npm ci --prefix game --ignore-scripts && npm run --prefix game vendor` generated PhysX adapter recovery.
- Python tests: **43/43**; source dependency inventory freshness passed (810 sources, 1,710 symbols, 1,384 reachable files, 127 ambiguous names still needing ownership review).
- EngineChecks **75**, FoundationChecks **121**, runtime contracts passed with `fullGameParity:false`.
- Added source-bound reload audio branch checks: the original detachable/internal magazine extraction and insertion paths, the chamber-empty early return, task cancellation cleanup, weapon-switch/late-fire interruption, and original internal-magazine return sound are checked directly against `WeaponBase.cs` and `ChangeMagazine.cs`. These are source-contract checks, not a dynamic reload simulation.
- Release WASM publish and exact package verification passed. Optional `wasm-tools` optimization was not installed.
- The first `dotnet --version` call reproduced the known `/proc` failure; the pinned SDK plus process shim succeeded with `8.0.426`.

## Remaining gates in dependency order

1. **Repository/reproducible build — PASS for code commit 12d6124.** Confirm the current remote tip and checks after each source change.
2. **Original source/package closure — PARTIAL.** Reconcile authored Operator, Occupant, scenes, weapons and mission dependencies, GUID/fileIDs, serialized fields, and built-in records against one exact profile. Spark source bytes remain unresolved: expected 1,539-byte SHA `6a28a6b773989684c38fd510bf8e49e66bfd5248f9b640c27639a5e99cb53344`; recovered profile and authorized Actions artifact `11903811738` contain only a 1,014-byte exported PNG with SHA `b33ee840...`. Next input is the exact original project export/archive or a source-owner copy matching the required hash. Do not substitute it.
3. **Mission asset delivery/local reuse — CODE CONTRACTS PASS; browser/device OPEN.** Exercise actual IndexedDB transactions, second launch, interrupted download/write, manifest invalidation, quota/eviction, cache status/clear and iPhone behavior at the former ~1,900-asset slowdown. Measure bytes, requests, concurrency, integrity time, memory and duration.
4. **Original scene/actor lifecycle — PARTIAL.** Load the authored Mission scene and Operator/Occupant prefabs in browser; prove original initialization/update/destruction and independent source ownership for CharacterBody, limbs, physiology, equipment, audio, mission, mask and presentation.
5. **Movement/camera/animation/IK — SOURCE CONTRACTS PASS; interactive/device OPEN.** Verify touch movement/sprint, absolute aim, aim-before-fire, sustained fire, arm IK and release behavior in interactive browser and owner iPhone Safari.
6. **Equipment/fire/hit/body state — PARTIAL.** Verify the original projectile through penetration, callbacks, injuries, hemorrhage, oxygen, shock, unconsciousness/death, ragdoll and weapon drop. Cover fast/repeated/cancelled/interrupted/empty-reserve/weapon-switch reload and audible source timing.
7. **Mission/NPC/render/effects — PARTIAL.** Verify actual spawn/standing, enemy interaction, transitions, wounds, production audio, particle lifecycle/modules and native visual comparison. Preserve `sourceVisualParity:false` until comparison; continue independent modules while Spark is unresolved.
8. **End-to-end web acceptance — PACKAGE/HTTP PASS; interaction OPEN.** Run actual package in Chromium/WebKit when a usable runner is found; exercise fresh/cached launch, interruption, manifest change, quota messaging and core gameplay. Physical iPhone Safari is owner acceptance.
9. **Exact-source closeout — PARTIAL.** Tie the complete focused/runtime/package/browser suite to one source SHA and retain logs/artifacts. Obtain owner device acceptance before any completion claim. Do not publish without separate explicit authorization.

## Next dependency-ready action

The latest Node suite is **129/129** after the reload source-contract addition; rerun the focused and complete verification on its pushed SHA. Continue particle dependency closure from `docs/MISSION_PARTICLE_DEPENDENCY_CLOSURE_2026-10-25.md` and `docs/mission-particle-dependency-inventory-2026-10-25.json.gz`. The packaged 14 impact prefabs have 14 active `ClampVelocityModule`s across three serialized configurations; trace Unity 2017.2's exact serialized magnitude/axis mapping and damping order before implementing a real runtime-path assertion. Current `magnitude.scalar=0` with `separateAxis=0` makes a guess unsafe. Independently locate a real Chromium/WebKit runner for IndexedDB acceptance. If runner or exact Spark source is unavailable, record attempted paths and evidence, defer only those acceptances, and continue source-backed gameplay work.
