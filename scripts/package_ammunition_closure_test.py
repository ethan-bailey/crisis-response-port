import copy
import hashlib
import unittest

from package_ammunition_closure import merge_builtin_resource_data, verify_closure


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


class AmmoClosureAuditTests(unittest.TestCase):
    def setUp(self):
        self.source_bytes = b'{"assets":{"ammo.prefab":{"records":[],"externalGuids":[]}}}'
        source_sha256 = hashlib.sha256(self.source_bytes).hexdigest()
        self.scene = {
            "assets": {
                "ammo.prefab": {"records": [], "externalGuids": ["a" * 32]},
                "dep.prefab": {"records": [{"fileID": "fresh"}], "externalGuids": []},
            },
            "guidPaths": {"a" * 32: "dep.prefab"}, "guidMetadata": {}, "scriptTypes": {}, "scriptAssemblies": {},
            "builtinObjects": {},
        }
        self.weapon = {"ammoPackage": {
            "assets": {"ammo.prefab": {"records": [], "externalGuids": ["a" * 32]}},
            "guidPaths": {"a" * 32: "dep.prefab"}, "guidMetadata": {}, "scriptTypes": {}, "scriptAssemblies": {},
            "closureAudit": {
                "sourceSceneSha256": source_sha256,
                "rootAssets": ["ammo.prefab", "dep.prefab"], "resolvedAssets": ["ammo.prefab", "dep.prefab"],
                "resolvedSourceScripts": [], "builtinGuids": [],
                "builtinResourcePackSha256": None, "builtinPlayerResourceSha256": None,
                "unresolved": [], "complete": True,
            },
        }}
        self.weapon["ammoPackage"]["assets"]["dep.prefab"] = {"records": [{"fileID": "fresh"}], "externalGuids": []}
        self.source_sha256 = source_sha256

    def test_source_snapshot_and_all_serialized_closure_records_must_match(self):
        report = verify_closure(self.weapon, self.scene, self.source_sha256)
        self.assertTrue(report["complete"])

        self.weapon["ammoPackage"]["closureAudit"]["sourceSceneSha256"] = "old-source-snapshot"
        report = verify_closure(self.weapon, self.scene, self.source_sha256)
        self.assertFalse(report["complete"])
        self.assertTrue(report["staleSourceAudit"])

        self.weapon["ammoPackage"]["closureAudit"]["sourceSceneSha256"] = self.source_sha256
        del self.weapon["ammoPackage"]["assets"]["dep.prefab"]
        report = verify_closure(self.weapon, self.scene, self.source_sha256)
        self.assertFalse(report["complete"])
        self.assertTrue(report["staleAssetData"])


if __name__ == "__main__":
    unittest.main()
