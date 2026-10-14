#!/usr/bin/env python3
"""Export an AssetRipper Unity YAML Mesh asset to OBJ using its serialized buffers.

Supports the common Unity 2017 YAML layout with interleaved float32 vertex data
and 16- or 32-bit triangle indices. Fails closed on unsupported layouts.
"""
from __future__ import annotations
import json
import re
import struct
import sys
from pathlib import Path

def required(pattern: str, text: str, label: str) -> str:
    m = re.search(pattern, text, re.M)
    if not m:
        raise ValueError(f"could not parse {label}")
    return m.group(1)

def main() -> int:
    if len(sys.argv) != 3:
        print("usage: unity_mesh_to_obj.py INPUT.asset OUTPUT.obj", file=sys.stderr)
        return 2
    src, out = Path(sys.argv[1]), Path(sys.argv[2])
    text = src.read_text(encoding="utf-8", errors="ignore")
    sub_block = re.search(r"^  m_SubMeshes:\s*\n(.*?)^  m_Shapes:", text, re.M | re.S)
    vert_block = re.search(r"^  m_VertexData:\s*\n(.*?)^  m_CompressedMesh:", text, re.M | re.S)
    if not sub_block or not vert_block:
        raise ValueError("expected Unity 2017 YAML m_SubMeshes, m_VertexData and m_CompressedMesh sections")
    subs = []
    for values in re.findall(
        r"- serializedVersion: \d+\s+firstByte: (\d+)\s+indexCount: (\d+)\s+topology: (\d+)\s+firstVertex: (\d+)\s+vertexCount: (\d+)",
        sub_block.group(1),
    ):
        first_byte, index_count, topology, first_vertex, vertex_count = map(int, values)
        subs.append({"firstByte": first_byte, "indexCount": index_count, "topology": topology,
                     "firstVertex": first_vertex, "vertexCount": vertex_count})
    if not subs:
        raise ValueError("no submeshes parsed")
    vb = vert_block.group(1)
    vertex_count = int(required(r"^\s+m_VertexCount: (\d+)", vb, "vertex count"))
    data_size = int(required(r"^\s+m_DataSize: (\d+)", vb, "vertex data size"))
    data_hex = required(r"^\s+_typelessdata: ([0-9a-fA-F]+)", vb, "vertex bytes")
    vertex_bytes = bytes.fromhex(data_hex)
    if len(vertex_bytes) != data_size:
        raise ValueError(f"vertex data size mismatch: declared {data_size}, decoded {len(vertex_bytes)}")
    if vertex_count == 0 or data_size % vertex_count:
        raise ValueError("invalid vertex count/stride")
    stride = data_size // vertex_count
    channel_block = re.search(r"^\s+m_Channels:\s*\n(.*?)^\s+m_DataSize:", vb, re.M | re.S)
    if not channel_block:
        raise ValueError("could not parse vertex channels")
    channels = []
    for values in re.findall(
        r"- stream: (\d+)\s+offset: (\d+)\s+format: (\d+)\s+dimension: (\d+)",
        channel_block.group(1),
    ):
        channels.append({"stream": int(values[0]), "offset": int(values[1]),
                         "format": int(values[2]), "dimension": int(values[3])})
    if not channels or any(c["stream"] != 0 for c in channels if c["dimension"]):
        raise ValueError("unsupported multi-stream vertex layout")
    def channel_offset(index: int, expected_dimension: int):
        if index >= len(channels):
            return None
        c = channels[index]
        if c["dimension"] == 0:
            return None
        if c["format"] not in (0, 1) or c["dimension"] < expected_dimension:
            raise ValueError(f"unsupported vertex channel {index}: {c}")
        component_size = 4 if c["format"] == 0 else 2
        if c["offset"] + c["dimension"] * component_size > stride:
            raise ValueError(f"channel exceeds vertex stride: {c}")
        return c["offset"]
    pos_offset = channel_offset(0, 3)
    normal_offset = channel_offset(1, 3)
    uv_offset = channel_offset(3, 2)
    if pos_offset is None:
        raise ValueError("mesh has no float32 position channel")
    idx_hex = required(r"^\s+m_IndexBuffer: ([0-9a-fA-F]+)", text, "index buffer")
    idx_bytes = bytes.fromhex(idx_hex)
    expected_indices = sum(s["indexCount"] for s in subs)
    if len(idx_bytes) == expected_indices * 2:
        index_size, fmt = 2, "<H"
    elif len(idx_bytes) == expected_indices * 4:
        index_size, fmt = 4, "<I"
    else:
        raise ValueError(f"index buffer length {len(idx_bytes)} does not match {expected_indices} indices at 2 or 4 bytes each")
    positions, normals, uvs = [], [], []
    for i in range(vertex_count):
        base = i * stride
        p = struct.unpack_from("<3f", vertex_bytes, base + pos_offset)
        positions.append((p[0], p[1], -p[2]))
        if normal_offset is not None:
            n = struct.unpack_from("<3f", vertex_bytes, base + normal_offset)
            normals.append((n[0], n[1], -n[2]))
        if uv_offset is not None:
            uv_format = channels[3]["format"]
            uvs.append(struct.unpack_from("<2e" if uv_format == 1 else "<2f", vertex_bytes, base + uv_offset))
    faces_by_submesh = []
    total_faces = 0
    max_index = -1
    for si, sub in enumerate(subs):
        if sub["topology"] != 0:
            raise ValueError(f"submesh {si} topology {sub['topology']} is not triangle list")
        start = sub["firstByte"]
        end = start + sub["indexCount"] * index_size
        if end > len(idx_bytes):
            raise ValueError(f"submesh {si} index range exceeds buffer")
        indices = [struct.unpack_from(fmt, idx_bytes, off)[0] for off in range(start, end, index_size)]
        if any(x >= vertex_count for x in indices):
            raise ValueError(f"submesh {si} contains out-of-range vertex index")
        if len(indices) % 3:
            raise ValueError(f"submesh {si} index count is not divisible by three")
        faces = [indices[i:i+3] for i in range(0, len(indices), 3)]
        faces_by_submesh.append(faces)
        total_faces += len(faces)
        if indices:
            max_index = max(max_index, max(indices))
    out.parent.mkdir(parents=True, exist_ok=True)
    with out.open("w", encoding="utf-8") as f:
        f.write(f"# Extracted from original serialized Unity mesh: {src.name}\n")
        f.write("# Coordinates converted to right-handed convention; face winding reversed.\n")
        for x,y,z in positions: f.write(f"v {x:.8g} {y:.8g} {z:.8g}\n")
        for u,v in uvs: f.write(f"vt {u:.8g} {v:.8g}\n")
        for x,y,z in normals: f.write(f"vn {x:.8g} {y:.8g} {z:.8g}\n")
        for si, faces in enumerate(faces_by_submesh):
            f.write(f"g submesh_{si:03d}\n")
            for a,b,c in faces:
                # Reverse winding after the Z-axis handedness conversion.
                ids = (a+1, c+1, b+1)
                if uvs and normals:
                    f.write("f " + " ".join(f"{i}/{i}/{i}" for i in ids) + "\n")
                elif uvs:
                    f.write("f " + " ".join(f"{i}/{i}" for i in ids) + "\n")
                else:
                    f.write("f " + " ".join(str(i) for i in ids) + "\n")
    meta = {
        "source": str(src), "output": str(out), "vertices": vertex_count,
        "strideBytes": stride, "indices": expected_indices, "triangles": total_faces,
        "submeshes": len(subs), "maxVertexIndex": max_index,
        "hasNormals": bool(normals), "hasUV0": bool(uvs),
        "conversion": "Unity left-handed coordinates to right-handed by negating Z and reversing triangle winding",
        "validation": "vertex/index bounds and buffer lengths passed",
    }
    out.with_suffix(".mesh-report.json").write_text(json.dumps(meta, indent=2), encoding="utf-8")
    print(json.dumps(meta))
    return 0

if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (ValueError, OSError, struct.error) as exc:
        print(f"mesh export failed: {exc}", file=sys.stderr)
        raise SystemExit(1)
