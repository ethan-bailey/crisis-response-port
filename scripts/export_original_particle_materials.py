#!/usr/bin/env python3
"""Recover source-exact original impact-particle Unity material definitions.

Verifies original material/texture SHA against the private AssetRipper export and
the already-staged original browser PNG copies. No guessed shader, blend or
particle simulation semantics are introduced.
"""
import argparse
import gzip
import hashlib
import json
from pathlib import Path
from extract_runtime_scene import parse_asset

p=argparse.ArgumentParser()
p.add_argument('project',type=Path)
p.add_argument('--audit',type=Path,required=True)
p.add_argument('--particle-manifest',type=Path,required=True)
p.add_argument('--browser-assets',type=Path,default=Path('game/assets/ballistics'))
p.add_argument('--out',type=Path,required=True)
args=p.parse_args()
root=args.project
audit=json.loads(args.audit.read_text())
assets=audit['assets']
with gzip.open(args.particle_manifest,'rt') as stream:
    particle=json.load(stream)
if len(assets['materials'])!=8 or len(assets['textures'])!=4 or len(assets['meshes'])!=3:
    raise SystemExit('STOP unexpected source native particle material/texture/mesh coverage')
if len(particle['effects'])!=14:
    raise SystemExit('STOP original particle effect manifest changed')
def sha_bytes(raw):
    return hashlib.sha256(raw).hexdigest()
def verify_source(source):
    rel=source['path'];path=root/rel
    if not path.is_file() or sha_bytes(path.read_bytes())!=source['sha256']:
        raise SystemExit('STOP source original asset SHA changed: '+rel)
    return path

original_materials={}
original_textures={}
for guid,source in sorted(assets['textures'].items()):
    if source.get('unresolvedEngineTexture'):
        raise SystemExit('STOP original particle material texture is unresolved '+guid)
    source_path=verify_source(source)
    browser=args.browser_assets/('texture-'+guid+'.png')
    if not browser.is_file():
        raise SystemExit('STOP missing original already-copied browser particle texture: '+str(browser))
    if sha_bytes(browser.read_bytes())!=source['sha256']:
        raise SystemExit('STOP browser copy of original native texture differs: '+guid)
    original_textures[guid]={'sourcePath':source['path'],
        'originalSha256':source['sha256'],
        'url':'./assets/ballistics/texture-'+guid+'.png'}
for guid,source in sorted(assets['materials'].items()):
    path=verify_source(source)
    records=parse_asset(path,source['path'])['records']
    records=[r['fields'] for r in records if r['classID']==21]
    if len(records)!=1:
        raise SystemExit('STOP original source material record ambiguous '+guid)
    fields=records[0]
    shader=fields.get('m_Shader',{}).get('guid')
    if shader!=source['shaderGuid']:
        raise SystemExit('STOP original shader reference differs from source audit '+guid)
    saved=fields.get('m_SavedProperties')
    if not isinstance(saved,dict):
        raise SystemExit('STOP original Unity material properties absent '+guid)
    all_source_refs={}
    for prop,ref in (saved.get('m_TexEnvs') or {}).items():
        if not isinstance(ref,dict):
            continue
        tex=ref.get('m_Texture',{}).get('guid')
        if tex:
            if tex not in original_textures:
                raise SystemExit('STOP original particle material texture not source-verified '+tex)
            all_source_refs[prop]=tex
    if all_source_refs!=source['textureRefs']:
        raise SystemExit('STOP native texture refs differ from audited source '+guid)
    original_materials[guid]={
        'sourcePath':source['path'],
        'originalSha256':source['sha256'],
        'shaderGuid':shader,
        'shaderSourceAvailable':not assets['shaders'][shader].get('unresolvedEngineShader',False),
        'originalRenderQueue':fields.get('m_CustomRenderQueue'),
        'originalSavedProperties':saved,
        'originalAdditionalFields':{
            key:value for key,value in fields.items()
            if key not in ('m_SavedProperties','m_PrefabParentObject','m_PrefabInternal')
        }
    }
effects_by_material={}
for e in particle['effects']:
    for emission in e['emitters']:
        for ref in emission['renderer'].get('m_Materials',[]):
            guid=ref.get('guid')
            if not guid:
                continue
            if guid not in original_materials:
                raise SystemExit('STOP unknown original renderer material '+guid)
            effects_by_material.setdefault(guid,set()).add(e['guid'])
if set(effects_by_material)!=set(original_materials):
    raise SystemExit('STOP source materials are missing renderer bindings')
payload={
  'kind':'Original Unity 2017 impact ParticleSystemRenderer material source',
  'version':1,
  'provenanceSha256':sha_bytes(args.audit.read_bytes()),
  'particleManifestSha256':sha_bytes(args.particle_manifest.read_bytes()),
  'materials':original_materials,
  'textures':original_textures,
  'shaders':assets['shaders'],
  'effectsByMaterial':{k:sorted(v) for k,v in sorted(effects_by_material.items())},
  'scope':'Serialized original material source, not a shader or particle simulation implementation'
}
raw=json.dumps(payload,sort_keys=True,separators=(',',':')).encode()
compressed=gzip.compress(raw,mtime=0)
args.out.parent.mkdir(parents=True,exist_ok=True)
args.out.write_bytes(compressed)
print(json.dumps({
 'materialSources':len(original_materials),'textureSources':len(original_textures),
 'shaderSourceEntries':len(assets['shaders']),'rendererBoundMaterials':len(effects_by_material),
 'compressedBytes':len(compressed),'originalPayloadSha256':sha_bytes(raw),
 'compressedSha256':sha_bytes(compressed),
 'unresolvedBuiltinShaderGuids':[k for k,v in assets['shaders'].items() if v.get('unresolvedEngineShader')]
},sort_keys=True))
