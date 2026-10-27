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
LISTENER_RE = re.compile(r"(?ms)^--- !u!81 &(\S+)\nAudioListener:\n(.*?)(?=^--- !u!|\Z)")
GAME_OBJECT_RE = re.compile(r"(?ms)^--- !u!1 &(\S+)\nGameObject:\n(.*?)(?=^--- !u!|\Z)")
COMPONENT_RE = re.compile(r"(?ms)^--- !u!(\d+) &(\S+)\n([^\n]+):\n(.*?)(?=^--- !u!|\Z)")
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
    metadata_path = profile_dir / "metadata.json.gz"
    metadata = json.loads(gzip.open(metadata_path, "rt", encoding="utf-8").read()) if metadata_path.exists() else {}
    guid_paths = metadata.get("guidPaths", {})
    found = {}
    components = []
    hierarchy_by_asset = {}
    listener_hierarchy_by_asset = {}
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
        object_active = {}
        for game_object in GAME_OBJECT_RE.finditer(source_text):
            name = scalar(game_object.group(2), "m_Name")
            if name is not None:
                object_names[game_object.group(1)] = name
            active_value = scalar(game_object.group(2), "m_IsActive")
            object_active[game_object.group(1)] = (active_value == "1") if active_value in ("0", "1") else None
        go_components = collections.defaultdict(list)
        go_script_paths = collections.defaultdict(list)
        go_script_guids = collections.defaultdict(list)
        transform_by_go = {}
        transform_go = {}
        parent_by_transform = {}
        for record in COMPONENT_RE.finditer(source_text):
            class_id, file_id, class_name, record_block = record.groups()
            owner_ref = scalar(record_block, "m_GameObject")
            owner_match = re.search(r"fileID:\s*(\d+)", owner_ref or "")
            owner_id = owner_match.group(1) if owner_match else None
            if owner_id:
                go_components[owner_id].append(class_name)
                if class_id == "114":
                    script_match = re.search(r"(?m)^  m_Script: \{fileID:\s*[^,]+, guid:\s*([0-9a-f]+)", record_block)
                    if script_match:
                        script_guid = script_match.group(1)
                        go_script_guids[owner_id].append(script_guid)
                        if script_guid in guid_paths:
                            go_script_paths[owner_id].append(guid_paths[script_guid])
            if class_id in ("4", "224") and owner_id:
                transform_by_go[owner_id] = file_id
                transform_go[file_id] = owner_id
                father_ref = scalar(record_block, "m_Father")
                father_match = re.search(r"fileID:\s*(\d+)", father_ref or "")
                parent_by_transform[file_id] = father_match.group(1) if father_match else "0"
        asset_hierarchy = []
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
            chain = []
            seen = set()
            transform_id = transform_by_go.get(owner_id)
            while transform_id and transform_id != "0" and transform_id not in seen:
                seen.add(transform_id)
                ancestor_go = transform_go.get(transform_id)
                if ancestor_go is None:
                    break
                chain.append({
                    "gameObjectFileID": ancestor_go,
                    "gameObjectName": object_names.get(ancestor_go),
                    "transformFileID": transform_id,
                    "componentTypes": sorted(go_components.get(ancestor_go, [])),
                    "monoBehaviourScriptGuids": sorted(go_script_guids.get(ancestor_go, [])),
                    "monoBehaviourScriptPaths": sorted(go_script_paths.get(ancestor_go, [])),
                    "activeSelf": object_active.get(ancestor_go),
                })
                transform_id = parent_by_transform.get(transform_id, "0")
            asset_hierarchy.append({
                "audioSourceFileID": match.group(1),
                "gameObjectFileID": owner_id,
                "gameObjectName": object_names.get(owner_id),
                "transformFound": bool(chain),
                "chain": chain,
            })
        if asset_hierarchy:
            hierarchy_by_asset[path] = asset_hierarchy
        asset_listeners = []
        for match in LISTENER_RE.finditer(source_text):
            block = match.group(2)
            owner_ref = scalar(block, "m_GameObject")
            owner_match = re.search(r"fileID:\s*(\d+)", owner_ref or "")
            owner_id = owner_match.group(1) if owner_match else None
            chain = []
            seen = set()
            transform_id = transform_by_go.get(owner_id)
            hierarchy_active = True
            active_known = bool(transform_id)
            while transform_id and transform_id != "0" and transform_id not in seen:
                seen.add(transform_id)
                ancestor_go = transform_go.get(transform_id)
                if ancestor_go is None:
                    break
                active_self = object_active.get(ancestor_go)
                active_known = active_known and active_self is not None
                hierarchy_active = hierarchy_active and active_self is True
                chain.append({
                    "gameObjectFileID": ancestor_go,
                    "gameObjectName": object_names.get(ancestor_go),
                    "transformFileID": transform_id,
                    "componentTypes": sorted(go_components.get(ancestor_go, [])),
                    "monoBehaviourScriptGuids": sorted(go_script_guids.get(ancestor_go, [])),
                    "monoBehaviourScriptPaths": sorted(go_script_paths.get(ancestor_go, [])),
                    "activeSelf": active_self,
                    "activeInHierarchy": hierarchy_active if active_known else None,
                })
                transform_id = parent_by_transform.get(transform_id, "0")
            asset_listeners.append({
                "fileID": match.group(1),
                "gameObjectFileID": owner_id,
                "gameObjectName": object_names.get(owner_id),
                "enabled": scalar(block, "m_Enabled"),
                "activeSelf": object_active.get(owner_id),
                "activeInHierarchy": hierarchy_active if active_known else None,
                "transformFound": bool(chain),
                "chain": chain,
            })
        if asset_listeners:
            listener_hierarchy_by_asset[path] = asset_listeners

    missing = set(expected) - set(found)
    if missing:
        raise ValueError(f"profile descriptors missing source text for {len(missing)} assets")
    if len(found) != manifest["assetCount"]:
        raise ValueError(f"profile descriptor count mismatch: {len(found)} != {manifest['assetCount']}")
    components.sort(key=lambda item: (item["assetPath"], int(item["fileID"])))
    chains = [item for entries in hierarchy_by_asset.values() for item in entries]
    hierarchy_depths = collections.Counter(str(len(item["chain"])) for item in chains)
    owner_types = collections.Counter()
    owner_and_ancestor_types = collections.Counter()
    direct_script_paths = collections.Counter()
    chain_script_paths = collections.Counter()
    unresolved_script_guids = collections.Counter()
    for item in chains:
        for depth, node in enumerate(item["chain"]):
            if depth == 0:
                owner_types.update(node["componentTypes"])
                direct_script_paths.update(node["monoBehaviourScriptPaths"])
            owner_and_ancestor_types.update(node["componentTypes"])
            chain_script_paths.update(node["monoBehaviourScriptPaths"])
            unresolved_script_guids.update(guid for guid in node["monoBehaviourScriptGuids"] if not guid_paths.get(guid))
    listeners = [item for entries in listener_hierarchy_by_asset.values() for item in entries]
    return {
        "schemaVersion": 2,
        "profile": manifest["profile"],
        "sourcePackageSha256": manifest["sourcePackageSha256"],
        "assetCount": manifest["assetCount"],
        "serializedAudioSourceCount": len(components),
        "spatialBlendSerializedCount": sum(item["fields"]["SpatialBlend"] is not None for item in components),
        "fieldPresenceCounts": dict(sorted(field_counts.items())),
        "valueHistograms": {key: dict(sorted(counter.items())) for key, counter in value_counts.items()},
        "components": components,
        "transformHierarchyByAsset": hierarchy_by_asset,
        "listenerHierarchyByAsset": listener_hierarchy_by_asset,
        "listenerHierarchySummary": {
            "listenerCount": len(listeners),
            "transformFoundCount": sum(item["transformFound"] for item in listeners),
            "enabledCount": sum(item["enabled"] == "1" for item in listeners),
            "activeSelfCount": sum(item["activeSelf"] is True for item in listeners),
            "activeInHierarchyCount": sum(item["activeInHierarchy"] is True for item in listeners),
            "activeInHierarchyUnknownCount": sum(item["activeInHierarchy"] is None for item in listeners),
        },
        "hierarchySummary": {
            "audioSourceCount": len(chains),
            "transformFoundCount": sum(item["transformFound"] for item in chains),
            "depthHistogram": dict(sorted(hierarchy_depths.items(), key=lambda pair: int(pair[0]))),
            "sourceOwnerComponentIncidences": dict(sorted(owner_types.items())),
            "sourceOwnerAndAncestorComponentIncidences": dict(sorted(owner_and_ancestor_types.items())),
            "sourceOwnerRigidbodyCount": sum(owner_types[name] for name in ("Rigidbody", "Rigidbody2D")),
            "sourceOwnerAndAncestorRigidbodyIncidences": sum(owner_and_ancestor_types[name] for name in ("Rigidbody", "Rigidbody2D")),
            "sourceOwnerAndAncestorMonoBehaviourIncidences": owner_and_ancestor_types["MonoBehaviour"],
            "sourceOwnerScriptPathIncidences": dict(sorted(direct_script_paths.items())),
            "sourceOwnerAndAncestorScriptPathIncidences": dict(sorted(chain_script_paths.items())),
            "unresolvedSourceScriptGuidIncidences": dict(sorted(unresolved_script_guids.items())),
            "audioSourcesWithOwnerRigidbody": sum(any(name in item["chain"][0]["componentTypes"] for name in ("Rigidbody", "Rigidbody2D")) for item in chains if item["chain"]),
            "audioSourcesWithOwnerOrAncestorRigidbody": sum(any(name in node["componentTypes"] for node in item["chain"] for name in ("Rigidbody", "Rigidbody2D")) for item in chains),
            "audioSourcesWithOwnerOrAncestorMonoBehaviour": sum(any("MonoBehaviour" in node["componentTypes"] for node in item["chain"]) for item in chains),
        },
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
