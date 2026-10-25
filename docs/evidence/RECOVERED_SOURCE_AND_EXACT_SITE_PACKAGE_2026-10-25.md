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

## Absolute FullSource aim contract and Site version 52

The source-side Mission contract was extended and pushed at game source SHA
`fb966cbf277ffe071d9d31a3360e4063834c2eef`. It asserts the browser source
actor keeps `WeaponPointer`'s screen-ray updater disabled, sets a fixed
absolute world point, ticks left then right movement with browser mouse X at
opposite extremes, and verifies the original pointer retains the same world
target. This confirms source movement and browser mouse X cannot overwrite the
absolute target in the FullSource bridge.

At this exact source tree, Node checks passed **81/81**, Python checks passed
**39/39**, and `--browser-entryteam-session-contract` passed with analog
movement, absolute aim, original input, and PhysX state round trip. Browser
Release publish passed with zero errors (optional wasm-tools optimization was
unavailable). Exact package build and independent verification passed; source
SHA is `fb966cbf277ffe071d9d31a3360e4063834c2eef`, manifest SHA-256 is
`587b94e77dcfa6573ffc6212b7d4c0535f5199b07d7886553f2dd2451841b42c`.

The Site artifact repository is at commit
`9c9cc86533dec18147bf6c40a3aab0747eb9d138`; the archive SHA-256 is
`47bdb1c9f3be61efb548e31790fe5c412c0d109a3296a0c22c351193b1c7af51`. A
streaming audit verified all 2,673 manifest files, sizes, and SHA-256 values,
with no unexpected payload entries. Site version
`appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_6e4944b82db0819198d0195173569abe`
deployed successfully as `appgdep_6ade43c2850081918d427bb426ce45ba`. The Site
was confirmed owner-only before deployment and its audience did not change.

The gun-audio follow-up remains: normal page playback has a bounded Web Audio
path that rejects suspended, unloaded, and stale events and applies gain plus
compression, while the FullSource Mission test driver is intentionally silent.
Add deterministic tests around those guarantees, then continue source-owned
audio/event integration. Owner iPhone acceptance must still reproduce or
clear the delayed, loud, out-of-sync shot report.

## Early gun-audio preload and Site version 53

At game source SHA `ea08c803d797eecbf4788f3364d8ed06b3e324c0`, the browser
audio runtime now starts fetching and decoding the original weapon clips as
soon as `weapon-runtime.json.gz` is available, before material preparation and
the heavier presentation dependencies finish. A previously fired shot still
cannot play late: stale, suspended, or unloaded events are rejected at the
call site. Per-shot gain is clamped, the master gain remains 0.16, and the
compressor remains in the output chain. This shortens the preload window but
does not establish iPhone latency or loudness acceptance.

`scripts/weapon_audio_runtime_test.mjs` verifies stale/future, suspended, and
unloaded event rejection; a suspended shot stays dropped after unlock; and
gain is bounded. Full Node checks passed **84/84**, Python checks **39/39**,
and the Mission source actor UI/input/analog/absolute aim/PhysX contract passed.
Browser Release publish passed with zero errors; optional `wasm-tools`
optimization was not available.

The exact package build and independent verify passed at game source SHA
`ea08c803d797eecbf4788f3364d8ed06b3e324c0`; manifest SHA-256
`b1100ef64bcf42eca9c717d50d2056f95d9a15dcfd5659a4a41c966ab1e70c08`.
Package totals are 2,674 files, 199 Blazor resources, 195 WASM files, and
1,919 source assets (97,767,549 compressed bytes; 205,514,345 source bytes).
Site-side tests passed **36/36**. The Site artifact repository is commit
`c51eff030c22d650f2ff2c6710627ad0a0510ce4`; archive SHA-256
`ad36621fd1b63ace6249133c656de90eeb6ea1d91548b0e0e1222543c663d7b4`.
Streaming archive audit verified all 2,674 manifest files, sizes, hashes, and
no unexpected payload entries.

Site version
`appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_cb0b6657250081919e4a7e206bf8f055`
deployed successfully as `appgdep_6ade453e1bc88191ab7fa788db01b156`. Owner
role and custom/one-user access were confirmed before deployment; audience
settings did not change. The FullSource Mission audio driver remains silent by
design; original source AudioSource-to-browser event delivery is the next
audio parity gate.

## FullSource AudioSource bridge and Site version 54

At exact game source commit `b159d007ccd4574c8d7f420388631ae31b40d916`, the
opt-in FullSource Mission installs a bounded browser audio event driver for all
original `AudioSource` objects. Source one-shots retain clip path, volume, and
`Time.time`; the actor bridge drains events with each source tick. Browser
playback resolves the original clip path to the existing weapon manifest and
rejects stale, suspended, or unloaded shots immediately. The source gunfire
clip path `Assets/AudioClip/5.56x45mm-01-unsuppressed.wav` matches the browser
`audioFire` entry exactly. Unsupported loop/play/stop routing and unknown
source clips are counted, not substituted.

The source Mission contract passed with bounded 64-event queue behavior and
one dropped overflow event. Node passed **85/85**, Python **39/39**, isolated
EngineChecks **69**, and FoundationChecks **121**. Browser Release publish
succeeded with zero errors; the optional `wasm-tools` workload was unavailable.
The first package attempt exposed a missing generated PhysX adapter in the
fresh worktree. Running `npm ci --prefix game --ignore-scripts` and
`npm run --prefix game vendor` restored the lockfile-pinned 2.7.3 dependency;
no project source change was needed for that environment setup.

Exact package verification passed at game source SHA
`b159d007ccd4574c8d7f420388631ae31b40d916`; manifest SHA-256 is
`157efaf1b376dacb88fbadf973c10301bbb5564dd8078996d986f2d51478a7c5`. The
package contains 2,674 files, 199 Blazor resources, 195 WASM files, and 1,919
source assets (97,767,549 compressed bytes; 205,514,345 source bytes). Sites
artifact tests passed **36/36**. The artifact repo commit is
`684ffbddc2d9fe1846563105407ab1c231b42de8`; its archive SHA-256 is
`0dbfc4fdc6f24941106a5f6c3ad88511b6132431754a379126bfee95d04c2052`. Streaming
archive audit matched every manifest file and found no unexpected payloads.

The exact verified archive deployed privately as Site version 54
`appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_3423555d530c8191b12d12169f7b776c`,
deployment `appgdep_6ade483c9bf88191b3b227310295e96f` (succeeded) at
`https://crisis-response-runtime.craftysnipe13.chatgpt.site`. Owner role and
custom access with one allowed user and no groups were confirmed before
publishing; no audience settings changed.

This closes only event delivery for supported FullSource one-shots. It does
not prove the real weapon callback, loop/mixer/spatial behavior, default-page
FullSource ownership, or iPhone audio acceptance. Next test an actual source
gunfire/reload callback and preserve the delayed/loud iPhone report as the
device acceptance case.

## Source weapon audio callback contract and Site version 55

At exact runtime source SHA `7739bbccc7583e09461c294975d77612c1499492`, the
Mission contract locates the original M4A1 and invokes its private source
`WeaponBase.EmitMuzzleAudio(true)` callback. It verifies that the original
`PropagatingAudioSample`/`AudioSource` path queues both authored clips:

- `Assets/AudioClip/5.56x45mm-01-unsuppressed.wav` for the weapon report.
- `Assets/AudioClip/5.56x45mm-crack-interior.wav` for the supersonic crack.

The check verifies one-shot disposition, bounded volume, and source-time
alignment in the browser audio event queue. It proves source clip identity and
browser mapping for the real source callback; it does **not** prove that a
player fire input reaches that callback. In the one-step headless Mission
input check, `controllerFireAudioEvents` remained zero. The original
`CharacterBody.PullTrigger()` only continues when `rightArm.OnTarget`, so the
next source gate is to trace arm readiness and input sequencing across source
fixed steps.

An exploratory direct call to the original `WeaponMechanism.StrikeFiringPin()`
reached `WeaponBase.EmitMuzzleSmoke` and threw
`System.NotSupportedException: Original particle simulation and render driver
is not bound`. The source fire path therefore cannot yet be claimed end-to-end:
the original particle system adapter must be bound and validated before the
test can prove projectile, recoil, smoke, and synchronized muzzle audio. Do
not ignore this failure or replace it with synthetic audio queue injection.

At this source SHA, Node checks passed **85/85**, Python checks **39/39**,
`--browser-entryteam-session-contract` passed including the source callback
clip assertions, EngineChecks passed **69**, and FoundationChecks passed
**121**. Browser Release publish succeeded with zero errors; optional
`wasm-tools` optimization was unavailable.

The exact package manifest SHA-256 is
`bbb7a1184dfe7e4041e27ce139dbe72ff1335aaf9189d26671b71293cca51755`.
Independent verification passed for 2,674 files, 199 Blazor resources, 195
WASM files, and 1,919 source assets (97,767,549 compressed bytes / 205,514,345
source bytes). Site checks passed **36/36**. Artifact repository commit is
`55b911ff6d55542cf9609723c2e288f45fe08f63`; archive SHA-256 is
`b6288693d8f68aae128b76c92a76838c4d9d1d648d01e6b2d6353c2fc128a4fc`. A
streaming extraction audit matched every package file and digest, with no
missing or unexpected payload.

On 2026-10-25, the package built from this exact source SHA was deployed as
private Site version 55:

- Version: `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_ae15aba1e49881919e7f6d07e0b2eb57`
- Deployment: `appgdep_6ade4a97891c8191a18ebd5722b1ca14`
- URL: `https://crisis-response-runtime.craftysnipe13.chatgpt.site`

The deployment is release-integrity verified, not full gameplay parity. iPhone
Safari acceptance and browser automation remain outstanding; Playwright and
browser binaries are unavailable in this runner. Owner retest should collect
the existing bounded delivery/memory report and check gun timing/loudness
immediately after load and after extended play. Remaining dependency-ordered
gates and the next action are indexed in `NEXT_AGENT.md`.

## Source fire contract, Mission audio initialization, and Site version 56

At exact game source SHA `bf8ec133d034e7763c075d06cb33b623dc78855c`, the
browser-entry Mission source contract waits for the original right arm's ready
state and enabled weapon input window before firing. It sends actual `fire`
input through the original source actor and observes one source shot, one
authored particle emission request, two casing force requests, and eleven
source audio events. The events include firing-pin, muzzle, interior crack,
and bolt sounds on the source timeline. Mission-scene AudioClip availability
is initialized after scene load; longer source ticks previously failed because
those clips had not been marked available.

The test particle and casing drivers are recorders. This proves source
callback/event behavior, not a native particle simulation, visible browser
effect, or physical casing trajectory. The default page still has not
migrated input, movement, physics, rendering, weapon, and audio ownership to
the same FullSource actor.

At this exact source commit, Node checks passed **85/85**, Python checks
**39/39**, EngineChecks **69**, FoundationChecks **121**, Browser Release
publish succeeded with zero errors, and
`--browser-entryteam-session-contract` passed. Optional `wasm-tools`
optimization was unavailable. The exact Mission package built and passed
independent verification: manifest SHA-256
`7224161f8a768d24144a3e8ef0ec708aff4397c9412081203ff3560c61403ccf`; 2,674
manifest files, 199 Blazor resources, 195 WASM files, and 1,919 Mission assets
(97,767,549 compressed bytes; 205,514,345 source bytes).

Site repository tests passed **36/36**. The pushed Site artifact commit is
`ad3bca8657fa114137df2819f08064e508d6e576`. Its archive
`/tmp/cpr-site-runtime-bf8ec.tar.gz` has SHA-256
`0fade0b33450ae11a243802182e35cbd29bd8ea1ac8582030f87852e9adda173`. An
independent extraction audit matched all 2,674 manifest entries by byte count
and SHA-256 with zero mismatches. It confirmed only the package build
manifest and required Site hosting descriptor are additional archive files;
the descriptor points to project
`appgprj_6acfef7174888191bc2bc2f269f60acc` and static directory `dist`.

The exact package deployed successfully on 2026-10-25 as Site version 56
(`appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_6789e55dcfcc8191943ecb736d5d3803`),
deployment `appgdep_6ade4faf129c8191a6f537cbf6253917`, URL
`https://crisis-response-runtime.craftysnipe13.chatgpt.site`. The Site was
confirmed owner-only and its access policy was unchanged. The Site version
tracks artifact source commit `ad3bca8`; the package manifest records runtime
source commit `bf8ec13`.

The previous owner iPhone measurements are from an older package and do not
establish acceptance for version 56. The latest screenshot's text
“Warning: truncated output” is consistent with tool-output truncation, not an
application stack trace; verify against live Site Worker logs before treating
it as a runtime defect. iPhone Safari load-tail, tap zoom, control behavior,
and delayed/loud gun-audio acceptance remain open. No Playwright/browser
binaries or physical iPhone are available in this runner. Playwright resolves
from the Codex Node runtime but Chromium is absent; an official Playwright
install attempt received an invalid zero-byte archive. The connected browser
did reach `/game/`, then Three.js failed to create a WebGL context because the
browser reports WebGL disabled. This runner-specific failure prevented a
rendering smoke and does not contradict the owner's prior iPhone view of the
character.

The delivered loader version sends compressed asset bytes through Blazor as
base64 text, then the existing .NET `byte[]` parameter decodes them. Its
regression test verifies a payload larger than one base64-encoding chunk by
round-tripping the exact gzip bytes. This addresses the observed
`ByteArrayJsonConverter` interop failure in the source-delivery path; the
version 56 Worker error query returned zero events over the last 180 minutes,
which is not proof of a successful iPhone session because no request evidence
was returned.
