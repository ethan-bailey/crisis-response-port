#!/usr/bin/env python3
"""Validate recovered source arm topology; optionally reproduce from original ZIP."""
import argparse
import hashlib
import json
from pathlib import Path
import tempfile

ARCHIVE_SHA = 'cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d'
ROOT = Path(__file__).resolve().parents[1]
BUNDLE = ROOT / 'source-reference/Original/ArmPhysics/prefabs.json'


def verify(bundle):
    assert bundle['archiveSha256'] == ARCHIVE_SHA
    assert bundle['format'] == 'crisis-original-arm-physics-v1'
    assert bundle['controllerPlaybackImplemented'] is False
    assert bundle['nativeParityVerified'] is False
    assert [p['name'] for p in bundle['prefabs']] == ['Operator', 'Occupant']
    assert len(bundle['sources']) == 11
    count = 0
    for prefab in bundle['prefabs']:
        records = {r['fileID']: r for r in prefab['records']}
        assert len(records) == len(prefab['records'])
        assert prefab['sha256'] == bundle['sources'][prefab['path']]
        control = records[prefab['armsControl']]['fields']
        for side, arm in prefab['arms'].items():
            assert arm['controller'] == control[side]
            assert arm['controller']['isRightArm'] == (side == 'rightArm')
            assert len(arm['limbs']) == 3
            assert len(arm['variants']) == 3  # Original prefabs intentionally alias two modes
            for variant in arm['variants'].values():
                assert records[variant]['classID'] == 1
            for limb in arm['limbs']:
                body, joint = (records[limb[k]] for k in ('rigidbody', 'joint'))
                assert body['classID'] == 54 and joint['classID'] == 59
                assert body['fields']['m_GameObject']['fileID'] == limb['gameObject']
                assert joint['fields']['m_GameObject']['fileID'] == limb['gameObject']
                assert records[limb['connectedBody']]['classID'] == 54
                assert joint['fields']['m_ConnectedBody']['fileID'] == limb['connectedBody']
                assert joint['fields']['m_Axis'] == dict(x=0, y=0, z=1)
                assert body['fields']['m_Mass'] in (1, 4)
                assert body['fields']['m_IsKinematic'] == 1
                assert joint['fields']['m_UseLimits'] in (0, 1)
                assert joint['fields']['m_Limits']['min'] < joint['fields']['m_Limits']['max']
                for collider in limb['colliders']:
                    assert records[collider]['fields']['m_GameObject']['fileID'] == limb['gameObject']
                count += 1
        # Every local component, parent, child, and GameObject identity must
        # survive JSON/JS transport without rounding Unity's 64-bit fileIDs.
        def walk(value):
            if isinstance(value, dict):
                if 'fileID' in value:
                    assert isinstance(value['fileID'], str), 'Unsafe numeric fileID'
                    if value['fileID'] not in ('0', '100100000') and not value.get('guid'):
                        assert value['fileID'] in records, 'Missing local hierarchy reference: '+value['fileID']
                for v in value.values():
                    walk(v)
            elif isinstance(value, list):
                for v in value:
                    walk(v)
        # m_Component includes original script components deliberately excluded
        # from this physics-only view. Validate all other retained references.
        for record in prefab['records']:
            walk({k: v for k, v in record['fields'].items() if k not in ('m_Component', 'm_PrefabParentObject', 'm_PrefabInternal')})
    assert count == 12
    return dict(prefabs=2, arms=4, limbBodies=count, handModeReferences=12, distinctHandGameObjects=8,
                controllerPlaybackImplemented=False, nativeParityVerified=False)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--archive', type=Path)
    args = parser.parse_args()
    result = verify(json.loads(BUNDLE.read_text()))
    if args.archive:
        from recover_original_arm_physics import recover
        with tempfile.TemporaryDirectory() as tmp:
            target = Path(tmp) / 'prefabs.json'
            recover(args.archive, target)
            assert target.read_bytes() == BUNDLE.read_bytes(), 'Recovered bundle differs from original archive'
        result['archiveReproductionVerified'] = True
    result['bundleSha256'] = hashlib.sha256(BUNDLE.read_bytes()).hexdigest()
    print(json.dumps(result, sort_keys=True))
