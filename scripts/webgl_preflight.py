#!/usr/bin/env python3
"""Static WebGL preflight for the recovered Crisis Response Unity project.

This does not claim to compile or execute Unity code. It creates a repeatable
inventory report to accompany a future licensed Unity build attempt.
"""
from pathlib import Path
import re
import sys

project = Path(sys.argv[1] if len(sys.argv) > 1 else "work/recovery-artifact/export/ExportedProject")
report = Path(sys.argv[2] if len(sys.argv) > 2 else "work/WEBGL_PREFLIGHT.md")
assets = project / "Assets"
settings = project / "ProjectSettings"
version_file = settings / "ProjectVersion.txt"
build_scenes = settings / "EditorBuildSettings.asset"

if not assets.is_dir() or not version_file.is_file():
    raise SystemExit(f"Recovered Unity project not found or incomplete: {project}")

cs_files = list(assets.rglob("*.cs"))
scene_files = list((assets / "Scenes").glob("*.unity")) if (assets / "Scenes").is_dir() else []
version = version_file.read_text(errors="replace").strip()
scene_text = build_scenes.read_text(errors="replace") if build_scenes.exists() else ""

# Matches are candidates for manual review, not a compiler-error detector.
patterns = {
    "Managed thread creation": r"\bnew\s+Thread\s*\(",
    "Thread.Abort": r"\.Abort\s*\(",
    "System.Net.WebRequest": r"\bWebRequest\b",
    "Microsoft.Win32.Registry": r"\bRegistry\s*\.",
    "BinaryFormatter": r"\bBinaryFormatter\b",
    "Desktop filesystem APIs": r"\b(File|Directory)\.(Create|Delete|Exists|GetFiles|GetDirectories|EnumerateFiles|EnumerateDirectories|Open|ReadAllText|WriteAllText)\s*\(",
    "Keyboard input polling": r"\bInput\.GetKey(?:Down|Up)?\s*\(",
    "Mouse input polling": r"\bInput\.GetMouseButton(?:Down|Up)?\s*\(|\bInput\.mousePosition\b|\bInput\.GetAxis\s*\(",
    "Cursor lock/confine": r"CursorLockMode|Cursor\.lockState|Cursor\.visible",
    "Native interop attributes": r"\b(?:DllImport|LibraryImport)\b",
}
results = {}
for label, pattern in patterns.items():
    rx = re.compile(pattern)
    hits = []
    for file in cs_files:
        try:
            source_lines = file.read_text(errors="replace").splitlines()
        except OSError:
            continue
        for line_no, line in enumerate(source_lines, 1):
            if rx.search(line):
                rel = file.relative_to(project).as_posix()
                hits.append((rel, line_no, line.strip()[:220]))
    results[label] = hits

enabled_scenes = len(re.findall(r"\benabled:\s*1\b", scene_text))
lines = [
    "# Crisis Response WebGL preflight",
    "",
    f"- Project path: {project.as_posix()}",
    f"- Unity version: {version}",
    f"- C# files scanned: {len(cs_files)}",
    f"- Scene assets found under Assets/Scenes: {len(scene_files)}",
    f"- Enabled scene entries in EditorBuildSettings.asset: {enabled_scenes}",
    "- Result type: static source inventory only; NOT a Unity compile or runtime test.",
    "",
    "## Candidate compatibility findings",
    "",
    "Counts below are raw source-line matches and may include comments or duplicate call sites. They are not confirmed errors.",
    "",
]
for label, hits in results.items():
    lines.append(f"### {label} — {len(hits)} source-line matches")
    if hits:
        for rel, line_no, excerpt in hits[:15]:
            safe_excerpt = excerpt.replace(chr(96), chr(39))
            lines.append(f"- {rel}:{line_no}: {safe_excerpt}")
        if len(hits) > 15:
            lines.append(f"- … {len(hits) - 15} additional matching lines omitted")
    else:
        lines.append("- No matches found.")
    lines.append("")
lines += [
    "## Required next gate",
    "",
    "Run the project through a valid, licensed Unity Editor build environment. Preserve the full Unity compiler log and build output. Use the compiler log to determine actual errors; do not treat these static matches as proof of failure.",
    "",
    "For iPhone Safari, compilation is only the first gate. Verify HTTPS/network behavior, touch input, persistence, startup, audio, performance, lifecycle, and gameplay parity on a real device.",
    "",
]
report.parent.mkdir(parents=True, exist_ok=True)
report.write_text("\n".join(lines), encoding="utf-8")
print(f"Wrote {report}; scanned {len(cs_files)} C# files and {len(scene_files)} scenes.")
