"""Metadata parser fixtures only; no substitute game/UI implementation."""
import copy
import struct
import unittest

from original_ui_bindings import (UI_GUID, UI_SHA, annotate_closure, script_file_id,
                                  typedefs, verify_reference_vectors)


def fixture(pe64=False, wide_strings=False):
    strings = b"\0Fixture\0Example\0<Module>\0"
    width = "<I" if wide_strings else "<H"
    size = 4 if wide_strings else 2
    idx = lambda n: struct.pack(width, n)
    module = struct.pack("<H", 0) + idx(1) + b"\0" * 6
    row = lambda flags, name, ns: struct.pack("<I", flags) + idx(name) + idx(ns) + b"\0\0\1\0\1\0"
    rows = module + row(0, 17, 0) + row(1, 9, 1)
    tables = struct.pack("<IBBBBQQII", 0, 2, 0, 1 if wide_strings else 0, 1, 5, 0, 1, 2) + rows
    version = b"v4.0.30319\0\0"
    header = struct.pack("<IHHII", 0x424A5342, 1, 1, 0, len(version)) + version + struct.pack("<HH", 0, 2)
    table_offset = len(header) + 12 + 20
    string_offset = (table_offset + len(tables) + 3) & ~3
    stream_headers = (struct.pack("<II", table_offset, len(tables)) + b"#~\0\0"
                      + struct.pack("<II", string_offset, len(strings)) + b"#Strings\0\0\0\0")
    metadata = header + stream_headers + tables
    metadata += b"\0" * (string_offset - len(metadata)) + strings
    image = bytearray(0x800)
    image[:2] = b"MZ"
    struct.pack_into("<I", image, 0x3C, 0x80)
    image[0x80:0x84] = b"PE\0\0"
    optional_size = 0xF0 if pe64 else 0xE0
    struct.pack_into("<H", image, 0x86, 1)
    struct.pack_into("<H", image, 0x94, optional_size)
    optional = 0x98
    struct.pack_into("<H", image, optional, 0x20B if pe64 else 0x10B)
    directory = optional + (112 if pe64 else 96)
    struct.pack_into("<II", image, directory + 14 * 8, 0x2000, 72)
    section = optional + optional_size
    struct.pack_into("<IIII", image, section + 8, 0x600, 0x2000, 0x600, 0x200)
    struct.pack_into("<IHHII", image, 0x200, 72, 2, 5, 0x2048, len(metadata))
    image[0x248:0x248 + len(metadata)] = metadata
    name_offset = 0x248 + table_offset + 32 + len(module) + (4 + 2 * size + 6) + 4
    return image, name_offset


class UIBindingTests(unittest.TestCase):
    def test_pe32_pe64_and_heap_widths(self):
        for pe64 in (False, True):
            for wide in (False, True):
                with self.subTest(pe64=pe64, wide=wide):
                    data, _ = fixture(pe64, wide)
                    values = typedefs(data)
                    self.assertEqual(len(values), 2)
                    self.assertFalse(values[0]["topLevel"])
                    self.assertEqual(values[1]["namespace"], "Fixture")
                    self.assertEqual(values[1]["name"], "Example")
                    self.assertEqual(values[1]["metadataToken"], "0x02000002")
                    self.assertTrue(values[1]["topLevel"])

    def test_truncated_and_invalid_images_fail(self):
        data, _ = fixture()
        for bad in (b"", b"not a DLL", data[:100], data[:0x250]):
            with self.subTest(size=len(bad)), self.assertRaises(ValueError):
                typedefs(bad)

    def test_heap_index_cannot_escape(self):
        data, offset = fixture()
        struct.pack_into("<H", data, offset, 0xFFFF)
        with self.assertRaisesRegex(ValueError, "String index leaves"):
            typedefs(data)

    def test_nested_type_is_not_guessed_as_top_level_component(self):
        data, offset = fixture()
        struct.pack_into("<I", data, offset - 4, 2)
        self.assertFalse(typedefs(data)[1]["topLevel"])

    def test_primary_reference_vectors(self):
        verify_reference_vectors()
        self.assertEqual(script_file_id("UnityEngine", "MonoBehaviour"), 888015496)

    def closure(self):
        return {"summary": {}, "scope": {"fullGameClosureEstablished": False},
                "assets": {"Assets/Fixture.unity": {"references": [
                    {"guid": UI_GUID, "fileID": "7", "status": "missingMeta"}]}},
                "scenes": [{"scene": "Assets/Fixture.unity", "unresolvedReferences": [
                    {"guid": UI_GUID, "fileID": "7", "source": "Assets/Fixture.unity", "status": "missingMeta"},
                    {"guid": "a" * 32, "fileID": "4300000", "source": "Assets/Fixture.unity", "status": "missingMeta"}]}]}

    def identity(self):
        return {"assemblySha256": UI_SHA, "bindings": {"7": {"fullName": "Fixture.MetadataType"}},
                "scope": {"uiRuntimeImplemented": False}}

    def test_identified_ui_stays_explicit_unimplemented_engine_dependency(self):
        result = annotate_closure(self.closure(), self.identity())
        scene = result["scenes"][0]
        self.assertEqual(len(scene["unresolvedReferences"]), 1)
        self.assertEqual(len(scene["originalAssemblyReferences"]), 1)
        self.assertFalse(scene["originalAssemblyReferences"][0]["runtimeBehaviorImplemented"])
        self.assertFalse(result["scope"]["fullGameClosureEstablished"])
        self.assertFalse(result["assets"]["Assets/Fixture.unity"]["references"][0]["exportedMetaPresent"])
        self.assertEqual(result["summary"]["unresolvedExternalGuids"], 1)

    def test_unknown_component_id_cannot_be_suppressed(self):
        proof = self.identity()
        proof["bindings"] = {}
        with self.assertRaisesRegex(ValueError, "lacks verified type identity"):
            annotate_closure(self.closure(), proof)

    def test_annotation_does_not_rewrite_non_ui_references(self):
        data = self.closure()
        original = copy.deepcopy(data["scenes"][0]["unresolvedReferences"][1])
        self.assertEqual(annotate_closure(data, self.identity())["scenes"][0]["unresolvedReferences"], [original])


if __name__ == "__main__":
    unittest.main()
