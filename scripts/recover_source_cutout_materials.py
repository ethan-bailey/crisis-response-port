#!/usr/bin/env python3
"""Recover original depth/alpha material settings for operator and M4 magazine."""
import argparse,hashlib,json,re
from pathlib import Path
from extract_runtime_scene import parse_asset

def recover(root,repo):
    sources={};index={}
    def read(path):
        rel=str(path.relative_to(root));sources[rel]=hashlib.sha256(path.read_bytes()).hexdigest()
        return parse_asset(path,rel)['records']
    for meta in (root/'Assets/Material').glob('*.meta'):
        match=re.search(r'^guid: (\w+)',meta.read_text(),re.M)
        if match:index[match[1]]=meta.with_suffix('')
    records=read(root/'Assets/Resources/characters/Operator.prefab')
    ids={x['gameObjectFileID'] for x in json.loads((repo/'game/assets/operator/operator.json').read_text())['limbs']}
    materials={}
    def material(path):
        f=read(path)[0]['fields'];p=f['m_SavedProperties'];v=p['m_Floats']
        assert v['_SrcBlend']==1 and v['_DstBlend']==0,'Blended material requires explicit support'
        return dict(path=str(path.relative_to(root)),cutoff=v['_Cutoff'],depthWrite=bool(v['_ZWrite']),color=p['m_Colors']['_Color'],nativeShaderParity=False)
    for r in records:
        if r['classID']==23 and r['fields']['m_GameObject']['fileID'] in ids:
            refs=r['fields']['m_Materials'];assert len(refs)==1
            materials[r['fields']['m_GameObject']['fileID']]=material(index[refs[0]['guid']])
    assert set(materials)==ids
    data=dict(format='original-source-cutout-v1',sources=sources,operator=materials,magazine=material(root/'Assets/Material/magazine.mat'))
    (repo/'game/assets/source-cutout-materials.json').write_text(json.dumps(data,sort_keys=True,indent=2)+'\n')
    return dict(operatorMaterials=len(materials),magazineDepthWrite=data['magazine']['depthWrite'])

if __name__=='__main__':
    p=argparse.ArgumentParser();p.add_argument('project',type=Path);p.add_argument('--repo',type=Path,default=Path('.'));a=p.parse_args();print(json.dumps(recover(a.project,a.repo)))
