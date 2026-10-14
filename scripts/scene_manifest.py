#!/usr/bin/env python3
"""Extract a machine-readable, non-lossless manifest from AssetRipper Unity YAML."""
from __future__ import annotations
import json
import re
import sys
from pathlib import Path

CLASS_NAMES = {
    1:"GameObject", 4:"Transform", 20:"Camera", 21:"Material",
    23:"MeshRenderer", 33:"MeshFilter", 43:"Mesh", 54:"Rigidbody",
    65:"BoxCollider", 74:"Animation", 95:"Animator", 108:"Light",
    114:"MonoBehaviour", 120:"LineRenderer", 122:"ParticleSystem",
    135:"SphereCollider", 136:"CapsuleCollider", 137:"SkinnedMeshRenderer",
    152:"TerrainCollider", 154:"Terrain", 156:"TerrainData", 198:"ParticleSystemRenderer",
    208:"LODGroup", 212:"SpriteRenderer", 223:"Canvas", 224:"RectTransform",
    225:"CanvasRenderer", 222:"CanvasGroup", 2:"Component"
}
DOC_RE = re.compile(r"^--- !u!(\d+) &(\d+)\s*$", re.M)
GUID_RE = re.compile(r"guid: ([0-9a-f]{32})")

def one(pattern: str, text: str):
    m = re.search(pattern, text, re.M)
    return m.groups() if m else None

def vector(pattern: str, text: str, n: int):
    g = one(pattern, text)
    if not g:
        return None
    try:
        return [float(x) for x in g[:n]]
    except ValueError:
        return list(g[:n])

def main() -> int:
    if len(sys.argv) != 4:
        print("usage: scene_manifest.py PROJECT_ROOT SCENE_PATH OUTPUT_JSON", file=sys.stderr)
        return 2
    project = Path(sys.argv[1]).resolve()
    scene_path = (project / sys.argv[2]).resolve()
    output = Path(sys.argv[3])
    assets = project / "Assets"
    guid_paths = {}
    for meta in assets.rglob("*.meta"):
        try:
            txt = meta.read_text(encoding="utf-8", errors="ignore")
        except OSError:
            continue
        match = re.search(r"^guid: ([0-9a-f]{32})", txt, re.M)
        if match:
            guid_paths[match.group(1)] = str(meta.with_suffix("").relative_to(project))
    source = scene_path.read_text(encoding="utf-8", errors="ignore")
    matches = list(DOC_RE.finditer(source))
    records = {}
    for idx, match in enumerate(matches):
        cls, fid = int(match.group(1)), int(match.group(2))
        body = source[match.end():matches[idx+1].start() if idx+1 < len(matches) else len(source)]
        rec = {"fileID":fid, "classID":cls, "className":CLASS_NAMES.get(cls, f"UnityClass{cls}")}
        name = one(r"^  m_Name: (.*)$", body)
        if name:
            rec["name"] = name[0].strip().strip("'\"")
        go = one(r"^  m_GameObject: \{fileID: (-?\d+)\}", body)
        if go: rec["gameObjectFileID"] = int(go[0])
        script = one(r"^  m_Script: \{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: (\d+)\}", body)
        if script:
            rec["script"] = {"fileID":int(script[0]),"guid":script[1],"type":int(script[2]),"path":guid_paths.get(script[1])}
        mesh = one(r"^  m_Mesh: \{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: (\d+)\}", body)
        if mesh:
            rec["mesh"] = {"fileID":int(mesh[0]),"guid":mesh[1],"type":int(mesh[2]),"path":guid_paths.get(mesh[1])}
        parent = one(r"^  m_Father: \{fileID: (-?\d+)\}", body)
        if parent: rec["parentFileID"] = int(parent[0])
        pos = vector(r"^  m_LocalPosition: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}", body, 3)
        rot = vector(r"^  m_LocalRotation: \{x: ([^,]+), y: ([^,]+), z: ([^,]+), w: ([^}]+)\}", body, 4)
        scale = vector(r"^  m_LocalScale: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}", body, 3)
        if pos is not None: rec["localPosition"] = pos
        if rot is not None: rec["localRotation"] = rot
        if scale is not None: rec["localScale"] = scale
        components = re.findall(r"^  - component: \{fileID: (-?\d+)\}", body, re.M) if cls == 1 else []
        if components: rec["components"] = [int(x) for x in components]
        materials = re.findall(r"^  - \{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: (\d+)\}", body, re.M) if cls in (23, 137, 198) else []
        if materials:
            rec["materials"] = [{"fileID":int(a),"guid":b,"type":int(c),"path":guid_paths.get(b)} for a,b,c in materials]
        # Preserve the external references found in every serialized record.
        refs = []
        for fm, gd, ty in re.findall(r"\{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: (\d+)\}", body):
            refs.append({"fileID":int(fm),"guid":gd,"type":int(ty),"path":guid_paths.get(gd)})
        if refs: rec["externalReferences"] = refs
        records[fid] = rec
    game_objects = []
    for rec in records.values():
        if rec["classID"] != 1: continue
        components = []
        for cid in rec.get("components", []):
            c = records.get(cid)
            if not c: continue
            summary = {"fileID":cid,"classID":c["classID"],"className":c["className"]}
            for k in ("script","mesh","materials","localPosition","localRotation","localScale","parentFileID"):
                if k in c: summary[k] = c[k]
            components.append(summary)
        rec["componentDetails"] = components
        rec["transform"] = next((c for c in components if c["className"] in ("Transform","RectTransform")), None)
        game_objects.append(rec)

    # Reconstruct parent/child GameObject links from serialized Transform references.
    transform_to_game_object = {
        rec["fileID"]: rec["gameObjectFileID"]
        for rec in records.values()
        if rec["classID"] in (4, 224) and "gameObjectFileID" in rec
    }
    by_game_object = {rec["fileID"]: rec for rec in game_objects}
    hierarchy_edges = 0
    unresolved_parent_refs = []
    roots = []
    without_transform = []
    for go in game_objects:
        tr = next(
            (records.get(cid) for cid in go.get("components", [])
             if records.get(cid, {}).get("classID") in (4, 224)),
            None,
        )
        if tr is None:
            without_transform.append(go["fileID"])
            continue
        parent_transform = tr.get("parentFileID", 0)
        if parent_transform in (0, None):
            go["parentGameObjectFileID"] = None
            roots.append(go["fileID"])
        elif parent_transform in transform_to_game_object:
            parent_go = transform_to_game_object[parent_transform]
            go["parentGameObjectFileID"] = parent_go
            parent = by_game_object.get(parent_go)
            if parent is not None:
                parent.setdefault("childGameObjectFileIDs", []).append(go["fileID"])
                hierarchy_edges += 1
        else:
            go["parentGameObjectFileID"] = None
            unresolved_parent_refs.append({"gameObjectFileID": go["fileID"], "parentTransformFileID": parent_transform})

    counts = {}
    for rec in records.values():
        key = rec["className"]
        counts[key] = counts.get(key,0)+1
    payload = {
        "sourceScene": str(scene_path.relative_to(project)),
        "extraction": "Heuristic AssetRipper Unity YAML extraction; not a lossless scene conversion",
        "records": len(records),
        "classCounts": dict(sorted(counts.items())),
        "guidIndexEntries": len(guid_paths),
        "gameObjects": game_objects,
        "hierarchy": {"rootGameObjectFileIDs": roots, "parentChildEdges": hierarchy_edges, "gameObjectsWithoutTransform": without_transform, "unresolvedParentTransformReferences": unresolved_parent_refs},
        "otherRecords": [r for r in records.values() if r["classID"] != 1]
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(payload, indent=2), encoding="utf-8")
    print(f"records={len(records)} gameObjects={len(game_objects)} classes={len(counts)} guidIndex={len(guid_paths)} roots={len(roots)} hierarchyEdges={hierarchy_edges} withoutTransform={len(without_transform)} unresolvedParents={len(unresolved_parent_refs)} output={output}")
    return 0

if __name__ == "__main__":
    raise SystemExit(main())
