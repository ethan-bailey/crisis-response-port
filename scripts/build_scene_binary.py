#!/usr/bin/env python3
"""Losslessly repack OBJ corners, optionally omitting source-owned submeshes."""
import hashlib
import json
import re
import struct
import sys
from pathlib import Path

if len(sys.argv) not in (3, 4):
    raise SystemExit("usage: build_scene_binary.py SOURCE.obj OUTPUT.crg [EXTERIOR_VISIBILITY.json]")
src, out = Path(sys.argv[1]), Path(sys.argv[2])
omission = json.loads(Path(sys.argv[3]).read_text()) if len(sys.argv) == 4 else None
omitted = set(omission.get("submeshIndices", [])) if omission else set()
if omission and (omission.get("format") != "crisis-mission-exterior-visibility-v1" or
                 omission.get("submeshCount") != 764 or not omitted):
    raise ValueError("Mission exterior submesh manifest is invalid")
vs, ns, uvs, corners, lookup, indices = [], [], [], [], {}, []
current_submesh = None
observed_submeshes = set()
omitted_triangles = 0
for line in src.read_text().splitlines():
    p = line.split()
    if not p:
        continue
    if p[0] == "g":
        match = re.fullmatch(r"submesh_(\d+)", p[1]) if len(p) > 1 else None
        current_submesh = int(match.group(1)) if match else None
        if current_submesh is not None:
            observed_submeshes.add(current_submesh)
    elif p[0] == "v":
        vs.append(tuple(map(float, p[1:4])))
    elif p[0] == "vn":
        ns.append(tuple(map(float, p[1:4])))
    elif p[0] == "vt":
        uvs.append(tuple(map(float, p[1:3])))
    elif p[0] == "f":
        if omitted and current_submesh in omitted:
            omitted_triangles += 1
            continue
        face = [tuple(int(v) if v else 0 for v in x.split("/")) for x in p[1:]]
        if len(face) != 3:
            raise ValueError("Expected original triangular mesh")
        for key in face:
            if any(x <= 0 for x in key) or len(key) != 3:
                raise ValueError("Expected positive position/UV/normal indices")
            if key not in lookup:
                lookup[key] = len(corners)
                corners.append(key)
            indices.append(lookup[key])
if omitted and not omitted.issubset(observed_submeshes):
    raise ValueError(f"Exterior mask references missing OBJ submeshes: {sorted(omitted-observed_submeshes)}")
positions = [v for i, _, _ in corners for v in vs[i-1]]
normals = [v for _, _, i in corners for v in ns[i-1]]
texcoords = [v for _, i, _ in corners for v in uvs[i-1]]
blob = struct.pack("<4sIII", b"CRG1", len(corners), len(indices), 3)
for arr in (positions, normals, texcoords):
    blob += struct.pack("<" + "f" * len(arr), *arr)
blob += struct.pack("<" + "I" * len(indices), *indices)
out.parent.mkdir(parents=True, exist_ok=True)
out.write_bytes(blob)
report = {"sourceSha256": hashlib.sha256(src.read_bytes()).hexdigest(),
          "binarySha256": hashlib.sha256(blob).hexdigest(), "vertices": len(corners),
          "triangles": len(indices)//3, "sourceBytes": src.stat().st_size,
          "binaryBytes": len(blob), "omittedSubmeshes": sorted(omitted),
          "omittedTriangles": omitted_triangles,
          "sourceSceneSha256": omission.get("sceneSha256") if omission else None,
          "attributes": "original corner positions/normals/UVs, Float32; original triangle order"}
out.with_suffix(".report.json").write_text(json.dumps(report, indent=2))
print(json.dumps(report))
