#!/usr/bin/env python3
"""Package the existing original-code WASM slice; never infer gameplay parity.

Uses only the standard library. The manifest detects accidental package mixing
and corruption; it is not a signature or an authorization/hosting mechanism.
"""
import argparse
import base64
import hashlib
import json
import re
import shutil
import subprocess
import sys
from pathlib import Path, PurePosixPath

MANIFEST = "build-manifest.json"
WASM_HEADER = b"\x00asm\x01\x00\x00\x00"


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
                "runtime-web/browser-input.mjs", "runtime-web/_framework/blazor.webassembly.js"]
    for name in required:
        require((site / name).is_file(), f"Missing site input: {name}")
    require(digest(site / "game/browser-input.mjs") == digest(site / "runtime-web/browser-input.mjs"),
            "Game and compiled runtime have different input adapters")
    html = (site / "runtime-web/index.html").read_text()
    require('<base href="/runtime-web/">' in html, "Runtime must be served at /runtime-web/")
    wasm = list((site / "runtime-web/_framework").rglob("*.wasm"))
    require(wasm, "Actual compiled .NET WebAssembly is missing")
    wasm.append(site / "game/vendor/physx-js-webidl.wasm")
    for path in wasm:
        with path.open("rb") as stream:
            require(stream.read(8) == WASM_HEADER, f"Invalid WebAssembly binary: {path.name}")
    return {"wasmFiles": len(wasm), "blazorResources": validate_boot(site)}


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


def package(repo, published, output, toolchain, run_url):
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
                    ignore=shutil.ignore_patterns("node_modules", ".git", "package.json", "package-lock.json"))
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
    shutil.copytree(repo / "scene-data", output / "scene-data")
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
    build.add_argument("--run-url", default="")
    check = sub.add_parser("verify")
    check.add_argument("site", type=Path)
    args = parser.parse_args()
    if args.command == "build":
        result = package(args.repo, args.published, args.output,
                         json.loads(args.toolchain.read_text()), args.run_url)
    else:
        result = verify(args.site)
    print(json.dumps(result, sort_keys=True))


if __name__ == "__main__":
    main()
