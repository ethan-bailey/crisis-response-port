#!/usr/bin/env python3
"""Recover original material/texture/mesh data and source interaction methods."""
import sys,re,json,gzip,hashlib,base64,subprocess,tempfile
from pathlib import Path
from PIL import Image
import numpy as np
from scipy.spatial import ConvexHull,QhullError
from extract_runtime_scene import parse_asset
from extract_runtime_sources import block
from unity_packed_mesh import compressed_mesh
root=Path(sys.argv[1]);out=Path(sys.argv[2]);dest=Path(sys.argv[3]);dest.mkdir(parents=True,exist_ok=True)
src=root/'Assets/Scripts/Assembly-CSharp';sources={};methods={};index={};cache={}
for p in (root/'Assets').rglob('*.meta'):
 m=re.search(r'^guid: (\w+)',p.read_text(errors='ignore'),re.M)
 if m:index[m[1]]=str(p.with_suffix('').relative_to(root))
def records(path):
 if path not in cache:
  a=parse_asset(root/path,path);sources[path]=a['sha256'];cache[path]=a.get('records',[])
 return cache[path]
def code(name):
 p=src/name;s=p.read_text();sources[str(p.relative_to(root))]=hashlib.sha256(p.read_bytes()).hexdigest();return s
def guid(name):return re.search(r'^guid: (\w+)',(src/(name+'.cs.meta')).read_text(),re.M)[1]
def clean(s):
 s=re.sub(r'^using .*?;\n','',s,flags=re.M);s=re.sub(r'^\s*\[(?:RequireComponent|Tooltip|Header|Range)\(.*?\)\]\n','\n',s,flags=re.M);return s
prefix='using System;\nusing System.Collections.Generic;\nusing UnityEngine;\nusing Object=UnityEngine.Object;\nusing Random=UnityEngine.Random;\n'
for name in ['BallisticMaterial.cs','BallisticWindow.cs','BallisticMaterialProperties.cs','DelayedBallisticExit.cs','BallisticResult.cs','StaticUtilities.cs','BallisticHitMeshEffect.cs']:
 s=code(name)
 if name=='BallisticHitMeshEffect.cs':s=s.replace('public class BallisticHitMeshEffect','public partial class BallisticHitMeshEffect').replace('base.transform.SetParent(parent, true);','ReparentBeforeScaleReset(parent); // Engine adapter: next source statement resets localScale.')
 (dest/name).write_text(prefix+clean(s))
# Preserve the full original non-limb hit and fragment arithmetic; native effects
# creation is bridged explicitly, without replacing material decisions.
p=code('Projectile.cs');sigs=['private BallisticResult HandleGenericHit(RaycastHit hit)','public void BeginNewInteraction()','public void NewTracePosition()','public void RestartAtPosition(Vector3 worldPosition)','public void Deflect(float maximum)','public void AssignNewSpinAndTumble()','public void OnFragmentation()','public void InitializeProjectileFragment(Projectile projectile, int uniqueId','public void SpawnSpecificFragments(Vector3 worldOrigin','public void SpawnRandomFragments(Vector3 worldOrigin']
chunks=[]
for sig in sigs:
 b=block(p,sig);methods[sig]=hashlib.sha256(b.encode()).hexdigest();chunks.append(b)
(dest/'ProjectileMaterial.Methods.cs').write_text(prefix+'using UnityEngine.Events;\npublic partial class Projectile {\n'+'\n'.join(chunks)+'\n}\n')
pool=code('PerformanceManager.cs');pool_chunks=[]
for sig in ['public BallisticHitEffect CreateEnvironmentalHitEffect(', 'public BallisticHitMeshEffect SpawnEnvironmentalHitMesh(']:
 b=block(pool,sig);methods[sig]=hashlib.sha256(b.encode()).hexdigest();pool_chunks.append(b)
(dest/'EffectPool.Methods.cs').write_text(prefix+'public partial class PerformanceManager {\n'+'\n'.join(pool_chunks)+'\n}\n')
a=code('AmmunitionBase.cs');(dest/'AmmunitionFragments.cs').write_text(prefix+'public partial class AmmunitionBase {\n'+block(a,'public enum ExplosionMode')+'\n'+'[Serializable]\n'+block(a,'public class FragmentationSpecification')+'\npublic List<FragmentationSpecification> specificFragmentation=new();\npublic ExplosionMode explosionMode;\n}\n')
scene='Assets/Scenes/Mission 01.unity';rs=records(scene);go={r['fileID']:r['fields'] for r in rs if r['classID']==1};byid={r['fileID']:r for r in rs}
mg, wg=guid('BallisticMaterial'),guid('BallisticWindow');mats=[];props={};textures={};effects={};meshes={};unsupported=[];meshColliders=[]
assetdir=out.parent/'ballistics';assetdir.mkdir(parents=True,exist_ok=True)
def texture(g):
 if g not in textures:
  path=index[g];p=root/path;im=Image.open(p).convert('RGBA');raw=im.transpose(Image.Transpose.FLIP_TOP_BOTTOM).tobytes();sources[path]=hashlib.sha256(p.read_bytes()).hexdigest()
  import shutil
  target='texture-'+g+'.png';shutil.copy2(p,assetdir/target)
  textures[g]={'width':im.width,'height':im.height,'rgba':base64.b64encode(raw).decode(),'url':'./assets/ballistics/'+target,'source':path}
def effect(g):
 if g in effects:return
 ers=records(index[g]);effects[g]={'source':index[g],'records':ers,'audio':[]}
 for rec in ers:
  for ref in rec['fields'].get('audioClips',[]):
   ag=ref.get('guid')
   if ag in index:
    import shutil
    p=root/index[ag];name='audio-'+ag+p.suffix;shutil.copy2(p,assetdir/name);sources[index[ag]]=hashlib.sha256(p.read_bytes()).hexdigest();effects[g]['audio'].append('./assets/ballistics/'+name)
 for rec in ers:
  for ref in rec['fields'].get('m_Materials',[]):
   gmat=ref.get('guid')
   if gmat in index:
    for mat in records(index[gmat]):
     def visit(value):
      if isinstance(value,dict):
       tg=value.get('m_Texture',{}).get('guid')
       if tg in index and Path(index[tg]).suffix.lower()=='.png':texture(tg)
       for v in value.values():visit(v)
      elif isinstance(value,list):
       for v in value:visit(v)
     visit(mat['fields'])
for rec in rs:
 f=rec['fields'];g=f.get('m_Script',{}).get('guid')
 if g not in [mg,wg]:continue
 mats.append({'id':rec['fileID'],'kind':'window' if g==wg else 'solid','fields':f})
 pg=f.get('properties',{}).get('guid')
 if pg and pg not in props:
  prs=records(index[pg]);prop=next(x['fields'] for x in prs if x['classID']==114 and 'surfaceDensity' in x['fields']);props[pg]={'source':index[pg],'fields':prop}
  for k in ['entryEffect','exitEffect','exitHole']:
   eg=prop[k].get('guid')
   if eg in index:effect(eg)
 if g==wg:
  for k in ['texture','holeTexture']:
   tg=f[k].get('guid')
   if tg in index:texture(tg)
for rec in rs:
 if rec['classID']!=64:continue
 f=rec['fields'];g=f['m_Mesh'].get('guid');entry={'id':rec['fileID'],'fields':f}
 if g not in index:unsupported.append({'id':rec['fileID'],'reason':'unresolved original built-in mesh','ref':f['m_Mesh']});continue
 if g not in meshes:
  path=index[g];records(path)
  with tempfile.TemporaryDirectory() as tmp:
   target=Path(tmp)/'mesh.obj';result=subprocess.run([sys.executable,str(Path(__file__).with_name('unity_mesh_to_obj.py')),str(root/path),str(target)],capture_output=True)
   vertices=[];triangles=[]
   if result.returncode:
    try:
     fields=next(r['fields'] for r in records(path) if r['classID']==43)
     vertices,triangles=compressed_mesh(fields)
    except (ValueError,KeyError,StopIteration) as error:
     meshes[g]={'source':path,'unsupported':str(error)};continue
   else:
    for line in target.read_text().splitlines():
     if line.startswith('v '):x,y,z=map(float,line[2:].split());vertices.append([x,y,-z])
     elif line.startswith('f '):ids=[int(x.split('/')[0])-1 for x in line[2:].split()];triangles.append([ids[0],ids[2],ids[1]])
   try:hull=ConvexHull(np.unique(np.array(vertices),axis=0));planes=hull.equations.tolist()
   except QhullError:planes=None
   meshes[g]={'source':path,'vertices':vertices,'triangles':triangles,'convexPlanes':planes}
 if meshes[g].get('unsupported'):
  unsupported.append({'id':rec['fileID'],'reason':meshes[g]['unsupported'],'source':meshes[g]['source']});continue
 if f['m_Convex'] and meshes[g]['convexPlanes'] is None:unsupported.append({'id':rec['fileID'],'reason':'zero-volume convex source mesh'});continue
 entry['mesh']=g;meshColliders.append(entry)
# Settings and provenance retain original values, not a replacement material table.
settings=code('SerializablePerformanceSettings.cs');defaults={n:int(re.search(r'public int '+n+r' = (-?\d+)',settings)[1]) for n in ['particlesLifetime','maxEnvironmentHitEffects','maxEnvironmentHitMeshes']}
code('BallisticHitEffect.cs');code('SingleEmissionEffect.cs');code('PerformanceManager.cs');code('MarksmanMode.cs')
def strip(v):
 if isinstance(v,dict):return {k:strip(x) for k,x in v.items() if k!='rawYaml'}
 if isinstance(v,list):return [strip(x) for x in v]
 return v
layers=records('ProjectSettings/TagManager.asset')[0]['fields']['layers'];mask=sum(1<<layers.index(n) for n in ['Limbs','Obstacle geometry','Wall geometry','Exterior geometry','Doors','Physical props','Windows'])
payload={'ballisticMask':mask,'limbMask':1<<layers.index('Limbs'),'scene':scene,'materials':mats,'properties':props,'textures':textures,'effects':effects,'meshes':meshes,'meshColliders':meshColliders,'unsupportedColliders':unsupported,'settings':defaults,'sourceHashes':sources,'methodHashes':methods,'scope':'Original material/fragment/window algorithms. Native particle, dynamic-body and convex cooking parity require separate verification; effect requests are explicit transports.'}
out.write_bytes(gzip.compress(json.dumps(strip(payload),separators=(',',':')).encode(),mtime=0));print({'materials':len(mats),'properties':len(props),'textures':len(textures),'meshes':len(meshes),'meshColliders':len(meshColliders),'unsupported':unsupported,'bytes':out.stat().st_size})
