#!/usr/bin/env python3
"""Hash recovered sources and traverse external serialized GUID references.

This is an inventory, not a Unity importer or a C# compiler. Built-ins, absent
assets, ambiguous GUIDs, dummy shaders and dynamic dependencies stay explicit.
"""
import argparse
import collections
import gzip
import hashlib
import json
import re
import xml.etree.ElementTree as ET
from pathlib import Path, PurePosixPath

BUILTINS = {"0000000000000000e000000000000000", "0000000000000000f000000000000000"}
SERIALIZED = {".unity", ".prefab", ".asset", ".mat", ".controller", ".anim",
              ".overrideController", ".renderTexture", ".mixer", ".guiskin"}
SHA = re.compile(r"[0-9a-f]{64}")
BLOCK = re.compile(r"\{([^{}]+)\}")
GUID = re.compile(r"\bguid:\s*([0-9a-fA-F]{32})\b")
FILE_ID = re.compile(r"\bfileID:\s*(-?\d+)\b")
ANCHOR = re.compile(r"^--- !u!(\d+) &(-?\d+)(?:\s+(stripped))?", re.M)
NATIVE_PATTERNS = {
    "nativePlugin": r"\bDllImport\s*\(",
    "windowsRegistry": r"\bMicrosoft\.Win32\b|\bRegistry\.(?:LocalMachine|CurrentUser)\b",
    "threading": r"\bSystem\.Threading\b|\bnew\s+Thread\s*\(",
    "legacyNetworking": r"\bUnityEngine\.Networking\b",
    "filesystemNamespace": r"\busing\s+System\.IO\s*;",
    "dynamicResources": r"\bResources\.(?:Load|LoadAll|LoadAsync)\b",
    "sceneTransition": r"\bSceneManager\.(?:LoadScene|LoadSceneAsync|UnloadSceneAsync)\b",
    "particles": r"\bParticleSystem\b",
    "navigation": r"\bNavMesh(?:Agent|Obstacle|Path)?\b",
}


def sha(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def checked_path(root, relative):
    if not isinstance(relative, str):
        raise ValueError("Source path must be a string")
    parts = PurePosixPath(relative)
    if not relative or parts.is_absolute() or ".." in parts.parts or "\\" in relative:
        raise ValueError(f"Unsafe source path: {relative!r}")
    result = root.joinpath(*parts.parts)
    if not result.resolve().is_relative_to(root.resolve()):
        raise ValueError(f"Source path leaves project: {relative}")
    return result


def references(text):
    counts = collections.Counter()
    for block in BLOCK.findall(text):
        guid, file_id = GUID.search(block), FILE_ID.search(block)
        if guid and file_id and guid[1] != "0" * 32:
            counts[(guid[1].lower(), file_id[1])] += 1
    return [{"guid": guid, "fileID": file_id, "occurrences": count}
            for (guid, file_id), count in sorted(counts.items())]


def read_json(path):
    value = gzip.decompress(path.read_bytes()) if path.suffix == ".gz" else path.read_bytes()
    return json.loads(value)


def audit(project, repo):
    project, repo = project.resolve(), repo.resolve()
    assets = project / "Assets"
    if not assets.is_dir():
        raise ValueError("Recovered project Assets directory missing")
    scenes = sorted(assets.rglob("*.unity"))
    if not scenes:
        raise ValueError("Recovered project has no scenes")
    index = collections.defaultdict(list)
    for meta in sorted(assets.rglob("*.meta")):
        text = meta.read_text(encoding="utf-8", errors="strict")
        match = re.search(r"^guid:\s*([0-9a-fA-F]{32})\s*$", text, re.M)
        if match:
            index[match[1].lower()].append(meta.with_suffix(""))
    records = {}

    def inspect(path):
        relative = path.relative_to(project).as_posix()
        if relative in records:
            return records[relative]
        if path.is_symlink() or not path.resolve().is_relative_to(project):
            raise ValueError(f"Asset symlink/escape unsupported: {relative}")
        data = path.read_bytes()
        result = {"path": relative, "sha256": hashlib.sha256(data).hexdigest(),
                  "bytes": len(data), "references": [], "scan": "opaqueAsset"}
        try:
            text = data.decode("utf-8")
        except UnicodeDecodeError:
            text = None
        if path.suffix in SERIALIZED and text is not None and text.lstrip("\ufeff").startswith("%YAML"):
            result["scan"] = "serializedExternalGuidReferences"
            result["references"] = references(text)
            anchors = ANCHOR.findall(text)
            result["recordClasses"] = dict(sorted(collections.Counter(a[0] for a in anchors).items()))
            result["strippedRecords"] = sum(bool(a[2]) for a in anchors)
        elif path.suffix in SERIALIZED:
            result["scan"] = "unreadableSerializedDependencyGraph"
        if text is not None and path.suffix == ".shader" and "DummyShaderTextExporter" in text:
            result["dummyShaderExport"] = True
        records[relative] = result
        return result

    def resolve(ref):
        guid = ref["guid"]
        if guid in BUILTINS:
            return {"status": "unityBuiltinDependency", "targets": []}
        found = index.get(guid, [])
        if not found:
            return {"status": "missingMeta", "targets": []}
        targets = [p.relative_to(project).as_posix() for p in found]
        if len(found) != 1:
            return {"status": "ambiguousGuid", "targets": targets}
        if not found[0].is_file():
            return {"status": "missingAsset", "targets": targets}
        return {"status": "assetPresent", "targets": targets}

    results = []
    for scene in scenes:
        queue, reached, problems, builtins = collections.deque([scene]), set(), [], []
        while queue:
            current = queue.popleft()
            name = current.relative_to(project).as_posix()
            if name in reached:
                continue
            reached.add(name)
            record = inspect(current)
            for ref in record["references"]:
                resolution = resolve(ref)
                ref.update(resolution)
                detail = {"source": name, **ref}
                if resolution["status"] == "assetPresent":
                    queue.append(checked_path(project, resolution["targets"][0]))
                elif resolution["status"] == "unityBuiltinDependency":
                    builtins.append(detail)
                else:
                    problems.append(detail)
        results.append({"scene": scene.relative_to(project).as_posix(),
                        "sourceSha256": sha(scene), "reachableAssets": sorted(reached),
                        "unresolvedReferences": problems, "unityBuiltinReferences": builtins,
                        "opaqueSerializedAssets": sorted(name for name in reached
                            if records[name]["scan"] == "unreadableSerializedDependencyGraph"),
                        "dummyShaders": sorted(name for name in reached
                            if records[name].get("dummyShaderExport"))})

    recovered_scripts, api_sites = [], []
    for source in sorted(assets.rglob("*.cs")):
        record = inspect(source)
        recovered_scripts.append({k: record[k] for k in ("path", "bytes", "sha256")})
        text = source.read_text(encoding="utf-8")
        # Lexical candidates only: matching text in comments/strings is not
        # resolved C# semantics and must be confirmed before adding adapters.
        for kind, pattern in NATIVE_PATTERNS.items():
            for match in re.finditer(pattern, text):
                api_sites.append({"source": record["path"], "kind": kind,
                                  "line": text.count("\n", 0, match.start()) + 1})

    source_claims = []
    manifest_paths = ["game/assets/presentation.json.gz", "game/assets/weapon-runtime.json.gz",
                      "game/assets/ballistics.json.gz", "runtime/Original/source-provenance.json"]
    manifests = []
    by_name = collections.defaultdict(list)
    for script in recovered_scripts:
        by_name[PurePosixPath(script["path"]).name].append(script["path"])
    for name in manifest_paths:
        path = checked_path(repo, name)
        if not path.is_file():
            continue
        data = read_json(path)
        manifests.append({"path": name, "sha256": sha(path)})
        claims = dict(data.get("sourceHashes", data.get("sources", {})))
        for filename, expected in data.get("originalFiles", {}).items():
            candidates = by_name.get(filename, [])
            if len(candidates) == 1:
                claims[candidates[0]] = expected
            else:
                source_claims.append({"manifest": name, "source": filename,
                                      "status": "ambiguousOrMissingSourceName", "expectedSha256": expected})
        for relative, expected in sorted(claims.items()):
            if not isinstance(expected, str) or not SHA.fullmatch(expected):
                raise ValueError(f"Malformed source hash in {name}: {relative}")
            source = checked_path(project, relative)
            actual = sha(source) if source.is_file() else None
            source_claims.append({"manifest": name, "source": relative, "expectedSha256": expected,
                                  "actualSha256": actual,
                                  "status": "verified" if actual == expected else "sourceHashMismatch"})

    compiled = []
    for kind in ("Original", "Engine"):
        for path in sorted((repo / "runtime" / kind).rglob("*.cs")):
            compiled.append({"path": path.relative_to(repo).as_posix(), "sha256": sha(path),
                             "bytes": path.stat().st_size, "group": kind})
    project_file = repo / "runtime/Crisis.Runtime.csproj"
    compile_project = None
    if project_file.is_file():
        xml = ET.parse(project_file)
        compile_project = {"path": "runtime/Crisis.Runtime.csproj", "sha256": sha(project_file),
                           "declaredCompileRules": [dict(item.attrib) for item in xml.iter("Compile")],
                           "effectiveMSBuildItemsResolved": False}
    return {
        "schemaVersion": 1,
        "scope": {"externalSerializedGuidInventoryOnly": True, "localFileIDsValidated": False,
                  "cSharpCompiledByThisAudit": False, "methodHashesRecomputed": False,
                  "dynamicDependenciesResolved": False, "nativeParityVerified": False,
                  "fullGameClosureEstablished": False},
        "summary": {"scenes": len(scenes), "indexedGuids": len(index),
                    "ambiguousGuidCount": sum(len(v) != 1 for v in index.values()),
                    "recoveredCSharpFiles": len(recovered_scripts),
                    "selectedOriginalCompileFiles": sum(x["group"] == "Original" for x in compiled),
                    "engineCompileFiles": sum(x["group"] == "Engine" for x in compiled),
                    "sourceHashClaims": len(source_claims),
                    "sourceHashFailures": sum(x["status"] != "verified" for x in source_claims)},
        "scenes": results,
        "assets": dict(sorted(records.items())),
        "recoveredCSharpInventory": recovered_scripts,
        "currentCompileSourceInventory": compiled,
        "compileProject": compile_project,
        "provenanceManifests": manifests,
        "sourceHashClaims": source_claims,
        "lexicalApiCandidates": api_sites,
        "ambiguousGuids": {k: [p.relative_to(project).as_posix() for p in v]
                           for k, v in sorted(index.items()) if len(v) > 1},
    }


def markdown(report):
    lines = ["# Original-source compile dependency inventory", "",
             "Static inventory only; no original behavior, Unity import or C# compilation is performed.", "",
             "| Scene | Reached assets | Unresolved refs | Built-in refs | Opaque serialized assets | Dummy shaders |",
             "| --- | ---: | ---: | ---: | ---: | ---: |"]
    for scene in report["scenes"]:
        lines.append("| `" + scene["scene"] + "` | " + " | ".join(str(len(scene[k])) for k in
            ("reachableAssets", "unresolvedReferences", "unityBuiltinReferences", "opaqueSerializedAssets", "dummyShaders")) + " |")
    lines += ["", "## Source selection", ""]
    for name, value in report["summary"].items():
        lines.append(f"- {name}: {value}")
    lines += ["", "Counts are file inventories, not a completion percentage. Selected files may contain only original method slices.",
              "Built-ins remain explicit engine dependencies; a matching .meta without its asset does not resolve a reference.",
              "Local fileIDs, prefab override semantics, binary graphs, runtime Resources calls, C# symbol resolution and behavioral parity require separate checks.",
              "", "## Unresolved external references (unique)", ""]
    unique = {(r["guid"], r["fileID"], r["status"]) for s in report["scenes"] for r in s["unresolvedReferences"]}
    lines += [f"- `{guid}` fileID `{file_id}`: {status}" for guid, file_id, status in sorted(unique)] or ["None found in the scanned external-reference graph; this does not establish full closure."]
    lines += ["", "## Lexical platform/API candidates", ""]
    for kind, count in sorted(collections.Counter(x["kind"] for x in report["lexicalApiCandidates"]).items()):
        lines.append(f"- {kind}: {count}")
    lines += ["", "Candidates may include comments/strings; inspect original source before implementing anything.", ""]
    return "\n".join(lines)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("project", type=Path)
    parser.add_argument("--repo", type=Path, default=Path("."))
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    report = audit(args.project, args.repo)
    args.output.mkdir(parents=True, exist_ok=True)
    (args.output / "source-closure.json").write_text(json.dumps(report, sort_keys=True, indent=2) + "\n")
    (args.output / "source-closure.md").write_text(markdown(report))
    print(json.dumps(report["summary"], sort_keys=True))
    if report["summary"]["sourceHashFailures"]:
        raise SystemExit("Original source hash mismatch: see private audit report; no original asset changed")


if __name__ == "__main__":
    main()
