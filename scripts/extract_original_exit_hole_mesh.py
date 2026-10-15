#!/usr/bin/env python3
"""Extract source Wood exit-hole prefab render mesh, transform and material binding.

Does not synthesize a visual effect or alter source assets. All coordinates are
converted by the project's pinned unity_mesh_to_obj.py extractor.
"""
import argparse,hashlib,json,re,subprocess,sys,tempfile
from pathlib import Path
from extract_runtime_scene import parse_asset
from unity_packed_mesh import compressed_mesh, unpack

p=argparse.ArgumentParser()
p.add_argument("project",type=Path)
p.add_argument("--out",type=Path,required=True)
args=p.parse_args()
root=args.project
rel="Assets/Resources/effects/Wood exit hole.prefab"
path=root/rel
if not path.is_file():raise SystemExit("STOP missing original exit-hole prefab")
prefab=parse_asset(path,rel)["records"]
def one(typeid):
 hits=[r for r in prefab if r["classID"]==typeid]
 if len(hits)!=1:raise SystemExit(f"STOP prefab component count {typeid}={len(hits)}")
 return hits[0]
mf=one(33)["fields"]
mr=one(23)["fields"]
tr=one(4)["fields"]
meshref=mf["m_Mesh"]
materials=mr.get("m_Materials",[])
if len(materials)!=1 or not meshref.get("guid") or not materials[0].get("guid"):
 raise SystemExit("STOP source renderer/mesh GUID is unavailable or ambiguous")
mg,matg=meshref["guid"],materials[0]["guid"]
index={}
for meta in (root/"Assets").rglob("*.meta"):
 m=re.search(r"(?m)^guid: ([0-9a-f]{32})$",meta.read_text(errors="replace"))
 if m:index[m[1]]=meta.with_suffix("")
if mg not in index or matg not in index:raise SystemExit("STOP original Circle mesh / material files missing")
meshpath=index[mg];matpath=index[matg]
if meshpath.name!="Circle.asset" or matpath.name!="Wood exit hole.mat":
 raise SystemExit(f"STOP unexpected source prefab mesh/material: {meshpath.name}, {matpath.name}")
vertices=[];uvs=[];normals=[];triangles=[]
with tempfile.TemporaryDirectory() as tmp:
 obj=Path(tmp)/"original.obj"
 run=subprocess.run([sys.executable,str(Path(__file__).with_name("unity_mesh_to_obj.py")),str(meshpath),str(obj)],capture_output=True,text=True)
 if run.returncode==0:
  source_format="Unity 2017 raw vertex channel buffers"
  for line in obj.read_text().splitlines():
   if line.startswith("v "):vertices.append([float(x) for x in line[2:].split()])
   elif line.startswith("vt "):uvs.append([float(x) for x in line[3:].split()])
   elif line.startswith("vn "):normals.append([float(x) for x in line[3:].split()])
   elif line.startswith("f "):
    idx=[int(x.split("/")[0])-1 for x in line[2:].split()]
    if len(idx)!=3:raise SystemExit("STOP non-triangle exit-hole source")
    triangles.extend(idx)
 else:
  source_format="Unity 2017 compressed mesh packed floats"
  source_records=parse_asset(meshpath,meshpath.relative_to(root).as_posix())["records"]
  fields=next((r["fields"] for r in source_records if r["classID"]==43),None)
  if fields is None:raise SystemExit("STOP compressed original mesh record not found")
  original_vertices,faces=compressed_mesh(fields)
  vertices=[[x,y,-z] for x,y,z in original_vertices]
  triangles=[i for a,b,c in faces for i in (a,c,b)]
  packed=fields["m_CompressedMesh"]
  uv_data=packed.get("m_UV")
  if not uv_data or not int(uv_data.get("m_NumItems",0)):
   raise SystemExit("STOP original mesh lacks packed UVs; cannot bind source texture faithfully")
  flat_uv=unpack(uv_data,True)
  if len(flat_uv)!=len(vertices)*2:
   raise SystemExit(f"STOP packed UV components {len(flat_uv)} != vertex pairs {len(vertices)}")
  uvs=[[float(flat_uv[2*i]),float(flat_uv[2*i+1])] for i in range(len(vertices))]
 if not vertices or not uvs or len(uvs)!=len(vertices) or not triangles or any(i>=len(vertices) for i in triangles):
  raise SystemExit("STOP original exit-hole mesh has invalid vertices/UVs")
 if normals and len(normals)!=len(vertices):raise SystemExit("STOP inconsistent normals")

matrecords=parse_asset(matpath,matpath.relative_to(root).as_posix())["records"]
mat=next((rec["fields"] for rec in matrecords if rec["classID"]==21),None)
if mat is None:raise SystemExit("STOP missing material class 21")
texenv=mat.get("m_SavedProperties",{}).get("m_TexEnvs",{})
texture_guid=None
if isinstance(texenv,dict):
 for key in ("_MainTex","_BaseMap"):
  t=texenv.get(key)
  if isinstance(t,dict):texture_guid=t.get("m_Texture",{}).get("guid")
  if texture_guid:break
texturepath=index.get(texture_guid)
out={
 "source":"Recovered original Wood exit hole.prefab and Circle.asset (no replacement geometry)",
 "source_sha256":{"prefab":hashlib.sha256(path.read_bytes()).hexdigest(),"mesh":hashlib.sha256(meshpath.read_bytes()).hexdigest(),"material":hashlib.sha256(matpath.read_bytes()).hexdigest()},
 "file_guids":{"prefab":"c0bb8365c7bd54b4597570f085f5a153","mesh":mg,"material":matg,"texture":texture_guid},
 "source_paths":{"prefab":rel,"mesh":meshpath.relative_to(root).as_posix(),"material":matpath.relative_to(root).as_posix(),"texture":texturepath.relative_to(root).as_posix() if texturepath else None},
 "prefab_local_transform":{"position":tr.get("m_LocalPosition"),"rotation":tr.get("m_LocalRotation"),"scale":tr.get("m_LocalScale")},
 "mesh":{"positions":vertices,"normals":normals,"uv":uvs,"indices":triangles,"source_format":source_format,"normals_from_source":bool(normals)},
 "counts":{"vertices":len(vertices),"triangles":len(triangles)//3},
 "note":"Geometry source preserved; no particle simulation, runtime renderer, or material/shader parity is claimed."
}
args.out.parent.mkdir(parents=True,exist_ok=True)
args.out.write_text(json.dumps(out,separators=(",",":"))+"\n")
print(json.dumps({k:out[k] for k in ("file_guids","source_paths","counts")},indent=2))
