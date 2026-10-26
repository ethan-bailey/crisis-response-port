import gzip
import json
import tempfile
import unittest
from pathlib import Path

from inventory_profile_audio import inventory


class ProfileAudioInventoryTests(unittest.TestCase):
    def make_profile(self, root: Path, *, has_reference=True):
        clip = {
            "path": "Assets/AudioClip/hurt-07.ogg",
            "bytes": 27302,
            "sha256": "a" * 64,
            "compressedBytes": 1024,
            "compressedSha256": "b" * 64,
        }
        manifest = {
            "profile": "fixture",
            "sourcePackageSha256": "c" * 64,
            "rootAssets": ["Assets/Scenes/Mission 01.unity"],
            "assets": [clip, {
                "path": "Assets/Resources/fixture.prefab",
                "bytes": 100,
                "sha256": "d" * 64,
                "compressedBytes": 90,
                "compressedSha256": "e" * 64,
            }],
        }
        metadata = {
            "guidPaths": {"audio-guid": clip["path"], "owner-guid": "Assets/Resources/fixture.prefab"},
            "assetManifest": {clip["path"]: {}, "Assets/Resources/fixture.prefab": {}},
            "objectReferences": ([{
                "source": "Assets/Resources/fixture.prefab",
                "sourceFileID": "10",
                "fieldPath": "audioClip",
                "guid": "audio-guid",
                "fileID": "8300000",
                "type": 2,
            }] if has_reference else []),
        }
        (root / "manifest.json").write_text(json.dumps(manifest), encoding="utf-8")
        with gzip.open(root / "metadata.json.gz", "wt", encoding="utf-8") as target:
            json.dump(metadata, target)

    def test_tracks_profile_identity_clip_hash_and_runtime_prefab_owner(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            self.make_profile(root)
            result = inventory(root)
        self.assertEqual(result["sourcePackageSha256"], "c" * 64)
        self.assertEqual(result["formatCounts"], {"ogg": 1})
        self.assertEqual(result["serializedReferenceCount"], 1)
        entry = result["assets"][0]
        self.assertEqual(entry["sourceSha256"], "a" * 64)
        self.assertEqual(entry["references"][0]["fieldPath"], "audioClip")
        self.assertTrue(entry["references"][0]["ownerMayBeInstantiatedAfterMissionStart"])

    def test_rejects_clip_without_serialized_owner(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            self.make_profile(root, has_reference=False)
            with self.assertRaisesRegex(ValueError, "no serialized owner"):
                inventory(root)

    def test_rejects_reference_outside_profile(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            self.make_profile(root)
            metadata_path = root / "metadata.json.gz"
            with gzip.open(metadata_path, "rt", encoding="utf-8") as source:
                metadata = json.load(source)
            metadata["objectReferences"][0]["source"] = "Assets/NotIncluded.prefab"
            with gzip.open(metadata_path, "wt", encoding="utf-8") as target:
                json.dump(metadata, target)
            with self.assertRaisesRegex(ValueError, "outside the selected profile"):
                inventory(root)


if __name__ == "__main__":
    unittest.main()
