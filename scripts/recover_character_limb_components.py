#!/usr/bin/env python3
"""Recover the genuine 19-limb character, physiology, muscle & vital component graph.
Only AssetRipper Unity YAML from the pinned private archive is used.
"""
import argparse, hashlib, json, re, tempfile, zipfile
from pathlib import Path
from extract_runtime_scene import parse_asset

PIN='cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d'
ROOT='export/ExportedProject/'
SOURCE_NAMES=('Limb','LimbPhysiology','LimbMuscle','VitalFunctions')
EXPECTED_NAMES=('Pelvis','Abdomen','Torso','Neck','Head',
 'Right upper arm','Right lower arm','Right hand',
 'Left upper arm','Left lower arm','Left hand',
 'Right upper leg','Right lower leg','Right foot','Right toes',
 'Left upper leg','Left lower leg','Left foot','Left toes')

def generate(archive:Path):
    sha=hashlib.sha256(archive.read_bytes()).hexdigest()
    if sha!=PIN:raise ValueError('Unpinned original game source archive')
    with zipfile.ZipFile(archive) as z, tempfile.TemporaryDirectory() as temp:
        paths=z.namelist()
        script_guids={}
        script_hashes={}
        for name in SOURCE_NAMES:
            matches=[p for p in paths if p.startswith(ROOT) and p.endswith('/'+name+'.cs.meta')]
            if len(matches)!=1:raise ValueError('Cannot establish original component GUID: '+name+': '+str(matches))
            meta=z.read(matches[0]);guid=re.search(rb'(?m)^guid: ([0-9a-f]{32})$',meta)
            if not guid:raise ValueError('Missing GUID in '+matches[0])
            script_guids[name]=guid.group(1).decode()
            script_path=matches[0][:-5]
            script_hashes[name]=hashlib.sha256(z.read(script_path)).hexdigest()
        outputs=[]
        for character in ('Operator','Occupant'):
            path=ROOT+'Assets/Resources/characters/'+character+'.prefab'
            data=z.read(path);local=Path(temp)/(character+'.prefab')
            local.write_bytes(data)
            records=parse_asset(local,path)['records']
            by_id={r['fileID']:r for r in records}
            game_objects={r['fileID']:r for r in records if r['classID']==1}
            transform_by_go={r['fields']['m_GameObject']['fileID']:r['fileID']
                             for r in records if r['classID']==4}
            scripts={}
            for r in records:
                fields=r['fields']
                if r['classID']!=114 or not isinstance(fields.get('m_Script'),dict):continue
                guid=fields['m_Script'].get('guid')
                if guid in script_guids.values():
                    go=fields.get('m_GameObject',{}).get('fileID')
                    scripts.setdefault(go,{}).setdefault(guid,[]).append(r)
            entries=[]
            for go,items in scripts.items():
                limb_records=items.get(script_guids['Limb'],[])
                if not limb_records:continue
                if len(limb_records)!=1:raise ValueError('Duplicate original Limb: '+go)
                name=game_objects[go]['fields']['m_Name']
                if name not in EXPECTED_NAMES:raise ValueError('Unexpected original limb '+name)
                entry={'name':name,'gameObject':go,'transform':transform_by_go[go],
                       'componentIds':{},'sourceFields':{}}
                for kind,guid in script_guids.items():
                    candidates=items.get(guid,[])
                    if len(candidates)>1:raise ValueError('Duplicate original '+kind+' on '+name)
                    if candidates:
                        entry['componentIds'][kind]=candidates[0]['fileID']
                        entry['sourceFields'][kind]=candidates[0]['fields']
                for required in ('Limb',):
                    if required not in entry['componentIds']:
                        raise ValueError('Original '+required+' absent on '+name)
                entries.append(entry)
            if sorted(e['name'] for e in entries)!=sorted(EXPECTED_NAMES):
                raise ValueError('Full source limb inventory mismatch: '+character)
            outputs.append({'name':character,'prefabSha256':hashlib.sha256(data).hexdigest(),
                 'limbs':sorted(entries,key=lambda x:EXPECTED_NAMES.index(x['name']))})
    return {'format':'crisis-original-character-limb-components-v1',
            'archiveSha256':sha,'scriptGuids':script_guids,
            'scriptSha256':script_hashes,'characters':outputs}

if __name__=='__main__':
    p=argparse.ArgumentParser()
    p.add_argument('archive',type=Path);p.add_argument('--output',type=Path,required=True)
    a=p.parse_args();out=generate(a.archive);a.output.parent.mkdir(parents=True,exist_ok=True)
    a.output.write_text(json.dumps(out,sort_keys=True,separators=(',',':'))+'\n')
    print(json.dumps({'limbs':[len(c['limbs']) for c in out['characters']],
       'physiologyComponents':[sum('LimbPhysiology' in v['componentIds'] for v in c['limbs']) for c in out['characters']],
       'sha256':hashlib.sha256(a.output.read_bytes()).hexdigest(),'guidTypes':out['scriptGuids']},sort_keys=True))
