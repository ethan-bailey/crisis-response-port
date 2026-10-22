#!/usr/bin/env python3
"""Regression tests for scene-source preflight and verified builtin geometry."""
import pathlib
import runpy
import subprocess
import sys
import tempfile
import unittest

ROOT = pathlib.Path(__file__).resolve().parents[1]
SCRIPT = ROOT / 'scripts' / 'extract_runtime_scene.py'
EXTRACTOR = runpy.run_path(str(SCRIPT))


class ExtractRuntimeSceneTests(unittest.TestCase):
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
            self.assertFalse(output.exists())


if __name__ == '__main__':
    unittest.main()
