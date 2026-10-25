#!/usr/bin/env python3
"""Package the existing original-code WASM slice; never infer gameplay parity.

Uses only the standard library. The manifest detects accidental package mixing
and corruption; it is not a signature or an authorization/hosting mechanism.
"""
import argparse
import base64
import hashlib
import gzip
import json
import re
import shutil
import subprocess
import sys
from pathlib import Path, PurePosixPath

MANIFEST = "build-manifest.json"
WASM_HEADER = b"\x00asm\x01\x00\x00\x00"
SCENE_ARCHIVE = "crisis-response-scenes.json.gz"
SCENE_MANIFEST = "crisis-response-scenes.manifest.json"
SCENE_CHUNK_BYTES = 16 * 1024 * 1024
SITE_FILE_LIMIT_BYTES = 25 * 1024 * 1024


def require(condition, message):
    if not condition:
        raise ValueError(message)


def digest(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def files(root):
    result = []
    for path in sorted(root.rglob("*")):
        require(not path.is_symlink(), f"Symlink is not a package input: {path}")
        if path.is_file():
            result.append(path)
    return result


def record(path, root):
    return {"path": path.relative_to(root).as_posix(),
            "bytes": path.stat().st_size, "sha256": digest(path)}


def split_scene_package(scene_data, chunk_bytes=SCENE_CHUNK_BYTES):
    """Expand the source gzip into independent gzip chunks below Sites' file limit."""
    archive = scene_data / SCENE_ARCHIVE
    if not archive.exists():
        return None
    require(chunk_bytes > 0, "Scene chunk size must be positive")
    chunks = []
    total_bytes = 0
    with gzip.open(archive, "rb") as source:
        index = 0
        while raw := source.read(chunk_bytes):
            name = f"crisis-response-scenes-{index:03d}.json.gz"
            data = gzip.compress(raw, compresslevel=6, mtime=0)
            require(len(data) < SITE_FILE_LIMIT_BYTES,
                    f"Scene chunk exceeds Sites' per-file limit: {name}")
            (scene_data / name).write_bytes(data)
            chunks.append({"path": name, "bytes": len(raw)})
            total_bytes += len(raw)
            index += 1
    require(chunks, "Original scene package is empty")
    archive.unlink()
    manifest = {"schemaVersion": 1, "encoding": "independent-gzip-chunks",
                "uncompressedBytes": total_bytes, "chunks": chunks}
    (scene_data / SCENE_MANIFEST).write_text(json.dumps(manifest, indent=2) + "\n")
    return manifest


def safe_path(root, name):
    require(isinstance(name, str) and name, f"Unsafe manifest path: {name!r}")
    relative = PurePosixPath(name)
    require(not relative.is_absolute()
            and ".." not in relative.parts and "\\" not in name,
            f"Unsafe manifest path: {name!r}")
    return root.joinpath(*relative.parts)


def validate_boot(site):
    framework = site / "runtime-web/_framework"
    boot = framework / "blazor.boot.json"
    require(boot.is_file(), "Missing compiled Blazor boot manifest")
    data = json.loads(boot.read_text())
    checked = []

    def walk(value):
        if not isinstance(value, dict):
            return
        for name, child in value.items():
            if isinstance(child, dict):
                walk(child)
            elif isinstance(child, str) and child.startswith("sha256-") and name != "hash":
                path = safe_path(framework, name)
                require(path.is_file(), f"Missing Blazor resource: {name}")
                expected = "sha256-" + base64.b64encode(bytes.fromhex(digest(path))).decode()
                require(child == expected, f"Blazor resource hash mismatch: {name}")
                checked.append(name)

    walk(data.get("resources", {}))
    require(checked, "No hashed Blazor resources found")
    require(any(name.startswith("Crisis.Runtime.Browser.") for name in checked),
            "Compiled original-code browser assembly missing from boot resources")
    require(any(name.startswith("Crisis.Runtime.") and not name.startswith("Crisis.Runtime.Browser.")
                for name in checked), "Compiled original-code runtime assembly missing")
    return len(checked)


def validate_site(site):
    required = ["index.html", "game/index.html", "game/assets/mission01.crg",
                "game/browser-input.mjs", "game/vendor/physx-js-webidl.mjs",
                "game/vendor/physx-js-webidl.wasm", "runtime-web/index.html",
                "runtime-web/browser-input.mjs", "runtime-web/_framework/blazor.webassembly.js",
                "scene-data/scene-package-loader.mjs"]
    for name in required:
        require((site / name).is_file(), f"Missing site input: {name}")
    require(digest(site / "game/browser-input.mjs") == digest(site / "runtime-web/browser-input.mjs"),
            "Game and compiled runtime have different input adapters")
    html = (site / "runtime-web/index.html").read_text()
    require('<base href="/runtime-web/">' in html, "Runtime must be served at /runtime-web/")
    require("scene-package-loader.mjs" in html, "Runtime must load scenes through the split-package loader")
    scene_manifest_path = site / "scene-data/crisis-response-scenes.manifest.json"
    scene_smoke_included = scene_manifest_path.is_file()
    scene_chunks = sorted((site / "scene-data").glob("crisis-response-scenes-*.json.gz"))
    if scene_smoke_included:
        scene_manifest = json.loads(scene_manifest_path.read_text())
        require(scene_manifest.get("encoding") == "independent-gzip-chunks"
                and bool(scene_manifest.get("chunks")), "Scene package chunk manifest is invalid")
        for chunk in scene_manifest["chunks"]:
            name = chunk.get("path", "")
            require(re.fullmatch(r"crisis-response-scenes-\d{3}\.json\.gz", name) is not None,
                    "Invalid scene package chunk name")
            path = site / "scene-data" / name
            require(path.is_file() and path.stat().st_size < SITE_FILE_LIMIT_BYTES,
                    f"Scene package chunk is missing or exceeds Sites' per-file limit: {name}")
        require(len(scene_chunks) == len(scene_manifest["chunks"]),
                "Scene package has unlisted diagnostic chunks")
    else:
        require(not scene_chunks, "Scene smoke chunks are present without their manifest")
    actor_bundle = site / "scene-data/fullsource/operator-prefab.json.gz"
    actor_manifest_path = site / "scene-data/fullsource/operator-prefab.manifest.json"
    has_actor_delivery = actor_bundle.exists() or actor_manifest_path.exists()
    if has_actor_delivery:
        require(actor_bundle.is_file() and actor_manifest_path.is_file(),
                "Bounded FullSource Operator bundle is incomplete")
        actor_manifest = json.loads(actor_manifest_path.read_text())
        actor_bytes = actor_bundle.read_bytes()
        require(actor_manifest.get("schemaVersion") == 1
                and actor_manifest.get("profile") == "operator-prefab"
                and actor_manifest.get("encoding") == "gzip-json"
                and actor_manifest.get("rootAssets") == ["Assets/Resources/characters/Operator.prefab"],
                "FullSource Operator bundle manifest identity is invalid")
        require(actor_manifest.get("compressedBytes") == len(actor_bytes)
                and actor_manifest.get("compressedBytes", SITE_FILE_LIMIT_BYTES) <= 12 * 1024 * 1024
                and actor_manifest.get("packageSha256") == hashlib.sha256(actor_bytes).hexdigest(),
                "FullSource Operator bundle size or checksum differs from its manifest")
        actor_json = gzip.decompress(actor_bytes)
        require(actor_manifest.get("uncompressedBytes") == len(actor_json)
                and len(actor_json) <= 32 * 1024 * 1024,
                "FullSource Operator bundle exceeds its decompressed delivery budget")
        actor_package = json.loads(actor_json)
        record_count = sum(len(asset.get("records", [])) for asset in actor_package.get("assets", {}).values())
        require("Assets/Resources/characters/Operator.prefab" in actor_package.get("assets", {})
                and actor_manifest.get("assetCount") == len(actor_package.get("assets", {}))
                and actor_manifest.get("recordCount") == record_count,
                "FullSource Operator bundle omits its authored prefab or source counts differ")
    wasm = list((site / "runtime-web/_framework").rglob("*.wasm"))
    require(wasm, "Actual compiled .NET WebAssembly is missing")
    wasm.append(site / "game/vendor/physx-js-webidl.wasm")
    for path in wasm:
        with path.open("rb") as stream:
            require(stream.read(8) == WASM_HEADER, f"Invalid WebAssembly binary: {path.name}")
    incremental_profiles = []
    for manifest_path in sorted((site / "scene-data/fullsource").glob("*/manifest.json")):
        profile_root = manifest_path.parent
        profile = json.loads(manifest_path.read_text())
        require(profile.get("schemaVersion") == 1 and profile.get("encoding") == "gzip-json-assets"
                and profile.get("profile") == profile_root.name
                and isinstance(profile.get("assets"), list)
                and len(profile["assets"]) == profile.get("assetCount"),
                f"Incremental source manifest is invalid: {manifest_path}")
        profile_asset_paths = {item.get("path") for item in profile["assets"]}
        require(isinstance(profile.get("rootAssets"), list) and bool(profile["rootAssets"])
                and set(profile["rootAssets"]).issubset(profile_asset_paths),
                f"Incremental source profile omits one of its root assets: {manifest_path}")
        metadata = profile.get("metadata", {})
        metadata_path = safe_path(profile_root, metadata.get("file", ""))
        compressed = metadata_path.read_bytes()
        require(len(compressed) == metadata.get("compressedBytes")
                and digest(metadata_path) == metadata.get("compressedSha256"),
                f"Incremental metadata compressed integrity mismatch: {manifest_path}")
        raw = gzip.decompress(compressed)
        require(len(raw) == metadata.get("bytes") and hashlib.sha256(raw).hexdigest() == metadata.get("sha256"),
                f"Incremental metadata source integrity mismatch: {manifest_path}")
        metadata_json = json.loads(raw)
        require(len(metadata_json.get("assetManifest", {})) == profile["assetCount"],
                f"Incremental metadata asset count mismatch: {manifest_path}")
        total_compressed = total_raw = 0
        for asset in profile["assets"]:
            asset_path = safe_path(profile_root, asset.get("file", ""))
            packed = asset_path.read_bytes()
            require(len(packed) < SITE_FILE_LIMIT_BYTES
                    and len(packed) == asset.get("compressedBytes")
                    and digest(asset_path) == asset.get("compressedSha256"),
                    f"Incremental asset compressed integrity mismatch: {asset_path}")
            asset_raw = gzip.decompress(packed)
            require(len(asset_raw) == asset.get("bytes")
                    and hashlib.sha256(asset_raw).hexdigest() == asset.get("sha256")
                    and metadata_json["assetManifest"].get(asset.get("path"), {}).get("bytes") == len(asset_raw)
                    and metadata_json["assetManifest"].get(asset.get("path"), {}).get("sha256") == asset["sha256"]
                    and metadata_json["assetManifest"].get(asset.get("path"), {}).get("compressedBytes") == len(packed)
                    and metadata_json["assetManifest"].get(asset.get("path"), {}).get("compressedSha256") == asset["compressedSha256"],
                    f"Incremental asset source integrity mismatch: {asset_path}")
            total_compressed += len(packed)
            total_raw += len(asset_raw)
        require(total_compressed == profile.get("compressedAssetBytes")
                and total_raw == profile.get("uncompressedAssetBytes"),
                f"Incremental source totals mismatch: {manifest_path}")
        incremental_profiles.append({"profile": profile.get("profile"), "assetCount": profile["assetCount"],
                                     "compressedBytes": total_compressed, "sourceBytes": total_raw})
    return {"wasmFiles": len(wasm), "blazorResources": validate_boot(site),
            "sceneSmokeIncluded": scene_smoke_included,
            "boundedOperatorSource": has_actor_delivery, "incrementalSourceProfiles": incremental_profiles}


def verify(site):
    files(site)  # Reject symlinks, including ones pointing outside the package.
    manifest = json.loads((site / MANIFEST).read_text())
    require(manifest.get("schemaVersion") == 1, "Unsupported build manifest version")
    require(re.fullmatch(r"[0-9a-f]{40}", manifest.get("sourceCommit", "")), "Invalid source commit")
    listed = set()
    for entry in manifest["files"]:
        name = entry["path"]
        path = safe_path(site, name)
        require(name != MANIFEST and name not in listed, f"Duplicate/reserved manifest path: {name}")
        require(path.is_file(), f"Missing package file: {name}")
        require(path.stat().st_size == entry["bytes"] and digest(path) == entry["sha256"],
                f"Package checksum mismatch: {name}")
        listed.add(name)
    actual = {p.relative_to(site).as_posix() for p in files(site)} - {MANIFEST}
    require(actual == listed, f"Unlisted package files: {sorted(actual - listed)}")
    result = validate_site(site)
    require(result == manifest["binaryValidation"], "Binary validation differs from manifest")
    return {"status": "verified", "sourceCommit": manifest["sourceCommit"],
            "files": len(listed), **result}


def git(repo, *args):
    return subprocess.check_output(["git", "-C", str(repo), *args], text=True).strip()


def package(repo, published, output, toolchain, run_url, incremental_source_package=None,
            incremental_profile="global-ui-operator", incremental_roots=None,
            include_original_scene_smoke=False):
    repo, published, output = repo.resolve(), published.resolve(), output.resolve()
    require((repo / "game/index.html").is_file(), "Game source missing")
    require(published.is_dir(), "Published wwwroot directory missing")
    require(not output.exists(), "Output must be new; refusing to overwrite existing package")
    for source in (repo / "game", published):
        require(not output.is_relative_to(source) and not source.is_relative_to(output),
                "Package output overlaps source inputs")
    require(not git(repo, "diff", "HEAD", "--name-only", "--", "game", "runtime"),
            "Tracked game/runtime source differs from input commit")
    commit = git(repo, "rev-parse", "HEAD")
    require(re.fullmatch(r"[0-9a-f]{40}", commit), "Invalid checkout commit")
    pin = repo / "tools/wasm-compile/global.json"
    require(pin.is_file() and (repo / "global.json").is_file(), "Explicit compiler SDK selection missing")
    require(digest(pin) == digest(repo / "global.json"), "SDK selection differs from tracked compiler pin")
    sdk = json.loads(pin.read_text())["sdk"]
    require(sdk.get("rollForward") == "disable" and sdk.get("allowPrerelease") is False,
            "Compiler SDK must be exact and stable")
    require(toolchain.get("dotnet") == sdk["version"],
            "Selected .NET SDK differs from tracked compiler pin")
    files(repo / "game")
    files(repo / "scene-data")
    files(published)
    inventory = []
    tracked = git(repo, "ls-files", "-z", "--", "game", "runtime", "scripts", ".github/workflows", "tools/wasm-compile", "scene-data")
    for name in sorted(filter(None, tracked.split("\0"))):
        path = repo / name
        require(path.is_file() and not path.is_symlink(), f"Invalid source input: {name}")
        inventory.append(record(path, repo))
    require(inventory, "Empty source inventory")
    shutil.copytree(repo / "game", output / "game",
                    ignore=shutil.ignore_patterns("node_modules", ".git", "package.json", "package-lock.json", "mission01.crg.report.json"))
    # The compact runtime scene is a deterministic build output from the
    # tracked original Unity mesh export. Generate it in the package so the
    # source tree does not need to commit a second copy of the scene geometry.
    scene_obj = output / "game/assets/mission01.obj"
    scene_binary = output / "game/assets/mission01.crg"
    scene_builder = repo / "scripts/build_scene_binary.py"
    require(scene_obj.is_file(), "Original Mission 01 OBJ source is missing")
    require(scene_builder.is_file(), "Original scene binary builder is missing")
    subprocess.run([sys.executable, str(scene_builder), str(scene_obj), str(scene_binary)],
                   check=True, cwd=repo)
    scene_report = json.loads(scene_binary.with_suffix(".report.json").read_text())
    require(scene_report.get("sourceSha256") == digest(scene_obj),
            "Generated scene report does not match the packaged original OBJ")
    require(scene_report.get("binarySha256") == digest(scene_binary),
            "Generated scene report does not match the packaged scene binary")
    shutil.copytree(published, output / "runtime-web")
    scene_ignore = None if include_original_scene_smoke else shutil.ignore_patterns(SCENE_ARCHIVE)
    shutil.copytree(repo / "scene-data", output / "scene-data", ignore=scene_ignore)
    if incremental_source_package is not None:
        source_builder = repo / "scripts/package_incremental_source.py"
        require(source_builder.is_file(), "Incremental original source package builder is missing")
        require(re.fullmatch(r"[A-Za-z0-9_-]{1,64}", incremental_profile) is not None,
                "Invalid incremental source profile name")
        source_output = output / "scene-data/fullsource" / incremental_profile
        command = [sys.executable, str(source_builder), str(Path(incremental_source_package).resolve()),
                   str(source_output), "--profile", incremental_profile]
        for root in incremental_roots or []:
            command.extend(["--root", root])
        subprocess.run(command, check=True, cwd=repo)
    archive = output / "scene-data/crisis-response-scenes.json.gz"
    if archive.is_file():
        with gzip.open(archive, "rt", encoding="utf-8") as source:
            original_format = source.read(80).startswith('{"format":"crisis-unity-records-v2"')
        if original_format:
            actor_package = repo / "scripts/package_fullsource_actor.py"
            require(actor_package.is_file(), "FullSource actor package builder is missing")
            subprocess.run([sys.executable, str(actor_package), str(archive),
                            str(output / "scene-data/fullsource/operator-prefab.json.gz")],
                           check=True, cwd=repo)
    if include_original_scene_smoke:
        split_scene_package(output / "scene-data")
    runtime_index = output / "runtime-web/index.html"
    html = runtime_index.read_text()
    require(html.count('<base href="/">') == 1, "Unexpected compiled runtime base path")
    runtime_index.write_text(html.replace('<base href="/">', '<base href="/runtime-web/">'))
    # Changing index.html invalidates publish-generated precompressed variants.
    for suffix in (".br", ".gz"):
        runtime_index.with_name(runtime_index.name + suffix).unlink(missing_ok=True)
    (output / "index.html").write_text(
        '<!doctype html><html lang="en"><meta charset="utf-8">'
        '<meta name="viewport" content="width=device-width,initial-scale=1">'
        '<meta http-equiv="refresh" content="0;url=game/">'
        '<title>Crisis Response</title><a href="game/">Open game</a></html>\n')
    validation = validate_site(output)
    game_html = (output / "game/index.html").read_text()
    importmap = re.search(r'<script type="importmap">(.*?)</script>', game_html, re.S)
    require(importmap is not None, "Original game import map missing")
    external = {key: value for key, value in json.loads(importmap[1])["imports"].items()
                if value.startswith(("https://", "http://"))}
    manifest = {
        "schemaVersion": 1, "sourceCommit": commit,
        "buildRun": run_url, "toolchain": toolchain,
        "compilerPolicy": sdk,
        "generatedSdkSelection": record(repo / "global.json", repo),
        "sourceInventory": inventory,
        "files": [record(p, output) for p in files(output)],
        "binaryValidation": validation, "externalImports": external,
        "scope": {"originalGameComplete": False, "nativeParityVerified": False,
                  "physicalIPhoneTested": False, "browserTestedThisBuild": False,
                  "originalSceneSmokeIncluded": include_original_scene_smoke,
                  "hosting": "Not deployed; owner-restricted hosting required",
                  "mountPath": "/", "entrypoint": "/game/",
                  "offlineComplete": False},
    }
    (output / MANIFEST).write_text(json.dumps(manifest, indent=2, sort_keys=True) + "\n")
    return verify(output)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    sub = parser.add_subparsers(dest="command", required=True)
    build = sub.add_parser("build")
    build.add_argument("--repo", type=Path, default=Path("."))
    build.add_argument("--published", type=Path, required=True, help="dotnet publish wwwroot")
    build.add_argument("--output", type=Path, required=True)
    build.add_argument("--toolchain", type=Path, required=True)
    build.add_argument("--incremental-source-package", type=Path,
                       help="Optional recovered full source JSON package used to build a bounded per-asset profile")
    build.add_argument("--incremental-profile", default="global-ui-operator")
    build.add_argument("--incremental-root", action="append", dest="incremental_roots",
                       help="Override a default profile root; repeat for multiple roots")
    build.add_argument("--include-original-scene-smoke", action="store_true",
                       help="Include the 220 MB uncompressed full-scene diagnostic archive (omitted by default)")
    build.add_argument("--run-url", default="")
    check = sub.add_parser("verify")
    check.add_argument("site", type=Path)
    args = parser.parse_args()
    if args.command == "build":
        result = package(args.repo, args.published, args.output,
                         json.loads(args.toolchain.read_text()), args.run_url,
                         args.incremental_source_package, args.incremental_profile, args.incremental_roots,
                         args.include_original_scene_smoke)
    else:
        result = verify(args.site)
    print(json.dumps(result, sort_keys=True))


if __name__ == "__main__":
    main()
