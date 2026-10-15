# Recovered Unity UNet task reference audit — 2026-10-15

## Findings

Read-only private GitHub Actions run [38773901868](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38773901868), artifact `unity-unet-task-reference-audit` ID `11993145655`, inspected the SHA-256-verified private AssetRipper recovery archive. This audit did **not** activate a Unity license, modify proprietary sources, or commit any game assets.

Recovered original tasks:

- `Assets/Scripts/Assembly-CSharp/BehaviorDesigner/Runtime/Tasks/Basic/UnityNetwork/IsServer.cs`, script GUID `adaab1508848919f83c42310c9c52abe`, checks `NetworkServer.active`, maps true to `TaskStatus.Success`.
- `Assets/Scripts/Assembly-CSharp/BehaviorDesigner/Runtime/Tasks/Basic/UnityNetwork/IsClient.cs`, script GUID `86e5e79a41eca15ee7c3223d03b2a5d5`, checks `NetworkClient.active`, maps true to `TaskStatus.Success`.

Audited scenes: `Disclaimer`, `Mission 01`, `Marksman range`, `Title`, `Transition`, and `Benchmark`. The scanner matched **zero** references to the recovered task class names or their script GUIDs in text-readable serialized scene, prefab, asset, controller, JSON, byte or text files, with a 15 MB per-file cap.

**Important limitation:** This is **not proof of zero usage**. The scanner did not decode binary Unity assets, asset bundles, or custom serialized Behavior Designer task blobs. The recovered source is a real networking dependency and has no supported 1:1 implementation in Unity 2022, which removed UNet.

## Diagnostic-only compile probe

A guarded opt-in option `UNITY_SPIKE_ALLOW_UNET_EXCLUSION=1` was added to the temporary patch script. If enabled, it wraps the **two original recovered task implementations verbatim** in `#if !UNITY_2022_3_OR_NEWER` / `#endif` inside the ephemeral CI workspace, preserving their content for the 2017 Unity path. It does **not** supply dummy `NetworkServer` or `NetworkClient` values or claim original networking works under Unity 2022. This is only a way to expose additional compile diagnostics after legacy UNet is excluded. If these tasks are actually used, Unity 2022 behavior will be absent; this cannot be shipped for full parity.

The active diagnostics remain in `.github/workflows/unity-scoped-four-errors.yml` and `scripts/unity_scoped_legacy_gui_patch.py`. Original ZIP stays private, with unchanged hash. Any WebGL build must be described as diagnostic-only until original behavior is recovered and tested.
