# Site source alignment blocker — 2026-10-26

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
