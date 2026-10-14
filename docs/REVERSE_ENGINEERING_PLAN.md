# Crisis Response Web Port — Reverse-Engineering Plan

## Source artifact status

The original user-provided archive is available in the ChatGPT Library as `Crisis Response.rar` (RAR v5, 67,925,884 bytes). It was extracted into the current working environment at `/mnt/data/work/CrisisResponseOriginal/Crisis Response/`. See [the verified archive inventory](ARCHIVE_INVENTORY.md).

A previously generated artifact, `CrisisResponseMobile_iPhone.zip`, contains a standalone HTML canvas adaptation. Its own README says it was built from scratch and does **not** embed the original executable or original assets. Treat it only as a separate prototype/reference, not as a faithful port and not as evidence of behavioral parity.

## Verified source-build facts

- Unity version: `2017.2.0f3`.
- Windows standalone x86 Mono player, including `Crisis Response.exe` and `UnityPlayer.dll`.
- `Assembly-CSharp.dll`: 1,504,768 bytes; 1,534 TypeDef rows, 8,640 MethodDef rows, 9,025 Field rows, 1,112 Property rows.
- Managed folder: 143 files, including 56 DLLs, 44 MDB debug-symbol files, and 43 XML documentation files.
- Archive: 415 regular files extracted, 204,344,165 bytes total. Archive SHA-256 and local per-file hashes are recorded in the inventory/working environment.
- The original Unity project structure and original C# source files are absent from the archive. The source logic exists as compiled managed assemblies and must be decompiled for analysis.

## Non-negotiable goals

1. Preserve original code and assets wherever technically possible; do not silently replace game systems with invented equivalents.
2. Keep the original archive immutable and maintain hashes for extracted files.
3. Decompile managed assemblies for analysis, retaining original binaries unchanged.
4. Identify Unity-specific dependencies and distinguish reusable game logic from Windows/Unity runtime dependencies.
5. Prefer a Unity/WebGL path if recovered assets, decompiled code, and a compatible build toolchain support it. If not, document blockers and evaluate a minimal compatibility layer; use Rust only where there is a concrete need.
6. Prioritize verified behavioral parity. Do not claim “100% parity” until each subsystem has evidence and tests.
7. Target iPhone 16 Safari, touch controls, responsive layout, and installability only after the original game's runtime path is understood.

## Work sequence

### Phase 1 — Archive integrity and inventory
- [x] Extract the RAR in a working environment.
- [x] Record the archive hash, extracted file count/size, and per-file SHA-256 hashes locally.
- [x] Inventory executable, DLLs, Unity data/assets, resource files, configuration, and debug symbols.
- [x] Verify Unity version and managed assembly type/method counts.
- [x] Document the absence of original source/project files.

### Phase 2 — Runtime and code reconnaissance
- [x] Inspect the Unity version, player architecture, managed assembly metadata, and key gameplay symbol names.
- [x] Add a dependency-free metadata inspection script at `tools/inspect_assembly_metadata.py`.
- [ ] Decompile `Assembly-CSharp.dll` and supporting assemblies for analysis.
- [ ] Map key systems in source-like detail: AI, teams/missions, doors, weapons, projectiles/ballistics, grenades, physiology, UI, save/config, and input.
- [ ] Identify all native plugins and Windows-only APIs.

### Phase 3 — Feasibility gate
- [ ] Export Unity assets/scenes and inspect the recovered project structure.
- [ ] Determine whether a Unity project can be reconstructed sufficiently for a supported WebGL build.
- [ ] Document missing project metadata/assets and each blocker.
- [ ] Validate a compatible Unity Editor/toolchain before selecting the final route.

### Phase 4 — Port and parity validation
- [ ] Implement the smallest viable changes needed for the selected runtime.
- [ ] Create a subsystem parity matrix linking original behavior, evidence, implementation, and test status.
- [ ] Compare deterministic scenarios and capture discrepancies before fixing them.
- [ ] Test touch input, viewport/resolution, audio, performance, and reload/resume behavior on mobile Safari.

## Current status

- [x] Private GitHub repository access verified.
- [x] Original RAR located and extracted in the working environment.
- [x] Earlier mobile HTML prototype identified as a from-scratch adaptation, not the original game.
- [x] Verified archive inventory committed.
- [x] Subsystem parity matrix and porting gates committed.
- [ ] C# decompilation and Unity asset export.
- [ ] Reconstructed Unity project and WebGL build.
- [ ] iPhone Safari testing and subsystem parity verification.

See [the porting status matrix](PORTING_STATUS.md) for the evidence and outstanding work for each subsystem.

## Current toolchain limitation

The current execution environment has no Unity Editor, .NET SDK/runtime, `monodis`, or ILSpy CLI, and cannot fetch packages from registries. Full decompilation and a WebGL build are therefore not completed in this environment. The extracted files are available in the current working container, but a separate cloud build environment would need the files transferred into it. Do not claim extraction, decompilation, or a playable faithful port is complete beyond the evidence listed above.
