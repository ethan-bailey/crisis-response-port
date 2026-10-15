#!/usr/bin/env python3
"""Read-only audit of serialized Mission 01 BallisticWindow rendering dependencies.

Emits component IDs, original mesh/material references and transforms only.
Never guesses mesh dimensions or creates replacement gameplay/render assets.
"""
import argparse
import collections
import json
import re
from pathlib import Path
from extract_runtime_scene import parse_asset

parser = argparse.ArgumentParser()
parser.add_argument("project", type=Path)
parser.add_argument("--out", type=Path, required=True)
args = parser.parse_args()
root = args.project
scene = "Assets/Scenes/Mission 01.unity"
script_meta = root / "Assets/Scripts/Assembly-CSharp/BallisticWindow.cs.meta"
match = re.search(r"(?m)^guid: ([0-9a-f]{32})$", script_meta.read_text())
if not match:
    raise SystemExit("Original BallisticWindow GUID missing")
window_guid = match.group(1)
scene_asset = parse_asset(root / scene, scene)
records = scene_asset["records"]
by_id = {r["fileID"]: r for r in records}
by_go = collections.defaultdict(list)
for r in records:
    go_id = r["fields"].get("m_GameObject", {}).get("fileID")
    if go_id:
        by_go[str(go_id)].append(r)
transforms = {}
children = collections.defaultdict(list)
for r in records:
    if r["classID"] not in (4, 224):
        continue
    f = r["fields"]
    go = str(f["m_GameObject"]["fileID"])
    transforms[go] = r
    parent_id = str(f.get("m_Father", {}).get("fileID", "0"))
    if parent_id in by_id:
        parent_go = str(by_id[parent_id]["fields"].get("m_GameObject", {}).get("fileID", ""))
        if parent_go:
            children[parent_go].append(go)

def ref(f, k):
    a = f.get(k, {})
    if not isinstance(a, dict):
        return None
    return {"fileID":str(a.get("fileID", "0")), "guid":a.get("guid")}

def render_component(r):
    f = r["fields"]
    data = {"classID":r["classID"],"sourceFileID":r["fileID"]}
    if r["classID"] in (33, 64, 137, 198, 199):
        data["mesh"] = ref(f, "m_Mesh")
    if r["classID"] in (23, 137, 199):
        data["materials"] = [ref({"item":v}, "item") for v in f.get("m_Materials", [])]
    if r["classID"] == 212:
        data["sprite"] = ref(f, "m_Sprite")
    if r["classID"] == 65:
        data["size"] = f.get("m_Size")
        data["center"] = f.get("m_Center")
    return data

def components(go):
    return [render_component(r) for r in by_go.get(go, [])
            if r["classID"] in (23, 33, 64, 65, 137, 198, 199, 212)]

windows = []
component_counts = collections.Counter()
mesh_refs = collections.Counter()
material_refs = collections.Counter()
for r in records:
    if r["classID"] != 114 or r["fields"].get("m_Script", {}).get("guid") != window_guid:
        continue
    f = r["fields"]
    go_id = str(f["m_GameObject"]["fileID"])
    tr = transforms.get(go_id)
    if not tr:
        raise SystemExit("Window lacks original Transform: "+go_id)
    direct = components(go_id)
    child_entries = [{"sourceGameObject":go,"components":components(go)}
                     for go in children.get(go_id, [])]
    child_entries = [x for x in child_entries if x["components"]]
    for c in direct + [c for x in child_entries for c in x["components"]]:
        component_counts[str(c["classID"])] += 1
        mesh = c.get("mesh")
        if mesh:
            mesh_refs[str(mesh["fileID"])+":"+str(mesh["guid"])] += 1
        for m in c.get("materials", []):
            if m:
                material_refs[str(m["fileID"])+":"+str(m["guid"])] += 1
    tf = tr["fields"]
    name = next((p["fields"].get("m_Name") for p in by_go[go_id] if p["classID"] == 1), None)
    windows.append({
        "sourceComponent":r["fileID"],"sourceGameObject":go_id,"name":name,
        "enabled":f.get("m_Enabled"),"active":next((p["fields"].get("m_IsActive") for p in by_go[go_id] if p["classID"] == 1),None),
        "texture":ref(f,"texture"),"holeTexture":ref(f,"holeTexture"),
        "transform":{"localPosition":tf.get("m_LocalPosition"),"localRotation":tf.get("m_LocalRotation"),
                     "localScale":tf.get("m_LocalScale"),"parentTransform":ref(tf,"m_Father")},
        "direct":direct,"immediateChildrenWithGraphicsOrColliders":child_entries,
    })
if not windows:
    raise SystemExit("No original BallisticWindow components recovered")
material_paths = {}
for meta in (root / "Assets").rglob("*.meta"):
    match = re.search(r"(?m)^guid: ([0-9a-f]{32})$", meta.read_text(errors="replace"))
    if match:
        material_paths[match.group(1)] = meta.with_suffix("")
original_materials = {}
for key in sorted(material_refs):
    file_id, guid = key.split(":", 1)
    path = material_paths.get(guid)
    if not path or not path.is_file():
        raise SystemExit("Window material asset unresolved: "+key)
    records_in_asset = parse_asset(path, path.relative_to(root).as_posix())["records"]
    material = next((r["fields"] for r in records_in_asset if r["classID"] == 21), None)
    if material is None:
        raise SystemExit("Source window material record missing: "+str(path))
    saved = material.get("m_SavedProperties", {})
    original_materials[key] = {
        "sourcePath":path.relative_to(root).as_posix(),
        "shader":ref(material, "m_Shader"),
        "renderQueue":material.get("m_CustomRenderQueue"),
        "colors":saved.get("m_Colors", {}),
        "floats":saved.get("m_Floats", {}),
        "mainTexture":saved.get("m_TexEnvs", {}).get("_MainTex")
    }
summary = {
    "scene":scene,"sceneSha256":scene_asset["sha256"],"originalScriptGUID":window_guid,"windowCount":len(windows),
    "componentClassCounts":dict(sorted(component_counts.items())),
    "uniqueMeshReferences":dict(mesh_refs),
    "uniqueMaterialReferences":dict(material_refs),
    "sourceWindowMaterials":original_materials,
    "windowsWithDirectRenderer":sum(any(c["classID"] in (23,137,199,212) for c in w["direct"]) for w in windows),
    "windowsWithDirectMeshFilter":sum(any(c["classID"] == 33 for c in w["direct"]) for w in windows),
    "windowsWithImmediateChildRenderer":sum(any(c["classID"] in (23,137,199,212)
       for ch in w["immediateChildrenWithGraphicsOrColliders"] for c in ch["components"]) for w in windows),
    "note":"Read-only source geometry dependency audit; does not establish visually correct browser panes."
}
args.out.parent.mkdir(parents=True,exist_ok=True)
args.out.write_text(json.dumps({"summary":summary,"windows":windows}, indent=2)+"\n")
print(json.dumps(summary, indent=2))
print("SAMPLE_WINDOW_RECORDS="+json.dumps(windows[:3], separators=(",",":")))
