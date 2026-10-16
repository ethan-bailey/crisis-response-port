#!/usr/bin/env python3
"""Read-only fail-closed checkpoint for original ArmController physics closure.

This is a dependency audit, not a replacement ArmController or a physics simulator.
Run from repository root: python3 scripts/check_original_arm_physics_closure.py
Exit 0 means the source constraints were verified, NOT that playback is implemented.
"""
from pathlib import Path
import hashlib
import json
import re

ROOT = Path(__file__).resolve().parents[1]
ARM = ROOT / "source-reference/Original/Animation/ArmController.cs"
ENGINE = ROOT / "runtime/Engine/Physics.cs"
EXPECTED_ARM_SHA256 = "5b98da4624c609f57fe1ec59adbe49b5daecf8d6380a7617711aa764bb641b3c"

REQUIRED_SOURCE = {
    "arm rigids": r"upperArm\.GetComponent<Rigidbody>\(\).*lowerArm\.GetComponent<Rigidbody>\(\).*hand\.GetComponent<Rigidbody>\(\)",
    "arm hinges": r"upperArm\.GetComponent<HingeJoint>\(\).*lowerArm\.GetComponent<HingeJoint>\(\).*hand\.GetComponent<HingeJoint>\(\)",
    "force placement": r"hand\.transform\.TransformPoint\(0f, -0\.1f, 0f\).*lowerArmRb\.AddForceAtPosition\(force, position, ForceMode\.Acceleration\)",
    "force target": r"hand\.transform\.TransformPoint\(0f, -0\.05f, 0f\)",
    "planar force": r"vector2\.z = 0f",
    "target clamp": r"Mathf\.Clamp\(magnitude, minimumForce, maxRange\)",
    "velocity timestep": r"forceFactor \* Time\.deltaTime \* 60f",
    "hinge stiffness": r"spring\.spring = 500f",
    "hinge damping": r"spring\.damper = 10f",
    "hinge angle": r"spring\.targetPosition = upperArmJoint\.angle",
    "limb kinematic": r"upperArmRb\.isKinematic = state;.*lowerArmRb\.isKinematic = state;.*handRb\.isKinematic = state;",
}

def audit():
    original_bytes = ARM.read_bytes()
    original = original_bytes.decode("utf-8")
    engine = ENGINE.read_text(encoding="utf-8")
    checks = {key: bool(re.search(pattern, original, re.S)) for key, pattern in REQUIRED_SOURCE.items()}
    source_sha = hashlib.sha256(original_bytes).hexdigest()
    checks["immutable_original_sha256"] = source_sha == EXPECTED_ARM_SHA256
    # Deliberately scan only engine classes, not source references.
    # Presence of an API signature would not prove correct PhysX dynamics.
    runtime_api = {
        "Rigidbody": bool(re.search(r"\bclass\s+Rigidbody\b", engine)),
        "HingeJoint": bool(re.search(r"\bclass\s+HingeJoint\b", engine)),
        "JointSpring": bool(re.search(r"\bstruct\s+JointSpring\b", engine)),
        "AddForceAtPosition": "AddForceAtPosition" in engine,
    }
    return {
        "original_script": str(ARM.relative_to(ROOT)),
        "original_sha256": source_sha,
        "original_source_checks": checks,
        "runtime_physics_api_in_Physics_cs": runtime_api,
        "source_integrity_pass": all(checks.values()),
        "arm_controller_compiled_by_this_check": False,
        "physics_playback_verified": False,
        "note": "API presence alone is insufficient: physical integration needs actual original joint settings, PhysX force/torque and spring tests."
    }

if __name__ == "__main__":
    report = audit()
    print(json.dumps(report, sort_keys=True, indent=2))
    if not report["source_integrity_pass"]:
        raise SystemExit("Original ArmController provenance/contract changed")
