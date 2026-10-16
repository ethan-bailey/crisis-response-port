# Native Unity WebGL test — evidence review 2026-10-16

## Result that was independently checked

The separate Unity feasibility test **did finish compiling a Marksman-only
WebGL player**, not just importing C#:

- [Actions run 38800542438](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38800542438)
  completed October 15, 2026 at approximately 20:27 UTC (4:27 p.m. Eastern).
- Tested source SHA `4ed43897973d9fac607b4e57baf2cbe4102acf3d` on the separate
  `codex/iphone-input-parity-foundation` branch; this review made **no change**
  to that branch, its workflows, Unity activation or main.
- [Private evidence artifact 12004975378](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38800542438/artifacts/12004975378),
  `unity-marksman-webgl-feasibility-logs`, includes `webgl.log`, status,
  output inventory, output manifest and static HTTP smoke results. Current
  expiry October 29, 2026.
- Actual `webgl.log` says `Build Finished, Result: Success.` and includes the
  IL2CPP/WebGL build pipeline and Brotli WASM output. Not inferred solely from
  the green Actions job or a wrapper exit code.
- Build status: `build_exit=0`, single-pass import/build, Unity **2022.3.62f1**,
  image `unityci/editor:ubuntu-2022.3.62f1-webgl-3`.
- Original recovery archive SHA256 matched the preserved source archive
  `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.

## Exactly what it produced

The captured output manifest records **18 files / 36,117,455 bytes** including
index, loader, compressed data, compressed framework and compressed WASM:

| File | Bytes | Recorded SHA256 |
| --- | ---: | --- |
| `Build/webgl.data.br` | 28,415,162 | `0b54f981f7cce745c305b0a6a5aac719f5aeb243f91e2f5a70d9bb9ff449667c` |
| `Build/webgl.wasm.br` | 7,584,162 | `63fd1821700146f2fb1e19d0d286bf09e97d7da1bfc328db017593fe9226ba80` |
| `Build/webgl.framework.js.br` | 75,059 | `fb14ddd631d7bb2b093d5b546a40423817b6cc14d05129161fc513651e30853e` |
| `Build/webgl.loader.js` | 20,680 | `95d413ec942c6764ee89174a13da7a4c7f80567f6a4fb43992a6e8273c3b93d2` |

The browser player binaries **were not preserved**: that workflow deliberately
uploaded evidence/logs only. This review downloaded the retained evidence, not
a playable native build. No evidence supports claiming the discarded binaries
are still hosted or that a Unity process/runner remains active.

Static HTTP check: `http_localhost_smoke_ok=true`, 18/18 output files served.
The output manifest correctly identifies `.wasm.br` and `.data.br`; the HTTP
summary's plain `.wasm`/`.data` suffix counts are zero because of compression,
not because compilation omitted those files.

## What this does not establish

- Only `Assets/Scenes/Marksman range.unity` was included in the player. The
  full side-scrolling Mission 01 game, Title/Disclaimer flow and other scenes
  were not exercised by this feasibility player.
- No retained Chromium/WebKit runtime execution, startup state, player input,
  game loop, original objective, AI or physical iPhone gameplay proof was found
  in this artifact. Static HTTP success does not establish those behaviors.
- Native import/build repairs and excluded legacy networking tasks remain
  documented on the independent Unity branch. A compile does not waive those
  compatibility boundaries.
- The AssetRipper dummy shaders and source-recovery limitations identified by
  the source audit apply to this route too. A native Unity build alone cannot
  restore shader behavior discarded during extraction.

## Next bounded experiment

If pursuing the native route, create a new isolated continuation branch without
resetting or editing the independent Unity branch. Reuse its exact successful
build inputs and preserve the resulting player in a **private** artifact,
then run actual browser startup/gameplay/error-capture checks. No automatic
engine migration, public hosting or claim of full-game parity follows from
these logs. Including all original scenes and validating their transitions is
a later milestone after a real running-player check.

This evidence warrants testing the native route rather than describing native
WebGL compilation as still pending or impossible. It does not finish animations,
character behavior, maps, missions or the complete browser port. The selected-
source SDK-8 port remains a separate verified checkpoint, not superseded by an
unexecuted native player.
