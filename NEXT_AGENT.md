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

- The checked-out repository does not contain `scene-data/fullsource/global-ui-mission-operator/`; the 1,919-asset Mission source profile and metadata needed to reproduce and integrity-verify the exact Site package are absent. Therefore the package build/integrity gate is **not satisfied**, and this source change was **not published**. Recover the owner-authorized source package through its approved durable connector, verify all compressed and decompressed assets against the manifest, rebuild and verify the Site archive for this exact source checkpoint, then publish only that verified archive.
- Playwright/browser binaries and exact-build iPhone Safari acceptance are still unavailable. Owner should verify Mission startup and completion, error copy/readability, and aim independence after a verified deployment.
- Gun audio remains open: the Mission audio bridge is silent. Some Unity components and NPC systems also remain unsupported or unverified.

## Next action

Recover and integrity-verify the Mission source profile, then build and verify the exact-source WASM Site package before any publication. If that package gate remains blocked, continue independent gameplay gates and preserve the blocker details in this handoff.
