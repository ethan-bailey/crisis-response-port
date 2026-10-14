#!/usr/bin/env python3
"""Preserve full Unity YAML records, parsed fields, source hashes and dependencies."""
import hashlib
import json
import re
import sys
from collections import deque
from pathlib import Path
import yaml

HEADER = re.compile(r'^--- !u!(\d+) &(-?\d+)(?: stripped)?\s*$', re.M)
GUID = re.compile(r'guid: ([0-9a-f]{32})')


def parse_asset(path, relative):
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
            'fields':fields, 'rawYaml':raw})
    return {'path':relative, 'sha256':hashlib.sha256(data).hexdigest(), 'sourceText':text,
        'records':records, 'externalGuids':sorted(set(GUID.findall(text)))}


def main():
    project = Path(sys.argv[1]).resolve(); output = Path(sys.argv[2])
    index = {}
    for meta in (project/'Assets').rglob('*.meta'):
        m = re.search(r'^guid: ([0-9a-f]{32})$', meta.read_text(errors='strict'), re.M)
        if m: index[m.group(1)] = str(meta.with_suffix('').relative_to(project))
    seeds = ['Assets/Scenes/Marksman range.unity', 'Assets/Scenes/Mission 01.unity',
        'Assets/Resources/characters/Operator.prefab', 'ProjectSettings/TimeManager.asset', 'ProjectSettings/TagManager.asset']
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
    summary = {'assets':len(assets),'records':sum(len(a.get('records',[])) for a in assets.values()),
        'unresolvedGuids':len(missing),'bytes':output.stat().st_size}
    output.with_suffix('.report.json').write_text(json.dumps(summary, indent=2)); print(json.dumps(summary))


if __name__ == '__main__': main()
