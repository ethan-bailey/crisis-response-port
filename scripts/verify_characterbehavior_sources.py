#!/usr/bin/env python3
"""Verify recovered original AI action/condition source and preserved Unity metadata."""
import argparse
import hashlib
import json
from pathlib import Path
import zipfile

ROOT = Path(__file__).resolve().parents[1]
PIN = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"


def sha(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def verify(archive_path: Path | None) -> None:
    archive = zipfile.ZipFile(archive_path) if archive_path else None
    try:
        if archive:
            with archive_path.open("rb") as stream:
                if hashlib.file_digest(stream, "sha256").hexdigest() != PIN:
                    raise SystemExit("Pinned original recovery archive SHA-256 mismatch")
        manifest_path = ROOT / "runtime/Original/CharacterBehavior/source-provenance.json"
        manifest = json.loads(manifest_path.read_text())
        if manifest["archiveSha256"] != PIN:
            raise SystemExit("AI source manifest does not point at the pinned recovery archive")
        checked = 0
        for rel, item in manifest["sources"].items():
            runtime_path = ROOT / "runtime/Original/CharacterBehavior" / rel
            if rel.startswith("FullSource/"):
                runtime_path = ROOT / "runtime/Original" / rel
            runtime = runtime_path.read_bytes()
            if sha(runtime) != item["runtimeSha256"]:
                raise SystemExit(f"AI runtime source changed without provenance update: {runtime_path}")
            if archive:
                original = archive.read(item["archiveEntry"])
                if sha(original) != item["sourceSha256"]:
                    raise SystemExit(f"Pinned archive source mismatch: {item['archiveEntry']}")
            checked += 1

        plugin_manifest = json.loads((ROOT / "runtime/Original/BehaviorDesigner/source-provenance.json").read_text())
        for name, item in plugin_manifest["sources"].items():
            if "sourceArchiveEntry" not in item:
                continue
            runtime = (ROOT / item["runtimePath"]).read_bytes()
            if sha(runtime) != item["runtimeSha256"]:
                raise SystemExit(f"Behavior Designer task runtime source changed: {item['runtimePath']}")
            if archive:
                original = archive.read(item["sourceArchiveEntry"])
                if sha(original) != item["decompiledSha256"]:
                    raise SystemExit(f"Behavior Designer task source mismatch: {item['sourceArchiveEntry']}")
            checked += 1
        print(f"PASS: {checked} original AI source/metadata records; archive {'verified' if archive else 'not supplied'}")
    finally:
        if archive:
            archive.close()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--archive", type=Path, help="Pinned original-recovery-artifact.zip")
    verify(parser.parse_args().archive)
