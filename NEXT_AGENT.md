# Crisis Response Port — WorkMode handoff

## Checkpoint

- Local checkout: `codex/game-scene-name-normalization-20261023`, checkpoint before this documentation commit `fbd5dbe9750ac4bfb585bf6da09a14ae3aa26999`. This documentation update changes no runtime source.
- Live GitHub state was checked through the GitHub connector: `develop/wasm` is `c6cee907eb12504e91e33963cd5e512fe20c491a`; live feature branch is `b3640f02009147eb7a3350489db382bb9ad30983`; no open PRs were returned. The local branch contains 12 commits not on the live feature branch. Do not force-update or discard them; reconcile with live `develop/wasm` when the correct GitHub write path is available. GitHub Actions remain disabled.
- Shell `git fetch` previously failed because this scratch checkout had no GitHub CLI/credential-helper credentials. This is separate from the connected GitHub API tool, which successfully read branch and PR state during this session. Keep the pinned SDK at .NET 8.0.426.

## Verified project state

- Last full runtime rebuild: 0 compile errors using SDK 8.0.426 and `/tmp/libprocshim.so`. Release Blazor WASM publish passed.
- EngineChecks passed 69 assertions; FoundationChecks passed 121 assertions. The latter uses the compile-only `FullSource.Projectile` declaration in `runtime/FoundationChecks/FoundationSourceBoundaryStubs.cs`; it does not alter production runtime sources.
- The package at `/tmp/crisis-response-site-20bfc1d` verified 766 output files, 199 Blazor boot resources and 195 WASM files. Browser/package acceptance remains separate from a successful compile.
- All prior focused checks and limitations are recorded in the preceding Git history. This documentation-only task does not require rerunning .NET builds.

## .NET preflight documentation added

- `docs/DOTNET_BUILD_RUNBOOK.md` records the pinned SDK setup, existing process-stat shim, restore-first commands, correct WASM publish path, failure classification, and fixes for observed failures.
- `AGENT_INSTRUCTIONS.md` now requires following the runbook before the first .NET command in a fresh checkout.
- The repository file index must be regenerated and checked after this doc and handoff update.

## Known failures and fixes

- `dotnet` aborted before compilation in `Process.GetStat` or `/proc` lookup. The pinned SDK plus `LD_PRELOAD=/tmp/libprocshim.so` worked; classify this as runner startup, not a source error.
- `NETSDK1004` / missing `project.assets.json`: restore the exact project, then use `--no-restore` for build/run.
- `NETSDK1112` during WebAssembly publish with `--no-restore`: run `dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release` with restore enabled so the WASM runtime pack is resolved.
- FoundationChecks initially lacked `Crisis.Runtime.FullSource.Projectile`; a compile-only boundary declaration fixed the check graph without changing production code.
- An invalid direct MSBuild switch and direct MSBuild process-name exception were not compiler failures. Use the pinned CLI and operation-specific flags in the runbook.
- `wasm-tools` produced an optional optimization warning during a successful publish; the warning did not block output.
- Shell fetch credential failure blocks CLI Git sync, not local builds or GitHub connector reads. Do not place credentials in repository files.

## Remaining project gates

- Real Chromium/WebKit/browser boot and touch gameplay smoke are unavailable in this workspace. Do not publish a new Site build until those gates pass. Private Site v31 remains the last deployed build.
- Marksman references missing Mesh GUID `b4b69dc6c74eeb54cbd22d28e37f9a19`, fileID `4300000`. The recovered candidate `Assets/Mesh/308 Winchester casing hi.asset` (GUID `cd74722033652fb409431b2947740490`) is only an owner-approved possible compatibility replacement, not original identity. Preserve the missing-GUID provenance record and use the existing scoped script if applying it.
- Original Operator/Occupant two-actor lifecycle and gameplay acceptance remain unverified despite successful compilation. Continue with real prefab lifecycle, two-actor spawn/equip/move/hit/physiology/ragdoll, then matching browser verification.

## Next action

After committing this documentation update, reconcile the local feature branch with live `develop/wasm` before any push or PR operation. Use the connected GitHub API for reads; arrange persistent host credentials if shell `git fetch` is required. Do not repeat passed local checks without source or dependency changes.
