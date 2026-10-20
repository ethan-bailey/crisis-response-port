#!/usr/bin/env python3
"""Verify source-byte, GUID/meta and transitive asset closure integrity."""
import base64
import hashlib
import json
import sys
from collections import Counter, defaultdict, deque
from pathlib import Path


def sha(data):
    return hashlib.sha256(data).hexdigest()


def verify(project, package_path):
    project = Path(project).resolve()
    package = json.loads(Path(package_path).read_text())
    if package.get('format') != 'crisis-unity-records-v2':
        raise ValueError(f"Unsupported package format: {package.get('format')}")
    errors = []
    guid_paths = package['guidPaths']
    guid_meta = package['guidMetadata']
    for guid, relative in guid_paths.items():
        meta = project / (relative + '.meta')
        if not meta.is_file():
            errors.append(f'missing meta: {guid} {relative}')
        elif guid_meta.get(guid, {}).get('metaSha256') != sha(meta.read_bytes()):
            errors.append(f'meta hash mismatch: {guid} {relative}')
    for relative, asset in package['assets'].items():
        path = project / relative
        if not path.is_file():
            errors.append(f'missing package asset: {relative}')
            continue
        data = path.read_bytes()
        if sha(data) != asset['sha256'] or len(data) != asset['size']:
            errors.append(f'asset hash/size mismatch: {relative}')
            continue
        if asset.get('binary'):
            try:
                payload = base64.b64decode(asset['dataBase64'], validate=True)
                if payload != data: errors.append(f'binary bytes mismatch: {relative}')
            except Exception as exc:
                errors.append(f'invalid binary payload {relative}: {exc}')
        elif asset.get('sourceText', '').encode('utf-8') != data:
            errors.append(f'text bytes mismatch: {relative}')
        record_ids = [str(record['fileID']) for record in asset.get('records', [])]
        if len(record_ids) != len(set(record_ids)):
            errors.append(f'duplicate object fileID in {relative}')

    outgoing = defaultdict(list)
    for ref in package['objectReferences']:
        outgoing[ref['source']].append(ref)
        resolution = ref['resolution']
        guid = ref['guid']
        if resolution == 'package':
            target = guid_paths.get(guid)
            if target is None:
                errors.append(f"missing GUID path: {ref['source']}:{ref['sourceFileID']} {guid}:{ref['fileID']}")
        elif resolution == 'script':
            if guid not in package['scriptTypes'] and not guid_paths.get(guid, '').lower().endswith('.cs'):
                errors.append(f"unmapped script GUID: {guid}")
        elif resolution == 'builtin':
            if not package['builtinObjects'].get(guid, {}).get(ref['fileID']):
                errors.append(f"unmapped builtin object: {guid}:{ref['fileID']}")
        elif resolution not in ('unsupported-builtin', 'unresolved'):
            errors.append(f"unknown edge classification: {resolution}")

    actors = {}
    for name in ('Operator', 'Occupant'):
        root = f'Assets/Resources/characters/{name}.prefab'
        if root not in package['assets']:
            errors.append(f'actual source prefab not packaged: {name}')
            continue
        seen = set(); pending = deque([root])
        while pending:
            source = pending.popleft()
            if source in seen: continue
            seen.add(source)
            for ref in outgoing[source]:
                if ref['resolution'] == 'package':
                    target = guid_paths[ref['guid']]
                    if target not in package['assets']:
                        errors.append(f'closure gap for {name}: {source} -> {target}')
                    elif target not in seen:
                        pending.append(target)
        for path in seen:
            for ref in outgoing[path]:
                if ref['resolution'] == 'package' and guid_paths[ref['guid']] not in package['assets']:
                    errors.append(f"closure gap for {name}: {path} -> {guid_paths[ref['guid']]}")
        actors[name] = {
            'assets': len(seen),
            'bytes': sum(package['assets'][path]['size'] for path in seen),
            'records': sum(len(package['assets'][path].get('records', [])) for path in seen),
            'unresolvedGuids': sum(1 for path in seen for ref in outgoing[path]
                if ref['resolution'] == 'unresolved'),
            'unsupportedBuiltins': sum(1 for path in seen for ref in outgoing[path]
                if ref['resolution'] == 'unsupported-builtin'),
            'scriptReferenceOccurrences': [
                {'guid':guid,'fileID':file_id,'count':count}
                for (guid,file_id),count in sorted(Counter(
                    (ref['guid'], ref['fileID'] or '') for path in seen for ref in outgoing[path]
                    if ref['resolution'] == 'script').items())],
            'builtinReferenceOccurrences': [
                {'guid':guid,'fileID':file_id,'objectType':kind,'count':count}
                for (guid,file_id,kind),count in sorted(Counter(
                    (ref['guid'], ref['fileID'], ref['objectType']) for path in seen
                    for ref in outgoing[path] if ref['resolution'] in ('builtin','unsupported-builtin')).items())],
        }
    if errors:
        raise ValueError('\n'.join(errors[:100]) + (f'\n... {len(errors)-100} more' if len(errors)>100 else ''))
    return {'packageAssets': len(package['assets']), 'packageRecords': sum(
        len(asset.get('records', [])) for asset in package['assets'].values()),
        'packageEdges': len(package['objectReferences']), 'actors': actors}


if __name__ == '__main__':
    project, package = map(Path, sys.argv[1:3])
    print(json.dumps(verify(project, package), indent=2))
