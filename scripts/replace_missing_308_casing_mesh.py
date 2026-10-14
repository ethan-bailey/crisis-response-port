#!/usr/bin/env python3
"""Apply the user-approved best-effort .308 casing mesh substitution.

This intentionally edits only the root MeshFilter in the .308 casing prefab.
It verifies the candidate mesh's .meta GUID and leaves the separate Projectile
child mesh untouched. The original missing GUID is retained in the report so
the substitution is documented as an approximation, not an identity recovery.
"""
from __future__ import annotations

import argparse
import json
import re
from pathlib import Path

MISSING_GUID = "b4b69dc6c74eeb54cbd22d28e37f9a19"
CANDIDATE_GUID = "cd74722033652fb409431b2947740490"
PREFAB_REL = Path("Assets/Resources/weapons/ammunition/casings/Casing .308.prefab")
MESH_REL = Path("Assets/Mesh/308 Winchester casing hi.asset")
META_REL = Path(str(MESH_REL) + ".meta")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("project", type=Path, help="Path to AssetRipper ExportedProject")
    parser.add_argument("--write", action="store_true", help="Apply the substitution (default is validation only)")
    args = parser.parse_args()
    root = args.project
    prefab = root / PREFAB_REL
    mesh = root / MESH_REL
    meta = root / META_REL
    for path in (prefab, mesh, meta):
        if not path.is_file():
            raise SystemExit(f"Required recovered asset is missing: {path}")

    meta_text = meta.read_text(encoding="utf-8")
    guid_match = re.search(r"(?m)^guid:\\s*([0-9a-f]{32})\\s*$", meta_text)
    if not guid_match or guid_match.group(1) != CANDIDATE_GUID:
        raise SystemExit("Candidate mesh GUID did not match the reviewed asset; refusing substitution.")

    text = prefab.read_text(encoding="utf-8")
    # Restrict replacement to the first MeshFilter block, which belongs to the
    # casing root. Do not alter the child GameObject named Projectile.
    blocks = list(re.finditer(r"(?ms)^--- !u!33 &.*?(?=^--- !u!|\\Z)", text))
    target = None
    for block in blocks:
        if MISSING_GUID in block.group(0):
            target = block
            break
    if target is None:
        if CANDIDATE_GUID in text:
            print("Already patched: candidate GUID is present and missing GUID is absent from root MeshFilters.")
            return
        raise SystemExit("Missing GUID was not found in a MeshFilter block; refusing a broad text replacement.")

    patched_block = target.group(0).replace(MISSING_GUID, CANDIDATE_GUID)
    patched = text[:target.start()] + patched_block + text[target.end():]
    if patched == text:
        raise SystemExit("No change produced; refusing to report success.")
    report = {
        "status": "user-approved-best-effort-substitution",
        "prefab": str(PREFAB_REL),
        "candidate_mesh": str(MESH_REL),
        "missing_guid": MISSING_GUID,
        "replacement_guid": CANDIDATE_GUID,
        "scope": "root casing MeshFilter only; Projectile child unchanged",
        "identity_verified": False,
    }
    if args.write:
        prefab.write_text(patched, encoding="utf-8")
        (root / "reports").mkdir(parents=True, exist_ok=True)
        (root / "reports" / "308-casing-mesh-substitution.json").write_text(
            json.dumps(report, indent=2) + "\\n", encoding="utf-8"
        )
        print(json.dumps(report, indent=2))
    else:
        print(json.dumps({**report, "status": "validated-dry-run", "would_change": True}, indent=2))


if __name__ == "__main__":
    main()
