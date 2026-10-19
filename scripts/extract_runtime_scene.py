#!/usr/bin/env python3
"""Preserve full Unity YAML records, parsed fields, source hashes and dependencies."""
import hashlib
import json
import re
import sys
from collections import deque
from pathlib import Path

HEADER = re.compile(r'^--- !u!(\d+) &(-?\d+)(?: stripped)?\s*$', re.M)
GUID = re.compile(r'guid: ([0-9a-f]{32})')


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
    try: text = data.decode('utf-8')
    except UnicodeDecodeError: return {'path':relative, 'sha256':hashlib.sha256(data).hexdigest(), 'binary':True}
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
    return {'path':relative, 'sha256':hashlib.sha256(data).hexdigest(), 'sourceText':text,
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
    index = {}
    for meta in (project/'Assets').rglob('*.meta'):
        m = re.search(r'^guid: ([0-9a-f]{32})$', meta.read_text(errors='strict'), re.M)
        if m: index[m.group(1)] = str(meta.with_suffix('').relative_to(project))
    seeds = runtime_asset_seeds(project)
    queue = deque(seeds); assets = {}; missing = []
    while queue:
        relative = queue.popleft()
        if relative in assets: continue
        asset = parse_asset(project/relative, relative); assets[relative] = asset
        for guid in asset.get('externalGuids', []):
            if guid in index:
                target = index[guid]
                # Source scripts/binary media are inventoried, not inlined into scene JSON.
                if Path(target).suffix in {'.asset','.prefab','.unity','.mat','.controller','.overrideController'}:
                    queue.append(target)
            elif not guid.startswith('0000000000000000'):
                missing.append({'asset':relative,'guid':guid})
    payload = {'format':'crisis-unity-records-v1','seeds':seeds,'guidPaths':index,'assets':assets,
        'unresolvedGuids':missing,'sourceOfTruth':'sourceText/rawYaml preserve original text; parsed fields are an execution view'}
    output.parent.mkdir(parents=True, exist_ok=True); output.write_text(json.dumps(payload, separators=(',',':')))
    execution = {'format':payload['format'],'guidPaths':index,'assets':{
        path:{'path':a['path'],'sha256':a['sha256'],'records':[{k:v for k,v in r.items() if k!='rawYaml'} for r in a.get('records',[])]}
        for path,a in assets.items()}}
    # The archive preserves raw text; the browser only needs each requested
    # asset's exact parsed fields. Avoid loading all raw copies on an iPhone.
    output.with_name('scene-execution.json').write_text(json.dumps(execution,separators=(',',':')))
    summary = {'assets':len(assets),'records':sum(len(a.get('records',[])) for a in assets.values()),
        'unresolvedGuids':len(missing),'bytes':output.stat().st_size}
    output.with_suffix('.report.json').write_text(json.dumps(summary, indent=2)); print(json.dumps(summary))


if __name__ == '__main__': main()
