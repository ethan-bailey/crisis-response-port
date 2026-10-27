import gzip
import hashlib
import importlib.util
import json
import tempfile
import unittest
from pathlib import Path


MODULE_PATH = Path(__file__).with_name("inventory_profile_audio_sources.py")
SPEC = importlib.util.spec_from_file_location("inventory_profile_audio_sources", MODULE_PATH)
INVENTORY = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(INVENTORY)


class ProfileAudioSourceInventoryTests(unittest.TestCase):
    def create_profile(self, root, source_text):
        profile = root / "profile"
        assets = profile / "assets"
        assets.mkdir(parents=True)
        descriptor = {
            "path": "Assets/Scenes/Mission.unity",
            "sha256": hashlib.sha256(source_text.encode()).hexdigest(),
            "sourceText": source_text,
        }
        with gzip.open(assets / "scene.json.gz", "wt", encoding="utf-8") as stream:
            json.dump(descriptor, stream)
        (profile / "manifest.json").write_text(json.dumps({
            "profile": "fixture", "sourcePackageSha256": "a" * 64,
            "assetCount": 1, "assets": [{"path": descriptor["path"]}],
        }))
        with gzip.open(profile / "metadata.json.gz", "wt", encoding="utf-8") as stream:
            json.dump({"guidPaths": {"a" * 32: "Assets/Scripts/Movement.cs"}}, stream)
        return profile

    def test_inventory_records_missing_spatial_blend_and_source_fields(self):
        source = """--- !u!1 &1
GameObject:
  m_Name: Gun
--- !u!82 &82
AudioSource:
  m_GameObject: {fileID: 1}
  m_Enabled: 1
  DopplerLevel: 0
  MinDistance: 1
  MaxDistance: 40
  Pan2D: 0
  rolloffMode: 1
"""
        with tempfile.TemporaryDirectory() as temp:
            result = INVENTORY.profile_inventory(self.create_profile(Path(temp), source))
        self.assertEqual(result["serializedAudioSourceCount"], 1)
        self.assertEqual(result["spatialBlendSerializedCount"], 0)
        component = result["components"][0]
        self.assertEqual(component["gameObjectName"], "Gun")
        self.assertIsNone(component["fields"]["SpatialBlend"])
        self.assertEqual(component["fields"]["DopplerLevel"], "0")
        self.assertEqual(result["valueHistograms"]["rolloffMode"], {"1": 1})

    def test_inventory_rejects_source_text_hash_mismatch(self):
        source = "--- !u!82 &82\nAudioSource:\n"
        with tempfile.TemporaryDirectory() as temp:
            profile = self.create_profile(Path(temp), source)
            asset = profile / "assets/scene.json.gz"
            with gzip.open(asset, "rt", encoding="utf-8") as stream:
                descriptor = json.load(stream)
            descriptor["sha256"] = "0" * 64
            with gzip.open(asset, "wt", encoding="utf-8") as stream:
                json.dump(descriptor, stream)
            with self.assertRaisesRegex(ValueError, "source text hash mismatch"):
                INVENTORY.profile_inventory(profile)

    def test_inventory_resolves_audio_source_parent_component_chain(self):
        source = """--- !u!1 &1
GameObject:
  m_Name: Weapon
  m_IsActive: 1
--- !u!4 &2
Transform:
  m_GameObject: {fileID: 1}
  m_Father: {fileID: 0}
--- !u!54 &3
Rigidbody:
  m_GameObject: {fileID: 1}
--- !u!114 &7
MonoBehaviour:
  m_GameObject: {fileID: 1}
  m_Script: {fileID: 11500000, guid: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa, type: 3}
--- !u!1 &4
GameObject:
  m_Name: Muzzle
  m_IsActive: 1
--- !u!4 &5
Transform:
  m_GameObject: {fileID: 4}
  m_Father: {fileID: 2}
--- !u!82 &6
AudioSource:
  m_GameObject: {fileID: 4}
  DopplerLevel: 1
--- !u!81 &8
AudioListener:
  m_GameObject: {fileID: 4}
  m_Enabled: 1
"""
        with tempfile.TemporaryDirectory() as temp:
            result = INVENTORY.profile_inventory(self.create_profile(Path(temp), source))
        self.assertEqual(result["schemaVersion"], 2)
        chain = result["transformHierarchyByAsset"]["Assets/Scenes/Mission.unity"][0]["chain"]
        self.assertEqual([item["gameObjectName"] for item in chain], ["Muzzle", "Weapon"])
        self.assertIn("Rigidbody", chain[1]["componentTypes"])
        self.assertEqual(chain[1]["monoBehaviourScriptPaths"], ["Assets/Scripts/Movement.cs"])
        summary = result["hierarchySummary"]
        self.assertEqual(summary["audioSourceCount"], 1)
        self.assertEqual(summary["transformFoundCount"], 1)
        self.assertEqual(summary["sourceOwnerRigidbodyCount"], 0)
        self.assertEqual(summary["sourceOwnerAndAncestorRigidbodyIncidences"], 1)
        self.assertEqual(summary["audioSourcesWithOwnerRigidbody"], 0)
        self.assertEqual(summary["audioSourcesWithOwnerOrAncestorRigidbody"], 1)
        listeners = result["listenerHierarchySummary"]
        self.assertEqual(listeners["listenerCount"], 1)
        self.assertEqual(listeners["enabledCount"], 1)
        self.assertEqual(listeners["activeInHierarchyCount"], 1)


if __name__ == "__main__":
    unittest.main()
