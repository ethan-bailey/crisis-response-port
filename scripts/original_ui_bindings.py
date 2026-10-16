#!/usr/bin/env python3
"""Identify original Unity UI script types without loading/executing the DLL.

PE/CLI TypeDef layouts: ECMA-335 II.22.2, II.22.30, II.22.37, II.24.
File-ID algorithm and reference vectors: AssetRipper ScriptHashing.cs and
ScriptHashingTests.cs at beeb50ee1c223e856c860caa338e5309404956a3.
This is extraction/identity evidence only, not a Unity UI runtime adapter.
"""
import argparse
import collections
import functools
import hashlib
import json
import struct
import subprocess
import zipfile
from pathlib import Path

ARCHIVE_SHA = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
UI_SHA = "30cf854b3db333ba8a3ce47c2091ea11387509861654d8209c1879f7bcb27eeb"
UI_PATH = "export/AuxiliaryFiles/GameAssemblies/UnityEngine.UI.dll"
UI_GUID = "f5f67c52d1564df4a8936ccd202a3bd8"
REFERENCE_COMMIT = "beeb50ee1c223e856c860caa338e5309404956a3"


def require(condition, message):
    if not condition:
        raise ValueError(message)


def typedefs(data):
    """Read only TypeDef identity/flags; no IL execution or dependency loading."""
    def number(fmt, offset):
        size = struct.calcsize(fmt)
        require(0 <= offset <= len(data) - size, "Truncated PE/CLI structure")
        return struct.unpack_from(fmt, data, offset)[0]

    require(data[:2] == b"MZ", "Not a PE image")
    pe = number("<I", 0x3C)
    require(data[pe:pe + 4] == b"PE\0\0", "Invalid PE signature")
    count, optional_size = number("<H", pe + 6), number("<H", pe + 20)
    optional = pe + 24
    magic = number("<H", optional)
    require(magic in (0x10B, 0x20B), "Unsupported PE optional header")
    directory = optional + (96 if magic == 0x10B else 112)
    require(directory + 15 * 8 <= optional + optional_size, "CLI directory missing")
    cli_rva = number("<I", directory + 14 * 8)

    def rva_offset(rva, length):
        for index in range(count):
            section = optional + optional_size + index * 40
            virtual_size = number("<I", section + 8)
            virtual_rva = number("<I", section + 12)
            raw_size = number("<I", section + 16)
            raw_start = number("<I", section + 20)
            delta = rva - virtual_rva
            if 0 <= delta < max(virtual_size, raw_size):
                require(delta + length <= raw_size, "RVA enters uninitialized section")
                offset = raw_start + delta
                require(offset + length <= len(data), "RVA leaves image")
                return offset
        raise ValueError("RVA has no section")

    cli = rva_offset(cli_rva, 16)
    metadata_size = number("<I", cli + 12)
    metadata = rva_offset(number("<I", cli + 8), metadata_size)
    end = metadata + metadata_size
    require(data[metadata:metadata + 4] == b"BSJB", "Invalid CLI metadata signature")
    version_length = number("<I", metadata + 12)
    cursor = (metadata + 16 + version_length + 3) & ~3
    require(cursor + 4 <= end, "Invalid metadata version length")
    streams = {}
    stream_count = number("<H", cursor + 2)
    cursor += 4
    for _ in range(stream_count):
        relative, size = number("<I", cursor), number("<I", cursor + 4)
        name_start = cursor + 8
        name_end = data.find(b"\0", name_start, min(name_start + 32, end))
        require(name_end >= 0, "Unterminated metadata stream name")
        name = data[name_start:name_end].decode("ascii")
        require(name not in streams and metadata + relative + size <= end,
                "Duplicate/out-of-bounds metadata stream")
        streams[name] = (metadata + relative, size)
        cursor = (name_end + 4) & ~3
    require("#Strings" in streams and ("#~" in streams or "#-" in streams),
            "Required CLI heaps/tables absent")
    tables, table_size = streams.get("#~", streams.get("#-"))
    table_end = tables + table_size
    require(table_size >= 24, "Truncated table stream")
    heap_flags, valid = number("<B", tables + 6), number("<Q", tables + 8)
    cursor = tables + 24
    rows = [0] * 64
    for table in range(64):
        if valid & (1 << table):
            require(cursor + 4 <= table_end, "Truncated row counts")
            rows[table] = number("<I", cursor)
            cursor += 4
    string_size = 4 if heap_flags & 1 else 2
    guid_size = 4 if heap_flags & 2 else 2

    def index_size(table):
        return 4 if rows[table] >= 65536 else 2

    def coded_size(tables, tag_bits):
        return 4 if max(rows[t] for t in tables) >= (1 << (16 - tag_bits)) else 2

    module_size = 2 + string_size + 3 * guid_size
    type_ref_size = coded_size((0, 26, 35, 1), 2) + 2 * string_size
    cursor += rows[0] * module_size + rows[1] * type_ref_size
    type_def_size = (4 + 2 * string_size + coded_size((2, 1, 27), 2)
                     + index_size(3 if rows[3] else 4) + index_size(5 if rows[5] else 6))
    require(rows[2] > 0 and cursor + rows[2] * type_def_size <= table_end,
            "Invalid/truncated TypeDef table")
    string_start, string_length = streams["#Strings"]

    def string(index):
        require(index < string_length, "String index leaves heap")
        start = string_start + index
        stop = data.find(b"\0", start, string_start + string_length)
        require(stop >= 0, "Unterminated metadata string")
        return data[start:stop].decode("utf-8")

    read_index = "<I" if string_size == 4 else "<H"
    result = []
    for row in range(1, rows[2] + 1):
        flags = number("<I", cursor)
        name = string(number(read_index, cursor + 4))
        namespace = string(number(read_index, cursor + 4 + string_size))
        result.append({"metadataToken": f"0x{0x02000000 + row:08x}",
                       "namespace": namespace, "name": name, "flags": flags,
                       "topLevel": flags & 7 in (0, 1) and name != "<Module>"})
        cursor += type_def_size
    return result


@functools.lru_cache(maxsize=4096)
def script_file_id(namespace, name):
    # Same byte prefix, UTF-8 concatenation and signed little-endian Int32 as
    # AssetRipper's original reference implementation. MD4 is an identity format.
    value = b"s\0\0\0" + namespace.encode("utf-8") + name.encode("utf-8")
    result = subprocess.run(["openssl", "dgst", "-md4", "-provider", "legacy", "-binary"],
                            input=value, capture_output=True, check=True)
    require(len(result.stdout) == 16, "Unexpected MD4 digest size")
    return struct.unpack("<i", result.stdout[:4])[0]


def verify_reference_vectors():
    for namespace, name, expected in (
        ("AssetRipper.Examples", "ExampleBehaviour", -218309758),
        ("UnityEngine", "Component", 468326297),
        ("UnityEngine", "GameObject", -1741091355),
        ("UnityEngine", "MonoBehaviour", 888015496),
        ("UnityEngine", "Object", 350709172),
    ):
        require(script_file_id(namespace, name) == expected, "Reference file-ID vector mismatch")


def identify_ui(archive, closure):
    with archive.open("rb") as stream:
        archive_hash = hashlib.file_digest(stream, "sha256").hexdigest()
    require(archive_hash == ARCHIVE_SHA, "Original recovery archive SHA256 mismatch")
    with zipfile.ZipFile(archive) as z:
        data = z.read(UI_PATH)
    require(hashlib.sha256(data).hexdigest() == UI_SHA, "Original UI assembly SHA256 mismatch")
    verify_reference_vectors()
    types = typedefs(data)
    candidates = collections.defaultdict(list)
    for item in types:
        if item["topLevel"]:
            file_id = script_file_id(item["namespace"], item["name"])
            candidates[str(file_id)].append(item)
    required = sorted({ref["fileID"] for scene in closure["scenes"]
                       for ref in scene["unresolvedReferences"] if ref["guid"] == UI_GUID}, key=int)
    require(required, "Closure contains no unresolved original UI references")
    bindings = {}
    for file_id in required:
        found = candidates.get(file_id, [])
        require(len(found) == 1, f"Original UI type ID missing/ambiguous: {file_id}")
        item = found[0]
        bindings[file_id] = {"fileID": file_id, "guid": UI_GUID, **item,
                             "fullName": (item["namespace"] + "." if item["namespace"] else "") + item["name"],
                             "runtimeBehaviorImplemented": False}
    serialized = json.dumps(closure, indent=2, sort_keys=True) + "\n"
    return {"schemaVersion": 1, "assemblyName": "UnityEngine.UI", "assemblyGuid": UI_GUID,
            "archiveSha256": archive_hash, "assemblyArchivePath": UI_PATH, "assemblySha256": UI_SHA,
            "assemblyBytes": len(data), "metadataTypes": len(types),
            "inputClosureSha256": hashlib.sha256(serialized.encode()).hexdigest(),
            "algorithmSource": f"https://github.com/AssetRipper/AssetRipper/blob/{REFERENCE_COMMIT}/Source/AssetRipper.Export.UnityProjects/Scripts/ScriptHashing.cs",
            "referenceVectorsPassed": 5, "bindings": bindings,
            "assemblyGuidEvidence": "Observed original serialized GUID; required IDs uniquely match the pinned original assembly types",
            "scope": {"typeIdentityValidated": True, "managedAssemblyExecuted": False,
                      "uiRuntimeImplemented": False, "nativeParityVerified": False}}


def annotate_closure(closure, identity):
    bindings = identity["bindings"]
    for record in closure["assets"].values():
        for ref in record["references"]:
            if ref["guid"] == UI_GUID:
                require(ref["fileID"] in bindings, "UI reference lacks verified type identity")
                ref.update({"status": "originalAssemblyTypeIdentified", "targets": [],
                            "type": bindings[ref["fileID"]]["fullName"],
                            "exportedMetaPresent": False, "runtimeBehaviorImplemented": False})
    for scene in closure["scenes"]:
        scene["originalAssemblyReferences"] = []
        remaining = []
        for ref in scene["unresolvedReferences"]:
            if ref["guid"] == UI_GUID:
                require(ref["fileID"] in bindings, "Unmatched original UI component ID")
                scene["originalAssemblyReferences"].append({**ref,
                    "status": "originalAssemblyTypeIdentified", "exportedMetaPresent": False,
                    "type": bindings[ref["fileID"]]["fullName"], "runtimeBehaviorImplemented": False})
            else:
                remaining.append(ref)
        scene["unresolvedReferences"] = remaining
    closure["originalAssemblyBindings"] = identity
    closure["summary"]["identifiedOriginalUIComponentTypes"] = len(bindings)
    closure["summary"]["unresolvedExternalGuids"] = len({ref["guid"] for s in closure["scenes"] for ref in s["unresolvedReferences"]})
    return closure


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("archive", type=Path)
    parser.add_argument("closure", type=Path)
    parser.add_argument("output", type=Path)
    args = parser.parse_args()
    identity = identify_ui(args.archive, json.loads(args.closure.read_text()))
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(identity, sort_keys=True, indent=2) + "\n")
    print(json.dumps({"identifiedTypes": len(identity["bindings"]), "assemblySha256": UI_SHA}))


if __name__ == "__main__":
    main()
