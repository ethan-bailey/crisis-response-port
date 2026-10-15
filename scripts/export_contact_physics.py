#!/usr/bin/env python3
"""Extract original static box contacts and Operator settings; no invented floor."""
import json, hashlib, sys
from pathlib import Path
import numpy as np
from scipy.spatial.transform import Rotation
from extract_runtime_scene import parse_asset

def main():
    root=Path(sys.argv[1]); out=Path(sys.argv[2]); sources={}
    def read(rel):
        a=parse_asset(root/rel,rel); sources[rel]=a['sha256']; return a['records']
    paths={}
    for meta in (root/'Assets').rglob('*.physicMaterial.meta'):
        import re
        paths[re.search(r'^guid: (\w+)',meta.read_text(),re.M)[1]]=str(meta.with_suffix('').relative_to(root))
    materials={}
    def material(ref):
        guid=ref.get('guid'); path=paths.get(guid)
        if not guid and str(ref.get('fileID'))=='0':
            # Unity's built-in surface defaults when DefaultMaterial is unset.
            key='unity-default'
            materials[key]={'m_Name':'Unity default','staticFriction':0.6,'dynamicFriction':0.6,'bounciness':0,'frictionCombine':0,'bounceCombine':0,
                'reference':'https://docs.unity3d.com/Manual/class-PhysicMaterial.html'}
            return key
        if not path: raise ValueError(f'Unresolved original material: {ref}')
        if guid not in materials: materials[guid]=read(path)[0]['fields']
        return guid
    r=read('Assets/Scenes/Mission 01.unity'); byid={x['fileID']:x for x in r}
    gos={x['fileID']:x['fields'] for x in r if x['classID']==1}
    ts={x['fileID']:x['fields'] for x in r if x['classID']==4}
    tg={str(t['m_GameObject']['fileID']):tid for tid,t in ts.items()}
    bodies={str(x['fields']['m_GameObject']['fileID']):x['fields'] for x in r if x['classID']==54}
    def vec(v):return np.array([v[k] for k in 'xyz'],float)
    cache={}
    def world(tid):
        if tid not in cache:
            t=ts[tid]; q=t['m_LocalRotation']; m=np.eye(4)
            m[:3,:3]=Rotation.from_quat([q[k] for k in 'xyzw']).as_matrix()@np.diag(vec(t['m_LocalScale']));m[:3,3]=vec(t['m_LocalPosition'])
            parent=str(t['m_Father']['fileID']); cache[tid]=world(parent)@m if parent in ts else m
        return cache[tid]
    def eligible(go):
        while go in tg:
            if not gos[go]['m_IsActive']:return False
            if go in bodies and not bodies[go]['m_IsKinematic']:return False
            parent=str(ts[tg[go]]['m_Father']['fileID'])
            go=str(ts[parent]['m_GameObject']['fileID']) if parent in ts else '0'
        return True
    settings=read('ProjectSettings/DynamicsManager.asset')[0]['fields']
    time=read('ProjectSettings/TimeManager.asset')[0]['fields']
    prefab=read('Assets/Resources/characters/Operator.prefab')
    body=next(x['fields'] for x in prefab if x['classID']==54 and x['fields']['m_Mass']==90)
    go=str(body['m_GameObject']['fileID']);cap=next(x['fields'] for x in prefab if x['classID']==136 and str(x['fields']['m_GameObject']['fileID'])==go)
    movement=next(x['fields'] for x in prefab if x['classID']==114 and 'walkSpeed' in x['fields'])
    cg=next(x['fields'] for x in prefab if x['classID']==1 and x['fileID']==go)
    characterMaterial=material(cap['m_Material']); layer=cg['m_Layer']
    # Unity serializes the 32 collision rows as little-endian uint32 values.
    matrix=bytes.fromhex(settings['m_LayerCollisionMatrix']);mask=int.from_bytes(matrix[layer*4:layer*4+4],'little')
    boxes=[]; exclusions={}
    for rec in r:
        if rec['classID']!=65:continue
        f=rec['fields'];go=str(f['m_GameObject']['fileID'])
        reason=None
        if not f['m_Enabled'] or f['m_IsTrigger'] or not eligible(go):reason='inactive/trigger/dynamic'
        elif not (mask & (1<<gos[go]['m_Layer'])):reason='original layer matrix'
        if reason:exclusions[reason]=exclusions.get(reason,0)+1;continue
        m=world(tg[go]);basis=m[:3,:3];scale=np.linalg.norm(basis,axis=0);rot=basis/scale
        if not np.allclose(rot.T@rot,np.eye(3),atol=1e-5):raise ValueError('Unsupported sheared collider '+rec['fileID'])
        if np.linalg.det(rot)<0:rot[:,0]*=-1
        center=(m@np.r_[vec(f['m_Center']),1])[:3]
        # Unity left-handed → rendering/PhysX right-handed: reflect Z.
        reflect=np.diag([1,1,-1]);rot=reflect@rot@reflect;center=reflect@center
        boxes.append({'fileID':rec['fileID'],'name':gos[go]['m_Name'],'layer':gos[go]['m_Layer'],
            'position':center.tolist(),'rotation':Rotation.from_matrix(rot).as_quat().tolist(),
            'halfExtents':(abs(vec(f['m_Size']))*scale/2).tolist(),'material':material(f['m_Material'])})
    data={'format':'crisis-original-box-contacts-v1','sources':sources,'materials':materials,
        'settings':settings,'fixedDeltaTime':time['Fixed Timestep'],'character':{'body':body,'capsule':cap,'material':characterMaterial,'layer':layer,'collisionMask':mask,'movement':{k:movement[k] for k in ['walkSpeed','sprintSpeed','colliderVerticalOffset']}},
        'boxes':boxes,'excludedBoxes':exclusions,'limitations':['Mesh colliders, dynamic props, joints and original Unity/PhysX version parity remain unimplemented.']}
    out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(data,separators=(',',':')))
    print(json.dumps({'boxes':len(boxes),'excluded':exclusions,'materials':len(materials),'bytes':out.stat().st_size}))
if __name__=='__main__':main()
