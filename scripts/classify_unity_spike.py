#!/usr/bin/env python3
"""Classify Unity 2022 import/build logs without interpreting non-runs as failures."""
import argparse
import json
import re
from pathlib import Path
from collections import Counter

def main():
    p=argparse.ArgumentParser()
    p.add_argument("--log", required=True)
    p.add_argument("--build-log")
    p.add_argument("--status-file")
    p.add_argument("--out", required=True)
    args=p.parse_args()
    status={}
    if args.status_file and Path(args.status_file).is_file():
        for line in Path(args.status_file).read_text(errors="replace").splitlines():
            if "=" in line:
                k,v=line.split("=",1)
                status[k]=v
    path=Path(args.log)
    log=path.read_text(errors="replace") if path.is_file() else ""
    buildpath=Path(args.build_log) if args.build_log else None
    build=buildpath.read_text(errors="replace") if buildpath and buildpath.is_file() else ""
    lines=log.splitlines()
    buckets={
        "compile": re.compile(r"\berror\s+CS\d{4}\b|\berror\s+BC\d+\b",re.I),
        "api_obsolete":re.compile(r"\berror\s+CS0619\b|\bis obsolete\b|removed API|obsolete API",re.I),
        "missing_guid_script": re.compile(r"missing \(mono script\)|missing script|the referenced script|could not be loaded|GUID.*(?:missing|not found)|missing.*GUID|\.meta.*(?:not found|missing)",re.I),
        "asset_import": re.compile(r"asset import (?:failed|error)|failed to import|importer.*(?:error|failed)|texture import failed|model import failed|failed importing|unable to load asset",re.I),
        "license":re.compile(r"licen[sc](?:e|ing).*?(?:error|fail|invalid|not found|unavailable)|no valid Unity Editor license|entitlement.*(?:fail|unavailable)|Activation.*failed",re.I),
        "editor_failure":re.compile(r"failed to launch|segmentation fault|could not start|out of memory|no space left on device|docker.*(?:error|fail)|Unable to find image",re.I),
        "il2cpp_risk":re.compile(r"Reflection\.Emit|DynamicMethod|System\.CodeDom|Assembly\.Load\(",re.I)
    }
    samples={}
    counts={}
    for key,regex in buckets.items():
        hits=[line.strip()[:500] for line in lines if regex.search(line)]
        counts[key]=len(hits)
        samples[key]=list(dict.fromkeys(hits))[:12]
    csids=Counter(re.findall(r"\berror\s+(CS\d{4})\b",log,re.I))
    build_lines=[line.strip()[:500] for line in build.splitlines() if re.search(
        r"\berror\b|\bfail(?:ed|ure)?\b|buildresult|BuildPlayer|Exception",line,re.I)]
    # Unity Bee prints [499/570 2s] phase tags even if the player later
    # times out. Retain the last numbered step as evidence, not an ETA.
    progress_re=re.compile(r"(?m)^\\[(\\d+)/(\\d+)\\s+[^\\]]*\\]\\s+([^\\r\\n]+)")
    matches=list(progress_re.finditer(build))
    build_progress=None
    if matches:
        item=matches[-1]
        build_progress={
            "last_completed_step":int(item.group(1)),
            "total_steps_reported":int(item.group(2)),
            "last_operation":item.group(3).strip()[:180],
            "matched_numbered_steps":len(matches),
            "caveat":"Step count is not a percentage of build time; linking/postprocessing may take longer."
        }
    output={"log_exists":path.is_file(),"log_lines":len(lines),"status":status,
            "counts":counts,"compile_codes":dict(csids),"examples":samples,
            "build_log_exists":bool(buildpath and buildpath.is_file()),
            "build_progress":build_progress,
            "build_failures":list(dict.fromkeys(build_lines))[:30],
            "caveat":"Heuristic line counts, not unique asset/script defect counts. Counts are not proof of success or failure without editor run status."}
    Path(args.out).parent.mkdir(parents=True,exist_ok=True)
    Path(args.out).write_text(json.dumps(output,indent=2)+"\n")
    print(json.dumps({"counts":counts,"compile_codes":dict(csids),"status":status,"build_log_exists":output["build_log_exists"]},indent=2))
if __name__=="__main__":main()
