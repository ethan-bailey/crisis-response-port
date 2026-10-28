# .NET build preflight and failure runbook

> This is a toolchain recovery guide. Follow [AGENTS.md](../AGENTS.md) for project policy and [NEXT_AGENT.md](../NEXT_AGENT.md) for live task state. Historical incident notes below are not the current source SHA or assignment.

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
2. The public repository can be cloned/fetched over HTTPS without GitHub CLI
   authorization when network access permits. Use the connected GitHub API to
   verify the live branch/PR state. For commits, use the connector Git Data API
   sequence in `AGENTS.md`, then fetch the updated public branch
   with Git. Do not install/authorize `gh` or repeat device login when the
   connected GitHub API is available. Connector-only file inspection is not a
   buildable checkout.
3. Do not try to recover a full repository tarball through the GitHub fetch
   connector; it rejects repository archive endpoints. Clone the live public
   branch into the current workspace instead.
4. Check for the pinned SDK and the process shim. Do not start a .NET build
   without the repository and SDK preflight in this runbook.

If the checkout or required SDK still cannot be recovered, classify it as a
critical environment blocker. Do not edit source through stale archives, make
a status-only commit, or repeat the same fetch hourly. Record the exact failed
paths/commands and live GitHub SHA, pause any recurring work automation, and
resume when the correct checkout and required toolchain are mounted. Avoid
Codespaces through 2026-11-01 unless the owner requests it or a required gate
cannot run by another practical means.

Historical incident record (2026-10-23; do not repeat its GitHub CLI recovery
steps): the prior
`/workspace/scratch/c5901021c6f6/crisis-response-port` checkout was absent;
the current mount contained only a separate Sites checkout and an older feature
archive. Shell `git ls-remote` could not prompt for GitHub credentials, the
GitHub connector rejected the tarball URL, and the pinned .NET SDK/process shim
were not present. The connector could still confirm `develop/wasm` at
`5bb158f0bb18ab3fbf17fcd478f2266226cd057e`. This was not a source/compiler
failure.

## Preflight

At the beginning of a checkout session, source the combined environment
preflight from any directory inside the repository:

```sh
source scripts/env/environment-preflight.sh
```

It checks the pinned SDK, the Chromium build specified by the installed
Playwright lock, and `git push --dry-run` on `develop/wasm`. It continues
through all three checks after a failure and redacts credentials from Git
output. A missing result is an environment blocker; do not treat it as a
source/build failure.

Before the first .NET operation, source the SDK-specific preflight below. It
exports the pinned SDK path and available process shim into the current shell.

Run the repository preflight as the first toolchain action in every fresh
checkout, before deciding that .NET is unavailable:

```sh
source scripts/env/dotnet-8.0.426-preflight.sh
```

It checks the documented SDK locations, loads the runner shim if present, and
requires the exact pinned version. A successful message is the required
checkpoint before any build/test/restore/publish command. `NEXT_AGENT.md` must
record the exact SDK path/version result so the next run does not repeat the
bare-PATH mistake.

A failed `command -v dotnet` only means the executable is not on the current `PATH`; it does not prove the SDK is absent. Before recording an SDK blocker, check the known runner locations `/tmp/codex-dotnet-8.0.426/dotnet` and `/root/.dotnet/dotnet`, plus the process shim at `/workspace/libprocshim.so` or `/tmp/libprocshim.so`. If the pinned executable exists, export `DOTNET_ROOT=/tmp/codex-dotnet-8.0.426`, prepend it to `PATH`, and load the available shim with `LD_PRELOAD`, then confirm `dotnet --version`. Do not download or reinstall the SDK when this verified copy is already present.

If the SDK archive has to be restored, use the pinned official archive and
verify its known SHA-256 before extracting. This checksum-verified archive path
is the proven SDK setup; do not retry `dotnet-install.sh`, which attempted to
restore archive ownership and failed in this container. The `dotnet` CLI is
still required for `build`, `run`, `restore`, and `publish`; avoid the failing
SDK installer and GitHub CLI login, not those .NET build commands. The
successful extraction path was:

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

Run from the repository root after the archive extraction above. Use the
verified SDK at `/tmp/codex-dotnet-8.0.426`; do not search remembered
generated-workspace paths. A file path in an earlier handoff is not proof that
it is mounted in this runner.

```sh
SDK_ROOT=/tmp/codex-dotnet-8.0.426
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
if it is present; do not repeatedly retry the same failing invocation. The
SDK archive and shim paths are runner-local and may not exist in a later Work
Mode checkout. Check current paths before use. If the SDK is absent, use the
verified archive extraction above; if only the shim is absent, compile it from
the tracked source above. Do not record either as a blocker until these proven
recovery steps have been attempted once.

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

When `dotnet publish` is given an explicit `--output <DIR>`, the Blazor static
site is written beneath `<DIR>/wwwroot`. Pass that inner directory to the site
packager, for example `--published /tmp/cpr-publish/wwwroot`. Passing the outer
publish directory makes `package_wasm_site.py` fail at
`runtime-web/index.html`; do not retry the same outer path.

`NETSDK1112` after `publish --no-restore` has meant that the required WASM
runtime pack was not present in the restored assets. Retry publish with restore
enabled; do not assume a successful native build restored the browser pack.
`dotnet publish` does not accept every build switch: `--no-incremental` is a
build option, while the prior publish command using it failed argument parsing.
Use the exact command for the operation rather than copying flags between
`build`, `run`, and `publish`.

When a check project's source has changed, build that check project before a
`dotnet run --no-build` invocation. `--no-build` intentionally reuses the
existing output and can execute a stale check harness even after the runtime
library itself was rebuilt.

The static site package also needs the generated browser PhysX adapter. If
`package_wasm_site.py` reports missing `game/vendor/physx-js-webidl.mjs`, restore
the lockfile-pinned dependency and regenerate its vendor output before
rebuilding the package:

```sh
npm ci --prefix game --ignore-scripts
npm run --prefix game vendor
```

## Diagnostic triage

| Symptom | Classification | First response |
| --- | --- | --- |
| `global.json` SDK resolution error or `dotnet --version` is not 8.0.426 | SDK selection | Use the SDK path from preflight and keep the root pin unchanged. |
| `Process.GetStat`, process/thread metadata, or `/proc` exception before MSBuild starts | Runner startup | Set the existing `LD_PRELOAD=/workspace/libprocshim.so` (fall back to `/tmp/libprocshim.so`); do not edit C# or project files. |
| `NETSDK1004` / missing `project.assets.json` | Restore state | Restore that exact `.csproj`, then run/build with `--no-restore`. |
| `NETSDK1112` during WebAssembly publish | Missing runtime pack | Repeat publish with restore enabled. |
| Missing `Crisis.Runtime.FullSource.Projectile` in FoundationChecks | Check-project source boundary | Confirm `runtime/FoundationChecks/FoundationSourceBoundaryStubs.cs` declares the compile-only boundary type. Keep the stub out of production runtime sources. |
| `DebugUtils` / `Process.get_ProcessName` exception from direct MSBuild | Runner startup | Prefer the pinned `dotnet` CLI with the process shim. Do not record it as a compiler diagnostic. |
| `MSB1025` from direct MSBuild while the runner cannot read process metadata | Runner startup | Use the pinned `dotnet` CLI with the existing process shim; do not repeatedly retry direct MSBuild or edit project/source files. |
| `wasm-tools` workload optimization warning with successful publish | Optional workload warning | Record the warning; it did not prevent the verified publish. Install workloads only if a later required gate specifically needs them. |
| `package_wasm_site.py` reports a missing `game/vendor/physx-js-webidl.mjs` | Generated browser dependency | Run the lockfile-pinned `npm ci --prefix game --ignore-scripts` and `npm run --prefix game vendor`, then rebuild to a new package output directory. |
| `--no-build` check output differs from changed check source | Stale check harness | Build that exact Checks `.csproj` before rerunning its binary; rebuilding only `Crisis.Runtime.csproj` does not rebuild a separate check executable. |
| `NullReferenceException` in original `PlayerController.Update` at `InteriorMode.UI.InteractionMenu.Visible` | Headless fixture omitted original InteriorMode UI | Do not classify as an input/compiler failure. The controller reads the authored menu before completing movement input; verify that the original UI was initialized before the callback. On 2026-10-23, activating the entire authored UI and manually invoking `Awake` stalled the headless mission during actor spawning, so do not repeat that workaround without isolating its startup dependency. |
| `NotSupportedException: Sheared capsule transforms require native PhysX shape geometry` during projectile raycast | Runtime query geometry limitation | Preserve the exact source transforms and record the missing native PhysX dependency. Do not silently replace the ray hit or deform the actor hierarchy. The original FullSource projectile-to-limb callbacks exist; this exception is in the analytic capsule query path. |
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

Before running a mission contract, reuse the verified original-source scene
package already generated in the current workspace. Do not re-extract it for
each contract. The trimmed WASM bundle
(`scene-data/crisis-response-scenes.json.gz`) omits
`Assets/Resources/Global context.prefab` and is not a valid input for contracts
that instantiate Global Context; an error such as “asset not found” means the
wrong package was selected. Generate the source package once from the
checksum-pinned recovery project with `scripts/extract_runtime_scene.py`, then
reuse that output for subsequent mission, AI, projectile, and outcome commands.
If the workspace or package is new, recheck the recovery archive SHA-256 before
extracting. Keep generated source packages and private recovery archives out of
Git. When an error is actionable, fix it and continue this gate pass; preserve
the successful resolution here for reuse.

## Durable original recovery archive

The authoritative original project recovery archive is privately saved in the
ChatGPT Library as `crisis-response-recovery-direct-v3-1.zip`. Search the
Library by that exact filename when a fresh workspace needs the archive, then
materialize it locally and verify SHA-256
`cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d` before
extracting. This is the same archive formerly available as GitHub Actions
artifact `11903811738` from the private release
`recovery-preservation-2026-10-15`. The pinned archive was recovered and
checksum-verified on 2026-10-24, then saved to the Library because the previous
instructions referred only to ephemeral `/tmp` paths. Never commit this
archive or its extracted proprietary game project to the public repository.

After verification, extract only the exported project tree needed by the
runtime scene extractor, generate `/tmp/cpr-scenes-full-source.json`, and
reuse that generated package for contracts in the current workspace. The
archive is persistent; extracted files and generated scene packages remain
workspace-local and must be regenerated when absent.

Then use the pinned SDK and process shim from this runbook:

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

## Gate 6 input and projectile path inspection

On 2026-10-23, the one bounded input probe reached the original
`PlayerController.Update` and failed because the headless fixture had disabled
`InteriorMode.UI`; the original controller dereferences
`UI.InteractionMenu.Visible` before it finishes processing movement. A retry
that activated the authored UI and called its original `Awake` stalled while
the mission spawned actors. The temporary probe was removed. Treat this as a
headless UI/bootstrap dependency, not a compiler error or a broken input map.
Do not repeat full UI activation until its startup callbacks are isolated.

The live projectile path has a different boundary: original
`Crisis.Runtime.FullSource.Projectile.Update` raycasts and dispatches to its
implemented `HandleLimbHit`, then original `Limb.HandleProjectile` and
`Limb.HandleInterior` apply material interaction, trauma, physiology/vitals,
shock, and wounds. `runtime/Engine/ProjectileFlightHost.cs` contains an
unsupported `HandleLimbHit` only for the separate global/transport projectile
partial; it is not the implementation used by the recovered FullSource
projectile. Original entry/exit tracing requires `LimbRenderer.Awake` to find
its child `MeshRenderer` and parent collider, a non-empty authored `skins` list
with readable pixel data, and `MissionHelper.Instance.RenderMaterialInstance`;
`LimbTextureData.Initialize` builds its trace mask from skin alpha. A live test
also requires a successful ray query. The headless analytic capsule query
rejects sheared transforms with `NotSupportedException` because exact native
PhysX shape geometry is unavailable. Keep this gate partial until those source
setup and query prerequisites can be exercised without replacing the original
hit path.

## Recovered AI source closure and latest Gate 2–6 preflight

Before compiling the recovered original Behavior Designer actor tasks, keep the
following fixes in place:

| Failure | Fix to apply before the next build |
| --- | --- |
| Hundreds of `CS0104` ambiguities for `Action` or `TaskStatus` in original author tasks | Add explicit aliases to the authored task source (`System.Action` where required and `BehaviorDesigner.Runtime.Tasks.TaskStatus`); preserve task declarations and rule bodies. |
| `CS0246` for SharedTarget, SharedDoor, SharedAudioStimuli, SharedFleeSource, SharedGameObject, or SharedGrenade | Restore the original typed shared-variable definitions from the pinned source archive and include them in the full runtime compile. |
| Missing `StaticBehavior`, `DoorReference`, `ConditionalEvaluator`, `EntryTask`, or `RandomProbability` | Restore the original source/dependency with its `.meta` identity and record its source provenance; do not create a no-op stub to make compilation green. |
| `CS1061` for `Collider.tag` in original door/obstacle tasks | Implement the Unity-compatible `Collider.tag` bridge from its owning GameObject. |
| FoundationChecks starts compiling restored actor tasks and fails `CS0246: ArmIKSolver` | Keep `../Original/CharacterBehavior/**/*.cs` out of the FoundationChecks compile glob. The complete runtime owns that source closure; FoundationChecks tests a narrower engine/foundation boundary. |
| Runtime compiles, but original AI throws `NullReferenceException` in `ConditionalEvaluator.OnUpdate` at `conditionalTask.OnUpdate()` | This is a runtime Behavior Designer graph/reference failure, not a C# compile error. Inspect task deserialization and authored embedded condition references; do not hide it by claiming AI acceptance or silently replacing the task. |
| Original `Mission.EndMission` computes outcome, then throws `NullReferenceException` in `MissionUI.PauseMenu` | Initialize the original MissionUI dependencies for full mission acceptance. A headless contract may record mission state before that presentation failure, but cannot count the transition as passed. |
| FoundationChecks reports `CS0246 SharedGameObject` in restored `RestartBehaviorTree.cs` | The restored gameplay task is outside FoundationChecks' focused compile boundary. Exclude `../Original/BehaviorDesigner/BehaviorDesigner.Runtime.Tasks/RestartBehaviorTree.cs` in `Crisis.Runtime.FoundationChecks.csproj`; keep it compiled by the complete runtime project. |
| Original `LimbAudio.Update` throws `NotSupportedException: Decoded original audio playback driver is not bound` during the headless AI mission contract | New AudioSource components can be instantiated after initial scene setup. Bind the silent test driver for every AudioSource in the per-tick headless adapter callback. This enables source execution but does not claim decoded or audible playback. |
| Mission outcome contract returns without a visible authored end menu | The fixture had deactivated the original MissionUI root, so children remained inactive in hierarchy. Reactivate that original root before its Awake path, then assert the authored `MissionEndMenu.Visible` state after `EndMission`. |

The 2026-10-23 Gate 2–6 run used the pinned SDK 8.0.426 and the process shim.
The full runtime compiled with 0 errors and 48 warnings; General Checks passed,
EngineChecks passed 69 assertions, and FoundationChecks passed 121 assertions.
The CharacterBehavior-specific pinned archive audit passed all 265 source and
metadata records. The broader pre-existing
`verify_foundation_sources.py --archive` remains blocked by stale source
provenance entries outside this recovered AI source set; do not report that
audit as passing. Later 2026-10-23 Behavior Designer recovery added one source
record; see `docs/evidence/GATES_2_TO_6_PROGRESS_2026-10-23.md` for the current
gate status and verification results.
