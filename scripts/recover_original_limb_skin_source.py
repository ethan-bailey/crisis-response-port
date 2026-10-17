#!/usr/bin/env python3
"""Preserve the original LimbRenderer texture-data/trace dependencies verbatim."""
from pathlib import Path
import hashlib,json,sys,zipfile
PIN="cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
PREFIX="export/ExportedProject/Assets/Scripts/Assembly-CSharp/"
NAMES=("LimbTextureData.cs","LimbRenderer.cs","LimbLayer.cs")
def recover(archive,out):
 if hashlib.sha256(archive.read_bytes()).hexdigest()!=PIN:raise ValueError("Original archive identity mismatch")
 out.mkdir(parents=True,exist_ok=True);provenance={}
 with zipfile.ZipFile(archive) as z:
  for name in NAMES:
   path=PREFIX+name
   if path not in z.namelist():raise ValueError("Original file missing: "+name)
   raw=z.read(path);(out/name).write_bytes(raw)
   provenance[name]={'archivePath':path,'sha256':hashlib.sha256(raw).hexdigest()}
 (out/'provenance.json').write_text(json.dumps({'archiveSha256':PIN,'sources':provenance},indent=2,sort_keys=True)+'\n')
 print(json.dumps(provenance))
if __name__=="__main__":recover(Path(sys.argv[1]),Path(sys.argv[2]))
