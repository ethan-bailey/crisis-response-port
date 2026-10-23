#!/usr/bin/env python3
"""Inventory Mesh geometry and MeshFilter links in the original Unity player.

Requires the pinned UnityPy==1.25.4 parser. The input directory must be produced
by extract_original_player_data.py; its manifest and original archive hash are
checked before any serialized asset is read. Geometry stays in the private
player archive; this report stores hashes and counts only.
"""
from __future__ import annotations

import argparse
import hashlib
import json
from importlib.metadata import version
from pathlib import Path
from typing import Any

ARCHIVE_SHA256 = "5a12ff494b26713d149f9be026619732247f817ddf8944a399b6ee1d12ea0ab3"
PARSER_VERSION = "1.25.4"
CONTAINERS = (
    "resources.assets",
    "globalgamemanagers.assets",
    "sharedassets0.assets",
    "sharedassets1.assets",
    "sharedassets2.assets",
    "sharedassets3.assets",
    "sharedassets4.assets",
    "sharedassets5.assets",
    "Resources/unity_builtin_extra",
)


def sha256(raw: bytes) -> str:
    return hashlib.sha256(raw).hexdigest()


def _vector(value: Any) -> list[float]:
    if hasattr(value, "x"):
        return [float(value.x), float(value.y), float(value.z)]
    return [float(part) for part in value]


def _name(tree: dict[str, Any]) -> str:
    direct = tree.get("m_Name")
    if direct:
        return str(direct)
    parsed = tree.get("m_ParsedForm")
    if isinstance(parsed, dict):
        return str(parsed.get("m_Name", ""))
    return ""


def build_inventory(player_data: Path, extraction_manifest: Path) -> dict[str, Any]:
    if version("UnityPy") != PARSER_VERSION:
        raise RuntimeError(f"UnityPy {PARSER_VERSION} is required; found {version('UnityPy')}")
    import UnityPy
    from UnityPy.helpers.MeshHelper import MeshHandler

    manifest = json.loads(extraction_manifest.read_text(encoding="utf-8"))
    if manifest.get("archiveSha256") != ARCHIVE_SHA256:
        raise RuntimeError("Extraction manifest does not identify the pinned original archive")
    extracted = {item["path"]: item for item in manifest.get("files", [])}
    files: list[dict[str, Any]] = []
    meshes: list[dict[str, Any]] = []
    builtins: list[dict[str, Any]] = []

    for relative in CONTAINERS:
        source = player_data / relative
        if not source.is_file() or relative not in extracted:
            raise RuntimeError(f"Verified player container is missing: {relative}")
        raw_file = source.read_bytes()
        if sha256(raw_file) != extracted[relative]["sha256"]:
            raise RuntimeError(f"Extracted player container hash differs from its manifest: {relative}")
        env = UnityPy.load(str(source))
        serialized_files = list(env.files.values())
        if len(serialized_files) != 1:
            raise RuntimeError(f"Expected one serialized file in {relative}; found {len(serialized_files)}")
        serialized = serialized_files[0]
        if serialized.unity_version != "2017.2.0f3" or int(serialized.target_platform) != 5:
            raise RuntimeError(f"Unexpected player container header in {relative}")
        objects = {obj.path_id: obj for obj in env.objects}
        files.append({
            "path": relative,
            "bytes": len(raw_file),
            "sha256": sha256(raw_file),
            "unityVersion": serialized.unity_version,
            "targetPlatform": int(serialized.target_platform),
            "objectCount": len(objects),
        })

        if relative == "Resources/unity_builtin_extra":
            for obj in sorted(env.objects, key=lambda item: item.path_id):
                tree = obj.read_typetree()
                builtins.append({
                    "pathId": str(obj.path_id),
                    "type": obj.type.name,
                    "name": _name(tree) if isinstance(tree, dict) else "",
                })

        references_by_mesh: dict[int, list[dict[str, str]]] = {}
        game_object_names: dict[int, str] = {}
        for component in env.objects:
            if component.type.name != "MeshFilter":
                continue
            fields = component.read_typetree()
            mesh_pointer = fields.get("m_Mesh", {})
            if int(mesh_pointer.get("m_FileID", 0)) != 0:
                continue
            mesh_id = int(mesh_pointer.get("m_PathID", 0))
            game_object_id = int(fields.get("m_GameObject", {}).get("m_PathID", 0))
            game_object = objects.get(game_object_id)
            if game_object and game_object_id not in game_object_names:
                game_object_names[game_object_id] = str(game_object.read_typetree().get("m_Name", ""))
            references_by_mesh.setdefault(mesh_id, []).append({
                "meshFilterPathId": str(component.path_id),
                "gameObjectPathId": str(game_object_id),
                "gameObjectName": game_object_names.get(game_object_id, ""),
            })

        for obj in env.objects:
            if obj.type.name != "Mesh":
                continue
            raw_object = obj.get_raw_data()
            tree = obj.read_typetree()
            handler = MeshHandler(obj.read())
            handler.process()
            vertices = [_vector(value) for value in (handler.m_Vertices or [])]
            normals = [_vector(value) for value in (handler.m_Normals or [])]
            uv0 = [[float(part) for part in value] for value in (handler.m_UV0 or [])]
            submeshes = [
                [list(map(int, triangle)) for triangle in submesh]
                for submesh in handler.get_triangles()
            ]
            geometry = {
                "vertices": vertices,
                "normals": normals,
                "uv0": uv0,
                "submeshes": submeshes,
            }
            geometry_bytes = json.dumps(geometry, sort_keys=True, separators=(",", ":")).encode()
            meshes.append({
                "sourceFile": relative,
                "pathId": str(obj.path_id),
                "name": str(tree.get("m_Name", "")),
                "rawBytes": len(raw_object),
                "rawSha256": sha256(raw_object),
                "vertexCount": len(vertices),
                "normalCount": len(normals),
                "uv0Count": len(uv0),
                "triangleCount": sum(len(submesh) for submesh in submeshes),
                "geometrySha256": sha256(geometry_bytes),
                "meshFilterReferences": sorted(references_by_mesh.get(obj.path_id, []), key=lambda item: (item["gameObjectName"], item["meshFilterPathId"])),
            })

    return {
        "schemaVersion": 1,
        "archiveSha256": ARCHIVE_SHA256,
        "parser": f"UnityPy=={PARSER_VERSION}",
        "scope": "Original Windows player serialized containers; mesh geometry is hashed, not copied. Player object IDs do not establish source .meta GUID identity.",
        "containerCount": len(files),
        "serializedObjectCount": sum(item["objectCount"] for item in files),
        "meshCount": len(meshes),
        "containers": files,
        "meshes": sorted(meshes, key=lambda item: (item["sourceFile"], int(item["pathId"]))),
        "builtinExtraObjects": builtins,
    }


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--player-data", type=Path, required=True, help="Verified extracted player-data directory")
    parser.add_argument("--extraction-manifest", type=Path, required=True, help="Manifest written by extract_original_player_data.py")
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--check", action="store_true", help="Compare with an existing inventory instead of writing")
    args = parser.parse_args()
    inventory = build_inventory(args.player_data, args.extraction_manifest)
    rendered = json.dumps(inventory, sort_keys=True, indent=2, ensure_ascii=False) + "\n"
    if args.check:
        if not args.output.is_file() or json.loads(args.output.read_text(encoding="utf-8")) != inventory:
            raise SystemExit("Player mesh inventory differs from verified source inputs")
    else:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered, encoding="utf-8")
    print(json.dumps({
        "status": "pass",
        "archiveSha256": ARCHIVE_SHA256,
        "containers": inventory["containerCount"],
        "serializedObjects": inventory["serializedObjectCount"],
        "meshes": inventory["meshCount"],
        "builtinExtraObjects": len(inventory["builtinExtraObjects"]),
        "output": str(args.output),
    }, sort_keys=True))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
