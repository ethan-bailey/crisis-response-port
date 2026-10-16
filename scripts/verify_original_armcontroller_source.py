#!/usr/bin/env python3
"""Fail closed on original ArmController source identity and engine requirements.

This verifies exact recovered code and records executable requirements; it does
not claim that original arm-controller physics/weapon callbacks are implemented.
"""
import hashlib,json
from pathlib import Path
root=Path(__file__).resolve().parents[1]
path=root/"source-reference/Original/Animation/ArmController.cs"
manifest_path=root/"source-reference/Original/Animation/armcontroller-provenance.json"
expected="5b98da4624c609f57fe1ec59adbe49b5daecf8d6380a7617711aa764bb641b3c"
archive="cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
manifest=json.loads(manifest_path.read_text())
raw=path.read_bytes()
assert hashlib.sha256(raw).hexdigest()==manifest["sha256"]==expected
assert len(raw)==manifest["bytes"]==6037
assert manifest["archiveSha256"]==archive
assert manifest["originalPath"]=="Assets/Scripts/Assembly-CSharp/ArmController.cs"
assert manifest["compiled"] is False and manifest["controllerPlaybackImplemented"] is False
assert manifest["originalSourceUnchanged"] is True
source=raw.decode("utf-8-sig")
# Confirm real developer enum/methods, spring controls, physics force and original hand-mode bridge.
required=(
    "public class ArmController",
    "HoldWeapon = 0", "HoldGrenade = 1", "TargetTransform = 2",
    "RelativePosition = 3", "Free = 4",
    "public void Initialize(ArmsControl controller, CharacterBody body)",
    "private HingeJoint upperArmJoint;", "private HingeJoint lowerArmJoint;",
    "public bool TargetReached { get; private set; }",
    "private void EnableSpring(bool state)",
    "spring.spring = 500f;", "spring.damper = 10f;",
    "ForceMode.Acceleration",
    "lowerArmRb.AddForceAtPosition(force, position, ForceMode.Acceleration);",
    "public void SetHandMode(Hand.Mode mode)",
    "hand.SetMode(mode);",
)
for needle in required:
    if needle not in source:raise AssertionError("Original developer ArmController contract changed: "+needle)
print(json.dumps({"status":"pass","originalSourceSha256":expected,
  "originalMethodCount":len(manifest["methods"]),"compiled":False,
  "controllerPlaybackImplemented":False,
  "blockedBy":"Original CharacterBody/ArmsControl and Unity Rigidbody/HingeJoint/JointSpring physics behavior"},sort_keys=True))
