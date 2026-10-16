#!/usr/bin/env python3
"""Source-identity and execution-closure audit; deliberately does not simulate game logic."""
from pathlib import Path
import re
import json

root = Path(__file__).resolve().parents[1]
source_root = root / "source-reference/Original/Animation"
runtime_root = root / "runtime/Original/Animation"
required = {
    "IKArmSequencer.cs": (
        "SetSibling", "Update", "Play", "NewFrame", "TargetTransform",
        "Invoke", "HasFinishedFrame"
    ),
    "IKArmController.cs": (
        "Awake", "Update", "Play", "SwitchWeapons", "HolsterWeapon",
        "IK_ActivatePendingWeapon", "IK_FireWeapon", "IK_Restrain"
    ),
    "CharacterBody.cs": (
        "Awake", "Start", "Update", "FixedUpdate", "LateUpdate",
        "SetKinematic", "SetRagdoll", "UpdateAnimation", "OnDeath",
        "OnWasHit", "WeaponAimAlignment"
    )
}

def has_method(text, name):
    return bool(re.search(r"\b" + re.escape(name) + r"\s*\([^;]*\)\s*(?:\{|=>)", text))

audit = {}
for filename, methods in required.items():
    original = (source_root / filename).read_text()
    compiled_path = runtime_root / filename
    compiled = compiled_path.read_text() if compiled_path.exists() else ""
    source_methods = [m for m in methods if has_method(original, m)]
    if len(source_methods) != len(methods):
        raise SystemExit(f"Expected recovered source method missing in {filename}: {set(methods)-set(source_methods)}")
    audit[filename] = {
        "source_methods": source_methods,
        "compiled_source_file_present": compiled_path.exists(),
        "compiled_methods_in_matching_file": [m for m in methods if has_method(compiled, m)],
        "uncompiled_methods_in_matching_file": [m for m in methods if not has_method(compiled, m)]
    }
for filename in ("ArmController.cs", "ArmsControl.cs", "Hand.cs", "IKArmSequence.cs", "IKArmSequenceRuntime.cs"):
    if not (runtime_root / filename).is_file():
        raise SystemExit(f"Recovered original component missing: {filename}")
# A note or a host method named alike is not source method execution.
assert not audit["IKArmSequencer.cs"]["compiled_source_file_present"]
assert not audit["IKArmController.cs"]["compiled_source_file_present"]
assert not audit["CharacterBody.cs"]["compiled_source_file_present"]
print(json.dumps({"source_character_animation_closure": audit}, indent=2))
