import copy
import unittest

from package_ammunition_closure import merge_builtin_resource_data


class PlayerResourceClosureTests(unittest.TestCase):
    def setUp(self):
        self.player_pack = {
            "unityVersion": "2017.2.0b11",
            "targetPlatform": 5,
            "sourceSha256": "b809e16256c643c6ac8b5e6d0af164a9c9ec07e0e5bd49460939c39da5fafb40",
            "records": {"10206": {
                "type": "Mesh",
                "rawSha256": "1aea642b2ab9c4fe2458206b6d95c956788b91fa854b2bbc6daa95e3c23886a7",
                "fields": {"m_Name": "Cylinder"},
                "runtimeGeometry": {"vertices": [[0, 0, 0]] * 88, "triangles": [0] * 240},
            }},
        }
        self.scene = {
            "builtinObjects": {"0000000000000000e000000000000000": {"10206": "Mesh"}},
            "builtinPlayerResourceRecords": self.player_pack,
        }
        self.ammo = {"assets": {"ammo.prefab": {"records": []}}}

    def test_copies_the_exact_player_mesh_record_for_a_builtin_mesh_reference(self):
        merge_builtin_resource_data(self.ammo, self.scene)
        self.assertEqual(self.ammo["builtinPlayerResourceRecords"], self.player_pack)
        self.assertEqual(self.ammo["builtinObjects"]["0000000000000000e000000000000000"]["10206"], "Mesh")

    def test_rejects_an_editor_version_relabelled_as_player_resource_data(self):
        self.scene["builtinPlayerResourceRecords"]["unityVersion"] = "2017.2.0f3"
        with self.assertRaisesRegex(ValueError, "provenance is invalid"):
            merge_builtin_resource_data(self.ammo, self.scene)

    def test_requires_source_data_when_the_scene_references_cylinder(self):
        del self.scene["builtinPlayerResourceRecords"]
        with self.assertRaisesRegex(ValueError, "without its recovered source record"):
            merge_builtin_resource_data(self.ammo, self.scene)


if __name__ == "__main__":
    unittest.main()
