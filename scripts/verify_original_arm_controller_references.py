#!/usr/bin/env python3
"""Fail closed on modified/misclassified original IK controller developer sources."""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PROVENANCE = ROOT / "source-reference/Original/Animation/source-provenance.json"
EXPECTED_ARCHIVE = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
EXPECTED_NAMES = {"IKArmType", "IKArmSequencer", "IKArmController", "ArmIKSolver", "CharacterBody"}

def verify(root=ROOT):
    manifest_path = root / PROVENANCE.relative_to(ROOT)
    manifest = json.loads(manifest_path.read_text())
    if manifest["archiveSha256"] != EXPECTED_ARCHIVE:
        raise ValueError("Unexpected original game recovery archive")
    if not manifest["originalSourceUnmodified"] or manifest["originalControllerPlaybackImplemented"]:
        raise ValueError("Unverified original source or false controller playback claim")
    if set(manifest["classes"]) != EXPECTED_NAMES:
        raise ValueError("Original controller source-class coverage changed")
    if manifest["compiledOriginalClassNames"] != ["IKArmType"]:
        raise ValueError("New class selected for runtime without a source-reviewed build")
    if set(manifest["sourceOnlyOriginalClassNames"]) != EXPECTED_NAMES - {"IKArmType"}:
        raise ValueError("Uncompiled original source classification changed")
    for name, spec in manifest["classes"].items():
        path = (root / spec["repositoryPath"]).resolve()
        if not path.is_relative_to(root.resolve()) or not path.is_file():
            raise ValueError("Invalid original file path " + name)
        raw = path.read_bytes()
        if hashlib.sha256(raw).hexdigest() != spec["sha256"] or len(raw) != spec["bytes"]:
            raise ValueError("Modified original developer source: " + name)
        if spec["originalPath"] != "Assets/Scripts/Assembly-CSharp/" + name + ".cs":
            raise ValueError("Original decompiler source path changed")
        if name == "IKArmType":
            if spec["status"] != "compiled_by_runtime" or path != root / "runtime/Original/Animation/IKArmType.cs":
                raise ValueError("Real original arm enum not included in compile")
            if raw != b"public enum IKArmType\n{\n\tRight = 0,\n\tLeft = 1\n}\n":
                raise ValueError("Original arm direction values mismatch")
        elif spec["status"] != "preserved_not_compiled" or not spec["repositoryPath"].startswith("source-reference/Original/Animation/"):
            raise ValueError("Unverified character/controller class was included in game compilation")
    seq=(root / manifest["classes"]["IKArmSequencer"]["repositoryPath"]).read_text()
    ctl=(root / manifest["classes"]["IKArmController"]["repositoryPath"]).read_text()
    solver=(root / manifest["classes"]["ArmIKSolver"]["repositoryPath"]).read_text()
    for condition in (
        "frameDone = Time.time - timestamp >= frameTime;" in seq,
        "frameDone = solver.OnTarget;" in seq,
        "frameDone = self.WeaponAimAlignment() <= 10f;" in seq,
        "frameDone = sibling.HasFinishedFrame(waitForSiblingFrame);" in seq,
        "controller.IK_HolsterWeapon();" in seq,
        "controller.IK_ActivatePendingWeapon();" in seq,
        "self.ActivateWeapon(PendingWeapon, 0.05f);" in ctl,
        "RightSequencer.Update();" in ctl and "LeftSequencer.Update();" in ctl,
        "InverseKinematics2D.SolveTwoBoneAnalytic" in solver,
        "public bool OnTarget { get; private set; }" in solver,
    ):
        if not condition:
            raise ValueError("Original arm-controller source method/callback no longer matches")
    return {"status": "pass", "originalClasses": 5, "compiledOriginalClassCount": 1,
            "preservedSourceOnlyClasses": 4,
            "controllerPlaybackImplemented": False,
            "originalArchiveSha256": EXPECTED_ARCHIVE}

if __name__ == "__main__":
    print(json.dumps(verify(),sort_keys=True))
