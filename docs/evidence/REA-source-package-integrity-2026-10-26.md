# Exact source package integrity — 2026-10-26

## Source and build

- Live repository branch at build: `ethan-bailey/crisis-response-port`, `develop/wasm`.
- Exact build source: `0248bdcb329ed453e527cfe5a57530ec977deadc` (`Recover complete source particle mesh dependencies`).
- GitHub Actions run **#45**, ID `40282585096`, completed `success` for that exact SHA: https://github.com/ethan-bailey/crisis-response-port/actions/runs/40282585096.
- Contracts/Release WASM job `121438948787` and Chromium IndexedDB job `121438948967` both succeeded. The source job passed Node, Python, runtime, engine, foundation, Release WASM build/output verification, recovered-source verification/index generation, complete Mission package construction, and artifact retention. Chromium passed all seven IndexedDB fixture assertions.
- CI reported the six-root `global-ui-mission-operator` package as **1,919 assets / 22,365 records**, **97,767,549 compressed bytes**, **205,514,345 source bytes**, with no unresolved GUID references in that selected closure. This does not mean the whole 2,060-asset source index has no unresolved references or that gameplay/visual parity is complete.
- Full recovered-source archive SHA-256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. Source index SHA-256 embedded in `build-manifest.json`: `9d866276582031d440c316c8e60d69d252506cf67a8ae9d03dbdbe0d91980f92`.

## Retained artifact audit

- Artifact ID `12632747721`, name `crisis-response-site-package-0248bdcb329ed453e527cfe5a57530ec977deadc`, GitHub API ZIP size **138,551,846 bytes**, API digest `sha256:60e83a612a173d1105030560cfc0b729ad574d4c8fa3d3b9dd7bb98e5c4488a1`. It expires 2026-11-09 06:58:26 UTC.
- Downloaded archive SHA-256 independently matched the API digest exactly: `60e83a612a173d1105030560cfc0b729ad574d4c8fa3d3b9dd7bb98e5c4488a1`.
- ZIP contains one deterministic `cpr-site-package.tar.gz`; compressed member size **138,717,178 bytes**, SHA-256 `4094731ef8d5228b246d2f812bccbb9a3f46956c486d707f336ec52d1b2b46d0`.
- Extracted package verifier command:

  ```sh
  python3 scripts/package_wasm_site.py verify /tmp/cpr-package-0248
  ```

- Result: `status: verified`, embedded `sourceCommit` exactly `0248bdcb329ed453e527cfe5a57530ec977deadc`, **2,686 manifest-listed files** plus `build-manifest.json`, **199 Blazor resources**, **195 WASM files**, and the expected six-root source profile. This command checks every listed file's size and SHA-256, rejects symlinks and unlisted/missing files, and validates the binary/profile invariants. The package manifest SHA-256 was `0622d08a3a9f5e8a5113a55a720860de5af63476c4615702deda4a9645506eb1`.
- Package scope explicitly records `physicalIPhoneTested:false`, `nativeParityVerified:false`, `originalGameComplete:false`, `offlineComplete:false`, and `sceneSmokeIncluded:false`. The Actions package is verified, but it is not a Sites deployment archive and was not deployed.

## Additional dependency inventory completed in this batch

`docs/particle-lifecycle-dependency-closure-2026-10-26.md` now records the complete-export `Projectile trace.mat` and its exact hash, built-in Standard shader GUID, null texture slots, serialized color/tint, source LineRenderer width override, and `ProjectileTrace` update/fade behavior. This closes that file/shader/texture inventory for the recovered export only. The Site/gameplay source-version crosswalk and renderer integration remain pending.

Focused local verification on the same content tree as the tested remote SHA: `node --test scripts/original_particle_lifecycle_dependency_test.mjs scripts/original_particle_mesh_source_test.mjs scripts/game_error_panel_test.mjs scripts/weapon_audio_runtime_test.mjs scripts/scene_package_loader_test.mjs` — **27/27 passed**; `git diff --check` passed. Local and remote tree SHA at this package checkpoint: `7f958660af240373ece961db8b70ac7fcc2943c2`.

## Release and parity boundary

Site version **71** remains live. Its exact source commit is now located in the available local Site checkout as `37e6d5dcebaaba71c405d898d161b3e36e946a78`, tree `e5b13db302dc6ede7dcde6c4d86d43d492074ce1`. A path/blob comparison with GitHub build SHA `0248bdcb...` found 2,230 shared paths: **2,191 identical blobs, 39 changed blobs**, plus one Site-only path and 35 GitHub-only paths. The Site archive itself remains unavailable for a new local archive-byte audit, and the exact 0248 package is not built from the Site branch head. See `docs/evidence/SITE_VERSION_71_RECONCILIATION_2026-10-26.md`.

Do not report the joystick, aim, reload audio, Mission cold/warm load, or particle behavior as fixed on the live Site. No new Site version was saved or deployed. The exact tested WASM package is ready as a verified GitHub artifact; source synchronization and a matching Sites `dist` archive still need to be built and audited before publishing. The owner must test the eventual exact deployed build in iPhone Safari.

## Site version 72 exact source and archive

- Sites source commit `c815e230355d576e080920e8340e36757f0bb9dc` was pushed to the configured Site `main` branch. Its app/build inputs (1,212 paths across `game/`, `runtime/`, `scripts/`, `scene-data/`, `tools/wasm-compile/`, and `.github/workflows/`) were byte-identical to tested GitHub source `0248bdcb329ed453e527cfe5a57530ec977deadc`; no files were missing or different. Site-specific `.openai/hosting.json` and `AGENT_INSTRUCTIONS.md` were preserved. The 83 MB source scene diagnostic archive was excluded from the Site commit because the production profile omits original-scene smoke.
- `dist` was rebuilt from the c815 checkout using the verified Run #45 compiled `runtime-web` tree, whose code inputs match byte-for-byte. Local .NET SDK is absent, so a fresh c815 compile was not run. Site source contracts passed Node **140/140** and Python **44/44**. `scripts/package_wasm_site.py verify dist` passed with 2,686 manifest-listed files, 199 Blazor resources, 195 WASM files, and the same exact Mission profile.
- Sites archive helper output `/tmp/cpr-site-v72.tar.gz` is 138,711,795 bytes, SHA-256 `7e4d87a64294e43b4e4413fac367b026ba6332128d7e562327001876855f6b48`. A separate tar audit matched every one of 2,688 regular archive members to `dist` by content and verified the archived hosting project ID.
- Sites saved and deployed version **72**, ID `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_c2aaeda727d4819199a5fa6bea65766f`, deployment `appgdep_6adeff78e0d08191a6083aaba330296f`, status **succeeded**. API version metadata reports stored tar digest `51d7e08a5ecdceff68d62d4721cdb6f487598216df2f77e381d6a470a42b3c4e`, size 178,032,640 bytes, 2,688 files. This differs from the local compressed upload; Sites does not provide the stored archive bytes for readback. The Site remains owner-only/custom.
- Physical iPhone Safari cold/warm run, reported near-1,900-asset stall, joystick feel, reload/gunshot sound, and actual gameplay/visual parity are still not device-accepted. See `docs/evidence/SITE_VERSION_71_RECONCILIATION_2026-10-26.md` for the deployment/crosswalk evidence and boundary.
