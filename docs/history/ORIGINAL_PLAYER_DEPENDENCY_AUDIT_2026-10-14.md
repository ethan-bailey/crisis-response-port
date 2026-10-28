# Crisis Response original-player dependency audit — 2026-10-14

## Decision

**NO-GO for investing in Wine-Assembly as the primary route at this time.** The actual player is a 32-bit Unity 2017 Mono game with a large native UnityPlayer dependency surface, Win32 device/window APIs, OpenGL/WGL imports, audio APIs, sockets/network APIs, and substantial thread/file/memory synchronization use. This is much broader than establishing that an x86 interpreter can parse PE32.

This is an evidence-based feasibility decision, not a runtime test: Wine-Assembly was not run with the game, and no claim is made that the original executable launches in a browser.

## Source and integrity

- Original archive: `Crisis Response.rar`, 67,925,884 bytes.
- Archive SHA-256: `5a12ff494b26713d149f9be026619732247f817ddf8944a399b6ee1d12ea0ab3` (matches `docs/ARCHIVE_INVENTORY.md`).
- Extracted with system libarchive into a private working directory; 415 regular files / 204,344,165 bytes, matching the recorded inventory.
- The archive remains unmodified. No game binaries or assets were committed or uploaded to a public runtime/demo.

## Direct PE findings

| File | Size | SHA-256 | PE format |
|---|---:|---|---|
| `Crisis Response.exe` | 641,024 bytes | `d4fec2d365c1a5c04fd385f51b815bf80cf39e0858fcaf497ae1a448a09b89f1` | PE32 / Intel i386 / Windows GUI |
| `UnityPlayer.dll` | 18,244,544 bytes | `30a16cbb616ffee4d430e948f484f5689379caccdd070bfb213efd629f581b60` | PE32 / Intel i386 / Windows GUI |
| `Crisis Response_Data/Mono/EmbedRuntime/mono.dll` | 2,118,592 bytes | `ae7543ee31d3225f4e73b1f854d9a9ac61e3f80c3d3b2f7eb890ca87a6756c79` | PE32 / Intel i386 / Windows DLL |
| `Crisis Response_Data/Mono/EmbedRuntime/MonoPosixHelper.dll` | 101,888 bytes | `591b5aa2ef26171f040fc0e8fadbb0cc4f99c44612e0b9378d54823a7d4aedea` | PE32 / Intel i386 / Windows DLL |

The executable directly imports `UnityPlayer.dll`, `KERNEL32.dll`, and `ADVAPI32.dll`. UnityPlayer's normal import table names 18 Windows DLL families; Mono imports 11. This is not a self-contained managed executable.

## UnityPlayer native dependency surface

Normal imports observed in `UnityPlayer.dll` include:

- Input/device/windowing: `HID.DLL`, `USER32.dll`, `SETUPAPI.dll`, `IMM32.dll`.
- Graphics/window contexts: `OPENGL32.dll` with WGL context creation/current-context/proc-address calls; `GDI32.dll` with pixel-format and buffer-swap calls.
- Audio/timing: `WINMM.dll` with wave input/output and multimedia timer functions.
- Core runtime: `KERNEL32.dll` with file operations, virtual memory, events, mutexes, semaphores, critical sections, TLS, threads, process/thread context, synchronization, and environment/path APIs.
- OS integration/configuration: `ADVAPI32.dll`, `SHELL32.dll`, `SHLWAPI.dll`, `VERSION.dll`, `ole32.dll`, `OLEAUT32.dll`.
- Network/proxy discovery: `WS2_32.dll`, `DNSAPI.dll`, `IPHLPAPI.DLL`, `WINHTTP.dll`.

The `UnityPlayer.dll` binary also contains the string `dxgi.dll`, suggesting graphics modules may be loaded dynamically. A normal import table is not a complete list of runtime-loaded DLLs or graphics paths. The import table does show OpenGL/WGL and GDI entry points, but does not by itself prove which renderer the game selects at startup.

The embedded Mono runtime also imports Winsock, PSAPI, WinMM, MSWSOCK, process/token/security, shell, COM, and extensive Kernel32 APIs. `MonoPosixHelper.dll` is present.

## Compatibility implications

1. **PE32:** architecture matches a 32-bit x86 interpreter in principle, but this alone is a weak pass.
2. **Unity/Mono startup:** the game needs the Unity native player plus its embedded Mono runtime, managed assemblies, Unity data, and native OS API behavior. No game-specific Wine-Assembly compatibility proof was found.
3. **Graphics:** WGL/OpenGL and GDI imports are confirmed. The exact active renderer and any dynamically loaded D3D/DXGI requirements are still unverified. Wine-Assembly's documented DirectX-era support is not enough to establish Unity 2017 renderer compatibility.
4. **Audio/input:** WinMM wave APIs, raw input, HID, and IMM32 imports create further implementation requirements. Touch input in Safari would also need a deliberate mapping.
5. **Networking/system APIs:** Winsock, DNS, IP helper, WinHTTP, SetupAPI, and registry/security APIs expand the compatibility surface.
6. **Threads/memory/files:** the player and Mono rely on Windows thread synchronization, virtual memory, file mapping, file I/O, TLS, and process APIs. These are core runtime behavior, not optional game features.
7. **iPhone Safari:** not tested; performance, memory, graphics translation, browser lifecycle, audio policies, and touch remain unknown.

## Go/no-go gate

| Gate | Result |
|---|---|
| Confirm original architecture | **PASS: PE32 / Intel i386** |
| Inventory native DLL dependencies | **PASS: initial static import inventory recorded above** |
| Establish Wine-Assembly support for all critical APIs | **FAIL / not established** |
| Establish Unity 2017 + embedded Mono startup compatibility | **UNKNOWN** |
| Establish usable 3D renderer path | **UNKNOWN** |
| Private original-game smoke test | **NOT RUN** |
| iPhone Safari execution | **NOT TESTED** |
| Reach playable mission | **NOT TESTED** |

**Recommendation:** stop short of building or forking a large compatibility layer. Keep the executable route as a small optional experiment only if the runtime's API/export tables can be checked against these concrete imports and dynamic-loading paths. The more defensible main route is the recovered Unity project: use a valid Unity build environment when possible, or investigate source-port feasibility against the recovered scenes/assets and decompiled C# without pretending those scripts are engine-independent.

## Next milestone

The static executable audit is now complete. The next meaningful task is not more general research: it is either (a) a private, time-boxed runtime smoke test with a verified graphics path, or (b) improving the recovered Unity project build path. Since Unity activation is currently unavailable and Wine-Assembly's critical API compatibility remains unproven, prioritize a source/project audit that isolates which recovered scripts and project settings block a modern WebGL build. No build should be labeled successful until it compiles and runs.
