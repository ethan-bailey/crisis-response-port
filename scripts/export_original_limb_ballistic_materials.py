#!/usr/bin/env python3
"""Recover exact original limb ballistic material assets, source armor and collider identity.
Never assign surrogate flesh properties or synthetic hitboxes.
"""
import hashlib,json,re,sys,tempfile,zipfile
from pathlib import Path
from extract_runtime_scene import parse_asset

ARCHIVE_HASH='cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d'
ROOT='export/ExportedProject/'
def ref(v):
 if not isinstance(v,dict) or 'fileID' not in v:raise ValueError('Expected exact original Unity object reference')
 return str(v['fileID'])
def recover(archive,limb_json):
 if hashlib.sha256(archive.read_bytes()).hexdigest()!=ARCHIVE_HASH:raise ValueError('Unverified private recovery artifact')
 data=json.loads(limb_json.read_text())
 if data['archiveSha256']!=ARCHIVE_HASH:raise ValueError('Mismatched limb graph archive')
 guids={limb['sourceFields']['Limb']['properties']['guid']
   for actor in data['characters'] for limb in actor['limbs']}
 if len(guids)!=8:raise ValueError('Expected eight original distinct limb material property assets')
 with zipfile.ZipFile(archive) as z,tempfile.TemporaryDirectory() as tmp:
  matches={}
  for path in z.namelist():
   if not path.startswith(ROOT+'Assets/') or not path.endswith('.meta') or not path.endswith(('.asset.meta','.prefab.meta')):continue
   meta=z.read(path)
   m=re.search(rb'(?m)^guid:\s*([0-9a-f]{32})\s*$',meta)
   if m and m.group(1).decode() in guids:
    guid=m.group(1).decode()
    if guid in matches:raise ValueError('Duplicate GUID source '+guid)
    matches[guid]=(path[:-5],hashlib.sha256(meta).hexdigest())
  if set(matches)!=guids:raise ValueError('Missing original ballistic material properties: '+str(sorted(guids-set(matches))))
  props={}
  for guid,(path,meta_hash) in matches.items():
   raw=z.read(path);file=Path(tmp)/(guid+'.asset');file.write_bytes(raw)
   parsed=parse_asset(file,path)
   rows=[r for r in parsed['records'] if r['classID']==114 and
         'surfaceThickness' in r['fields'] and 'surfaceDensity' in r['fields']]
   if len(rows)!=1:raise ValueError('Original BallisticMaterialProperties record is not unique: '+path)
   record=rows[0];field=record['fields']
   for key in ('surfaceThickness','surfaceDensity','maximumDeflection','density','densityRandomness'):
    if key not in field:raise ValueError('Unresolved source ballistic property '+key)
   props[guid]={'assetPath':path[len(ROOT):],'sourceSha256':hashlib.sha256(raw).hexdigest(),
     'metaSha256':meta_hash,'fileID':record['fileID'],
     'fields':{key:field[key] for key in ('surfaceThickness','surfaceDensity','maximumDeflection','density','densityRandomness',
                                       'entryEffect','exitEffect','exitHole')}}
  # Resolve true source armor script GUID and authored MonoBehaviour fields.
  armor_meta=[path for path in z.namelist()
     if path.startswith(ROOT+'Assets/') and path.endswith('/LimbArmor.cs.meta')]
  if len(armor_meta)!=1:raise ValueError('Cannot establish original LimbArmor source GUID')
  meta=z.read(armor_meta[0]);armor_guid=re.search(rb'(?m)^guid:\s*([0-9a-f]{32})\s*$',meta)
  if armor_guid is None:raise ValueError('Invalid source LimbArmor GUID')
  armorguid=armor_guid.group(1).decode()
  armor={}
  for character in ('Operator','Occupant'):
   relative=ROOT+'Assets/Resources/characters/'+character+'.prefab'
   file=Path(tmp)/(character+'.prefab');file.write_bytes(z.read(relative))
   rows=parse_asset(file,relative)['records']
   armor_rows=[r for r in rows if r['classID']==114 and
      r['fields'].get('m_Script',{}).get('guid')==armorguid]
   actual={r['fields']['m_GameObject']['fileID']:{'fileID':r['fileID'],'fields':r['fields']}
           for r in armor_rows}
   original={l['gameObject'] for c in data['characters'] if c['name']==character for l in c['limbs']}
   if not set(actual).issubset(original):raise ValueError('Armor attached outside authentic limb graph')
   armor[character]=actual
 result={'format':'crisis-original-limb-ballistic-materials-v1',
         'archiveSha256':ARCHIVE_HASH,
         'limbGraphSha256':hashlib.sha256(limb_json.read_bytes()).hexdigest(),
         'materials':dict(sorted(props.items())),
         'armorScriptGuid':armorguid,'armor':armor}
 return result
if __name__=='__main__':
 if len(sys.argv)!=4:raise SystemExit('usage: SCRIPT ARCHIVE LIMBS.json OUTPUT.json')
 obj=recover(Path(sys.argv[1]),Path(sys.argv[2]))
 path=Path(sys.argv[3]);path.parent.mkdir(parents=True,exist_ok=True)
 path.write_text(json.dumps(obj,sort_keys=True,separators=(',',':'))+'\n')
 print(json.dumps({'materials':len(obj['materials']),'armors':{k:len(v) for k,v in obj['armor'].items()},
        'sha256':hashlib.sha256(path.read_bytes()).hexdigest()},sort_keys=True))
