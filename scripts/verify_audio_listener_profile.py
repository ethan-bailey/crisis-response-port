#!/usr/bin/env python3
"""Verify the complete AudioListener dependency set in a packaged Unity profile."""

import argparse
import gzip
import json
from pathlib import Path


EXPECTED = {
    ("Assets/Scenes/Mission 01.unity", "5409"): ("Camera", 1),
    ("Assets/Scenes/Mission 01.unity", "5410"): ("Mission intro", 0),
    ("Assets/GameObject/Interior mode.prefab", "81977153420339851"): ("Audio listener", 1),
    ("Assets/GameObject/Marksman mode.prefab", "81571718729274231"): ("Marksman camera", 1),
}


def identifier(value):
    if isinstance(value, dict):
        value = value.get("fileID", "0")
    return str(value)


def verify(profile_root: Path) -> dict:
    manifest = json.loads((profile_root / "manifest.json").read_text(encoding="utf-8"))
    assets = {entry["path"]: entry for entry in manifest["assets"]}
    listeners = []
    expected_paths = {path for path, _ in EXPECTED}

    # Count the full selected profile, not only the four known files. This
    # catches a newly introduced listener hidden in a transitive prefab/scene.
    for entry in manifest["assets"]:
        path = entry["path"]
        if not path.lower().endswith((".unity", ".prefab")):
            continue
        descriptor = json.loads(gzip.decompress((profile_root / entry["file"]).read_bytes()))
        records = descriptor.get("records", [])
        game_objects = {
            record["fileID"]: record
            for record in records
            if record.get("classID") == 1
        }
        transforms = {
            record["fileID"]: record
            for record in records
            if record.get("classID") in (4, 224)
        }
        transform_by_game_object = {
            identifier(record.get("fields", {}).get("m_GameObject")): record
            for record in transforms.values()
        }
        for record in records:
            if record.get("classID") != 81:
                continue
            fields = record.get("fields", {})
            go_id = identifier(fields.get("m_GameObject"))
            go = game_objects.get(go_id)
            if go is None:
                raise ValueError(f"AudioListener {path}:{record['fileID']} has no owner GameObject {go_id}")
            go_fields = go.get("fields", {})
            go_name = go_fields.get("m_Name", "")
            hierarchy = [go_name]
            active_hierarchy = bool(go_fields.get("m_IsActive", 1))
            transform = transform_by_game_object.get(go_id)
            seen = set()
            while transform is not None:
                transform_id = transform["fileID"]
                if transform_id in seen:
                    raise ValueError(f"Transform cycle while resolving AudioListener {path}:{record['fileID']}")
                seen.add(transform_id)
                parent_id = identifier(transform.get("fields", {}).get("m_Father"))
                if parent_id == "0":
                    break
                parent_transform = transforms.get(parent_id)
                if parent_transform is None:
                    raise ValueError(f"AudioListener {path}:{record['fileID']} has unresolved parent Transform {parent_id}")
                parent_go_id = identifier(parent_transform.get("fields", {}).get("m_GameObject"))
                parent_go = game_objects.get(parent_go_id)
                if parent_go is None:
                    raise ValueError(f"AudioListener {path}:{record['fileID']} has unresolved parent GameObject {parent_go_id}")
                parent_fields = parent_go.get("fields", {})
                hierarchy.append(parent_fields.get("m_Name", ""))
                active_hierarchy = active_hierarchy and bool(parent_fields.get("m_IsActive", 1))
                transform = parent_transform
            listeners.append({
                "assetPath": path,
                "componentFileID": record["fileID"],
                "ownerName": go_name,
                "ownerFileID": go_id,
                "enabled": fields.get("m_Enabled") == 1,
                "activeSelf": bool(go_fields.get("m_IsActive", 1)),
                "activeInHierarchy": active_hierarchy,
                "hierarchy": hierarchy,
            })

    found = {(item["assetPath"], item["componentFileID"]): item for item in listeners}
    if len(listeners) != len(EXPECTED):
        raise ValueError(f"Expected exactly four AudioListeners in the complete selected profile; found {len(listeners)}: {listeners}")
    missing_assets = expected_paths - assets.keys()
    if missing_assets:
        raise ValueError(f"Listener dependency assets are absent from the selected profile: {sorted(missing_assets)}")
    for key, (owner_name, active_self) in EXPECTED.items():
        item = found.get(key)
        if item is None:
            raise ValueError(f"Expected serialized AudioListener is missing: {key}")
        if item["ownerName"] != owner_name or not item["enabled"] or item["activeSelf"] != bool(active_self):
            raise ValueError(f"AudioListener owner/state changed for {key}: {item}")
    camera = found[("Assets/Scenes/Mission 01.unity", "5409")]
    intro = found[("Assets/Scenes/Mission 01.unity", "5410")]
    interior = found[("Assets/GameObject/Interior mode.prefab", "81977153420339851")]
    marksman = found[("Assets/GameObject/Marksman mode.prefab", "81571718729274231")]
    if "Negotiation mode" not in camera["hierarchy"] or camera["activeInHierarchy"]:
        raise ValueError(f"Mission Camera listener hierarchy changed: {camera}")
    if intro["activeInHierarchy"]:
        raise ValueError(f"Mission intro listener is unexpectedly active in hierarchy: {intro}")
    if "Interior mode" not in interior["hierarchy"] or interior["activeInHierarchy"]:
        raise ValueError(f"InteriorMode listener prefab hierarchy changed: {interior}")
    if "Marksman mode" not in marksman["hierarchy"] or marksman["activeInHierarchy"]:
        raise ValueError(f"Marksman listener prefab hierarchy changed: {marksman}")
    return {"status": "passed", "profile": manifest.get("profile"), "listenerCount": len(listeners), "listeners": listeners}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("profile", type=Path, help="Verified source profile directory")
    args = parser.parse_args()
    print(json.dumps(verify(args.profile), sort_keys=True))


if __name__ == "__main__":
    main()
