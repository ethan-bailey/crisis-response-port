# Current handoff — 2026-10-25

## Exact verified checkpoint

- Branch: `develop/wasm`.
- Game source commit: `b159d007ccd4574c8d7f420388631ae31b40d916`.
- Source tree was built from a detached worktree at that exact commit. Release
  `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release`
  succeeded with zero errors; optional `wasm-tools` optimization was unavailable.
- Regression gates at this checkpoint: Node **85/85**, Python **39/39**,
  Mission source actor contract passed, isolated EngineChecks **69** passed,
  FoundationChecks **121** passed. Mission contract covers authored UI,
  original input, analog movement force and speed restoration, absolute aim,
  bounded source audio callbacks, and PhysX position/velocity round trip.
- FullSource AudioSource callbacks now flow through a 64-event bounded queue
  into the browser Web Audio dispatcher. Each event carries original clip path,
  volume, and source time. The real M4 fire clip path
  `Assets/AudioClip/5.56x45mm-01-unsuppressed.wav` matches the browser
  `audioFire` manifest path. Suspended, unloaded, or stale shots are discarded;
  unknown clips and non-one-shot operations are counted rather than guessed.
  This avoids delayed catch-up gunshots, but iPhone loudness/latency acceptance
  remains owner-run.
- Exact package manifest SHA-256:
  `157efaf1b376dacb88fbadf973c10301bbb5564dd8078996d986f2d51478a7c5`.
  Independent package verification passed: **2,674 files**, 199 Blazor
  resources, 195 WASM files, 1,919 incremental source assets; 97,767,549
  compressed bytes / 205,514,345 source bytes. Package integrity check compared
  every package file and digest.
- Site artifact source commit: `684ffbddc2d9fe1846563105407ab1c231b42de8`.
  Site tests passed **36/36**. Archive SHA-256:
  `0dbfc4fdc6f24941106a5f6c3ad88511b6132431754a379126bfee95d04c2052`.
  Streaming archive audit matched all 2,674 files with no missing, corrupted,
  or unexpected payload entries. Deployment is private Site version **54**,
  `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_3423555d530c8191b12d12169f7b776c`,
  deployment `appgdep_6ade483c9bf88191b3b227310295e96f` (succeeded):
  `https://crisis-response-runtime.craftysnipe13.chatgpt.site`.
  Before publishing, Sites confirmed owner role, custom access, one allowed
  user, and no groups; audience settings were unchanged.
- Package recovery note: the first exact-tree package attempt found the
  generated PhysX adapter missing. `npm ci --prefix game --ignore-scripts`
  and `npm run --prefix game vendor` restored lockfile-pinned
  `physx-js-webidl@2.7.3`; the package then verified successfully. No source
  files were changed for this setup dependency.
- Full evidence: `docs/evidence/RECOVERED_SOURCE_AND_EXACT_SITE_PACKAGE_2026-10-25.md`.

## Remaining gates, in dependency order

1. **iPhone Safari acceptance:** Owner opens the private Site, taps Start once,
   and records the delivered asset count, total time, any stall range, and
   memory report. Test gun audio immediately after load and after extended
   play, recording timing and loudness. Desktop/browser binaries and Playwright
   are unavailable in this runner; this does not block source or package work.
2. **Default page actor ownership:** Current FullSource bridge is opt-in. The
   normal page still advances browser movement and the transport weapon host.
   Migrate controls and weapon callbacks to the same FullSource actor without
   keeping duplicate authorities; add exact Mission contracts per migrated
   callback before changing the default path.
3. **Complete FullSource audio parity:** One-shot playback is bridged. Source
   loops, stop/state, mixer routing, spatial attenuation, and non-weapon actor
   clips remain unsupported and are measured in `unmappedSourceEvents` rather
   than approximated. Trace source call sites and asset mappings; implement only
   source-backed behavior and extend contracts.
4. **Gameplay and presentation parity:** Native PhysX solving, projectile to
   limb/hit callbacks, physiology, ragdoll/joints, NPC behavior, original
   animation/IK, and rendered source effects still need source-backed runtime
   contracts and browser/device evidence. Keep replacement rendering/physics
   boundaries explicit; do not mark native parity from isolated API checks.
5. **Release closure:** After each source batch, push `develop/wasm`, test that
   exact SHA, publish Browser Release, build and independently verify the
   package and archive from it, then privately deploy. Keep source and Site
   provenance hashes linked in this file.

## Next action

Trace original weapon `AudioSource` call sites and captured events from a real
FullSource fire/reload input. Add a contract that exercises the actual gun
callback (not a synthetic queue injection) and proves the clip mapping,
volume, timestamp, and immediate/no-late playback disposition. In parallel,
continue the next independently testable source-owned weapon callback. Defer
only device/browser gates that this runner cannot execute, record the exact
blocker, and continue code work.
