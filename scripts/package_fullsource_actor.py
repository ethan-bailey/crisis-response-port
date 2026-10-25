#!/usr/bin/env python3
"""Build a bounded, provenance-preserving package for the original Operator prefab."""
import argparse
import gzip
import hashlib
import json
from pathlib import Path

ROOTS = ["Assets/Resources/characters/Operator.prefab"]
MAX_JSON_BYTES = 32 * 1024 * 1024
MAX_GZIP_BYTES = 12 * 1024 * 1024


def sha256(data):
    return hashlib.sha256(data).hexdigest()


def build(source_path, output_path):
    source_path, output_path = Path(source_path), Path(output_path)
    source_bytes = source_path.read_bytes()
    source_hash = sha256(source_bytes)
    with gzip.open(source_path, "rt", encoding="utf-8") as stream:
        source = json.load(stream)
    assets = source.get("assets")
    guid_paths = source.get("guidPaths")
    references = source.get("objectReferences")
    if not isinstance(assets, dict) or not isinstance(guid_paths, dict) or not isinstance(references, list):
        raise ValueError("Original scene package lacks asset dependency indexes")

    by_source = {}
    for ref in references:
        if ref.get("resolution") == "package":
            by_source.setdefault(ref.get("source"), []).append(ref)
    selected, pending = set(), list(ROOTS)
    while pending:
        path = pending.pop()
        if path in selected:
            continue
        asset = assets.get(path)
        if asset is None:
            raise ValueError(f"Original Operator package dependency is missing: {path}")
        selected.add(path)
        for ref in by_source.get(path, []):
            dependency = guid_paths.get(ref.get("guid"))
            if dependency:
                pending.append(dependency)

    package = {key: source[key] for key in (
        "format", "guidPaths", "guidMetadata", "scriptTypes", "scriptAssemblies",
        "builtinObjects", "builtinResourceRecords", "builtinPlayerResourceRecords", "sourceOfTruth"
    ) if key in source}
    package["seeds"] = ROOTS
    package["assets"] = {path: assets[path] for path in sorted(selected)}
    package["objectReferences"] = [ref for ref in references if ref.get("source") in selected]
    package["unresolvedGuids"] = [
        item for item in source.get("unresolvedGuids", [])
        if item.get("source") in selected
    ]
    raw = json.dumps(package, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    compressed = gzip.compress(raw, compresslevel=6, mtime=0)
    if len(raw) > MAX_JSON_BYTES or len(compressed) > MAX_GZIP_BYTES:
        raise ValueError(
            f"Operator source bundle exceeds its delivery budget: {len(raw)} JSON bytes, "
            f"{len(compressed)} gzip bytes"
        )
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_bytes(compressed)
    manifest = {
        "schemaVersion": 1,
        "profile": "operator-prefab",
        "encoding": "gzip-json",
        "rootAssets": ROOTS,
        "assetCount": len(selected),
        "recordCount": sum(len(assets[path].get("records", [])) for path in selected),
        "sourcePackageSha256": source_hash,
        "packageSha256": sha256(compressed),
        "compressedBytes": len(compressed),
        "uncompressedBytes": len(raw),
        "limits": {"compressedBytes": MAX_GZIP_BYTES, "uncompressedBytes": MAX_JSON_BYTES},
    }
    manifest_path = output_path.with_suffix("").with_suffix(".manifest.json")
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n")
    return manifest


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path, help="Full original scene package (.json.gz)")
    parser.add_argument("output", type=Path, help="Output gzip JSON package path")
    args = parser.parse_args()
    print(json.dumps(build(args.source, args.output), sort_keys=True))


if __name__ == "__main__":
    main()
