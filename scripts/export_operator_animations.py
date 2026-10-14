#!/usr/bin/env python3
"""Export original Unity transform animation curves for the Operator prefab."""
from __future__ import annotations
import json,re,sys
from pathlib import Path

def parse_section(text,name,next_names):
    start=re.search(r"^\s*"+re.escape(name)+r":\s*$",text,re.M)
    if not start:return ""
    end=len(text)
    for nxt in next_names:
        m=re.search(r"^\s*"+re.escape(nxt)+r":",text[start.end():],re.M)
        if m:end=min(end,start.end()+m.start())
    return text[start.end():end]
def curves(text,section,kind):
    block=parse_section(text,section,["m_RotationCurves","m_CompressedRotationCurves","m_EulerCurves","m_PositionCurves","m_ScaleCurves","m_FloatCurves","m_PPtrCurves","m_SampleRate"])
    if not block:return []
    entries=re.split(r"(?m)^\s*-\s+curve:\s*$",block)
    result=[]
    for entry in entries:
        path_match=re.search(r"(?m)^\s*path:\s*(.*)\s*$",entry)
        if not path_match:continue
        path=path_match.group(1).strip()
        keys=[]
        for m in re.finditer(r"(?m)^\s*time:\s*([-+0-9.eE]+)\s*\n\s*value:\s*\{([^}]+)\}",entry):
            time=float(m.group(1)); fields={}
            for part in m.group(2).split(","):
                if ":" in part:
                    k,v=part.split(":",1)
                    try:fields[k.strip()]=float(v.strip())
                    except ValueError:pass
            if kind=="rotation" and all(k in fields for k in ("x","y","z","w")):
                value=[-fields["x"],-fields["y"],fields["z"],fields["w"]]
            elif kind in ("position","scale") and all(k in fields for k in ("x","y","z")):
                value=[fields["x"],fields["y"],-fields["z"] if kind=="position" else fields["z"]]
            else:continue
            keys.append({"time":time,"value":value})
        if keys:result.append({"path":path,"property":kind,"keys":keys})
    return result
def main():
    if len(sys.argv)!=3:raise SystemExit("usage: export_operator_animations.py PROJECT_ROOT OUTPUT.json")
    root=Path(sys.argv[1]).resolve(); assets=root/"Assets"/"AnimationClip"
    names=["Body stand.anim","Body walk.anim","Body walk backward.anim","Body sprint.anim","Body kneel.anim","One leg limp.anim"]
    output={"source":"Recovered Unity AnimationClip YAML","clips":{}}
    for name in names:
        p=assets/name
        if not p.is_file():continue
        text=p.read_text(errors="ignore")
        tracks=[]
        for sec,kind in (("m_RotationCurves","rotation"),("m_PositionCurves","position"),("m_ScaleCurves","scale")):
            tracks.extend(curves(text,sec,kind))
        duration=max((k["time"] for t in tracks for k in t["keys"]),default=0)
        loop_match=re.search(r"m_AnimationClipSettings:\s*\n(?:(?!^---).*\n)*?\s*m_LoopTime:\s*(\d+)",text,re.M)
        output["clips"][p.stem]={"duration":duration,"loop":bool(loop_match and loop_match.group(1)=="1"),"tracks":tracks}
        print(f"{p.stem}: {len(tracks)} transform tracks, {duration:.3f}s, loop={output['clips'][p.stem]['loop']}")
    Path(sys.argv[2]).parent.mkdir(parents=True,exist_ok=True)
    Path(sys.argv[2]).write_text(json.dumps(output,separators=(",",":")))
if __name__=="__main__":main()
