#!/usr/bin/env python3
"""Export exact Unity Occupant render hierarchy and source spawn-area records.
No synthesized NPC placement. All identity IDs are retained as decimal strings.
"""
import hashlib, json, re, sys
from pathlib import Path
import yaml

ROOT=Path("source-reference/Original/NPCRecovery/Assets")
HEADER=re.compile(r"^--- !u!(\d+) &(-?\d+)(?: stripped)?\s*$",re.M)
def read(path):
    data=path.read_text()
    matches=list(HEADER.finditer(data))
    out={}
    for i,m in enumerate(matches):
        chunk=data[m.end():matches[i+1].start() if i+1<len(matches) else len(data)]
        record=yaml.safe_load(chunk)
        if not isinstance(record,dict) or len(record)!=1: raise ValueError(f"Bad Unity record {path}:{m.group(2)}")
        out[m.group(2)]=(int(m.group(1)),next(iter(record.values())))
    return out,hashlib.sha256(path.read_bytes()).hexdigest()

def fid(ref):
    return str(ref.get("fileID","0"))

def main():
    prefab=ROOT/"Resources/characters/Occupant.prefab"
    records,sha=read(prefab)
    names={i:v["m_Name"] for i,(cls,v) in records.items() if cls==1}
    transforms={i:v for i,(cls,v) in records.items() if cls==4}
    go_transforms={fid(v["m_GameObject"]):i for i,v in transforms.items()}
    hierarchy=[]
    for i,v in transforms.items():
        go=fid(v["m_GameObject"])
        hierarchy.append({"transformFileID":i,"gameObjectFileID":go,
          "name":names[go],"parentTransformFileID":fid(v["m_Father"]),
          "localPosition":v["m_LocalPosition"],"localRotation":v["m_LocalRotation"],
          "localScale":v["m_LocalScale"]})
    renderers=[]
    for i,(cls,v) in records.items():
        if cls!=23: continue
        go=fid(v["m_GameObject"])
        renderer={"componentFileID":i,"gameObjectFileID":go,
          "transformFileID":go_transforms[go],
          "materials":v.get("m_Materials",[]),"enabled":bool(v.get("m_Enabled",1))}
        mesh=[x for x in records.values() if x[0]==33 and fid(x[1]["m_GameObject"])==go]
        if len(mesh)!=1:raise ValueError("Missing original MeshFilter "+go)
        renderer["mesh"]=mesh[0][1]["m_Mesh"]
        renderers.append(renderer)
    masks=[]
    GUID="e7e7511b06487b36859cf80d2968198a"
    for i,(cls,v) in records.items():
        if cls==114 and v.get("m_Script",{}).get("guid")==GUID:
            go=fid(v["m_GameObject"])
            masks.append({"componentFileID":i,"gameObjectFileID":go,
              "transformFileID":go_transforms[go],
              "skinCandidates":v.get("skins",[]),
              "dynamicMaterial":v.get("dynamicMaterial"),
              "resolution":v.get("resolution"),
              "useDynamicDecals":bool(v.get("useDynamicDecals",1))})
    if (len(renderers),len(masks),len(hierarchy))!=(20,16,56):
        raise ValueError(f"Source Occupant visual graph mismatch: {len(renderers)} {len(masks)} {len(hierarchy)}")
    sceneSpawners={}
    for name in ("Mission 01","Marksman range"):
        source=ROOT/"Scenes"/(name+".unity")
        objs,sceneSha=read(source)
        spawns=[{"componentFileID":i,"fields":v} for i,(cls,v) in objs.items() if
          cls==114 and v.get("m_Script",{}).get("guid")=="b6775fa88f191b1a6dbb5cea21f8f7f4"]
        if len(spawns)!=1:raise ValueError("Original OccupantSpawner not unique in "+name)
        sceneSpawners[name]={"sourceSha256":sceneSha,"spawner":spawns[0]}
    guidMap={}
    for meta in ROOT.rglob("*.meta"):
        match=re.search(r"^guid: ([0-9a-f]{32})$",meta.read_text(errors="ignore"),re.M)
        if match:guidMap[match.group(1)]=str(meta.with_suffix("").relative_to(ROOT))
    for skin in masks:
        for ref in skin["skinCandidates"]:
            if ref.get("guid") not in guidMap:
                raise ValueError("Missing original Occupant skin GUID "+str(ref))
    output={"format":"crisis-source-occupant-visual-spawn-v1",
      "sourcePrefabSha256":sha,"nodes":hierarchy,"renderers":renderers,
      "dynamicLimbSkins":masks,"sceneSpawners":sceneSpawners,
      "guidPaths":guidMap,
      "placementPolicy":"Execute OccupantSpawner.Spawn/RandomSpawnArea against source settings; scene definition alone is not a spawn instance"}
    target=Path("game/assets/occupant/source-visual-spawn.json")
    target.parent.mkdir(parents=True,exist_ok=True)
    target.write_text(json.dumps(output,sort_keys=True,separators=(",",":"))+"\n")
    print(json.dumps({"nodes":len(hierarchy),"renderers":len(renderers),
      "dynamicLimbSkins":len(masks),"scenes":list(sceneSpawners),"output":str(target)}))
if __name__=="__main__":main()
