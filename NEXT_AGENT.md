# Crisis Response Port — WorkMode handoff

## Checkpoint

- Observed local branch/head: `codex/game-scene-name-normalization-20261023` at `20bfc1d4996dcd8c9e6abd042e74d4e3721cd935`. The only changes made after this checkpoint are this handoff and the generated repository index.
- Live GitHub has no open PRs. `develop/wasm` is `c6cee907eb12504e91e33963cd5e512fe20c491a`; the live feature branch is `b3640f02009147eb7a3350489db382bb9ad30983`. GitHub comparison shows the feature branch is 2 commits ahead and 6 behind `develop/wasm` from merge base `9e234ba325d83fb1ba2f7ab38d7097e75e83fdbb`.
- Local commits `d59cba1`, `c3bccc1`, and `20bfc1d` are not on the live branch. Shell `git fetch` failed because this checkout has no GitHub username/credential. Do not force-update a ref or lose the local commits; reconcile with live `develop/wasm` when authenticated Git access is available. Keep one consolidated line of work; do not open a PR for each small fix. GitHub Actions remain disabled.
- Root `global.json` remains pinned to .NET SDK `8.0.426`. SDK is installed at `/tmp/codex-dotnet-8.0.426`; this host needs `LD_PRELOAD=/tmp/libprocshim.so` for the normal CLI to avoid its process-stat startup failure.

## Verified work

- Full `runtime/Crisis.Runtime.csproj` rebuild passed with .NET 8.0.426: 0 compile errors. Release Blazor WASM publish passed; the compiled runtime was produced from the same `game/` and `runtime/` sources in `d59cba1` (later commits changed only audit tests/index tooling and documentation).
- The regenerated package at source commit `20bfc1d` passed `scripts/package_wasm_site.py verify`: 766 output files, 199 Blazor boot resources, and 195 WASM files. `game/browser-input.mjs` and `runtime-web/browser-input.mjs` have identical SHA-256. Package manifest correctly says `browserTestedThisBuild: false`.
- Same-process localhost HTTP smoke returned 200 for the root, game and runtime HTML, both input modules, the scene loader, and a WASM file; WASM MIME was `application/wasm`. Chromium/Firefox executables are absent, so this is not browser boot or gameplay evidence.
- Repository file index `docs/audit/repository-file-index.json` covers 1,887 tracked/non-ignored files against the latest package (893 source inputs, 766 outputs); zero indexed source hashes mismatch. Its `--check` now ignores only the recorded generation HEAD, so the index remains checkable after the index commit advances HEAD.
- Passing checks: browser input 18/18, source character-step 5/5, scene loader 3/3, package tests 13/13, audit tools 12/12, actor migration (0 violations), strict ownership scan, and source dependency inventory (662 sources, 1,704 symbols, 1,332 reachable files; 124 ambiguous names remain classified for review). EngineChecks passed 69 assertions; FoundationChecks passed 121 assertions.
- Recovered player-data extractor validated the pinned `Crisis Response.rar` SHA-256 and all 411 files / 185,036,038 bytes without altering the archive.
- Added stationary touch hold-to-fire after 350 ms while preserving tap-to-aim, same-spot tap fire, drag-to-aim, and cancel/release semantics. Added cancellation and hold regression tests. Added the missing compile-only `FullSource.Projectile` declaration to FoundationChecks; this changes no production runtime owner.
- Corrected the audit test to verify the six recovered Unity built-in resource records: Shader 46, Material 10303, Sprite Shader 10753, Sprite Material 10754, and Sprite records 10907/10913.

## Outstanding gates

- No real Chromium/WebKit/browser boot or touch gameplay smoke was available. Do not publish a new Site build until that gate passes. Existing private Site v31 remains the last deployed build; this new package is local at `/tmp/crisis-response-site-20bfc1d` and was not deployed.
- Marksman still references missing Mesh GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`, fileID `4300000`. The recovered candidate `Assets/Mesh/308 Winchester casing hi.asset` (GUID `cd74722033652fb409431b2947740490`) is owner-approved as a possible compatibility replacement, not as original identity. Defer it until the other completion gates because it affects the Marksman scene. Apply only with the existing scoped script and preserve the missing-GUID provenance record.
- Original Operator/Occupant two-actor lifecycle and full gameplay acceptance remain unverified even though the C# graph compiles. After browser access and branch reconciliation, continue with the real prefab lifecycle, then two-actor spawn/equip/move/hit/physiology/ragdoll, and matching browser verification.

## Errors encountered and disposition

- Plain `dotnet` initially failed before compilation at `Process.GetStat`; the pinned SDK plus `/tmp/libprocshim.so` worked. A direct MSBuild invocation with an invalid `-noIncremental` switch was discarded; `-target:Rebuild` was used instead.
- First FoundationChecks compile exposed missing compile-only `Crisis.Runtime.FullSource.Projectile`; the boundary stub was added and the suite then passed. An early run also lacked `project.assets.json`; restore succeeded with the shim.
- One audit test still expected only records 46/10303; the expectation was corrected to the six source-backed records above and all 12 tests passed.
- Shell fetch cannot authenticate (`could not read Username for 'https://github.com'`). This prevents syncing or pushing the local commits; it does not affect local verification.

## Next action

Restore authenticated Git access, fetch the live feature and `develop/wasm` refs, preserve and reconcile local head `20bfc1d`, and land the verified batch back to `develop/wasm` as one consolidated integration. Then obtain a real browser and continue the original prefab/two-actor acceptance gates. Do not re-run the already passing local checks without new code or a changed dependency.
