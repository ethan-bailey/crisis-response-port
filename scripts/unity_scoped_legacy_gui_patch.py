#!/usr/bin/env python3
"""Patch ONLY the exact four CS0619 source lines identified in prior Unity import log."""
import argparse, json, re
from pathlib import Path

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("project",type=Path)
    parser.add_argument("--out",required=True,type=Path)
    args=parser.parse_args()
    # Prior Unity 2022.3.62f1 import diagnostics, not arbitrary string matches.
    specs={
      "ForcedReset.cs": [(4,"GUITexture","UnityEngine.UI.RawImage")],
      "FPSCounter.cs": [(5,"GUIText","UnityEngine.UI.Text"),(18,"GUIText","UnityEngine.UI.Text"),(23,"GUIText","UnityEngine.UI.Text")],
      "SimpleActivatorMenu.cs": [(7,"GUIText","UnityEngine.UI.Text")],
      "DepthOfField.cs": [(245,"Graphics.DrawProceduralIndirect","Graphics.DrawProceduralIndirectNow"),(299,"Graphics.DrawProceduralIndirect","Graphics.DrawProceduralIndirectNow")],
      "SunShafts.cs": [(93,".hdr",".allowHDR")],
      "BloomAndFlares.cs": [(105,".hdr",".allowHDR")]
    }
    planned=[]; audit=[]
    for name,entries in specs.items():
        matches=list((args.project/"Assets").rglob(name))
        if len(matches)!=1:raise SystemExit(f"STOP: {name} matched {len(matches)} paths (expected 1)")
        path=matches[0]
        contents=path.read_text(encoding="utf-8-sig")
        lines=contents.splitlines(keepends=True)
        for number,old,new in entries:
            if number>len(lines):raise SystemExit(f"STOP: {name}:{number} out of range")
            current=lines[number-1]
            token=(re.escape(old)+r"\b") if old.startswith(".") else (r"\b"+re.escape(old)+r"\b")
            occurrences=len(list(re.finditer(token,current)))
            if occurrences!=1:
                raise SystemExit(f"STOP: {name}:{number} {old} occurrences {occurrences} expected 1")
            lines[number-1]=re.sub(token,new,current)
            audit.append({"file":str(path.relative_to(args.project)),"line":number,
                          "old_type":old,"new_type":new})
        planned.append((path,"".join(lines)))
    if len(audit)!=9 or len(planned)!=6:raise SystemExit("STOP: nine-site guard rejected")
    # Diagnostic-only: preserve original UNet tasks verbatim but skip them
    # in 2022 where UNet is removed. Never fabricate replacement behavior.
    import os
    if os.environ.get("UNITY_SPIKE_ALLOW_UNET_EXCLUSION") == "1":
        unet={
          "IsServer.cs":("NetworkServer.active","adaab1508848919f83c42310c9c52abe"),
          "IsClient.cs":("NetworkClient.active","86e5e79a41eca15ee7c3223d03b2a5d5")
        }
        for name,(token,guid) in unet.items():
            matches=list((args.project/"Assets").rglob(name))
            if len(matches)!=1:raise SystemExit(f"STOP: {name} matches {len(matches)}")
            path=matches[0]
            source=path.read_text(encoding="utf-8-sig")
            meta=Path(str(path)+".meta")
            if token not in source or not meta.exists() or guid not in meta.read_text(errors="replace"):
                raise SystemExit(f"STOP: unexpected source or GUID for {name}")
            planned.append((path,"#if !UNITY_2022_3_OR_NEWER\n"+source+"\n#endif\n"))
            audit.append({"file":str(path.relative_to(args.project)),
                          "diagnostic_only":"Unity 2022 temporary UNet exclusion; original source retained"})
    for path,content in planned:path.write_text(content,encoding="utf-8")
    args.out.parent.mkdir(parents=True,exist_ok=True)
    args.out.write_text(json.dumps({"patch_count":len(audit),"modified_files":len(planned),"changes":audit},indent=2)+"\n")
    for p in audit: print('PATCH '+p['file']+' '+str(p.get('line','guard'))+' '+p.get('old_type','legacy UNet')+' => '+p.get('new_type','excluded in Unity 2022'))
if __name__=="__main__":main()
