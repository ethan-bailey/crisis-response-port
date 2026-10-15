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
