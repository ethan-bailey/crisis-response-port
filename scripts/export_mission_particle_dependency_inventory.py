#!/usr/bin/env python3
"""Export a compact, source-hashed Mission particle dependency closure.

Input is the verified incremental full-source package produced by
package_wasm_site.py. The inventory retains all serialized prefab records for
assets containing ParticleSystem components and the full serialized records for
their referenced material dependencies. Meshes and textures are represented by
source GUID/path/hash plus serialized metadata; their binary payloads remain in
the source package and are not duplicated here.
"""
import argparse
import gzip
import hashlib
import json
from pathlib import Path


def sha256(raw):
    return hashlib.sha256(raw).hexdigest()


def load_json_gzip(path):
    with gzip.open(path, "rt", encoding="utf-8") as stream:
        return json.load(stream)


def export_inventory(manifest_path):
    manifest_path = Path(manifest_path)
    root = manifest_path.parent
    manifest = json.loads(manifest_path.read_text())
    metadata_path = root / (manifest.get("metadata", {}).get("file") or "metadata.json.gz")
    metadata = load_json_gzip(metadata_path)
    asset_list = manifest.get("assets", [])
    assets = {item["path"]: item for item in asset_list}
    guid_paths = metadata.get("guidPaths", {})
    guid_by_path = {}
    for guid, path in guid_paths.items():
        guid_by_path.setdefault(path, []).append(guid)
    packaged_paths = set(assets)
    incoming_references = {}
    edges = {}
    for reference in metadata.get("objectReferences", []):
        source = reference.get("source")
        guid = reference.get("guid")
        target = guid_paths.get(guid)
        if source not in packaged_paths or not target:
            continue
        if target in packaged_paths:
            edges.setdefault(source, set()).add(target)
        incoming_references.setdefault(target, []).append({
            "source": source,
            "sourceFileID": reference.get("sourceFileID"),
            "fieldPath": reference.get("fieldPath"),
            "guid": guid,
            "resolution": reference.get("resolution"),
        })
    mission_closure = set()
    pending = ["Assets/Scenes/Mission 01.unity"]
    while pending:
        path = pending.pop()
        if path in mission_closure or path not in packaged_paths:
            continue
        mission_closure.add(path)
        pending.extend(edges.get(path, set()) - mission_closure)

    decoded = {}
    particle_prefabs = {}
    material_guids = set()
    mesh_guids = set()

    for path, item in assets.items():
        payload = load_json_gzip(root / item["file"])
        records = payload.get("records", [])
        if not any(record.get("classID") == 198 for record in records):
            continue
        decoded[path] = payload
        systems = [record for record in records if record.get("classID") == 198]
        renderers = [record for record in records if record.get("classID") == 199]
        by_game_object = {}
        for record in renderers:
            game_object = str(record.get("fields", {}).get("m_GameObject", {}).get("fileID", "0"))
            if game_object in by_game_object:
                raise ValueError(f"duplicate ParticleSystemRenderer on {path} GameObject {game_object}")
            by_game_object[game_object] = record
        for system in systems:
            fields = system.get("fields", {})
            game_object = str(fields.get("m_GameObject", {}).get("fileID", "0"))
            if game_object not in by_game_object:
                raise ValueError(f"ParticleSystem missing source renderer: {path} GameObject {game_object}")
            renderer_fields = by_game_object[game_object].get("fields", {})
            for reference in renderer_fields.get("m_Materials", []) or []:
                if reference.get("guid"):
                    material_guids.add(reference["guid"])
            for key in ("m_Mesh", "m_Mesh1", "m_Mesh2", "m_Mesh3"):
                reference = renderer_fields.get(key, {})
                if reference.get("guid"):
                    mesh_guids.add(reference["guid"])
            modules = {key: value for key, value in fields.items()
                       if key.endswith("Module") and isinstance(value, dict)}
            shape = modules.get("ShapeModule", {})
            for key in ("m_Mesh", "m_MeshRenderer", "m_SkinnedMeshRenderer"):
                reference = shape.get(key, {})
                if isinstance(reference, dict) and reference.get("guid"):
                    mesh_guids.add(reference["guid"])
        particle_prefabs[path] = {
            "guid": sorted(guid_by_path.get(path, [])),
            "sha256": item["sha256"],
            "bytes": item["bytes"],
            "missionSceneGuidClosure": path in mission_closure,
            "incomingReferences": sorted(incoming_references.get(path, []),
                                          key=lambda item: (item["source"], str(item["sourceFileID"]), item["fieldPath"] or "")),
            "records": records,
            "systemCount": len(systems),
            "rendererCount": len(renderers),
        }

    dependency_records = {"materials": {}, "meshes": {}, "textures": {}, "shaders": {}}
    unresolved = []
    for guid in sorted(material_guids | mesh_guids):
        path = guid_paths.get(guid)
        if not path:
            unresolved.append({"guid": guid,
                               "reason": "Unity built-in mesh reference; behavior/geometry is engine supplied"
                               if guid == "0000000000000000e000000000000000"
                               else "GUID absent from source metadata"})
            continue
        item = assets.get(path)
        if not item:
            unresolved.append({"guid": guid, "path": path,
                               "reason": "GUID path is outside the bounded source package"})
            continue
        payload = load_json_gzip(root / item["file"])
        records = payload.get("records", [])
        entry = {"guid": guid, "path": path, "sha256": item["sha256"], "bytes": item["bytes"]}
        is_material = any(record.get("classID") == 21 for record in records)
        is_mesh = any(record.get("classID") == 43 for record in records)
        if is_material:
            entry["records"] = records
            dependency_records["materials"][guid] = entry
            material_record = next(record["fields"] for record in records if record.get("classID") == 21)
            shader_guid = material_record.get("m_Shader", {}).get("guid")
            if shader_guid:
                if shader_guid in guid_paths:
                    shader_path = guid_paths[shader_guid]
                    shader_item = assets.get(shader_path)
                    if shader_item:
                        dependency_records["shaders"][shader_guid] = {
                            "guid": shader_guid, "path": shader_path,
                            "sha256": shader_item["sha256"], "bytes": shader_item["bytes"]}
                    else:
                        unresolved.append({"guid": shader_guid, "path": shader_path,
                                           "reason": "shader source is outside the bounded package"})
                else:
                    dependency_records["shaders"][shader_guid] = {
                        "guid": shader_guid, "builtin": True,
                        "behaviorVerified": False,
                        "reason": "Unity built-in shader; engine behavior remains an acceptance dependency"}
            saved = material_record.get("m_SavedProperties", {})
            for value in (saved.get("m_TexEnvs", {}) or {}).values():
                texture_guid = value.get("m_Texture", {}).get("guid") if isinstance(value, dict) else None
                if not texture_guid:
                    continue
                texture_path = guid_paths.get(texture_guid)
                texture_item = assets.get(texture_path) if texture_path else None
                if texture_item:
                    dependency_records["textures"][texture_guid] = {
                        "guid": texture_guid, "path": texture_path,
                        "sha256": texture_item["sha256"], "bytes": texture_item["bytes"]}
                else:
                    unresolved.append({"guid": texture_guid, "path": texture_path,
                                       "reason": "material texture source is outside the bounded package"})
        elif is_mesh:
            mesh_record = next(record["fields"] for record in records if record.get("classID") == 43)
            entry["sourceMetadata"] = {
                "serializedVersion": mesh_record.get("m_SerializedVersion"),
                "nativeVertexCount": mesh_record.get("m_VertexData", {}).get("m_VertexCount"),
                "submeshCount": len(mesh_record.get("m_SubMeshes", []) or []),
                "compressedVertexItems": mesh_record.get("m_CompressedMesh", {}).get("m_Vertices", {}).get("m_NumItems"),
                "compressedTriangleItems": mesh_record.get("m_CompressedMesh", {}).get("m_Triangles", {}).get("m_NumItems"),
            }
            dependency_records["meshes"][guid] = entry
        else:
            unresolved.append({"guid": guid, "path": path,
                               "reason": "source GUID does not resolve to a Material or Mesh record"})

    module_counts = {}
    active_modules = {}
    render_modes = {}
    source_shapes = {}
    burst_entries = 0
    sub_emitter_references = 0
    for prefab in particle_prefabs.values():
        renderer_by_game_object = {
            str(record["fields"].get("m_GameObject", {}).get("fileID", "0")): record["fields"]
            for record in prefab["records"] if record.get("classID") == 199
        }
        for record in prefab["records"]:
            if record.get("classID") != 198:
                continue
            fields = record["fields"]
            modules = {key: value for key, value in fields.items()
                       if key.endswith("Module") and isinstance(value, dict)}
            for key, value in modules.items():
                module_counts[key] = module_counts.get(key, 0) + 1
                if value.get("enabled") in (1, True):
                    active_modules[key] = active_modules.get(key, 0) + 1
                if key == "EmissionModule":
                    burst_entries += len(value.get("m_Bursts") or [])
                if key == "SubModule" and value.get("enabled") in (1, True):
                    sub_emitter_references += len(value.get("subEmitters") or [])
            shape_code = str(modules.get("ShapeModule", {}).get("type"))
            source_shapes[shape_code] = source_shapes.get(shape_code, 0) + 1
            renderer = renderer_by_game_object.get(str(fields.get("m_GameObject", {}).get("fileID", "0")), {})
            mode = str(renderer.get("m_RenderMode", "unpaired"))
            render_modes[mode] = render_modes.get(mode, 0) + 1

    system_count = sum(item["systemCount"] for item in particle_prefabs.values())
    renderer_count = sum(item["rendererCount"] for item in particle_prefabs.values())
    expected = {"systems": 51, "renderers": 51, "prefabs": 27,
                "materials": 15, "customMeshes": 5,
                "missionClosureSystems": 32, "missionClosurePrefabs": 18}
    actual = {"systems": system_count, "renderers": renderer_count,
              "prefabs": len(particle_prefabs),
              "materials": len(dependency_records["materials"]),
              "customMeshes": len(dependency_records["meshes"]),
              "missionClosureSystems": sum(item["systemCount"] for path, item in particle_prefabs.items()
                                            if item["missionSceneGuidClosure"]),
              "missionClosurePrefabs": sum(item["missionSceneGuidClosure"] for item in particle_prefabs.values())}
    # The sixth renderer mesh reference in the full package is Unity's built-in
    # Quad GUID, which intentionally has no source asset path.
    if actual != expected:
        raise ValueError(f"bounded Mission particle coverage changed: expected {expected}, found {actual}")
    return {
        "schemaVersion": 1,
        "source": {
            "profile": manifest.get("profile"),
            "manifestSha256": sha256(manifest_path.read_bytes()),
            "sourcePackageSha256": manifest.get("sourcePackageSha256"),
            "seedAssets": metadata.get("seeds", []),
            "assetCount": manifest.get("assetCount"),
            "sourceOfTruth": metadata.get("sourceOfTruth"),
        },
        "coverage": {
            **actual,
            "renderModes": render_modes,
            "activeModules": active_modules,
            "moduleSerializationCounts": module_counts,
            "shapeTypeCodes": source_shapes,
            "emissionBurstEntries": burst_entries,
            "nativeSubEmitterReferences": sub_emitter_references,
        },
        "prefabs": particle_prefabs,
        "dependencies": dependency_records,
        "unresolved": unresolved,
        "limitations": [
            "Serialized source is inventoried; no browser ParticleSystem driver or rendered-particle parity is claimed.",
            "Unity built-in shader and mesh behavior require engine-compatible browser verification.",
            "Source particle curves are preserved verbatim; a browser curve evaluator is not implemented by this inventory.",
        ],
    }


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--manifest", type=Path, required=True,
                        help="manifest.json from the bounded incremental source package")
    parser.add_argument("--out", type=Path, required=True,
                        help="gzip JSON output path")
    args = parser.parse_args()
    result = export_inventory(args.manifest)
    raw = json.dumps(result, sort_keys=True, separators=(",", ":")).encode()
    args.out.parent.mkdir(parents=True, exist_ok=True)
    args.out.write_bytes(gzip.compress(raw, mtime=0))
    print(json.dumps({"coverage": result["coverage"],
                      "manifestSha256": result["source"]["manifestSha256"],
                      "inventoryBytes": len(raw),
                      "compressedBytes": args.out.stat().st_size,
                      "inventorySha256": sha256(raw),
                      "unresolved": result["unresolved"]}, sort_keys=True))


if __name__ == "__main__":
    main()
