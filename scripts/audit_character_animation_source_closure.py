#!/usr/bin/env python3
"""Audit preserved animation methods against the current FullSource compile inputs."""
from pathlib import Path
import hashlib
import json
import re

ROOT = Path(__file__).resolve().parents[1]
SPECS = {
    "IKArmSequencer.cs": {
        "source": "source-reference/Original/Animation/IKArmSequencer.cs",
        "runtime": "runtime/Original/FullSource/IKArmSequencer.cs",
        "methods": (
            "SetSibling", "Update", "Play", "NewFrame", "TargetTransform",
            "Invoke", "HasFinishedFrame",
        ),
    },
    "IKArmController.cs": {
        "source": "source-reference/Original/Animation/IKArmController.cs",
        "runtime": "runtime/Original/FullSource/IKArmController.cs",
        "methods": (
            "Awake", "Update", "Play", "SwitchWeapons", "HolsterWeapon",
            "IK_ActivatePendingWeapon", "IK_FireWeapon", "IK_Restrain",
        ),
    },
    "CharacterBody.cs": {
        "source": "source-reference/Original/Animation/CharacterBody.cs",
        "runtime": "runtime/Original/CharacterBody.Full.cs",
        "methods": (
            "Awake", "Start", "Update", "FixedUpdate", "LateUpdate",
            "SetKinematic", "SetRagdoll", "UpdateAnimation", "OnDeath",
            "OnWasHit", "WeaponAimAlignment",
        ),
    },
}


def has_method(text: str, name: str) -> bool:
    return bool(re.search(r"\b" + re.escape(name) + r"\s*\([^;]*\)\s*(?:\{|=>)", text))


def sha256(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


audit = {}
for name, spec in SPECS.items():
    source_path = ROOT / spec["source"]
    runtime_path = ROOT / spec["runtime"]
    if not source_path.is_file():
        raise SystemExit(f"Preserved source missing: {spec['source']}")
    if not runtime_path.is_file():
        raise SystemExit(f"Current runtime source missing: {spec['runtime']}")
    source = source_path.read_text()
    runtime = runtime_path.read_text()
    source_methods = [method for method in spec["methods"] if has_method(source, method)]
    runtime_methods = [method for method in spec["methods"] if has_method(runtime, method)]
    missing_source = sorted(set(spec["methods"]) - set(source_methods))
    if missing_source:
        raise SystemExit(f"Expected original source methods missing in {name}: {missing_source}")
    audit[name] = {
        "sourceFile": spec["source"],
        "sourceSha256": sha256(source_path),
        "runtimeFile": spec["runtime"],
        "runtimeSha256": sha256(runtime_path),
        "requiredMethods": list(spec["methods"]),
        "sourceMethodsPresent": source_methods,
        "runtimeMethodsPresent": runtime_methods,
        "runtimeMethodsMissing": sorted(set(spec["methods"]) - set(runtime_methods)),
    }

for filename in ("ArmController.cs", "ArmsControl.cs", "Hand.cs", "IKArmSequence.cs", "IKArmSequenceRuntime.cs"):
    if not (ROOT / "runtime/Original/Animation" / filename).is_file():
        raise SystemExit(f"Recovered original animation component missing: {filename}")

print(json.dumps({
    "status": "pass",
    "scope": "source/runtime path and required method-presence audit; not gameplay or visual parity",
    "source_character_animation_closure": audit,
}, indent=2))
