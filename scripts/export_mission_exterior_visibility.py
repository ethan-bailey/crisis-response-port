#!/usr/bin/env python3
"""Export Mission 01 Room exterior mesh and collider ownership from exact Unity scene YAML."""
import hashlib, json, re, sys
from pathlib import Path

ROOM_SCRIPT_GUID = "f3a1fb7605bda3ec004b5ea521a048f9"
HEADER = re.compile(r"^--- !u!(\d+) &(\d+)", re.M)

def records(text):
    matches = list(HEADER.finditer(text))
    for index, match in enumerate(matches):
        yield int(match.group(1)), match.group(2), text[match.end():matches[index+1].start() if index+1 < len(matches) else len(text)]

def ref(body, field):
    match = re.search(r"^\s*" + re.escape(field) + r": \{fileID: (-?\d+)\}", body, re.M)
    return match.group(1) if match else "0"

def main():
    if len(sys.argv) != 3:
        raise SystemExit("usage: export_mission_exterior_visibility.py MISSION_01_UNITY OUTPUT_JSON")
    scene, output = Path(sys.argv[1]), Path(sys.argv[2])
    raw = scene.read_bytes()
    text = raw.decode("utf-8", errors="strict")
    objects, transforms, components, rooms = {}, {}, [], []
    for class_id, file_id, body in records(text):
        if class_id == 1:
            name = re.search(r"^  m_Name: (.*)$", body, re.M)
            active = re.search(r"^  m_IsActive: (\d+)$", body, re.M)
            objects[file_id] = {"name": name.group(1).strip() if name else "",
                                "active": active is None or active.group(1) == "1"}
        elif class_id in (4, 224):
            transforms[file_id] = {"gameObject": ref(body, "m_GameObject"), "parent": ref(body, "m_Father")}
        elif class_id == 114:
            script = re.search(r"m_Script: \{fileID: \d+, guid: ([0-9a-f]{32})", body)
            if script and script.group(1) == ROOM_SCRIPT_GUID:
                exterior = re.search(r"^  exterior: (\d+)$", body, re.M)
                rooms.append({"componentFileID": file_id, "gameObjectFileID": ref(body, "m_GameObject"),
                              "exteriorTransformFileID": ref(body, "exteriorGeometry"),
                              "isExteriorRoom": exterior is not None and exterior.group(1) == "1"})
            components.append((class_id, file_id, body))
        elif class_id in (23, 64, 65, 135, 136):
            components.append((class_id, file_id, body))
    game_object_transforms = {item["gameObject"]: transform_id for transform_id, item in transforms.items()}
    exterior_rooms = [room for room in rooms if room["isExteriorRoom"]]
    if len(exterior_rooms) != 1:
        raise ValueError(f"Expected one exterior Room hierarchy, found {len(exterior_rooms)}")
    children = {}
    for transform_id, item in transforms.items():
        children.setdefault(item["parent"], []).append(transform_id)
    renderer_data, collider_data = [], []
    for class_id, file_id, body in components:
        game_object = ref(body, "m_GameObject")
        if class_id == 23:
            batch = re.search(r"m_StaticBatchInfo:\s*\n\s*firstSubMesh: (\d+)\s*\n\s*subMeshCount: (\d+)", body)
            if batch:
                first, count = map(int, batch.groups())
                if count:
                    renderer_data.append({"componentFileID": file_id, "gameObjectFileID": game_object,
                                          "firstSubMesh": first, "subMeshCount": count})
        elif class_id in (64, 65, 135, 136):
            collider_data.append({"componentFileID": file_id, "gameObjectFileID": game_object,
                                  "classID": class_id})
    all_indices, all_colliders = set(), set()
    for room in rooms:
        root = room["exteriorTransformFileID"]
        if root != "0":
            stack, descendant_transforms = [root], []
            while stack:
                current = stack.pop()
                if current in descendant_transforms:
                    raise ValueError("Mission exterior hierarchy contains a cycle")
                descendant_transforms.append(current)
                stack.extend(children.get(current, []))
            game_objects = {transforms[item]["gameObject"] for item in descendant_transforms if item in transforms}
            room["visibilityRootKind"] = "exteriorGeometry"
            room["visibilityRootTransformFileID"] = root
        elif room["isExteriorRoom"]:
            # The exterior Room has no exteriorGeometry reference. Its authored
            # Entrance exterior wall must still follow disableExteriorWalls.
            root = game_object_transforms.get(room["gameObjectFileID"], "0")
            if root == "0":
                raise ValueError("Exterior Room has no scene Transform")
            stack, descendant_transforms = [root], []
            while stack:
                current = stack.pop()
                if current in descendant_transforms:
                    raise ValueError("Mission exterior hierarchy contains a cycle")
                descendant_transforms.append(current)
                stack.extend(children.get(current, []))
            descendants = {transforms[item]["gameObject"] for item in descendant_transforms if item in transforms}
            game_objects = {item for item in descendants
                            if objects.get(item, {}).get("name", "").lower().endswith("exterior wall")}
            if len(game_objects) != 1:
                raise ValueError(f"Expected one authored exterior wall under the exterior Room, found {len(game_objects)}")
            room["visibilityRootKind"] = "named-exterior-wall-descendant"
            room["visibilityRootTransformFileID"] = root
        else:
            continue
        room["gameObjectFileIDs"] = sorted(game_objects, key=int)
        room["name"] = objects.get(room["gameObjectFileID"], {}).get("name", "")
        renderers = [item for item in renderer_data if item["gameObjectFileID"] in game_objects]
        colliders = [item for item in collider_data if item["gameObjectFileID"] in game_objects]
        room["renderers"], room["colliders"] = renderers, colliders
        room["submeshIndices"] = sorted({i for item in renderers
                                         for i in range(item["firstSubMesh"], item["firstSubMesh"] + item["subMeshCount"])})
        room["colliderFileIDs"] = sorted({item["componentFileID"] for item in colliders}, key=int)
        all_indices.update(room["submeshIndices"])
        all_colliders.update(room["colliderFileIDs"])
    if len(rooms) != 8 or not all_indices:
        raise ValueError(f"Unexpected Mission 01 Room exterior inventory: rooms={len(rooms)}, submeshes={len(all_indices)}")
    result = {"format": "crisis-mission-exterior-visibility-v1", "scenePath": "Assets/Scenes/Mission 01.unity",
              "sceneSha256": hashlib.sha256(raw).hexdigest(), "roomScriptGuid": ROOM_SCRIPT_GUID,
              "roomCount": len(rooms), "submeshCount": 764, "rooms": rooms,
              "submeshIndices": sorted(all_indices), "colliderFileIDs": sorted(all_colliders, key=int)}
    if any(i < 0 or i >= result["submeshCount"] for i in all_indices):
        raise ValueError("Exterior renderer references an out-of-range combined mesh submesh")
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(result, indent=2, sort_keys=True) + "\n")
    print(json.dumps({"sceneSha256": result["sceneSha256"], "roomCount": result["roomCount"],
                      "exteriorSubmeshCount": len(all_indices), "exteriorColliderCount": len(all_colliders)}))

if __name__ == "__main__":
    main()
