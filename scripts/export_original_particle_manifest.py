#!/usr/bin/env python3
"""Build original impact-emitter data for the private WASM browser artifact.

Source is an audited, exact Unity serialized prefab; no particle behavior or
renderer defaults are invented. Disabled native modules are not executed.
"""
import argparse
import gzip
import hashlib
import json
from pathlib import Path
from extract_runtime_scene import parse_asset

p = argparse.ArgumentParser()
p.add_argument('--source-audit', type=Path, required=True)
p.add_argument('--project', type=Path, required=True)
p.add_argument('--out', type=Path, required=True)
a = p.parse_args()
source = json.loads(a.source_audit.read_text())
root = a.project
assets = []
seen_effects = set()
num_systems = num_renderers = 0
renderer_modes = {}


def file_id(value):
    if not isinstance(value, dict):
        raise ValueError('Expected serialized Unity reference')
    return str(value.get('fileID', '0'))


def immutable_hash(fields):
    return hashlib.sha256(json.dumps(fields, sort_keys=True,
                                     default=str).encode()).hexdigest()


for effect in source['effects']:
    if effect['guid'] in seen_effects:
        raise ValueError('Duplicate original effect GUID')
    seen_effects.add(effect['guid'])
    prefab_path = root / effect['path']
    parsed = parse_asset(prefab_path, effect['path'])
    transforms_by_go = {
        file_id(rec['fields']['m_GameObject']): rec['fields']
        for rec in parsed['records'] if rec['classID'] in (4, 224)
    }
    if len(effect['systems']) != len(effect['renderers']):
        raise ValueError('Renderer and original emitter component counts differ')
    renderers = {file_id(r['sourceFields']['m_GameObject']): r
                 for r in effect['renderers']}
    if len(renderers) != len(effect['renderers']):
        raise ValueError('One or more original renderers share a GameObject')
    out_effect = {'guid': effect['guid'], 'source': effect['path'],
                  'sourcePrefabSha256': parsed['sha256'], 'roles': effect['roles'],
                  'emitters': []}
    for component in effect['systems']:
        f = component['sourceFields']
        if immutable_hash(f) != component['sourceFieldsSha256']:
            raise ValueError('Original ParticleSystem field hash mismatch')
        go = file_id(f['m_GameObject'])
        if go not in renderers or go not in transforms_by_go:
            raise ValueError('Original ParticleSystem missing renderer or Transform')
        renderer = renderers.pop(go)
        rf = renderer['sourceFields']
        if immutable_hash(rf) != renderer['sourceFieldsSha256']:
            raise ValueError('Original ParticleSystemRenderer field hash mismatch')
        tr = transforms_by_go[go]
        active = {key: val for key, val in f.items()
                  if key.endswith('Module') and isinstance(val, dict)
                  and val.get('enabled') in (1, True)}
        disabled = sorted(key for key, val in f.items()
                          if key.endswith('Module') and isinstance(val, dict)
                          and val.get('enabled') in (0, False))
        all_modules = sum(1 for key in f if key.endswith('Module'))
        if len(active) + len(disabled) != all_modules:
            raise ValueError('Unspecified original module enable state')
        if not {'InitialModule', 'ShapeModule', 'EmissionModule'} <= active.keys():
            raise ValueError('Original mandatory impact emission module disabled')
        mode = rf['m_RenderMode']
        if mode not in (0, 4):
            raise ValueError('Unsupported original renderer mode: ' + str(mode))
        renderer_modes[mode] = renderer_modes.get(mode, 0) + 1
        settings = {key: val for key, val in f.items()
                    if not key.endswith('Module') and key not in
                    ('m_PrefabInternal', 'm_PrefabParentObject')}
        render_settings = {key: val for key, val in rf.items()
                           if key not in ('m_PrefabInternal', 'm_PrefabParentObject')}
        out_effect['emitters'].append({
            'sourceGameObject': go,
            'sourceEmitterFileID': component['fileID'],
            'sourceRendererFileID': renderer['fileID'],
            'originalEmitterFieldSha256': component['sourceFieldsSha256'],
            'originalRendererFieldSha256': renderer['sourceFieldsSha256'],
            'localTransform': {
                'position': tr['m_LocalPosition'],
                'rotation': tr['m_LocalRotation'],
                'scale': tr['m_LocalScale'],
                'parentTransform': tr.get('m_Father', {'fileID': '0'})
            },
            'sourceSettings': settings,
            'enabledModules': active,
            'disabledModules': disabled,
            'renderer': render_settings,
        })
        num_systems += 1
        num_renderers += 1
    if renderers:
        raise ValueError('Original renderers remain unpaired')
    assets.append(out_effect)
if len(assets) != 14 or num_systems != 26 or num_renderers != 26:
    raise ValueError('Exact native impact prefab/system coverage changed')
if renderer_modes != {0: 14, 4: 12}:
    raise ValueError('Unexpected original Unity impact renderer mode coverage')
result = {
    'kind': 'Recovered original Unity 2017 impact particle source manifest',
    'version': 1,
    'sourceAuditSha256': hashlib.sha256(a.source_audit.read_bytes()).hexdigest(),
    'effects': assets,
}
plain = json.dumps(result, sort_keys=True, separators=(',', ':')).encode('utf-8')
compressed = gzip.compress(plain, mtime=0)
a.out.parent.mkdir(parents=True, exist_ok=True)
a.out.write_bytes(compressed)
print(json.dumps({
    'sourceEffectPrefabs': len(assets),
    'sourceParticleSystems': num_systems,
    'sourceParticleRenderers': num_renderers,
    'nativeRenderModes': renderer_modes,
    'payloadBytes': len(plain),
    'compressedBytes': len(compressed),
    'payloadSha256': hashlib.sha256(plain).hexdigest(),
    'compressedSha256': hashlib.sha256(compressed).hexdigest(),
    'note': 'Source-only input for future browser adapter; no particle runtime implemented.'
}, sort_keys=True))
