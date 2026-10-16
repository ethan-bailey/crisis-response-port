#!/usr/bin/env python3
"""Preserve exact original developer ArmController source, do not compile missing Unity dependencies."""
import hashlib,json,re,zipfile
from pathlib import Path
ARCHIVE_SHA="cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
SRC="export/ExportedProject/Assets/Scripts/Assembly-CSharp/ArmController.cs"
OUT=Path("source-reference/Original/Animation/ArmController.cs")
REPORT=Path("source-reference/Original/Animation/armcontroller-provenance.json")
def sha(b):return hashlib.sha256(b).hexdigest()
def run(archive):
    if sha(archive.read_bytes())!=ARCHIVE_SHA:raise ValueError("Recovery archive changed")
    with zipfile.ZipFile(archive) as z:blob=z.read(SRC)
    s=blob.decode("utf-8-sig")
    if "public class ArmController" not in s or s.count("{")!=s.count("}"):raise ValueError("Original class malformed")
    OUT.parent.mkdir(parents=True,exist_ok=True)
    if OUT.exists() and OUT.read_bytes()!=blob:raise ValueError("Will not overwrite source")
    OUT.write_bytes(blob)
    deps=sorted(set(re.findall(r"\b(?:public|private|protected)\s+(?:[\w<>\[\],]+)\s+(\w+)\s*\(",s)))
    types=sorted(set(re.findall(r"\b(?:CharacterBody|WeaponBase|Limb|Hand|Joint|HingeJoint|ArmController|ArmsControl|ArmIKSolver|MonoBehaviour|Rigidbody|Transform)\b",s)))
    report={"archiveSha256":ARCHIVE_SHA,"originalPath":SRC.removeprefix("export/ExportedProject/"),
       "repositoryPath":str(OUT),"sha256":sha(blob),"bytes":len(blob),"lines":len(s.splitlines()),
       "methods":deps,"typeReferences":types,
       "originalSourceUnchanged":True,"compiled":False,"controllerPlaybackImplemented":False}
    REPORT.write_text(json.dumps(report,indent=2,sort_keys=True)+"\n")
    print(json.dumps(report,sort_keys=True))
    print("SOURCE_EXCERPT_FIRST_130_LINES")
    for n,line in enumerate(s.splitlines()[:130],1):print(f"{n:04d} {line}")
if __name__=="__main__":
    import sys
    run(Path(sys.argv[1]))
