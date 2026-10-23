#!/usr/bin/env python3
"""Regression tests for scene-source preflight and verified builtin geometry."""
import pathlib
import runpy
import subprocess
import sys
import tempfile
import unittest
import hashlib

ROOT = pathlib.Path(__file__).resolve().parents[1]
SCRIPT = ROOT / 'scripts' / 'extract_runtime_scene.py'
EXTRACTOR = runpy.run_path(str(SCRIPT))


class ExtractRuntimeSceneTests(unittest.TestCase):
    def test_unity_empty_material_tag_key_parses_without_changing_source(self):
        source = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  stringTagMap:
    :
    RenderType: TransparentCutout
"""
        with tempfile.TemporaryDirectory() as directory:
            path = pathlib.Path(directory) / 'empty-tag.mat'
            path.write_text(source, encoding='utf-8')
            asset = EXTRACTOR['parse_asset'](path, 'Assets/Material/empty-tag.mat')
        self.assertEqual(asset['sourceText'], source)
        self.assertEqual(asset['sha256'], hashlib.sha256(source.encode()).hexdigest())
        self.assertEqual(asset['records'][0]['fields']['stringTagMap'][''], None)
        self.assertEqual(asset['records'][0]['fields']['stringTagMap']['RenderType'], 'TransparentCutout')

    def test_packed_weapon_enum_list_remains_hex_without_changing_source(self):
        source = """%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &1140000
MonoBehaviour:
  fireModes: 030000000100000000000000
  functions: 01000000
"""
        with tempfile.TemporaryDirectory() as directory:
            path = pathlib.Path(directory) / 'weapon.prefab'
            path.write_text(source, encoding='utf-8')
            asset = EXTRACTOR['parse_asset'](path, 'Assets/Resources/weapons/M4A1.prefab')
        self.assertEqual(asset['sourceText'], source)
        self.assertEqual(asset['sha256'], hashlib.sha256(source.encode()).hexdigest())
        self.assertEqual(asset['records'][0]['fields']['fireModes'], '030000000100000000000000')
        self.assertEqual(asset['records'][0]['fields']['functions'], '01000000')

    def test_recovered_unity_ui_typedef_ids_are_mapped(self):
        types = EXTRACTOR['SCRIPT_GUIDS']['f5f67c52d1564df4a8936ccd202a3bd8']
        self.assertEqual(types['1741964061'], 'ContentSizeFitter')
        self.assertEqual(types['1392445389'], 'Button')
        self.assertEqual(types['853051423'], 'Dropdown')
        self.assertEqual(types['1367256648'], 'ScrollRect')
        self.assertEqual(types['-1200242548'], 'Mask')
        self.assertEqual(types['2109663825'], 'Toggle')
        self.assertEqual(types['-2061169968'], 'Scrollbar')

    def test_verified_mesh_10210_is_accepted(self):
        records = EXTRACTOR['load_builtin_resource_records']()['playerResourceRecords']['records']
        mesh = records['10210']
        geometry = mesh['runtimeGeometry']
        self.assertEqual(mesh['type'], 'Mesh')
        self.assertEqual(mesh['fileID'], '10210')
        self.assertEqual(len(geometry['vertices']), 4)
        self.assertEqual(len(geometry['triangles']), 6)

    def test_missing_original_scenes_report_actionable_error_without_output(self):
        with tempfile.TemporaryDirectory() as directory:
            output = pathlib.Path(directory) / 'scene.json'
            result = subprocess.run(
                [sys.executable, str(SCRIPT), directory, str(output)],
                cwd=ROOT, capture_output=True, text=True, check=False,
            )
            self.assertNotEqual(result.returncode, 0)
            self.assertIn('Required original Unity scene source file(s) are missing', result.stderr)
            self.assertIn('Assets/Scenes/Marksman range.unity', result.stderr)
            self.assertIn('Assets/Scenes/Mission 01.unity', result.stderr)
        self.assertIn('Assets/Scenes/Title.unity', result.stderr)
        self.assertIn('Assets/Scenes/Transition.unity', result.stderr)
        self.assertFalse(output.exists())

    def test_runtime_resource_seeds_include_original_items_loaded_by_string(self):
        with tempfile.TemporaryDirectory() as directory:
            item = pathlib.Path(directory) / 'Assets' / 'Resources' / 'items' / 'Primary magazine.prefab'
            item.parent.mkdir(parents=True)
            item.write_text('')
            weapon = pathlib.Path(directory) / 'Assets' / 'Resources' / 'weapons' / 'M4A1.prefab'
            weapon.parent.mkdir(parents=True)
            weapon.write_text('')
            version = pathlib.Path(directory) / 'Assets' / 'Resources' / 'Version.prefab'
            version.parent.mkdir(parents=True, exist_ok=True)
            version.write_text('')
            global_context = pathlib.Path(directory) / 'Assets' / 'Resources' / 'Global context.prefab'
            global_context.parent.mkdir(parents=True, exist_ok=True)
            global_context.write_text('')
            seeds = EXTRACTOR['runtime_asset_seeds'](directory)
        self.assertIn('Assets/Resources/items/Primary magazine.prefab', seeds)
        self.assertIn('Assets/Resources/weapons/M4A1.prefab', seeds)
        self.assertIn('Assets/Resources/Version.prefab', seeds)
        self.assertIn('Assets/Resources/Global context.prefab', seeds)


if __name__ == '__main__':
    unittest.main()
