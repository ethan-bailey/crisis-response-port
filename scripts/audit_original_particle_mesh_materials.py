#!/usr/bin/env python3
"""Read-only provenance map for the original ParticleSystem mesh/material assets.

Uses recovered serialized manifest, never invents geometry or particle effects.
Writes original file references and SHA256 only; fails on unresolved GUIDs.
"""
import argparse
import collections
import gzip
import hashlib
import json
import re
from pathlib import Path
from extract_runtime_scene import parse_asset

parser=argparse.ArgumentParser()
parser.add_argument("project",type=Path)
parser.add_argument("--manifest",type=Path,required=True)
parser.add_argument("--out",type=Path,required=True)
args=parser.parse_args()
root=args.project
with gzip.open(args.manifest,"rt") as f:
    source=json.load(f)
if source.get("version")!=1 or len(source["effects"])!=14:
    raise SystemExit("STOP original particle source manifest version or coverage changed")
meshes=collections.Counter()
materials=collections.Counter()
for effect in source["effects"]:
    for emitter in effect["emitters"]:
        renderer=emitter["renderer"]
        mesh=renderer.get("m_Mesh",{})
        if renderer["m_RenderMode"]==4:
            if not mesh.get("guid"):
                raise SystemExit("STOP original mesh-mode particle lacks source mesh GUID")
            meshes[mesh["guid"]]+=1
        for ref in renderer["m_Materials"]:
            if ref.get("guid"):
                materials[ref["guid"]]+=1
if len(meshes)!=3 or sum(meshes.values())!=12 or len(materials)!=8:
    raise SystemExit("STOP original particle mesh/material reference count changed")
guid_index={}
for meta in (root/"Assets").rglob("*.meta"):
    match=re.search(r"(?m)^guid:\s*([a-f0-9]{32})",meta.read_text(errors="replace"))
    if match:
        guid_index[match.group(1)]=meta.with_suffix("")
def source_path(guid):
    path=guid_index.get(guid)
    if path is None or not path.is_file():
        raise SystemExit("STOP missing original GUID "+guid)
    return path

def fingerprint(path):
    return {"path":path.relative_to(root).as_posix(),
            "sha256":hashlib.sha256(path.read_bytes()).hexdigest(),
            "bytes":path.stat().st_size}
assets={"meshes":{},"materials":{},"textures":{},"shaders":{}}
for guid,count in sorted(meshes.items()):
    path=source_path(guid)
    records=parse_asset(path,path.relative_to(root).as_posix())["records"]
    item=next((x["fields"] for x in records if x["classID"]==43),None)
    if item is None:raise SystemExit("STOP original mesh record absent "+guid)
    vertex_data=item.get("m_VertexData",{})
    packed=item.get("m_CompressedMesh",{})
    assets["meshes"][guid]={
        **fingerprint(path),"references":count,
        "nativeVertexCount":vertex_data.get("m_VertexCount"),
        "packedVertexComponents":packed.get("m_Vertices",{}).get("m_NumItems"),
        "packedTriangleIndices":packed.get("m_Triangles",{}).get("m_NumItems"),
        "nativeSubmeshes":len(item.get("m_SubMeshes",[])),
        "hasSourceUV":bool(vertex_data.get("m_Channels")) or bool(packed.get("m_UV",{}).get("m_NumItems"))
    }
for guid,count in sorted(materials.items()):
    path=source_path(guid)
    records=parse_asset(path,path.relative_to(root).as_posix())["records"]
    item=next((x["fields"] for x in records if x["classID"]==21),None)
    if item is None:raise SystemExit("STOP original particle material absent "+guid)
    saved=item.get("m_SavedProperties",{})
    shader=item.get("m_Shader",{}).get("guid")
    if shader:
        assets["shaders"][shader]=fingerprint(source_path(shader))
    texture_refs={}
    tex_envs=saved.get("m_TexEnvs",{})
    if isinstance(tex_envs,dict):
        for key,val in tex_envs.items():
            if isinstance(val,dict):
                texture=val.get("m_Texture",{}).get("guid")
                if texture:
                    texture_refs[key]=texture
                    assets["textures"][texture]=fingerprint(source_path(texture))
    assets["materials"][guid]={
        **fingerprint(path),"references":count,
        "shaderGuid":shader,
        "textureRefs":texture_refs,
        "renderQueue":item.get("m_CustomRenderQueue"),
        "colors":saved.get("m_Colors",{}),
        "floats":saved.get("m_Floats",{})
    }
summary={
    "meshInstances":sum(meshes.values()),
    "uniqueMeshAssets":len(meshes),
    "uniqueMaterialAssets":len(materials),
    "uniqueShaderAssets":len(assets["shaders"]),
    "uniqueTextureAssets":len(assets["textures"]),
    "nativeMeshGuids":dict(meshes),
    "note":"Source asset provenance only; geometry extraction, shader semantics and particle simulation remain to be verified."
}
args.out.parent.mkdir(parents=True,exist_ok=True)
args.out.write_text(json.dumps({"summary":summary,"assets":assets},indent=2,sort_keys=True)+"\n")
print(json.dumps(summary,sort_keys=True))
print("MESH_SOURCE_PATHS",json.dumps({g:v["path"] for g,v in assets["meshes"].items()},sort_keys=True))
print("MATERIAL_SOURCE_PATHS",json.dumps({g:v["path"] for g,v in assets["materials"].items()},sort_keys=True))
