#!/usr/bin/env python3
"""Read-only audit of recovered Unity UNet BehaviorDesigner task references."""
import argparse, json, re
from pathlib import Path
parser=argparse.ArgumentParser()
parser.add_argument("project",type=Path)
parser.add_argument("--output",type=Path,required=True)
a=parser.parse_args()
assets=a.project/"Assets"
targets=["IsServer.cs","IsClient.cs"]
report={"target_scripts":[],"scenes":[],"text_asset_references":[],"limitations":[]}
guids=set()
for name in targets:
 files=list(assets.rglob(name))
 for f in files:
  m=Path(str(f)+".meta")
  guid=re.search(r"^guid:\s*([0-9a-f]{32})\s*$",m.read_text(errors="replace"),re.M) if m.exists() else None
  if guid:guids.add(guid.group(1))
  report["target_scripts"].append({"path":str(f.relative_to(assets)),"guid":guid.group(1) if guid else None,
                                  "source":f.read_text(errors="replace")[:7000]})
for scene in assets.rglob("*.unity"):
 report["scenes"].append(str(scene.relative_to(assets)))
tokens=list(guids)+["BehaviorDesigner.Runtime.Tasks.Basic.UnityNetwork.IsServer",
                    "BehaviorDesigner.Runtime.Tasks.Basic.UnityNetwork.IsClient",
                    "UnityNetwork.IsServer","UnityNetwork.IsClient"]
for f in assets.rglob("*"):
 if not f.is_file() or f.suffix.lower() not in {".unity",".prefab",".asset",".controller",".overridecontroller",".json",".bytes",".txt"}:continue
 if f.stat().st_size>15_000_000:continue
 try:s=f.read_text(errors="replace")
 except OSError:continue
 hits=[t for t in tokens if t and t in s]
 if hits:
  report["text_asset_references"].append({"path":str(f.relative_to(assets)),"hits":hits})
report["limitations"]=[
 "No binary Unity serialized asset or BehaviorDesigner custom task-blob decoder is used.",
 "Absence of plain-text GUID/class matches cannot prove a task is unused in a scene.",
 "This script audits only named targets; broader legacy UNet dependencies may exist."
]
a.output.parent.mkdir(parents=True,exist_ok=True)
a.output.write_text(json.dumps(report,indent=2)+"\n")
print(json.dumps({"targets":[{"path":x["path"],"guid":x["guid"]} for x in report["target_scripts"]],
 "scene_count":len(report["scenes"]),"references":report["text_asset_references"][:40]},indent=2))
