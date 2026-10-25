# Recovered Mission source and exact Site package — 2026-10-25

## Recovery inputs

- Recovery archive: `/workspace/scratch/original-recovery-artifact.zip`.
- Archive SHA-256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
- It contains original Unity source files for `Assets/Scenes/Mission 01.unity`,
  `Assets/Scenes/Marksman range.unity`, Global Context, and Operator. The earlier
  handoff incorrectly said this archive lacked the original scenes. Its source
  project is under `export/ExportedProject/`.
- Pinned SDK archive SHA-256:
  `bb691c98abc8797eedbd48d3a766bc734214aed746e52e2691d8bbb268c60764` for
  .NET SDK 8.0.426. The tracked `scripts/env/procstat-namespace-shim.c` was
  built as `/tmp/libprocshim.so` to resolve this runner's `/proc` issue.

## Rebuilt index and profile identities

The regenerated source index `/tmp/cpr-scenes-full-source.json` has SHA-256
`92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb` and is
251,585,827 bytes. It contains 2,060 assets, 40,139 records, 24,856 references,
and 776 binary assets. The earlier delivery note records a different index
SHA (`6586b293…`). Preserve this identity difference; the current index and
profile are validated internally, not represented as byte-identical to the
older index.

The full Mission profile has six roots: Global Context, Interior Mode UI,
Interaction Menu, Operator, Version, and Mission 01.

| Measurement | Result |
|---|---:|
| Assets / records | 1,919 / 22,365 |
| Compressed asset bytes | 97,767,549 |
| Source asset bytes | 205,514,345 |
| Largest source / compressed asset | 14,858,177 / 3,108,968 bytes |
| Metadata source / compressed bytes | 6,912,902 / 851,251 |
| Metadata SHA-256 | `b610de818b5b99a4a011efbbc10ae5dc50cd3ad806fe5d386284a62e57ff5450` |
| Metadata gzip SHA-256 | `2cdfbc1251a6e7be503f84cad15348923f7a79dbb521202dc28f4674dfd77399` |

The five-root Global/UI/Operator/Version profile is 1,230 assets, 36,971,121
compressed asset bytes and 77,806,659 source bytes; it does not include the
Mission 01 scene closure.

## Verification

- At runtime source SHA `f30f36e2d99661aaca221d529b1c831c9dda2d7a`, the C# source
  profile verifier and `--mission-ui-incremental-contract` passed against the
  full Mission profile. Mission 01 started; authored UI, original actors,
  input press/release, actor snapshot, movement force, and browser physics state
  round-trip checks passed. These are headless checks, not native PhysX or
  rendered-pixel evidence.
- EngineChecks and FoundationChecks passed. Runtime Release build passed with
  0 errors and 48 warnings. Browser Release publish passed with SDK 8.0.426;
  the optional wasm-tools optimizer was unavailable.
- `npm ci --prefix game --ignore-scripts` and
  `npm run --prefix game vendor` generated the lockfile-pinned PhysX adapter.
- On source SHA `925fa4baf6abb56bc2e5e86eeaf1f903d2cf674a`,
  `python3 -m unittest discover -s scripts -p '*test.py'` passed 39/39.
  The added regression asserts that the default Mission Site profile name and
  six roots match what the runtime page requests.
- The default `scripts/package_wasm_site.py build` command was run with
  `/tmp/cpr-scenes-full-source.json`; no profile name or roots were overridden.
  Build and independent `verify` passed for
  `/tmp/cpr-default-mission-site-925fa4b`, recording source SHA
  `925fa4baf6abb56bc2e5e86eeaf1f903d2cf674a`, 2,673 files, 199 Blazor boot
  resources, 195 valid WASM files, and 1,919 hashed source assets. The packaged
  file path is
  `scene-data/fullsource/global-ui-mission-operator/manifest.json`, matching
  `game/index.html`.
- The source packager default had previously emitted the five-root profile
  `global-ui-operator` while the page requested the six-root Mission profile
  `global-ui-mission-operator`. `scripts/package_wasm_site.py` now defaults to
  the requested profile name and Mission root; callers may still provide
  explicit roots/profile. The exact default build and regression test pass.
- Sites confirmed the Runtime Site remains custom/owner-private. Its archive
  source repository reports branch `main` on a separate Cloudflare artifact
  repository, not this game's `develop/wasm` branch. No Site deployment or
  browser/iPhone test occurred; use the supported Sites source workflow to
  prepare an archive-backed Site version before deployment.

## Transfer interpretation and next investigation

`scene-data/scene-package-loader.mjs` downloads bounded batches of six
compressed assets in parallel, then verifies and delivers each asset to the C#
store in manifest order. The Mission profile is still about 98 MB compressed.
This proves package integrity, not that the reported slow tail or pause near
asset 1,900 is fixed. Next acceptance is exact iPhone Safari startup with
per-asset timing and memory captured. If it reproduces, optimize the measured
requests while preserving cancellation, bounded memory, order, and fail-closed
hash checks.
