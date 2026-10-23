#!/usr/bin/env python3
"""Safely extract the original Windows Unity player's Data directory from its RAR5 archive.

Requires the system libarchive shared library. Only regular files beneath
``Crisis Response/Crisis Response_Data/`` are written. The known archive SHA is
checked by default; extracted bytes are recorded in a deterministic manifest.
"""
from __future__ import annotations

import argparse
import ctypes
import ctypes.util
import hashlib
import json
import os
import stat
import sys
from pathlib import Path, PurePosixPath

ARCHIVE_SHA256 = "5a12ff494b26713d149f9be026619732247f817ddf8944a399b6ee1d12ea0ab3"
PLAYER_PREFIX = "Crisis Response/Crisis Response_Data/"
BUFFER_SIZE = 1024 * 1024


def _libarchive() -> ctypes.CDLL:
    library = ctypes.util.find_library("archive")
    if not library:
        raise RuntimeError("libarchive shared library is required to read the original RAR5 archive")
    api = ctypes.CDLL(library)
    api.archive_read_new.restype = ctypes.c_void_p
    signatures = {
        "archive_read_support_filter_all": ([ctypes.c_void_p], ctypes.c_int),
        "archive_read_support_format_all": ([ctypes.c_void_p], ctypes.c_int),
        "archive_read_open_filename": ([ctypes.c_void_p, ctypes.c_char_p, ctypes.c_size_t], ctypes.c_int),
        "archive_read_next_header": ([ctypes.c_void_p, ctypes.POINTER(ctypes.c_void_p)], ctypes.c_int),
        "archive_entry_pathname": ([ctypes.c_void_p], ctypes.c_char_p),
        "archive_entry_filetype": ([ctypes.c_void_p], ctypes.c_uint),
        "archive_entry_symlink": ([ctypes.c_void_p], ctypes.c_char_p),
        "archive_entry_hardlink": ([ctypes.c_void_p], ctypes.c_char_p),
        "archive_read_data": ([ctypes.c_void_p, ctypes.c_void_p, ctypes.c_size_t], ctypes.c_longlong),
        "archive_read_data_skip": ([ctypes.c_void_p], ctypes.c_int),
        "archive_error_string": ([ctypes.c_void_p], ctypes.c_char_p),
        "archive_read_free": ([ctypes.c_void_p], ctypes.c_int),
    }
    for name, (arguments, result) in signatures.items():
        function = getattr(api, name)
        function.argtypes = arguments
        function.restype = result
    return api


def _digest(path: Path) -> str:
    value = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(BUFFER_SIZE), b""):
            value.update(chunk)
    return value.hexdigest()


def _safe_target(root: Path, relative: str) -> Path:
    parsed = PurePosixPath(relative)
    if parsed.is_absolute() or not parsed.parts or any(part in ("", ".", "..") for part in parsed.parts):
        raise RuntimeError(f"Unsafe archive path rejected: {relative!r}")
    target = root.joinpath(*parsed.parts)
    current = root
    for part in parsed.parts[:-1]:
        current = current / part
        if current.is_symlink():
            raise RuntimeError(f"Refusing to write through an existing symlink: {current}")
        current.mkdir(exist_ok=True)
    if target.is_symlink():
        raise RuntimeError(f"Refusing to replace an existing symlink: {target}")
    if root not in target.resolve(strict=False).parents:
        raise RuntimeError(f"Archive path escaped extraction root: {relative!r}")
    return target


def extract(archive_path: Path, destination: Path, expected_sha256: str) -> dict[str, object]:
    actual_archive_sha = _digest(archive_path)
    if actual_archive_sha != expected_sha256:
        raise RuntimeError(
            f"Original archive SHA-256 mismatch: expected {expected_sha256}, got {actual_archive_sha}"
        )

    api = _libarchive()
    reader = api.archive_read_new()
    if not reader:
        raise RuntimeError("libarchive could not allocate a reader")
    destination.mkdir(parents=True, exist_ok=True)
    root = (destination / "player-data").resolve()
    root.mkdir(parents=True, exist_ok=True)
    if api.archive_read_support_filter_all(reader) < 0 or api.archive_read_support_format_all(reader) < 0:
        raise RuntimeError("libarchive could not enable RAR5 reader support")
    if api.archive_read_open_filename(reader, os.fsencode(archive_path), 65536) < 0:
        message = api.archive_error_string(reader)
        raise RuntimeError(message.decode("utf-8", "replace") if message else "Could not open archive")

    files: list[dict[str, object]] = []
    total_bytes = 0
    entry = ctypes.c_void_p()
    buffer = ctypes.create_string_buffer(BUFFER_SIZE)
    try:
        while True:
            status = api.archive_read_next_header(reader, ctypes.byref(entry))
            if status == 1:  # ARCHIVE_EOF
                break
            if status < 0:
                message = api.archive_error_string(reader)
                raise RuntimeError(message.decode("utf-8", "replace") if message else "Archive read failed")

            raw_name = api.archive_entry_pathname(entry)
            name = raw_name.decode("utf-8", "strict") if raw_name else ""
            if not name.startswith(PLAYER_PREFIX) or not stat.S_ISREG(api.archive_entry_filetype(entry)):
                api.archive_read_data_skip(reader)
                continue
            if api.archive_entry_symlink(entry) or api.archive_entry_hardlink(entry):
                api.archive_read_data_skip(reader)
                continue

            relative = name[len(PLAYER_PREFIX):]
            target = _safe_target(root, relative)
            temporary = target.with_name(target.name + ".extracting")
            if temporary.exists():
                raise RuntimeError(f"Temporary output already exists; inspect it before retrying: {temporary}")
            digest = hashlib.sha256()
            size = 0
            try:
                with temporary.open("xb") as output:
                    while True:
                        count = api.archive_read_data(reader, buffer, len(buffer))
                        if count == 0:
                            break
                        if count < 0:
                            message = api.archive_error_string(reader)
                            raise RuntimeError(message.decode("utf-8", "replace") if message else "Entry read failed")
                        block = buffer.raw[:count]
                        output.write(block)
                        digest.update(block)
                        size += count
                if target.exists():
                    if not target.is_file() or _digest(target) != digest.hexdigest():
                        raise RuntimeError(f"Existing extraction differs from the archive entry: {target}")
                    temporary.unlink()
                else:
                    temporary.replace(target)
            except Exception:
                temporary.unlink(missing_ok=True)
                raise
            files.append({"path": relative, "bytes": size, "sha256": digest.hexdigest()})
            total_bytes += size
    finally:
        api.archive_read_free(reader)

    files.sort(key=lambda item: str(item["path"]))
    manifest = {
        "archive": archive_path.name,
        "archiveSha256": actual_archive_sha,
        "format": "RAR5",
        "playerDataPrefix": PLAYER_PREFIX,
        "files": files,
        "fileCount": len(files),
        "totalBytes": total_bytes,
    }
    manifest_path = destination / "player-data-extraction-manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2, sort_keys=True) + "\n", encoding="utf-8")
    return {"status": "pass", "manifest": str(manifest_path), "fileCount": len(files), "totalBytes": total_bytes}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("archive", type=Path, help="Path to the verified Crisis Response.rar")
    parser.add_argument("destination", type=Path, help="Output directory; writes only beneath player-data/")
    parser.add_argument("--expected-sha256", default=ARCHIVE_SHA256)
    args = parser.parse_args()
    try:
        print(json.dumps(extract(args.archive, args.destination, args.expected_sha256), sort_keys=True))
    except (OSError, RuntimeError, UnicodeError) as error:
        print(f"ERROR: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
