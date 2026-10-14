# Crisis Response Web Port — Reverse-Engineering Plan

## Source artifact status

The original user-provided archive is available in the ChatGPT Library as `Crisis Response.rar` (RAR v5, 67,925,884 bytes). It is not necessary to ask the user to upload it again.

A previously generated artifact, `CrisisResponseMobile_iPhone.zip`, contains a standalone HTML canvas adaptation. Its own README says it was built from scratch and does **not** embed the original executable or original assets. Treat it only as a separate prototype/reference, not as a faithful port and not as evidence of behavioral parity.

## Known source-build facts

Prior inspection reported a Unity 2017.2.0f3 Windows build containing `Assembly-CSharp.dll`, with approximately 1,534 types and 8,640 methods, plus Unity data/assets, managed assemblies, Mono runtime files, and 44 `.mdb` debug-symbol files. These counts are prior inspection notes and must be verified against the archive before being treated as final inventory.

The original Unity project and original C# source files were not present in the inspected Windows build. Decompiled code, if used, must be labeled as reconstructed/decompiled and compared against observed behavior.

## Non-negotiable goals

1. Preserve original code and assets wherever technically possible; do not silently replace game systems with invented equivalents.
2. Establish an inventory and hashes of all archive contents before changing or transforming them.
3. Decompile managed assemblies for analysis, retaining original binaries unchanged.
4. Identify Unity-specific dependencies and distinguish reusable game logic from Windows/Unity runtime dependencies.
5. Prefer a WebGL/Unity-compatible path if the original project/assets and a usable build toolchain can support it. If not, document each blocker and evaluate a minimal compatibility layer; use Rust only where there is a concrete need.
6. Prioritize verified behavioral parity. Do not claim “100% parity” until each subsystem has evidence and tests.
7. Target iPhone 16 Safari, touch controls, responsive layout, and installability only after the original game's runtime path is understood.

## Work sequence

### Phase 1 — Archive integrity and inventory
- Extract the RAR in a suitable development workspace.
- Record the archive hash, extraction tool/version, file count, total extracted size, and per-file SHA-256 hashes.
- Inventory executable, DLLs, Unity data files, scenes/assets, audio, textures, configuration, and debug symbols.
- Keep the original archive immutable.

### Phase 2 — Runtime and code reconnaissance
- Inspect Unity version, player architecture, managed assembly references, plugins, scenes, and resource bundles.
- Decompile `Assembly-CSharp.dll` for analysis; preserve the binary and record tool/version.
- Map key systems: AI, teams/missions, doors, weapons, projectiles/ballistics, grenades, physiology, UI, save/config, and input.
- Identify all native plugins and Windows-only APIs.

### Phase 3 — Feasibility gate
- Determine whether the original Unity project can be reconstructed sufficiently for a supported WebGL build.
- If the available artifact is only a compiled Windows player, explicitly list missing project metadata/assets and determine what can be recovered.
- Do not build a from-scratch game as a substitute for the original without clearly marking that work as a non-parity prototype.

### Phase 4 — Port and parity validation
- Implement the smallest viable changes needed for the selected runtime.
- Create a subsystem parity matrix linking original behavior, evidence, implementation, and test status.
- Compare deterministic scenarios and capture discrepancies before fixing them.
- Test touch input, viewport/resolution, audio, performance, and reload/resume behavior on mobile Safari.

## Current status

- [x] Private GitHub repository access verified.
- [x] Original RAR located in the ChatGPT Library.
- [x] Earlier mobile HTML prototype ZIP located and identified as a from-scratch adaptation.
- [ ] Extract and inventory the original archive in the development workspace.
- [ ] Verify archive facts and assembly inventory.
- [ ] Produce the decompilation and subsystem map.
- [ ] Select and validate the actual porting route.
- [ ] Implement and test the port.

## Important limitation

Repository access does not itself make the 67.9 MB archive available inside a cloud build workspace. The archive is available to ChatGPT's file tools, but it still needs to be transferred into the chosen build environment for compilation and automated testing. Do not claim extraction or a playable faithful port is complete until the actual extracted files and tests confirm it.
