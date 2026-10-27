import importlib.util
from pathlib import Path
import unittest

MODULE_PATH = Path(__file__).with_name("verify_foundation_sources.py")
SPEC = importlib.util.spec_from_file_location("verify_foundation_sources", MODULE_PATH)
VERIFY = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(VERIFY)


class SourceAdaptationTests(unittest.TestCase):

    def test_pinned_ilspy_reproduction_is_distinct_from_runtime_adaptation(self):
        delta = {
            "status": "reproduced: pinned ILSpy output matches assembly-derived SHA-256",
            "normalizedRuntimeSha256": "runtime-adaptation-hash",
            "reproductionTool": "ilspycmd 9.1.0.7988",
            "reproducedDecompiledSha256": "assembly-output-hash",
        }
        item = {"decompiledSha256": "assembly-output-hash"}
        self.assertEqual(
            "reproduced",
            VERIFY.verify_plugin_decompilation_delta("Plugin.cs", item, delta, "runtime-adaptation-hash"),
        )

    def test_rejects_reproduction_claim_for_different_decompiler_bytes(self):
        delta = {
            "status": "reproduced: pinned ILSpy output matches assembly-derived SHA-256",
            "normalizedRuntimeSha256": "runtime-adaptation-hash",
            "reproductionTool": "ilspycmd 9.1.0.7988",
            "reproducedDecompiledSha256": "wrong-output-hash",
        }
        item = {"decompiledSha256": "assembly-output-hash"}
        with self.assertRaises(AssertionError):
            VERIFY.verify_plugin_decompilation_delta("Plugin.cs", item, delta, "runtime-adaptation-hash")

    def test_rejects_unknown_plugin_lineage_status(self):
        delta = {"status": "verified by code presence", "normalizedRuntimeSha256": "runtime-hash"}
        with self.assertRaisesRegex(AssertionError, "unknown plugin decompilation status"):
            VERIFY.verify_plugin_decompilation_delta("Plugin.cs", {"decompiledSha256": "x"}, delta, "runtime-hash")
    def test_aliases_are_inserted_before_original_namespace(self):
        original = b"namespace Example\n{\n}\n"
        aliases = ["using TaskStatus = Example.TaskStatus;"]
        runtime = b"using TaskStatus = Example.TaskStatus;\n\n" + original
        self.assertEqual(VERIFY.restore_source_aliases(original, aliases), runtime)

    def test_aliases_can_be_inserted_after_existing_imports(self):
        original = b"using System;\n\nnamespace Example\n{\n}\n"
        aliases = ["using TaskStatus = Example.TaskStatus;", "using Action = Example.Action;"]
        expected = (
            b"using System;\n"
            b"using TaskStatus = Example.TaskStatus;\n"
            b"using Action = Example.Action;\n\n"
            b"namespace Example\n{\n}\n"
        )
        self.assertEqual(VERIFY.restore_source_aliases(original, aliases, "using System;"), expected)

    def test_verified_source_mapping_rejects_changed_runtime_body(self):
        original = b"namespace Example\n{\n    public class Task {}\n}\n"
        aliases = ["using TaskStatus = Example.TaskStatus;"]
        runtime = VERIFY.restore_source_aliases(original, aliases)
        item = {"decompiledSha256": VERIFY.sha(original), "sourceAdaptationAliases": aliases}
        VERIFY.verify_source_adaptation("Task.cs", item, original, runtime)
        with self.assertRaisesRegex(AssertionError, "differs from exact archived original"):
            VERIFY.verify_source_adaptation("Task.cs", item, original, runtime.replace(b"Task {}", b"Task { }") )

    def test_source_mapping_rejects_wrong_archive_identity(self):
        item = {"decompiledSha256": VERIFY.sha(b"expected")}
        with self.assertRaisesRegex(AssertionError, "archived source identity mismatch"):
            VERIFY.verify_source_adaptation("Task.cs", item, b"different", b"different")

    def test_source_alias_marker_must_be_unique(self):
        original = b"using System;\n\nnamespace Example\n{\n}\nusing System;\n\n"
        with self.assertRaisesRegex(AssertionError, "ambiguous"):
            VERIFY.restore_source_aliases(original, ["using Alias = Example.Type;"], "using System;")


if __name__ == "__main__":
    unittest.main()
