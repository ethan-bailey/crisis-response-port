#!/usr/bin/env python3
"""Private metadata-only inventory of native Unity ParticleSystems in original
BallisticMaterial entry/exit effects. No replacement particle implementation."""
import argparse
import collections
import hashlib
import json
import re
from pathlib import Path
from extract_runtime_scene import parse_asset

parser=argparse.ArgumentParser()
parser.add_argument("project",type=Path)
parser.add_argument("--out",type=Path,required=True)
args=parser.parse_args();root=args.project

def guid(name):
    p=root/"Assets/Scripts/Assembly-CSharp"/(name+".cs.meta")
    m=re.search(r"(?m)^guid:\s*([0-9a-f]{32})",p.read_text())
    if not m:raise SystemExit("STOP missing original script "+name)
    return m.group(1)
by_guid={}
for meta in (root/"Assets").rglob("*.meta"):
    m=re.search(r"(?m)^guid:\s*([0-9a-f]{32})",meta.read_text(errors="replace"))
    if m:by_guid[m.group(1)]=meta.with_suffix("")
def parse(path):
    return parse_asset(path,path.relative_to(root).as_posix())["records"]
original_scene=parse(root/"Assets/Scenes/Mission 01.unity")
kinds={guid("BallisticMaterial"),guid("BallisticWindow")}
source_props=set()
for r in original_scene:
    f=r["fields"]
    if r["classID"]==114 and f.get("m_Script",{}).get("guid") in kinds:
        g=f.get("properties",{}).get("guid")
        if g:source_props.add(g)
source_effects=collections.defaultdict(set)
for pg in source_props:
    path=by_guid.get(pg)
    if path is None:raise SystemExit("STOP missing properties GUID: "+pg)
    f=next((r["fields"] for r in parse(path) if r["classID"]==114 and "surfaceDensity" in r["fields"]),None)
    if f is None:raise SystemExit("STOP missing original material props: "+str(path))
    for role in ("entryEffect","exitEffect"):
        eg=f.get(role,{}).get("guid")
        if eg:source_effects[eg].add(role)
# Particle modules may be large. Retain original field values verbatim only in
# the short-lived PRIVATE Actions artifact; stdout shows key maps/statistics.
out=[]
module_counts=collections.Counter()
module_states=collections.defaultdict(collections.Counter)
render_modes=collections.Counter()
render_refs=collections.defaultdict(set)
system_count=renderer_count=0
for eg,roles in sorted(source_effects.items()):
    path=by_guid.get(eg)
    if path is None:raise SystemExit("STOP missing original impact effect: "+eg)
    prefab=parse(path)
    go_names={str(r["fileID"]):r["fields"].get("m_Name") for r in prefab if r["classID"]==1}
    tr_go={str(r["fileID"]):str(r["fields"].get("m_GameObject",{}).get("fileID")) for r in prefab if r["classID"] in (4,224)}
    effect={"guid":eg,"path":path.relative_to(root).as_posix(),"roles":sorted(roles),"systems":[],"renderers":[]}
    for rec in prefab:
        cls=rec["classID"]
        if cls not in (198,199):continue
        f=rec["fields"];name=go_names.get(str(f.get("m_GameObject",{}).get("fileID")))
        data={"classID":cls,"fileID":rec["fileID"],"gameObjectName":name,
              "sourceFieldsSha256":hashlib.sha256(json.dumps(f,sort_keys=True,default=str).encode()).hexdigest(),
              "sourceFieldKeys":sorted(f),
              "sourceFields":f}
        if cls==198:
            module_counts.update(f.keys())
            # Unity exports all modules even when their enabled switch is off.
            # Never interpret mere presence as evidence of an active emitter.
            data["sourceModuleEnabled"]={}
            for mod_name, mod in f.items():
                if not mod_name.endswith("Module") or not isinstance(mod,dict):
                    continue
                enable=mod.get("enabled")
                state=("enabled" if enable in (1,True) else
                       "disabled" if enable in (0,False) else "unspecified")
                data["sourceModuleEnabled"][mod_name]=state
                module_states[mod_name][state]+=1
            effect["systems"].append(data);system_count+=1
        else:
            # These are source renderer fields, not guessed WebGL defaults.
            mode=f.get("m_RenderMode")
            render_modes[str(mode)]+=1
            data["originalRenderMode"]=mode
            data["originalRenderReferences"]={
              k: v for k,v in f.items()
              if ("mesh" in k.lower() or "material" in k.lower() or
                  "sorting" in k.lower() or "texture" in k.lower())
            }
            for k,v in data["originalRenderReferences"].items():
                render_refs[k].add(json.dumps(v,sort_keys=True,default=str))
            effect["renderers"].append(data);renderer_count+=1
    out.append(effect)
summary={"sourceProperties":len(source_props),"effectPrefabs":len(out),
         "particleSystems":system_count,"particleRenderers":renderer_count,
         "moduleKeysCount":dict(module_counts.most_common()),
         "originalModuleEnabledCounts":{k:dict(v) for k,v in sorted(module_states.items())},
         "originalRendererModeCounts":dict(render_modes),
         "originalRendererReferenceFieldNames":{k:len(v) for k,v in sorted(render_refs.items())},
         "note":"Original serialized component fields stored only in private artifact; no simulation or rendering parity claim"}
if system_count<1:raise SystemExit("STOP: no original particle systems found")
args.out.parent.mkdir(parents=True,exist_ok=True)
args.out.write_text(json.dumps({"summary":summary,"effects":out},sort_keys=True,separators=(",",":"))+"\n")
print(json.dumps({k:v for k,v in summary.items() if k!="moduleKeysCount"},sort_keys=True))
print("TOP_MODULE_KEYS",json.dumps(module_counts.most_common(60)))
print("ENABLED_MODULES",json.dumps({k:dict(v) for k,v in sorted(module_states.items())},sort_keys=True))
print("SOURCE_RENDER_MODES",json.dumps(dict(render_modes),sort_keys=True))
print("SOURCE_RENDER_FIELDS",json.dumps({k:len(v) for k,v in sorted(render_refs.items())},sort_keys=True))
print("SOURCE_SAMPLES",json.dumps([
 {"guid":v["guid"],"roles":v["roles"],"systems":len(v["systems"]),
  "renderers":len(v["renderers"]),
  "firstKeys":v["systems"][0]["sourceFieldKeys"][:60] if v["systems"] else []}
 for v in out[:7]],sort_keys=True))
