# Unity 2022.3 LTS recovery open spike — 2026-10-15

## Verdict: PARTIALLY OPENED, DOES NOT COMPILE — CONDITIONAL NO-GO FOR WEBGL TODAY

**Recovered project reached Unity 2022.3.62f1's AssetDatabase, API Updater and C# compiler, so the original licensing barrier was removed.** The editor did **not** finish a clean import: the firstpass script assembly failed with **4 distinct removed-API errors**, printed **12 times** in the log. This is real evidence that the exported Unity project is readable enough to enter Unity's compilation pipeline, **not** evidence that all scripts/scenes run. No Marksman WebGL build was attempted, appropriately, because compilation failed.

**Decision:** Do not switch the primary port track to Unity WebGL on the basis of this spike. Continue source-backed WASM development while keeping the licensed Unity project viable for a *separate, bounded repair evaluation*. These failures do **not** establish that Unity would be an effective rewrite or that engine replacement is the only road.

## Runs and provenance

- Original recovery record: [RECOVERY_RUN_2026-10-14.md](RECOVERY_RUN_2026-10-14.md).
- Original recovered 2017.2.0f3 project: approximately 7,231 files (169 MiB), Unity `Assets/`, `ProjectSettings/`, `Packages/`, `Marksman range` among build-settings scenes.
- Source ZIP was **read from existing private release**, not re-uploaded: [recovery-preservation-2026-10-15](https://github.com/ethan-bailey/crisis-response-port/releases/tag/recovery-preservation-2026-10-15). Verified SHA-256: `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
- First Personal activation attempt: [run 38763803865](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38763803865). 2022.3.13f1 `Unity.Licensing.Client` rejected `--activate-all --include-personal` (`CommandLine.UnknownOptionError`); 2022.3.62f1 fallback image then exhausted Docker disk. Neither reached project import.
- Successful-activation **project diagnostic**: [run 38764820365](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38764820365), artifact **`unity-2022-personal-open-spike-evidence`** ID `11990121069`, expires **2026-11-14**. Contains full 1,559-line `import.log`, `classification.json`, sanitized license logs, environment/status and scene inventory. `import.log` has the actual compiler output.
- No recovered game binaries, original archive, compiled WebGL artifact or proprietary assets were written to Git history; repository must stay private while the source ZIP is attached to its private release.

## Headless editor and Personal activation

| Check | Observed result |
| --- | --- |
| Linux GitHub-hosted runner | Ubuntu 24.04, 2026-10-15 |
| Docker image | `unityci/editor:ubuntu-2022.3.62f1-base-3` |
| Disk after runner cleanup | `37,564,948 KiB` free before project/image (approx. 35.8 GiB) |
| Docker image pull | Exit `0`; verified downloaded |
| Unity Personal activation via credentials | **Succeeded**, activation exit `0` |
| Project opened in headless editor | **Partial** — API updater and C# compilation reached |
| Unity batch import exit | `1`; not a successful import |
| WebGL module | Not installed (base image); compile blocked build first |
| Marksman range WebGL build | **Not attempted** |

Licensing-client excerpt:

```text
Trying to activate all seats ...
Activation processed successfully.
Status: [200] SameMachine
Trying to update entitlement license file ...
Successfully updated UnityEntitlementLicense.xml!
```

Editor launch uses `/opt/unity/Editor/Unity -batchmode -nographics -quit -projectPath /spike/recovery/export/ExportedProject -logFile /spike/import.log`. The Editor printed some `No ULF license found` / token-cache warnings while also reporting successful Personal entitlement resolution. Those warnings **did not stop asset processing or compilation** and are not the cause of the import failure.

Compiler evidence:

```text
[API Updater] Update finished with success in 5484 ms (0/4 assembly(ies) updated).
[ScriptCompilation] Requested script compilation because: Assembly Definition File(s) changed
DisplayProgressbar: Compiling Scripts
Assets/Plugins/Assembly-CSharp-firstpass/ForcedReset.cs(4,26): error CS0619: 'GUITexture' is obsolete: 'GUITexture has been removed. Use UI.Image instead.'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/FPSCounter.cs(5,27): error CS0619: 'GUIText' is obsolete: 'GUIText has been removed. Use UI.Text instead.'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/FPSCounter.cs(18,11): error CS0619: 'GUIText' is obsolete: 'GUIText has been removed. Use UI.Text instead.'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/SimpleActivatorMenu.cs(7,10): error CS0619: 'GUIText' is obsolete: 'GUIText has been removed. Use UI.Text instead.'
Scripts have compiler errors.
```

## Failure categories and counts

| Category | Count / interpretation |
| --- | --- |
| C# compile | **4 unique CS0619 diagnostics**, repeated **12 times** in Unity's combined log; **3 source files** affected |
| Obsolete 2017 → 2022 APIs | **4 unique occurrences:** `GUITexture` (1) and `GUIText` (3); same four as C# errors, not additional errors |
| Missing/broken scripts or GUID references | **0 matching log lines so far**; **not an exhaustive scene/GUID audit**, because compilation failed early |
| Asset import | **0 matched failures in captured log**, but overall project import **did not finish** |
| Other C# diagnostics | **0 observed beyond CS0619 in this firstpass compilation phase**; later assemblies may reveal more |
| IL2CPP-hostile dynamic code | **Not established.** Three `System.Reflection.Emit` lines refer to Unity reference-assembly compile flags, not evidence of game `Reflection.Emit` use |
| Editor/licensing | Personal activation **succeeded**. Editor emits **3 licensing warning/error lines** about ULF/token cache; not fatal to compilation |

**Log counts are lines, not a count of independent root causes.** The project is partially importable and blocked at the legacy firstpass assembly. Do not claim there are only four problems in the entire game.

## Marksman range WebGL build

**Not attempted.** Script compilation failed and the spike stopped at that gate, as requested. The base Unity image did not include the WebGL module. The next possible build evaluation would require a licensed WebGL image, selection of only the Marksman scene and another *explicitly authorized* bounded attempt; this spike did not repair code or build.

## Rough repair estimate (low confidence)

- **Initial observed blockers:** The four GUIText/GUITexture references are small, localized Unity API migrations; an experienced developer may address the known firstpass errors in **hours**.
- **Whole project compilation and correct 2017-to-2022 scene/game behavior:** **Days or longer, unknown**. Subsequent assembly, asset and runtime errors are hidden behind the firstpass failure. There is **no evidence supporting “effectively a rewrite”** yet, nor evidence that a complete WebGL port is only hours away.
- **Port-path recommendation:** **NO-GO for promoting Unity WebGL as the primary track today**; **GO for considering another scoped compatibility assessment** once a repair phase is explicitly approved. Keep ongoing WASM work progressing.

## Seat-return cleanup exception — requires verification

**Important:** Although activation succeeded, the originally scripted seat return did *not* succeed:

```text
An error occured while trying to return the ULF license. Ulf license file not found (.../Unity_lic.ulf) (1404)
[Licensing::Module] Error: Failed to return entitlement license
[Licensing::Module] Error: Serial number unavailable for ULF return; skipping operation
```

The editor used a **Unity entitlement XML license**, so the `--return-ulf` path was inappropriate. In response a separate [Personal entitlement deactivation run](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38766883502) was started with the correct `--deactivate-all --username / --password` command (and `--show-seats` verification). **Do not assume the Personal seat has been returned until that job's evidence is checked.** If still active, use Unity ID seat management to release it. This should be corrected in any future license-bearing workflow before execution.

## Stop condition / reproducibility

One project diagnosis, no code or asset repair. Workflow: `.github/workflows/unity-open-spike.yml`; script `scripts/unity_open_spike.sh`; classifier `scripts/classify_unity_spike.py`. Original private archive remains outside Git history. Full logs in [run 38764820365](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38764820365).
