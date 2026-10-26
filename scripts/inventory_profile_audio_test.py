import base64
import gzip
import hashlib
import json
import tempfile
import unittest
from pathlib import Path

from inventory_profile_audio import inventory


class ProfileAudioInventoryTests(unittest.TestCase):
    def make_profile(self, root: Path, *, has_reference=True):
        audio = b"fixture ogg bytes"
        audio_b64 = base64.b64encode(audio).decode("ascii")
        audio_sha = hashlib.sha256(audio).hexdigest()
        descriptor = json.dumps({
            "path": "Assets/AudioClip/hurt-07.ogg",
            "sha256": audio_sha,
            "size": len(audio),
            "binary": True,
            "dataBase64": audio_b64,
        }, separators=(",", ":")).encode()
        packed = gzip.compress(descriptor, mtime=0)
        (root / "clip.json.gz").write_bytes(packed)
        clip = {
            "path": "Assets/AudioClip/hurt-07.ogg",
            "file": "clip.json.gz",
            "bytes": len(descriptor),
            "sha256": hashlib.sha256(descriptor).hexdigest(),
            "compressedBytes": len(packed),
            "compressedSha256": hashlib.sha256(packed).hexdigest(),
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
        self.assertEqual(result["schemaVersion"], 2)
        self.assertEqual(result["sourcePackageSha256"], "c" * 64)
        self.assertEqual(result["formatCounts"], {"ogg": 1})
        self.assertEqual(result["serializedReferenceCount"], 1)
        self.assertEqual(result["sourceAudioBytes"], len(b"fixture ogg bytes"))
        self.assertEqual(result["descriptorJsonBytes"], result["assets"][0]["descriptorJsonBytes"])
        entry = result["assets"][0]
        self.assertEqual(entry["sourceAudioBytes"], len(b"fixture ogg bytes"))
        self.assertEqual(entry["sourceAudioSha256"], hashlib.sha256(b"fixture ogg bytes").hexdigest())
        self.assertEqual(entry["descriptorJsonBytes"], len(json.dumps({
            "path": "Assets/AudioClip/hurt-07.ogg",
            "sha256": hashlib.sha256(b"fixture ogg bytes").hexdigest(),
            "size": len(b"fixture ogg bytes"),
            "binary": True,
            "dataBase64": base64.b64encode(b"fixture ogg bytes").decode("ascii"),
        }, separators=(",", ":")).encode()))
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

    def test_rejects_embedded_audio_hash_mismatch(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            self.make_profile(root)
            descriptor_path = root / "clip.json.gz"
            with gzip.open(descriptor_path, "rt", encoding="utf-8") as source:
                descriptor = json.load(source)
            descriptor["sha256"] = "0" * 64
            raw = json.dumps(descriptor, separators=(",", ":")).encode()
            packed = gzip.compress(raw, mtime=0)
            descriptor_path.write_bytes(packed)
            manifest_path = root / "manifest.json"
            manifest = json.loads(manifest_path.read_text())
            manifest["assets"][0].update(
                bytes=len(raw), sha256=hashlib.sha256(raw).hexdigest(),
                compressedBytes=len(packed), compressedSha256=hashlib.sha256(packed).hexdigest())
            manifest_path.write_text(json.dumps(manifest))
            with self.assertRaisesRegex(ValueError, "Embedded audio byte identity"):
                inventory(root)


if __name__ == "__main__":
    unittest.main()
