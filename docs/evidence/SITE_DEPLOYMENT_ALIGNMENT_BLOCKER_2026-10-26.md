# Site source alignment and deployment — 2026-10-26

## Disposition

The exact-source Site build/package gate is closed for version 74. The Site remains owner-private/custom access. Production URL: https://crisis-response-runtime.craftysnipe13.chatgpt.site.

- Site project: `appgprj_6acfef7174888191bc2bc2f269f60acc`.
- Version: 74; version ID `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_0f9fe8a4d73081919ec6ea3655289698`.
- Deployment: `appgdep_6adf84df5dd481919cd70f72f33f0889`, `succeeded`.
- Bound source: Cloudflare artifact repository, branch `main`, commit `ae134ef153149e6b6ab262502dd978c4737087ca`.
- Audience: `custom`; only the owner account is allowed; no groups. This preserves the pre-existing restricted audience.

## Exact source build and package

The Site-bound source commit was rebuilt with pinned .NET SDK 8.0.426. `dotnet build runtime/Crisis.Runtime.csproj -c Release --no-incremental` passed with 0 errors and 48 existing warnings. `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release -o /tmp/cpr-site-v74-publish-ae134` succeeded; SDK reports the optional `wasm-tools` workload is not installed and publication is unoptimized.

`python3 scripts/package_wasm_site.py build` used that publish output, the exact Site recovered-source index and `global-ui-mission-operator`. `python3 scripts/package_wasm_site.py verify dist` returned `status=verified` with exact sourceCommit `ae134ef153149e6b6ab262502dd978c4737087ca`, 2,686 files, 199 Blazor resources and 195 WASM files. The complete profile is 1,919 assets / 22,365 records / 97,767,549 compressed bytes / 205,514,345 source bytes. Source-package SHA-256: `ea3592ff5bdd39534bb905cd0abac74f456578b46eb4b12cdd0cb79fe8520cf7`. `dist/build-manifest.json` SHA-256: `1b734839f875c15121d815faefd0656f0fffbb8800e6dcdb22af4ca413bf3e16`.

The Site source helper pushed the exact commit and made `/tmp/cpr-site-v74-ae134.tar.gz`. Local archive SHA-256 `72a017a7c673b688abfe459fe8d5ad2100c3153fa6d378922ed6ce2d1de20fa0`, size 138,696,962 bytes. The archive was extracted and every one of the 2,686 manifest-listed files was independently checked for presence, length and SHA-256. The embedded `.openai/hosting.json` resolved to this project and `static.directory=dist`; `dist/index.html` was present. The local recovered mesh `.Combined Mesh (root_ scene)_1.asset.QpoHQd` was verified absent from the archive. Site version metadata stores a normalized TAR representation with `sha256:a8727a341a0de81f7235c01ca37804416dbbc1b8f500ad4b4a711e6ad11e6075`, 178,022,400 bytes and 2,688 files; it is not the byte representation of the local gzip archive.

## Source crosswalk to public develop

Compared every tracked Git blob at Site `ae134ef153149e6b6ab262502dd978c4737087ca` and public `develop/wasm` `4428f5f5adc06edeb19b1dc17f111e66e407d04c`: Site has 2,264 files; develop 2,283; Site-only 1 (`.openai/hosting.json`), develop-only 20, shared-but-different 14. The full path inventory is available from the commit trees; previous v73 path list below is historical and should not be treated as current.

These current product paths are byte-identical: `game/aim-direction.mjs`, `game/browser-input.mjs`, `game/index.html`, `scene-data/scene-package-loader.mjs`, `game/original-particle-event-binding.mjs`, `game/original-particle-renderer.mjs`, and `runtime/Engine/BallisticEffects.cs`. Thus v74 carries the particle inherited-velocity fix, but the joystick/aim implementation is unchanged. `runtime/Browser/BrowserEntryTeamMission.cs` remains one of the shared production paths with different content; CI on public develop is not attributed to Site `ae134ef...`.

## Applied source fix and checks

The Site branch now includes the source Rigidbody velocity transport fix. The original InheritVelocity particle path is supplied its source Rigidbody velocity; the matching renderer/runtime checks were copied from the tested develop implementation. Targeted particle tests passed 13/13, full Node suite passed 143/143, and Runtime BallisticContracts reported `sourceRigidbodyVelocityTransported=true`. Site Python baseline checks passed 45/45; EngineChecks passed 75 assertions; FoundationChecks passed 121 assertions. These are Site-tree checks, separate from public develop run #79.

Run #79 / Actions `40356543988` on public develop SHA `4428f5f5adc06edeb19b1dc17f111e66e407d04c` had both jobs pass (`121690423604`, `121690424261`). It is separate evidence, not the Site build's CI attestation. Full-profile browser measurements and WebAudio results are in the IndexedDB closure note.

## Recovery note

The pre-existing untracked recovered mesh file is preserved locally and excluded by local `.git/info/exclude`. The source helper's first push auto-staged it in intermediate commit `28d550c001cc9a163e80d63179f47e89ed56571c`; cleanup commit `ae134ef153149e6b6ab262502dd978c4737087ca` removed it from the latest source tree. The v74 package/archive and deployed version were built from the cleanup commit and do not include the mesh. Keep the file and local exclude rule intact.

## Remaining acceptance gates

- The Site v74 build passed source/package integrity and is deployed, but the owner must still test that exact version on physical iPhone Safari: cold/warm Mission load and persistence; completion near the reported 1,900-asset point; joystick aim anchored correctly and independent of X; walk-to-sprint control; rapid-tap zoom prevention; audible reload and fire clips.
- Public run #79 used Chromium, actual browser IndexedDB and the production JS loader, but modeled the full-profile .NET bridge. It is not evidence of iPhone storage, performance, touch feel, Safari speaker output or actual .NET throughput at this scale.
- Do not claim complete parity until source behavior, native rendering/physics/audio and the device acceptance gates have evidence or are explicitly scoped out.
