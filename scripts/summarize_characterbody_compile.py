"""Summarize actual compiler output without turning a failed build green."""
import argparse
import json
import os
from pathlib import Path
import re

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("log", type=Path)
parser.add_argument("--output", type=Path, required=True)
args = parser.parse_args()
diagnostics = set()
pattern = re.compile(r"(.+?\.cs)\((\d+),(\d+)\): error (CS\d+): (.+?)(?: \[.*)?$")
for line in args.log.read_text().splitlines():
    if match := pattern.match(line):
        file, row, col, code, message = match.groups()
        file = file.replace("\\", "/")
        if "/runtime/" in file:
            file = "runtime/" + file.split("/runtime/", 1)[1]
        diagnostics.add((file, int(row), int(col), code, message))
symbols = sorted({m.group(1) for d in diagnostics if (m := re.search(r"type or namespace name '([^']+)'", d[4]))})
summary = {
    "sourceSha": os.getenv("GITHUB_SHA"),
    "uniqueErrorCount": len(diagnostics),
    "missingSymbols": symbols,
    "compilerDiagnostics": [dict(zip(("path", "line", "column", "code", "message"), row)) for row in sorted(diagnostics)],
    "scope": "Reported diagnostics only. Missing declarations can hide method-body/transitive errors. Zero parsed errors is not evidence of build success; use the build exit status.",
}
args.output.write_text(json.dumps(summary, indent=2) + "\n")
print(f"Recorded {len(diagnostics)} distinct compiler errors; {len(symbols)} missing type/namespace names")
