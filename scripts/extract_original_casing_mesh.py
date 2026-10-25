#!/usr/bin/env python3
"""Export the authored 5.56 casing visual/material inputs with source provenance."""
import hashlib
import json
import re
import shutil
import sys
from pathlib import Path

from extract_runtime_scene import parse_asset
from unity_packed_mesh import compressed_mesh, compressed_normals, unpack

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "source-reference/Original"
MESH = "Assets/Mesh/5.56x45mm casing.asset"
PREFAB = "Assets/Resources/weapons/ammunition/casings/Casing 5.56x45mm.prefab"
MATERIAL = "Assets/Material/Casing brass rough.mat"
PHYSIC_MATERIAL = "Assets/PhysicMaterial/Shell casing.physicMaterial"
TEXTURES = {
    "normal": "Assets/Texture2D/metal dented normal.png",
    "occlusion": "Assets/Texture2D/NoiseEffectGrain.png",
}
def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    out = Path(sys.argv[1]) if len(sys.argv) > 1 else ROOT / "game/assets/casing-556.json"
    mesh_path = PROJECT / MESH
    mesh_text = mesh_path.read_text(encoding="utf-8")
    records = parse_asset(mesh_path, MESH)["records"]
    fields = next((r["fields"] for r in records if r["classID"] == 43), None)
    if not fields:
        raise SystemExit("STOP original 5.56 casing Mesh record is absent")
    # Preserve the exact lexical source hex: generic YAML parsing can turn
    # all-decimal packed bytes into a number and lose leading zeroes.
    for section in ("m_Vertices", "m_UV", "m_Normals", "m_NormalSigns", "m_Triangles"):
        m = re.search(r"^    " + section + r":\n(.*?)(?=^    \w+:|^  m_|\Z)", mesh_text, re.M | re.S)
        if not m:
            raise SystemExit("STOP missing original packed mesh stream " + section)
        data = re.search(r"^      m_Data:\s*([0-9a-fA-F]*)\s*$", m.group(1), re.M)
        if not data:
            raise SystemExit("STOP missing original packed bytes " + section)
        value = fields["m_CompressedMesh"][section]
        value["m_Data"] = data.group(1)
    vertices, faces = compressed_mesh(fields)
    uv_data = fields["m_CompressedMesh"].get("m_UV", {})
    uv_values = unpack(uv_data, True) if uv_data.get("m_NumItems") else []
    if uv_values and len(uv_values) != 2 * len(vertices):
        raise SystemExit("STOP original casing UV count does not match its vertices")
    normals = compressed_normals(fields)
    if len(normals) != len(vertices):
        raise SystemExit("STOP original casing normals do not match its vertices")
    mesh = {
        "positions": [[x, y, -z] for x, y, z in vertices],
        "uv": [[uv_values[i], uv_values[i + 1]] for i in range(0, len(uv_values), 2)],
        "normals": [[x, y, -z] for x, y, z in normals],
        "indices": [i for a, b, c in faces for i in (a, c, b)],
    }
    if not mesh["uv"]:
        raise SystemExit("STOP source casing renderer requires its authored UV channel")
    prefab_text = (PROJECT / PREFAB).read_text(encoding="utf-8")
    for guid in ("d495eb21fe3efb34eb05d4f2f79b43c5", "f2f4f981cd6ffb54d8f381d4a33915a8", "93036436942eabf4fa82c48534da0279"):
        if guid not in prefab_text:
            raise SystemExit("STOP original casing prefab no longer references " + guid)
    material_text = (PROJECT / MATERIAL).read_text(encoding="utf-8")
    phys_text = (PROJECT / PHYSIC_MATERIAL).read_text(encoding="utf-8")
    if not all(token in material_text for token in ("_Metallic: 1", "_Glossiness: 0.8", "r: 0.85294116", "g: 0.6698961", "b: 0.27595156")):
        raise SystemExit("STOP source casing brass material parameters changed")
    if not all(token in phys_text for token in ("dynamicFriction: 0.1", "staticFriction: 0.1", "bounciness: 0.35")):
        raise SystemExit("STOP source casing PhysicMaterial parameters changed")
    manifest = {path: sha(PROJECT / path) for path in (MESH, PREFAB, MATERIAL, PHYSIC_MATERIAL)}
    manifest.update({path: sha(PROJECT / path) for path in TEXTURES.values()})
    payload = {
        "format": "original-unity-casing-mesh-v1",
        "source": {"project": "recovered Original Unity project", "hashes": manifest,
                   "meshGuid": "d495eb21fe3efb34eb05d4f2f79b43c5",
                   "materialGuid": "f2f4f981cd6ffb54d8f381d4a33915a8",
                   "renderer": "root MeshFilter/MeshRenderer from original Casing 5.56x45mm prefab"},
        "mesh": mesh,
        "material": {"colorLinear": [0.85294116, 0.6698961, 0.27595156],
                     "metallic": 1, "smoothness": 0.8, "normalScale": -0.05,
                     "occlusionStrength": 0.5, "normalTexture": "casing-556-normal.png",
                     "occlusionTexture": "casing-556-occlusion.png"},
        "physicsMaterial": {"staticFriction": 0.1, "dynamicFriction": 0.1,
                            "bounciness": 0.35, "combine": "Average"},
        "collider": {"size": [0.044699993, 0.009, 0.009], "center": [0, 0.0002970919, 0],
                     "massKg": 0.002, "drag": 0.1, "angularDrag": 0.1,
                     "collisionDetection": "Continuous", "interpolation": "Interpolate"},
        "count": {"vertices": len(vertices), "triangles": len(mesh["indices"]) // 3},
    }
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(payload, sort_keys=True, separators=(",", ":")) + "\n", encoding="utf-8")
    assets = out.parent
    for key, source in TEXTURES.items():
        shutil.copyfile(PROJECT / source, assets / ("casing-556-normal.png" if key == "normal" else "casing-556-occlusion.png"))
    print(json.dumps({"output": str(out), "vertices": len(vertices), "triangles": len(mesh["indices"]) // 3,
                      "sourceHashes": manifest, "textureBytes": {name: (assets / name).stat().st_size for name in ("casing-556-normal.png", "casing-556-occlusion.png")}}))


if __name__ == "__main__":
    main()
