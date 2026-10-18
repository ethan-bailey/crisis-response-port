"""Verify source identity and the explicit adapter-only decompiler edits.

Run with --archive to additionally compare against the pinned recovery ZIP.
No archive, binary assemblies, or downloaded package is committed by this tool.
"""
import argparse
import hashlib
import json
from pathlib import Path
import zipfile

ROOT = Path(__file__).resolve().parents[1]
PIN = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"


def sha(data):
    return hashlib.sha256(data).hexdigest()


def verify(archive_path=None):
    archive = None
    if archive_path:
        with open(archive_path, "rb") as stream:
            assert hashlib.file_digest(stream, "sha256").hexdigest() == PIN
        archive = zipfile.ZipFile(archive_path)
    try:
        foundation = ROOT / "runtime/Original/Foundation"
        manifest = json.loads((foundation / "source-provenance.json").read_text())
        assert manifest["archiveSha256"] == PIN
        count = 0
        for name, item in manifest["scripts"].items():
            data = (foundation / name).read_bytes()
            assert sha(data) == item["runtimeSha256"], name
            if item["adaptation"] == "Random alias only":
                prefix = b"using Random = UnityEngine.Random;\n"
                assert data.startswith(prefix), name
                data = data[len(prefix):]
            else:
                assert item["adaptation"] == "none", name
            assert sha(data) == item["sha256"], name
            if archive:
                assert data == archive.read(item["archiveEntry"]), name
            count += 1

        full_source = ROOT / "runtime/Original/FullSource"
        manifest = json.loads((full_source / "dependency-source-provenance.json").read_text())
        assert manifest["archiveSha256"] == PIN
        for name, item in manifest["scripts"].items():
            data = (full_source / name).read_bytes()
            assert sha(data) == item["runtimeSha256"], name
            prefix, suffix = b"namespace Crisis.Runtime.FullSource\n{\n", b"\n}\n"
            assert data.startswith(prefix) and data.endswith(suffix), name
            original = data[len(prefix):-len(suffix)]
            if item["adaptation"] == "FullSource namespace wrapper, weapon namespace import and Unity Random/Object aliases":
                aliases = b"using Crisis.Runtime.Weapons;\nusing Random = UnityEngine.Random;\nusing Object = UnityEngine.Object;\n"
                assert original.startswith(aliases), name
                original = original[len(aliases):]
            elif item["adaptation"] == "FullSource namespace wrapper and Unity Random/Object aliases":
                aliases = b"using Random = UnityEngine.Random;\nusing Object = UnityEngine.Object;\n"
                assert original.startswith(aliases), name
                original = original[len(aliases):]
            else:
                assert item["adaptation"] == "FullSource namespace wrapper only", name
            assert sha(original) == item["sha256"], name
            if archive:
                assert original == archive.read(item["archiveEntry"]), name
            count += 1

        # Unity Standard Assets retain their original assembly namespace and
        # source bytes; they are not wrapped into the game FullSource namespace.
        for name, item in manifest.get("plugins", {}).items():
            data = (ROOT / item["runtimePath"]).read_bytes()
            assert sha(data) == item["runtimeSha256"], name
            assert item["adaptation"] == "Unity Standard Assets source and namespace preserved", name
            assert sha(data) == item["sha256"], name
            if archive:
                assert data == archive.read(item["archiveEntry"]), name
            count += 1

        plugin = ROOT / "runtime/Original/BehaviorDesigner"
        manifest = json.loads((plugin / "source-provenance.json").read_text())
        assert manifest["archiveSha256"] == PIN
        if archive:
            assert sha(archive.read(manifest["assemblyEntry"])) == manifest["assemblySha256"]
        aliases = (
            b"using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;\n",
            b"using Object = UnityEngine.Object;\n",
            b"using Task = BehaviorDesigner.Runtime.Tasks.Task;\n",
        )
        for name, item in manifest["sources"].items():
            data = (ROOT / item["runtimePath"]).read_bytes()
            assert sha(data) == item["runtimeSha256"], name
            for prefix in aliases:
                if data.startswith(prefix):
                    data = data[len(prefix):]
            if name in {"BehaviorDesigner.Runtime/ExternalBehavior.cs", "BehaviorDesigner.Runtime/Behavior.cs"}:
                assert data.count(b"\tint IBehavior.GetInstanceID()") == 1
                data = data.replace(b"\tint IBehavior.GetInstanceID()", b"\tvirtual int IBehavior.GetInstanceID()")
            assert sha(data) == item["decompiledSha256"], name
            count += 1
        for item in manifest["exportedWrappers"].values():
            data = (ROOT / item["runtimePath"]).read_bytes()
            assert sha(data) == item["sha256"]
            if archive:
                assert data == archive.read(item["archiveEntry"])
            count += 1
        print(f"PASS: {count} original/decompiled source identities; pinned plugin assembly; no recovered rule-body edits")
    finally:
        if archive:
            archive.close()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--archive", type=Path)
    verify(parser.parse_args().archive)
