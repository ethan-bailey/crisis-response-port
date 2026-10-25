# Current handoff — 2026-10-25

## Checkpoint

- Project branch: `develop/wasm`; starting source SHA for this verified batch:
  `f30f36e2d99661aaca221d529b1c831c9dda2d7a`.
- Worktree began clean. Live GitHub `develop/wasm` matched this SHA.
- No runtime source change was made in this batch. The source recovery, pinned
  .NET SDK, Mission contract and Site package gates that earlier handoffs called
  blocked were reconstituted and verified locally; details are in
  `docs/evidence/RECOVERED_SOURCE_AND_EXACT_SITE_PACKAGE_2026-10-25.md`.
- **Do not publish the current 98 MB Mission profile as a fix for the reported
  slow download.** Exact build/package integrity now passes, but user-reported
  slow tail near 1,900 assets and iPhone Safari startup acceptance have not
  been verified as solved. Keep Site access owner-private.

## Gates closed at source SHA `f30f36e`

- Recovered source archive SHA-256:
  `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
  The archive contains `Mission 01.unity`, `Marksman range.unity`, Global
  Context, and Operator prefab inputs. The regenerated full source index SHA is
  `92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb`, not
  the older index SHA `6586b293…` recorded in the original delivery evidence.
  Use the regenerated index's own identity; do not assert byte identity with
  the older index.
- Pinned .NET SDK 8.0.426 restored and selected with the tracked process shim.
- Mission 01 source profile: 1,919 assets / 22,365 records, 97,767,549
  compressed asset bytes, 205,514,345 source bytes. Integrity and C# incremental
  profile contract passed. The Mission UI incremental contract passed: Mission
  started, authored UI initialized, two original Operators and four original
  Occupants loaded, and left press/release and actor bridge checks passed.
- EngineChecks, FoundationChecks, Runtime build and Browser publish passed.
  Runtime Release build: 0 errors; 48 warnings. Browser publish succeeded
  without optional wasm-tools optimizations.
- `scripts/package_wasm_site.py build` plus independent `verify` passed for
  both the default 1,230-asset profile and the Mission 01 1,919-asset profile.
  The exact Mission artifact contains 2,673 files, 199 Blazor resources and 195
  valid WASM files; it records source commit `f30f36e…`. No deployment was
  performed.
- The UI failure shown as “Warning: truncated output ...” is a ChatGPT display
  truncation notice, not evidence of a game runtime exception. The prior
  `JsonException` for a JS byte array is handled in current source by sending a
  bounded base64 string through Blazor interop. Do not re-open either as an
  unresolved gate without new device evidence.

## Remaining gates, in dependency order

1. **Preserve exact-source package evidence on the pushed checkpoint.** Commit
   and push the evidence/handoff update, verify remote SHA, then rebuild the
   package against that resulting SHA because package inventory includes docs.
2. **Mission download/startup on target iPhone.** The 1,919-asset package still
   totals about 98 MB compressed (205 MB source). Have the owner test the exact
   pushed build on iPhone Safari, including progress through the former 1,900
   asset point, completion, start button, and memory. If tail latency/hang
   remains, collect a per-asset timing trace and optimize the transfer in a
   bounded, testable batch before publishing another build. No Playwright
   browser binaries are available in this runner.
3. **Page-to-actor parity.** The default page still has unfinished wiring for
   input, physics, rendering and weapon callbacks through the same source actor.
   Close each against the active Mission UI contract and exact source build.
4. **Gameplay parity.** Verify remaining native PhysX solve/render output,
   weapon/audio synchronization and volume, animation/IK, NPC/source component
   coverage, and projectile effects against original source contracts. Keep
   unsupported Unity components explicitly indexed with dispositions.
5. **Release.** Build and package the exact final code and source profile, run
   integrity verification, obtain target-device acceptance, then deploy only to
   the confirmed existing owner-private Site and record deployment/version
   identity. Never widen access.

## Exact next action

Commit/push this documentation checkpoint, verify the remote ref, and rebuild
and independently verify the Mission 01 Site package against that pushed SHA.
Then move to the target-device startup/throughput gate; do not count the 98 MB
transfer profile as a download-performance fix.
