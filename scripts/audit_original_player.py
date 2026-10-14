#!/usr/bin/env python3
"""Audit original Mission 01 objects and map serialized asset references."""
from __future__ import annotations
import json, re, sys
from pathlib import Path

HEADER = re.compile(r"^--- !u!(\d+) &(\d+)", re.M)
GUID = re.compile(r"guid: ([0-9a-f]{32})")
REF = re.compile(r"fileID: (-?\d+)")
NAME = re.compile(r"^  m_Name: (.*)$", re.M)

def docs(text):
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
    guid_to_asset = {}
    for meta in assets.rglob("*.meta"):
        m = re.search(r"^guid: ([0-9a-f]{32})", meta.read_text(errors="ignore"), re.M)
        if m:
            guid_to_asset[m.group(1)] = str(meta.with_suffix("").relative_to(root))
    script_guid = {guid: path for guid, path in guid_to_asset.items() if path.endswith(".cs")}
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
        elif cls in (114, 95, 136, 54, 2):
            gameobj = re.search(r"m_GameObject:\s*\{fileID: (-?\d+)\}", body)
            refs = sorted(set(GUID.findall(body)))
            components[fid] = {"fileID": fid, "classID": cls, "gameObjectID": gameobj.group(1) if gameobj else None,
                               "scriptGUID": refs[0] if refs and cls == 114 else None,
                               "assetGUIDs": refs, "serializedFields": body.strip()}
    records = []
    for obj in objects.values():
        comps = []
        for cid in obj["componentIDs"]:
            c = components.get(cid)
            if not c: continue
            item = dict(c)
            if item.get("scriptGUID"):
                item["scriptPath"] = script_guid.get(item["scriptGUID"])
            item["assetRefs"] = [{"guid": g, "path": guid_to_asset.get(g)} for g in item.get("assetGUIDs", [])]
            comps.append(item)
        name = obj["name"]
        scripts = " ".join((c.get("scriptPath") or "") for c in comps).lower()
        lname = name.lower()
        score = 0
        if any(t in lname for t in ("player", "firstperson", "first person", "fpscontroller", "character")): score += 3
        if any(t in scripts for t in ("player", "charactercontroller", "movement", "firstperson", "fpscontroller")): score += 4
        if any(t in scripts for t in ("weapon", "inventory", "input")): score += 1
        if score or any(comp.get("classID") == 114 for comp in comps):
            records.append({"score": score, "name": name, "gameObjectFileID": obj["fileID"], "components": comps})
    records.sort(key=lambda x: (-x["score"], x["name"]))
    all_scripts = sorted({comp.get("scriptPath") for item in records for comp in item["components"] if comp.get("scriptPath")})
    output = {"sourceScene": str(scene.relative_to(root)), "sceneObjectCount": len(objects),
              "sceneComponentCount": len(components), "scriptGuidCount": len(script_guid),
              "assetGuidCount": len(guid_to_asset), "scriptBearingObjectCount": len(records),
              "scriptPaths": all_scripts, "candidates": records}
    out = Path(sys.argv[2]); out.parent.mkdir(parents=True, exist_ok=True)
    out.write_text(json.dumps(output, indent=2), encoding="utf-8")
    print(f"Objects: {len(objects)}; components: {len(components)}; script-bearing objects: {len(records)}")
    for item in records:
        for comp in item["components"]:
            if comp.get("scriptPath") and any(k in comp["scriptPath"] for k in ("OperatorSpawner", "OccupantSpawner", "GlobalContextInitializer", "Marksmen", "Mission.cs")):
                print(f"{item['name']} [{item['gameObjectFileID']}] -> {comp['scriptPath']}")
                for ref in comp.get("assetRefs", []):
                    if ref.get("path"): print(f"    asset: {ref['path']}")
if __name__ == "__main__":
    main()
