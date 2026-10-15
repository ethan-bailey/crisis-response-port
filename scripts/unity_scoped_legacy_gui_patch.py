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
      "SimpleActivatorMenu.cs": [(7,"GUIText","UnityEngine.UI.Text")]
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
            token=r"\b"+re.escape(old)+r"\b"
            occurrences=len(list(re.finditer(token,current)))
            if occurrences!=1:
                raise SystemExit(f"STOP: {name}:{number} {old} occurrences {occurrences} expected 1")
            lines[number-1]=re.sub(token,new,current)
            audit.append({"file":str(path.relative_to(args.project)),"line":number,
                          "old_type":old,"new_type":new})
        planned.append((path,"".join(lines)))
    if len(audit)!=5 or len(planned)!=3:raise SystemExit("STOP: five-site guard rejected")
    for path,content in planned:path.write_text(content,encoding="utf-8")
    args.out.parent.mkdir(parents=True,exist_ok=True)
    args.out.write_text(json.dumps({"patch_count":5,"modified_files":3,"changes":audit},indent=2)+"\n")
    for p in audit:print(f"PATCH {p['file']}:{p['line']} {p['old_type']} => {p['new_type']}")
if __name__=="__main__":main()
