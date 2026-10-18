#!/usr/bin/env python3
"""Audit original CharacterBody dependency ownership without mutating source.

Usage: python3 scripts/check_characterbody_ownership.py
Exit 0 means inventory completed, NOT that the original class compiles.
Use --strict to fail while transport CharacterBody bindings remain.
"""
from pathlib import Path
import argparse
import re

ROOT = Path(__file__).resolve().parents[1]
FULL = ROOT / "runtime/Original/CharacterBody.Full.cs"
SOURCES = {
    "arms control": ROOT / "runtime/Original/Animation/ArmsControl.cs",
    "arm controller": ROOT / "runtime/Original/Animation/ArmController.cs",
    "equipment": ROOT / "runtime/Original/Weapons/Equipment.cs",
    "full character": FULL,
}
TRANSPORT_TYPE = "Crisis.Runtime.Weapons.CharacterBody"
ORIGINAL_TYPE = "Crisis.Runtime.FullSource.CharacterBody"
REQUIRED_COMPONENTS = (
    "CharacterAudio", "AudioInstigator", "Blackboard", "Communication",
    "WeaponAimer", "Animator", "IKArmController", "Rigidbody",
    "Physiology", "ArmsControl", "CapsuleCollider", "MuscleControl", "Target",
)
REQUIRED_LIMBS = (
    "Head", "Neck", "Torso", "Abdomen", "Pelvis",
    "RightUpperArm", "RightLowerArm", "RightHand",
    "LeftUpperArm", "LeftLowerArm", "LeftHand",
    "RightUpperLeg", "RightLowerLeg", "RightFoot", "RightToes",
    "LeftUpperLeg", "LeftLowerLeg", "LeftFoot", "LeftToes",
)

def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--strict", action="store_true")
    args = parser.parse_args()
    errors = []
    for label, path in SOURCES.items():
        if not path.is_file():
            errors.append(f"missing: {path.relative_to(ROOT)}")
            continue
        source = path.read_text(encoding="utf-8")
        for line_number, line in enumerate(source.splitlines(), 1):
            if TRANSPORT_TYPE in line:
                errors.append(f"{path.relative_to(ROOT)}:{line_number}: transport CharacterBody referenced")
        if label == "full character":
            if f"namespace {ORIGINAL_TYPE.rsplit('.', 1)[0]};" not in source:
                errors.append("full character's namespace changed")
            for type_name in REQUIRED_COMPONENTS:
                if not re.search(r"GetComponent<" + type_name + r">\\s*\\(", source):
                    errors.append(f"original Awake component lookup missing: {type_name}")
            for limb in REQUIRED_LIMBS:
                if not re.search(r"\\b" + limb + r"\\b", source):
                    errors.append(f"original limb identity missing: {limb}")
            for marker in (
                "EquipmentHelper.Instance.SpawnEquipment(equipment)",
                "gameObject.GetComponent<Equipment>()",
                "Mission.onMissionBegin.AddListener(OnMissionBegin)",
            ):
                if marker not in source:
                    errors.append(f"original ownership/lifecycle callsite missing: {marker}")
    print(f"Original CharacterBody: {ORIGINAL_TYPE}")
    print(f"Legacy browser transport: {TRANSPORT_TYPE}")
    print(f"Required actor components: {len(REQUIRED_COMPONENTS)}; limb identities: {len(REQUIRED_LIMBS)}")
    for issue in errors:
        print("BLOCKER:", issue)
    if not errors:
        print("Ownership source scan clear (native compile still required).")
    else:
        print(f"{len(errors)} ownership/source blockers detected; native compilation NOT verified.")
    return int(args.strict and bool(errors))

if __name__ == "__main__":
    raise SystemExit(main())
