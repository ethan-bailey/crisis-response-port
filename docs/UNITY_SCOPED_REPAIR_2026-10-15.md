# Unity 2022.3.62f1 — scoped four-error repair measurement (2026-10-15)

## Decision: original four diagnostic sites cleared; new errors emerged — NO-GO for WebGL build now

**Project partially opens, but still does not compile.** Exactly four targeted substitutions were applied in **three utility scripts** in a temporary copy of the recovered Unity project. All four original diagnosed `CS0619` locations were eliminated from the subsequent compiler log. Compilation exposed additional `GUIText` use and two deprecated graphics API calls. Per the stop condition, **no fifth repair or broader migration was performed**. No WebGL build was attempted.

**Recommendation:** Keep the source-backed WASM port as the primary track. This is a *limited additional blocker set*, not proof of a large error cascade or an effectively unrecoverable project. A Marksman WebGL build is not the next step until the project compiles. Any further Unity repair wave must be separately authorized.

## Reproducible run, archive provenance, and seat safety

- [Scoped run 38770459626](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38770459626), artifact `unity-scoped-four-error-evidence`, ID `11991411664`, retained until approximately **2026-11-14**. Includes `import.log` (full editor log), `classification.json`, `scoped-patch.json`, `status.env`, and sanitized activation/deactivation logs.
- Docker image: `unityci/editor:ubuntu-2022.3.62f1-base-3`; import exit **1** after compilation.
- Original recovery artifact was downloaded **read-only from the private release** `recovery-preservation-2026-10-15` and verified SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`. No reupload and no game binaries committed.
- Personal seat: activation exit **0**, status `[200] ASSIGN_SEAT`; entitlement deactivation exit **0**, status `[200] REVOKE_SEAT`; post-return seat query exit **0** with no seats listed. Verified.
- Preceding [run 38769889373](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38769889373) stopped safely before activation or source changes because a guard discovered **three** `GUIText` references in `FPSCounter.cs` rather than two. The measured run constrained replacements to the four specific original diagnostic lines.
- No original scenes, gameplay files, or proprietary assets were changed. The repaired source exists **only in the ephemeral Actions workspace**, not in Git history.

## Exactly four fixes applied

| Recovered file | Source line | Targeted replacement |
| --- | ---: | --- |
| `Assets/Plugins/Assembly-CSharp-firstpass/ForcedReset.cs` | 4 | `GUITexture` → `UnityEngine.UI.RawImage` |
| `Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/FPSCounter.cs` | 5 | `GUIText` → `UnityEngine.UI.Text` |
| `Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/FPSCounter.cs` | 18 | `GUIText` → `UnityEngine.UI.Text` |
| `Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/SimpleActivatorMenu.cs` | 7 | `GUIText` → `UnityEngine.UI.Text` |

The patch script `scripts/unity_scoped_legacy_gui_patch.py` verified the exact filenames, line positions and tokens, and wrote a source-free JSON manifest before invoking Unity. **No claim of runtime behavioral equivalence** is made: UI.RawImage/UI.Text have different component/scene wiring requirements from legacy GUITexture/GUIText. The replacements were solely a compile-gate experiment.

## Actual new error categories

| Category | Unique new diagnostics | Notes |
| --- | ---: | --- |
| Remaining obsolete GUI API | **1 CS0619** | `FPSCounter.cs:23`, previously unreached third `GUIText` reference |
| GUI type incompatibility | **1 CS0029** | Same `FPSCounter.cs:23`; returned old `GUIText` cannot assign to `UnityEngine.UI.Text` |
| Deprecated graphics API | **2 CS0619** | `DepthOfField.cs:245` and `:299`; `Graphics.DrawProceduralIndirect` removed/deprecated |
| Missing/broken GUID/script references | **0 observed** | Not an exhaustive asset/scene audit; script compilation still failing |
| Asset import errors | **0 observed** | Does not establish successful complete import |
| Additional gameplay assembly diagnostics | **Unknown** | Assembly-CSharp-firstpass failed; later code may still be blocked |
| IL2CPP/WebGL errors | **Not tested** | WebGL module not used, no build |

Unity's combined log contains **14 lines matching `error CS####`**, but only **four new distinct code/file/source positions** before considering Script Updater duplicate messages. Code breakdown across repeated lines: `CS0619` **10** occurrences; `CS0029` **4** occurrences. The generic classifier matched **115** lines containing `obsolete` because it also matches non-error API updater text; that number is **not** an obsolete compiler error count.

### Verbatim representative compiler excerpts

```text
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/FPSCounter.cs(23,29): error CS0619: 'GUIText' is obsolete: 'GUIText has been removed. Use UI.Text instead.'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/FPSCounter.cs(23,16): error CS0029: Cannot implicitly convert type 'UnityEngine.GUIText' to 'UnityEngine.UI.Text'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/ImageEffects/DepthOfField.cs(245,6): error CS0619: 'Graphics.DrawProceduralIndirect(MeshTopology, ComputeBuffer, int)' is obsolete: 'Method DrawProceduralIndirect has been deprecated. Use Graphics.DrawProceduralIndirectNow instead. (UnityUpgradable) -> DrawProceduralIndirectNow(*)'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/ImageEffects/DepthOfField.cs(299,6): error CS0619: 'Graphics.DrawProceduralIndirect(MeshTopology, ComputeBuffer, int)' is obsolete: 'Method DrawProceduralIndirect has been deprecated. Use Graphics.DrawProceduralIndirectNow instead. (UnityUpgradable) -> DrawProceduralIndirectNow(*)'
```

Both `ForcedReset.cs` and `SimpleActivatorMenu.cs` cleared the original reported compiler errors. `FPSCounter.cs` cleared the two **modified locations** but **does not compile as a whole** because line 23 exposes an additional dependent legacy reference.

## Scope, estimate and next gate

- **Compile result:** no (Editor returned 1).
- **Rough next repair scale:** newly visible issues are localized to two utility/effect scripts; potentially **hours** for these particular diagnostics, but **overall migration time remains unknown** because other scripts may reveal further errors. No repair beyond the requested four was undertaken.
- **Marksman-only WebGL:** not attempted. Require a clean licensed Unity import/compile in a new explicitly authorized run before considering WebGL.
- **GO/NO-GO:** **NO-GO for committing to Unity WebGL today; retain the WASM track**. This is not evidence that the Unity project is unrecoverable.

## Implementation artifacts

- `scripts/unity_scoped_legacy_gui_patch.py`: line-guarded, exactly four replacements.
- `scripts/unity_open_spike.sh`: opt-in `UNITY_SPIKE_SCOPED_REPAIR=1` patch after ZIP extraction, import-only stopping rule, same-container activation/entitlement deactivation.
- `.github/workflows/unity-scoped-four-errors.yml`: licensed transient CI run with disk cleanup, scoped patch, emergency seat cleanup and private source-free log artifact.
