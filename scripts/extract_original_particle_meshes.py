#!/usr/bin/env python3
"""Recover exact three Unity 2017 particle mesh geometries into private JSON.

No replacement meshes or particle animation are generated. Both decoders already
convert Unity local vertices/normals and triangle winding into Three.js basis.
"""
import argparse
import hashlib
import json
import subprocess
import sys
import tempfile
from pathlib import Path
from extract_runtime_scene import parse_asset
from unity_packed_mesh import compressed_mesh, compressed_normals, unpack

p=argparse.ArgumentParser()
p.add_argument('project',type=Path)
p.add_argument('--out-dir',type=Path,required=True)
a=p.parse_args(); root=a.project
sources={
 '8ed663818ef63e741bd160cb133722fe': ('Assets/Mesh/Particle shard.asset','661b5a9d7f3f45d558e9f6f11864eacb449e031a38561fffe51b6bff3d4fbdb1'),
 'eedec0cf1e8d71a49b3949fbbc2cf3b4': ('Assets/Mesh/Tetraedron.asset','906bd249eb17b8d5e2d41877b53a2eee335bb2590e55cd97de8263d09cc19c3f'),
 'f6fe08f20af97e149b26c77ded5823d7': ('Assets/Mesh/Splinter.asset','37d2f797c49d4bb3055a65a25f50b059ebf41583638e6896fca76f75a4013876')
}
results=[]
for guid,(rel,sha) in sorted(sources.items()):
 src=root/rel
 if not src.is_file() or hashlib.sha256(src.read_bytes()).hexdigest()!=sha:
  raise SystemExit('STOP source original particle mesh path/hash changed: '+rel)
 raw=parse_asset(src,rel)
 mesh=next((r['fields'] for r in raw['records'] if r['classID']==43),None)
 if mesh is None:raise SystemExit('STOP original Mesh serialized record unavailable: '+rel)
 positions,uv,normals,indices=[],[],[],[]
 with tempfile.TemporaryDirectory() as temp:
  outfile=Path(temp)/'original.obj'
  proc=subprocess.run([sys.executable,str(Path(__file__).with_name('unity_mesh_to_obj.py')),str(src),str(outfile)],capture_output=True,text=True)
  if proc.returncode==0:
   fmt='Unity 2017 raw vertex channels (already converted to right-handed)'
   for line in outfile.read_text().splitlines():
    if line.startswith('v '):positions.append([float(n) for n in line[2:].split()])
    elif line.startswith('vt '):uv.append([float(n) for n in line[3:].split()])
    elif line.startswith('vn '):normals.append([float(n) for n in line[3:].split()])
    elif line.startswith('f '):
     tri=[int(item.split('/')[0])-1 for item in line[2:].split()]
     if len(tri)!=3:raise SystemExit('STOP non-triangular original source mesh')
     indices.extend(tri)
  else:
   fmt='Unity 2017 compressed packed vertices (already converted to right-handed)'
   try: verts,faces=compressed_mesh(mesh)
   except (ValueError,KeyError) as exc:
    raise SystemExit('STOP raw and compressed source mesh extraction failed: '+str(exc))
   positions=[[x,y,-z] for x,y,z in verts]
   indices=[i for x,y,z in faces for i in (x,z,y)]
   packed=mesh['m_CompressedMesh']
   uvec=packed.get('m_UV',{})
   if uvec.get('m_NumItems'):
    vals=unpack(uvec,True)
    if len(vals)!=2*len(positions):
     raise SystemExit('STOP compressed source UV count does not match vertices')
    uv=[[float(vals[2*i]),float(vals[2*i+1])] for i in range(len(positions))]
   if packed.get('m_Normals',{}).get('m_NumItems'):
    native_normals=compressed_normals(mesh)
    if len(native_normals)!=len(positions):
     raise SystemExit('STOP decoded source normal count differs from source vertices')
    normals=[[x,y,-z] for x,y,z in native_normals]
 if not positions or len(indices)%3 or not indices:
  raise SystemExit('STOP original mesh has no usable triangle geometry')
 if any(i<0 or i>=len(positions) for i in indices):
  raise SystemExit('STOP original triangle index out of bounds')
 if uv and len(uv)!=len(positions):
  raise SystemExit('STOP source mesh UV vertex count differs')
 if normals and len(normals)!=len(positions):
  raise SystemExit('STOP source normals length differs')
 payload={
  'source':'Recovered original Unity particle mesh (not simulated)',
  'sourceGuid':guid,'sourcePath':rel,'sourceSha256':sha,
  'sourceFormat':fmt,'mesh':{'positions':positions,'uv':uv,'normals':normals,'indices':indices},
  'nativeNormalsPresent':bool(mesh.get('m_CompressedMesh',{}).get('m_Normals',{}).get('m_NumItems')) or bool(normals),
  'hasDecodedSourceNormals':bool(normals),
  'count':{'vertices':len(positions),'triangles':len(indices)//3}
 }
 dest=a.out_dir/('particle-mesh-'+guid+'.json')
 dest.parent.mkdir(parents=True,exist_ok=True)
 dest.write_text(json.dumps(payload,sort_keys=True,separators=(',',':'))+'\n')
 results.append({'guid':guid,'path':rel,'sourceSha256':sha,
                 'convertedVertices':len(positions),'triangles':len(indices)//3,
                 'hasUV':bool(uv),'hasDecodedSourceNormals':bool(normals),
                 'needsNativeNormalDecoding':payload['nativeNormalsPresent'] and not bool(normals),
                 'bytes':dest.stat().st_size})
print(json.dumps({'sourceMeshAssets':len(results),'meshes':results,
                  'note':'Source-only right-handed geometry, no fake particles or native shader parity'},sort_keys=True))
