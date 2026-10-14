#!/usr/bin/env python3
"""Export textured quad hierarchy for original M4A1 and Glock-18 prefab visuals."""
from __future__ import annotations
import json,re,shutil,sys
from pathlib import Path

HEADER=re.compile(r"^--- !u!(\d+) &(\d+)",re.M)
def docs(text):
    ms=list(HEADER.finditer(text))
    for i,m in enumerate(ms): yield int(m.group(1)),m.group(2),text[m.end():ms[i+1].start() if i+1<len(ms) else len(text)]
def vec(body,key,default):
    m=re.search(r"^\s*"+re.escape(key)+r": \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}",body,re.M)
    return [float(m.group(i)) for i in range(1,4)] if m else default
def quat(body):
    m=re.search(r"^\s*m_LocalRotation: \{x: ([^,]+), y: ([^,]+), z: ([^,]+), w: ([^}]+)\}",body,re.M)
    return [float(m.group(i)) for i in range(1,5)] if m else [0,0,0,1]
def main():
    if len(sys.argv)!=3: raise SystemExit("usage: export_weapon_manifest.py PROJECT_ROOT OUTPUT_ROOT")
    root=Path(sys.argv[1]).resolve(); assets=root/"Assets"; outroot=Path(sys.argv[2])
    guid_path={}
    for meta in assets.rglob("*.meta"):
        m=re.search(r"^guid: ([0-9a-f]{32})",meta.read_text(errors="ignore"),re.M)
        if m: guid_path[m.group(1)]=meta.with_suffix("")
    for weapon in ("M4A1","Glock-18"):
        prefab=assets/"Resources"/"weapons"/(weapon+".prefab")
        if not prefab.is_file(): print("skip missing",prefab); continue
        text=prefab.read_text(errors="ignore"); objects={}; transforms={}; renderers=[]
        for cls,fid,body in docs(text):
            if cls==1:
                nm=re.search(r"^  m_Name: (.*)$",body,re.M); ids=[]
                for line in body.splitlines():
                    if "component:" in line:
                        im=re.search(r"fileID: (-?\d+)",line)
                        if im: ids.append(im.group(1))
                objects[fid]={"id":fid,"name":nm.group(1).strip() if nm else "","components":ids}
            elif cls in (4,224):
                gm=re.search(r"m_GameObject: \{fileID: (-?\d+)\}",body); fm=re.search(r"m_Father: \{fileID: (-?\d+)\}",body)
                if gm:
                    p=vec(body,"m_LocalPosition",[0,0,0]); q=quat(body)
                    transforms[fid]={"id":fid,"gameObject":gm.group(1),"parent":fm.group(1) if fm else "0",
                                     "position":[p[0],p[1],-p[2]],"rotation":[-q[0],-q[1],q[2],q[3]],
                                     "scale":vec(body,"m_LocalScale",[1,1,1])}
            elif cls==23:
                gm=re.search(r"m_GameObject: \{fileID: (-?\d+)\}",body)
                block=re.search(r"m_Materials:\s*\n((?:\s+-.*\n)+)",body,re.M)
                mats=re.findall(r"guid: ([0-9a-f]{32})",block.group(1)) if block else []
                if gm:
                    textures=[]
                    for mg in mats:
                        mat_path=guid_path.get(mg)
                        tex_guid=None
                        if mat_path and mat_path.is_file():
                            mt=mat_path.read_text(errors="ignore")
                            tm=re.search(r"_MainTex:\s*\{fileID: -?\d+, guid: ([0-9a-f]{32})",mt)
                            if tm: tex_guid=tm.group(1)
                        tex_path=guid_path.get(tex_guid) if tex_guid else None
                        if tex_path and not tex_path.is_file():
                            tex_path=next((p for p in (tex_path.with_suffix(".png"),tex_path.with_suffix(".jpg")) if p.is_file()),None)
                        textures.append({"materialGuid":mg,"material":str(mat_path.relative_to(root)) if mat_path else None,
                                         "textureGuid":tex_guid,"texture":str(tex_path.relative_to(root)) if tex_path and tex_path.is_file() else None})
                    renderers.append({"componentFileID":fid,"gameObject":gm.group(1),"materials":textures})
        transform_by_go={t["gameObject"]:tid for tid,t in transforms.items()}
        nodes=[]
        path_cache={}
        def path(tid,seen=None):
            if tid in path_cache:return path_cache[tid]
            seen=seen or set()
            if tid in seen:return ""
            seen.add(tid);t=transforms[tid];name=objects.get(t["gameObject"],{}).get("name","")
            par=t["parent"]
            if par in transforms:
                pp=path(par,seen);result=name if not pp else pp+"/"+name
            else: result=""
            path_cache[tid]=result;return result
        for tid,t in transforms.items():
            obj=objects.get(t["gameObject"],{"name":"unknown"});par=t["parent"]
            pgo=transforms[par]["gameObject"] if par in transforms else None
            nodes.append({"transformFileID":tid,"gameObjectFileID":t["gameObject"],"name":obj["name"],
                          "parentGameObjectFileID":pgo,"path":path(tid),"position":t["position"],
                          "rotation":t["rotation"],"scale":t["scale"]})
        out=outroot/weapon; (out/"textures").mkdir(parents=True,exist_ok=True)
        exported=[]; texture_map={}
        for renderer in renderers:
            go=objects.get(renderer["gameObject"],{"name":"unknown"});texref=next((m for m in renderer["materials"] if m.get("texture")),None)
            tex_url=None
            if texref:
                src=root/texref["texture"]
                key=str(src)
                if key not in texture_map:
                    dest=f"textures/material_{len(texture_map):02d}{src.suffix.lower()}"
                    shutil.copy2(src,out/dest);texture_map[key]=dest
                tex_url=texture_map[key]
            if tex_url:
                exported.append({"gameObjectFileID":renderer["gameObject"],"name":go["name"],"texture":tex_url,
                                 "material":texref.get("material") if texref else None})
        report={"sourcePrefab":str(prefab.relative_to(root)),"nodes":nodes,"renderers":exported,
                "nodeCount":len(nodes),"rendererCount":len(exported),"textureCount":len(texture_map)}
        (out/"weapon.json").write_text(json.dumps(report,indent=2))
        print(weapon,"nodes",len(nodes),"renderers",len(exported),"textures",len(texture_map))
        for x in exported: print(" ",x["name"],x["texture"])
if __name__=="__main__":main()
