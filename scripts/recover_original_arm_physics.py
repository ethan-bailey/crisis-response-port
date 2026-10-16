#!/usr/bin/env python3
"""Recover arm physics references verbatim from the checksum-pinned private ZIP.

This is asset recovery, not controller playback or a parity claim. References
stay decimal strings; no browser-unsafe numeric Unity IDs are emitted.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import tempfile
import zipfile

from extract_runtime_scene import parse_asset

ARCHIVE_SHA = 'cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d'
PREFIX = 'export/ExportedProject/'
ARMS_GUID = 'a4d84857e59f0d1f1911b22b0cd6f4b5'
HAND_GUID = 'b7828263351db8f41bb669bc050fac82'


def digest(data):
    return hashlib.sha256(data).hexdigest()


def recover(archive, output):
    if digest(archive.read_bytes()) != ARCHIVE_SHA:
        raise ValueError('Original archive SHA256 mismatch')
    sources = {}
    with zipfile.ZipFile(archive) as z, tempfile.TemporaryDirectory() as tmp:
        def read(path):
            raw = z.read(PREFIX + path)
            sources[path] = digest(raw)
            local = Path(tmp) / Path(path).name
            local.write_bytes(raw)
            return parse_asset(local, path)['records']

        # Resolve external mesh/material GUIDs against the same archive, never
        # a name match or an independently regenerated source asset.
        guid_paths = {}
        for name in z.namelist():
            if name.startswith(PREFIX) and name.endswith('.meta'):
                match = re.search(rb'^guid: ([a-f0-9]{32})$', z.read(name), re.M)
                if match:
                    guid_paths[match[1].decode()] = name[len(PREFIX):-5]

        prefabs = []
        for name in ('Operator', 'Occupant'):
            path = 'Assets/Resources/characters/' + name + '.prefab'
            records = read(path)
            by_id = {r['fileID']: r for r in records}
            by_go = {}
            for record in records:
                go = record['fields'].get('m_GameObject', {}).get('fileID')
                if go:
                    by_go.setdefault(go, []).append(record)

            def record(ref, class_id=None):
                value = by_id.get(ref['fileID'])
                if not value or (class_id is not None and value['classID'] != class_id):
                    raise ValueError(f'Unresolved {name} reference: {ref}, expected {class_id}')
                return value

            def component(go, class_id):
                found = [r for r in by_go.get(go, []) if r['classID'] == class_id]
                if len(found) != 1:
                    raise ValueError(f'Expected one {class_id} on {name}:{go}, got {len(found)}')
                return found[0]

            control = [r for r in records if r['classID'] == 114 and
                       r['fields'].get('m_Script', {}).get('guid') == ARMS_GUID]
            if len(control) != 1:
                raise ValueError('Expected exactly one original ArmsControl: ' + name)
            arms = {}
            for side in ('rightArm', 'leftArm'):
                fields = control[0]['fields'][side]
                hand = record(fields['hand'], 114)
                if hand['fields']['m_Script']['guid'] != HAND_GUID:
                    raise ValueError('Arm hand does not resolve to original Hand')
                hand_go = hand['fields']['m_GameObject']['fileID']
                transforms = [record(fields[k], 4) for k in ('upperArm', 'lowerArm')]
                transforms.append(component(hand_go, 4))
                limbs = []
                for transform in transforms:
                    go = transform['fields']['m_GameObject']['fileID']
                    rigidbody = component(go, 54)
                    joint = component(go, 59)
                    connected = record(joint['fields']['m_ConnectedBody'], 54)
                    colliders = [r['fileID'] for r in by_go[go] if r['classID'] in (65, 135, 136)]
                    if not colliders:
                        raise ValueError('No original limb collider: ' + go)
                    limbs.append(dict(gameObject=go, transform=transform['fileID'],
                                      rigidbody=rigidbody['fileID'], joint=joint['fileID'],
                                      connectedBody=connected['fileID'], colliders=colliders))
                variants = {k: record(hand['fields'][k], 1)['fileID']
                            for k in ('fist', 'trigger', 'forwardAssist')}
                record(hand['fields']['grabPoint'], 4)
                arms[side] = dict(controller=fields, limbs=limbs, hand=hand['fileID'],
                                  variants=variants)

            # Preserve original native hierarchy: parent frames, connected body
            # colliders, disabled hand variants, and authored activation state.
            clean = [{k: v for k, v in r.items() if k != 'rawYaml'} for r in records
                     if r['classID'] in (1, 4, 23, 33, 54, 59, 65, 135, 136, 137) or
                     (r['classID'] == 114 and r['fields'].get('m_Script', {}).get('guid') in (ARMS_GUID, HAND_GUID))]
            external = {}
            def refs(value):
                if isinstance(value, dict):
                    if value.get('guid'):
                        yield value['guid']
                    for child in value.values():
                        yield from refs(child)
                elif isinstance(value, list):
                    for child in value:
                        yield from refs(child)
            for guid in sorted(set(refs(clean))):
                source = guid_paths.get(guid)
                external[guid] = dict(path=source, sha256=digest(z.read(PREFIX + source)) if source else None)
            prefabs.append(dict(name=name, path=path, sha256=sources[path],
                                armsControl=control[0]['fileID'], arms=arms,
                                records=clean, externalAssets=external))

        settings = read('ProjectSettings/DynamicsManager.asset')
        time = read('ProjectSettings/TimeManager.asset')
        for script in ('ArmController', 'ArmsControl', 'Hand', 'CharacterBody', 'ArmIKSolver', 'IKArmController', 'IKArmSequencer'):
            path = 'Assets/Scripts/Assembly-CSharp/' + script + '.cs'
            sources[path] = digest(z.read(PREFIX + path))
        bundle = dict(format='crisis-original-arm-physics-v1', archiveSha256=ARCHIVE_SHA,
                      sources=sources, physics=settings, time=time, prefabs=prefabs,
                      controllerPlaybackImplemented=False, nativeParityVerified=False)
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_text(json.dumps(bundle, sort_keys=True, separators=(',', ':'), allow_nan=False) + '\n')
        return dict(prefabs=len(prefabs), arms=sum(len(p['arms']) for p in prefabs),
                    limbBodies=sum(len(a['limbs']) for p in prefabs for a in p['arms'].values()),
                    bundleSha256=digest(output.read_bytes()))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('archive', type=Path)
    parser.add_argument('--output', type=Path, default=Path('source-reference/Original/ArmPhysics/prefabs.json'))
    args = parser.parse_args()
    print(json.dumps(recover(args.archive, args.output), sort_keys=True))
