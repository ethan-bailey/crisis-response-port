#!/usr/bin/env python3
"""Verify five immutable original limb/weapon classes, with real Hand.cs selected for C#."""
import hashlib
import json
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
MANIFEST=ROOT/"source-reference/Original/ArmDependencies/source-provenance.json"
EXPECTED={
    "ArmsControl":"3e623b5568950ff4efe6ad280df84e41566af31e4a03dba08c30d6a7ce34162c",
    "Hand":"d5e6b25be76a184dc6b117ae35127018b0cb65eccaa69db9f6d8922434e76537",
    "Limb":"4057e0b5dbb0fd4c7cab847f6168d655dc09fdabf0564587b403274210e6b8de",
    "TeamOperator":"cbc91bfe42d2d5c5c3562e20e16fbf60348af13309697016c3f3d2c9476e7ba7",
    "WeaponBase":"cc60b54d30225be4d0f770d4bdaaab1ebd32e805be98ba5704745c48ed1be979",
}
ARCHIVE="cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
def verify(root=ROOT):
    m=json.loads((root/MANIFEST.relative_to(ROOT)).read_text())
    if m["archiveSha256"]!=ARCHIVE or not m["originalSourceUnmodified"]:
        raise ValueError("Wrong or modified original source archive")
    if m["controllerPlaybackImplemented"] or m["compiledOriginalCount"]!=1:
        raise ValueError("False controller playback/compiled-source claim")
    if m["engineTypesNotDeveloperScripts"]!=["UnityEngine.Joint"]:
        raise ValueError("Unity Joint must be handled as a real engine type, never a fake original developer class")
    if set(m["classes"])!=set(EXPECTED):
        raise ValueError("Missing original class")
    for name, digest in EXPECTED.items():
        entry=m["classes"][name]
        actual=root/entry["repositoryPath"]
        if not actual.resolve().is_relative_to(root.resolve()) or not actual.is_file():
            raise ValueError("Unsafe or missing developer source path: "+name)
        data=actual.read_bytes()
        if hashlib.sha256(data).hexdigest()!=digest or entry["sha256"]!=digest or len(data)!=entry["bytes"]:
            raise ValueError("Original class source bytes differ: "+name)
        if entry["sourcePath"]!="Assets/Scripts/Assembly-CSharp/"+name+".cs":
            raise ValueError("Original developer path changed: "+name)
        if name=="Hand":
            if entry["repositoryPath"]!="runtime/Original/Animation/Hand.cs" or entry["status"]!="selected_original_runtime_source":
                raise ValueError("Original Hand is not actually selected by runtime compile glob")
            text=data.decode("utf-8-sig")
            if not all(term in text for term in ("public enum Mode","Trigger = 0","Fist = 1","ForwardAssist = 2","public void SetMode(Mode mode)","CurrentMode = mode;","SetActive(")):
                raise ValueError("Actual original Hand.SetMode implementation no longer preserved")
        elif entry["status"]!="original_recovered_not_compiled" or not entry["repositoryPath"].startswith("source-reference/Original/ArmDependencies/"):
            raise ValueError("Unreviewed larger original class selected for compilation: "+name)
    return {"status":"pass","originalClasses":5,"compiledSourceClasses":1,
            "preservedUncompiledClasses":4,"originalHandSetModeSelected":True,
            "actualIKControllerPlayback":False}

if __name__=="__main__":
    print(json.dumps(verify(),sort_keys=True))
