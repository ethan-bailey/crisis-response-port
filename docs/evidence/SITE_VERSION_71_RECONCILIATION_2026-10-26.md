# Site source crosswalk and version 72 deployment — 2026-10-26

## Current Site state

- Site: `Crisis Response Runtime`, project `appgprj_6acfef7174888191bc2bc2f269f60acc`, live URL https://crisis-response-runtime.craftysnipe13.chatgpt.site.
- Latest version **72**, deployment `appgdep_6adeff78e0d08191a6083aaba330296f`, status **succeeded**. Version ID: `appgprj_6acfef7174888191bc2bc2f269f60acc~appgver_c2aaeda727d4819199a5fa6bea65766f`.
- Exact Site source branch/commit: `main` / `c815e230355d576e080920e8340e36757f0bb9dc`. Site API readback confirms this exact source commit on version 72.
- Site access remains `custom`, owner-only allowlist, no groups. Access settings were read before release and unchanged.
- Version 71 remains historical: source `37e6d5dcebaaba71c405d898d161b3e36e946a78`, deployment `appgdep_6adec6d333188191a297e5ff157c996f`, status succeeded.

## Site 71 → GitHub 0248 source crosswalk

The exact v71 Site source commit was recovered locally in `.sites-checkout`. Comparing its tree with public GitHub build SHA `0248bdcb329ed453e527cfe5a57530ec977deadc` found 2,230 shared paths (2,191 identical blobs, 39 differing), one Site-only path, and 35 GitHub-only paths. Thirteen differences were in the game/runtime/package boundary.

The configured Site source was opened through the Sites helper, then synced from the verified GitHub 0248 source tree. All **1,212** physical source inputs under `game/`, `runtime/`, `scripts/`, `scene-data/`, `tools/wasm-compile/`, and `.github/workflows/` were byte-compared against 0248: **zero missing, zero different**. The Site-specific `AGENT_INSTRUCTIONS.md` and `.openai/hosting.json` were preserved. The 83 MB full-scene diagnostic archive was excluded from the Site commit (the production package omits it) and `/dist/` was added to Site-local ignore rules to keep generated package files out of Git.

The first push attempt included generated `dist/` and the diagnostic scene archive and was rejected by the Site Git server with `receive-pack request exceeds maximum size`. No remote ref moved. Both unnecessary additions were excluded, the package was rebuilt, and the source-only push succeeded at `c815e230...`.

## Version 72 package and deployment evidence

- Release source commit: `c815e230355d576e080920e8340e36757f0bb9dc`.
- Site package built from that exact Site checkout and verified by `scripts/package_wasm_site.py verify dist`: `status=verified`, 2,686 manifest-listed payload files, 199 Blazor resources, 195 WASM files, and profile `global-ui-mission-operator` with 1,919 assets / 22,365 records / 97,767,549 compressed asset bytes / 205,514,345 source bytes.
- Since this workspace has no .NET SDK, the compiled `runtime-web` output was reused from GitHub Actions run #45, which built exact source SHA 0248. This reuse is bounded by the 1,212-file byte-for-byte input crosswalk above; it is not a fresh .NET compilation from Site commit c815. Site Node contracts **140/140** and Python contracts **44/44** passed after `npm run --prefix game vendor`.
- Site package archive `/tmp/cpr-site-v72.tar.gz`: **138,711,795 bytes**, SHA-256 `7e4d87a64294e43b4e4413fac367b026ba6332128d7e562327001876855f6b48`. Independent tar audit verified all **2,688 regular files** against `dist` by content, confirmed the included `.openai/hosting.json` project ID, and rejected non-regular or unexpected paths.
- Site version metadata reports stored archive format `tar`, 178,032,640 bytes, 2,688 files, digest `sha256:51d7e08a5ecdceff68d62d4721cdb6f487598216df2f77e381d6a470a42b3c4e`. The stored digest/size differ from the locally uploaded gzip archive; Sites does not expose stored archive bytes for a post-save byte comparison. Preserve both values; do not claim server-side archive readback.
- Sites returned deployment `succeeded`; a fresh `get_site` confirms latest version 72 and the production URL above. No second deploy was submitted.

## What version 72 changes and what remains unverified

Version 72 advances the live Site to the 0248 code/package inputs, including the touch-down-anchored vertical-only aim, deliberate sprint break-through/hysteresis, incremental Mission source profile/cache flow, and bounded weapon-audio playback/preload behavior present in that source. This is a build provenance statement, not user acceptance of their feel or audible output.

- No iPhone Safari interaction was available in this run. Mission cold/warm load, Safari persistence/quota/eviction, the reported stall near 1,900 assets, joystick feel, reload timing, and audible gunshot level/synchronization remain unverified on device.
- CI #45 ran the source Mission UI incremental contract and Chromium IndexedDB fixtures on code SHA 0248; these are not an iPhone browser run.
- Full gameplay and native/rendered visual parity remain unclaimed. Keep `fullGameParity:false`, `sourceVisualParity:false`, and `originalGameComplete:false` until their actual acceptance evidence exists.

Related exact GitHub package evidence is in `docs/evidence/REA-source-package-integrity-2026-10-26.md`.
