"""Failure-oriented packaging tests. Fixture bytes are not game build evidence."""
import base64
import hashlib
import gzip
import json
import subprocess
import struct
import tempfile
import unittest
from pathlib import Path

from package_wasm_site import MANIFEST, WASM_HEADER, package, split_scene_package, verify


class PackageTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.repo = self.root / "repo"
        self.pub = self.root / "publish"
        self.site = self.root / "site"
        self.repo.mkdir()
        for path, value in {
            "game/index.html": '<script type="importmap">{"imports":{"three":"https://example.test/pinned.js"}}</script><script type="module">import { loadOriginalScenePackage } from "../scene-data/scene-package-loader.mjs";</script>',
            "game/browser-input.mjs": "// input adapter\n",
            "game/assets/mission01.obj": "\n".join([
                "v 0 0 0", "v 1 0 0", "v 0 1 0",
                "vt 0 0", "vt 1 0", "vt 0 1", "vn 0 0 1",
                "f 1/1/1 2/2/1 3/3/1", "",
            ]),
            "game/assets/casing-556.json": json.dumps({
                "format": "original-unity-casing-mesh-v1",
                "collider": {"massKg": 0.002},
                "count": {"vertices": 3, "triangles": 1},
                "source": {"hashes": {}},
            }),
            "game/assets/casing-556-normal.png": b"test normal texture",
            "game/assets/casing-556-occlusion.png": b"test occlusion texture",
            "game/original-casing-runtime.mjs": "// test fixture casing bridge\n",
            "game/vendor/physx-js-webidl.mjs": "// pinned PhysX loader",
            "game/vendor/physx-js-webidl.wasm": WASM_HEADER + b"physx-fixture",
            "game/package-lock.json": "{}",
            "runtime/Original/Example.cs": "// recovered source fixture",
            "tools/wasm-compile/global.json": '{"sdk":{"version":"8.0.426","rollForward":"disable","allowPrerelease":false}}',
            "scripts/package_incremental_source.py": Path(__file__).with_name("package_incremental_source.py").read_text(),
        }.items():
            self.write(self.repo / path, value)
        for path, value in {
            "index.html": '<base href="/"><script type="module">import { loadOriginalScenePackage } from "../scene-data/scene-package-loader.mjs";</script>',
            "index.html.br": b"stale root index compression",
            "index.html.gz": b"stale root index compression",
            "browser-input.mjs": "// input adapter\n",
            "_framework/blazor.webassembly.js": "// compiled loader fixture",
            "_framework/dotnet.native.wasm": WASM_HEADER + b"native-fixture",
            "_framework/Crisis.Runtime.Browser.wasm": WASM_HEADER + b"browser-fixture",
            "_framework/Crisis.Runtime.wasm": WASM_HEADER + b"runtime-fixture",
        }.items():
            self.write(self.pub / path, value)
        boot = {"resources": {"hash": "sha256-aggregate-is-not-a-file",
                              "assembly": {}, "runtime": {}}}
        for name in ("Crisis.Runtime.Browser.wasm", "Crisis.Runtime.wasm", "dotnet.native.wasm"):
            value = (self.pub / "_framework" / name).read_bytes()
            boot["resources"]["assembly"][name] = "sha256-" + base64.b64encode(hashlib.sha256(value).digest()).decode()
        self.write(self.pub / "_framework/blazor.boot.json", json.dumps(boot))
        self.write(self.repo / "scene-data" / "fixture.json", "{}")
        self.write(self.repo / "scene-data" / "crisis-response-scenes.json.gz",
                   gzip.compress('{"scene":"fixture-☃"}'.encode(), mtime=0))
        self.write(self.repo / "scene-data" / "scene-package-loader.mjs",
                   "export async function loadOriginalScenePackage() {}\n")
        self.git("init", "-q")
        self.git("add", ".")
        self.git("-c", "user.name=Fixture", "-c", "user.email=fixture@example.test", "commit", "-qm", "Fixture")
        self.write(self.repo / "global.json", (self.repo / "tools/wasm-compile/global.json").read_bytes())
        self.write(self.repo / "scripts/build_scene_binary.py",
                   Path(__file__).with_name("build_scene_binary.py").read_text())
        self.git("add", ".")
        self.git("-c", "user.name=Fixture", "-c", "user.email=fixture@example.test", "commit", "--amend", "-qm", "Fixture")

    def write(self, path, value):
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(value.encode() if isinstance(value, str) else value)

    def git(self, *args):
        return subprocess.check_output(["git", "-C", str(self.repo), *args], text=True)

    def build(self, **kwargs):
        return package(self.repo, self.pub, self.site, {"dotnet": "8.0.426"}, "", **kwargs)

    def test_complete_package_is_verifiable_and_records_scope(self):
        result = self.build(include_original_scene_smoke=True)
        self.assertEqual(result["wasmFiles"], 4)
        self.assertEqual(result["blazorResources"], 3)
        self.assertTrue(result["sceneSmokeIncluded"])
        self.assertFalse((self.site / "runtime-web/index.html.br").exists())
        self.assertFalse((self.site / "runtime-web/index.html.gz").exists())
        self.assertFalse((self.site / "game/package-lock.json").exists())
        manifest = json.loads((self.site / MANIFEST).read_text())
        self.assertIn("scene-data/fixture.json", [entry["path"] for entry in manifest["files"]])
        scene_manifest = json.loads((self.site / "scene-data/crisis-response-scenes.manifest.json").read_text())
        self.assertEqual(scene_manifest["encoding"], "independent-gzip-chunks")
        scene_bytes = b"".join(gzip.decompress((self.site / "scene-data" / chunk["path"]).read_bytes())
                                 for chunk in scene_manifest["chunks"])
        self.assertEqual(scene_bytes.decode(), '{"scene":"fixture-☃"}')
        self.assertFalse((self.site / "scene-data/crisis-response-scenes.json.gz").exists())
        self.assertIn("scene-data/scene-package-loader.mjs", [entry["path"] for entry in manifest["files"]])
        self.assertFalse(manifest["scope"]["physicalIPhoneTested"])
        self.assertFalse(manifest["scope"]["browserTestedThisBuild"])
        self.assertEqual(manifest["externalImports"]["three"], "https://example.test/pinned.js")
        self.assertIn("runtime/Original/Example.cs", [r["path"] for r in manifest["sourceInventory"]])
        self.assertIn("game/assets/mission01.obj", [r["path"] for r in manifest["sourceInventory"]])
        scene = (self.site / "game/assets/mission01.crg").read_bytes()
        magic, vertices, indices, attributes = struct.unpack("<4sIII", scene[:16])
        self.assertEqual(magic, b"CRG1")
        self.assertEqual(attributes, 3)
        self.assertEqual((vertices, indices), (3, 3))
        self.assertEqual(len(scene), 16 + vertices * 32 + indices * 4)
        report = json.loads((self.site / "game/assets/mission01.report.json").read_text())
        self.assertEqual(report["sourceSha256"], hashlib.sha256((self.repo / "game/assets/mission01.obj").read_bytes()).hexdigest())
        self.assertEqual(report["binarySha256"], hashlib.sha256(scene).hexdigest())
        self.assertEqual(verify(self.site), result)

    def test_default_package_omits_the_large_diagnostic_scene_archive(self):
        result = self.build()
        self.assertFalse(result["sceneSmokeIncluded"])
        self.assertFalse((self.site / "scene-data/crisis-response-scenes.manifest.json").exists())
        self.assertEqual(list((self.site / "scene-data").glob("crisis-response-scenes-*.json.gz")), [])
        manifest = json.loads((self.site / MANIFEST).read_text())
        self.assertFalse(manifest["scope"]["originalSceneSmokeIncluded"])
        self.assertEqual(verify(self.site), result)

    def test_scene_split_keeps_utf8_boundaries_as_bytes(self):
        scene_data = self.root / "split-scene-data"
        scene_data.mkdir()
        source = '{"scene":"snowman ☃ and a long continuation"}'.encode()
        (scene_data / "crisis-response-scenes.json.gz").write_bytes(gzip.compress(source, mtime=0))
        manifest = split_scene_package(scene_data, chunk_bytes=7)
        rebuilt = b"".join(gzip.decompress((scene_data / chunk["path"]).read_bytes())
                           for chunk in manifest["chunks"])
        self.assertGreater(len(manifest["chunks"]), 1)
        self.assertEqual(rebuilt, source)
        self.assertEqual(manifest["uncompressedBytes"], len(source))

    def test_incremental_profile_is_packaged_and_verified_per_asset(self):
        root = "Assets/Resources/fixture.prefab"
        asset = {"records": [{"classID": 1, "fileID": "1", "className": "GameObject",
                              "fields": {"m_Name": "fixture"}}]}
        source = {"format": "fixture", "assets": {root: asset}, "guidPaths": {},
                  "objectReferences": [], "sourceOfTruth": {"fixture": True}}
        source_path = self.root / "full-source.json.gz"
        source_path.write_bytes(gzip.compress(json.dumps(source).encode(), mtime=0))
        result = self.build(incremental_source_package=source_path,
                            incremental_profile="fixture", incremental_roots=[root])
        profile = json.loads((self.site / "scene-data/fullsource/fixture/manifest.json").read_text())
        self.assertEqual(profile["assetCount"], 1)
        self.assertEqual(result["incrementalSourceProfiles"], [{
            "profile": "fixture", "assetCount": 1,
            "compressedBytes": profile["compressedAssetBytes"],
            "sourceBytes": profile["uncompressedAssetBytes"],
        }])
        self.assertTrue((self.site / "scene-data/fullsource/fixture/assets" / profile["assets"][0]["file"].split("/")[-1]).is_file())

    def test_incremental_source_reports_all_missing_roots_before_closure(self):
        roots = ["Assets/Resources/available.prefab", "Assets/Resources/missing-one.prefab",
                 "Assets/Resources/missing-two.prefab"]
        source = {"format": "fixture", "assets": {roots[0]: {"records": []}},
                  "guidPaths": {}, "objectReferences": []}
        source_path = self.root / "incomplete-source.json"
        source_path.write_text(json.dumps(source))
        result = subprocess.run(["python3", str(self.repo / "scripts/package_incremental_source.py"),
                                 str(source_path), str(self.root / "out"),
                                 *[arg for root in roots for arg in ("--root", root)]],
                                cwd=self.repo, capture_output=True, text=True)
        self.assertNotEqual(result.returncode, 0)
        self.assertIn("Original source roots are missing (2)", result.stderr)
        self.assertIn(roots[1], result.stderr)
        self.assertIn(roots[2], result.stderr)

    def test_prebuilt_incremental_profile_is_reused_when_raw_source_package_is_unavailable(self):
        root = "Assets/Resources/fixture.prefab"
        source = {"format": "fixture", "assets": {root: {"records": [{
            "classID": 1, "fileID": "1", "className": "GameObject", "fields": {"m_Name": "fixture"}}]}},
            "guidPaths": {}, "objectReferences": [], "sourceOfTruth": {"fixture": True}}
        source_path = self.root / "full-source.json.gz"
        source_path.write_bytes(gzip.compress(json.dumps(source).encode(), mtime=0))
        profile_path = self.root / "prebuilt-profile"
        subprocess.run(["python3", str(self.repo / "scripts/package_incremental_source.py"),
                        str(source_path), str(profile_path), "--profile", "fixture", "--root", root],
                       check=True, cwd=self.repo, capture_output=True, text=True)
        result = self.build(incremental_profile="fixture", incremental_profile_package=profile_path)
        packaged = self.site / "scene-data/fullsource/fixture"
        self.assertEqual(result["incrementalSourceProfiles"][0]["assetCount"], 1)
        self.assertEqual((packaged / "manifest.json").read_bytes(), (profile_path / "manifest.json").read_bytes())
        self.assertEqual(len(list(packaged.glob("assets/*.json.gz"))), 1)

    def test_default_incremental_profile_matches_mission_page_and_includes_scene_root(self):
        roots = [
            "Assets/Resources/Global context.prefab",
            "Assets/GameObject/Interior mode UI.prefab",
            "Assets/Resources/ui/Interaction menu.prefab",
            "Assets/Resources/characters/Operator.prefab",
            "Assets/Resources/Version.prefab",
            "Assets/Scenes/Mission 01.unity",
        ]
        source = {"format": "fixture", "assets": {
            root: {"records": [{"classID": 1, "fileID": str(index), "className": "GameObject",
                                "fields": {"m_Name": root}}]}
            for index, root in enumerate(roots, 1)
        }, "guidPaths": {}, "objectReferences": [], "sourceOfTruth": {"fixture": True}}
        source_path = self.root / "mission-source.json.gz"
        source_path.write_bytes(gzip.compress(json.dumps(source).encode(), mtime=0))

        self.build(incremental_source_package=source_path)

        profile_path = self.site / "scene-data/fullsource/global-ui-mission-operator/manifest.json"
        self.assertTrue(profile_path.is_file())
        profile = json.loads(profile_path.read_text())
        self.assertEqual(profile["profile"], "global-ui-mission-operator")
        self.assertEqual(profile["rootAssets"], roots)
        self.assertIn("Assets/Scenes/Mission 01.unity", profile["rootAssets"])

    def test_modified_binary_fails(self):
        self.build()
        (self.site / "game/vendor/physx-js-webidl.wasm").write_bytes(b"corrupt")
        with self.assertRaisesRegex(ValueError, "checksum mismatch"):
            verify(self.site)

    def test_unlisted_and_missing_assets_fail(self):
        self.build()
        extra = self.site / "game/old-build.mjs"
        extra.write_text("mixed build")
        with self.assertRaisesRegex(ValueError, "Unlisted"):
            verify(self.site)
        extra.unlink()
        (self.site / "game/assets/mission01.crg").unlink()
        with self.assertRaisesRegex(ValueError, "Missing package"):
            verify(self.site)

    def test_stale_boot_hash_fails_build(self):
        (self.pub / "_framework/Crisis.Runtime.wasm").write_bytes(WASM_HEADER + b"wrong-assembly")
        with self.assertRaisesRegex(ValueError, "Blazor resource hash mismatch"):
            self.build()

    def test_wrong_input_adapter_fails_build(self):
        (self.pub / "browser-input.mjs").write_text("stale adapter")
        with self.assertRaisesRegex(ValueError, "different input adapters"):
            self.build()

    def test_missing_and_fake_wasm_fail(self):
        wasm = self.pub / "_framework/dotnet.native.wasm"
        wasm.write_text("an HTML error is not WASM")
        with self.assertRaisesRegex(ValueError, "Invalid WebAssembly"):
            self.build()

    def test_dirty_gameplay_source_fails(self):
        (self.repo / "runtime/Original/Example.cs").write_text("unexpected change")
        with self.assertRaisesRegex(ValueError, "differs from input commit"):
            self.build()

    def test_excluded_node_modules_symlinks_do_not_block_site_package(self):
        dependency_bin = self.repo / "game/node_modules/.bin"
        dependency_bin.mkdir(parents=True)
        (dependency_bin / "playwright").symlink_to(self.root / "external-playwright")
        result = self.build()
        self.assertEqual(result["status"], "verified")
        self.assertFalse((self.site / "game/node_modules").exists())

    def test_unexcluded_game_symlink_still_fails_package_input_scan(self):
        (self.repo / "game/escape").symlink_to(self.root)
        with self.assertRaisesRegex(ValueError, "Symlink is not a package input"):
            self.build()

    def test_symlink_cannot_escape_manifest(self):
        self.build()
        (self.site / "game/escape").symlink_to(self.repo)
        with self.assertRaisesRegex(ValueError, "Symlink"):
            verify(self.site)

    def test_manifest_path_escape_fails(self):
        self.build()
        path = self.site / MANIFEST
        manifest = json.loads(path.read_text())
        manifest["files"][0]["path"] = "../repo/game/index.html"
        path.write_text(json.dumps(manifest))
        with self.assertRaisesRegex(ValueError, "Unsafe manifest path"):
            verify(self.site)

    def test_output_cannot_overwrite_existing_files(self):
        self.site.mkdir()
        with self.assertRaisesRegex(ValueError, "refusing to overwrite"):
            self.build()

    def test_newer_preinstalled_sdk_cannot_silently_compile(self):
        with self.assertRaisesRegex(ValueError, "Selected .NET SDK differs"):
            package(self.repo, self.pub, self.site, {"dotnet": "10.0.401"}, "")

    def test_untracked_sdk_selection_must_match_pin(self):
        (self.repo / "global.json").write_text('{"sdk":{"version":"10.0.401"}}')
        with self.assertRaisesRegex(ValueError, "SDK selection differs"):
            self.build()


if __name__ == "__main__":
    unittest.main()
