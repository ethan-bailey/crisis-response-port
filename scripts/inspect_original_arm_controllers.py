#!/usr/bin/env python3
"""Inspect selected *unchanged* recovered original C# source; no stub generation.

Source input: private pinned Crisis Response recovery ZIP. Stages original files
outside runtime/Original so merely inspecting them never changes WASM inputs.
The report is private; never attach proprietary source to a public issue.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import zipfile

ARCHIVE_SHA = "cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
NAMES = ("IKArmSequencer", "IKArmController", "ArmIKSolver", "CharacterBody")
PREFIX = "export/ExportedProject/Assets/Scripts/Assembly-CSharp/"
IMPORTANT = re.compile(r"\b(class|struct|enum|interface|void|IEnumerator|IKArm|ArmIK|CharacterBody|Update|FixedUpdate|Start|Awake|callback|event|delegate|Sequence|Coroutine|YieldInstruction)\b", re.I)

def report(zip_path: Path, out: Path) -> None:
    digest = hashlib.sha256(zip_path.read_bytes()).hexdigest()
    if digest != ARCHIVE_SHA:
        raise ValueError("Original archive hash mismatch: " + digest)
    out.mkdir(parents=True, exist_ok=True)
    files = []
    with zipfile.ZipFile(zip_path) as src:
        names = set(src.namelist())
        for name in NAMES:
            path = PREFIX + name + ".cs"
            if path not in names:
                raise FileNotFoundError("Original source is missing: " + path)
            data = src.read(path)
            text = data.decode("utf-8-sig")
            if not data or text.count("{") != text.count("}"):
                raise ValueError("Unexpected original source: " + path)
            target = out / (name + ".cs")
            target.write_bytes(data)
            lines = text.splitlines()
            matches = [f"{i+1}: {line.strip()}" for i, line in enumerate(lines)
                       if IMPORTANT.search(line)]
            files.append({"name":name,"sourcePath":path.removeprefix("export/ExportedProject/"),
                          "sha256":hashlib.sha256(data).hexdigest(),"bytes":len(data),
                          "lines":len(lines),"interestingLines":matches[:160],
                          "methodMatches":len(matches)})
    summary={"archiveSha256":digest,"classes":files,
             "compilationImplemented":False,"gameplayImplemented":False,
             "scope":"recovered source inventory only, no fabricated game adapters"}
    (out/"source-review.json").write_text(json.dumps(summary,indent=2)+"\n")
    # Bounded console review for code selection, without dumping an entire proprietary game.
    for item in files:
        print(f"ORIGINAL_CLASS {item['name']} BYTES {item['bytes']} LINES {item['lines']} SHA256 {item['sha256']}")
        print("SOURCE_BEGIN "+item["name"])
        data=(out/(item['name']+".cs")).read_text("utf-8-sig")
        cap=210 if item['name'] in ("IKArmSequencer","IKArmController") else 110
        for i,line in enumerate(data.splitlines()[:cap]):
            print(f"{i+1:04d} {line}")
        print("SOURCE_END "+item["name"])
    print("SOURCE_REVIEW_COMPLETE "+json.dumps({f["name"]:f["sha256"] for f in files},sort_keys=True))

if __name__=="__main__":
    p=argparse.ArgumentParser()
    p.add_argument("archive",type=Path)
    p.add_argument("--out",type=Path,default=Path("work/arm-controller-source"))
    a=p.parse_args()
    report(a.archive,a.out)
