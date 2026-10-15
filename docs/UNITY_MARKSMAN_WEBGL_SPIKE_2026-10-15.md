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
