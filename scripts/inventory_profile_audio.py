#!/usr/bin/env python3
"""Build a deterministic dependency inventory of WAV/OGG clips in a source profile."""

import argparse
import gzip
import json
from collections import Counter
from pathlib import Path, PurePosixPath

AUDIO_EXTENSIONS = {".ogg", ".wav"}


def inventory(profile_root: Path) -> dict:
    manifest = json.loads((profile_root / "manifest.json").read_text(encoding="utf-8"))
    with gzip.open(profile_root / "metadata.json.gz", "rt", encoding="utf-8") as source:
        metadata = json.load(source)

    path_to_guid = {}
    for guid, path in metadata["guidPaths"].items():
        if path in path_to_guid:
            raise ValueError(f"Multiple GUIDs map to the same source path: {path}")
        path_to_guid[path] = guid

    source_paths = set(metadata["assetManifest"])
    audio_assets = [
        asset for asset in manifest["assets"]
        if PurePosixPath(asset["path"]).suffix.lower() in AUDIO_EXTENSIONS
    ]
    entries = []
    for asset in sorted(audio_assets, key=lambda value: value["path"].casefold()):
        path = asset["path"]
        guid = path_to_guid.get(path)
        if not guid:
            raise ValueError(f"Packaged source audio has no GUID/path entry: {path}")
        references = [reference for reference in metadata["objectReferences"] if reference["guid"] == guid]
        if not references:
            raise ValueError(f"Packaged source audio has no serialized owner reference: {path}")
        normalized_references = []
        for reference in references:
            owner = reference["source"]
            if owner not in source_paths:
                raise ValueError(f"Audio reference owner is outside the selected profile: {owner} -> {path}")
            normalized_references.append({
                "source": owner,
                "sourceFileID": reference["sourceFileID"],
                "fieldPath": reference["fieldPath"],
                "targetFileID": reference["fileID"],
                "serializedReferenceType": reference["type"],
                "ownerMayBeInstantiatedAfterMissionStart": owner.lower().endswith(".prefab"),
            })
        normalized_references.sort(key=lambda value: (
            value["source"].casefold(), value["sourceFileID"], value["fieldPath"], value["targetFileID"]
        ))
        entries.append({
            "path": path,
            "guid": guid,
            "format": PurePosixPath(path).suffix.lower()[1:],
            "sourceBytes": asset["bytes"],
            "sourceSha256": asset["sha256"],
            "compressedJsonBytes": asset["compressedBytes"],
            "compressedJsonSha256": asset["compressedSha256"],
            "serializedReferenceCount": len(normalized_references),
            "references": normalized_references,
        })

    return {
        "schemaVersion": 1,
        "profile": manifest["profile"],
        "sourcePackageSha256": manifest["sourcePackageSha256"],
        "rootAssets": manifest["rootAssets"],
        "audioAssetCount": len(entries),
        "sourceBytes": sum(entry["sourceBytes"] for entry in entries),
        "compressedJsonBytes": sum(entry["compressedJsonBytes"] for entry in entries),
        "formatCounts": dict(sorted(Counter(entry["format"] for entry in entries).items())),
        "serializedReferenceCount": sum(entry["serializedReferenceCount"] for entry in entries),
        "referencesToRuntimeInstantiablePrefabs": sum(
            reference["ownerMayBeInstantiatedAfterMissionStart"]
            for entry in entries for reference in entry["references"]
        ),
        "assets": entries,
    }


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("profile", type=Path, help="Verified source profile directory")
    parser.add_argument("output", type=Path, help="Write deterministic JSON inventory here")
    args = parser.parse_args()
    result = inventory(args.profile)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(result, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    print(json.dumps({key: value for key, value in result.items() if key != "assets"}, sort_keys=True))


if __name__ == "__main__":
    main()
