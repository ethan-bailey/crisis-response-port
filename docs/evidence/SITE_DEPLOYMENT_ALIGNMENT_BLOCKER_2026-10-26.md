# Site source alignment blocker — 2026-10-26

## Updated exact-source checkpoint — 2026-10-26

Workflow #78 passed on `develop/wasm` SHA `30de783d924312a9483fc5f5a566968b9e64183d`. Its package verifier reported sourceCommit equal to that SHA, 2,686 files, 199 Blazor resources, 195 WASM files, and the complete six-root Mission profile. Source index SHA-256 is `9d866276582031d440c316c8e60d69d252506cf67a8ae9d03dbdbe0d91980f92`; deterministic package tar.gz SHA-256 is `7a6b0d5b6f945ef2457276c6d8a6186638d8d0a0a6969ba57f1140e5a77fa6f5`. Retained artifact `12664937990` has upload digest `sha256:67069fd6fc0a4bcc1a0076fdda44744dfcc414a663a5d3525d7cb80b9a7c1b3b`. Both Contracts/Release and Chromium IndexedDB/WebAudio jobs passed. This closes the exact source/package gate for this develop commit only.

Read-only Sites metadata still reports live version 73 source SHA `5ca69fc5a4b6538121b00aece66a8587997cf1f5` and archive SHA `sha256:d893e55c88784cdd059d1e386445c217dddbf29f9ea5d6d5ac89fbd3105658dc`. GitHub API lookup cannot resolve the v73 source SHA (422). The Site getter does not expose its configured source repo or branch, and `https://api.github.com/repos/ethan-bailey/crisis-response-port/contents/.openai/hosting.json?ref=main` returns 404. Public `main` is `d7af0076274ff0ae4da3da5f84e2d346e94e3292`; tested `develop/wasm` is `30de783d924312a9483fc5f5a566968b9e64183d`; compare is diverged by 1,320 ahead / 21 behind. Site version save requires the source commit to equal the configured remote source branch HEAD. Therefore no source-bound save/deploy input is currently verified; preserve v73 and do not use the develop archive against it.

## Next concrete integration batch

1. Obtain the Site-bound repository and configured source branch from a supported read-only metadata surface or the Site handoff/hosting configuration.
2. If it is the public repository's `main`, prepare a reviewed integration commit that carries the intended develop runtime onto that branch without silently merging the divergent histories. Run the exact Release WASM, full package integrity and Chromium jobs on that integration SHA.
3. If the Site supports an explicit source-branch change, verify the new branch binding before packaging. In either route, save/deploy only when the supplied archive and commit SHA both match the current configured Site source branch HEAD.
4. Continue independent parity work and owner-controlled iPhone acceptance while integration metadata is obtained. This is a concrete missing identity/binding, not a reason to stop all other gates.

The verified build now exists, but it cannot safely be deployed to the existing Site until its source provenance is aligned.

| Item | Exact source |
|---|---|
| Live Site | Crisis Response Runtime, project `appgprj_6acfef7174888191bc2bc2f269f60acc`, version 73 |
| Version 73 source SHA | `5ca69fc5a4b6538121b00aece66a8587997cf1f5` |
| Current repository `main` | `d7af0076274ff0ae4da3da5f84e2d346e94e3292` |
| Tested `develop/wasm` source/package | `b173b1daebcb0337fb1def6da948d08f3d990d79` |
| GitHub compare | diverged; develop ahead 1,291 commits and behind 21; merge base `13b24c4ce296a81f4d6803cd4a3325020d13229b` |

The Site handoff identifies `main` as the Site source branch. Current `main` does not contain `game/weapon-audio-runtime.mjs`, `game/original-particle-renderer.mjs`, `runtime/Engine/BallisticEffects.cs`, or `.github/workflows/source-contracts.yml`, all part of the tested develop source. Site publication requires the supplied archive and `commit_sha` to match the current configured remote branch head.

## Safe disposition

Do not pass the develop package with a main-branch SHA, publish the stale v73 archive, or merge this branch divergence blindly. The exact next step is to inspect the Site's configured source repository/branch and establish the supported integration route:

1. If the Site supports changing its source branch, verify that branch on the Site before building from it.
2. Otherwise create a reviewed integration commit on the Site-bound branch that includes the intended runtime source and tests.
3. Run the exact Release WASM build, complete Mission package verifier and Chromium acceptance against that integration SHA.
4. Save/deploy only an archive whose embedded sourceCommit exactly matches that Site-bound pushed SHA.

Run #68 fully passed for `develop/wasm`: 2,686 files, 199 Blazor, 195 WASM; Mission profile 1,919 assets / 22,365 records; inner package SHA-256 `a5522d46a7dc7976aac57787ddf8100b8b3e4ef5e9fc201b6c549ad841fd01ec`; retained artifact `12649418198`, outer SHA-256 `f30f8ccdd34e0829706e98f3736003f7bc152486f7d49d550b32ab26a7c6a883`. Those integrity results do not change which source branch the Site expects. Site v73 remains live; no mismatched deployment was attempted.
