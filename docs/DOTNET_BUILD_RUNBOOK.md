# .NET build preflight and failure runbook

Use this before the first .NET command in a fresh Work Mode checkout. The repo
pins .NET SDK **8.0.426** in the root `global.json`; a newer SDK is not a
substitute. This host has also shown a runner-only `/proc` process-stat failure
before compilation. The exact SDK and existing process shim below resolved it.

## Preflight

Run from the repository root:

```sh
SDK_ROOT=/tmp/codex-dotnet-8.0.426
if [ ! -x "$SDK_ROOT/dotnet" ]; then
  SDK_ROOT=/workspace/scratch/c5901021c6f6/.dotnet-8.0.426
fi
test -x "$SDK_ROOT/dotnet"
export DOTNET_ROOT="$SDK_ROOT"
export PATH="$DOTNET_ROOT:$PATH"
if [ -f /tmp/libprocshim.so ]; then
  export LD_PRELOAD=/tmp/libprocshim.so
fi
dotnet --version
```

The version must print `8.0.426`. If `dotnet` aborts before printing a version
with a `Process.GetStat`, process/thread metadata, or `/proc` error, classify it
as a host startup failure. Keep the source unchanged. Reuse the existing shim
if it is present; do not repeatedly retry the same failing invocation. If the
SDK or shim is absent, record that as an environment blocker and continue with
independent work.

## Restore, then verify

Missing `obj/project.assets.json` produces `NETSDK1004`. Restore the specific
project before `build` or `run`; do not treat this as a C# compile error. In a
network-restricted workspace, `--ignore-failed-sources` worked with the already
available packages. If restore then reports a missing package, keep that as a
restore/dependency failure rather than retrying compilation.

```sh
dotnet restore runtime/EngineChecks/Crisis.Runtime.EngineChecks.csproj --ignore-failed-sources
dotnet run --project runtime/EngineChecks/Crisis.Runtime.EngineChecks.csproj --no-restore

dotnet restore runtime/FoundationChecks/Crisis.Runtime.FoundationChecks.csproj --ignore-failed-sources
dotnet run --project runtime/FoundationChecks/Crisis.Runtime.FoundationChecks.csproj --no-restore

dotnet restore runtime/Crisis.Runtime.csproj --ignore-failed-sources
dotnet build runtime/Crisis.Runtime.csproj --no-restore --no-incremental -v:q
```

For a Release WebAssembly package, restore as part of publish so the browser
runtime pack can be resolved:

```sh
dotnet publish runtime/Browser/Crisis.Runtime.Browser.csproj -c Release
```

`NETSDK1112` after `publish --no-restore` has meant that the required WASM
runtime pack was not present in the restored assets. Retry publish with restore
enabled; do not assume a successful native build restored the browser pack.
`dotnet publish` does not accept every build switch: `--no-incremental` is a
build option, while the prior publish command using it failed argument parsing.
Use the exact command for the operation rather than copying flags between
`build`, `run`, and `publish`.

## Diagnostic triage

| Symptom | Classification | First response |
| --- | --- | --- |
| `global.json` SDK resolution error or `dotnet --version` is not 8.0.426 | SDK selection | Use the SDK path from preflight and keep the root pin unchanged. |
| `Process.GetStat`, process/thread metadata, or `/proc` exception before MSBuild starts | Runner startup | Set the existing `LD_PRELOAD=/tmp/libprocshim.so`; do not edit C# or project files. |
| `NETSDK1004` / missing `project.assets.json` | Restore state | Restore that exact `.csproj`, then run/build with `--no-restore`. |
| `NETSDK1112` during WebAssembly publish | Missing runtime pack | Repeat publish with restore enabled. |
| Missing `Crisis.Runtime.FullSource.Projectile` in FoundationChecks | Check-project source boundary | Confirm `runtime/FoundationChecks/FoundationSourceBoundaryStubs.cs` declares the compile-only boundary type. Keep the stub out of production runtime sources. |
| `DebugUtils` / `Process.get_ProcessName` exception from direct MSBuild | Runner startup | Prefer the pinned `dotnet` CLI with the process shim. Do not record it as a compiler diagnostic. |
| `wasm-tools` workload optimization warning with successful publish | Optional workload warning | Record the warning; it did not prevent the verified publish. Install workloads only if a later required gate specifically needs them. |
| `CSxxxx` diagnostics after compilation begins | Source/compiler result | Preserve the exact exit status and full diagnostics; compare only against a baseline built from the same compiler graph. |

Do not use `dotnet build`/`run`/`publish` exit codes as interchangeable evidence.
Capture the command, exit code, stdout/stderr, SDK version, and tested source
SHA. A nonzero compiler exit is a real failing gate; host startup and restore
errors are recorded separately. One bounded attempt per diagnosed blocker is
enough unless new evidence or a dependency change appears.

## Last verified baseline

On 2026-10-23, pinned SDK 8.0.426 with the process shim passed the full runtime
rebuild (0 compile errors), Release Blazor WebAssembly publish, EngineChecks (69
assertions), and FoundationChecks (121 assertions). The runbook documents those
observed environment/build failures and is a preflight aid; it does not replace
the current source SHA's required local verification.
