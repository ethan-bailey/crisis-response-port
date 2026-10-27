"""Verify source identity and the explicit adapter-only decompiler edits.

Run with --archive to additionally compare against the pinned recovery ZIP.
No archive, binary assemblies, or downloaded package is committed by this tool.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import zipfile

ROOT = Path(__file__).resolve().parents[1]
PIN = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"


def sha(data):
    return hashlib.sha256(data).hexdigest()


def restore_source_aliases(data, aliases, insert_after=None):
    """Apply the exact compile aliases recorded for an archived source file."""
    if not aliases:
        return data
    encoded = [line.encode("utf-8") + b"\n" for line in aliases]
    insertion = b"".join(encoded)
    if insert_after is None:
        return insertion + b"\n" + data
    marker = insert_after.encode("utf-8") + b"\n\n"
    assert data.count(marker) == 1, f"source alias insertion marker missing or ambiguous: {insert_after}"
    return data.replace(marker, insert_after.encode("utf-8") + b"\n" + insertion + b"\n", 1)


def verify_source_adaptation(name, item, original, runtime):
    """Require the runtime source to be exactly the archived file plus aliases."""
    assert sha(original) == item["decompiledSha256"], f"archived source identity mismatch: {name}"
    expected = restore_source_aliases(
        original,
        item.get("sourceAdaptationAliases", []),
        item.get("sourceAdaptationInsertAfter"),
    )
    assert runtime == expected, f"source adaptation differs from exact archived original: {name}"


def normalize_ilspy_runtime(name, runtime):
    """Undo only the compile aliases recorded for the decompiled plugin sources."""
    aliases = (
        b"using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;\n",
        b"using Object = UnityEngine.Object;\n",
        b"using Task = BehaviorDesigner.Runtime.Tasks.Task;\n",
    )
    for prefix in aliases:
        if runtime.startswith(prefix):
            runtime = runtime[len(prefix):]
    if name in {"BehaviorDesigner.Runtime/ExternalBehavior.cs", "BehaviorDesigner.Runtime/Behavior.cs"}:
        assert runtime.count(b"\tint IBehavior.GetInstanceID()") == 1
        runtime = runtime.replace(b"\tint IBehavior.GetInstanceID()", b"\tvirtual int IBehavior.GetInstanceID()")
    return runtime


def verify_plugin_decompilation_delta(name, item, delta, normalized_hash):
    """Keep original pinned ILSpy identity distinct from intentional runtime fixes."""
    assert delta is not None, name
    assert delta["normalizedRuntimeSha256"] == normalized_hash, name
    status = delta["status"]
    if status == "unresolved: decompiled source snapshot omitted from recovery artifact":
        return "unresolved"
    if status == "reproduced: pinned ILSpy output matches assembly-derived SHA-256":
        assert delta["reproductionTool"] == "ilspycmd 9.1.0.7988", name
        assert delta["reproducedDecompiledSha256"] == item["decompiledSha256"], name
        return "reproduced"
    raise AssertionError(f"unknown plugin decompilation status for {name}: {status!r}")


def verify(archive_path=None, source_root=None):
    assert not (archive_path and source_root), "choose either --archive or --source-root"
    archive = None
    if archive_path:
        with open(archive_path, "rb") as stream:
            assert hashlib.file_digest(stream, "sha256").hexdigest() == PIN
        archive = zipfile.ZipFile(archive_path)
    source_root = Path(source_root) if source_root else None

    def recovered(entry):
        if archive:
            return archive.read(entry)
        if source_root:
            return (source_root / entry).read_bytes()
        return None
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
            archived = recovered(item["archiveEntry"])
            if archived is not None:
                assert data == archived, name
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
            elif item["adaptation"] == "FullSource namespace wrapper and Unity Object alias":
                alias = b"using Object = UnityEngine.Object;\n"
                assert original.startswith(alias), name
                original = original[len(alias):]
            elif item["adaptation"] == "FullSource namespace wrapper and Unity Random alias":
                alias = b"using Random = UnityEngine.Random;\n"
                assert original.startswith(alias), name
                original = original[len(alias):]
            elif item["adaptation"] == "FullSource namespace wrapper and armor bridge binding":
                original = original.replace(
                    b"FullSourceArmorBridge.PenetrationTest(type, damage, projectile)",
                    b"ArmorClass.PenetrationTest(type, damage, projectile)",
                )
                assert original.count(b"ArmorClass.PenetrationTest(type, damage, projectile)") == 1, name
            elif item["adaptation"] == "FullSource namespace wrapper and Casing partial modifier for browser adapter":
                original = original.replace(b"public partial class Casing", b"public class Casing")
                assert original.count(b"public class Casing") == 1, name
            elif item["adaptation"] == "FullSource namespace wrapper, weapon namespace import, Unity aliases and save-directory creation adapter":
                aliases = b"using Crisis.Runtime.Weapons;\nusing Random = UnityEngine.Random;\nusing Object = UnityEngine.Object;\n"
                assert original.startswith(aliases), name
                original = original[len(aliases):]
                adapter = b"\t\t\tstring directory = Path.GetDirectoryName(path);\n\t\t\tif (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);\n"
                assert original.count(adapter) == 1, name
                original = original.replace(adapter, b"")
            else:
                assert item["adaptation"] == "FullSource namespace wrapper only", name
            assert sha(original) == item["sha256"], name
            archived = recovered(item["archiveEntry"])
            if archived is not None:
                assert original == archived, name
            if "guid" in item or "metaSha256" in item:
                meta_path = full_source / (name + ".meta")
                meta = meta_path.read_bytes()
                assert sha(meta) == item["metaSha256"], f"{name}.meta"
                match = re.search(rb"(?m)^guid: ([0-9a-f]{32})\s*$", meta)
                assert match and match.group(1).decode("ascii") == item["guid"], f"{name}.meta GUID"
                archived = recovered(item["archiveEntry"] + ".meta")
                if archived is not None:
                    assert meta == archived, f"{name}.meta"
            count += 1

        for name, item in manifest.get("portAdapters", {}).items():
            data = (ROOT / item["runtimePath"]).read_bytes()
            assert sha(data) == item["sha256"], name
            assert item["kind"] == "browser adapter", name
            count += 1

        # Unity Standard Assets retain their original assembly namespace and
        # source bytes; they are not wrapped into the game FullSource namespace.
        for name, item in manifest.get("plugins", {}).items():
            data = (ROOT / item["runtimePath"]).read_bytes()
            assert sha(data) == item["runtimeSha256"], name
            alias_prefixes = {
                "Unity Standard Assets source and namespace preserved": b"",
                "Unity Standard Assets source and namespace preserved with Unity alias": (b"using Random = UnityEngine.Random;\n" if name == "NoiseAndGrain.cs" else b"using Object = UnityEngine.Object;\n"),
            }
            assert item["adaptation"] in alias_prefixes, name
            alias = alias_prefixes[item["adaptation"]]
            assert data.startswith(alias), name
            original = data[len(alias):]
            assert sha(original) == item["sha256"], name
            archived = recovered(item["archiveEntry"])
            if archived is not None:
                assert original == archived, name
            count += 1

        # Some game dependencies live in their own original namespaces (for
        # example Usage); preserve those identities instead of nesting them.
        for name, item in manifest.get("namespaceSources", {}).items():
            data = (ROOT / item["runtimePath"]).read_bytes()
            assert sha(data) == item["runtimeSha256"], name
            if item["adaptation"] == "Original namespace preserved with FullSource import and first-run client save":
                prefix = b"using Crisis.Runtime.FullSource;\n"
                assert data.startswith(prefix), name
                data = data[len(prefix):]
                adapter = (b"\t\t\ttry\n\t\t\t{\n\t\t\t\tclient.Load();\n\t\t\t}\n"
                           b"\t\t\tcatch (IOException)\n\t\t\t{\n\t\t\t\t// First launch has no persisted client record yet. Keep the authored\n"
                           b"\t\t\t\t// defaults and create the record through the normal save path.\n"
                           b"\t\t\t\tclient.Save();\n\t\t\t}")
                assert data.count(adapter) == 1, name
                data = data.replace(adapter, b"\t\t\tclient.Load();")
            elif item["adaptation"] == "Original namespace preserved with FullSource import":
                prefix = b"using Crisis.Runtime.FullSource;\n"
                assert data.startswith(prefix), name
                data = data[len(prefix):]
            elif item["adaptation"] == "Original namespace preserved with FullSource Version alias":
                prefix = b"using Version = Crisis.Runtime.FullSource.Version;\n"
                assert data.startswith(prefix), name
                data = data[len(prefix):]
            else:
                assert item["adaptation"] == "Original declared namespace preserved", name
            assert sha(data) == item["sha256"], name
            archived = recovered(item["archiveEntry"])
            if archived is not None:
                assert data == archived, name
            count += 1

        plugin = ROOT / "runtime/Original/BehaviorDesigner"
        manifest = json.loads((plugin / "source-provenance.json").read_text())
        assert manifest["archiveSha256"] == PIN
        assembly = recovered(manifest["assemblyEntry"])
        if assembly is not None:
            assert sha(assembly) == manifest["assemblySha256"]
        source_backed = set()
        plugin_reproduced = set()
        plugin_unresolved = set()
        for name, item in manifest["sources"].items():
            data = (ROOT / item["runtimePath"]).read_bytes()
            assert sha(data) == item["runtimeSha256"], name
            runtime_data = data
            normalized_runtime = normalize_ilspy_runtime(name, data)
            normalized_hash = sha(normalized_runtime)
            if normalized_hash != item["decompiledSha256"]:
                delta = manifest.get("decompilationDeltas", {}).get(name)
                source_entry = item.get("sourceArchiveEntry")
                if source_entry:
                    original = recovered(source_entry)
                    if original is not None:
                        verify_source_adaptation(name, item, original, runtime_data)
                        source_backed.add(name)
                if name not in source_backed:
                    state = verify_plugin_decompilation_delta(name, item, delta, normalized_hash)
                    if state == "reproduced":
                        plugin_reproduced.add(name)
                    else:
                        plugin_unresolved.add(name)
            else:
                assert name not in manifest.get("decompilationDeltas", {}), name
            count += 1
        for item in manifest["exportedWrappers"].values():
            data = (ROOT / item["runtimePath"]).read_bytes()
            assert sha(data) == item["sha256"]
            archived = recovered(item["archiveEntry"])
            if archived is not None:
                assert data == archived
            count += 1
        print(
            f"PASS: {count} hashed source identities and pinned plugin assembly; "
            f"{len(source_backed)} task bodies match exact archived game sources plus recorded aliases; "
            f"{len(plugin_reproduced)} plugin sources have reproduced pinned ILSpy output; "
            f"{len(plugin_unresolved)} plugin decompiler lineages remain unresolved"
        )
    finally:
        if archive:
            archive.close()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--archive", type=Path)
    parser.add_argument("--source-root", type=Path, help="root of the verified extracted recovery artifact")
    args = parser.parse_args()
    verify(args.archive, args.source_root)
