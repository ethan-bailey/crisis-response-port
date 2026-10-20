#!/usr/bin/env python3
"""Preserve full Unity YAML records, parsed fields, source hashes and dependencies."""
import hashlib
import json
import base64
import re
import sys
from collections import deque
from pathlib import Path

HEADER = re.compile(r'^--- !u!(\d+) &(-?\d+)(?: stripped)?\s*$', re.M)
GUID = re.compile(r'guid: ([0-9a-f]{32})')
SCRIPT_GUIDS = {
    # Unity 2017.2.0f3 uGUI assembly identity is pinned from the recovered
    # UnityEngine.UI.dll. The fileIDs are AssetRipper's serialized TypeDef IDs.
    'f5f67c52d1564df4a8936ccd202a3bd8': {
        '-765806418': 'Image', '-405508275': 'HorizontalLayoutGroup',
        '-113659843': 'Slider', '708705254': 'Text',
        '1297475563': 'VerticalLayoutGroup', '1301386320': 'GraphicRaycaster',
        '1679637790': 'LayoutElement', '1980459831': 'CanvasScaler',
    }
}
SOURCE_ONLY_SUFFIXES = {'.cs'}
SCRIPT_ASSEMBLIES = {
    'f5f67c52d1564df4a8936ccd202a3bd8': {
        'path':'export/AuxiliaryFiles/GameAssemblies/UnityEngine.UI.dll',
        'sha256':'30cf854b3db333ba8a3ce47c2091ea11387509861654d8209c1879f7bcb27eeb',
        'length':241152,
    }
}
BUILTIN_OBJECTS = {
    '0000000000000000e000000000000000': {
        '10102':'Font', '10210':'Mesh',
    },
    '0000000000000000f000000000000000': {
        '45':'Shader', '46':'Shader', '208':'Shader',
    },
}


def execution_value(value, key=None):
    # Unity prefab references exceed JavaScript's exact integer range. Keep
    # identity values decimal strings through every JSON/JS/.NET boundary.
    if isinstance(value, dict): return {k:execution_value(v,k) for k,v in value.items()}
    if isinstance(value, list): return [execution_value(v) for v in value]
    if isinstance(value, int) and (key=='fileID' or abs(value)>2**53-1): return str(value)
    return value


def parse_asset(path, relative):
    import yaml
    data = path.read_bytes()
    digest=hashlib.sha256(data).hexdigest()
    try: text = data.decode('utf-8')
    except UnicodeDecodeError:
        return {'path':relative,'sha256':digest,'size':len(data),'binary':True,
            'dataBase64':base64.b64encode(data).decode('ascii'),'records':[],'externalGuids':[]}
    matches = list(HEADER.finditer(text))
    records = []
    for index, match in enumerate(matches):
        end = matches[index + 1].start() if index + 1 < len(matches) else len(text)
        raw = text[match.end():end]
        parsed = yaml.safe_load(raw)
        if not isinstance(parsed, dict) or len(parsed) != 1: raise ValueError(f'Bad record {relative}:{match.group(2)}')
        kind, fields = next(iter(parsed.items()))
        records.append({'fileID':match.group(2), 'classID':int(match.group(1)), 'className':kind,
            'fields':execution_value(fields), 'rawYaml':raw})
    return {'path':relative, 'sha256':digest,'size':len(data),'sourceText':text,
        'records':records, 'externalGuids':sorted(set(GUID.findall(text)))}


def runtime_asset_seeds(project):
    project = Path(project).resolve()
    seeds = ['Assets/Scenes/Marksman range.unity', 'Assets/Scenes/Mission 01.unity',
        'Assets/Resources/characters/Operator.prefab', 'Assets/Resources/characters/Occupant.prefab',
        'ProjectSettings/TimeManager.asset', 'ProjectSettings/TagManager.asset']
    # Resources.Load paths are runtime strings, not serialized GUID edges.
    seeds.extend(str(p.relative_to(project)) for p in sorted((project/'Assets/Resources/weapons/ammunition').glob('*.prefab')))
    return seeds


def main():
    project = Path(sys.argv[1]).resolve(); output = Path(sys.argv[2])
    index = {}; metadata={}
    for meta in (project/'Assets').rglob('*.meta'):
        m = re.search(r'^guid: ([0-9a-f]{32})$', meta.read_text(errors='strict'), re.M)
        if m:
            guid=m.group(1); target=str(meta.with_suffix('').relative_to(project))
            if guid in index and index[guid]!=target: raise ValueError(f'Duplicate Unity GUID {guid}: {index[guid]} and {target}')
            index[guid]=target
            metadata[guid]={'path':target,'metaSha256':hashlib.sha256(meta.read_bytes()).hexdigest()}
    seeds = runtime_asset_seeds(project)
    queue = deque(seeds); assets = {}; missing = []
    while queue:
        relative = queue.popleft()
        if relative in assets: continue
        asset = parse_asset(project/relative, relative); assets[relative] = asset
        for guid in asset.get('externalGuids', []):
            if guid in index:
                target = index[guid]
                # Follow every referenced non-source object, including binary media,
                # shaders, animation clips and import settings that are not YAML.
                if Path(target).suffix.lower() not in SOURCE_ONLY_SUFFIXES:
                    queue.append(target)
            elif guid in SCRIPT_GUIDS:
                # Known imported package script assembly: the GUID has no Assets
                # path in this export, so preserve its TypeDef fileID mapping.
                continue
            elif not guid.startswith('0000000000000000'):
                missing.append({'asset':relative,'guid':guid})
    references=[]
    for source,asset in assets.items():
        before=len(references)
        for record in asset.get('records',[]):
            def walk(value,trail):
                if isinstance(value,dict):
                    if 'fileID' in value and 'guid' in value:
                        references.append({'source':source,'sourceFileID':record['fileID'],
                            'fieldPath':trail,'guid':value['guid'],'fileID':str(value['fileID']),
                            'type':value.get('type')})
                    else:
                        for key,item in value.items(): walk(item,f'{trail}.{key}' if trail else key)
                elif isinstance(value,list):
                    for i,item in enumerate(value): walk(item,f'{trail}[{i}]')
            walk(record.get('fields',{}),'')
        represented={ref['guid'] for ref in references[before:]}
        # Some text formats declare dependencies outside Unity YAML object maps.
        # Keep those GUID-only edges visible with null fileIDs instead of dropping
        # them or fabricating an object identity.
        for guid in asset.get('externalGuids',[]):
            if not asset.get('records') and guid not in represented:
                references.append({'source':source,'sourceFileID':None,'fieldPath':'$text',
                    'guid':guid,'fileID':None,'type':None})
    for ref in references:
        builtin=BUILTIN_OBJECTS.get(ref['guid'])
        if builtin is not None:
            ref['objectType']=builtin.get(ref['fileID'])
            ref['resolution']='builtin' if ref['objectType'] else 'unsupported-builtin'
            if not ref['objectType']: ref['objectType']='UNRESOLVED_BUILTIN_FILEID'
        elif ref['guid'] in SCRIPT_GUIDS:
            ref['resolution']='script'
        elif ref['guid'] in index and Path(index[ref['guid']]).suffix.lower() == '.cs':
            ref['resolution']='script'
        elif ref['guid'] in index:
            ref['resolution']='package'
        else:
            ref['resolution']='unresolved'
    payload = {'format':'crisis-unity-records-v2','seeds':seeds,'guidPaths':index,
        'guidMetadata':metadata,'scriptTypes':SCRIPT_GUIDS,'scriptAssemblies':SCRIPT_ASSEMBLIES,
        'builtinObjects':BUILTIN_OBJECTS,'assets':assets,
        'objectReferences':references,'unresolvedGuids':missing,
        'sourceOfTruth':'sourceText/rawYaml or dataBase64 preserve original bytes; parsed fields are an execution view'}
    output.parent.mkdir(parents=True, exist_ok=True); output.write_text(json.dumps(payload, separators=(',',':')))
    execution = {'format':payload['format'],'guidPaths':index,'guidMetadata':metadata,
        'scriptTypes':SCRIPT_GUIDS,'scriptAssemblies':SCRIPT_ASSEMBLIES,
        'builtinObjects':BUILTIN_OBJECTS,
        'objectReferences':references,'assets':{
        path:{'path':a['path'],'sha256':a['sha256'],'size':a['size'],
            **({'dataBase64':a['dataBase64']} if a.get('binary') else {'sourceText':a.get('sourceText','')}),
            'records':[{k:v for k,v in r.items() if k!='rawYaml'} for r in a.get('records',[])]}
        for path,a in assets.items()}}
    # The archive preserves raw text; the browser only needs each requested
    # asset's exact parsed fields. Avoid loading all raw copies on an iPhone.
    output.with_name('scene-execution.json').write_text(json.dumps(execution,separators=(',',':')))
    summary = {'assets':len(assets),'records':sum(len(a.get('records',[])) for a in assets.values()),
        'unresolvedGuids':len(missing),'objectReferences':len(references),
        'binaryAssets':sum(bool(a.get('binary')) for a in assets.values()),'bytes':output.stat().st_size}
    output.with_suffix('.report.json').write_text(json.dumps(summary, indent=2)); print(json.dumps(summary))


if __name__ == '__main__': main()
