#!/usr/bin/env python3
"""Extract pinned official Unity records without installing/running Unity.

Requires UnityPy==1.25.4 and 7-Zip's Linux 7zz. No Unity activation or Actions
runner is needed. Download inputs with --download, then verify/reproduce the
checked-in source record pack with --check. Never infer player parity from an
Editor resource record or execute the Windows installer.
"""
import argparse
import base64
import hashlib
import json
import subprocess
import urllib.request
import zipfile
from pathlib import Path

BASE = 'https://download.unity3d.com/download_unity/46dda1414e51/'
INPUTS = {
    'UnitySetup64-2017.2.0f3.exe': (
        'Windows64EditorInstaller/UnitySetup64-2017.2.0f3.exe',
        'df48bdc3e204c510e5a1fe257694a8b52f8bef477aaca975723936cec7bc91da'),
    'builtin_shaders-2017.2.0f3.zip': (
        'builtin_shaders-2017.2.0f3.zip',
        '90cc8a0536303d9d044646d611d6305ea732551366aa842893eedaf322c235b0'),
}
RESOURCES = {
    'Editor/Data/Resources/unity_builtin_extra':
        '4c754e7c16d3897981890f5befcf3b68f540e7f1dd5e572360c39b2931af62b0',
    'Editor/Data/Resources/unity default resources':
        'e30e482fba6803b21fd6ea039a8aa92f74566bdc0425d34f654e07e31ae28795',
    'Editor/Data/PlaybackEngines/windowsstandalonesupport/Variations/'
    'win32_nondevelopment_mono/Data/Resources/unity default resources':
        'b809e16256c643c6ac8b5e6d0af164a9c9ec07e0e5bd49460939c39da5fafb40',
}
PLAYER_RESOURCE = ('Editor/Data/PlaybackEngines/windowsstandalonesupport/Variations/'
                   'win32_nondevelopment_mono/Data/Resources/unity default resources')


def digest(raw):
    return hashlib.sha256(raw).hexdigest()


def checked(path, expected):
    raw = path.read_bytes()
    if digest(raw) != expected:
        raise ValueError('SHA256 mismatch: ' + str(path))
    return raw


def json_value(value):
    if isinstance(value, bytes):
        return {'encoding': 'base64', 'data': base64.b64encode(value).decode()}
    if isinstance(value, dict):
        return {k: json_value(v) for k, v in value.items()}
    if isinstance(value, (tuple, list)):
        return [json_value(v) for v in value]
    return value


def recover(cache, extractor):
    import UnityPy
    from importlib.metadata import version
    if version('UnityPy') != '1.25.4':
        raise ValueError('Use UnityPy==1.25.4 for reproducible type trees')
    for name, (_, sha) in INPUTS.items():
        checked(cache / name, sha)
    extracted = cache / 'extracted'
    subprocess.run([str(extractor), 'x', '-y', '-o' + str(extracted),
                    str(cache / 'UnitySetup64-2017.2.0f3.exe'),
                    *RESOURCES], check=True, stdout=subprocess.DEVNULL)
    files, records = [], []
    player_resource_records = None
    for relative, sha in RESOURCES.items():
        raw = checked(extracted / relative, sha)
        env = UnityPy.load(str(extracted / relative))
        assets = list(env.files.values())
        asset = assets[0]
        files.append({'installerPath': relative, 'sha256': sha, 'bytes': len(raw),
                      'serializedVersion': asset.unity_version,
                      'targetPlatform': int(asset.target_platform),
                      'objectIDs': sorted(o.path_id for o in env.objects)})
        if relative.endswith('unity_builtin_extra'):
            if asset.unity_version != '2017.2.0f3':
                raise ValueError('Wrong built-in-extra serialized Unity version')
            builtin_extra = {obj.path_id: obj for obj in env.objects}
            sprite_material = builtin_extra.get(10754)
            if sprite_material is None or sprite_material.type.name != 'Material':
                raise ValueError('Exact-release editor unity_builtin_extra has no Material 10754')
            sprite_fields = sprite_material.read_typetree()
            sprite_shader_id = int(sprite_fields['m_Shader']['m_PathID'])
            wanted_ids = {46, 10303, 10754, sprite_shader_id}
            for obj in sorted(env.objects, key=lambda o: o.path_id):
                if obj.path_id not in wanted_ids:
                    continue
                raw_obj = obj.get_raw_data()
                tree = obj.read_typetree()
                records.append({'fileID': str(obj.path_id), 'type': obj.type.name,
                                'sourceFile': relative, 'rawSha256': digest(raw_obj),
                                'rawBytes': len(raw_obj),
                                'rawBase64': base64.b64encode(raw_obj).decode(),
                                'fields': json_value(tree)})
        elif relative == PLAYER_RESOURCE:
            if asset.unity_version != '2017.2.0b11' or int(asset.target_platform) != 5:
                raise ValueError('Windows x86 Mono resource header or target platform changed')
            from UnityPy.helpers.MeshHelper import MeshHandler
            obj = next((item for item in env.objects if item.path_id == 10206), None)
            if obj is None or obj.type.name != 'Mesh':
                raise ValueError('Windows x86 Mono resource has no Mesh 10206')
            raw_obj = obj.get_raw_data()
            tree = obj.read_typetree()
            mesh = obj.read()
            handler = MeshHandler(mesh)
            handler.process()
            geometry = {
                'vertices': handler.m_Vertices,
                'normals': handler.m_Normals,
                'uv': handler.m_UV0,
                'triangles': [index for submesh in handler.get_triangles()
                              for triangle in submesh for index in triangle],
            }
            if (tree.get('m_Name') != 'Cylinder' or len(geometry['vertices'] or []) != 88
                    or len(geometry['triangles'] or []) != 240):
                raise ValueError('Windows x86 Mono Mesh 10206 is not the expected Cylinder geometry')
            player_resource_records = {
                'unityVersion': asset.unity_version,
                'targetPlatform': int(asset.target_platform),
                'sourceFile': relative,
                'sourceSha256': sha,
                'sourceBytes': len(raw),
                'parser': 'UnityPy==1.25.4',
                'records': {'10206': {
                    'fileID': '10206', 'type': obj.type.name, 'sourceFile': relative,
                    'rawSha256': digest(raw_obj), 'rawBytes': len(raw_obj),
                    'rawBase64': base64.b64encode(raw_obj).decode(),
                    'fields': json_value(tree), 'runtimeGeometry': json_value(geometry),
                }},
            }
    by_id = {r['fileID']: r for r in records}
    assert by_id['10303']['type'] == 'Material'
    assert by_id['10303']['fields']['m_Name'] == 'Default-Material'
    assert by_id['10303']['fields']['m_Shader'] == {'m_FileID': 0, 'm_PathID': 46}
    assert by_id['46']['type'] == 'Shader'
    assert by_id['46']['fields']['m_ParsedForm']['m_Name'] == 'Standard'
    assert by_id['10754']['type'] == 'Material'
    sprite_shader_id = str(by_id['10754']['fields']['m_Shader']['m_PathID'])
    assert by_id[sprite_shader_id]['type'] == 'Shader'
    shader_sources = []
    with zipfile.ZipFile(cache / 'builtin_shaders-2017.2.0f3.zip') as archive:
        # Include the official Standard shader and every matching-version include
        # so downstream translation does not silently mix Unity releases.
        for name in sorted(archive.namelist()):
            if not (name == 'DefaultResourcesExtra/Standard.shader' or
                    name.startswith('CGIncludes/') and not name.endswith('/')):
                continue
            raw = archive.read(name)
            entry = {'path': name, 'sha256': digest(raw), 'bytes': len(raw)}
            try:
                entry.update(sourceEncoding='utf-8-sig', source=raw.decode('utf-8-sig'))
            except UnicodeDecodeError:
                entry.update(sourceEncoding='base64',
                             source=base64.b64encode(raw).decode())
            shader_sources.append(entry)
    return {
        'schemaVersion': 1, 'unityVersion': '2017.2.0f3', 'revision': '46dda1414e51',
        'provenance': [{'url': BASE + path, 'sha256': sha,
                        'bytes': (cache / name).stat().st_size}
                       for name, (path, sha) in INPUTS.items()],
        'parser': 'UnityPy==1.25.4', 'resources': files, 'records': records,
        'shaderSources': shader_sources,
        'playerResources': player_resource_records,
        'scope': {
            'recordRecovery': 'verified from official exact-release installer',
            'material10303': 'Default-Material; shader pointer resolves to Standard (46)',
            'material10754': by_id['10754']['fields']['m_Name'] + '; shader pointer resolves to source Shader ' + sprite_shader_id,
            'playerResource': 'Exact win32_nondevelopment_mono payload extracted; '
                              'its serialized header is 2017.2.0b11, target 5. '
                              'Mesh 10206 is Cylinder. Keep this record separate from '
                              'the 2017.2.0f3 Editor records; do not relabel its header.',
            'correction': 'The required 10303/46 source records are in Editor/Data/'
                          'Resources/unity_builtin_extra. Searching only unity default '
                          'resources cannot establish that these records are unavailable.',
            'runtimeMappingIntegrated': True,
            'originalGamePlayerRecordIdentityVerified': False,
            'browserRenderingParityVerified': False,
        },
    }


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--cache', type=Path, required=True)
    parser.add_argument('--extractor', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--download', action='store_true')
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    args.cache.mkdir(parents=True, exist_ok=True)
    if args.download:
        for name, (path, _) in INPUTS.items():
            if not (args.cache / name).is_file():
                urllib.request.urlretrieve(BASE + path, args.cache / name)
    pack = recover(args.cache, args.extractor)
    rendered = json.dumps(pack, sort_keys=True, indent=2, ensure_ascii=False) + '\n'
    if args.check:
        if json.loads(args.output.read_text()) != pack:
            raise ValueError('Resource record pack differs from official pinned inputs')
    else:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(rendered)
    print(json.dumps({'status': 'pass', 'records': len(pack['records']),
                      'shaderSources': len(pack['shaderSources']),
                      'targetResourceRecovered': True,
                      'material': '10303 Default-Material', 'shader': '46 Standard',
                      'packSha256': digest(rendered.encode())}, sort_keys=True))


if __name__ == '__main__':
    main()
