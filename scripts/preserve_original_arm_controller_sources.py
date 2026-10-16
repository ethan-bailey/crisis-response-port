#!/usr/bin/env python3
"""Preserve SHA-exact original developer IK controller references without compiling fake dependencies.

The four untouched Unity C# files are staged under source-reference/ (outside
runtime/Crisis.Runtime.csproj's Original/**/*.cs glob). They can be moved into
the actual runtime ONLY when each original dependency closure is available.
"""
import argparse
import hashlib
import json
from pathlib import Path
import zipfile

ARCHIVE_SHA = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
SOURCE_PREFIX = "export/ExportedProject/Assets/Scripts/Assembly-CSharp/"
EXPECTED = {
    "IKArmType": ("bc0801f3b205f09da8a2fb1f108d34f23e25f29090c77b28fa1e86731bcb589d", 48),
    "IKArmSequencer": ("29f48bdb6446fd560dae5a05f235af5701b8a484401392fb5528ec696ac59610", 5452),
    "IKArmController": ("848283e9b0a3e25ce1650529b1ac57a0bebb8f5b600cada1ad33d56a3fa2ec6e", 12158),
    "ArmIKSolver": ("ddc4291b1ab5f73e74ddb65b8df67232bff2afddd1a591403e8aceed13a9c5dc", 7585),
    "CharacterBody": ("76301d45e6e94556d1a902f0d37f5f97111bc2efb89f36de7ed9acd935b55742", 62958),
}
COMPILE_DEPENDENCIES = {
    "IKArmSequencer": ["IKArmController", "ArmIKSolver", "IKArmType", "CharacterBody", "WeaponBase", "TeamOperator", "UnityEngine.Transform/Time"],
    "IKArmController": ["IKArmSequencer", "IKArmType", "CharacterBody", "ArmIKSolver", "WeaponBase", "Grenade", "WeaponBrace", "UnityEngine"],
    "ArmIKSolver": ["IKArmController", "CharacterBody", "Limb", "Hand", "Joint", "InverseKinematics2D", "UnityEngine"],
    "CharacterBody": ["BehaviorDesigner.Runtime", "UnityEngine.Events", "ArmsControl", "WeaponBase", "Limb", "Hand", "medical/AI/team/weapons/animation classes"],
}
def sha(b): return hashlib.sha256(b).hexdigest()

def preserve(archive: Path, repo: Path):
    if sha(archive.read_bytes()) != ARCHIVE_SHA:
        raise ValueError("Original archive SHA256 mismatch")
    dest = repo / "source-reference/Original/Animation"
    dest.mkdir(parents=True, exist_ok=True)
    included = repo / "runtime/Original/Animation/IKArmType.cs"
    records = {}
    with zipfile.ZipFile(archive) as z:
        for name, (expected, size) in EXPECTED.items():
            path = SOURCE_PREFIX + name + ".cs"
            data = z.read(path)
            if len(data) != size or sha(data) != expected:
                raise ValueError("Original class source mismatch: " + name)
            if name == "IKArmType":
                if included.read_bytes() != data:
                    raise ValueError("Original IKArmType in runtime must remain unchanged")
                state, target = "compiled_by_runtime", included
            else:
                target = dest / (name + ".cs")
                if target.exists() and target.read_bytes() != data:
                    raise ValueError("Existing original source differs; refusing overwrite: " + name)
                target.write_bytes(data)
                state = "preserved_not_compiled"
            records[name] = dict(originalPath=path.removeprefix("export/ExportedProject/"),
                                 sha256=expected, bytes=size, repositoryPath=target.relative_to(repo).as_posix(),
                                 status=state, directDependencies=COMPILE_DEPENDENCIES.get(name, []))
    manifest = {
        "schema": 1, "archiveSha256": ARCHIVE_SHA,
        "purpose": "unchanged original C# reference source, not a playable controller implementation",
        "originalSourceUnmodified": True,
        "originalControllerPlaybackImplemented": False,
        "compiledOriginalClassNames": ["IKArmType"],
        "sourceOnlyOriginalClassNames": sorted(set(EXPECTED)-{"IKArmType"}),
        "classes": records,
        "blocker": "Actual IKArmSequencer depends on source CharacterBody, original controller callbacks, and original ArmIKSolver/Limb/Hand physics; no placeholder classes may be invented."
    }
    output = dest / "source-provenance.json"
    serialized = json.dumps(manifest, sort_keys=True, indent=2) + "\n"
    if output.exists() and output.read_text() != serialized:
        raise ValueError("Existing original source provenance differs; refuse overwrite")
    output.write_text(serialized)
    return {name: dict(hash=v["sha256"], status=v["status"]) for name,v in records.items()}

if __name__ == "__main__":
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument("archive",type=Path)
    p.add_argument("--repo",type=Path,default=Path("."))
    a=p.parse_args()
    print(json.dumps(preserve(a.archive,a.repo),sort_keys=True))
