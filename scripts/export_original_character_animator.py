#!/usr/bin/env python3
"""Recover executable AnimatorController transitions and blend trees from ORIGINAL Unity YAML.
No heuristic locomotion-state recreation; only source fileID/GUID relationships.
"""
import argparse
import hashlib
import json
import re
from pathlib import Path

CLIP_GUIDS = {
 "bc384850d352dd044b3ab066965f6f06": "Body stand",
 "ae9f3a81fa9fc284f8355040c4b66523": "Body kneel",
 "2f5a69c2a5c027342a2db0801ba259c5": "Body walk",
 "e4ec5e60d58d9184e9a261f8a7507b2d": "Body sprint",
 "d65c0d12f3db5594181d3f01b395a218": "Body walk backward",
 "4b62a96303559c2489b71ac3bdded891": "One leg limp",
}
def prop(block,key,default=None):
 m=re.search(r"(?m)^  "+re.escape(key)+r":\s*(.*?)\s*$",block)
 return m.group(1).strip() if m else default

def blocks(text):
 matches=list(re.finditer(r"(?m)^--- !u!(\d+) &(\d+)\s*$",text))
 for i,m in enumerate(matches):
  yield m.group(1),m.group(2),text[m.end():matches[i+1].start() if i+1<len(matches) else len(text)]
def generate(path):
 raw=path.read_bytes();src=raw.decode()
 sections={(ty,ident):text for ty,ident,text in blocks(src)}
 states={}
 for (kind,ident),b in sections.items():
  if kind!="1102":continue
  name=prop(b,"m_Name")
  motion=re.search(r"(?m)^  m_Motion: \{fileID: (\d+)(?:, guid: ([^,]+))?",b)
  if not motion:raise ValueError("Animator state has no source motion: "+name)
  transitions=re.findall(r"(?m)^  - \{fileID: (\d+)\}\s*$",b.split("m_Transitions:",1)[1].split("m_StateMachineBehaviours:",1)[0])
  states[ident]={"id":ident,"name":name,"motionId":motion.group(1),"motionGuid":motion.group(2),
                 "transitions":transitions}
 trees={}
 for (kind,ident),b in sections.items():
  if kind!="206":continue
  children=[]
  for item in re.split(r"(?m)^  - serializedVersion: 2\s*$",b)[1:]:
   match=re.search(r"m_Motion: \{fileID: (\d+)(?:, guid: ([^,]+))?",item)
   thresh=re.search(r"m_Threshold:\s*([-+0-9.eE]+)",item)
   if match and thresh:
    guid=match.group(2)
    if guid not in CLIP_GUIDS:raise ValueError("Missing source clip "+str(guid))
    children.append({"clip":CLIP_GUIDS[guid],"guid":guid,"threshold":float(thresh.group(1))})
  trees[ident]={"parameter":prop(b,"m_BlendParameter"),"children":children,"blendType":int(prop(b,"m_BlendType","0"))}
 transitions={}
 for (kind,ident),b in sections.items():
  if kind!="1101":continue
  dest=re.search(r"m_DstState: \{fileID: (\d+)\}",b)
  if not dest:raise ValueError("Missing transition destination "+ident)
  conditions=[{"mode":int(m.group(1)),"parameter":m.group(2),"threshold":float(m.group(3))}
              for m in re.finditer(r"m_ConditionMode:[ \t]*(\d+)\r?\n[ \t]*m_ConditionEvent:[ \t]*([^\n]+)\r?\n[ \t]*m_EventTreshold:[ \t]*([-+0-9.eE]+)",b)]
  transitions[ident]={"destination":dest.group(1),"duration":float(prop(b,"m_TransitionDuration","0")),
    "hasExitTime":prop(b,"m_HasExitTime","0")=="1","fixedDuration":prop(b,"m_HasFixedDuration","0")=="1",
    "interruptionSource":int(prop(b,"m_InterruptionSource","0")),"conditions":conditions}
 for state in states.values():
  guid=state.pop("motionGuid")
  motionId=state.pop("motionId")
  if guid:
   if guid not in CLIP_GUIDS:raise ValueError("Unmapped original clip "+guid)
   state["motion"]={"clip":CLIP_GUIDS[guid],"guid":guid}
  else:
   if motionId not in trees:raise ValueError("Unresolved original BlendTree "+motionId)
   state["motion"]={"blendTree":trees[motionId]}
  state["transitions"]=[transitions[i] for i in state["transitions"]]
 machines=[b for (kind,ident),b in sections.items() if kind=="1107"]
 if len(machines)!=1:raise ValueError("Expected one recovered Character Animator state machine")
 default=re.search(r"m_DefaultState: \{fileID: (\d+)\}",machines[0])
 if not default:raise ValueError("Missing original default state")
 obj={"schema":1,"source":"Original Unity AnimatorController YAML",
  "sourceSha256":hashlib.sha256(raw).hexdigest(),"defaultState":default.group(1),
  "parameters":["Moving","Backwards","KneelRatio","CrippledRatio","MoveSpeed"],
  "states":states}
 if len(states)!=4 or sum(len(s["transitions"]) for s in states.values())!=11 or any(not t["conditions"] for s in states.values() for t in s["transitions"]):
  raise ValueError("Original controller state/transition count mismatch")
 return obj
if __name__=="__main__":
 a=argparse.ArgumentParser();a.add_argument("source",type=Path);a.add_argument("output",type=Path);ns=a.parse_args()
 obj=generate(ns.source);ns.output.parent.mkdir(parents=True,exist_ok=True)
 ns.output.write_text(json.dumps(obj,indent=2,sort_keys=True)+"\n")
 print("Recovered",len(obj["states"]),"original states",sum(len(s["transitions"]) for s in obj["states"].values()),"transitions",obj["sourceSha256"])
