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

## Per-asset diagnostics and Site version 50

At game source SHA `d65f867d0dca48205b3b50e6133b76251fc189ba`, the loader now
reports three stages (`fetching`, `adding`, and `delivered`), per-asset fetch
and hash time, base64 encoding time, and Blazor bridge time. The game UI shows
the current asset or pending download range and retains the last 64 samples
plus the ten slowest fetches and ten slowest bridge calls. “Copy report” copies
this bounded report; if clipboard access is unavailable, it displays selectable
JSON. This makes the previous near-1,900-asset stall diagnosable, but does not
claim a performance improvement before iPhone measurements.

On `d65f867`, the Node suite passed **81/81**, Python suite passed **39/39**,
the exact default package build passed, and independent package verification
passed: 2,673 files, 199 Blazor resources, 195 valid WASM files, 1,919 source
assets. Package location:
`/tmp/cpr-site-package-d65f867`; its manifest records source SHA
`d65f867d0dca48205b3b50e6133b76251fc189ba`.

The Sites artifact repository was opened and updated through its supported
source workflow. Its pushed `main` commit is
`b59e6617e9252e431356ffcab55aac142274e719`; the separately generated archive
`/tmp/cpr-site-runtime-d65f867.tar.gz` has SHA-256
`62d28c833f390d8ed0cc89bc0c0a513352f26850af8ac447dea7cc6c5a94fdf9`. An
independent streaming extraction audit found all 2,673 files in the package
manifest, verified every byte count and SHA-256, and confirmed the required
Sites hosting manifest. The manifest's package source SHA is the game commit,
not the separate Sites-repository commit.

The owner-private Site deployment succeeded on 2026-10-25:

- Version: `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_e330bf53211081918e941b4c257130cd`
- Deployment: `appgdep_6ade3fd53780819188345cb08bc77bca`
- URL: `https://crisis-response-runtime.craftysnipe13.chatgpt.site`
- Audience remained custom/owner-only; no access settings changed.

The Site-side regression suite was reconciled to the current package: browser
input tests now match the exact package control behavior, release metadata
identifies the new build manifest, and the large original scene smoke archive
test explicitly checks the default omission when `sceneSmokeIncluded` is false.
All **36/36** Site-side tests then passed. The omitted 220 MB full-scene smoke
archive remains a deferred parity check, not a release-integrity failure.

The page already sets a non-zoomable viewport and `touch-action:none` for the
game surface; interactive buttons use `touch-action:manipulation`. The recent
version adds visible progress and a report button for the greyed-out Start
Mission state. These UI protections still need iPhone Safari acceptance; no
Playwright or iPhone automation was available in this environment.

## FullSource analog movement and Site version 51

At game source SHA `f7d96a413f05c7106c9f57de4c646cdbef3309b3`, the opt-in
FullSource actor bridge now receives the eased movement magnitude calculated
by the browser joystick. The bridge applies that magnitude to the original
`CharacterBody` walk and sprint caps across delayed source fixed steps, then
restores the authored values even when the source tick throws. Its snapshot
records the effective caps and magnitude. The `--browser-entryteam-session-contract`
passed the Mission 01 UI contract, source input press/release, source movement
force, half-magnitude cap, authored speed restoration, and PhysX state round
trip. Existing browser-input tests assert horizontal joystick travel does not
alter vertical aim and changing horizontal travel while aiming does not pull
the aim point back toward center.

At `f7d96a4`, Node checks passed **81/81** and Python checks passed **39/39**.
Browser Release publish passed with zero errors; optional `wasm-tools`
optimization was unavailable. Exact package build and independent verify
passed with source SHA `f7d96a413f05c7106c9f57de4c646cdbef3309b3`, manifest
SHA-256 `09c660d5e426f656c95fea1daf31363371e8b3e689a42a5b01e8a2084e89f2`,
2,673 files, 199 Blazor resources, 195 WASM files, and 1,919 Mission profile
assets (97,767,549 compressed bytes; 205,514,345 source bytes). A separate
archive extraction verified every manifest file's byte count and SHA-256.

The Sites artifact repository is at commit
`563963f14b613eb85df2992e35db1984e66e6cf2`; exact archive SHA-256 is
`e803b0e5a21eac7c42d2a5cd70d98a7284ac464a14aa37abab3840a1858e2396`. Site
version
`appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_1b4447e9d2608191a24b97b6ad379ee0`
deployed successfully as deployment `appgdep_6ade425248d88191b7d5129cc459a5c5`.
The existing Site was confirmed custom/owner-only (one user, no groups) before
deployment and remains so; no audience settings changed. iPhone acceptance is
still owner-run. Record delivery of asset 1,919, load time, stalled range if
any, and memory before making a performance claim.
