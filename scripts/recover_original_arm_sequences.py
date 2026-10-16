#!/usr/bin/env python3
"""Recover unchanged arm-sequence C# and prefab fields from the pinned archive."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import zipfile

ARCHIVE_SHA = 'cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d'
SCRIPT_GUID = 'c7d54f5ebb1ade51a48c2fef96bf390f'
CLASSES = ['IKArmAction', 'IKArmTarget', 'IKControllerInvoke', 'IKArmAnimationFrame', 'IKArmSequence', 'IKArmSequenceRuntime']
PREFABS = ['Apply gauze', 'Holster pistol', 'Lower weapon', 'Pickup grenade', 'Raise shoulder weapon', 'Return grenade', 'Unholster pistol']

def sha(data):
    return hashlib.sha256(data).hexdigest()

def recover(archive, repo):
    # PyYAML is needed only for offline source recovery, never in the game.
    import yaml
    if sha(archive.read_bytes()) != ARCHIVE_SHA:
        raise ValueError('Original archive SHA256 mismatch')
    source_dir = repo / 'runtime/Original/Animation'
    source_dir.mkdir(parents=True, exist_ok=True)
    records = {}
    sequences = []
    with zipfile.ZipFile(archive) as z:
        prefix = 'export/ExportedProject/'
        for name in CLASSES:
            path = 'Assets/Scripts/Assembly-CSharp/' + name + '.cs'
            raw = z.read(prefix + path)
            target = source_dir / (name + '.cs')
            if target.exists() and target.read_bytes() != raw:
                raise ValueError('Refusing to overwrite different original source: ' + str(target))
            target.write_bytes(raw)
            records[path] = sha(raw)
        for name in PREFABS:
            path = 'Assets/GameObject/' + name + '.prefab'
            raw = z.read(prefix + path)
            meta = z.read(prefix + path + '.meta')
            text = '\n'.join('---' if line.startswith('--- !u!') else line
                             for line in raw.decode().splitlines() if not line.startswith('%'))
            components = [d['MonoBehaviour'] for d in yaml.safe_load_all(text)
                          if d and 'MonoBehaviour' in d and d['MonoBehaviour'].get('m_Script', {}).get('guid') == SCRIPT_GUID]
            if len(components) != 1:
                raise ValueError('Expected one original sequence: ' + path)
            m = components[0]
            guid = re.search(r'^guid: ([a-f0-9]{32})$', meta.decode(), re.M).group(1)
            sequences.append(dict(name=name, path=path, guid=guid, sourceSha256=sha(raw),
                                  right=m['right'], left=m['left']))
            records[path] = sha(raw)
            records[path + '.meta'] = sha(meta)
    bundle = dict(schema=1, archiveSha256=ARCHIVE_SHA, scriptGuid=SCRIPT_GUID,
                  playbackImplemented=False, sequences=sequences)
    asset = repo / 'game/assets/original-arm-sequences.json'
    asset.parent.mkdir(parents=True, exist_ok=True)
    asset.write_text(json.dumps(bundle, sort_keys=True, indent=2) + '\n')
    manifest = dict(archiveSha256=ARCHIVE_SHA, originalFiles=records,
                    bundleSha256=sha(asset.read_bytes()), originalSourcesUnmodified=True,
                    playbackImplemented=False)
    (source_dir / 'source-provenance.json').write_text(json.dumps(manifest, sort_keys=True, indent=2) + '\n')
    return dict(originalClasses=len(CLASSES), prefabs=len(sequences),
                frames=sum(len(s['right']) + len(s['left']) for s in sequences), bundleSha256=manifest['bundleSha256'])

if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('archive', type=Path)
    p.add_argument('--repo', type=Path, default=Path('.'))
    args = p.parse_args()
    print(json.dumps(recover(args.archive, args.repo), sort_keys=True))
