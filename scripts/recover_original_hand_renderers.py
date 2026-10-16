#!/usr/bin/env python3
"""Recover exact authored Operator hand quads/textures and existing-host bindings."""
import argparse
import gzip
import hashlib
import json
from pathlib import Path
import re
from extract_runtime_scene import parse_asset
from source_arm_runtime import arm_bindings, build_arm_transition, provenance


def recover(root, repo):
    asset='Assets/Resources/characters/Operator.prefab'
    parsed=parse_asset(root/asset,asset);records=parsed['records']
    bindings=arm_bindings(records)
    by_id={r['fileID']:r for r in records};index={}
    for meta in (root/'Assets').rglob('*.meta'):
        match=re.search(r'^guid: ([a-f0-9]{32})$',meta.read_text(errors='strict'),re.M)
        if match:index[match[1]]=meta.with_suffix('')
    out=repo/'game/assets/operator';out.mkdir(parents=True,exist_ok=True)
    sources={asset:parsed['sha256']};renderers=[];textures={}
    sha=lambda raw:hashlib.sha256(raw).hexdigest()
    def read(path):
        rel=str(path.relative_to(root));sources[rel]=sha(path.read_bytes())
        return parse_asset(path,rel)['records'][0]['fields']
    gos=sorted({variant['gameObject'] for arm in bindings.values() for variant in arm['variants'].values()})
    for go in gos:
        mesh=next(r for r in records if r['classID']==33 and r['fields']['m_GameObject']['fileID']==go)
        if mesh['fields']['m_Mesh']!={'fileID':'10210','guid':'0000000000000000e000000000000000','type':0}:
            raise ValueError('Expected original Unity built-in Quad, no replacement geometry')
        renderer=next(r for r in records if r['classID']==23 and r['fields']['m_GameObject']['fileID']==go)
        materials=renderer['fields']['m_Materials']
        if len(materials)!=1:raise ValueError('Expected one original hand material')
        material_path=index[materials[0]['guid']];mat=read(material_path)
        properties=mat['m_SavedProperties'];main=properties['m_TexEnvs']['_MainTex']
        tex_guid=main['m_Texture']['guid'];source=index[tex_guid]
        if not source.is_file():source=source.with_suffix('.png')
        raw=source.read_bytes();rel=str(source.relative_to(root));sources[rel]=sha(raw)
        if not raw.startswith(b'\x89PNG\r\n\x1a\n'):raise ValueError('Expected original PNG bytes')
        target='hands/'+tex_guid+'.png';(out/'hands').mkdir(exist_ok=True)
        (out/target).write_bytes(raw);textures[tex_guid]=dict(path=rel,sha256=sha(raw),url=target)
        shader=index[mat['m_Shader']['guid']];sources[str(shader.relative_to(root))]=sha(shader.read_bytes())
        renderers.append(dict(gameObjectFileID=go,rendererFileID=renderer['fileID'],name=by_id[go]['fields']['m_Name'],
            mesh=mesh['fields']['m_Mesh'],renderer=renderer['fields'],material=mat,
            materialPath=str(material_path.relative_to(root)),texture=target,textureSha256=sha(raw),
            sourceShaderIsDummy='DummyShaderTextExporter' in shader.read_text()))
    payload=dict(format='crisis-original-hand-renderers-v1',sources=sources,renderers=renderers,textures=textures,
                 limitations=['Original texture/quad/activation recovered; exported shader is a dummy, native shading parity unverified'])
    (out/'original-hand-renderers.json').write_text(json.dumps(payload,indent=2,sort_keys=True)+'\n')
    # Existing weapon package state is retained; only source arm references added.
    package=repo/'game/assets/weapon-runtime.json.gz'
    data=json.loads(gzip.decompress(package.read_bytes()));data['armRigBindings']=bindings
    package.write_bytes(gzip.compress(json.dumps(data,separators=(',',':')).encode(),mtime=0))
    source=(root/'Assets/Scripts/Assembly-CSharp/ArmIKSolver.cs').read_text();output=build_arm_transition(source)
    (repo/'runtime/Original/Weapons/ArmTransition.cs').write_text(output)
    (repo/'source-reference/Original/ArmPhysics/ik-runtime-provenance.json').write_text(json.dumps(provenance(source,output),indent=2,sort_keys=True)+'\n')
    return dict(handQuads=len(renderers),originalTextures=len(textures),sourcePrefabSha256=parsed['sha256'])


if __name__=='__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('project',type=Path);parser.add_argument('--repo',type=Path,default=Path('.'))
    args=parser.parse_args();print(json.dumps(recover(args.project,args.repo),sort_keys=True))
