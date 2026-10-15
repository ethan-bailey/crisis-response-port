#!/usr/bin/env python3
"""Read-only source mapping for original impact-hole and particle prefab renderers.

Produces metadata (GUID/file path/class ID) only, never proprietary meshes or
textures. Does not create replacement gameplay/effects or alter original assets.
"""
import argparse, collections, json, re
from pathlib import Path
from extract_runtime_scene import parse_asset

ap=argparse.ArgumentParser()
ap.add_argument("project",type=Path)
ap.add_argument("--out",type=Path,required=True)
args=ap.parse_args()
root=args.project
assets=root/"Assets"
script=root/"Assets/Scripts/Assembly-CSharp"
def script_guid(name):
    match=re.search(r"(?m)^guid: ([0-9a-f]{32})$", (script/(name+".cs.meta")).read_text())
    if not match:raise SystemExit("Missing original script GUID: "+name)
    return match.group(1)
def ref_guid(fields,key):
    v=fields.get(key,{})
    return v.get("guid") if isinstance(v,dict) else None

by_guid={}
for meta in assets.rglob("*.meta"):
    m=re.search(r"(?m)^guid: ([0-9a-f]{32})$",meta.read_text(errors="replace"))
    if m:by_guid[m.group(1)]=meta.with_suffix("").relative_to(root).as_posix()
def resolved(g):
    return {"guid":g,"path":by_guid.get(g),"resolved":g in by_guid} if g else None
def parse(rel):
    path=root/rel
    if not path.is_file():raise SystemExit("Missing original source: "+rel)
    a=parse_asset(path,rel)
    if a.get("binary"):raise SystemExit("Binary prefab not safely parsed: "+rel)
    return a["records"]

material_guid=script_guid("BallisticMaterial")
window_guid=script_guid("BallisticWindow")
scene=parse("Assets/Scenes/Mission 01.unity")
props={}
for rec in scene:
    fields=rec["fields"]
    if rec["classID"]!=114 or ref_guid(fields,"m_Script") not in (material_guid,window_guid):continue
    guid=ref_guid(fields,"properties")
    if guid in by_guid:props[guid]=by_guid[guid]
effect_refs=collections.defaultdict(set)
for pg,rel in props.items():
    candidates=[x["fields"] for x in parse(rel) if x["classID"]==114 and "surfaceDensity" in x["fields"]]
    if len(candidates)!=1:raise SystemExit(f"Expected single material props in {rel}: {len(candidates)}")
    f=candidates[0]
    for kind in ("entryEffect","exitEffect","exitHole"):
        guid=ref_guid(f,kind)
        if guid:effect_refs[guid].add(kind)
events=[]
for guid in sorted(effect_refs):
    rel=by_guid.get(guid)
    if not rel:events.append({"guid":guid,"roles":sorted(effect_refs[guid]),"missing_prefab":True});continue
    records=parse(rel)
    counted=collections.Counter(str(x["classID"]) for x in records)
    mesh_ref=[]
    material_ref=[]
    particle_components=[]
    for rec in records:
        f=rec["fields"]
        if rec["classID"]==33: # MeshFilter
            target=ref_guid(f,"m_Mesh")
            mesh_ref.append(resolved(target) if target else {"builtin_fileID":f.get("m_Mesh",{}).get("fileID")})
        if rec["classID"] in (23,137): # MeshRenderer, SkinnedMeshRenderer
            for m in f.get("m_Materials",[]):material_ref.append(resolved(m.get("guid")) if m.get("guid") else {"builtin_fileID":m.get("fileID")})
            if rec["classID"]==137:mesh_ref.append(resolved(ref_guid(f,"m_Mesh")))
        if rec["classID"] in (198,199): # ParticleSystem, ParticleSystemRenderer
            particle_components.append({"classID":rec["classID"],
                                       "mesh":resolved(ref_guid(f,"m_Mesh")),
                                       "materials":[resolved(x.get("guid")) for x in f.get("m_Materials",[]) if x.get("guid")]})
    shader_refs=[]
    if "exitHole" in effect_refs[guid]:
        for material in material_ref:
            matpath=material.get("path") if material else None
            if not matpath:continue
            source_mat=next((r["fields"] for r in parse(matpath) if r["classID"]==21),None)
            if source_mat is None:raise SystemExit("STOP original exit-hole material lacks Unity material record")
            shader=source_mat.get("m_Shader",{})
            shader_refs.append({"material":matpath,"shader":resolved(shader.get("guid")),
                                "source_render_queue":source_mat.get("m_CustomRenderQueue"),
                                "color":source_mat.get("m_SavedProperties",{}).get("m_Colors",{}).get("_Color")})
    events.append({"guid":guid,"roles":sorted(effect_refs[guid]),"prefab":rel,
                   "classCounts":dict(counted),"meshes":mesh_ref,"materials":material_ref,"sourceShaders":shader_refs,
                   "particleComponents":particle_components,
                   "has_render_geometry":bool(mesh_ref or particle_components)})
summary={"original_scene":"Assets/Scenes/Mission 01.unity",
         "source_property_count":len(props),"effect_prefab_count":len(events),
         "entry_prefabs":sum("entryEffect" in x["roles"] for x in events),
         "exit_prefabs":sum("exitEffect" in x["roles"] for x in events),
         "hole_prefabs":sum("exitHole" in x["roles"] for x in events),
         "mesh_filter_total":sum(int(x["classCounts"].get("33",0)) for x in events),
         "mesh_renderer_total":sum(int(x["classCounts"].get("23",0)) for x in events),
         "particle_system_total":sum(int(x["classCounts"].get("198",0)) for x in events),
         "unresolved_prefabs":sum(x.get("missing_prefab",False) for x in events),
         "unresolved_meshes":sum(not m.get("resolved",False) for x in events for m in x["meshes"] if m and "guid" in m),
         "note":"Read-only effect geometry source inventory; no emitted effect renderer or parity claim."}
args.out.parent.mkdir(parents=True,exist_ok=True)
args.out.write_text(json.dumps({"summary":summary,"effects":events},indent=2)+"\n")
print(json.dumps(summary,indent=2))
