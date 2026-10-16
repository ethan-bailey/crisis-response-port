# Protected manual iPhone WASM preview — October 16, 2026

## Corrected link and exact repair — 2026-10-16

**Current protected game link:** https://crisis-response-port-fb7x1neld-ethanbaileymarsh-8332.vercel.app/game/

**Current protected runtime link:** https://crisis-response-port-fb7x1neld-ethanbaileymarsh-8332.vercel.app/runtime-web/

Current Vercel preview **READY**, ID `dpl_7T1JZtAUjQLRZw2mys7i5xyh8tvH`, source Git snapshot `preview/wasm-hand-modes-20261016` SHA `4c1b0d27014b1aa9776fd7da46b7f46630b50be2`. Old preview `dpl_FunFRks2CrYVvft3j4SaFtckc2sS` is **BROKEN** despite Vercel reporting READY: `game/assets/mission01.crg` returned 404 on iPhone. GitHub tree/manifest comparison proved that a prior `git add -A` had dropped four gitignored generated files (scene binary, report and two PhysX vendor files) even though they existed in the verified local package. [Repair #38913180609](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38913180609) succeeded: redownloaded original [verified CI artifact #38909327063](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38909327063), SHA-checked and force-staged only four missing files, then verified that **all 725 compiled-package files** were tracked and content-hashed; no runtime/gameplay code was changed. Current preview was explicitly deployed once from the corrected SHA. Original release protection is retained, and website HTTP/physical Safari functionality is not yet independently verified.

**Future releases:** reuse permanent default-branch [manual-verified-wasm-preview.yml](https://github.com/ethan-bailey/crisis-response-port/blob/main/.github/workflows/manual-verified-wasm-preview.yml), with exact successful Actions run ID and source SHA; it builds nothing, force-adds all generated files and fails closed on any missing Git-index/manifest path, then pushes a reusable private `preview/wasm-current-iphone` snapshot. After the job succeeds, use one deliberate SSO-protected Vercel preview deployment from the resultant SHA. No auto Git deploys, repeated custom workflows, or recompilation of an unchanged package. This workflow is newly installed and has not yet been run. The section below documents the **previous broken deployment for historical traceability only**, and is not the current testing link.

## Historical first deployment (broken; superseded)

**Owner-authenticated Vercel URL:** https://crisis-response-port-bpbgx2jdn-ethanbaileymarsh-8332.vercel.app/game/

**Owner-authenticated runtime diagnostic:** https://crisis-response-port-bpbgx2jdn-ethanbaileymarsh-8332.vercel.app/runtime-web/

Vercel deployment ID `dpl_FunFRks2CrYVvft3j4SaFtckc2sS`, project ID `prj_EgsDBXSKhvina2fDecbdMf6ylTKE`, targeted **preview only**, API status **READY**. Original private, orphan static snapshot branch `preview/wasm-hand-modes-20261016` SHA `bca1f5b151a6abf7f12ae901ba9e9c27b745eda1`. Owner Vercel SSO protection was enabled for preview URLs when deployment was created; **do not disable it or promote to an unprotected public custom domain**, because game source/assets are private. Vercel connection has insufficient rights to create a shareable authentication-bypass link or to read protected pages; this deployment is **not yet real-iPhone-tested or externally HTTP-verified** by the agent. The user can sign into their owning Vercel account before opening the URL. A READY deployment status does not prove full game functionality.

## Source-verified package chain

- **Actual original game code/SDK8 WASM/PhysX/Chromium/WebKit combined integration:** [GitHub Actions #38909327063](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38909327063) PASS at exact `develop/wasm` *gameplay* source SHA `252337d96b845f5b2eb6dd406c32a52a423d3e37`.
- Private tested site artifact `12049238437`, private evidence artifact `12049348261`, both expire November 15, 2026.
- [One-shot private snapshot staging #38911332510](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38911332510) PASS: GitHub CI downloaded the **existing matching compiled output**, verified archive SHA256 against the original build evidence, unpacked and rechecked every package-file SHA, confirmed original input gameplay commit, then committed **static site files only** to a new private orphan branch `preview/wasm-hand-modes-20261016` at `bca1f5b151a6abf7f12ae901ba9e9c27b745eda1`. Neither Unity nor WASM was recompiled by staging.
- The site snapshot contains root `index.html` redirect to `/game/`, `/runtime-web/` compiled Blazor/.NET runtime, original renderer/scene/PhysX files, and `build-manifest.json` showing source and each package file hash. This is **partial** original game behavior, not 1:1 native parity, all missions, full animations, or hardware Safari acceptance.
- A **single explicit** Vercel API preview deployment was created from the snapshot Git SHA, rather than restarting Vercel deploys on each commit. Snapshot `vercel.json` preserves `git.deploymentEnabled:false` and normal source branches inherit the same. This did not promote to production and did not expose the original full recovery ZIP.
- **Subsequent source-only original ArmController work [PR #25](https://github.com/ethan-bailey/crisis-response-port/pull/25) is merged at `2ee6f01fc4d6111e8948608c7bf39e10843358e8`**, but adds source/reference docs and verifier only, not executable original controller. Hence this preview deliberately represents the latest *tested* WASM gameplay, not the newer documentation/source-only HEAD.

## Test on iPhone 16 Safari

Open the protected URL while signed into the owner Vercel account. Check whether the page loads `/game/`, character responds to touch movement and drag-to-aim, fire/weapon/projectile actions function, and whether the WebAssembly runtime starts. Record device iOS/Safari version, screenshot or error text, and which action fails. Do not equate Chromium/WebKit desktop CI with physical iPhone. If Vercel displays a permission or sign-in screen, ask for project/team access authorization; **do not switch original game assets to public hosting** as a workaround.

**Next release:** only when another meaningful original executable source milestone has passed matching native C#/WASM/browser tests, create another protected static snapshot and make exactly one intentional preview deploy. Keep normal Vercel Git deployment opt-out and reuse the stable preview branch alias or the explicitly validated immutable URL.

## Provenance and rollback

The tested original game package is retained privately in GitHub Actions and the private snapshot branch; the Vercel deployment ID/URL refers to the single immutable staging SHA. Roll back by using the prior protected deployment rather than rebuilding untested source. Avoid deleting private source or changing visibility. Owner bypass access could not be generated automatically due to Vercel project scope permissions.
