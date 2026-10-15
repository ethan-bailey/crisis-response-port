#!/usr/bin/env python3
"""Losslessly repack OBJ corner attributes into the same Float32 values Three uses."""
import hashlib,json,struct,sys
from pathlib import Path
src=Path(sys.argv[1]);out=Path(sys.argv[2]);vs=[];ns=[];uvs=[];corners=[];lookup={};indices=[]
for line in src.read_text().splitlines():
 p=line.split()
 if not p:continue
 if p[0]=='v':vs.append(tuple(map(float,p[1:4])))
 elif p[0]=='vn':ns.append(tuple(map(float,p[1:4])))
 elif p[0]=='vt':uvs.append(tuple(map(float,p[1:3])))
 elif p[0]=='f':
  face=[tuple(int(v) if v else 0 for v in x.split('/')) for x in p[1:]]
  if len(face)!=3:raise ValueError('Expected original triangular mesh')
  for key in face:
   if any(x<=0 for x in key) or len(key)!=3:raise ValueError('Expected positive position/UV/normal indices')
   if key not in lookup:lookup[key]=len(corners);corners.append(key)
   indices.append(lookup[key])
positions=[v for i,_,_ in corners for v in vs[i-1]]
normals=[v for _,_,i in corners for v in ns[i-1]]
texcoords=[v for _,i,_ in corners for v in uvs[i-1]]
blob=struct.pack('<4sIII',b'CRG1',len(corners),len(indices),3)
for arr in [positions,normals,texcoords]:blob+=struct.pack('<'+'f'*len(arr),*arr)
blob+=struct.pack('<'+'I'*len(indices),*indices)
out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes(blob)
report={'sourceSha256':hashlib.sha256(src.read_bytes()).hexdigest(),'binarySha256':hashlib.sha256(blob).hexdigest(),'vertices':len(corners),'triangles':len(indices)//3,'sourceBytes':src.stat().st_size,'binaryBytes':len(blob),'attributes':'original corner positions/normals/UVs, Float32; original triangle order'}
out.with_suffix('.report.json').write_text(json.dumps(report,indent=2));print(json.dumps(report))
