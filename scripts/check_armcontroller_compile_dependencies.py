#!/usr/bin/env python3
"""Static compile gate for unchanged developer ArmController; no fake C# types."""
import hashlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "source-reference/Original/Animation/ArmController.cs"
EXPECTED_SHA256 = "5b98da4624c609f57fe1ec59adbe49b5daecf8d6380a7617711aa764bb641b3c"
DEPENDENCIES = {
    "CharacterBody": ("body.Kinematic", "body.ActiveWeapon", "body.PendingGrenadeType",
                      "body.stunGrenadeAnchor", "body.fragGrenadeAnchor", "body.transform.position"),
    "ArmsControl": ("Initialize(ArmsControl controller, CharacterBody body)",),
    "WeaponBase": ("body.ActiveWeapon.leftHandAnchor", "body.ActiveWeapon.rightHandAnchor"),
    "Grenade": ("Grenade.Type.Frag",),
    "Rigidbody": ("GetComponent<Rigidbody>()", "AddForceAtPosition(", ".isKinematic"),
    "HingeJoint": ("GetComponent<HingeJoint>()", ".useSpring", ".spring", ".angle"),
    "Hand": ("hand.SetMode(", "hand.CurrentMode"),
    "Transform": ("TransformPoint(", ".position"),
    "UnityEngine.Time": ("Time.deltaTime",),
    "UnityEngine.Mathf": ("Mathf.Clamp(",),
    "UnityEngine.JointSpring": ("JointSpring spring",),
}
def verify():
    raw = SOURCE.read_bytes()
    actual = hashlib.sha256(raw).hexdigest()
    if actual != EXPECTED_SHA256:
        raise ValueError("Original developer source SHA mismatch: " + actual)
    source = raw.decode("utf-8")
    for name, terms in DEPENDENCIES.items():
        for term in terms:
            if term not in source:
                raise ValueError("Unexpected source change in %s: %s" % (name, term))
    compiled_path = ROOT / "runtime/Original/Animation/ArmController.cs"
    if compiled_path.exists() and compiled_path.read_bytes() != raw:
        raise ValueError("Runtime ArmController differs from recovered developer source")
    return {"sourceSha256": actual, "dependencyGroups": len(DEPENDENCIES),
            "developerSourceUnmodified": True,
            "includedInRuntime": compiled_path.exists(),
            "originalControllerCompilationVerified": False,
            "note": "Static source gate only. Real types and APIs must close before compiling."}
if __name__ == "__main__":
    import json
    print(json.dumps(verify(), sort_keys=True))
