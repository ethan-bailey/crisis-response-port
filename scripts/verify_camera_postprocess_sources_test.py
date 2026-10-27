import json
import importlib.util
import tempfile
import unittest
from pathlib import Path

MODULE_PATH = Path(__file__).with_name("verify_camera_postprocess_sources.py")
SPEC = importlib.util.spec_from_file_location("verify_camera_postprocess_sources", MODULE_PATH)
verifier = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(verifier)


class CameraPostprocessSourceVerifierTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name) / "repo"
        self.recovery = Path(self.temp.name) / "recovery"
        self.manifest_path = self.root / "runtime/Original/FullSource/dependency-source-provenance.json"
        self.manifest_path.parent.mkdir(parents=True)
        (self.recovery / "recovery-provenance.json").parent.mkdir(parents=True, exist_ok=True)
        (self.recovery / "recovery-provenance.json").write_text(
            json.dumps({"artifactSha256": verifier.PIN}), encoding="utf-8"
        )
        recovered = {}
        for index, (name, archive_entry) in enumerate(verifier.SOURCE_ENTRIES.items()):
            data = f"// exact recovered source {index}: {name}\n".encode()
            original = self.recovery / archive_entry
            original.parent.mkdir(parents=True, exist_ok=True)
            original.write_bytes(data)
            runtime_path = f"runtime/Reference/RecoveredCameraPostprocess/{name}"
            runtime = self.root / runtime_path
            runtime.parent.mkdir(parents=True, exist_ok=True)
            runtime.write_bytes(data)
            digest = verifier.sha(data)
            recovered[name] = {
                "archiveEntry": archive_entry,
                "runtimePath": runtime_path,
                "sha256": digest,
                "runtimeSha256": digest,
                "adaptation": "exact recovered source preserved as noncompiled reference",
            }
        self.manifest_path.write_text(
            json.dumps({"archiveSha256": verifier.PIN, "recoveredReferences": recovered}),
            encoding="utf-8",
        )

    def tearDown(self):
        self.temp.cleanup()

    def test_accepts_exact_recovery_artifact_and_runtime_references(self):
        verifier.verify(self.recovery, self.root)

    def test_rejects_runtime_source_substitution(self):
        runtime = self.root / "runtime/Reference/RecoveredCameraPostprocess/ExteriorCamera.cs"
        runtime.write_text("// substituted\n", encoding="utf-8")
        with self.assertRaisesRegex(ValueError, "Recovered and reference source differs: ExteriorCamera.cs"):
            verifier.verify(self.recovery, self.root)

    def test_rejects_unpinned_recovery_artifact(self):
        (self.recovery / "recovery-provenance.json").write_text(
            json.dumps({"artifactSha256": "0" * 64}), encoding="utf-8"
        )
        with self.assertRaisesRegex(ValueError, "incorrect archive provenance"):
            verifier.verify(self.recovery, self.root)


if __name__ == "__main__":
    unittest.main()
