#!/usr/bin/env python3
"""Private, read-only audit of the game's *actual* Unity window shaders.

Outputs verified paths and SHA256s. Prints a bounded shader source excerpt
for inspection during the private GitHub Actions run; never fabricates GLSL.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument("project", type=Path)
parser.add_argument("--out", type=Path, required=True)
args = parser.parse_args()
root = args.project

targets = {
    "regular_glass": "c5c4e0da9b5e8fa4cb7c2f0db89a9c36",
    "mirror_glass": "fb32b7a5134a45a41997e05b8b5dc9e2",
}
guids = {}
for meta in (root / "Assets").rglob("*.meta"):
    match = re.search(r"(?m)^guid:\s*([0-9a-f]{32})", meta.read_text(errors="replace"))
    if match and match.group(1) in targets.values():
        guids[match.group(1)] = meta.with_suffix("")
results = {}
for key, guid in targets.items():
    path = guids.get(guid)
    if not path or not path.is_file():
        raise SystemExit("STOP: source shader GUID absent: " + key + " " + guid)
    raw = path.read_bytes()
    text = raw.decode("utf-8", errors="replace")
    lines = text.splitlines()
    match = re.search(r'\bShader\s+"([^"]+)"', text)
    # All verbatim source is retained only in an authorized private artifact,
    # never committed to the repository by this audit job.
    source = {
        "guid": guid,
        "path": path.relative_to(root).as_posix(),
        "sha256": hashlib.sha256(raw).hexdigest(),
        "bytes": len(raw),
        "lines": len(lines),
        "extension": path.suffix.lower(),
        "unityShaderName": match.group(1) if match else None,
        "signals": {
            term: [i + 1 for i, line in enumerate(lines)
                   if re.search(r"\b" + re.escape(term) + r"\b", line, re.I)][:16]
            for term in ("SubShader", "Pass", "Blend", "ZWrite", "ZTest", "Cull",
                         "Transparent", "Queue", "frag", "surf", "tex2D",
                         "MainTex", "Opacity", "Alpha", "GrabPass", "Reflection")
        },
    }
    results[key] = source
    print("SOURCE", key, json.dumps(source, sort_keys=True))
    print("VERBATIM_SOURCE_EXCERPT", key, "first 180 lines or 16000 chars")
    for index, line in enumerate(lines[:180], 1):
        if sum(len(x) + 1 for x in lines[:index]) > 16000:
            break
        print(f"{key}:{index:04d}: {line[:500]}")
out = {"audit": "Original Unity shader source audit only: not a parity implementation",
       "targets": results}
args.out.parent.mkdir(parents=True, exist_ok=True)
args.out.write_text(json.dumps(out, indent=2) + "\n")
