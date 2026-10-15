#!/usr/bin/env python3
"""Exactly four replacements across exactly three 2017 utility scripts; no gameplay edits."""
import argparse
import json
import re
from pathlib import Path

def main():
    parser=argparse.ArgumentParser()
    parser.add_argument("project",type=Path)
    parser.add_argument("--out",required=True,type=Path)
    args=parser.parse_args()
    files=[
        ("ForcedReset.cs", "GUITexture", "UnityEngine.UI.RawImage",1),
        ("FPSCounter.cs", "GUIText", "UnityEngine.UI.Text",2),
        ("SimpleActivatorMenu.cs", "GUIText", "UnityEngine.UI.Text",1),
    ]
    patch=[]
    targets=[]
    for name,old,new,expected in files:
        matches=list((args.project/"Assets").rglob(name))
        if len(matches)!=1:raise SystemExit(f"STOP: {name} matched {len(matches)} paths (expected exactly 1)")
        path=matches[0]
        before=path.read_text(encoding="utf-8-sig")
        occurrences=list(re.finditer(r"\b"+re.escape(old)+r"\b",before))
        if len(occurrences)!=expected:
            raise SystemExit(f"STOP: {name} {old} occurrences {len(occurrences)}, expected {expected}")
        after=re.sub(r"\b"+re.escape(old)+r"\b",new,before)
        if after==before:raise SystemExit(f"STOP: {name} no changes")
        for match in occurrences:
            line=before.count("\n",0,match.start())+1
            patch.append({"file":str(path.relative_to(args.project)), "line":line,
                          "old_type":old, "new_type":new})
        targets.append((path,after))
    if len(patch)!=4:raise SystemExit("STOP: would exceed four changes")
    for path,after in targets:path.write_text(after,encoding="utf-8")
    args.out.parent.mkdir(parents=True,exist_ok=True)
    args.out.write_text(json.dumps({"patch_count":len(patch),"modified_files":len(targets),"changes":patch},indent=2)+"\n")
    print("Applied exactly 4 C# type replacements in exactly 3 recovered utility files")
    for p in patch:print(f"{p['file']}:{p['line']} {p['old_type']} => {p['new_type']}")
if __name__=="__main__":main()
