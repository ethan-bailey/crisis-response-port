#!/usr/bin/env python3
"""Verify recovered camera scripts against the pinned Unity source archive."""
import argparse
import hashlib
import json
import zipfile
from pathlib import Path

PIN = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
PLUGIN_PREFIX = "export/ExportedProject/Assets/Plugins/Assembly-CSharp-firstpass/UnityStandardAssets/ImageEffects/"
GAME_PREFIX = "export/ExportedProject/Assets/Scripts/Assembly-CSharp/"
SOURCE_ENTRIES = {
    "ColorCorrectionCurves.cs": PLUGIN_PREFIX + "ColorCorrectionCurves.cs",
    "Tonemapping.cs": PLUGIN_PREFIX + "Tonemapping.cs",
    "VignetteAndChromaticAberration.cs": PLUGIN_PREFIX + "VignetteAndChromaticAberration.cs",
    "ExteriorCamera.cs": GAME_PREFIX + "ExteriorCamera.cs",
}


def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def verify(archive_path: Path, root: Path) -> None:
    if archive_path.is_dir():
        recovery_root = archive_path
        provenance = json.loads((recovery_root / "recovery-provenance.json").read_text(encoding="utf-8"))
        if provenance.get("artifactSha256") != PIN:
            raise ValueError("Expanded Unity recovery artifact has incorrect archive provenance")
        archive = None
    else:
        if sha(archive_path.read_bytes()) != PIN:
            raise ValueError("Pinned Unity source archive SHA-256 mismatch")
        recovery_root = None
        archive = zipfile.ZipFile(archive_path)
    manifest_path = root / "runtime/Original/FullSource/dependency-source-provenance.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    if manifest.get("archiveSha256") != PIN:
        raise ValueError("FullSource provenance does not name the pinned Unity archive")
    try:
        for name, entry in SOURCE_ENTRIES.items():
            original = archive.read(entry) if archive else (recovery_root / entry).read_bytes()
            item = manifest["recoveredReferences"][name]
            runtime = (root / item["runtimePath"]).read_bytes()
            if item["archiveEntry"] != entry or item["adaptation"] != "exact recovered source preserved as noncompiled reference":
                raise ValueError(f"Unexpected recovered source provenance: {name}")
            if sha(original) != item["sha256"] or original != runtime or sha(runtime) != item["runtimeSha256"]:
                raise ValueError(f"Recovered and reference source differs: {name}")
        print("PASS: four Mission camera scripts match the pinned Unity archive and noncompiled source provenance")
    finally:
        if archive:
            archive.close()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("archive", type=Path)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    args = parser.parse_args()
    verify(args.archive, args.root)
