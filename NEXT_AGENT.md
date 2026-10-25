# Current handoff — 2026-10-25

## Checkpoint

- Branch: `develop/wasm`; tested and pushed source SHA:
  `925fa4baf6abb56bc2e5e86eeaf1f903d2cf674a`.
- This commit fixes the Site packager's default incremental Mission profile and
  adds a regression test. Runtime/game code is unchanged from
  `f30f36e2d99661aaca221d529b1c831c9dda2d7a`.
- Full evidence, source identities, and build commands are in
  `docs/evidence/RECOVERED_SOURCE_AND_EXACT_SITE_PACKAGE_2026-10-25.md`.
- Do not treat the 98 MB Mission transfer as a performance fix. The slow tail
  near 1,900 assets and exact iPhone Safari startup remain unverified. Site
  access remains owner-private; nothing was deployed.

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
  `925fa4b`.
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

1. **Make a testable Site version.** Existing Crisis Response Runtime is
   owner-private. Sites reports its artifact source repository uses branch
   `main` on a separate Cloudflare artifact repository, distinct from the game
   repository's `develop/wasm`. Use the supported Sites source workflow to
   align the archive-backed source commit and exact package before deploying;
   do not pass the game SHA as that repository's commit.
2. **iPhone download/start acceptance.** Test exact pushed build on iPhone
   Safari, including progress through asset 1,900, completion, Start Mission,
   and memory. If the slow tail/pause persists, capture per-asset duration and
   memory, then optimize that measured path while preserving bounded memory,
   ordered delivery, cancellation, and hash checks. No Playwright browser
   binaries are installed here.
3. **Page-to-actor parity.** Normal page startup still uses browser movement and
   the transport weapon host. Continue wiring page input, physics, rendering,
   and weapon callbacks through the same original source actor; verify each
   slice with the Mission UI contract and exact package.
4. **Gameplay parity.** Verify native PhysX solve and rendered output, gun
   audio timing/volume, animation/IK, NPC/source component coverage, and
   projectile effects against original contracts. Index unsupported Unity
   components with dispositions.
5. **Release.** Build and integrity-verify the exact final package, obtain
   target-device acceptance, deploy only to the existing owner-private Site,
   then record the deployment/version identity. Never widen Site access.

## Exact next action

Use the supported Sites source workflow to align the current pushed game
checkpoint and verified Mission archive with the existing private Site's
archive-backed source branch. Then arrange the owner iPhone Safari startup and
throughput check. Do not label the current 98 MB delivery as a speed fix.
