# Unity 2022.3 LTS open/import spike — 2026-10-15

## Decision

**Blocked** — editor license validation failed; project importability remains unmeasured.

**Conditional NO-GO / hold** for switching the main track to Unity WebGL now. The recovered project has not been proven to import, compile and build. This is *not* a finding that Unity recovery is impossible; continue existing WASM work until a licensed Unity import produces project-level evidence.

This is a single-session import diagnosis. No proprietary asset or recovered Unity project is committed in Git history; only this report, scripts and workflow are.

## Provenance and preservation

- Recovery source: [2026-10-14 recovery record](RECOVERY_RUN_2026-10-14.md).
- Original GitHub Actions run: [38582072502](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38582072502); artifact `crisis-response-recovery-direct-v3-1`, ID `11903811738`.
- Original artifact expiry: **2026-10-28 14:38:58 UTC**; expected SHA-256 `cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d`.
- Verified source archive: `true`; privately preserved as release attachment: `true`.
- Target private release: [recovery-preservation-2026-10-15](https://github.com/ethan-bailey/crisis-response-port/releases/tag/recovery-preservation-2026-10-15). **Do not change repository visibility while third-party assets are attached**.
- Diagnostic run: [GitHub Actions run 38760336963](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38760336963).

## Environment and editor

- Runner: `Linux_6.17.0-1022-azure_x86_64`; initial free disk: `12983388` KiB.
- Unity docker image: `unityci/editor:ubuntu-2022.3.13f1-base-3`. Image pull exit: `0`.
- Editor executable available: `true`. License secret supplied: `false`. Import invoked: `true`.
- Unity import exit code: `1`. Import log present: `true` (58 lines).
- WebGL module selection: `false`; build attempted: `false`.

## Failure classification (log-line counts, overlapping categories)

| Category | Observed log lines |
| --- | ---: |
| Missing/broken scripts or GUIDs | 0 |
| C# compiler errors | 0 |
| Obsolete/API errors (can overlap C#) | 0 |
| Asset import failures | 0 |
| Unity licensing diagnostics | 6 |
| Editor/container/environment failures | 0 |
| Potential IL2CPP-hostile patterns (references only) | 0 |

**Important:** Although an import log exists, Unity's license check failed **before script compilation or asset import**. Therefore the counts of **0 for scripts, C# errors, obsolete APIs, assets and IL2CPP are NOT test results for the recovered project**. They mean those phases were never reached. Compiler counts are heuristic log-line counts, not distinct root causes. Original AssetRipper archive upload warned of case-insensitive filename collisions; those occurred at artifact creation and are *not* Unity import failures without corresponding editor errors.

### Actual log excerpts

**C# compile** (0):

No matching excerpt in captured import log.

**Scripts/GUID** (0):

No matching excerpt in captured import log.

**API obsolescence** (0):

No matching excerpt in captured import log.

**Asset import** (0):

No matching excerpt in captured import log.

**Licensing** (6):

```text
[Licensing::IpcConnector] Connection attempt to the License Client on channel: "LicenseClient-root" failed because channel doesn't exist; code: "0x80000002"
[Licensing::Module] Error: Access token is unavailable; failed to update
[Licensing::Client] Error: Code 500 while processing request (status: Unable to update licenses. Errors: No ULF license found.,Token not found in cache)
[Licensing::Client] Error: Code 500 while updating license in client (status: Unable to update licenses. Errors: No ULF license found.,Token not found in cache)
[Licensing::Module] Error: License is not active (com.unity.editor.headless). HasEntitlements will fail.
```

**Environment** (0):

No matching excerpt in captured import log.

**IL2CPP risk** (0):

No matching excerpt in captured import log.

## Marksman range WebGL test

Not attempted. Reason: `import_or_compilation_failed`.

The build driver, if reached, selects exactly one scene with filename containing `Marksman` from Unity's AssetDatabase; no other scenes are included and no original gameplay code was altered.

## Repair estimate

**Unknown**; a license blocker says nothing about the effort to repair the recovered project.

## Reproduction and logs

- Workflow: `.github/workflows/unity-open-spike.yml`.
- Diagnostics: `scripts/classify_unity_spike.py` and `scripts/report_unity_spike.py`.
- Action artifact: `unity-2022-open-spike-evidence` includes `import.log`, `webgl.log` if attempted, `status.env`, `classification.json`, scene inventory and system/disk notes.
- No Unity fixes, third-party assets or compiled game binaries were committed.

## Stop condition

One attempt in one working session. If a valid editor/license could not be obtained, this is **infrastructure blocked**, not a conclusive assessment of game recoverability.

## Direct answer to the spike question

- **Can Unity 2022.3 open and compile this recovered project?** **Unknown / not yet tested at the project level.** The editor was present and the command started, but licensing prevented the recovered project from reaching an import/compile stage.
- **Observed Unity version:** `2022.3.13f1`, GameCI Docker image `unityci/editor:ubuntu-2022.3.13f1-base-3`. This is a 2022.3 LTS patch release.
- **Batch-mode command:** `/opt/unity/Editor/Unity -batchmode -nographics -quit -projectPath /spike/recovery/export/ExportedProject -logFile /spike/import.log`. Exit **1**.
- **Licensing evidence:** `No ULF license found.,Token not found in cache`; `License is not active (com.unity.editor.headless). HasEntitlements will fail.` These appear in the 58-line original editor log.
- **C# compiler/GUI/asset pipeline:** Not reached; no valid code, GUID or import-failure counts can be assigned. The recovery archive *was* extracted and the private backup SHA-256 matched.
- **WebGL test:** Not attempted because the import was blocked and the runner selected a base-only image with approximately **12.4 GiB free disk before the job**.
- **Repair estimate:** **Unknown**. The issue found concerns editor licensing, not repaired game code. There is no basis to label the project an hours/days fix or an effective rewrite.
- **Recommendation:** **HOLD/NO-GO on promoting Unity WebGL to the primary track today**, not because the Unity project has failed, but because its compatibility is unproven. Continue the existing WASM compatibility track. A renewed go/no-go requires a legitimate headless Unity 2022.3 license, verified disk/module availability and a new import log. Do not infer that WASM is the only technically viable path from this result.

The full diagnostic log and classifier output are retained in [Actions run 38760336963](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38760336963), artifact `unity-2022-open-spike-evidence` (ID `11985407322`, expires 2026-11-14). A **private release backup**, rather than this diagnostic artifact, preserves the original recovered project past October 28.
