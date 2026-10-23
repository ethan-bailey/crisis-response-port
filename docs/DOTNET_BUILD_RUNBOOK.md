# .NET build preflight and failure runbook

Use this before the first .NET command in a fresh Work Mode checkout. The repo
pins .NET SDK **8.0.426** in the root `global.json`; a newer SDK is not a
substitute. This host has also shown a runner-only `/proc` process-stat failure
before compilation. The exact SDK and existing process shim below resolved it.

## Repeated workspace/checkout unavailability

A Work Mode turn can start with a different scratch mount. Do not assume a
repository path remembered from a prior turn still exists. Before any project
command, confirm that the current working directory is the repository root:

```sh
pwd
git rev-parse --show-toplevel
git status --short --branch
find /workspace/scratch -maxdepth 5 -type d -name .git -print
```

If the expected checkout is absent, make one bounded recovery attempt:

1. Search the current scratch mount for the project checkout. Do not treat a
   separate `.sites-checkout`, an old feature-branch ZIP, or a source archive
   as the live `develop/wasm` checkout.
2. If shell Git authentication is already configured, test the live remote once
   with `git ls-remote`. If it fails with
   `could not read Username ... No such device or address`, the noninteractive
   runner cannot prompt for credentials. Do not retry the same shell fetch.

   Shell CLI login is separate from the connected GitHub app and is local to
   the current runner. A successful `gh auth login` in another chat or runner
   does not install `gh` or transfer its credentials here. Before offering the
   device-login URL, confirm `gh` exists and run `gh auth status`. Once logged
   in, run `gh auth setup-git`, then verify the target remote with one
   `git ls-remote`. Do not copy or print access tokens. On 2026-10-23, an earlier
   runner completed GitHub CLI device authorization, but this runner has no
   `gh` executable or CLI credential file; the connected GitHub API is still
   authorized. A device code cannot be generated until `gh` is available in
   the target environment. On 2026-10-23, installing GitHub CLI 2.45.0 from
   Ubuntu's apt snapshot and completing device authorization restored shell
   access. `gh auth setup-git` followed by `git ls-remote` verified the private
   repo. The CLI warned that credentials were saved in plain text under
   `/root/.config/gh/hosts.yml`; do not copy or commit that file. This fixes the
   current runner only. To avoid repeating device authorization, use a
   persistent development environment with GitHub CLI and a secure credential
   store; a fresh Work Mode scratch runner does not inherit either.
3. Use the connected GitHub API to verify the live branch/PR state or read
   individual files. This is useful for inspection, but it does not provide a
   buildable local repository. The GitHub fetch connector rejects repository
   tarball endpoints and does not download binary archives.
4. Check for the pinned SDK and the process shim. Do not start a .NET build
   without the repository and SDK preflight in this runbook.

If the checkout or required SDK still cannot be recovered, classify it as a
critical environment blocker. Do not edit source through stale archives, make
a status-only commit, or repeat the same fetch hourly. Record the exact failed
paths/commands and live GitHub SHA, pause any recurring work automation, and
resume when the correct checkout and required toolchain are mounted. Avoid
Codespaces through 2026-11-01 unless the owner requests it or a required gate
cannot run by another practical means.

Observed on 2026-10-23: the prior
`/workspace/scratch/c5901021c6f6/crisis-response-port` checkout was absent;
the current mount contained only a separate Sites checkout and an older feature
archive. Shell `git ls-remote` could not prompt for GitHub credentials, the
GitHub connector rejected the tarball URL, and the pinned .NET SDK/process shim
were not present. The connector could still confirm `develop/wasm` at
`5bb158f0bb18ab3fbf17fcd478f2266226cd057e`. This was not a source/compiler
failure.

## Preflight

If the SDK archive has to be restored, use the pinned official archive and
verify its known SHA-256 before extracting. Avoid `dotnet-install.sh` on this
runner because its tar extraction tries to restore archive ownership, which the
container rejects. The successful extraction path was:

```sh
curl -fL https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.426/dotnet-sdk-8.0.426-linux-x64.tar.gz \
  -o /tmp/dotnet-sdk-8.0.426-linux-x64.tar.gz
echo 'bb691c98abc8797eedbd48d3a766bc734214aed746e52e2691d8bbb268c60764  /tmp/dotnet-sdk-8.0.426-linux-x64.tar.gz' | sha256sum -c -
mkdir -p /tmp/codex-dotnet-8.0.426
tar --no-same-owner -xzf /tmp/dotnet-sdk-8.0.426-linux-x64.tar.gz -C /tmp/codex-dotnet-8.0.426
```

If no prior shim exists and `dotnet --version` aborts in
`System.Diagnostics.Process.GetStat`, build the runner workaround tracked at
`scripts/env/procstat-namespace-shim.c`:

```sh
gcc -shared -fPIC -O2 -Wall -Wextra -o /tmp/libprocshim.so \
  scripts/env/procstat-namespace-shim.c -ldl
export LD_PRELOAD=/tmp/libprocshim.so
```

The shim only redirects the current process's `/proc/<pid>/stat` read to
`/proc/self/stat`. It is not production code. On 2026-10-23, that preflight
made the exact SDK report `8.0.426` successfully.

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
| `MSB1025` from direct MSBuild while the runner cannot read process metadata | Runner startup | Use the pinned `dotnet` CLI with the existing process shim; do not repeatedly retry direct MSBuild or edit project/source files. |
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

## Full original CharacterBody compilation

On 2026-10-23, the pinned SDK **8.0.426** and `/tmp/libprocshim.so` compiled the
entire `runtime/Crisis.Runtime.csproj` source graph, including
`runtime/Original/CharacterBody.Full.cs` (`Crisis.Runtime.FullSource.CharacterBody`),
at source SHA `1237e360b829a5a80289d0136d2f8be7a7159050`. The exact build command
was `dotnet build runtime/Crisis.Runtime.csproj --no-restore --no-incremental
-v:q`; it exited **0**, with **0 errors** and **45 warnings**. The run also
passed `check_characterbody_ownership.py --strict`, the exact recovered enum
verification, and both compile-summary regression tests. See
`docs/evidence/CHARACTERBODY_FULL_COMPILE_2026-10-23.json` for the machine-readable
result. This establishes compilation of the original class, not runtime gameplay
or parity.

## Gameplay gate 7: Mission 01 and recurring runtime failures

On 2026-10-23, Mission 01 reached `Mission.Start` and `BeginMission` with the
original spawners and produced 2 TeamOperators and 5 Occupants (8 CharacterBody
instances total). The headless contract uses silent audio/mixer adapters and
disables the presentation-only MissionUI, interior canvas, FogOfWarElement, and
vocalization callbacks. It verifies mission/actor startup and gameplay ownership;
it does not verify rendered UI, audible output, AI behavior-tree parity, or
browser/iPhone acceptance.

Before running the mission contract, restore the package and source prerequisites
recorded by earlier gates. Then use the pinned SDK and process shim from this
runbook:

```sh
dotnet run --project runtime/Checks/Crisis.Runtime.Checks.csproj --no-restore -- \
  --mission-contract /tmp/cpr-original/scene-data/crisis-response-scenes-with-gameplay-resources.json
```

The mission contract passed after these recurring runtime issues were corrected:

| Failure | Fix to apply before the next mission run |
| --- | --- |
| Operators absent although the prefab and serialized script GUID exist | Include the recovered original `OperatorSpawner.cs` in the compiled source set. Resolve its GUID to the original source type. |
| Spawned `CharacterBody.Physiology` is null inside `OperatorSpawner.Awake` | An active `Instantiate` must finish `Awake`/`OnEnable` for the complete, remapped clone before returning, including when called from another `Awake`. `PrefabCloner` now activates that clone immediately after deserialization. |
| `Singular transform` under the original Interior mode UI root (serialized scale is zero) | `SetParent` retains the serialized local pose when the parent world matrix has no inverse. |
| `Cannot preserve sheared transform when reparenting` during weapon arm IK | Extract translation, axis scales, and closest rotation when a hierarchy produces a matrix that cannot be represented as exact TRS. |
| Original reticle `WeaponReticle.Awake` finds no `LineRenderer` | Import built-in Unity class ID 120 for prefab assets before invoking original callbacks. |
| Behavior Designer task load crashes unboxing absent `ID`, `FriendlyName`, or node metadata | Apply safe defaults for optional task/node metadata before loading task fields. |
| Property mappings report `Unable to resolve component type Blackboard` | Resolve unqualified original type names by their short name when assembly-qualified lookup fails; the runtime source namespace differs from the original `Assembly-CSharp`. |
| EngineChecks expects `Renderer.material` to be the original material object | Match Unity semantics: first `Renderer.material` access creates a private copy; keep the test focused on that copy and preserve identities for the remaining material slots. |

The gate 7 run passed with no `Blackboard` property-mapping errors after the
short-name type lookup fix. Scene import still reports intentionally unsupported
presentation/physics asset records and unresolved external resources in the
incomplete package; the check's JSON output is diagnostic inventory, not proof
that every scene asset is executable. The unrelated Marksman `.308` round prefab
still has a missing original asset GUID; the approved `.308` casing-only mapping
does not authorize replacing that projectile mesh.

Verification on the working source tree for this checkpoint:

- `Crisis.Runtime.Checks --mission-contract`: passed; 2 Operators, 5 Occupants.
- `Crisis.Runtime.Checks --spawner-contract`: passed; actor-owned trauma and
  physiology remained independent and original movement/animation contracts ran.
- `scripts/package_ammunition_closure.py ... --check`: passed; 29/29 assets,
  no unresolved assets or stale script/builtin maps.
- `python3 -m unittest scripts.extract_runtime_scene_test`: passed, 6 tests.
- `runtime/EngineChecks`: passed, 69 assertions.
- `runtime/FoundationChecks`: passed, 121 assertions.
- Full `runtime/Crisis.Runtime.csproj` rebuild: 0 errors, 45 warnings.

The warnings are existing source/compiler diagnostics (including member hiding,
nullable annotations, obsolete legacy serialization/crypto APIs, unassigned
fields, and Windows-only registry calls); they are not build errors. Capture the
full output at `/tmp/cpr-full-build-final.log` if a later run needs individual
warning locations. Do not treat this gate as completion of gate 8 or as browser
or mobile acceptance.
