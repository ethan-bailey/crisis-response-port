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
    '00000000000000000000000000000000': {
        '10206':'Mesh',
    },
    '0000000000000000e000000000000000': {
        '10102':'Font', '10206':'Mesh', '10210':'Mesh',
    },
    '0000000000000000f000000000000000': {
        '45':'Shader', '46':'Shader', '208':'Shader', '10303':'Material', '10753':'Shader', '10754':'Material',
            '10907': 'Sprite', '10913': 'Sprite',
    },
}
BUILTIN_RESOURCE_PACK_SHA256 = 'ad613f8d50a4d58be86781e0b332f8410727c8b2a77306d100ee3bc162e5272e'


def load_builtin_resource_records(repository_root=None):
    """Load only the exact-version built-in records used by serialized references."""
    root = Path(repository_root) if repository_root else Path(__file__).resolve().parents[1]
    path = root / 'source-reference/Unity/2017.2.0f3/builtin-resource-records.json'
    data = path.read_bytes()
    digest = hashlib.sha256(data).hexdigest()
    if digest != BUILTIN_RESOURCE_PACK_SHA256:
        raise ValueError(f'Unity built-in resource pack SHA-256 mismatch: {digest}')
    pack = json.loads(data)
    if pack.get('unityVersion') != '2017.2.0f3' or pack.get('revision') != '46dda1414e51':
        raise ValueError('Unity built-in resource pack version or revision is unexpected')
    records = {record['fileID']: record for record in pack.get('records', [])}
    material = records.get('10303')
    shader = records.get('46')
    if not material or material.get('type') != 'Material' or material.get('fields', {}).get('m_Name') != 'Default-Material':
        raise ValueError('Unity built-in Material 10303 source record is missing or invalid')
    if not shader or shader.get('type') != 'Shader' or shader.get('fields', {}).get('m_ParsedForm', {}).get('m_Name') != 'Standard':
        raise ValueError('Unity built-in Shader 46 source record is missing or invalid')
    sprite_material = records.get('10754')
    sprite_shader = records.get('10753')
    if not sprite_material or sprite_material.get('type') != 'Material' or sprite_material.get('fields', {}).get('m_Name') != 'Sprites-Default':
        raise ValueError('Unity built-in Sprite Material 10754 source record is missing or invalid')
    if not sprite_shader or sprite_shader.get('type') != 'Shader' or sprite_shader.get('fields', {}).get('m_ParsedForm', {}).get('m_Name') != 'Sprites/Default':
        raise ValueError('Unity built-in Sprite Shader 10753 source record is missing or invalid')
    sprite_pointer = sprite_material['fields'].get('m_Shader', {})
    if sprite_pointer.get('m_FileID') != 0 or sprite_pointer.get('m_PathID') != 10753:
        raise ValueError('Unity built-in Sprite Material 10754 no longer points to Shader 10753')
    pointer = material['fields'].get('m_Shader', {})
    if pointer.get('m_FileID') != 0 or pointer.get('m_PathID') != 46:
        raise ValueError('Unity built-in Material 10303 no longer points to Shader 46')
    player = pack.get('playerResources')
    if (not isinstance(player, dict) or player.get('unityVersion') != '2017.2.0b11'
            or player.get('targetPlatform') != 5
            or player.get('sourceSha256') != 'b809e16256c643c6ac8b5e6d0af164a9c9ec07e0e5bd49460939c39da5fafb40'):
        raise ValueError('Windows x86 Mono resource provenance is missing or changed')
    cylinder = player.get('records', {}).get('10206', {})
    if (cylinder.get('type') != 'Mesh' or cylinder.get('fields', {}).get('m_Name') != 'Cylinder'
            or cylinder.get('rawSha256') != '1aea642b2ab9c4fe2458206b6d95c956788b91fa854b2bbc6daa95e3c23886a7'):
        raise ValueError('Windows x86 Mono Mesh 10206 source record is missing or changed')
    geometry = cylinder.get('runtimeGeometry', {})
    if (len(geometry.get('vertices', [])) != 88 or len(geometry.get('normals', [])) != 88
            or len(geometry.get('uv', [])) != 88 or len(geometry.get('triangles', [])) != 240):
        raise ValueError('Windows x86 Mono Cylinder runtime geometry is incomplete')
    scene_mesh = player.get('records', {}).get('10210', {})
    scene_geometry = scene_mesh.get('runtimeGeometry', {})
    if (scene_mesh.get('type') != 'Mesh' or scene_mesh.get('fileID') != '10210'
            or not scene_mesh.get('rawSha256') or not scene_mesh.get('rawBase64')
            or not scene_geometry.get('vertices') or not scene_geometry.get('triangles')
            or len(scene_geometry['triangles']) % 3):
        raise ValueError('Windows x86 Mono source Mesh 10210 record or geometry is incomplete')
    return {
        'packSha256': digest,
        'revision': pack['revision'],
        'unityVersion': pack['unityVersion'],
        'records': {'46': shader, '10303': material, '10753': sprite_shader, '10754': sprite_material, '10907': records['10907'], '10913': records['10913']},
        'playerResourceRecords': {
            'unityVersion': player['unityVersion'],
            'targetPlatform': player['targetPlatform'],
            'sourceFile': player['sourceFile'],
            'sourceSha256': player['sourceSha256'],
            'sourceBytes': player['sourceBytes'],
            'parser': player['parser'],
            'records': player['records'],
        },
    }


def execution_value(value, key=None):
    # Unity prefab references exceed JavaScript's exact integer range. Keep
    # identity values decimal strings through every JSON/JS/.NET boundary.
    if isinstance(value, dict): return {k:execution_value(v,k) for k,v in value.items()}
    if isinstance(value, list): return [execution_value(v) for v in value]
    if isinstance(value, int) and (key=='fileID' or abs(value)>2**53-1): return str(value)
    return value


def pptr_guids(records):
    """Return GUIDs from Unity object pointers, not GUID-shaped data keys."""
    found = set()

    def walk(value):
        if isinstance(value, dict):
            if 'fileID' in value and 'guid' in value:
                guid = value.get('guid')
                if isinstance(guid, str) and re.fullmatch(r'[0-9a-f]{32}', guid.lower()):
                    found.add(guid.lower())
            for child in value.values():
                walk(child)
        elif isinstance(value, list):
            for child in value:
                walk(child)

    for record in records:
        walk(record.get('fields', {}))
    return sorted(found)


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
    external_guids = sorted(set(GUID.findall(text)))
    if Path(relative).suffix.lower() == '.mixer':
        # AudioMixer snapshots store internal parameter IDs as GUID-keyed scalar
        # maps. Only serialized fileID+guid pointers in the parsed records are
        # dependencies; treating every GUID-shaped map key as an asset edge
        # invents missing source assets.
        external_guids = pptr_guids(records)
    return {'path':relative, 'sha256':digest,'size':len(data),'sourceText':text,
        'records':records, 'externalGuids':external_guids}


def runtime_asset_seeds(project):
    project = Path(project).resolve()
    seeds = ['Assets/Scenes/Marksman range.unity', 'Assets/Scenes/Mission 01.unity',
        'Assets/Resources/characters/Operator.prefab', 'Assets/Resources/characters/Occupant.prefab',
        'ProjectSettings/TimeManager.asset', 'ProjectSettings/TagManager.asset']
    # Resources.Load paths are runtime strings, not serialized GUID edges.
    seeds.extend(str(p.relative_to(project)) for p in sorted((project/'Assets/Resources/weapons/ammunition').glob('*.prefab')))
    return seeds


def require_scene_sources(project):
    """Fail early when the original Unity scenes needed for extraction are absent."""
    project = Path(project).resolve()
    required = (project/'Assets/Scenes/Marksman range.unity', project/'Assets/Scenes/Mission 01.unity')
    missing = [path.relative_to(project).as_posix() for path in required if not path.is_file()]
    if missing:
        raise MissingSceneSourcesError(
            'Required original Unity scene source file(s) are missing: ' + ', '.join(missing) +
            '. Provide the complete original Unity project; scene data will not be synthesized.'
        )


class MissingSceneSourcesError(ValueError):
    """Raised when the source project lacks required original scene files."""


def main():
    project = Path(sys.argv[1]).resolve(); output = Path(sys.argv[2])
    require_scene_sources(project)
    builtin_resource_records = load_builtin_resource_records()
    index = {}; metadata={}
    for meta in (project/'Assets').rglob('*.meta'):
        m = re.search(r'^guid: ([0-9a-f]{32})$', meta.read_text(errors='strict'), re.M)
        if m:
            guid=m.group(1); target=str(meta.with_suffix('').relative_to(project))
            if guid in index and index[guid]!=target: raise ValueError(f'Duplicate Unity GUID {guid}: {index[guid]} and {target}')
            index[guid]=target
            metadata[guid]={'path':target,'metaSha256':hashlib.sha256(meta.read_bytes()).hexdigest()}
            if Path(target).suffix.lower() in {'.png','.jpg','.jpeg','.tga','.bmp'}:
                # Preserve the recovered import contract alongside the source bytes.
                metadata[guid]['importerSourceText']=meta.open('r', encoding='utf-8', errors='strict', newline='').read()
                metadata[guid]['importerSourceBase64']=base64.b64encode(meta.read_bytes()).decode('ascii')
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
        'builtinObjects':BUILTIN_OBJECTS,'builtinResourceRecords':builtin_resource_records,
        'builtinPlayerResourceRecords':builtin_resource_records['playerResourceRecords'],'assets':assets,
        'objectReferences':references,'unresolvedGuids':missing,
        'sourceOfTruth':'sourceText/rawYaml or dataBase64 preserve original bytes; parsed fields are an execution view'}
    output.parent.mkdir(parents=True, exist_ok=True); output.write_text(json.dumps(payload, separators=(',',':')))
    execution = {'format':payload['format'],'guidPaths':index,'guidMetadata':metadata,
        'scriptTypes':SCRIPT_GUIDS,'scriptAssemblies':SCRIPT_ASSEMBLIES,
        'builtinObjects':BUILTIN_OBJECTS,'builtinResourceRecords':builtin_resource_records,
        'builtinPlayerResourceRecords':builtin_resource_records['playerResourceRecords'],
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


if __name__ == '__main__':
    try:
        main()
    except MissingSceneSourcesError as error:
        raise SystemExit(str(error))
