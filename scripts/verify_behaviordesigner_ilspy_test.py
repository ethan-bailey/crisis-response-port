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
