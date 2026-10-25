import hashlib
import json
import subprocess
import tempfile
import unittest
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


class OriginalCasingSourceTests(unittest.TestCase):
    def test_mesh_prefab_material_texture_and_collider_closure(self):
        with tempfile.TemporaryDirectory() as directory:
            target = Path(directory) / "casing-556.json"
            result = subprocess.run(["python3", str(ROOT / "scripts/extract_original_casing_mesh.py"), str(target)],
                                    cwd=ROOT, capture_output=True, text=True, check=True)
            report = json.loads(result.stdout)
            source = json.loads(target.read_text())
            self.assertEqual(source["format"], "original-unity-casing-mesh-v1")
            self.assertEqual(source["source"]["meshGuid"], "d495eb21fe3efb34eb05d4f2f79b43c5")
            self.assertEqual(source["source"]["materialGuid"], "f2f4f981cd6ffb54d8f381d4a33915a8")
            self.assertEqual(source["count"]["vertices"], 54)
            self.assertEqual(source["count"]["triangles"], 52)
            self.assertEqual(len(source["mesh"]["normals"]), 54)
            self.assertEqual(len(source["mesh"]["uv"]), 54)
            self.assertEqual(source["collider"]["massKg"], 0.002)
            self.assertEqual(source["physicsMaterial"], {"staticFriction": 0.1, "dynamicFriction": 0.1,
                                                           "bounciness": 0.35, "combine": "Average"})
            self.assertEqual(report["vertices"], 54)
            for relative, expected in source["source"]["hashes"].items():
                actual = hashlib.sha256((ROOT / "source-reference/Original" / relative).read_bytes()).hexdigest()
                self.assertEqual(actual, expected, relative)
            self.assertEqual((target.parent / "casing-556-normal.png").stat().st_size, 758153)
            self.assertEqual((target.parent / "casing-556-occlusion.png").stat().st_size, 180659)


if __name__ == "__main__":
    unittest.main()
