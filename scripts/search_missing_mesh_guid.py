#!/usr/bin/env python3
"""Evidence-only search for a missing Unity asset GUID and nearby mesh candidates."""
from __future__ import annotations
import hashlib
import re
import sys
from pathlib import Path

GUID = "b4b69dc6c74eeb54cbd22d28e37f9a19"
NAME_RE = re.compile(r"(308|\.308|casing|shell|winchester|projectile|cartridge)", re.I)
GUID_RE = re.compile(r"(?m)^guid:\s*([0-9a-f]{32})\s*$")

def main() -> int:
    if len(sys.argv) != 3:
        print("usage: search_missing_mesh_guid.py <recovery-root> <report.md>", file=sys.stderr)
        return 2
    root, report = map(Path, sys.argv[1:])
    if not root.is_dir():
        raise SystemExit(f"Recovery root not found: {root}")
    exact_hits, owner_matches, candidates = [], [], []
    scanned = unreadable = 0
    for path in root.rglob("*"):
        if not path.is_file():
            continue
        try:
            data = path.read_bytes()
            scanned += 1
        except OSError:
            unreadable += 1
            continue
        rel = path.relative_to(root).as_posix()
        if GUID.encode("ascii") in data.lower():
            exact_hits.append(rel)
        if path.name.endswith(".meta"):
            match = GUID_RE.search(data.decode("utf-8", errors="ignore"))
            if match and match.group(1) == GUID:
                owner_matches.append(rel[:-5])
        if path.suffix.lower() in {".asset", ".prefab", ".obj", ".fbx", ".mesh"} and NAME_RE.search(path.name):
            candidates.append((rel, len(data), hashlib.sha256(data).hexdigest()))
    lines = [
        "# Missing .308 casing mesh GUID search", "",
        f"- Target GUID: {GUID}",
        f"- Recovery root: {root}",
        f"- Files scanned: {scanned}",
        f"- Files unreadable: {unreadable}",
        f"- Exact GUID byte hits: {len(exact_hits)}",
        f"- .meta files declaring target GUID: {len(owner_matches)}", "",
        "## Exact GUID hits", "",
    ]
    lines += [f"- {p}" for p in exact_hits] or ["- No exact GUID bytes found."]
    lines += ["", "## Meta owners", ""]
    lines += [f"- {p}" for p in owner_matches] or ["- No asset .meta declares the target GUID."]
    lines += ["", "## Candidate-named assets (inventory only; not identity proof)", "",
              "| Path | Bytes | SHA-256 |", "|---|---:|---|"]
    lines += [f"| {p} | {size} | {digest} |" for p, size, digest in sorted(candidates)] or ["| No candidates found | — | — |"]
    lines += ["", "## Interpretation", "",
        "This is an evidence search only. A filename match is not proof that a mesh is the missing asset. "
        "Do not rewrite serialized GUIDs or alias a candidate unless geometry, dimensions, material assignment, "
        "and prefab references have been compared against the original source. If the exact GUID is absent, "
        "the dependency remains unresolved.", ""]
    report.parent.mkdir(parents=True, exist_ok=True)
    report.write_text("\n".join(lines), encoding="utf-8")
    print("\n".join(lines[:12]))
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
