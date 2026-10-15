#!/usr/bin/env python3
"""Source-free manifest for private WebGL build; never embeds binary contents."""
import argparse, hashlib, json
from pathlib import Path
p=argparse.ArgumentParser()
p.add_argument("output",type=Path)
p.add_argument("--out",type=Path,required=True)
a=p.parse_args()
def sha_file(file_path):
 h=hashlib.sha256()
 with file_path.open("rb") as handle:
  for chunk in iter(lambda:handle.read(4*1024*1024),b""):
   h.update(chunk)
 return h.hexdigest()
entries=[]
for f in sorted(a.output.rglob("*")) if a.output.is_dir() else []:
 if f.is_file():
  entries.append({"relative_path":f.relative_to(a.output).as_posix(),"size_bytes":f.stat().st_size,
                  "sha256":sha_file(f)})
names=[x["relative_path"] for x in entries]
state={"index_present":"index.html" in names,
       "wasm_present":any(s.endswith(".wasm") or s.endswith(".wasm.unityweb") or s.endswith(".wasm.br") or s.endswith(".wasm.gz") for s in names),
       "data_present":any(s.endswith(".data") or s.endswith(".data.unityweb") or s.endswith(".data.br") or s.endswith(".data.gz") for s in names),
       "loader_present":any(s.endswith(".loader.js") for s in names),
       "file_count":len(entries),"total_bytes":sum(x["size_bytes"] for x in entries),
       "files":entries}
a.out.parent.mkdir(parents=True,exist_ok=True)
a.out.write_text(json.dumps(state,indent=2)+"\n")
print("WebGL output:",{k:v for k,v in state.items() if k!="files"})
