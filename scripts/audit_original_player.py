#!/usr/bin/env python3
"""Audit the recovered Mission 01 scene for original player/controller objects.

Static YAML inspection only; this does not instantiate Unity objects or infer behavior
from object names alone. It emits component/script references for the next source-port step.
"""
from __future__ import annotations
import json, re, sys
from pathlib import Path

HEADER = re.compile(r"^--- !u!(\d+) &(\d+)", re.M)
GUID = re.compile(r"guid: ([0-9a-f]{32})")
REF = re.compile(r"fileID: (-?\d+)")
NAME = re.compile(r"^  m_Name: (.*)$", re.M)

def docs(text: str):
    matches = list(HEADER.finditer(text))
    for i, m in enumerate(matches):
        yield int(m.group(1)), m.group(2), text[m.end():matches[i+1].start() if i+1 < len(matches) else len(text)]

def main():
    if len(sys.argv) != 3:
        raise SystemExit("usage: audit_original_player.py PROJECT_ROOT OUTPUT.json")
    root = Path(sys.argv[1]).resolve()
    assets = root / "Assets"
    scene = assets / "Scenes" / "Mission 01.unity"
    if not scene.is_file():
        raise SystemExit(f"Missing recovered scene: {scene}")
    script_guid = {}
    for meta in assets.rglob("*.cs.meta"):
        m = re.search(r"^guid: ([0-9a-f]{32})", meta.read_text(errors="ignore"), re.M)
        if m:
            script_guid[m.group(1)] = str(meta.with_suffix("").relative_to(root))
    raw = scene.read_text(errors="ignore")
    objects, components = {}, {}
    for cls, fid, body in docs(raw):
        if cls == 1:
            nm = NAME.search(body)
            comp_refs = []
            for line in body.splitlines():
                if "component:" in line:
                    ids = REF.findall(line)
                    if ids: comp_refs.append(ids[-1])
            objects[fid] = {"fileID": fid, "name": nm.group(1).strip() if nm else "", "componentIDs": comp_refs}
        elif cls in (114, 95, 136, 54,  Character := 2):
            gameobj = re.search(r"m_GameObject:\s*\{fileID: (-?\d+)\}", body)
            refs = GUID.findall(body)
            components[fid] = {"fileID": fid, "classID": cls, "gameObjectID": gameobj.group(1) if gameobj else None,
                               "scriptGUID": refs[0] if refs and cls == 114 else None}
    records = []
    for obj in objects.values():
        comps = []
        for cid in obj["componentIDs"]:
            c = components.get(cid)
            if not c: continue
            item = dict(c)
            if item.get("scriptGUID"):
                item["scriptPath"] = script_guid.get(item["scriptGUID"])
            comps.append(item)
        name = obj["name"]
        scripts = " ".join((c.get("scriptPath") or "") for c in comps).lower()
        lname = name.lower()
        score = 0
        if any(t in lname for t in ("player", "firstperson", "first person", "fpscontroller", "character")): score += 3
        if any(t in scripts for t in ("player", "charactercontroller", "movement", "firstperson", "fpscontroller")): score += 4
        if any(t in scripts for t in ("weapon", "inventory", "input")): score += 1
        if score:
            records.append({"score": score, "name": name, "gameObjectFileID": obj["fileID"], "components": comps})
    records.sort(key=lambda x: (-x["score"], x["name"]))
    output = {"sourceScene": str(scene.relative_to(root)), "sceneObjectCount": len(objects),
              "sceneComponentCount": len(components), "scriptGuidCount": len(script_guid),
              "candidates": records}
    out = Path(sys.argv[2]); out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(output, indent=2), encoding="utf-8")
    print(f"Objects: {len(objects)}; components: {len(components)}; candidate player/controller objects: {len(records)}")
    for item in records[:30]:
        print(f"[{item['score']}] {item['name']} (fileID {item['gameObjectFileID']})")
        for comp in item["components"]:
            if comp.get("scriptPath"): print(f"    {comp['scriptPath']}")
if __name__ == "__main__":
    main()
