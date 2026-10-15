# Unity 2022.3.62f1 — iterative compiler compatibility, 2026-10-15

## Outcome: seven safe source-site changes passed, legacy UNet integration now blocks compilation

Following the user's request to keep removing compiler errors, the repair-only GitHub Actions harness was updated to patch **seven exact source locations** in a temporary recovered-project copy: the original four `GUIText`/`GUITexture` errors; the additional `FPSCounter.cs:23` `GUIText` reference; and `DepthOfField.cs:245,299` calls updated per Unity's explicit compiler message from `Graphics.DrawProceduralIndirect` to `Graphics.DrawProceduralIndirectNow`.

**Result:** None of those seven original locations now emits compiler errors. Unity 2022.3.62f1 gets beyond the prior `Assembly-CSharp-firstpass` compilation blockers. Compilation then stops on **two unique CS0103 errors** in recovered third-party Behavior Designer UnityNetwork tasks:

```text
Assets/Scripts/Assembly-CSharp/BehaviorDesigner/Runtime/Tasks/Basic/UnityNetwork/IsServer.cs(9,13): error CS0103: The name 'NetworkServer' does not exist in the current context
Assets/Scripts/Assembly-CSharp/BehaviorDesigner/Runtime/Tasks/Basic/UnityNetwork/IsClient.cs(9,13): error CS0103: The name 'NetworkClient' does not exist in the current context
```

In the full Unity import log these are repeated by the Script Updater (eight `error CS` lines in total, only two unique file/line/code diagnostics). The recovered project **still does not compile**; Unity exits **1**. No Marksman WebGL build has been attempted.

## Evidence

- Five-change intermediate run: [38772492329](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38772492329), private artifact `unity-scoped-four-error-evidence` ID `11991239849`: the two DepthOfField graphics errors remain after only five changes. Personal activation and entitlement return both exited 0; server said `REVOKE_SEAT`.
- **Seven-change run:** [38772521352](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38772521352), private artifact `unity-scoped-four-error-evidence` ID `11991628858`: exact seven-change patch manifest, full import log, **two unique UnityNetwork CS0103 errors**, no original seven errors. Personal activation exit 0, return exit 0, seat query exit 0.
- Subsequent serialized-workflow correction: [38772560681](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38772560681); source state and seven-change patch unchanged. The workflow was updated to use a `concurrency` group for future runs to avoid consuming overlapping Unity Personal seats.
- The recovery ZIP remains unchanged in the **private** GitHub release `recovery-preservation-2026-10-15` and is downloaded read-only with SHA-256 verification. No proprietary assets, game binaries or recovered project files were checked into Git history.

## Root-cause classification

| Category | Unique errors | Scope |
| --- | ---: | --- |
| Earlier Unity GUI obsolete APIs | 0 remaining | Seven source-site patches remove errors at all previously identified locations |
| Earlier DepthOfField obsolete graphics API | 0 remaining | Two `DrawProceduralIndirectNow` updates compiled past the previous error |
| Removed Unity UNet / HLAPI integration | **2 CS0103** | `NetworkServer` / `NetworkClient` references in Behavior Designer UnityNetwork tasks |
| Missing-script/GUID import errors | 0 matched so far | Not a complete scene-level assessment |
| WebGL / IL2CPP | Not tested | Project still fails C# compilation |

**External verification:** Unity officially announced that UNet low-level API would not be included in **Unity 2022 LTS**: https://discussions.unity.com/t/unet-including-low-level-api-now-removed-from-unity-2022-2-find-links-to-alternatives-here/877316 . Migration to Netcode for GameObjects entails explicit API and behavior mapping, not renaming `NetworkServer` or `NetworkClient`: https://docs-multiplayer.unity3d.com/netcode/1.5.2/installation/upgrade_from_UNet/ .

## Stop boundary and recommendation

Do **not** replace `NetworkServer`/`NetworkClient` with fake `false` values, empty mocks, or guessed networking states just to obtain a green compiler result. Those would undermine the user requirement to preserve developer systems and behavioral parity. These symbols are in `Assembly-CSharp/BehaviorDesigner/Runtime/Tasks/Basic/UnityNetwork`, a package integration area, but whether the recovered Marksman scene or other original game logic actually calls them has **not been established**.

**Decision: Unity WebGL is still NO-GO for primary track on present evidence; preserve existing WASM progress.** The two errors are attributable to genuine removed networking infrastructure, not simply syntax or renamed GUI widgets. A separate evidence-backed examination of script references, networking dependency use, and an approved 2022-compatible implementation or strictly proven scene-local exclusion is required to proceed faithfully.

Because the user requested iterative repairs, the repair script and reproducible CI harness have been left in the branch for further source-backed work. No claims of clean compilation, completed game or iPhone playable WebGL build are warranted.

## Update — successful C# compile after diagnostic UNet exclusion (2026-10-15)

**Latest run [38774237714](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38774237714)**, artifact **`unity-scoped-four-error-evidence` ID `11994570128`**, contains the full **2.8 MB Unity 2022.3.62f1 import log**, `status.env`, `classification.json`, seven-patch + two-guard `scoped-patch.json`, sanitized licensing logs and scene inventory.

- **Unity batch import returned exit `0`**, and **zero `error CS####` diagnostics** appeared. All seven previously observed GUI/DepthOfField errors and both UNet symbol errors are absent.
- This is an actual successful **C# compilation** and batch-mode completion of the temporarily adapted recovered project, not just a successful GitHub workflow shell exit.
- Personal license **activation exit 0**, **deactivation exit 0**, seat check exit 0. Sanitized license return log states **`[200] REVOKE_SEAT`** and deletion of `UnityEntitlementLicense.xml`. Original recovery ZIP was read only and SHA-256-verified from a private GitHub release; no proprietary binaries added to Git history.
- Temporary compilation changes: seven source-line API substitutions across four recovered utility/effects files, plus **two disabled obsolete UNet Behavior Designer task definitions** using `#if !UNITY_2022_3_OR_NEWER`. The tasks remain verbatim in the temporary source, and are omitted for Unity 2022. **This excludes their behavior; it is not full original game parity.**
- Read-only scene audit [38773901868](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38773901868) found no plain-text GUID references to those two tasks in six scenes, including Marksman range, but **could not rule out binary/custom task serialization**.

### Important non-C# errors and warnings found in full editor log

A clean C# compile **does not** mean a clean asset import:

| Remaining category | Observed in import log | Example / impact |
| --- | ---: | --- |
| AudioClip import failures | **250** | `Errors during import of AudioClip Assets/AudioClip/bolt-extract-01.wav` followed by `FSBTool ERROR: The format of the source file is invalid`. Sounds may be absent at runtime |
| Corrupt/missing prefab components | **2** | `Interior mode.prefab` missing component index 2; `Marksman mode.prefab` missing component index 3 |
| Lighting data missing | **6 log lines** | Three lighting data sets reported missing 150, 30 and 200 files, with repeated warnings; game visual lighting may differ |
| C# compiler errors | **0** | No `error CS####` in the full log |

These asset import errors did **not** make the Unity batch process return nonzero. They require separate source-asset integrity investigation before any complete game-parity claim. **Do not simply fabricate substitute audio, missing prefab scripts, or lighting.**

### Next decision

**GO for a strictly limited Marksman-range Unity WebGL build feasibility test**, conditional on Unity Personal license cleanup and sufficient WebGL module/disk. This is **not** a GO for adopting Unity WebGL as the main port without a working scene and original-system parity audit. A WebGL build may reveal IL2CPP, asset or runtime blockers not observable during editor compilation.

This update supersedes the earlier section above that says the C# compiler still fails at removed UNet symbols. Those symbols were removed **only from the Unity 2022 diagnostic compilation**, not reimplemented.
