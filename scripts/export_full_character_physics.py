#!/usr/bin/env python3
"""Export ALL source Operator/Occupant original rigidbody/hinge/capsule actors.
The previous arm-only runtime transport remains untouched and testable.
"""
import hashlib,json,sys
from pathlib import Path

PIN='cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d'
CLASSES={1,4,54,59,136}
EXPECTED={'Operator':(21,18,21),'Occupant':(20,18,20)}
def export(source:Path,output:Path):
 raw=source.read_bytes();v=json.loads(raw)
 if v.get('archiveSha256')!=PIN or v.get('format')!='crisis-original-arm-physics-v1':
  raise ValueError('Unverified original full character physical source')
 prefab=[]
 for src in v['prefabs']:
  name=src['name']
  original=[x for x in src['records'] if x['classID'] in CLASSES]
  counts=tuple(sum(r['classID']==k for r in original) for k in (54,59,136))
  if counts!=EXPECTED[name]:raise ValueError('Full original actor graph mismatch '+name+': '+str(counts))
  ids={r['fileID'] for r in original}
  go={r['fileID'] for r in original if r['classID']==1}
  transforms={r['fields']['m_GameObject']['fileID'] for r in original if r['classID']==4}
  for rec in original:
   if rec['classID'] in (54,59,136) and rec['fields']['m_GameObject']['fileID'] not in transforms:
    raise ValueError('Missing source actor Transform')
   if rec['classID']==59 and rec['fields']['m_ConnectedBody']['fileID'] not in ids:
    raise ValueError('Missing original hinge parent')
  prefab.append({'name':name,'records':original,'arms':src['arms']})
 result={'format':'crisis-original-arm-physics-runtime-v1','archiveSha256':PIN,
         'referenceSha256':hashlib.sha256(raw).hexdigest(),'physics':v['physics'],'prefabs':prefab}
 output.parent.mkdir(parents=True,exist_ok=True)
 output.write_text(json.dumps(result,sort_keys=True,separators=(',',':'))+'\n')
 print(json.dumps({'characters':[p['name'] for p in prefab],
 'actorCount':[sum(x['classID']==54 for x in p['records']) for p in prefab],
 'hingeCount':[sum(x['classID']==59 for x in p['records']) for p in prefab],
 'outputSha256':hashlib.sha256(output.read_bytes()).hexdigest()}))
if __name__=='__main__':
 if len(sys.argv)!=3:raise SystemExit('usage: export_full_character_physics.py SOURCE OUTPUT')
 export(Path(sys.argv[1]),Path(sys.argv[2]))
