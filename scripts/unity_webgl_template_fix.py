#!/usr/bin/env python3
"""Fix ONLY AssetRipper's verified blank WebGL template in ephemeral project settings."""
import argparse, json, re
from pathlib import Path
p=argparse.ArgumentParser()
p.add_argument("project",type=Path)
p.add_argument("--out",type=Path,required=True)
args=p.parse_args()
settings=args.project/"ProjectSettings"/"ProjectSettings.asset"
quality=args.project/"ProjectSettings"/"QualitySettings.asset"
text=settings.read_text(encoding="utf-8-sig")
q=quality.read_text(encoding="utf-8-sig")
matches=re.findall(r"(?m)^(  webGLTemplate:)([^\r\n]*)$",text)
if len(matches)!=1:
    raise SystemExit(f"STOP: expected one webGLTemplate property, got {len(matches)}")
current=matches[0][1].strip()
if current and current!="APPLICATION:Default":
    raise SystemExit(f"STOP: template set to unrecognized value {current!r}; preserve it for audit")
profiles=len(re.findall(r"(?m)^    -?\s*name:",q))
# Old QualitySettings YAML has m_QualitySettings list, each entry starts '- serializedVersion:'
qm=re.search(r"(?ms)^  m_QualitySettings:\s*\n(.*?)(?=^  m_PerPlatformDefaultQuality:)",q)
if qm:
    profiles=max(profiles,len(re.findall(r"(?m)^  - ",qm.group(1))))
quality_index_match=re.search(r"(?m)^    WebGL: (\d+)\s*$",q)
quality_index=int(quality_index_match.group(1)) if quality_index_match else None
report={"original_webGLTemplate":current, "new_webGLTemplate":"APPLICATION:Default",
        "quality_profile_count":profiles,"quality_webgl_index":quality_index,
        "quality_index_valid":quality_index is not None and quality_index<profiles}
if not report["quality_index_valid"]:
    raise SystemExit(f"STOP: WebGL quality index invalid or cannot be determined: {report}")
if not current:
    out=re.sub(r"(?m)^  webGLTemplate:\s*$",
               "  webGLTemplate: APPLICATION:Default",text,count=1)
    if out==text:raise SystemExit("STOP: no single template substitution made")
    settings.write_text(out,encoding="utf-8")
args.out.parent.mkdir(parents=True,exist_ok=True)
args.out.write_text(json.dumps(report,indent=2)+"\n")
print("WebGL template configured from blank to APPLICATION:Default; quality index validated")
