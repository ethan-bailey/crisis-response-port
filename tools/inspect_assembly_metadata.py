#!/usr/bin/env python3
"""Inventory ECMA-335 type/method names from a managed PE assembly.

This is a metadata reader, not a C# decompiler. It preserves the input file and
writes a TSV mapping each TypeDef to the MethodDef names assigned to it.
No third-party Python packages are required.
"""
from __future__ import annotations

import argparse
import struct
from pathlib import Path


def read_assembly(path: Path) -> list[tuple[str, list[str]]]:
    data = path.read_bytes()

    def u16(offset: int) -> int:
        return struct.unpack_from("<H", data, offset)[0]

    def u32(offset: int) -> int:
        return struct.unpack_from("<I", data, offset)[0]

    pe = u32(0x3C)
    if data[pe:pe + 4] != b"PE\0\0":
        raise ValueError("Input is not a PE file")
    section_count = u16(pe + 6)
    optional_size = u16(pe + 20)
    optional = pe + 24
    magic = u16(optional)
    if magic == 0x10B:
        directory_base = optional + 96
    elif magic == 0x20B:
        directory_base = optional + 112
    else:
        raise ValueError(f"Unsupported PE optional-header magic: {magic:#x}")

    cli_rva = u32(directory_base + 14 * 8)
    section_header = optional + optional_size
    sections: list[tuple[int, int, int]] = []
    for i in range(section_count):
        pos = section_header + i * 40
        virtual_size, virtual_address, raw_size, raw_pointer = struct.unpack_from(
            "<IIII", data, pos + 8
        )
        sections.append((virtual_address, max(virtual_size, raw_size), raw_pointer))

    def rva_to_offset(rva: int) -> int:
        for virtual_address, size, raw_pointer in sections:
            if virtual_address <= rva < virtual_address + size:
                return raw_pointer + rva - virtual_address
        raise ValueError(f"RVA {rva:#x} does not map to a PE section")

    cli = rva_to_offset(cli_rva)
    metadata = rva_to_offset(u32(cli + 8))
    if data[metadata:metadata + 4] != b"BSJB":
        raise ValueError("CLI metadata signature BSJB not found")

    version_length = u32(metadata + 12)
    cursor = (metadata + 16 + version_length + 3) & ~3
    _, stream_count = struct.unpack_from("<HH", data, cursor)
    cursor += 4
    streams: dict[str, tuple[int, int]] = {}
    for _ in range(stream_count):
        stream_offset, stream_size = struct.unpack_from("<II", data, cursor)
        cursor += 8
        name_end = data.index(b"\0", cursor)
        name = data[cursor:name_end].decode("ascii", "replace")
        cursor = (name_end + 4) & ~3
        streams[name] = (metadata + stream_offset, stream_size)

    table_start, _ = streams["#~"]
    cursor = table_start + 4
    _, _, heap_sizes, _ = struct.unpack_from("<BBBB", data, cursor)
    cursor += 4
    valid_mask = struct.unpack_from("<Q", data, cursor)[0]
    cursor += 8
    cursor += 8  # Sorted mask
    row_counts: dict[int, int] = {}
    for table_id in range(64):
        if valid_mask & (1 << table_id):
            row_counts[table_id] = u32(cursor)
            cursor += 4

    string_size = 4 if heap_sizes & 1 else 2
    guid_size = 4 if heap_sizes & 2 else 2
    blob_size = 4 if heap_sizes & 4 else 2

    def table_index_size(table_id: int) -> int:
        return 4 if row_counts.get(table_id, 0) >= 65536 else 2

    def coded_index_size(table_ids: list[int], tag_bits: int) -> int:
        largest = max((row_counts.get(t, 0) for t in table_ids), default=0)
        return 4 if largest >= (1 << (16 - tag_bits)) else 2

    # ECMA-335 metadata table row sizes required to reach TypeDef and MethodDef.
    row_sizes = {
        0: 2 + string_size + 3 * guid_size,  # Module
        1: coded_index_size([0, 26, 35, 1], 2) + 2 * string_size,  # TypeRef
        2: 4 + 2 * string_size + coded_index_size([2, 1, 27], 2)
        + table_index_size(4) + table_index_size(6),  # TypeDef
        3: table_index_size(4),  # FieldPtr
        4: 2 + string_size + blob_size,  # Field
        5: table_index_size(6),  # MethodPtr
        6: 8 + string_size + blob_size + table_index_size(8),  # MethodDef
    }
    table_offsets: dict[int, int] = {}
    offset = cursor
    for table_id in sorted(row_counts):
        if table_id > 6:
            break
        table_offsets[table_id] = offset
        offset += row_counts[table_id] * row_sizes[table_id]

    strings_offset, strings_size = streams["#Strings"]
    strings = data[strings_offset:strings_offset + strings_size]

    def get_string(index: int) -> str:
        if index == 0:
            return ""
        end = strings.find(b"\0", index)
        if end < 0:
            raise ValueError("Unterminated string heap entry")
        return strings[index:end].decode("utf-8", "replace")

    def heap_index(offset: int, size: int) -> int:
        return int.from_bytes(data[offset:offset + size], "little")

    # TypeDef rows store the first Field and Method row owned by that type.
    type_defs: list[tuple[str, int]] = []
    pos = table_offsets[2]
    type_row_size = row_sizes[2]
    extends_size = coded_index_size([2, 1, 27], 2)
    for _ in range(row_counts.get(2, 0)):
        name_index = heap_index(pos + 4, string_size)
        namespace_index = heap_index(pos + 4 + string_size, string_size)
        lists_offset = pos + 4 + 2 * string_size + extends_size
        method_list = heap_index(
            lists_offset + table_index_size(4), table_index_size(6)
        )
        namespace = get_string(namespace_index)
        name = get_string(name_index)
        type_defs.append(((namespace + "." if namespace else "") + name, method_list))
        pos += type_row_size

    method_defs: list[str] = []
    pos = table_offsets[6]
    method_row_size = row_sizes[6]
    for _ in range(row_counts.get(6, 0)):
        name_index_offset = pos + 8
        method_defs.append(get_string(heap_index(name_index_offset, string_size)))
        pos += method_row_size

    result: list[tuple[str, list[str]]] = []
    for index, (type_name, first_method) in enumerate(type_defs):
        next_method = (
            type_defs[index + 1][1]
            if index + 1 < len(type_defs)
            else len(method_defs) + 1
        )
        names = method_defs[first_method - 1:next_method - 1] if first_method else []
        result.append((type_name, names))
    return result


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("assembly", type=Path, help="Path to a managed .NET/Mono PE assembly")
    parser.add_argument("-o", "--output", type=Path, help="Output TSV path (default: stdout)")
    args = parser.parse_args()
    rows = read_assembly(args.assembly)
    output = args.output.open("w", encoding="utf-8", newline="") if args.output else None
    import sys
    stream = output or sys.stdout
    try:
        stream.write("Type\tMethodCount\tMethods\n")
        for type_name, methods in rows:
            stream.write(f"{type_name}\t{len(methods)}\t{'; '.join(methods)}\n")
    finally:
        if output:
            output.close()
    print(f"Inspected {len(rows):,} type definitions from {args.assembly}", file=sys.stderr)


if __name__ == "__main__":
    main()
