# Original recovery run — 2026-10-14

## Verified source match

The official Sandbox 7 installer was downloaded from a signed DBolical/IndieDB mirror without executing it.

- Installer filename: `cr-setup-0.0.9.7919f.exe`
- Installer size: 61,809,961 bytes
- Published ModDB MD5 and observed MD5: `9e00a2b43b7140aaac57c828bd6566ce`
- Extracted `Assembly-CSharp.dll` SHA-256: `482f8bd6ef4546965b060463377470fc2da2db996de7ec16f29eb88f56a021b3`
- That assembly SHA-256 exactly matches the assembly extracted from the user's original `Crisis Response.rar`.

## Recovery results

- Decompiled gameplay assembly: **1,333 C# files** produced by ILSpy.
- AssetRipper CLI: built successfully from source in the GitHub Actions runner.
- AssetRipper export: completed successfully.
- Recovered Unity project: **7,231 files, about 169 MiB**.
- Recovered project version: Unity **2017.2.0f3**.
- Recovered scenes in build settings: Disclaimer, Title, Transition, Mission 01, Marksman range, Benchmark.
- The project includes recovered `Assets/`, `ProjectSettings/`, `Packages/manifest.json`, scripts, textures, sprites, audio, materials, prefabs/game objects, and scene files.

## Artifact

The full recovery artifact (about 97 MiB compressed) is attached to this run and retained by GitHub Actions for 14 days:

- [Recovery run and artifact](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38582072502)

Artifact name: `crisis-response-recovery-direct-v3-1`

It contains the decompiled C# source, the recovered Unity project, and reports/logs. The original installer and original archive were not committed to the repository.

## Not yet complete

This is not yet a playable iPhone app. The recovered Unity project has not been compiled to WebGL, run in Safari, or compared against the original game in deterministic scenarios. The next gate is a licensed Unity WebGL build, followed by script/API repair if the recovered project does not compile, touch-control adaptation, and mobile behavioral/performance tests.

Decompiled C# is reconstructed source, not the developer's original source code. AssetRipper's project is a recovery artifact and may require manual repair.
