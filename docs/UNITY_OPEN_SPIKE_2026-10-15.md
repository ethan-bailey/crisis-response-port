# Unity 2022.3 LTS open/import spike — 2026-10-15

## Decision

**Blocked** — editor license validation failed; project importability remains unmeasured.

**Conditional NO-GO / hold** for switching the main track to Unity WebGL now. The recovered project has not been proven to import, compile and build. This is *not* a finding that Unity recovery is impossible; continue existing WASM work until a licensed Unity import produces project-level evidence.

This is a single-session import diagnosis. No proprietary asset or recovered Unity project is committed in Git history; only this report, scripts and workflow are.

## Provenance and preservation

- Recovery source: [2026-10-14 recovery record](RECOVERY_RUN_2026-10-14.md).
- Original GitHub Actions run: [38582072502](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38582072502); artifact \`crisis-response-recovery-direct-v3-1\`, ID \`11903811738\`.
- Original artifact expiry: **2026-10-28 14:38:58 UTC**; expected SHA-256 \`cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d\`.
- Verified source archive: \`true\`; privately preserved as release attachment: \`true\`.
- Target private release: [recovery-preservation-2026-10-15](https://github.com/ethan-bailey/crisis-response-port/releases/tag/recovery-preservation-2026-10-15). **Do not change repository visibility while third-party assets are attached**.
- Diagnostic run: [GitHub Actions run 38764820365](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38764820365).

## Environment and editor

- Runner: \`Linux_6.17.0-1022-azure_x86_64\`; initial free disk: \`37564948\` KiB.
- Unity docker image: \`unityci/editor:ubuntu-2022.3.62f1-base-3\`. Image pull exit: \`0\`.
- Editor executable available: \`true\`. License secret supplied: \`true\`. Import invoked: \`true\`.
- Unity import exit code: \`1\`. Import log present: \`true\` (1559 lines).
- WebGL module selection: \`false\`; build attempted: \`false\`.

## Failure classification (log-line counts, overlapping categories)

| Category | Observed log lines |
| --- | ---: |
| Missing/broken scripts or GUIDs | 0 |
| C# compiler errors | 12 |
| Obsolete/API errors (can overlap C#) | 12 |
| Asset import failures | 0 |
| Unity licensing diagnostics | 3 |
| Editor/container/environment failures | 0 |
| Potential IL2CPP-hostile patterns (references only) | 3 |

**Important:** Zero when no import log exists means **not measured**, not zero project errors. Compiler errors count log lines, not necessarily distinct root causes. Original AssetRipper archive upload warned of case-insensitive filename collisions; those occurred at artifact creation and are *not* Unity import failures without corresponding editor errors.

### Actual log excerpts

**C# compile** (12):

\`\`\`text
Assets/Plugins/Assembly-CSharp-firstpass/ForcedReset.cs(4,26): error CS0619: 'GUITexture' is obsolete: 'GUITexture has been removed. Use UI.Image instead.'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/FPSCounter.cs(5,27): error CS0619: 'GUIText' is obsolete: 'GUIText has been removed. Use UI.Text instead.'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/FPSCounter.cs(18,11): error CS0619: 'GUIText' is obsolete: 'GUIText has been removed. Use UI.Text instead.'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/SimpleActivatorMenu.cs(7,10): error CS0619: 'GUIText' is obsolete: 'GUIText has been removed. Use UI.Text instead.'
\`\`\`

**Scripts/GUID** (0):

No matching excerpt in captured import log.

**API obsolescence** (12):

\`\`\`text
Assets/Plugins/Assembly-CSharp-firstpass/ForcedReset.cs(4,26): error CS0619: 'GUITexture' is obsolete: 'GUITexture has been removed. Use UI.Image instead.'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/FPSCounter.cs(5,27): error CS0619: 'GUIText' is obsolete: 'GUIText has been removed. Use UI.Text instead.'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/FPSCounter.cs(18,11): error CS0619: 'GUIText' is obsolete: 'GUIText has been removed. Use UI.Text instead.'
Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/Utility/SimpleActivatorMenu.cs(7,10): error CS0619: 'GUIText' is obsolete: 'GUIText has been removed. Use UI.Text instead.'
\`\`\`

**Asset import** (0):

No matching excerpt in captured import log.

**Licensing** (3):

\`\`\`text
[Licensing::Module] Error: Access token is unavailable; failed to update
[Licensing::Client] Error: Code 500 while processing request (status: Unable to update licenses. Errors: No ULF license found.,Token not found in cache)
[Licensing::Client] Error: Code 500 while updating license in client (status: Unable to update licenses. Errors: No ULF license found.,Token not found in cache)
\`\`\`

**Environment** (0):

No matching excerpt in captured import log.

**IL2CPP risk** (3):

\`\`\`text
-r:"/opt/unity/Editor/Data/UnityReferenceAssemblies/unity-4.8-api/Facades/System.Reflection.Emit.ILGeneration.dll"
-r:"/opt/unity/Editor/Data/UnityReferenceAssemblies/unity-4.8-api/Facades/System.Reflection.Emit.Lightweight.dll"
-r:"/opt/unity/Editor/Data/UnityReferenceAssemblies/unity-4.8-api/Facades/System.Reflection.Emit.dll"
\`\`\`

## Marksman range WebGL test

Not attempted. Reason: \`import_or_compilation_failed\`.

The build driver, if reached, selects exactly one scene with filename containing \`Marksman\` from Unity's AssetDatabase; no other scenes are included and no original gameplay code was altered.

## Repair estimate

**Unknown**; a license blocker says nothing about the effort to repair the recovered project.

## Reproduction and logs

- Workflow: \`.github/workflows/unity-open-spike.yml\`.
- Diagnostics: \`scripts/classify_unity_spike.py\` and \`scripts/report_unity_spike.py\`.
- Action artifact: \`unity-2022-open-spike-evidence\` includes \`import.log\`, \`webgl.log\` if attempted, \`status.env\`, \`classification.json\`, scene inventory and system/disk notes.
- No Unity fixes, third-party assets or compiled game binaries were committed.

## Stop condition

One attempt in one working session. If a valid editor/license could not be obtained, this is **infrastructure blocked**, not a conclusive assessment of game recoverability.
