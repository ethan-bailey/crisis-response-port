# Current handoff — 2026-10-25

## Exact verified checkpoint

- Runtime source: `7739bbccc7583e09461c294975d77612c1499492` on `develop/wasm`.
- Exact detached Release publish succeeded: `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release -o /tmp/cpr-publish-7739bb`; zero errors. Optional `wasm-tools` optimization unavailable.
- Gates at that source: Node **85/85**; Python **39/39**; Mission `--browser-entryteam-session-contract` passed; EngineChecks **69**; FoundationChecks **121**.
- Mission contract now calls the original `WeaponBase.EmitMuzzleAudio(true)` callback and verifies both original clip mappings, one-shot semantics, bounded volume, and source-time alignment. A controller fire input still produced **0** audio events in the one-step headless check; the real user input → weapon callback path is not yet proven.
- An exploratory direct `StrikeFiringPin()` reaches the original `EmitMuzzleSmoke` and fails because the source `ParticleSystem` simulation/render driver is unbound (`System.NotSupportedException: Original particle simulation and render driver is not bound`). Do not suppress or fake this effect to make a shot test pass. Original player fire is gated by `rightArm.OnTarget`; trace arm readiness and input sequencing next.
- Exact package built from source `7739bb...` and passed package `verify`: manifest SHA-256 `bbb7a1184dfe7e4041e27ce139dbe72ff1335aaf9189d26671b71293cca51755`; **2,674** files, 199 Blazor resources, 195 WASM files, 1,919 source assets; 97,767,549 compressed / 205,514,345 source bytes. Package output: `/tmp/cpr-site-package-7739bb-retry`.
- Site tests **36/36**; streaming archive audit verified all 2,674 files, sizes, and digests with no missing/unexpected entries. Artifact repo commit `55b911ff6d55542cf9609723c2e288f45fe08f63`, archive SHA-256 `b6288693d8f68aae128b76c92a76838c4d9d1d648d01e6b2d6353c2fc128a4fc`.
- Exact source package deployed privately as Site version **55** (`appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_ae15aba1e49881919e7f6d07e0b2eb57`), deployment `appgdep_6ade4a97891c8191a18ebd5722b1ca14`, URL `https://crisis-response-runtime.craftysnipe13.chatgpt.site`. This deploy contains source `7739bb...`; subsequent docs-only commit does not alter the deployed payload.
- iPhone Safari and real browser acceptance are outstanding. Previous reports of slow load tail, stall near asset 1,900, and delayed/loud gunshot require owner retest against version 55 with the bounded diagnostic report. No Playwright/browser binaries are installed in this runner.

## Remaining gates — dependency order and batches

### Batch A — source weapon fire chain (next)

1. Instrument the Mission source contract to report right-arm solver `OnTarget`/transition state before and after original input press/release, weapon mechanism readiness, and each source fixed step. Determine why input does not reach `Body.PullTrigger()`; do not synthesize a queue event as proof of firing.
2. Bind the original particle-system simulation/render driver, including authored muzzle smoke, as a source-backed runtime dependency; then drive `StrikeFiringPin()` and assert projectile, recoil, smoke, muzzle audio, and crack occur once with aligned source time. If renderer is a separate subsystem, implement a compatible event adapter that preserves every authored particle parameter and verifies browser presentation; do not drop the effect.
3. Add reload/bolt and weapon-state contracts through original actor callbacks. Keep default page transport weapon host enabled until source input + firing + effects parity contracts pass; then migrate input/weapon ownership in one batch to avoid dual authority.

### Batch B — delivery and iPhone acceptance

4. Owner retests private version 55 on iPhone Safari. Save total load time, asset count and stall interval, memory sample, fire latency/loudness immediately after load and after extended play. Fix measurable failures and rebuild from the exact tested source before publishing.
5. Revisit per-asset transfer and near-1,900 stall using report timings; optimize only the measured slow stage while preserving bounded concurrency, ordered commit, cancellation, and fail-closed hashes. Rebuild, verify package and archive, and deploy only after all exact-source gates pass.

### Batch C — parity closure

6. Migrate normal page movement/input, physics, rendering and weapon callbacks to one FullSource actor in dependency order; add contracts per callback and remove browser-owned duplicates only after parity evidence.
7. Close native physics/projectile-hit/physiology/ragdoll/joint, NPC behavior, authored animation/IK, and source effects/presentation gates with source contracts plus browser/device evidence. Isolated checks do not count as native/render parity.
8. Re-run all source, browser, Site, package, archive, and device gates at one exact source SHA. Update this file with checksums and provenance. Publish only the package verified from that SHA.

## Immediate next action

Work Batch A item 1 in a clean worktree based on the current branch head. The existing `runtime/Checks/Program.cs` headless Mission contract and the actual `PlayerController` / `CharacterBody.PullTrigger()` / `ArmIKSolver.OnTarget` code are the entry points. Record solver transition and trigger gate per fixed step, run the exact Mission contract, then implement the smallest source-faithful correction. Push tested code and update this handoff before proceeding to the particle driver.

Detailed release and historical evidence: `docs/evidence/RECOVERED_SOURCE_AND_EXACT_SITE_PACKAGE_2026-10-25.md`.
