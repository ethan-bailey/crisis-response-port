import importlib.util
from pathlib import Path
import unittest

spec = importlib.util.spec_from_file_location(
    "verify_behaviordesigner_ilspy",
    Path(__file__).with_name("verify_behaviordesigner_ilspy.py"),
)
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class IlspyNormalizationTests(unittest.TestCase):

    def test_pinned_decompiler_and_runtime_adaptation_hashes_are_independent(self):
        original = b"pinned decompiler output"
        adapted_runtime = b"intentional runtime compatibility changes"
        item = {"decompiledSha256": module.sha(original)}
        delta = {"normalizedRuntimeSha256": module.sha(adapted_runtime)}
        module.verify_decompiler_output("Example.cs", original, adapted_runtime, item, delta)

    def test_rejects_changed_decompiler_output_even_when_runtime_hash_matches(self):
        original = b"pinned decompiler output"
        adapted_runtime = b"intentional runtime compatibility changes"
        item = {"decompiledSha256": module.sha(original)}
        delta = {"normalizedRuntimeSha256": module.sha(adapted_runtime)}
        with self.assertRaisesRegex(AssertionError, "pinned ILSpy output hash changed"):
            module.verify_decompiler_output("Example.cs", b"changed output", adapted_runtime, item, delta)

    def test_rejects_changed_runtime_adaptation_even_when_decompiler_matches(self):
        original = b"pinned decompiler output"
        item = {"decompiledSha256": module.sha(original)}
        delta = {"normalizedRuntimeSha256": module.sha(b"expected runtime")}
        with self.assertRaisesRegex(AssertionError, "normalized runtime source hash changed"):
            module.verify_decompiler_output("Example.cs", original, b"changed runtime", item, delta)

    def test_selects_reproduced_plugin_lineages_and_skips_archived_game_tasks(self):
        self.assertTrue(module.is_plugin_decompilation_candidate({
            "status": "reproduced: pinned ILSpy output matches assembly-derived SHA-256",
        }))
        self.assertTrue(module.is_plugin_decompilation_candidate({
            "status": "unresolved: decompiled source snapshot omitted from recovery artifact",
        }))
        self.assertFalse(module.is_plugin_decompilation_candidate({
            "status": "resolved: exact archived game-task source plus recorded compiler aliases",
        }))

    def test_removes_only_recorded_compile_alias_and_restores_decompiler_modifier(self):
        runtime = (
            b"using Object = UnityEngine.Object;\n"
            b"namespace BehaviorDesigner.Runtime {\n"
            b"\tint IBehavior.GetInstanceID() { return 7; }\n"
            b"}\n"
        )
        expected = (
            b"namespace BehaviorDesigner.Runtime {\n"
            b"\tvirtual int IBehavior.GetInstanceID() { return 7; }\n"
            b"}\n"
        )
        self.assertEqual(
            expected,
            module.normalize_runtime("BehaviorDesigner.Runtime/Behavior.cs", runtime),
        )

    def test_does_not_rewrite_unrecorded_runtime_body(self):
        runtime = b"namespace BehaviorDesigner.Runtime { public class SharedVariable {} }\n"
        self.assertEqual(
            runtime,
            module.normalize_runtime("BehaviorDesigner.Runtime/SharedVariable.cs", runtime),
        )


if __name__ == "__main__":
    unittest.main()
