# Original explosive and grenade dependencies — 2026-10-18

Full original CharacterBody compiler closure remains incomplete. Executable
source `f2ef153edee7bbd3239e1e19e02e1846dc69f865` (tree
`7ddc70677fbc7c474db2908aa075c2a2ae987cd6`) builds through the normal
`runtime/Crisis.Runtime.csproj` project and exits **1** with **28 diagnostics / 22
missing names**. The preceding executable source `a4f36ebc7b1ef6ab2bf045c52dfbd83a457117ad`
had 29 diagnostics / 23 names. One original grenade-spoon reference is resolved;
no diagnostics were introduced.

## Live repository state and CI comparison

Before this source batch, PR #42 was open at
`15dba449151cf9d53344705e8155d14fe98884ab`, with `develop/wasm` at
`2fc0a8e93be97c4d99123910fad4bd25798e71a8`. Open PRs remained #7, #42, #56 and
#57. PRs #56 and #57 had no changes to the original `GrenadeSpoon` or `Explosive`
sources. PR #56 continues its separate projectile/limb injury work. PR #7 remains
an older movement lane with stale engine-file overlap; no branch was rebased or
merged. No Actions workflow is associated with this exact source commit.

The latest comparable full-source Actions build is still failed run 39091701268
at `407c66d026784b72c96490eabae6085dd87c22e6` (144 errors / 60 names). The
narrower remediation run 39093133256 at `389d8e8dcc3cc49d4327cc55588d874271dc4271`
passed its isolated checks, but it did not test this source. Locally, each
successive executable source is compared with the immediately prior full build;
this 28-error checkpoint supersedes the previous 29-error checkpoint for current
progress, while the historical CI count remains a separate graph.

## Recovered source and verification

The complete original `Explosive.cs` and `GrenadeSpoon.cs` are recovered from the
SHA-pinned game archive. Their method bodies and order are byte-identical after
removing the documented namespace wrapper and Unity aliases. No temporary
detonation or grenade-spoon system was added.

| Gate | Result |
| --- | --- |
| Normal `Crisis.Runtime.csproj` build | **FAIL, exit 1; 28 diagnostics / 22 names** |
| Diagnostic comparison with `a4f36eb` | **1 resolved; 0 introduced** |
| Original/decompiled source identities | Pass, **206 identities** |
| FoundationChecks contracts | Pass, **120 assertions** |
| Isolated EngineChecks | Pass, **52 assertions** |
| Inventory, audit tests, actor migration and ownership | Pass; zero ownership violations |

No engine sources changed in this batch; the foundation and engine test binaries
were rerun against those unchanged engine sources. The normal full runtime build
compiled the entire current project graph and its failing exit status is
preserved. Source identities, diagnostics, tests, inventory, actor guards and
manifest are stored under `docs/evidence/grenade-dependencies-20261018/`, pinned
to source `f2ef153`.

The remaining missing names are `Benchmark`, `BloodStream`,
`BloodStreamGenerator`, `CellPhone`, `DoorTrap`, `DynamicLimbDecalRenderer`,
`FloorPoolPainter`, `FogOfWarElement`, `InteriorMode`, `InteriorModeUI`,
`Marksman`, `Marksmen`, `MissionUI`, `Mixer`, `NegotiationMode`, `Occupants`,
`Operators`, `PaintableSurface`, `ScoreKeeper`, `TutorialMode`, `Usage` and
`WallStreakPainter`. The `MarksmanMode` static/instance identity conflict also
remains. `DoorTrap` still requires original operator/game-state owners; it has not
been replaced by a temporary implementation.

No successful full compile, current-SHA Actions run, original actor initialization,
combat, or browser result is claimed. The next acceptance gate remains a
zero-error normal runtime build and Actions verification at that exact SHA.
Character ownership unification, two-character combat and WASM/browser checks
remain subsequent, separate gates.
