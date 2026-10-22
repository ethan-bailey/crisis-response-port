#!/usr/bin/env python3
"""Verify recovered limb skin source provenance without running deferred gameplay code."""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
BASE = ROOT / "source-reference/Original/LimbSkin"
manifest = json.loads((BASE / "provenance.json").read_text())
assert manifest["archiveSha256"] == "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
for name, source in manifest["sources"].items():
    actual = hashlib.sha256((BASE / name).read_bytes()).hexdigest()
    assert actual == source["sha256"], f"source hash mismatch: {name}"
print("Exact source pixel trace methods and developer SHA files verified")
