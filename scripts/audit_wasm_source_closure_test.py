"""Synthetic asset-reference fixtures; not game or Unity execution evidence."""
import hashlib
import json
import tempfile
import unittest
from pathlib import Path

from audit_wasm_source_closure import audit, markdown, references

PREFAB = "1" * 32
SCRIPT = "2" * 32
MISSING_ASSET = "3" * 32
MISSING_META = "4" * 32
SHADER = "5" * 32
OPAQUE = "6" * 32
BUILTIN = "0000000000000000f000000000000000"
UI = "f5f67c52d1564df4a8936ccd202a3bd8"


class SourceClosureTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.project, self.repo = self.root / "original", self.root / "port"
        self.scene("First", [PREFAB, MISSING_ASSET, MISSING_META, BUILTIN, UI, OPAQUE])
        self.scene("Second", [PREFAB])
        self.asset("Assets/Example.prefab", PREFAB,
                   "%YAML 1.1\n--- !u!114 &1 stripped\n" + self.ref(PREFAB) + self.ref(SCRIPT) + self.ref(SHADER))
        self.asset("Assets/Scripts/Assembly-CSharp/Example.cs", SCRIPT,
                   "using System.IO;\nclass Example { void Load() { Resources.Load(\"original\"); } }\n")
        self.asset("Assets/Shader/Original.shader", SHADER, "//DummyShaderTextExporter\n")
        self.asset("Assets/Opaque.asset", OPAQUE, b"\x00\xffbinary")
        self.write(self.project / "Assets/Absent.asset.meta", "guid: " + MISSING_ASSET)
        self.write(self.repo / "runtime/Original/Example.cs", "// selected source fixture\n")
        self.write(self.repo / "runtime/Engine/Adapter.cs", "// adapter fixture\n")
        self.write(self.repo / "runtime/Crisis.Runtime.csproj", '<Project><ItemGroup><Compile Include="Original/**/*.cs" /></ItemGroup></Project>')
        original = self.project / "Assets/Scripts/Assembly-CSharp/Example.cs"
        self.write(self.repo / "runtime/Original/source-provenance.json", json.dumps({
            "originalFiles": {"Example.cs": hashlib.sha256(original.read_bytes()).hexdigest()}}))

    def write(self, path, data):
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(data.encode() if isinstance(data, str) else data)

    def ref(self, guid, file_id="1"):
        return "ref: {fileID: " + file_id + ", guid: " + guid + ", type: 3}\n"

    def asset(self, name, guid, data):
        self.write(self.project / name, data)
        self.write(self.project / (name + ".meta"), "fileFormatVersion: 2\nguid: " + guid + "\n")

    def scene(self, name, guids):
        self.write(self.project / f"Assets/Scenes/{name}.unity",
                   "%YAML 1.1\n--- !u!1 &1\n" + "".join(self.ref(guid) for guid in guids))

    def test_all_scenes_cycle_and_source_inventory_are_read_only(self):
        before = {p: p.read_bytes() for p in self.project.rglob("*") if p.is_file()}
        result = audit(self.project, self.repo)
        self.assertEqual(result["summary"]["scenes"], 2)
        self.assertEqual(result["summary"]["recoveredCSharpFiles"], 1)
        self.assertEqual(result["summary"]["selectedOriginalCompileFiles"], 1)
        self.assertEqual(result["summary"]["sourceHashFailures"], 0)
        self.assertEqual(len(result["scenes"][1]["reachableAssets"]), 4)
        self.assertEqual(result["assets"]["Assets/Example.prefab"]["strippedRecords"], 1)
        self.assertEqual(before, {p: p.read_bytes() for p in before})
        self.assertFalse(result["scope"]["cSharpCompiledByThisAudit"])
        self.assertFalse(result["scope"]["fullGameClosureEstablished"])
        self.assertFalse(result["compileProject"]["effectiveMSBuildItemsResolved"])
        self.assertIn("not a completion percentage", markdown(result))

    def test_meta_without_asset_and_unresolved_ui_are_not_hidden(self):
        scene = audit(self.project, self.repo)["scenes"][0]
        status = {r["guid"]: r["status"] for r in scene["unresolvedReferences"]}
        self.assertEqual(status[MISSING_ASSET], "missingAsset")
        self.assertEqual(status[MISSING_META], "missingMeta")
        self.assertEqual(status[UI], "missingMeta")
        self.assertEqual(scene["unityBuiltinReferences"][0]["guid"], BUILTIN)

    def test_dummy_shaders_and_binary_graphs_stay_explicit(self):
        scene = audit(self.project, self.repo)["scenes"][0]
        self.assertEqual(scene["opaqueSerializedAssets"], ["Assets/Opaque.asset"])
        self.assertEqual(scene["dummyShaders"], ["Assets/Shader/Original.shader"])

    def test_duplicate_guid_cannot_choose_arbitrary_asset(self):
        self.asset("Assets/Duplicate.prefab", PREFAB, "%YAML 1.1\n")
        result = audit(self.project, self.repo)
        self.assertEqual(result["summary"]["ambiguousGuidCount"], 1)
        self.assertTrue(any(r["status"] == "ambiguousGuid" for r in result["scenes"][0]["unresolvedReferences"]))
        self.assertNotIn("Assets/Example.prefab", result["scenes"][0]["reachableAssets"])

    def test_original_hash_change_cannot_be_claimed_verified(self):
        original = self.project / "Assets/Scripts/Assembly-CSharp/Example.cs"
        original.write_text("changed source")
        result = audit(self.project, self.repo)
        self.assertEqual(result["summary"]["sourceHashFailures"], 1)
        self.assertEqual(result["sourceHashClaims"][0]["status"], "sourceHashMismatch")

    def test_manifest_cannot_read_outside_recovered_project(self):
        self.write(self.repo / "game/assets/ballistics.json.gz", __import__("gzip").compress(
            json.dumps({"sourceHashes": {"../secret.cs": "0" * 64}}).encode()))
        with self.assertRaisesRegex(ValueError, "Unsafe source path"):
            audit(self.project, self.repo)

    def test_reference_order_case_and_counts_are_preserved(self):
        guid = "abcdefabcdefabcdefabcdefabcdefab"
        text = "{guid: " + guid.upper() + ", fileID: -3, type: 3}\n"
        result = references(text + text + "{fileID: 0}\n" + text.replace(", fileID", ",\n fileID"))
        self.assertEqual(result, [{"guid": guid, "fileID": "-3", "occurrences": 3}])

    def test_utf8_bom_does_not_hide_serialized_references(self):
        self.write(self.project / "Assets/Scenes/Second.unity", "\ufeff%YAML 1.1\n" + self.ref(SCRIPT))
        result = audit(self.project, self.repo)
        self.assertIn("Assets/Scripts/Assembly-CSharp/Example.cs", result["scenes"][1]["reachableAssets"])

    def test_lexical_candidates_do_not_assert_runtime_execution(self):
        result = audit(self.project, self.repo)
        self.assertEqual({r["kind"] for r in result["lexicalApiCandidates"]},
                         {"filesystemNamespace", "dynamicResources"})
        self.assertFalse(result["scope"]["dynamicDependenciesResolved"])


if __name__ == "__main__":
    unittest.main()
