# Original room visibility and camera effects closure — 2026-10-18

Executable source: `517ce292cf2c668a9555cfa342e8d382e02b70d8` (tree `1ca28a0457f8c875a346d03823570ae012e1c939`), branch `feature/full-source-characterbody-integration`, PR #42.

Recovered the original `FogOfWarElement` and `InteriorCamera`, plus the four Unity Standard Assets effects that the camera enables: `NoiseAndGrain`, `BloomOptimized`, `CameraMotionBlur` and `DepthOfField`, with their original `PostEffectsBase`. The plugin sources keep their original `UnityStandardAssets.ImageEffects` namespace and match the pinned archive byte for byte. The Unity shader/compute adapter retains metadata and typed buffer state, returns shaders only when a host resolver supplies them, and reports unsupported compute execution when no native driver is bound. No original source files were excluded from `Crisis.Runtime.csproj`.

The normal full project build exits **1** with **19 error occurrences / 15 missing names**. The prior source checkpoint had 19 occurrences / 16 names. `FogOfWarElement` now resolves; the original `InteriorCamera` adds another reference to the already missing `InteriorMode`. The diagnostic diff records one occurrence resolved and one introduced, with **zero new missing names** and no remaining image-effect API errors. The `MarksmanMode` static/instance conflict remains. Unresolved declarations may hide further errors.

At the exact executable SHA, isolated EngineChecks pass **60 assertions**, FoundationChecks pass **120 assertions**, original-source provenance passes **220 identities**, and eight audit tests, dependency inventory freshness, actor migration and strict ownership checks pass. Inventory: **384 sources, 1,631 symbols, 1,063 conservatively reachable files, 86 ambiguous names**. Focused projects were compiled using the installed .NET 8 Roslyn compiler and run by its host because the container's `dotnet run`/MSBuild entry points fail before compilation on process-stat access.

There is no Actions run for this executable SHA. Latest comparable full-source Actions build remains failed run 39091701268 at `407c66d`; narrow remediation run 39093133256 at `389d8e8` passed its focused checks only. No full compile success is claimed.

[Exact-source logs, diagnostic diff, test outputs and checksums](evidence/room-camera-closure-20261018/). Next compiler work returns to original mission modes, UI, owners and the `MarksmanMode` conflict. Character initialization, combat execution and browser integration remain separate verification gates.
