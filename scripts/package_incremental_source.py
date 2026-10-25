#!/usr/bin/env python3
"""Build a hash-verified, per-asset source profile from an exported package."""
import argparse
import gzip
import hashlib
import json
from pathlib import Path

DEFAULT_ROOTS = [
    "Assets/Resources/Global context.prefab",
    "Assets/GameObject/Interior mode UI.prefab",
    "Assets/Resources/ui/Interaction menu.prefab",
    "Assets/Resources/characters/Operator.prefab",
]


def digest(data):
    return hashlib.sha256(data).hexdigest()


def read_package(path):
    data = Path(path).read_bytes()
    raw = gzip.decompress(data) if data.startswith(b"\x1f\x8b") else data
    return json.loads(raw), digest(data)


def build(source_path, output_dir, profile, roots):
    source, source_hash = read_package(source_path)
    assets = source.get("assets")
    guid_paths = source.get("guidPaths")
    references = source.get("objectReferences")
    if not isinstance(assets, dict) or not isinstance(guid_paths, dict) or not isinstance(references, list):
        raise ValueError("Original source package lacks asset dependency indexes")

    by_source = {}
    for ref in references:
        if ref.get("resolution") == "package":
            by_source.setdefault(ref.get("source"), []).append(ref)
    selected, pending = set(), list(roots)
    while pending:
        path = pending.pop()
        if path in selected:
            continue
        if path not in assets:
            raise ValueError(f"Original source dependency is missing: {path}")
        selected.add(path)
        for ref in by_source.get(path, []):
            dependency = guid_paths.get(ref.get("guid"))
            if dependency:
                pending.append(dependency)

    output_dir = Path(output_dir)
    if output_dir.exists():
        raise ValueError(f"Output directory must be new; refusing to mix profiles: {output_dir}")
    assets_dir = output_dir / "assets"
    assets_dir.mkdir(parents=True, exist_ok=True)
    asset_manifest, deliveries = {}, []
    for path in sorted(selected):
        raw = json.dumps(assets[path], ensure_ascii=False, separators=(",", ":")).encode("utf-8")
        compressed = gzip.compress(raw, compresslevel=6, mtime=0)
        name = hashlib.sha256(path.encode("utf-8")).hexdigest()[:24] + ".json.gz"
        (assets_dir / name).write_bytes(compressed)
        entry = {"path": path, "file": "assets/" + name, "bytes": len(raw),
                 "sha256": digest(raw), "compressedBytes": len(compressed),
                 "compressedSha256": digest(compressed)}
        asset_manifest[path] = {"bytes": len(raw), "sha256": digest(raw)}
        deliveries.append(entry)

    metadata = {key: source[key] for key in (
        "format", "guidPaths", "guidMetadata", "scriptTypes", "scriptAssemblies",
        "builtinObjects", "builtinResourceRecords", "builtinPlayerResourceRecords", "sourceOfTruth"
    ) if key in source}
    metadata["seeds"] = roots
    metadata["assetManifest"] = asset_manifest
    metadata["objectReferences"] = [ref for ref in references if ref.get("source") in selected]
    metadata["unresolvedGuids"] = [item for item in source.get("unresolvedGuids", [])
                                   if item.get("source") in selected]
    metadata_raw = json.dumps(metadata, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    metadata_gzip = gzip.compress(metadata_raw, compresslevel=6, mtime=0)
    metadata_file = "metadata.json.gz"
    (output_dir / metadata_file).write_bytes(metadata_gzip)
    manifest = {
        "schemaVersion": 1, "profile": profile, "encoding": "gzip-json-assets",
        "rootAssets": roots, "sourcePackageSha256": source_hash,
        "assetCount": len(selected),
        "recordCount": sum(len(assets[path].get("records", [])) for path in selected),
        "uncompressedAssetBytes": sum(item["bytes"] for item in deliveries),
        "compressedAssetBytes": sum(item["compressedBytes"] for item in deliveries),
        "maximumAssetBytes": max((item["bytes"] for item in deliveries), default=0),
        "maximumCompressedAssetBytes": max((item["compressedBytes"] for item in deliveries), default=0),
        "metadata": {"file": metadata_file, "bytes": len(metadata_raw),
                     "sha256": digest(metadata_raw), "compressedBytes": len(metadata_gzip),
                     "compressedSha256": digest(metadata_gzip)},
        "assets": deliveries,
    }
    (output_dir / "manifest.json").write_text(json.dumps(manifest, indent=2) + "\n")
    return manifest


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path, help="Original source package JSON or gzip JSON")
    parser.add_argument("output", type=Path, help="Profile output directory")
    parser.add_argument("--profile", default="global-ui-operator")
    parser.add_argument("--root", action="append", dest="roots", help="Root source asset; repeatable")
    args = parser.parse_args()
    result = build(args.source, args.output, args.profile, args.roots or DEFAULT_ROOTS)
    print(json.dumps({key: value for key, value in result.items() if key != "assets"}, sort_keys=True))


if __name__ == "__main__":
    main()
