#!/usr/bin/env python3
"""Render a bounded Unity import spike report from recorded evidence, no guessing."""
import argparse
import json
import os
from pathlib import Path

def main():
    ap=argparse.ArgumentParser()
    ap.add_argument("--classification",required=True)
    ap.add_argument("--out",required=True)
    args=ap.parse_args()
    c=json.loads(Path(args.classification).read_text())
    status=c.get("status",{})
    counts=c["counts"]
    examples=c["examples"]
    launched=status.get("import_attempted") == "true"
    completed=launched and status.get("import_exit") == "0"
    compiler=counts["compile"]
    license_blocked=counts["license"]>0 or status.get("license_present") == "false" and status.get("import_exit") not in (None,"0")
    if not launched:
        verdict="**Not measured** — Unity was not invoked; import viability remains unknown."
        estimate="**Unknown**; cannot estimate repairs without an editor import log."
    elif license_blocked and status.get("import_exit") != "0":
        verdict="**Blocked** — editor license validation failed; project importability remains unmeasured."
        estimate="**Unknown**; a license blocker says nothing about the effort to repair the recovered project."
    elif completed and compiler == 0 and counts["asset_import"] == 0:
        verdict="**Yes for headless import** (subject to manual inspection of recovered project, API and scene)."
        estimate="**Hours to days** for editor migration validation only; not evidence that WebGL parity is easy."
    elif c.get("log_exists") and (counts["compile"] or counts["asset_import"] or counts["missing_guid_script"]):
        verdict="**Partial or failed** — see compilation/import log excerpts; full project compilation not established."
        estimate=("**Days (very low confidence)** for small isolated failures; could be considerably more after dependencies are addressed." if compiler < 25 else
                  "**Multiple days or more (very low confidence)**. The error count does not establish whether repairs amount to a rewrite.")
    else:
        verdict="**Not established** — editor did not provide enough valid import evidence."
        estimate="**Unknown** due to infrastructure/editor failure without project-specific diagnostics."
    webgl=status.get("build_attempted")=="true"
    if webgl:
        build=f"Attempted; Unity batch exit code \`{status.get('build_exit','unknown')}\`. Full \`webgl.log\` retained in workflow artifact."
    else:
        build=f"Not attempted. Reason: \`{status.get('build_skipped_reason','import_or_module_not_ready')}\`."
    go="**Conditional NO-GO / hold** for switching the main track to Unity WebGL now. The recovered project has not been proven to import, compile and build. This is *not* a finding that Unity recovery is impossible; continue existing WASM work until a licensed Unity import produces project-level evidence."
    if completed and compiler==0 and webgl and status.get("build_exit")=="0":
        go="**GO for a scoped technical WebGL evaluation**, not confirmed game parity. Evaluate scene/play and real iPhone performance next."
    elif completed and compiler==0:
        go="**Conditional GO for editor recovery**, but **NO-GO for committing to WebGL** until the Marksman scene build, license and original behavior are confirmed."
    heads=[
        "# Unity 2022.3 LTS open/import spike — 2026-10-15",
        "",
        "## Decision",
        "",
        verdict,
        "",
        go,
        "",
        "This is a single-session import diagnosis. No proprietary asset or recovered Unity project is committed in Git history; only this report, scripts and workflow are.",
        "",
        "## Provenance and preservation",
        "",
        "- Recovery source: [2026-10-14 recovery record](RECOVERY_RUN_2026-10-14.md).",
        "- Original GitHub Actions run: [38582072502](https://github.com/ethan-bailey/crisis-response-port/actions/runs/38582072502); artifact \`crisis-response-recovery-direct-v3-1\`, ID \`11903811738\`.",
        "- Original artifact expiry: **2026-10-28 14:38:58 UTC**; expected SHA-256 \`cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d\`.",
        f"- Verified source archive: \`{status.get('source_verified','unverified')}\`; privately preserved as release attachment: \`{status.get('preserved','unverified')}\`.",
        "- Target private release: [recovery-preservation-2026-10-15](https://github.com/ethan-bailey/crisis-response-port/releases/tag/recovery-preservation-2026-10-15). **Do not change repository visibility while third-party assets are attached**.",
        f"- Diagnostic run: [GitHub Actions run {os.environ.get('GITHUB_RUN_ID','unknown')}](https://github.com/ethan-bailey/crisis-response-port/actions/runs/{os.environ.get('GITHUB_RUN_ID','unknown')}).",
        "",
        "## Environment and editor",
        "",
        f"- Runner: \`{status.get('runner','unknown')}\`; initial free disk: \`{status.get('disk_available_kb','unknown')}\` KiB.",
        f"- Unity docker image: \`{status.get('docker_image','not selected')}\`. Image pull exit: \`{status.get('docker_exit','not attempted')}\`.",
        f"- Editor executable available: \`{status.get('editor_detected','unknown')}\`. License secret supplied: \`{status.get('license_present','unknown')}\`. Import invoked: \`{status.get('import_attempted','false')}\`.",
        f"- Unity import exit code: \`{status.get('import_exit','not run')}\`. Import log present: \`{str(c['log_exists']).lower()}\` ({c['log_lines']} lines).",
        f"- WebGL module selection: \`{status.get('webgl_requested','unknown')}\`; build attempted: \`{str(webgl).lower()}\`.",
        "",
        "## Failure classification (log-line counts, overlapping categories)",
        "",
        "| Category | Observed log lines |",
        "| --- | ---: |",
        f"| Missing/broken scripts or GUIDs | {counts['missing_guid_script']} |",
        f"| C# compiler errors | {counts['compile']} |",
        f"| Obsolete/API errors (can overlap C#) | {counts['api_obsolete']} |",
        f"| Asset import failures | {counts['asset_import']} |",
        f"| Unity licensing diagnostics | {counts['license']} |",
        f"| Editor/container/environment failures | {counts['editor_failure']} |",
        f"| Potential IL2CPP-hostile patterns (references only) | {counts['il2cpp_risk']} |",
        "",
        "**Important:** Zero when no import log exists means **not measured**, not zero project errors. Compiler errors count log lines, not necessarily distinct root causes. Original AssetRipper archive upload warned of case-insensitive filename collisions; those occurred at artifact creation and are *not* Unity import failures without corresponding editor errors.",
        "",
        "### Actual log excerpts",
        "",
    ]
    names=[
      ("compile","C# compile"),("missing_guid_script","Scripts/GUID"),("api_obsolete","API obsolescence"),
      ("asset_import","Asset import"),("license","Licensing"),("editor_failure","Environment"),("il2cpp_risk","IL2CPP risk")]
    for key,title in names:
        heads.extend([f"**{title}** ({counts[key]}):", ""])
        if examples[key]:
            heads+=["\`\`\`text"]+[line.replace("\`","'") for line in examples[key][:5]]+["\`\`\`",""]
        else:heads+=["No matching excerpt in captured import log.",""]
    heads.extend([
        "## Marksman range WebGL test",
        "",
        build,
        "",
        "The build driver, if reached, selects exactly one scene with filename containing \`Marksman\` from Unity's AssetDatabase; no other scenes are included and no original gameplay code was altered.",
        "",
        "## Repair estimate",
        "",
        estimate,
        "",
        "## Reproduction and logs",
        "",
        "- Workflow: \`.github/workflows/unity-open-spike.yml\`.",
        "- Diagnostics: \`scripts/classify_unity_spike.py\` and \`scripts/report_unity_spike.py\`.",
        "- Action artifact: \`unity-2022-open-spike-evidence\` includes \`import.log\`, \`webgl.log\` if attempted, \`status.env\`, \`classification.json\`, scene inventory and system/disk notes.",
        "- No Unity fixes, third-party assets or compiled game binaries were committed.",
        "",
        "## Stop condition",
        "",
        "One attempt in one working session. If a valid editor/license could not be obtained, this is **infrastructure blocked**, not a conclusive assessment of game recoverability.",
        "",
    ])
    Path(args.out).parent.mkdir(parents=True,exist_ok=True)
    Path(args.out).write_text("\n".join(heads))
    print(verdict)
if __name__=="__main__":main()
