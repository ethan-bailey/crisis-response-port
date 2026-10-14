# Crisis Response — iPhone Web Port Research

This repository tracks a source-preserving, evidence-driven effort to make the original *Crisis Response* playable on iPhone 16 through a web runtime if feasible.

## Important distinction

The original archive has been extracted and inventoried in the current working environment. See [the verified archive inventory](docs/ARCHIVE_INVENTORY.md).

A separate generated file named `CrisisResponseMobile_iPhone.zip` is a from-scratch HTML canvas adaptation. It explicitly does not contain the original Windows executable or original game assets. It is not a faithful port and must not be represented as one.

## Start here

- [Cloud recovery pipeline](.github/workflows/recover-moddb.yml) — downloads the publicly hosted Sandbox 7 installer in GitHub Actions, verifies its MD5 against ModDB and its gameplay DLL against the exact SHA-256 from the user's archive, then attempts C# decompilation and Unity asset/project recovery. A mismatch intentionally stops the pipeline.
- [Reverse-engineering plan](docs/REVERSE_ENGINEERING_PLAN.md)
- [Verified archive inventory](docs/ARCHIVE_INVENTORY.md)
- [Subsystem parity matrix and porting status](docs/PORTING_STATUS.md)

## Current state

- Original archive extracted: 415 files, 204,344,165 bytes.
- Verified engine/player: Unity 2017.2.0f3, Windows x86 Mono.
- Verified gameplay assembly metadata: 1,534 types and 8,640 methods in `Assembly-CSharp.dll`.
- Original C# source and editable Unity project structure are not present in the archive.
- Cloud recovery is now being attempted using the public ModDB mirror because the game's older direct ZIP host has been parked. The cloud job must still prove the downloaded gameplay DLL is byte-for-byte identical before any recovery output is treated as relevant.
- No verified faithful WebGL build exists yet; decompilation, asset export, reconstruction, compilation, and behavioral tests remain.

The goal is to preserve original logic and assets wherever possible, document unavoidable changes, and validate behavior subsystem by subsystem. Do not claim full parity without evidence and tests.
