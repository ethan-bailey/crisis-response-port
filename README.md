# Crisis Response — iPhone Web Port Research

This repository tracks a source-preserving, evidence-driven effort to make the original *Crisis Response* playable on iPhone 16 through a web runtime if feasible.

## Important distinction

The original archive has been extracted and inventoried in the current working environment. See [the verified archive inventory](docs/ARCHIVE_INVENTORY.md).

A separate generated file named `CrisisResponseMobile_iPhone.zip` is a from-scratch HTML canvas adaptation. It explicitly does not contain the original Windows executable or original game assets. It is not a faithful port and must not be represented as one.

## Start here

- [Verified recovery results](docs/RECOVERY_RUN_2026-10-14.md) — the matching game assembly was decompiled and AssetRipper recovered a 7,231-file Unity project.
- [Cloud WebGL build gate](.github/workflows/unity-webgl-build.yml) — downloads the verified recovery artifact and attempts a licensed WebGL build; Unity licensing and mobile parity tests remain required.
- [Unity license gate instructions](docs/UNITY_BUILD_GATE.md) — explains the missing build activation requirement and how to configure repository secrets without sharing credentials in chat.
- [Reverse-engineering plan](docs/REVERSE_ENGINEERING_PLAN.md)
- [Verified archive inventory](docs/ARCHIVE_INVENTORY.md)
- [Subsystem parity matrix and porting status](docs/PORTING_STATUS.md)
- [WebGL compatibility audit](docs/WEBGL_COMPATIBILITY_AUDIT_2026-10-14.md) — static scan of the recovered project for likely browser build blockers.

## Current state

- Original archive extracted: 415 files, 204,344,165 bytes.
- Verified engine/player: Unity 2017.2.0f3, Windows x86 Mono.
- Verified gameplay assembly metadata: 1,534 types and 8,640 methods in `Assembly-CSharp.dll`.
- Original C# source and editable Unity project structure are not present in the archive.
- Cloud recovery completed successfully using a signed official mirror. The downloaded gameplay DLL exactly matches the SHA-256 recorded from the user's archive; ILSpy produced 1,333 C# files and AssetRipper exported a 7,231-file Unity project. See the recovery report for the artifact link and remaining gates.
- No verified faithful WebGL build exists yet; decompilation, asset export, reconstruction, compilation, and behavioral tests remain.

The goal is to preserve original logic and assets wherever possible, document unavoidable changes, and validate behavior subsystem by subsystem. Do not claim full parity without evidence and tests.
