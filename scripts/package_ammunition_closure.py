#!/usr/bin/env python3
"""Package original Unity assets referenced transitively by the ammo prefab."""
from __future__ import annotations

import argparse
import copy
import gzip
import hashlib
import json
import os
from pathlib import Path

UNITY_BUILTIN_RESOURCE_GUID = "0000000000000000f000000000000000"
ZERO_GUID = "00000000000000000000000000000000"


def load_gzip_json(path: Path) -> dict:
    with gzip.open(path, "rt", encoding="utf-8") as stream:
        return json.load(stream)


def closure(scene: dict, ammo_package: dict) -> tuple[dict, set, set, set, set, dict, list, set]:
    source_assets = scene.get("assets", {})
    source_guid_paths = {str(guid).lower(): path for guid, path in scene.get("guidPaths", {}).items()}
    packaged_assets = ammo_package.get("assets")
    if not isinstance(packaged_assets, dict) or not packaged_assets:
        raise ValueError("original ammo package has no seed assets")

    roots = sorted(packaged_assets)
    pending = list(roots)
    visited: set[str] = set()
    resolved: dict = {}
    unresolved: set[tuple[str, str, str]] = set()
    used_guids: set[str] = set()
    source_scripts: set[str] = set()
    builtin_guids: set[str] = set()
    package_guid_paths = copy.deepcopy(ammo_package.get("guidPaths", {}))

    while pending:
        asset_path = pending.pop()
        if asset_path in visited:
            continue
        visited.add(asset_path)
        source_asset = source_assets.get(asset_path)
        if not isinstance(source_asset, dict) or not isinstance(source_asset.get("records"), list):
            unresolved.add((asset_path, "", "original serialized asset is absent"))
            continue

        packaged = copy.deepcopy(source_asset)
        existing = packaged_assets.get(asset_path)
        if isinstance(existing, dict) and isinstance(existing.get("records"), list):
            # Keep the already-extracted root records while attaching the original
            # export's hash, source text, and dependency inventory.
            packaged["records"] = existing["records"]
        resolved[asset_path] = packaged

        for raw_guid in source_asset.get("externalGuids", []):
            guid = str(raw_guid).lower()
            if not guid or guid == ZERO_GUID:
                continue
            used_guids.add(guid)
            if guid == UNITY_BUILTIN_RESOURCE_GUID:
                builtin_guids.add(guid)
                continue
            target = source_guid_paths.get(guid)
            if target is None:
                unresolved.add((asset_path, guid, "GUID is absent from recovered source mapping"))
                continue
            previous = package_guid_paths.get(guid)
            if previous is not None and previous != target:
                unresolved.add((asset_path, guid, "package/source path identity conflict"))
                continue
            package_guid_paths[guid] = target
            if target in source_assets:
                pending.append(target)
            elif target.lower().endswith(".cs"):
                source_scripts.add(target)
            else:
                unresolved.add((asset_path, guid, f"mapped original path has no serialized asset: {target}"))

    # Keep existing seed data and add the exact records extracted from the same
    # recovered Unity project for every transitive dependency found.
    merged_assets = copy.deepcopy(packaged_assets)
    merged_assets.update(resolved)
    return (merged_assets, sorted(unresolved), used_guids, source_scripts, builtin_guids,
            package_guid_paths, roots, sorted(visited))


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("weapon_package", type=Path)
    parser.add_argument("source_scene_package", type=Path)
    parser.add_argument("--allow-unresolved", action="store_true",
                        help="write resolved original data and report missing dependencies for continued local checks")
    parser.add_argument("--check", action="store_true",
                        help="verify the current package has complete original dependency closure without writing")
    args = parser.parse_args()

    weapon = load_gzip_json(args.weapon_package)
    scene = json.loads(args.source_scene_package.read_text(encoding="utf-8"))
    ammo = weapon.get("ammoPackage")
    if not isinstance(ammo, dict):
        raise ValueError("weapon package has no original ammoPackage")

    (assets, unresolved, used_guids, source_scripts, builtin_guids,
     guid_paths, roots, visited) = closure(scene, ammo)
    existing_asset_paths = set(ammo.get("assets", {}))
    missing_paths = sorted(set(assets) - existing_asset_paths)
    source_sha256 = hashlib.sha256(args.source_scene_package.read_bytes()).hexdigest()

    if args.check:
        audit = ammo.get("closureAudit", {})
        stale_script_maps = {key: ammo.get(key) != scene.get(key, {}) for key in ("scriptTypes", "scriptAssemblies")}
        source_metadata = scene.get("guidMetadata", {})
        package_metadata = ammo.get("guidMetadata", {})
        missing_metadata = sorted(g for g in used_guids if g in source_metadata and package_metadata.get(g) != source_metadata[g])
        complete = (not unresolved and not missing_paths and not any(stale_script_maps.values())
                        and not missing_metadata and audit.get("complete") is True)
        print(json.dumps({"complete": complete, "rootAssets": roots,
                          "resolvedAssets": sorted(existing_asset_paths),
                          "missingPackageAssets": missing_paths,
                              "staleScriptMaps": stale_script_maps,
                              "missingGuidMetadata": missing_metadata,
                          "unresolved": unresolved, "sourceSha256": source_sha256}, indent=2))
        return 0 if complete else 1

    if not args.allow_unresolved and unresolved:
        print(json.dumps({"complete": False, "rootAssets": roots,
                          "resolvedAssetCount": len(assets), "unresolved": unresolved,
                          "sourceSha256": source_sha256}, indent=2))
        return 1

    ammo["assets"] = assets
    ammo["guidPaths"] = guid_paths
    ammo["scriptTypes"] = copy.deepcopy(scene.get("scriptTypes", {}))
    ammo["scriptAssemblies"] = copy.deepcopy(scene.get("scriptAssemblies", {}))
    source_metadata = scene.get("guidMetadata", {})
    ammo_metadata = copy.deepcopy(ammo.get("guidMetadata", {}))
    for guid in used_guids:
        if guid in source_metadata:
            ammo_metadata[guid] = copy.deepcopy(source_metadata[guid])
    ammo["guidMetadata"] = ammo_metadata
    source_builtins = scene.get("builtinObjects", {})
    if source_builtins:
        merged_builtins = copy.deepcopy(ammo.get("builtinObjects", {}))
        merged_builtins.update(source_builtins)
        ammo["builtinObjects"] = merged_builtins
    ammo["closureAudit"] = {
        "sourceSceneSha256": source_sha256,
        "rootAssets": roots,
        "resolvedAssets": sorted(assets),
        "resolvedSourceScripts": sorted(source_scripts),
        "builtinGuids": sorted(builtin_guids),
        "unresolved": [{"asset": asset, "guid": guid, "reason": reason}
                       for asset, guid, reason in unresolved],
        "complete": not unresolved,
    }
    serialized = json.dumps(weapon, ensure_ascii=False, separators=(",", ":")).encode("utf-8")
    temporary = args.weapon_package.with_name(args.weapon_package.name + ".tmp")
    temporary.write_bytes(gzip.compress(serialized, mtime=0))
    os.replace(temporary, args.weapon_package)
    print(json.dumps({"complete": not unresolved, "rootAssets": roots,
                      "resolvedAssetCount": len(assets),
                      "resolvedSourceScriptCount": len(source_scripts),
                      "builtinGuidCount": len(builtin_guids),
                      "unresolved": [{"asset": asset, "guid": guid, "reason": reason}
                                     for asset, guid, reason in unresolved],
                      "sourceSceneSha256": source_sha256}, indent=2))
    return 0 if not unresolved or args.allow_unresolved else 1


if __name__ == "__main__":
    raise SystemExit(main())
