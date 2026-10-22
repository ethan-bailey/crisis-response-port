#!/usr/bin/env python3
"""Recover exact Operator head Texture2D sources and importer metadata."""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import struct
import zipfile
from pathlib import Path

ARCHIVE_SHA256 = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
PROJECT = "export/ExportedProject/"
ASSET_ROOT = "Assets/Texture2D/"
ACTOR_PREFAB = "Assets/Resources/characters/Operator.prefab"
HEAD_NAMES = [f"head {index}.png" for index in range(1, 6)]
ROOT = Path(__file__).resolve().parents[1]
DESTINATION = ROOT / "source-reference/Original/Assets/Texture2D"
REFERENCE_FILE_ID = "2800000"
HEAD_RENDERER_GAME_OBJECT_FILE_ID = "1589667821646929"


def sha256(raw: bytes) -> str:
    return hashlib.sha256(raw).hexdigest()


def importer_fields(meta: bytes) -> tuple[str, dict]:
    text = meta.decode("utf-8")

    def match(pattern: str) -> str:
        found = re.search(pattern, text, re.M)
        if not found:
            raise ValueError(f"Original TextureImporter metadata lacks required field: {pattern}")
        return found.group(1).strip()

    guid = match(r"^guid:\s*([0-9a-f]{32})$")
    fields = {
        "fileIDToRecycleName": match(r"^  fileIDToRecycleName:\s*(.+)$"),
        "isReadable": int(match(r"^  isReadable:\s*(\d+)$")),
        "textureFormat": int(match(r"^  textureFormat:\s*(\d+)$")),
        "maxTextureSize": int(match(r"^  maxTextureSize:\s*(\d+)$")),
        "textureType": int(match(r"^  textureType:\s*(\d+)$")),
        "textureShape": int(match(r"^  textureShape:\s*(\d+)$")),
    }
    expected = {"fileIDToRecycleName": "{}", "isReadable": 1,
                "maxTextureSize": 2048, "textureType": 0, "textureShape": 1}
    if any(fields[key] != value for key, value in expected.items()):
        raise ValueError(f"Unsupported original head TextureImporter settings for GUID {guid}")
    if fields["textureFormat"] not in (4, 12):
        raise ValueError(f"Unsupported original head TextureImporter format {fields['textureFormat']} for GUID {guid}")
    return guid, fields


def recover(archive_path: Path, destination: Path) -> dict:
    archive_bytes = archive_path.read_bytes()
    archive_hash = sha256(archive_bytes)
    if archive_hash != ARCHIVE_SHA256:
        raise ValueError(f"Unexpected recovery archive SHA-256: {archive_hash}")

    with zipfile.ZipFile(archive_path) as archive:
        prefab = archive.read(PROJECT + ACTOR_PREFAB).decode("utf-8")
        matches = list(re.finditer(r"^--- !u!(\d+) &(\d+).*?$", prefab, re.M))
        head_skin_block = None
        for index, match in enumerate(matches):
            end = matches[index + 1].start() if index + 1 < len(matches) else len(prefab)
            body = prefab[match.end():end]
            game_object = re.search(r"^  m_GameObject: \{fileID: (\d+)\}$", body, re.M)
            if match.group(1) == "114" and game_object and game_object.group(1) == HEAD_RENDERER_GAME_OBJECT_FILE_ID:
                head_skin_block = re.search(r"^  skins:\n((?:  - .*\n)+)", body, re.M)
                break
        if not head_skin_block:
            raise ValueError("Could not locate the original Operator head LimbRenderer skin list")
        references = re.findall(
            r"\{fileID:\s*(\d+),\s*guid:\s*([0-9a-f]{32}),\s*type:\s*3\}", head_skin_block.group(1))
        if any(file_id != REFERENCE_FILE_ID for file_id, _ in references):
            raise ValueError("Operator head skins do not all use the expected serialized Texture2D fileID")
        texture_refs = {guid for _, guid in references}
        records = []
        source_files = {}
        for name in HEAD_NAMES:
            asset_path = ASSET_ROOT + name
            meta_path = asset_path + ".meta"
            png = archive.read(PROJECT + asset_path)
            meta = archive.read(PROJECT + meta_path)
            guid, fields = importer_fields(meta)
            if guid not in texture_refs:
                raise ValueError(f"Operator.prefab does not reference {asset_path} at Texture2D fileID {REFERENCE_FILE_ID}")
            if png[:8] != b"\x89PNG\r\n\x1a\n" or png[12:16] != b"IHDR":
                raise ValueError(f"Original head texture is not a PNG: {asset_path}")
            width, height = struct.unpack(">II", png[16:24])
            record = {
                "path": asset_path,
                "metaPath": meta_path,
                "guid": guid,
                "fileID": REFERENCE_FILE_ID,
                "className": "Texture2D",
                "bytes": len(png),
                "sha256": sha256(png),
                "metaBytes": len(meta),
                "metaSha256": sha256(meta),
                "width": width,
                "height": height,
                "importer": fields,
                "runtimeDisposition": "Original PNG decoded to CPU RGBA32; exact source GUID/fileID/importer bytes are retained. Unity platform-compressed output is not claimed.",
            }
            records.append(record)
            source_files[name] = png
            source_files[name + ".meta"] = meta

    missing = sorted(texture_refs - {record["guid"] for record in records})
    if missing:
        raise ValueError(f"Operator head Texture2D references have no recovered source image/meta: {missing}")
    runtime_copy = ROOT / "game/assets/operator/skins/skin_004.png"
    runtime_copy_record = None
    if runtime_copy.is_file():
        copied_hash = sha256(runtime_copy.read_bytes())
        original_head_hash = next(record["sha256"] for record in records if record["path"].endswith("head 1.png"))
        if copied_hash != original_head_hash:
            raise ValueError("Operator skin_004.png differs from the original head 1.png source bytes")
        runtime_copy_record = {"path": str(runtime_copy.relative_to(ROOT)), "sha256": copied_hash,
                               "matchesOriginalHeadTexture": "head 1.png"}
    manifest = {
        "schemaVersion": 1,
        "sourceArchive": "crisis-response-recovery-original-source.zip",
        "sourceArchiveSha256": archive_hash,
        "sourcePrefab": ACTOR_PREFAB,
        "records": records,
        "runtimeCopy": runtime_copy_record,
    }
    return {**source_files, "Head-Texture2D-provenance.json":
            (json.dumps(manifest, indent=2, ensure_ascii=False) + "\n").encode()}


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--archive", required=True, type=Path)
    parser.add_argument("--destination", type=Path, default=DESTINATION)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    expected = recover(args.archive, args.destination)
    if args.check:
        for name, data in expected.items():
            actual_path = args.destination / name
            if not actual_path.is_file() or actual_path.read_bytes() != data:
                raise ValueError(f"Recovered Operator head source differs from pinned archive: {actual_path}")
    else:
        args.destination.mkdir(parents=True, exist_ok=True)
        for name, data in expected.items():
            (args.destination / name).write_bytes(data)
    print(json.dumps({"status": "pass", "archiveSha256": ARCHIVE_SHA256,
                      "textures": len(HEAD_NAMES), "sourceTexture2DFileID": REFERENCE_FILE_ID,
                      "formats": [record["importer"]["textureFormat"] for record in json.loads(
                          expected["Head-Texture2D-provenance.json"])["records"]]}, sort_keys=True))


if __name__ == "__main__":
    main()
