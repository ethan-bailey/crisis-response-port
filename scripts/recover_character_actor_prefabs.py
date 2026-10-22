#!/usr/bin/env python3
"""Recover the original Operator and Occupant prefabs without adaptation."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import re
import zipfile

ROOT = Path(__file__).resolve().parents[1]
ARCHIVE_SHA256 = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
DESTINATION = ROOT / "source-reference/Original/CharacterDependencies/ActorPrefabs"
ASSETS = (
    ("Operator", "export/ExportedProject/Assets/Resources/characters/Operator.prefab"),
    ("Occupant", "export/ExportedProject/Assets/Resources/characters/Occupant.prefab"),
)
HEADER = re.compile(r"^--- !u!(\d+) &(\d+)", re.M)
REQUIRED_ROOT_COMPONENTS = {
    "CharacterAudio", "AudioInstigator", "Blackboard", "Communication",
    "WeaponAimer", "Animator", "IKArmController", "Rigidbody",
    "Physiology", "ArmsControl", "CapsuleCollider", "MuscleControl", "Target",
}
EXPECTED_LIMBS = {
    "Head", "Neck", "Torso", "Abdomen", "Pelvis", "Right upper arm",
    "Right lower arm", "Right hand", "Left upper arm", "Left lower arm",
    "Left hand", "Right upper leg", "Right lower leg", "Right foot",
    "Right toes", "Left upper leg", "Left lower leg", "Left foot", "Left toes",
}
UNITY_COMPONENTS = {4: "Transform", 54: "Rigidbody", 95: "Animator", 136: "CapsuleCollider"}


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def inspect_prefab(text: str, actor_name: str, script_names: dict[str, str]) -> dict:
    matches = list(HEADER.finditer(text))
    docs = {
        file_id: (int(class_id), text[match.end():matches[i + 1].start() if i + 1 < len(matches) else len(text)])
        for i, match in enumerate(matches)
        for class_id, file_id in (match.groups(),)
    }
    game_objects = []
    root_components = []
    limb_names = []
    for file_id, (class_id, body) in docs.items():
        if class_id != 1:
            continue
        name_match = re.search(r"^  m_Name: (.*)$", body, re.M)
        tag_match = re.search(r"^  m_TagString: (.*)$", body, re.M)
        name = name_match.group(1) if name_match else ""
        game_objects.append(name)
        if tag_match and tag_match.group(1) == "Limb":
            limb_names.append(name)
        if name != actor_name:
            continue
        component_ids = re.findall(r"component: \{fileID: (-?\d+)\}", body)
        for component_id in component_ids:
            component = docs.get(component_id)
            if component is None:
                raise ValueError(f"Unresolved original component {component_id} on {actor_name}")
            component_class, component_body = component
            if component_class == 114:
                script = re.search(r"m_Script: \{fileID: \d+, guid: ([0-9a-f]{32})", component_body)
                root_components.append(script_names.get(script.group(1), "UnknownScript") if script else "UnknownScript")
            else:
                root_components.append(UNITY_COMPONENTS.get(component_class, f"UnityClassID:{component_class}"))
    missing = sorted(REQUIRED_ROOT_COMPONENTS - set(root_components))
    if missing:
        raise ValueError(f"{actor_name} prefab is missing required root components: {', '.join(missing)}")
    if set(limb_names) != EXPECTED_LIMBS:
        raise ValueError(f"{actor_name} prefab limb hierarchy differs from the original 19 limb identities")
    return {
        "sourcePrefab": f"Assets/Resources/characters/{actor_name}.prefab",
        "serializedRecords": len(docs),
        "gameObjects": len(game_objects),
        "monoBehaviours": sum(class_id == 114 for class_id, _ in docs.values()),
        "rootComponents": root_components,
        "limbNames": limb_names,
    }


def read_archive(path: Path) -> tuple[dict[str, bytes], str, dict[str, dict]]:
    digest = sha256(path.read_bytes())
    if digest != ARCHIVE_SHA256:
        raise ValueError(f"Unexpected recovery archive SHA-256: {digest}")
    extracted: dict[str, bytes] = {}
    inventories: dict[str, dict] = {}
    with zipfile.ZipFile(path) as archive:
        script_names = {}
        for source in archive.namelist():
            if not source.endswith(".cs.meta"):
                continue
            meta = archive.read(source).decode("utf-8", errors="replace")
            match = re.search(r"^guid:\s*([0-9a-f]{32})", meta, re.M)
            if match:
                script_names[match.group(1)] = Path(source[:-5]).stem
        for name, entry in ASSETS:
            prefab_text = archive.read(entry).decode("utf-8", errors="replace")
            inventories[name] = inspect_prefab(prefab_text, name, script_names)
            for source in (entry, entry + ".meta"):
                try:
                    extracted[name + (".prefab.meta" if source.endswith(".meta") else ".prefab")] = archive.read(source)
                except KeyError as error:
                    raise ValueError(f"Missing original actor asset {source}") from error
    return extracted, digest, inventories


def write_assets(path: Path) -> None:
    files, archive_hash, inventories = read_archive(path)
    DESTINATION.mkdir(parents=True, exist_ok=True)
    records = []
    for name, data in files.items():
        (DESTINATION / name).write_bytes(data)
        records.append({"path": name, "bytes": len(data), "sha256": sha256(data)})
    manifest = {
        "sourceArchive": "crisis-response-recovery-direct-v3-1.zip",
        "sourceArchiveSha256": archive_hash,
        "assets": records,
        "actorInventory": inventories,
        "adaptations": [],
        "scope": "Original serialized Operator and Occupant actor prefab hierarchies and Unity metadata only.",
        "runtimeStatus": "Preserved source assets; runtime actor instantiation and gameplay parity are not implied.",
    }
    (DESTINATION / "provenance.json").write_text(json.dumps(manifest, indent=2) + "\n")
    print(f"Recovered {len(records) // 2} original actor prefabs verbatim; no adaptations")


def check_assets(path: Path) -> None:
    expected, archive_hash, inventories = read_archive(path)
    manifest = json.loads((DESTINATION / "provenance.json").read_text())
    if manifest.get("sourceArchiveSha256") != archive_hash:
        raise ValueError("Actor prefab provenance points at a different recovery archive")
    if manifest.get("actorInventory") != inventories:
        raise ValueError("Actor prefab inventory differs from the pinned original archive")
    for name, data in expected.items():
        actual = (DESTINATION / name).read_bytes()
        if actual != data:
            raise ValueError(f"Recovered actor asset differs from the original: {name}")
        if sha256(actual) != sha256(data):
            raise ValueError(f"Recovered actor asset hash mismatch: {name}")
    print(f"PASS: {len(expected)} verbatim actor assets and metadata match the pinned archive")


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("--archive", required=True, type=Path)
    parser.add_argument("--check", action="store_true", help="verify existing recovered files without writing")
    args = parser.parse_args()
    if args.check:
        check_assets(args.archive)
    else:
        write_assets(args.archive)


if __name__ == "__main__":
    main()
