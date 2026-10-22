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

def active_full_runtime_lines(source):
    """Evaluate the foundation switch, preserving unknown branches conservatively."""
    active=True
    stack=[]
    def condition(expression):
        return {'CRISIS_FOUNDATION_TRANSPORT':False,'!CRISIS_FOUNDATION_TRANSPORT':True}.get(expression)
    for number,line in enumerate(source.splitlines(),1):
        directive=line.strip()
        if directive.startswith('#if '):
            value=condition(directive[4:].strip());stack.append((active,value))
            active=active and value is not False
        elif directive=='#else' and stack:
            parent,value=stack[-1];active=parent and value is not True
        elif directive.startswith('#elif ') and stack:
            parent,previous=stack[-1];value=condition(directive[6:].strip())
            active=parent and previous is not True and value is not False
            stack[-1]=(parent,True if previous is True or value is True else None)
        elif directive=='#endif' and stack:
            active,_=stack.pop()
        elif active:
            yield number,line

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
        # The foundation harness deliberately compiles the legacy transport
        # under CRISIS_FOUNDATION_TRANSPORT. Reject it in the full runtime,
        # but do not report a false ownership conflict for that isolated target.
        for line_number, line in active_full_runtime_lines(source):
            if TRANSPORT_TYPE in line:
                errors.append(f"{path.relative_to(ROOT)}:{line_number}: transport CharacterBody referenced in full runtime")
        if label == "full character":
            if f"namespace {ORIGINAL_TYPE.rsplit('.', 1)[0]};" not in source:
                errors.append("full character's namespace changed")
            for type_name in REQUIRED_COMPONENTS:
                if not re.search(r"GetComponent<" + type_name + r">\s*\(", source):
                    errors.append(f"original Awake component lookup missing: {type_name}")
            for limb in REQUIRED_LIMBS:
                if not re.search(r"\b" + limb + r"\b", source):
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
