# Pinned SDK recovery and exact-source verification — 2026-10-26

## Recurrent environment issue corrected

A bare `command -v dotnet` returned no path and was initially mistaken for an absent SDK. This runner already had .NET SDK 8.0.426 at `/tmp/codex-dotnet-8.0.426/dotnet` and the verified process shim at `/tmp/libprocshim.so`. The documented environment resolved it immediately:

```sh
export DOTNET_ROOT=/tmp/codex-dotnet-8.0.426
export PATH="$DOTNET_ROOT:$PATH"
export LD_PRELOAD=/tmp/libprocshim.so
dotnet --version
```

It reported `8.0.426`. The runbook now explicitly distinguishes “not on PATH” from “not installed” and checks the known SDK/shim paths before declaring a blocker.

## Exact source tested

- Repository: `ethan-bailey/crisis-response-port`
- Branch source commit: `5693194aef4d614a99764c8683c0c344a15ebe1b`
- Verification used a clean detached Git worktree at that exact SHA.
- Runtime checks: passed.
- EngineChecks: **75 passed**.
- FoundationChecks: passed (the previous exact checkpoint is 121 assertions).
- Release runtime build: passed, **48 warnings, 0 errors**.
- Release WASM publish: passed; optional `wasm-tools` optimization workload was unavailable.
- Output assertions: published `wwwroot/index.html`, `wwwroot/browser-input.mjs`, and a `.wasm` file all exist and are non-empty.
- Local Node suite against the equivalent code tree: **127 passed**. Local Node version is 24.22.1; CI pins Node 22.
- GitHub combined status/PR-run connector returned no checks for this push SHA. These results are local exact-SHA verification; no hosted-run result is claimed.
- No Site deployment or publication occurred.

The live handoff contains the current remaining gates and next action. This verification proves compilation/publish and source contracts, not native visual parity, browser runtime behavior, or physical iPhone Safari acceptance.
