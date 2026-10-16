#!/usr/bin/env python3
"""Verify source-selected original IK execution and recovered hand renderer bytes."""
import gzip
import hashlib
import json
from pathlib import Path
from source_arm_runtime import build_arm_transition

ROOT=Path(__file__).resolve().parents[1]
sha=lambda data:hashlib.sha256(data).hexdigest()
source=(ROOT/'source-reference/Original/Animation/ArmIKSolver.cs').read_text()
runtime=(ROOT/'runtime/Original/Weapons/ArmTransition.cs').read_text()
assert runtime==build_arm_transition(source), 'Original IK selection drifted'
provenance=json.loads((ROOT/'source-reference/Original/ArmPhysics/ik-runtime-provenance.json').read_text())
assert provenance['sourceSha256']==sha(source.encode())
assert provenance['outputSha256']==sha(runtime.encode())
assert provenance['fullIKArmControllerPlayback'] is False
hands=json.loads((ROOT/'game/assets/operator/original-hand-renderers.json').read_text())
assert hands['format']=='crisis-original-hand-renderers-v1'
assert hands['sources']['Assets/Resources/characters/Operator.prefab']=='94bcda5de74f45d83f4bbf88e80874b8f77bf0339d307294187b085c03170536'
assert len(hands['renderers'])==4 and len(hands['textures'])==3
operator=json.loads((ROOT/'game/assets/operator/operator.json').read_text())
node_ids={n['gameObjectFileID'] for n in operator['nodes']}
source_skin_ids={l['gameObjectFileID'] for l in operator['limbs']}
for renderer in hands['renderers']:
    assert renderer['gameObjectFileID'] in node_ids
    assert renderer['gameObjectFileID'] not in source_skin_ids, 'Duplicate LimbRenderer/hand rendering'
    assert renderer['mesh']['fileID']=='10210'
    assert renderer['sourceShaderIsDummy'] is True, 'Do not claim recovered native shader'
    assert sha((ROOT/'game/assets/operator'/renderer['texture']).read_bytes())==renderer['textureSha256']
config=json.loads(gzip.decompress((ROOT/'game/assets/weapon-runtime.json.gz').read_bytes()))
assert set(config['armRigBindings'])=={'left','right'}
for side,rig in config['armRigBindings'].items():
    assert all(isinstance(rig[key],str) for key in ['solverTransform','upperTransform','lowerTransform','handTransform'])
    for visual in rig['variants'].values():
        assert visual['gameObject'] in node_ids
        assert visual['gameObject'] in {r['gameObjectFileID'] for r in hands['renderers']}
    assert len({v['gameObject'] for v in rig['variants'].values()})==2, 'Preserve original aliased hand modes'
print(json.dumps(dict(originalIKSelectedMethods=len(provenance['selectedMethods']),handQuads=4,
                     sourceTextures=3,originalHandSetModeConnected=True,
                     fullIKArmControllerPlayback=False,nativeShaderParity=False),sort_keys=True))
