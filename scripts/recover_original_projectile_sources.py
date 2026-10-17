#!/usr/bin/env python3
"""Preserve original projectile and contact-to-limb sources from pinned game export.
Reference sources are not executable merely because they are recovered.
"""
import hashlib,json,sys,zipfile
from pathlib import Path
PIN="cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d"
PREFIX="export/ExportedProject/Assets/Scripts/Assembly-CSharp/"
NAMES=("Projectile.cs","LimbRenderer.cs","LimbArmor.cs","LimbVitalFunction.cs",
       "VitalFunctions.cs","Physiology.cs")
def main(archive,out):
 if hashlib.sha256(archive.read_bytes()).hexdigest()!=PIN:
  raise ValueError("Original source archive checksum mismatch")
 out.mkdir(parents=True,exist_ok=True)
 manifest={}
 with zipfile.ZipFile(archive) as zipped:
  for name in NAMES:
   original=PREFIX+name
   if original not in zipped.namelist():raise ValueError("Original developer script absent: "+name)
   raw=zipped.read(original)
   # This is immutable developer-authored source; no adapted substitutions.
   (out/name).write_bytes(raw)
   manifest[name]={"archivePath":original,"sha256":hashlib.sha256(raw).hexdigest(),"bytes":len(raw)}
 (out/"provenance.json").write_text(json.dumps({"archiveSha256":PIN,"sources":manifest},sort_keys=True,indent=2)+"\n")
 print(json.dumps(manifest,sort_keys=True))
if __name__=="__main__":
 if len(sys.argv)!=3:raise SystemExit("usage: recover_original_projectile_sources.py ARCHIVE DEST")
 main(Path(sys.argv[1]),Path(sys.argv[2]))
