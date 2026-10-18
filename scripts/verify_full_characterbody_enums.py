#!/usr/bin/env python3
"""Verify compiled full-CharacterBody enums retain exact recovered developer definitions."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
for name in ("WeaponMode", "LimbId"):
    original = (ROOT / "source-reference/Original/CharacterDependencies" / (name + ".cs")).read_text()
    compiled = (ROOT / "runtime/Original/FullSource" / (name + ".cs")).read_text()
    source_tail = compiled[compiled.index("public enum " + name):]
    if source_tail != original:
        raise SystemExit(f"Original {name} definition has changed; source parity failed")
    if not compiled.startswith("namespace Crisis.Runtime.FullSource;"):
        raise SystemExit(f"{name} must remain isolated from weapon-host enum types")
    print(f"PASS recovered original {name} exact definition")
