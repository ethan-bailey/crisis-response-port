# Recovered Mission source and exact Site package — 2026-10-25

## Recovery inputs

- Recovery archive: `/workspace/scratch/original-recovery-artifact.zip`.
- Archive SHA-256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
- It contains the Unity source files for `Assets/Scenes/Mission 01.unity`,
  `Assets/Scenes/Marksman range.unity`, Global Context, and Operator. The
  earlier handoff incorrectly described this archive as lacking the original
  scenes. Its useful source tree is `export/ExportedProject/`.
- Pinned SDK archive SHA-256:
  `bb691c98abc8797eedbd48d3a766bc734214aed746e52e2691d8bbb268c60764` for
  .NET SDK 8.0.426. `scripts/env/procstat-namespace-shim.c` was built as
  `/tmp/libprocshim.so`; `LD_PRELOAD` enabled the pinned CLI in this runner.

## Rebuilt index and profile identities

The regenerated source index was `/tmp/cpr-scenes-full-source.json`,
SHA-256 `92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb`,
251,585,827 bytes. It contains 2,060 assets, 40,139 records, 24,856
references, and 776 binary assets. The prior delivery note records a different
source-index SHA (`6586b293…`). That identity discrepancy is preserved here:
the current index and profile were verified internally and are not represented
as byte-identical to the prior generated index.

Using `scripts/package_incremental_source.py` and roots Global Context,
Interior Mode UI, Interaction Menu, Operator, Version, and Mission 01 produced:

| Measurement | Result |
|---|---:|
| Assets / records | 1,919 / 22,365 |
| Compressed asset bytes | 97,767,549 |
| Source asset bytes | 205,514,345 |
| Largest source / compressed asset | 14,858,177 / 3,108,968 bytes |
| Metadata source / compressed bytes | 6,912,902 / 851,251 |
| Metadata SHA-256 | `b610de818b5b99a4a011efbbc10ae5dc50cd3ad806fe5d386284a62e57ff5450` |
| Metadata gzip SHA-256 | `2cdfbc1251a6e7be503f84cad15348923f7a79dbb521202dc28f4674dfd77399` |

The ordinary five-root Global/UI/Operator/Version profile is 1,230 assets,
36,971,121 compressed asset bytes and 77,806,659 source bytes. It does not
include the Mission 01 scene closure; use all six roots for the Mission UI
contract and full Mission package.

## Verification at runtime source SHA `f30f36e2d99661aaca221d529b1c831c9dda2d7a`

The exact package was rebuilt at documentation checkpoint
`4c219170c30035433357480419b652ef5e7633fc`; runtime and game source were
unchanged from the tested runtime source SHA above.

- Runtime source-profile verifier passed for 1,919 assets and 22,365 records;
  lazy parsed documents were released and rehydrated as expected.
- `--mission-ui-incremental-contract /tmp/cpr-incremental-mission-profile-v3`
  passed. It started Mission 01, initialized the authored UI, loaded original
  actors, and passed the input/actor bridge checks. This is headless evidence;
  it does not establish physical-device performance, native PhysX or pixels.
- EngineChecks and FoundationChecks passed. Runtime Release build completed
  with 0 errors and 48 warnings. Browser Release publish passed; wasm-tools
  optimization was unavailable and not required for publish success.
- Before packaging, `npm ci --prefix game --ignore-scripts` and
  `npm run --prefix game vendor` generated the lockfile-pinned PhysX adapter.
- `scripts/package_wasm_site.py build` and separate `verify` passed for
  `/tmp/cpr-exact-mission-site-pathmatched-4c21917`, recording source commit
  `4c219170c30035433357480419b652ef5e7633fc`, 2,673 files, 199 Blazor boot
  resources, 195 valid WASM files, and the 1,919-asset incremental profile.
  Its profile name `global-ui-mission-operator` exactly matches the URL requested
  by `game/index.html`. An earlier integrity-valid package used a different
  profile name and was not accepted for release.
- A separate package for the five-root profile also passed: 1,984 files, 199
  Blazor resources, 195 valid WASM files.
- Sites confirmed the existing Runtime Site remains custom/owner-private. Its
  artifact source repository reports branch `main` on a separate Cloudflare
  artifact repository. No Site deployment or browser/iPhone test occurred in
  this run; use the supported Sites source workflow to align an archive-backed
  Site version before deployment.

## Transfer interpretation and next investigation

`scene-data/scene-package-loader.mjs` currently downloads bounded batches of
six compressed assets in parallel, then verifies and delivers each asset to
the C# store in manifest order. The full Mission profile is still about 98 MB
compressed. This proves profile and package integrity, not that the slow tail
or reported pause near asset 1,900 is fixed. The next acceptance is an exact
iPhone Safari run with per-asset duration/progress and memory captured. If it
reproduces the pause, inspect which requests dominate and optimize that measured
path while preserving cancellation, bounded memory, order, and fail-closed
hash checks.
