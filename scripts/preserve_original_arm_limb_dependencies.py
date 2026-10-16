#!/usr/bin/env python3
"""Preserve exact developer Hand/Limb/Joint/Weapon classes as non-compiled private references.

No gameplay is reimplemented. The archive checksum is the immutable input;
original source files stay outside the runtime/Original/**/*.cs compiler glob.
"""
import argparse
import hashlib
import json
from pathlib import Path
import zipfile

ARCHIVE_SHA="cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
PREFIX="export/ExportedProject/Assets/Scripts/Assembly-CSharp/"
NAMES=("Hand","Limb","WeaponBase","TeamOperator","ArmsControl")

def sha(data): return hashlib.sha256(data).hexdigest()

def preserve(archive: Path, root: Path):
    if sha(archive.read_bytes()) != ARCHIVE_SHA:
        raise ValueError("Original game archive SHA256 mismatch")
    out = root/"source-reference/Original/ArmDependencies"
    out.mkdir(parents=True, exist_ok=True)
    files={}
    with zipfile.ZipFile(archive) as z:
        available=set(z.namelist())
        for name in NAMES:
            source = PREFIX+name+".cs"
            if source not in available:
                raise ValueError("Missing original developer source: "+source)
            data = z.read(source)
            source_text=data.decode("utf-8-sig")
            if not data or not (("class "+name) in source_text or ("enum "+name) in source_text or ("struct "+name) in source_text):
                raise ValueError("Original source type declaration not verified: "+name)
            if source_text.count("{")!=source_text.count("}"):
                raise ValueError("Unexpected source braces: "+name)
            compiled_target = root/"runtime/Original/Animation/Hand.cs" if name=="Hand" else None
            if compiled_target is not None and compiled_target.exists():
                if compiled_target.read_bytes() != data:
                    raise ValueError("Actual compiled original Hand source has been modified")
                path=compiled_target
                status="selected_original_runtime_source"
            else:
                path=out/(name+".cs")
                if path.exists() and path.read_bytes()!=data:
                    raise ValueError("Refusing to overwrite altered original source "+name)
                path.write_bytes(data)
                status="original_recovered_not_compiled"
            files[name]={
                "sourcePath": source.removeprefix("export/ExportedProject/"),
                "repositoryPath": path.relative_to(root).as_posix(),
                "sha256": sha(data), "bytes":len(data),
                "lines":len(source_text.splitlines()),
                "status":status
            }
    manifest={
        "schema":1,"archiveSha256":ARCHIVE_SHA,
        "originalSourceUnmodified":True,"compiledOriginalCount":sum(v["status"]=="selected_original_runtime_source" for v in files.values()),
        "controllerPlaybackImplemented":False,
        "classes":files,
        "engineTypesNotDeveloperScripts":["UnityEngine.Joint"],
        "scope":"Preserved true original source dependencies, not fake gameplay runtime or executed animation"
    }
    dest=out/"source-provenance.json"
    raw=json.dumps(manifest,indent=2,sort_keys=True)+"\n"
    if dest.exists() and dest.read_text()!=raw:
        raise ValueError("Refusing to replace inconsistent source provenance")
    dest.write_text(raw)
    return {name:dict(bytes=r["bytes"],sha256=r["sha256"],lines=r["lines"]) for name,r in files.items()}

if __name__=="__main__":
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument("archive",type=Path)
    p.add_argument("--repo",type=Path,default=Path("."))
    a=p.parse_args()
    print(json.dumps(preserve(a.archive,a.repo),sort_keys=True))
