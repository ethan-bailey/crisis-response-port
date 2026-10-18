"""Focused regression for grouped original compiler-dependency call sites."""
import importlib.util
from pathlib import Path
import unittest

SPEC = importlib.util.spec_from_file_location(
    "compiler_summary",
    Path(__file__).with_name("summarize_characterbody_compile.py"),
)
module = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(module)


class CompilerCallsiteTests(unittest.TestCase):
    def test_deduplicates_repeated_lines_but_keeps_distinct_callsites(self):
        log = (
            "/repo/runtime/Original/CharacterBody.Full.cs(10,2): error CS0246: "
            "The type or namespace name 'Mission' could not be found [build]\n"
            "/repo/runtime/Original/CharacterBody.Full.cs(20,4): error CS0246: "
            "The type or namespace name 'Mission' could not be found [build]\n"
            "/repo/runtime/Original/CharacterBody.Full.cs(10,2): error CS0246: "
            "The type or namespace name 'Mission' could not be found [build]\n"
            "/repo/runtime/Original/CharacterBody.Full.cs(30,1): error CS0234: "
            "The type or namespace name 'CharacterAudio' does not exist in the namespace 'Original' [build]\n"
        )
        result = module.summarize(log, "test-sha", 1)
        self.assertEqual(result["uniqueErrorCount"], 3)
        self.assertEqual(result["buildExitCode"], 1)
        self.assertEqual([s["line"] for s in result["missingSymbolCallsites"]["Mission"]], [10, 20])
        self.assertEqual(result["missingSymbolCallsites"]["CharacterAudio"][0]["code"], "CS0234")

    def test_non_missing_errors_are_not_misclassified(self):
        log = "a.cs(1,2): error CS1002: ; expected [build]\n"
        result = module.summarize(log, "test-sha", 1)
        self.assertEqual(result["missingSymbolCallsites"], {})
        self.assertEqual(result["uniqueErrorCount"], 1)


if __name__ == "__main__":
    unittest.main()
