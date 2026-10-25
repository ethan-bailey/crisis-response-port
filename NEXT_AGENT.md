# Current handoff — 2026-10-25

## Checkpoint

- Branch: `develop/wasm`; current pushed source SHA:
  `d65f867d0dca48205b3b50e6133b76251fc189ba`.
- This source adds per-asset transfer and WASM bridge timings, visible transfer
  stages, a top-slowest-assets report, and a copyable diagnostic report. Exact
  package and Site archive were verified and deployed to the existing private
  Site as version 50. No Site audience change was made.
- Full source identities, package hashes, Site source commit, deployment ID,
  and tests are recorded in
  `docs/evidence/RECOVERED_SOURCE_AND_EXACT_SITE_PACKAGE_2026-10-25.md`.
- The slow tail and iPhone Safari acceptance remain unmeasured; diagnostics
  now identify whether network/hash or WASM interop time dominates. Keep the
  transfer budget and integrity checks until device data supports an
  optimization.

## Gates closed

- Recovery archive SHA-256:
  `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
  It contains original Mission 01 and Marksman range scenes, Global Context,
  and Operator. Regenerated source-index SHA-256 is
  `92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb`; it
  differs from older expected index `6586b293…`, so do not claim those indexes
  are byte-identical.
- .NET SDK 8.0.426 was restored and used with the tracked process-stat shim.
  Runtime profile verification and the Mission UI incremental contract passed
  on the full 1,919-asset profile. EngineChecks, FoundationChecks, Runtime
  Release build, and Browser Release publish passed at runtime source SHA
  `f30f36e`; Runtime build reported 0 errors and 48 warnings. Optional
  `wasm-tools` optimization was unavailable.
- `python3 -m unittest discover -s scripts -p '*test.py'`: **39/39 passed** on
  `d65f867`.
- `node --test scripts/*test.mjs`: **81/81 passed** on `d65f867`.
- `scripts/package_wasm_site.py build` and independent `verify` passed on
  `d65f867`; package source SHA is exact, with 2,673 files, 199 Blazor
  resources, 195 valid WASM files, and 1,919 Mission profile assets.
- Final Sites archive SHA-256:
  `62d28c833f390d8ed0cc89bc0c0a513352f26850af8ac447dea7cc6c5a94fdf9`.
  A separate archive audit verified all 2,673 package manifest entries, every
  byte count and SHA-256, plus the Sites hosting manifest. The manifest records
  game source `d65f867`; the pushed Sites artifact-repository commit is
  `b59e6617e9252e431356ffcab55aac142274e719`.
- Site deployment succeeded: version
  `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_e330bf53211081918e941b4c257130cd`,
  deployment `appgdep_6ade3fd53780819188345cb08bc77bca`, owner-only URL
  `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.
- Exact default Site command `scripts/package_wasm_site.py build` with the
  recovered source package passed, followed by independent `verify`. It records
  source SHA `925fa4b…`, includes the exact requested path
  `scene-data/fullsource/global-ui-mission-operator/manifest.json`, has 2,673
  files, 199 Blazor resources and 195 valid WASM files, and verifies all
  1,919 profile assets. Profile totals: 97,767,549 compressed asset bytes and
  205,514,345 source bytes. This fixes a packager default that previously
  produced `global-ui-operator` with five roots, while the page requests
  `global-ui-mission-operator` and requires the Mission 01 scene root.
- The former `Warning: truncated output ...` screenshot text is a ChatGPT
  display-truncation notice, not a game runtime exception. The former JS
  `JsonException` is handled by sending one bounded base64 string per asset
  through Blazor interop.

## Remaining gates, in dependency order

1. **Measure iPhone startup with version 50.** On Safari, tap Start Mission
   once and leave the page open. The progress panel reports asset ranges and
   separates fetch/hash from WASM bridge time. Tap Copy report if the load
   completes, errors, or stalls; record memory and whether asset 1,919 arrives.
   Device acceptance is deferred to the owner, while independent code work
   continues.
2. **Close page-to-actor parity.** The opt-in Mission mode already routes
   touch/keyboard action levels through `FullSourceActorBridge`, feeds its
   source acceleration commands into the capsule physics adapter, sends the
   resulting state back to `CharacterBody`, and applies original weapon
   callbacks. Normal page startup still uses browser-side movement and the
   transport weapon host. Audit and migrate the remaining ownership edges in
   dependency order, with source-contract tests for each edge.
3. **Gameplay parity.** Verify native PhysX solve and rendered output, gun
   audio timing/volume, animation/IK, NPC/source component coverage, and
   projectile effects against original contracts. Index unsupported Unity
   components with dispositions.
4. **Release completion.** Rebuild and integrity-verify the exact final package
   after every source change, run the regression suites, and deploy only to the
   existing owner-private Site. Never widen Site access.

## Exact next action

Continue with the source-actor ownership audit while the owner runs the iPhone
startup check on Site version 50. Use that report to pick a measured transfer
optimization batch; do not label the current 98 MB delivery as a speed fix.
