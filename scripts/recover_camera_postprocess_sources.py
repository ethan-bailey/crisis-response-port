#!/usr/bin/env python3
"""Reproduce the recovered Mission camera source slice from its pinned archive."""
import argparse
import hashlib
import json
import zipfile
from pathlib import Path

PIN = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
PLUGIN_PREFIX = "export/ExportedProject/Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/ImageEffects/"
GAME_PREFIX = "export/ExportedProject/Assets/Scripts/Assembly-CSharp/"
PLUGIN_NAMES = ("ColorCorrectionCurves.cs", "Tonemapping.cs", "VignetteAndChromaticAberration.cs")
EXTERIOR = "ExteriorCamera.cs"


def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def main(archive_path: Path, root: Path) -> None:
    archive_bytes = archive_path.read_bytes()
    if sha(archive_bytes) != PIN:
        raise ValueError("Pinned Unity source archive SHA-256 mismatch")
    manifest_path = root / "runtime/Original/FullSource/dependency-source-provenance.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    if manifest.get("archiveSha256") != PIN:
        raise ValueError("FullSource provenance does not name the pinned Unity archive")
    full_source = root / "runtime/Original/FullSource"
    reference_root = root / "runtime/Reference/RecoveredCameraPostprocess"
    reference_root.mkdir(parents=True, exist_ok=True)
    recovered = manifest.setdefault("recoveredReferences", {})
    with zipfile.ZipFile(archive_path) as archive:
        for name in PLUGIN_NAMES:
            entry = PLUGIN_PREFIX + name
            raw = archive.read(entry)
            destination = reference_root / name
            destination.write_bytes(raw)
            recovered[name] = {
                "runtimePath": str(destination.relative_to(root)),
                "archiveEntry": entry,
                "sha256": sha(raw),
                "runtimeSha256": sha(raw),
                "adaptation": "exact recovered source preserved as noncompiled reference",
            }

        entry = GAME_PREFIX + EXTERIOR
        raw = archive.read(entry)
        destination = reference_root / EXTERIOR
        destination.write_bytes(raw)
        recovered[EXTERIOR] = {
            "archiveEntry": entry,
            "runtimePath": str(destination.relative_to(root)),
            "sha256": sha(raw),
            "runtimeSha256": sha(raw),
            "adaptation": "exact recovered source preserved as noncompiled reference",
        }
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("archive", type=Path)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    main(args.archive, args.root)
