# Exact-source package, persistence and audio acceptance — 2026-10-26

## Run identity

- Repository: `ethan-bailey/crisis-response-port`
- Branch: `develop/wasm`
- Source SHA: `b173b1daebcb0337fb1def6da948d08f3d990d79`
- GitHub Actions run #68: `40322845592` — overall success
- Contracts/Release job: `121571910792` — success
- Chromium IndexedDB/audio job: `121571911030` — success

## Source and package

- Node contracts: 143/143 passed; Python contracts: 45/45. Runtime, engine, foundation, Release WASM publication, recovery/index validation and complete Mission package checks also passed in the Contracts/Release job.
- Independent package verifier: `status=verified`; 2,686 files; 199 Blazor resources; 195 WASM files; sourceCommit exactly matches the workflow SHA.
- Complete Mission profile: 1,919 assets / 22,365 records / 97,767,549 compressed bytes / 205,514,345 source bytes.
- Recovery archive SHA-256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
- Full source index SHA-256: `9d866276582031d440c316c8e60d69d252506cf67a8ae9d03dbdbe0d91980f92`.
- Deterministic inner package tar.gz SHA-256: `a5522d46a7dc7976aac57787ddf8100b8b3e4ef5e9fc201b6c549ad841fd01ec`.
- Retained artifact `12649418198`: 138,555,231 bytes; outer ZIP SHA-256 `f30f8ccdd34e0829706e98f3736003f7bc152486f7d49d550b32ab26a7c6a883`.

## Browser persistence

All seven Chromium checks passed using actual IndexedDB:

1. First launch stored a manifest-verified asset.
2. Reload reused the persistent record without an asset network request.
3. Interrupted fetch wrote no partial record; retry stored only verified bytes.
4. Quota failure was reported while delivery succeeded without claiming persistence.
5. Corrupt cache data was rejected and repaired from verified network bytes.
6. Manifest source hash isolated cache versions.
7. Clearing removed only the selected manifest version.

## Headless weapon audio

The browser loaded the production compressed manifest and original WAV files, decoded 7 source clips and resumed the real production AudioContext through a trusted Playwright pointer click. The source dispatcher played each required event once:

| Event | Original source clip | Played |
|---|---|---:|
| Fire | `Assets/AudioClip/5.56x45mm-01-unsuppressed.wav` | 1 |
| Magazine extract | `Assets/AudioClip/magazine-out-02.wav` | 1 |
| Magazine insert | `Assets/AudioClip/magazine-in-01.wav` | 1 |
| Bolt return | `Assets/AudioClip/bolt-return-02.wav` | 1 |

Configured max output gain was 0.16; Float32 readback was 0.1599999964237213. Peak output analyser RMS was 0.1590921945306555. Played events: 4. Stale, suspended, unloaded, unmapped and unlock-failure counts: zero.

This proves decoded clips traversed the production WebAudio graph in headless Chromium. It does not prove physical speaker audibility, iPhone Safari autoplay behavior, or device volume/mute settings. Those remain device acceptance.

## Deployment alignment

This build is verified for `develop/wasm`, not for the Site-bound source tree. Site v73 references `5ca69fc5a4b6538121b00aece66a8587997cf1f5`; repository `main` currently points at `d7af0076274ff0ae4da3da5f84e2d346e94e3292`; tested develop points at `b173b1daebcb0337fb1def6da948d08f3d990d79`. GitHub compare reports diverged (develop ahead 1,291, behind 21, merge base `13b24c4ce296a81f4d6803cd4a3325020d13229b`). Main does not contain the tested audio/particle source files or source-contract workflow. Sites requires the published archive and commit SHA to match its configured source HEAD. Therefore no deployment was made from this mismatched branch package; v73 remains live. The next integration batch must establish the supported Site source path, build the resulting exact SHA, and rerun package integrity before publication.

## Other gate evidence and boundary

The 143-test Node suite includes the touch joystick aim anchor behavior: touch-down preserves current absolute aim, horizontal displacement does not change aim, vertical travel adjusts it, and releasing preserves the result. This is not physical iPhone touch-feel evidence. Full Unity particle lifecycle/render parity, native PhysX equivalence and overall game parity are not claimed.
