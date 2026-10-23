#!/usr/bin/env python3
"""Index repository files and map them to an existing Site package manifest.

The index records each non-ignored working-tree file (including untracked
files), content hash, byte size, and packaging role. It does not copy source
files into a Site archive. Run after any source change and before packaging.
"""
import argparse
import hashlib
import json
import subprocess
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
DEFAULT_OUTPUT = ROOT / "docs/audit/repository-file-index.json"


def git(*args):
    return subprocess.check_output(["git", "-C", str(ROOT), *args])


def sha256(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def package_role(path, source_inputs):
    if path in source_inputs:
        if path.startswith("game/"):
            return "game-source-or-asset"
        if path.startswith("scene-data/"):
            return "scene-package-source"
        if path.startswith("runtime/"):
            return "runtime-build-source"
        return "build-or-validation-input"
    return "repository-only"


def generate(site_manifest_path):
    manifest_bytes = site_manifest_path.read_bytes()
    site_manifest = json.loads(manifest_bytes)
    source_entries = {entry["path"]: entry for entry in site_manifest["sourceInventory"]}
    output_entries = {entry["path"]: entry for entry in site_manifest["files"]}
    index_path = DEFAULT_OUTPUT.relative_to(ROOT).as_posix()
    names = sorted({p.decode("utf-8") for p in git(
        "ls-files", "--cached", "--others", "--exclude-standard", "-z"
    ).split(b"\0") if p and p.decode("utf-8") != index_path})
    files = []
    for name in names:
        path = ROOT / name
        entry = source_entries.get(name)
        packaged = output_entries.get(name)
        digest = sha256(path)
        files.append({
            "path": name,
            "bytes": path.stat().st_size,
            "sha256": digest,
            "packageRole": package_role(name, source_entries),
            "inLatestSourceInventory": entry is not None,
            "matchesLatestSourceInventory": bool(entry and entry["sha256"] == digest),
            "copiedAtSamePathToLatestPackage": bool(packaged and packaged["sha256"] == digest),
        })
    role_counts = Counter(item["packageRole"] for item in files)
    return {
        "schemaVersion": 1,
        "scope": "Every tracked or non-ignored working-tree file in this repository, excluding this generated index. Hashes identify file contents; file contents remain in their repository/package source paths.",
        "repositoryHead": git("rev-parse", "HEAD").decode().strip(),
        "sitePackage": {
            "sourceCommit": site_manifest["sourceCommit"],
            "sourceInventoryCount": len(source_entries),
            "outputFileCount": len(output_entries),
            "manifestSha256": hashlib.sha256(manifest_bytes).hexdigest(),
        },
        "reviewedCandidates": [
            {
                "path": "game/source-character-step.mjs",
                "origin": "Necessary substitute: ports CharacterBody movement/speed formulas because the deployed WASM lacks those JS-invokable methods; its velocity-based moving predicate retains the known-good browser behavior between fixed physics steps.",
                "errorCheck": "Covered by scripts/source_character_step_test.mjs.",
                "duplicateCheck": "No equivalent implementation is present in the compiled WASM bridge; this module is the active adapter.",
                "decision": "keep-and-package",
            },
            {
                "path": "game/browser-input.mjs",
                "origin": "Owner-approved touch input additions, retained in the latest source.",
                "errorCheck": "Covered by scripts/browser_input_test.mjs.",
                "duplicateCheck": "No more complete touch adapter exists in the deployed browser package.",
                "decision": "restore-regressed-touch-behavior-from-known-good-site-build",
            },
            {
                "path": "game/index.html",
                "origin": "Owner-approved touch controls and iOS interaction safeguards from the previously working Site build.",
                "errorCheck": "Uses the existing movement and aim adapter; event capture/cancellation contracts pass in scripts/browser_input_test.mjs.",
                "duplicateCheck": "Current build lacks the vertical joystick aim callback, global callout suppression, and touch guide.",
                "decision": "restore-and-package",
            },
            {
                "path": "runtime/Original/CharacterBody.Full.cs",
                "origin": "Original game source; authoritative movement and animation formulas.",
                "errorCheck": "Existing dependency inventory and compiler baseline identify its closure; source edits require the prescribed validation gates.",
                "duplicateCheck": "Do not copy this C# file into the website output; its runtime form is compiled WASM.",
                "decision": "keep-in-source-index; do-not-duplicate-in-site-output",
            },
        ],
        "fileCount": len(files),
        "countsByPackageRole": dict(sorted(role_counts.items())),
        "files": files,
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--site-manifest", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    parser.add_argument("--check", action="store_true")
    args = parser.parse_args()
    result = json.dumps(generate(args.site_manifest.resolve()), indent=2, sort_keys=True) + "\n"
    if args.check:
        if not args.output.is_file() or args.output.read_text() != result:
            raise SystemExit("Repository file index is incomplete or stale; regenerate and review before packaging")
    else:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(result)
    print(f"Indexed {len(json.loads(result)['files'])} repository files against the latest Site manifest")


if __name__ == "__main__":
    main()
