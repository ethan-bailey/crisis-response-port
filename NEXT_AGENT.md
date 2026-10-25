# Current handoff — 2026-10-25

## Source checkpoint and deployed Site

- Branch: `develop/wasm`.
- Tested source change: `64cd6c3e94b8f197ecc82100d705a91418053370` (`fix: restore Mission page startup and error UI`), based on live branch head `4e4e0887c2b71da230c881e95c9b630fde743706`.
- The change restores the full `game/index.html` document shell, which had been truncated in an earlier change. This brings back the Mission controls, selectable error panel, M4 magazine wiring, initialization path, and opt-in `scene-smoke` guard. Loading status now explicitly switches to “initializing original Mission actors…” after verified delivery completes.
- Site v49 remains owner-only and unchanged at source `2c62158f49f71903971d35838cd28cc5e427a629` (package source `19165a5`). No new Site package was published.

## Verification at the tested source change

- `node --test scripts/*test.mjs`: **81/81 passed**.
- `python3 -m unittest discover -s scripts -p '*test.py'`: **38/38 passed**.
- EngineChecks: **69 assertions passed**.
- FoundationChecks: **121 assertions passed**.
- Runtime checks passed, including ballistics and original-source checks.
- `dotnet build runtime/Crisis.Runtime.csproj -c Release --no-restore -v:q`: succeeded, **0 warnings, 0 errors**.
- `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release -o /tmp/cpr-publish-restore-shell`: succeeded using checksum-verified .NET SDK **8.0.426** and the tracked `/proc` shim. WASM optimization workload was unavailable; publish completed successfully.
- `git diff --check`: passed.

## Blocked release gate and acceptance

- The checked-out repository does not contain `scene-data/fullsource/global-ui-mission-operator/`; the 1,919-asset Mission source profile and metadata needed to reproduce and integrity-verify the exact Site package are absent. A recovered `crisis-response-recovery-direct-v3-1.zip` was verified at SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`, but it is a Unity asset export and contains neither the generated profile nor `cpr-scenes-full-source.json` (the expected source-index SHA-256 is `6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639` in `docs/evidence/INCREMENTAL_SOURCE_DELIVERY_2026-10-25.md`). The package build/integrity gate is therefore **not satisfied**, and this source change was **not published**. Recover the exact source index or a complete profile and manifest through an approved durable source, verify every compressed and decompressed asset, rebuild and verify the Site archive for this exact source checkpoint, then publish only that verified archive.
- Playwright/browser binaries and exact-build iPhone Safari acceptance are still unavailable. Owner should verify Mission startup and completion, error copy/readability, and aim independence after a verified deployment.
- Gun audio remains open: the Mission audio bridge is silent. Some Unity components and NPC systems also remain unsupported or unverified.

## Next action

Recover the exact source index or a complete Mission profile and manifest, then verify all assets and build/verify the exact-source WASM Site package before any publication. If unavailable, keep publication blocked and continue independent gameplay gates.

## User-reported iPhone load screen — 2026-10-25

- The supplied screenshot shows a black page containing only “Warning: truncated output (original token count: 14171) Total output lines: 582”; the browser chrome identifies the in-app browser as ChatGPT. This exact phrasing is a ChatGPT output-truncation notice, not a .NET/JavaScript exception emitted by the game runtime. The screenshot alone does not establish whether the game page loaded successfully behind that UI or whether the notice was rendered as page content.
- Read-only Site checks: v49 deployment `appgdep_6addd0d050cc8191a1d35cd94f432895` reports `succeeded` at `https://crisis-response-runtime.craftysnipe13.chatgpt.site` (updated 2026-10-25 09:50 UTC). Worker error logs returned no events for the previous 240 minutes. Deployment status has no screenshot asset. The Site is owner-only, and the web retrieval tool cannot access its URL, so these checks do not verify the rendered page or the user's session.
- No runtime change or publication was made from this screenshot because it does not identify a game-side exception and the exact source package gate remains blocked. Continue with direct owner-session page verification using the provided screenshot/visible UI; capture a game error panel or browser console/network error if present. Keep v49 intact unless concrete evidence identifies a new source fault. Publication remains blocked pending exact source index/profile recovery and package integrity verification.

## Source profile availability recheck — 2026-10-25 10:41 EDT

- Confirmed against live `develop/wasm` head `7d28858d0724682f258e6df9573e8a1cbc44ce97`: GitHub Contents API returns 404 for `scene-data/fullsource/`. The repository's `scene-data/` root contains `crisis-response-scenes.json.gz` (86,196,480 bytes), `marksman-range.json`, and the loader, but not the required Mission FullSource profile directory. This confirms the existing package-source blocker remains; the gzip scene file is not evidence of the missing indexed 1,919-asset profile and does not clear package integrity.
- No build or publication was attempted. Next gate is still exact profile/index recovery, then per-asset validation and archive integrity before any Site operation.

## Parity gates and exact checkpoint — 2026-10-25 10:41 EDT

The working checkout was cloned from live `develop/wasm` at `9651b39b41f6c649b3529d5abfc3c5a8b0cad556`; worktree was clean before verification. On that exact SHA:

- `node --test scripts/*test.mjs`: **81/81 passed**.
- `python3 -m unittest discover -s scripts -p '*test.py'`: **38/38 passed** after expanding the sparse checkout to include required source-reference fixtures. A first sparse-only invocation had 8 missing-fixture errors and is not counted as a pass.
- .NET SDK is not installed in this fresh workspace (`dotnet: command not found`); no .NET build, WASM publish, or exact package verification was run here. Prior successful SDK 8.0.426 results above belong to their explicitly named earlier source checkpoint only.

### Remaining gates, in dependency order

1. **Recover authoritative Mission source inputs.** Obtain the exact `cpr-scenes-full-source.json` (expected SHA-256 `6586b293eb1455669eba29042469ebd6740b0ad08629fd39ea625237626dd639`) or a complete matching 1,919-asset Mission FullSource profile, manifest, and provenance from an approved durable source. The current branch has no `scene-data/fullsource/` directory; its existing 86,196,480-byte monolithic gzip is not a substitute. Do not synthesize or silently replace source data.
2. **Batch-build and verify source delivery.** Generate the incremental profile from those inputs; validate every compressed and decompressed asset against its expected size/hash, all 1,919 paths and roots, metadata/index hashes, and aggregate byte totals. Then investigate the reported slow tail/hang near 1,900 assets with measured request/progress traces; preserve bounded delivery, cancellation, ordering, and fail-closed integrity. Existing loader unit coverage is not an iPhone network test.
3. **Build the exact browser source and Site package.** On the same code SHA and profile, use pinned .NET SDK 8.0.426, run the full runtime/WASM build and relevant runtime contracts, assemble the site archive, and verify its complete file count and integrity manifest. No publish before these pass.
4. **Close actor authority and page wiring in one dependency-aware batch.** Load the authored Mission with the recovered original Operator/Occupant objects, then route page movement, absolute aim (independent of X), physics state/forces, rendering, and weapon callbacks through those same original actor instances. Verify the authored Mission UI and lifecycle; do not use the transport weapon/character as a competing authority.
5. **Reach gameplay parity against original contracts.** Batch remaining source-backed character physiology, animation/IK, projectile-to-limb combat, wound/ragdoll/weapon-drop, NPC behavior/spawning, and audio bridge work by shared actor lifecycle and dependencies. Preserve unresolved Unity-only or unavailable native services as explicit dispositions with observable contracts; report gun audio separately until shot timing/volume is verified.
6. **Exact-build acceptance.** Run browser transfer and runtime smoke on the exact package; obtain owner iPhone Safari acceptance for Mission startup/completion, controls, aim, movement/sprint, combat, error readability, and memory/throughput. Desktop Playwright is optional under owner policy; mobile acceptance remains the target-device check.
7. **Publish and verify.** Only after gates 1–6 applicable to the release pass, save/deploy the verified archive to the existing owner-private Site, confirm deployment status and exact source/archive hashes, then retest the deployed build. No deployment was attempted in this batch.

**Next action:** recover the authoritative source index/profile. Meanwhile, the Node and Python contract suites pass at `9651b39`; the full build is blocked in this workspace by the missing .NET SDK, and the package/release gate is blocked by missing Mission source inputs.

## Follow-up validation — 2026-10-25 12:20 EDT

- Re-cloned the live branch into the current workspace and confirmed clean `develop/wasm` at `4d9143e6aa6e390bb684c0031f8c3b5d25ad4b00` (docs-only update from the previously tested code checkpoint).
- Re-ran `node --test scripts/*test.mjs`: **81/81 passed**; `python3 -m unittest discover -s scripts -p '*test.py'`: **38/38 passed** on this exact SHA.
- Connected GitHub reports no workflow runs for this docs-only SHA. This workspace still has no `dotnet` executable, and the Mission profile/index is still absent, so full-source build, WASM package build, and package integrity remain open. No runtime files changed and nothing was published.
- Continue from the ordered gates above; first dependency is authoritative source index/profile recovery. Preserve 100% integrity verification before publication.
