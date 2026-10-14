# Compatibility-runtime feasibility gate — 2026-10-14

## Decision

**Do not attempt to run Crisis Response in Wine-Assembly as the primary port path yet.** Keep it as a bounded experiment, but it has not passed the minimum compatibility gate. No game files have been uploaded to the public demo, and no claim is made that the game launches.

## Evidence checked

- The recovered game is Unity 2017.2.0f3, Windows x86, Mono scripting backend. The recovery report records a gameplay assembly SHA-256 matching the user's original archive.
- Unity 2017 documents `-force-d3d9` and `-force-d3d11` as Windows player options. A D3D9 fallback is therefore worth testing only if the original player supports it.
- Wine-Assembly's own README describes a Windows 98 Win32 emulator, an x86 interpreter and a reimplemented Win32 API. It explicitly says heavier 3D titles do not yet run well.
- Wine-Assembly has documented DirectX 5–7 and Direct3D 9 coverage, but this is not proof that Unity 2017's player startup, Mono runtime, D3D9 device creation, shader model, threading, file access, or Unity native dependencies are supported.

References:
- https://github.com/vgrichina/berrry-wine
- https://github.com/vgrichina/berrry-wine/blob/main/articles/loading-real-windows-dlls-in-the-browser.md
- https://docs.unity.cn/2017.2/Documentation/Manual/CommandLineArguments.html
- ../RECOVERY_RUN_2026-10-14.md (same-directory link in repository)

## Gate result

| Requirement | Evidence/status |
|---|---|
| 32-bit PE support | Wine-Assembly advertises Win32; game-specific import compatibility not checked |
| Unity 2017 player startup / Mono | Unknown; no game-specific smoke test |
| Graphics API | D3D9 is a possible experiment; D3D11 is not established by the inspected compatibility evidence |
| Unity engine native DLLs | Unknown; needs import and dependency inventory |
| Touch input / audio / persistence | No game-specific test |
| Runs on iPhone Safari | Not tested |
| Original game reaches a playable mission | Not tested |

**Conclusion:** compatibility runtime remains unproven and should not be described as a working port. The evidence does not justify investing in a large emulator fork before inspecting the actual player executable and import tables.

## Next executable task

1. Obtain the original archive privately in a cloud runner; do not commit or upload the archive or game binaries to a public repository.
2. Inventory the player EXE and adjacent DLLs: PE architecture, imported DLLs/functions, UnityPlayer version, graphics/runtime DLLs, Mono files, and any required data folders.
3. Compare the exact imports and startup requirements with Wine-Assembly's implemented API/export tables.
4. If the executable/import gate looks plausible, run a private minimal smoke test and capture the first unsupported API / graphics failure. Try `-force-d3d9` only as a diagnostic, not as an assumed fix.
5. Stop the compatibility-runtime track if core Unity/Mono or graphics requirements are missing. Continue with the recovered Unity project and the existing WebGL build gate instead.
6. Keep all game binaries private. Publish only reports, hashes, and code that do not redistribute the game.

## Milestone

Research-only feasibility milestone completed: a plausible general technique was identified, but it has **not** passed the game-specific execution gate. The next milestone requires actual executable dependency evidence from the private original archive.
