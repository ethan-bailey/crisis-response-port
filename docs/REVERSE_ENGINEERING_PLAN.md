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
- [x] Decompile `Assembly-CSharp.dll` with ILSpy in a cloud runner; 1,333 C# files were produced.
- [x] AssetRipper decompiled and exported the game assemblies as part of project recovery.
- [x] Map key systems at symbol level; deeper source review and behavior tracing remain.
- [ ] Identify all native plugins and Windows-only APIs.

### Phase 3 — Feasibility gate
- [x] Export Unity assets/scenes; 7,231 files (about 169 MiB) were recovered.
- [x] Reconstruct an initial Unity project with `Assets/`, `ProjectSettings/`, `Packages/manifest.json`, and six scenes.
- [ ] Compile and repair the recovered project.
- [ ] Document missing project metadata/assets and each blocker.
- [ ] Validate a compatible Unity Editor/toolchain before selecting the final route.

### Phase 4 — Port and parity validation
- [ ] Obtain valid Unity build activation through GitHub Actions secrets, then compile the recovered project for WebGL.
- [ ] Create a subsystem parity matrix linking original behavior, evidence, implementation, and test status.
- [ ] Compare deterministic scenarios and capture discrepancies before fixing them.
- [ ] Test touch input, viewport/resolution, audio, performance, and reload/resume behavior on mobile Safari.

## Current status

- [x] Private GitHub repository access verified.
- [x] Original RAR located and extracted in the working environment.
- [x] Earlier mobile HTML prototype identified as a from-scratch adaptation, not the original game.
- [x] Verified archive inventory committed.
- [x] Subsystem parity matrix and porting gates committed.
- [x] C# decompilation and Unity asset export.
- [x] Initial reconstructed Unity project.
- [ ] WebGL build; currently blocked by missing Unity license secrets.
- [ ] iPhone Safari testing and subsystem parity verification.

See [the porting status matrix](history/PORTING_STATUS.md) for the evidence and outstanding work for each subsystem.

## Current toolchain limitation

The current cloud workflow has successfully downloaded a publicly hosted installer whose gameplay assembly exactly matches the user's original archive, decompiled the assembly with ILSpy, and exported a Unity project with AssetRipper. The recovery artifact is linked in `docs/RECOVERY_RUN_2026-10-14.md`. The WebGL build workflow can fetch that artifact, but the current Unity license activation action is retired and no valid Unity license secrets are configured. Do not claim a playable faithful port until a licensed WebGL build and iPhone Safari parity tests succeed.
