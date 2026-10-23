
#!/usr/bin/env python3
"""Export exact Unity 2017 Mesh channels to the browser CRG1 scene format."""
from __future__ import annotations
import hashlib
import json
import math
import re
import struct
import sys
from pathlib import Path


def required(pattern: str, text: str, label: str) -> str:
    match = re.search(pattern, text, re.M)
    if not match:
        raise ValueError(f"could not parse {label}")
    return match.group(1)


def export_mesh(src: Path, out: Path) -> dict:
    text = src.read_text(encoding="utf-8", errors="strict")
    sub_block = re.search(r"^  m_SubMeshes:\s*\n(.*?)^  m_Shapes:", text, re.M | re.S)
    vert_block = re.search(r"^  m_VertexData:\s*\n(.*?)^  m_CompressedMesh:", text, re.M | re.S)
    if not sub_block or not vert_block:
        raise ValueError("expected Unity 2017 YAML submeshes and vertex data")
    subs = []
    for values in re.findall(r"- serializedVersion: \d+\s+firstByte: (\d+)\s+indexCount: (\d+)\s+topology: (\d+)\s+firstVertex: (\d+)\s+vertexCount: (\d+)", sub_block.group(1)):
        first_byte, index_count, topology, first_vertex, vertex_count = map(int, values)
        subs.append((first_byte, index_count, topology, first_vertex, vertex_count))
    if not subs:
        raise ValueError("no source submeshes found")
    vb = vert_block.group(1)
    vertex_count = int(required(r"^\s*m_VertexCount: (\d+)", vb, "vertex count"))
    data_size = int(required(r"^\s*m_DataSize: (\d+)", vb, "vertex byte count"))
    vertex_data = bytes.fromhex(required(r"^\s*_typelessdata: ([0-9a-fA-F]+)", vb, "vertex bytes"))
    if not vertex_count or len(vertex_data) != data_size or data_size % vertex_count:
        raise ValueError("source vertex buffer length/count mismatch")
    stride = data_size // vertex_count
    channel_block = re.search(r"^\s*m_Channels:\s*\n(.*?)^\s*m_DataSize:", vb, re.M | re.S)
    if not channel_block:
        raise ValueError("could not parse source vertex channels")
    channels = [tuple(map(int, values)) for values in re.findall(r"- stream: (\d+)\s+offset: (\d+)\s+format: (\d+)\s+dimension: (\d+)", channel_block.group(1))]
    def offset_for(index: int, dimensions: int) -> int:
        if index >= len(channels):
            raise ValueError(f"missing source vertex channel {index}")
        stream, offset, fmt, dimension = channels[index]
        if stream != 0 or fmt not in (0, 1) or dimension < dimensions:
            raise ValueError(f"unsupported source vertex channel {index}: {channels[index]}")
        component_bytes = 4 if fmt == 0 else 2
        if offset + dimensions * component_bytes > stride:
            raise ValueError(f"source vertex channel {index} exceeds stride")
        return offset
    pos_offset = offset_for(0, 3)
    normal_offset = offset_for(1, 3)
    uv_offset = offset_for(3, 2)
    formats = (channels[0][2], channels[1][2], channels[3][2])
    def read_components(row: int, offset: int, count: int, fmt: int) -> tuple[float, ...]:
        code = "f" if fmt == 0 else "e"
        values = struct.unpack_from("<" + str(count) + code, vertex_data, row * stride + offset)
        if any(not math.isfinite(value) for value in values):
            raise ValueError("non-finite source mesh channel")
        return values
    positions, normals, uvs = [], [], []
    for row in range(vertex_count):
        x, y, z = read_components(row, pos_offset, 3, formats[0])
        nx, ny, nz = read_components(row, normal_offset, 3, formats[1])
        u, v = read_components(row, uv_offset, 2, formats[2])
        positions.extend((x, y, -z))
        normals.extend((nx, ny, -nz))
        uvs.extend((u, v))
    index_count = sum(sub[1] for sub in subs)
    index_bytes = bytes.fromhex(required(r"^\s*m_IndexBuffer: ([0-9a-fA-F]+)", text, "index buffer"))
    if len(index_bytes) == index_count * 2:
        index_width, index_format = 2, "<H"
    elif len(index_bytes) == index_count * 4:
        index_width, index_format = 4, "<I"
    else:
        raise ValueError("source index buffer size does not match submeshes")
    indices = []
    for first_byte, count, topology, first_vertex, sub_vertex_count in subs:
        if topology != 0 or count % 3 or first_byte % index_width:
            raise ValueError("source submesh is not a valid triangle list")
        start, end = first_byte, first_byte + count * index_width
        if end > len(index_bytes):
            raise ValueError("source submesh exceeds index buffer")
        sub_indices = [struct.unpack_from(index_format, index_bytes, off)[0] for off in range(start, end, index_width)]
        if any(index >= vertex_count for index in sub_indices):
            raise ValueError("source index references a missing vertex")
        for i in range(0, len(sub_indices), 3):
            a, b, c = sub_indices[i:i + 3]
            indices.extend((a, c, b))
    if not positions or len(normals) != vertex_count * 3 or len(uvs) != vertex_count * 2 or len(indices) % 3:
        raise ValueError("source mesh geometry channels are incomplete")
    payload = bytearray(struct.pack("<4I", 0x31475243, vertex_count, len(indices), 3))
    payload.extend(struct.pack("<" + "f" * len(positions), *positions))
    payload.extend(struct.pack("<" + "f" * len(normals), *normals))
    payload.extend(struct.pack("<" + "f" * len(uvs), *uvs))
    payload.extend(struct.pack("<" + "I" * len(indices), *indices))
    out.parent.mkdir(parents=True, exist_ok=True)
    out.write_bytes(payload)
    report = {
        "source": str(src), "sourceSha256": hashlib.sha256(src.read_bytes()).hexdigest(),
        "output": str(out), "outputSha256": hashlib.sha256(payload).hexdigest(),
        "format": "CRG1", "vertices": vertex_count, "indices": len(indices),
        "triangles": len(indices) // 3, "submeshes": len(subs), "strideBytes": stride,
        "channels": ["position3f32", "normal3f32", "uv0_2f32", "indices_u32"],
        "conversion": "Unity to right-handed coordinates: negate position/normal Z and reverse each source triangle winding",
        "validation": "source channel bounds, finite values, and triangle indices passed",
    }
    out.with_suffix(out.suffix + ".report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    return report


def main() -> int:
    if len(sys.argv) != 3:
        print("usage: unity_mesh_to_crg.py INPUT.asset OUTPUT.crg", file=sys.stderr)
        return 2
    print(json.dumps(export_mesh(Path(sys.argv[1]), Path(sys.argv[2]))))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except (ValueError, OSError, struct.error) as exc:
        print(f"CRG export failed: {exc}", file=sys.stderr)
        raise SystemExit(1)
