# Original powered lighting dependencies — 2026-10-18

Full original CharacterBody compiler closure remains incomplete. The executable
source is `a4f36ebc7b1ef6ab2bf045c52dfbd83a457117ad` (tree
`665d5428d4db48082b3d45a6b84dd8c3030ab707`). The normal
`dotnet build runtime/Crisis.Runtime.csproj` exits **1** with **29 diagnostics / 23
missing names**. Compared with the immediately prior full build at
`7b592b18fd8972cd31f03725124b10c7aadc290f` (31 diagnostics / 24 missing names),
the two original `PoweredLight` references are resolved and no diagnostics are
introduced. Later errors remain masked by missing declarations.

## Live checkpoint and overlap review

Before editing, PR #42 was open at `715b6432e295d2f0ed0a78b747155e48fc6c79f9`
and `develop/wasm` was `2fc0a8e93be97c4d99123910fad4bd25798e71a8`. Open PRs were #7,
#42, #56 and #57. PRs #56 and #57 had no light/material API overlap. Older PR #7
contains a historical `runtime/Engine/Objects.cs` version; it will need careful
reconciliation before any integration with that lane. No branch was rebased or
merged.

The latest comparable full-source GitHub Actions compile remains failed run
39091701268 at `407c66d026784b72c96490eabae6085dd87c22e6` (144 errors / 60 names).
The narrower remediation run 39093133256 at `389d8e8dcc3cc49d4327cc55588d874271dc4271`
passed its engine/foundation and ownership checks, but it did not compile this
runtime source. No Actions run is associated with the current commit. For the
local source-to-source diagnostic comparison, the closer `7b592b1` checkpoint
supersedes older 35-, 42-, 64- and 144-error reports.

## Recovered source and engine contracts

`PoweredLight.cs` is recovered byte-for-byte from the verified original archive
after removing only the documented `Crisis.Runtime.FullSource` namespace wrapper.
Its lighting decisions, material updates, room notifications, audio behavior and
ballistic callback ordering remain original.

The runtime now preserves original light enabled/shadow state and forwards those
changes through an optional typed light driver. Material color values remain
readable CPU state and can notify a presentation observer; renderer material
arrays preserve their assigned identities and snapshot boundaries. These APIs do
not claim shader execution or actual scene lighting without a bound renderer.

## Verification

| Gate | Result |
| --- | --- |
| Normal `Crisis.Runtime.csproj` build | **FAIL, exit 1; 29 diagnostics / 23 missing names** |
| Diagnostic comparison with `7b592b1` | **2 resolved; 0 introduced** |
| EngineChecks compile via direct Roslyn invocation | Pass, all project source inputs compiled |
| Isolated engine contracts | Pass, **52 assertions** |
| FoundationChecks rebuild and run | Pass, **120 assertions** |
| Original/decompiled source identity | Pass, **204 identities** |
| Inventory, eight audit tests and actor/ownership guards | Pass; zero violations |

The current compile diagnostics, project log and exit code, Roslyn command log,
focused test output, inventory, ownership checks and SHA256 manifest are in
`docs/evidence/powered-light-closure-20261018/`, pinned to source `a4f36eb`.
The normal full runtime project build completed its compilation and retained its
actual failing exit status. EngineChecks was compiled with the SDK's Roslyn
compiler and then executed; its separate MSBuild startup path was not required for
that result.

The remaining missing names are `Benchmark`, `BloodStream`,
`BloodStreamGenerator`, `CellPhone`, `DoorTrap`, `DynamicLimbDecalRenderer`,
`FloorPoolPainter`, `FogOfWarElement`, `GrenadeSpoon`, `InteriorMode`,
`InteriorModeUI`, `Marksman`, `Marksmen`, `MissionUI`, `Mixer`,
`NegotiationMode`, `Occupants`, `Operators`, `PaintableSurface`, `ScoreKeeper`,
`TutorialMode`, `Usage` and `WallStreakPainter`. The `MarksmanMode`
static/instance identity conflict also remains. The original fog and trap systems,
mission owners and modes, blood/decal painters, scoring, audio mix and telemetry
need their actual dependency chains.

No original files are excluded, no temporary actor is substituted, and no frozen
transport gameplay file changed. No full compile success, current-SHA Actions run,
initialization, combat or browser result is claimed. The next acceptance gate is
still a zero-error normal runtime build, followed by Actions verification at that
exact source SHA. Two-character gameplay and WASM/browser checks remain separate.
