#!/usr/bin/env python3
"""Extract the recovered Operator prefab's original limb textures and transforms."""
from __future__ import annotations
import json, math, re, shutil, sys
from pathlib import Path

HEADER = re.compile(r"^--- !u!(\d+) &(\d+)", re.M)
GUID = re.compile(r"guid: ([0-9a-f]{32})")
def docs(text):
    ms=list(HEADER.finditer(text))
    for i,m in enumerate(ms):
        yield int(m.group(1)),m.group(2),text[m.end():ms[i+1].start() if i+1<len(ms) else len(text)]
def vec(body,key,default):
    m=re.search(r"^\s*"+re.escape(key)+r": \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}",body,re.M)
    return [float(m.group(i)) for i in range(1,4)] if m else default
def quat(body):
    m=re.search(r"^\s*m_LocalRotation: \{x: ([^,]+), y: ([^,]+), z: ([^,]+), w: ([^}]+)\}",body,re.M)
    return [float(m.group(i)) for i in range(1,5)] if m else [0,0,0,1]
def qmul(a,b):
    ax,ay,az,aw=a; bx,by,bz,bw=b
    return [aw*bx+ax*bw+ay*bz-az*by, aw*by-ax*bz+ay*bw+az*bx,
            aw*bz+ax*by-ay*bx+az*bw, aw*bw-ax*bx-ay*by-az*bz]
def qrot(q,v):
    u=q[:3]; uv=[u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0]]
    uuv=[u[1]*uv[2]-u[2]*uv[1],u[2]*uv[0]-u[0]*uv[2],u[0]*uv[1]-u[1]*uv[0]]
    return [v[i]+2*(q[3]*uv[i]+uuv[i]) for i in range(3)]
def compose(parent,local):
    pp,pq,ps=parent; lp,lq,ls=local
    scaled=[lp[i]*ps[i] for i in range(3)]
    rotated=qrot(pq,scaled)
    pos=[pp[i]+rotated[i] for i in range(3)]
    return pos,qmul(pq,lq),[ps[i]*ls[i] for i in range(3)]
def main():
    if len(sys.argv)!=3: raise SystemExit("usage: export_operator_manifest.py PROJECT_ROOT OUTPUT_DIR")
    root=Path(sys.argv[1]).resolve(); out=Path(sys.argv[2]); assets=root/"Assets"
    prefab=assets/"Resources/characters/Operator.prefab"
    if not prefab.is_file(): raise SystemExit(f"Missing {prefab}")
    guid_path={}
    for meta in assets.rglob("*.meta"):
        m=re.search(r"^guid: ([0-9a-f]{32})",meta.read_text(errors="ignore"),re.M)
        if m: guid_path[m.group(1)]=meta.with_suffix("")
    text=prefab.read_text(errors="ignore")
    objects={}; transforms={}; component_records=[]
    for cls,fid,body in docs(text):
        if cls==1:
            nm=re.search(r"^  m_Name: (.*)$",body,re.M)
            ids=[]
            for line in body.splitlines():
                if "component:" in line:
                    im=re.search(r"fileID: (-?\d+)",line)
                    if im: ids.append(im.group(1))
            objects[fid]={"id":fid,"name":nm.group(1).strip() if nm else "", "components":ids}
        elif cls in (4,224):
            gm=re.search(r"m_GameObject: \{fileID: (-?\d+)\}",body)
            fm=re.search(r"m_Father: \{fileID: (-?\d+)\}",body)
            if gm:
                transforms[fid]={"id":fid,"gameObject":gm.group(1),"parent":fm.group(1) if fm else "0",
                                 "position":vec(body,"m_LocalPosition",[0,0,0]),"rotation":quat(body),
                                 "scale":vec(body,"m_LocalScale",[1,1,1])}
        elif cls==114:
            gm=re.search(r"m_GameObject: \{fileID: (-?\d+)\}",body)
            sm=re.search(r"m_Script: \{fileID: \d+, guid: ([0-9a-f]{32})",body)
            if gm:
                skin_block=re.search(r"^\s*skins:\s*\n((?:\s+-.*\n)+)",body,re.M)
                skin_guids=GUID.findall(skin_block.group(1)) if skin_block else []
                component_records.append({"id":fid,"gameObject":gm.group(1),"scriptGuid":sm.group(1) if sm else None,
                                         "skinGuids":skin_guids,"raw":body})
    transform_by_go={t["gameObject"]:tid for tid,t in transforms.items()}
    cache={}
    def world(tid,stack=None):
        if tid in cache:return cache[tid]
        stack=stack or set()
        if tid in stack:return ([0,0,0],[0,0,0,1],[1,1,1])
        stack.add(tid); t=transforms[tid]
        lp=t["position"]; lp=[lp[0],lp[1],-lp[2]]
        lq=t["rotation"]; lq=[-lq[0],-lq[1],lq[2],lq[3]]
        local=(lp,lq,t["scale"]); parent=t["parent"]
        result=local
        if parent not in ("0","") and parent in transforms: result=compose(world(parent,stack),local)
        cache[tid]=result; return result
    out.mkdir(parents=True,exist_ok=True); (out/"skins").mkdir(exist_ok=True)
    limbs=[]; copied={}
    for comp in component_records:
        script=guid_path.get(comp["scriptGuid"])
        if not script or script.name!="LimbRenderer.cs": continue
        go=objects.get(comp["gameObject"],{"name":"unknown","components":[]})
        tid=transform_by_go.get(comp["gameObject"])
        if not tid: continue
        pos,rot,scale=world(tid)
        skin_paths=[]
        for g in comp["skinGuids"]:
            p=guid_path.get(g)
            if p and p.is_file(): skin_paths.append(p)
        texture_url=None
        for src in skin_paths:
            candidates=[src]
            if src.suffix.lower() not in {".png",".jpg",".jpeg",".webp"}:
                candidates=[src.with_suffix(".png"),src.with_suffix(".jpg")]
            chosen=next((x for x in candidates if x.is_file()),None)
            if not chosen: continue
            key=str(chosen)
            if key not in copied:
                name=f"skin_{len(copied):03d}{chosen.suffix.lower()}"
                shutil.copy2(chosen,out/"skins"/name); copied[key]="skins/"+name
            texture_url=copied[key]; break
        limbs.append({"name":go["name"],"gameObjectFileID":comp["gameObject"],"transformFileID":tid,
                      "position":pos,"rotation":rot,"scale":scale,"skinGuids":comp["skinGuids"],
                      "texture":texture_url,"skinCandidates":[guid_path[g].name for g in comp["skinGuids"] if g in guid_path]})
    report={"sourcePrefab":"Assets/Resources/characters/Operator.prefab","limbRendererCount":len(limbs),
            "texturedLimbCount":sum(1 for x in limbs if x["texture"]),"copiedTextureCount":len(copied),
            "limbs":limbs}
    (out/"operator.json").write_text(json.dumps(report,indent=2))
    print(json.dumps({k:report[k] for k in ("sourcePrefab","limbRendererCount","texturedLimbCount","copiedTextureCount")},indent=2))
    for limb in limbs: print(limb["name"],limb["position"],limb["scale"],limb["texture"])
if __name__=="__main__": main()
