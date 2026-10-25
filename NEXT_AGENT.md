# Current handoff — 2026-10-25

## Verified checkpoint

- Development branch: `develop/wasm`; exact pushed game source SHA:
  `f7d96a413f05c7106c9f57de4c646cdbef3309b3`.
- The current batch connects the eased touch-stick walk magnitude to the
  original FullSource `CharacterBody` movement cap. The browser passes the
  bounded magnitude to the bridge; the bridge retains it across source fixed
  steps, restores authored walk/sprint speeds in `finally`, and exposes the
  active cap in its snapshot. The Mission contract verifies a 0.5 magnitude
  produces a sub-60%-of-full-walk acceleration and preserves authored speed.
- Exact Mission UI/source actor contract passed, including original input
  press/release, movement force, analog movement magnitude, and PhysX position /
  velocity round trip. Browser input tests already verify vertical stick aim
  stays unchanged when horizontal movement changes and that horizontal stick
  does not change the aim axis.
- At `f7d96a4`, Node checks passed **81/81**, Python checks **39/39**; Browser
  Release publish succeeded with zero errors. The optional wasm-tools optimizer
  is unavailable in this environment. Package build and independent verify
  passed at the exact source SHA; manifest SHA-256:
  `09c660d5e426f656c95fea1daf31363371e8e3b8e689a42a5b01e8a2084e89f2`.
- Exact package: 2,673 files, 199 Blazor resources, 195 WASM files, 1,919
  profile assets; 97,767,549 compressed asset bytes and 205,514,345 source
  bytes. Separate archive audit verified every packaged file's size and hash.
- Site artifact-repository commit:
  `563963f14b613eb85df2992e35db1984e66e6cf2`; archive SHA-256:
  `e803b0e5a21eac7c42d2a5cd70d98a7284ac464a14aa37abab3840a1858e2396`.
  Exact package is deployed privately as Site version
  `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_1b4447e9d2608191a24b97b6ad379ee0`,
  deployment `appgdep_6ade425248d88191b7d5129cc459a5c5` (succeeded). The Site
  remains custom/owner-only (one user, no groups); its audience was not changed.
- Full checkpoint evidence is in
  `docs/evidence/RECOVERED_SOURCE_AND_EXACT_SITE_PACKAGE_2026-10-25.md`.

## Remaining gates — dependency order

1. **iPhone acceptance (owner-run, independent of source work):** on the
   private Site, tap Start once, let the Mission load, and use Copy report.
   Capture whether asset 1,919 arrives, total time, stalled asset range, and
   memory. Diagnostics now separate fetch/hash, base64, and WASM bridge time.
   Do not treat the earlier “Warning: truncated output” image as a Site error;
   it was the ChatGPT display truncation banner.
2. **Finish page-to-source actor ownership:** continue auditing the opt-in
   source mission's input, absolute aim, fire/reload, movement, physics, pose,
   and effects paths. Keep normal startup and its transport weapon host
   separately identified until the actor integration is complete. Add focused
   source-contract evidence for each migrated edge.
3. **Audio playback parity:** the page's current Web Audio adapter drops stale,
   unloaded, or suspended one-frame events and clamps output through a gain and
   compressor. The FullSource Mission harness itself deliberately binds silent
   audio drivers. Preserve the user's delayed / excessively loud / unsynced
   iPhone report as an acceptance case; after the current actor ownership
   section, wire the original actor callbacks into the bounded browser audio
   path and verify that exact failure case.
4. **Gameplay parity:** native PhysX solve, rendered pixels, animation/IK,
   source NPC/component coverage, projectiles and original effects still need
   their own evidence. Playwright/browser binaries and iPhone automation are
   not available here; use source tests/builds in the meantime and defer only
   the device-dependent acceptance.
5. **Release closure:** after any source edit, commit and push to
   `develop/wasm`, run the regression gates, publish Browser Release, rebuild
   and independently verify the exact package, then refresh the Site archive
   from that package. Deploy only after source and archive integrity pass; keep
   the existing owner-only audience.

## Next action for the scheduled continuation

Add a source-side absolute-aim contract: set a fixed world aim point, tick the
source actor while moving left/right and with different browser mouse X values,
then assert the original `WeaponPointer.WorldPoint` remains the chosen absolute
point. If the contract exposes an overwrite, fix the ordering and re-run the
Mission UI and input contracts. Regardless of owner-run iPhone acceptance,
continue the next independent source-actor ownership edge; do not stop at a
deferred gate.
