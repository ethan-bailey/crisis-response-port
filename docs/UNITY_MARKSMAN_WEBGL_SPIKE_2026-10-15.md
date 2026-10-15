# Unity Marksman-only WebGL feasibility run — October 15, 2026

## Goal

Attempt the recovered 2017 Unity project's **Marksman range scene only** as a modern Unity 2022.3.62f1 Personal WebGL build. This is a feasibility experiment, not a guarantee of asset fidelity or gameplay parity.

## Current known baseline

- [C# compilation success](UNITY_COMPILER_ITERATION_2026-10-15.md): headless Unity 2022.3.62f1 returned 0 with no C# compiler errors after seven precise compatibility patches and two conditional omissions of obsolete UNet Behavior Designer tasks.
- Import warned about approximately 250 AudioClip failures, two prefab component failures including Marksman mode, and missing generated lighting data.
- Unity scene state and WebGL build success remain separate questions; an editor compilation success does not establish a playable game.

## WebGL run

- [GitHub Actions run 38776778380](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38776778380).
- Workflow: `.github/workflows/unity-marksman-webgl.yml`.
- Reused `scripts/unity_open_spike.sh` with `UNITY_SPIKE_WEBGL=1`, `UNITY_SPIKE_SCOPED_REPAIR=1`, and `UNITY_SPIKE_ALLOW_UNET_EXCLUSION=1`.
- Editor image: `unityci/editor:ubuntu-2022.3.62f1-webgl-3`, provided runner can pull and extract it.
- Marksman scene selected exclusively via AssetDatabase GUID paths by temporary editor script `SpikeWebGL.BuildMarksman`; no source game scene or asset changes were committed.
- License: GitHub Secrets for Unity Personal, activation within same persistent Docker container; `--deactivate-all` and seat query on exit, with emergency `always()` cleanup.
- Source ZIP downloaded read-only from the previously verified private GitHub release. No recovered assets or produced WebGL binaries are uploaded as public artifacts. CI retains only source-free `*.log`, `*.json`, `*.env`, `*.txt` diagnostics in `unity-marksman-webgl-feasibility-logs` with 14-day retention.

## Interpretation

**Status at workflow dispatch:** build outcome not yet measured; never interpret a green GitHub job as success unless `status.env` confirms `build_attempted=true` and `build_exit=0` together with `webgl.log` reporting success. If `build_attempted=false`, identify the exact disk/image/licensing/import blocker. If Unity builds but scene assets are incomplete, treat it as a technical WebGL feasibility milestone, not validated 1:1 gameplay.

Follow-up review must verify the specific editor and IL2CPP errors, the missing audio/prefab integrity and any missing original systems before exposing a playable private preview.

## First actual WebGL player build result

[Run 38776778380](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38776778380), private log artifact ID `11994727494`, **did reach the Unity WebGL player build stage**, but `build_exit=1` and `MARKSMAN_WEBGL_RESULT=Failed`. GitHub workflow success here means diagnostics/cleanup succeeded, **not** that the game built.

Confirmed from `status.env`: `import_exit=0`, `build_attempted=true`, `build_exit=1`, Personal activation 0, deactivation 0, seat query 0. Return log confirms `[200] REVOKE_SEAT`.

The WebGL player compiler revealed **two unique CS1061 errors** absent from the editor import:

```text
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/ImageEffects/SunShafts.cs(93,60): error CS1061: 'Camera' does not contain a definition for 'hdr' and no accessible extension method 'hdr' accepting a first argument of type 'Camera' could be found
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/ImageEffects/BloomAndFlares.cs(105,85): error CS1061: 'Camera' does not contain a definition for 'hdr' and no accessible extension method 'hdr' accepting a first argument of type 'Camera' could be found
```

WebGL log ends with `Error building Player because scripts had compiler errors` and `Exception: MARKSMAN_WEBGL_RESULT=Failed`. These are not missing original gameplay systems or networking; the legacy `Camera.hdr` property is replaced by the documented Unity 2022 `Camera.allowHDR` field: https://docs.unity.cn/2022.2/Documentation/ScriptReference/Camera-allowHDR.html .

**Follow-up:** Guarded, line-specific `.hdr` → `.allowHDR` source transformations in the two Unity Standard Assets effect scripts were added to the *ephemeral compatibility patch*, bringing its recorded changes to nine types/expressions plus the two legacy UNet compile exclusions. [Rerun 38778367036](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38778367036) is the next WebGL attempt. Its result must be assessed from its own `webgl.log` and `status.env`; no success implied from dispatch alone.

The original 250 audio import failures, missing prefab components and lighting-data warnings remain known fidelity concerns independent of WebGL compilation.

## WebGL template-fixed build outcome and faster subsequent workflow

- Completed [run 38784669819](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38784669819), private log artifact `unity-marksman-webgl-feasibility-logs` ID **11998548162**. GitHub Actions job was green because the diagnostic script completed, **NOT because WebGL was built**.
- `status.env` proves recovered ZIP digest verified, Unity Personal activated, **editor import exit 0**, WebGL template guard passed (blank → `APPLICATION:Default` with index 3/6 valid), but **player `build_exit=124`**. Unity process was deliberately killed by the original **750-second WebGL build timeout**. It **was not stuck**: `webgl.log` ends during active IL2CPP compilation at **`[373/570] C_WebGL_wasm`** after numerous earlier WebAssembly C++ object files were produced. The absence of the old postprocessor exception in this partial log does **not** prove it has been fixed, since postprocessing was not reached.
- Personal license cleanup confirmed **`license_deactivate_exit=0`** and `license_seat_query_exit=0`. It was returned in the same persistent Docker container, preserving seat hygiene.
- **Optimization implemented for next run:** `UNITY_SPIKE_SINGLE_PASS=1` in `scripts/unity_open_spike.sh` generates the original temporary `scripts/SpikeWebGL.cs` Editor driver **before launching Unity**. Unity import, C# compilation, IL2CPP generation and Marksman scene build all execute in **one Unity invocation**, rather than the previous import-run + second editor launch. Single log `webgl.log` is also mirrored into `import.log` for the classifier, and actual `build_exit` remains authoritative.
- Increase both WebGL timeouts from **750/1200 seconds** to **2100 seconds** to avoid killing an actively compiling job at 373/570. The overall job timeout is 70 minutes, so source-free evidence upload and entitlement deactivation still have headroom.
- Manual-only workflow `.github/workflows/unity-marksman-audio-http.yml` now reuses **GitHub Actions private `Library` cache** across runs, keyed by editor version and compatibility script hashes. Cache may be unavailable, evicted or too large; no speed guarantee on the first use. New GitHub-hosted runner still must pull the Unity WebGL image. The path includes imported proprietary assets, so the repository and cache must remain **private**.
- WAV header repairs were independently verified by [run 38786168932](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38786168932), but **were not mixed into the template-only run**. The separate optimized manual workflow enables `UNITY_SPIKE_FIX_WAV_HEADERS=1` for its eventual next build.
- These changes were **not applied to the already-running GitHub job** and that job was not restarted to save time. No new optimized Unity WebGL job was automatically triggered; the workflow is intentionally manual to avoid repeated expensive builds.

**Next actual test:** one cache-enabled single-pass licensed build on the manually dispatched workflow, with build output inventory + localhost HTTP smoke. If WebGL succeeds, inspect runtime content and test privately on a mobile browser before labeling it playable.
