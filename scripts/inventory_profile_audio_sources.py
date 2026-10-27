#!/usr/bin/env python3
"""Inventory serialized AudioSource dependencies in a verified Unity profile."""

from __future__ import annotations

import argparse
import collections
import gzip
import hashlib
import json
import re
import sys
from pathlib import Path


ASSET_RE = re.compile(r"(?ms)^--- !u!82 &(\S+)\nAudioSource:\n(.*?)(?=^--- !u!|\Z)")
GAME_OBJECT_RE = re.compile(r"(?ms)^--- !u!1 &(\S+)\nGameObject:\n(.*?)(?=^--- !u!|\Z)")
SOURCE_FIELDS = (
    "m_Enabled", "m_audioClip", "OutputAudioMixerGroup", "m_PlayOnAwake", "m_Volume",
    "m_Pitch", "Loop", "Mute", "Spatialize", "SpatializePostEffects", "Priority",
    "DopplerLevel", "MinDistance", "MaxDistance", "Pan2D", "rolloffMode",
    "rolloffCustomCurve", "panLevelCustomCurve", "spreadCustomCurve", "ReverbZoneMix",
    "reverbZoneMixCustomCurve", "BypassEffects", "BypassListenerEffects", "BypassReverbZones",
)


def scalar(block: str, key: str) -> str | None:
    match = re.search(rf"(?m)^  {re.escape(key)}:\s*(.*)$", block)
    return match.group(1).strip() if match else None


def profile_inventory(profile_dir: Path) -> dict:
    manifest = json.loads((profile_dir / "manifest.json").read_text(encoding="utf-8"))
    assets_dir = profile_dir / "assets"
    expected = {entry["path"]: entry for entry in manifest["assets"]}
    found = {}
    components = []
    field_counts = collections.Counter()
    value_counts = {name: collections.Counter() for name in (
        "SpatialBlend", "DopplerLevel", "MinDistance", "MaxDistance", "Pan2D", "rolloffMode"
    )}

    for asset_file in sorted(assets_dir.glob("*.json.gz")):
        with gzip.open(asset_file, "rt", encoding="utf-8") as stream:
            descriptor = json.load(stream)
        path = descriptor.get("path")
        if path not in expected:
            raise ValueError(f"asset not present in profile manifest: {path}")
        found[path] = True
        source_text = descriptor.get("sourceText")
        if source_text is None:
            continue
        raw = source_text.encode("utf-8")
        if hashlib.sha256(raw).hexdigest() != descriptor.get("sha256"):
            raise ValueError(f"source text hash mismatch: {path}")
        if not path.lower().endswith((".prefab", ".unity")):
            continue

        object_names = {}
        for game_object in GAME_OBJECT_RE.finditer(source_text):
            name = scalar(game_object.group(2), "m_Name")
            if name is not None:
                object_names[game_object.group(1)] = name
        for match in ASSET_RE.finditer(source_text):
            block = match.group(2)
            values = {key: scalar(block, key) for key in SOURCE_FIELDS}
            values = {key: value for key, value in values.items() if value is not None}
            owner_ref = scalar(block, "m_GameObject")
            owner_match = re.search(r"fileID:\s*(\d+)", owner_ref or "")
            owner_id = owner_match.group(1) if owner_match else None
            fields = set(values)
            field_counts.update(fields)
            # SpatialBlend is a runtime API property; record its serialized absence explicitly.
            spatial = scalar(block, "SpatialBlend")
            values["SpatialBlend"] = spatial if spatial is not None else None
            for field in value_counts:
                value_counts[field][str(values.get(field, "<omitted>"))] += 1
            components.append({
                "assetPath": path,
                "fileID": match.group(1),
                "gameObjectFileID": owner_id,
                "gameObjectName": object_names.get(owner_id),
                "fields": values,
            })

    missing = set(expected) - set(found)
    if missing:
        raise ValueError(f"profile descriptors missing source text for {len(missing)} assets")
    if len(found) != manifest["assetCount"]:
        raise ValueError(f"profile descriptor count mismatch: {len(found)} != {manifest['assetCount']}")
    components.sort(key=lambda item: (item["assetPath"], int(item["fileID"])))
    return {
        "schemaVersion": 1,
        "profile": manifest["profile"],
        "sourcePackageSha256": manifest["sourcePackageSha256"],
        "assetCount": manifest["assetCount"],
        "serializedAudioSourceCount": len(components),
        "spatialBlendSerializedCount": sum(item["fields"]["SpatialBlend"] is not None for item in components),
        "fieldPresenceCounts": dict(sorted(field_counts.items())),
        "valueHistograms": {key: dict(sorted(counter.items())) for key, counter in value_counts.items()},
        "components": components,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("profile_dir", type=Path)
    parser.add_argument("output_json", type=Path)
    args = parser.parse_args()
    try:
        result = profile_inventory(args.profile_dir)
        args.output_json.parent.mkdir(parents=True, exist_ok=True)
        args.output_json.write_text(json.dumps(result, indent=2, sort_keys=True) + "\n", encoding="utf-8")
        print(json.dumps({key: result[key] for key in (
            "profile", "sourcePackageSha256", "assetCount", "serializedAudioSourceCount",
            "spatialBlendSerializedCount", "fieldPresenceCounts", "valueHistograms",
        )}, sort_keys=True))
        return 0
    except (OSError, ValueError, KeyError, json.JSONDecodeError) as error:
        print(f"AudioSource inventory failed: {error}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
