import copy
import json
from pathlib import Path
import shutil
import tempfile
import unittest
from unittest.mock import patch
import check_actor_migration as actor
import inventory_source_dependencies as inventory
from check_characterbody_ownership import active_full_runtime_lines
from summarize_characterbody_compile import summarize, compare
import extract_runtime_scene as runtime_scene


class DiagnosticDiffTests(unittest.TestCase):
    def test_runner_paths_summary_duplicates_and_line_movement(self):
        a="2026-10-18T02:00:00Z /home/runner/work/repo/runtime/Original/A.cs(1,2): error CS0246: The type or namespace name 'Lost' could not be found [x.csproj]"
        before=summarize(a+'\n'+a, 'old',1)
        after=summarize(a.replace('(1,2)','(10,2)'), 'new',1)
        self.assertEqual(before['uniqueErrorCount'],1)
        self.assertEqual(before['compilerDiagnostics'][0]['path'],'runtime/Original/A.cs')
        self.assertEqual(compare(before,after)['introduced'],[])
        self.assertEqual(compare(before,after)['remainingCount'],1)

    def test_new_and_resolved_diagnostics_and_occurrences(self):
        def log(name,line):return f'runtime/A.cs({line},1): error CS0246: The type or namespace name \'{name}\' could not be found'
        before=summarize(log('A',1)+'\n'+log('A',2)+'\n'+log('B',3))
        after=summarize(log('A',5)+'\n'+log('C',6))
        delta=compare(before,after)
        self.assertEqual(sum(x['occurrences'] for x in delta['resolved']),2)
        self.assertEqual(delta['resolvedSymbols'],['B'])
        self.assertEqual(delta['introducedSymbols'],['C'])

    def test_empty_or_infrastructure_failure_never_implies_success(self):
        report=summarize('', 'head',1)
        self.assertEqual(report['buildExitCode'],1)
        report=summarize('MSBUILD : error MSB1009: Project file does not exist.', 'head',1)
        self.assertEqual(report['uniqueErrorCount'],1)
        self.assertEqual(report['compilerDiagnostics'][0]['code'],'MSB1009')


class UnitySerializedDependencyTests(unittest.TestCase):
    def test_mixer_parameter_guids_are_not_external_asset_edges(self):
        internal = (
            'a6cad62f53b20d3498b1e4290043d6e5',
            'afbac48e663910a4ab80259dd0827f74',
            'c97d77ee9defe194781c1ac0e6b4f82e1',
            'd8f6aa80b67e5ed42aab0244cabd47bc',
            'fb85821311eb68d46bcb1281a048b8a3',
        )
        external = '1234567890abcdef1234567890abcdef'
        text = chr(10).join([
            '--- !u!241 &24100000',
            'AudioMixerController:',
            '  m_FloatValues:',
            *[f'    {guid}: 1' for guid in internal],
            f'  m_OutputAudioMixerGroup: {{fileID: 24300000, guid: {external}, type: 2}}',
        ])
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'Master mixer.mixer'
            path.write_text(text)
            asset = runtime_scene.parse_asset(path, 'Assets/AudioMixerController/Master mixer.mixer')
        self.assertEqual(asset['externalGuids'], [external])
        self.assertTrue(all(guid not in asset['externalGuids'] for guid in internal))


class RuntimePackageInputTests(unittest.TestCase):
    def test_original_operator_and_occupant_prefabs_are_packaged(self):
        root = Path(runtime_scene.__file__).resolve().parents[1]
        seeds = set(runtime_scene.runtime_asset_seeds(root))
        manifest_path = root / 'source-reference/Original/CharacterDependencies/ActorPrefabs/provenance.json'
        manifest = json.loads(manifest_path.read_text())
        for actor in ('Operator', 'Occupant'):
            prefab_path = f'Assets/Resources/characters/{actor}.prefab'
            self.assertIn(prefab_path, seeds)
            self.assertGreater(manifest['actorInventory'][actor]['serializedRecords'], 0)
            self.assertTrue((root / 'source-reference/Original/CharacterDependencies/ActorPrefabs' / f'{actor}.prefab').is_file())


class BoundaryTests(unittest.TestCase):
    def test_record_and_struct_transport_owners_are_rejected(self):
        root = actor.ROOT
        with tempfile.TemporaryDirectory() as d:
            target = Path(d)
            manifest = json.loads((root/'docs/audit/actor-migration-boundary.json').read_text())
            files = list(manifest['frozenLegacyFiles']) + [
                'docs/audit/actor-migration-boundary.json',
                'runtime/Crisis.Runtime.csproj']
            for name in files:
                destination = target/name
                destination.parent.mkdir(parents=True, exist_ok=True)
                shutil.copyfile(root/name, destination)
            self.assertEqual(actor.check(target), [])
            extra = target/'runtime/Original/Weapons/AnotherOwner.cs'
            extra.parent.mkdir(parents=True, exist_ok=True)
            for declaration in ('struct Limb', 'record CharacterBody', 'record class Equipment',
                                'record struct Blackboard'):
                extra.write_text('namespace Crisis.Runtime.Weapons;\\npublic ' +
                                 declaration + ' {}\\n')
                self.assertTrue(any('New legacy gameplay owner declaration' in e
                                    for e in actor.check(target)), declaration)

    def test_transport_change_and_production_exclusion_are_rejected(self):
        root=actor.ROOT
        with tempfile.TemporaryDirectory() as d:
            target=Path(d)
            manifest=json.loads((root/'docs/audit/actor-migration-boundary.json').read_text())
            files=list(manifest['frozenLegacyFiles'])+['docs/audit/actor-migration-boundary.json','runtime/Crisis.Runtime.csproj']
            for name in files:
                p=target/name;p.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(root/name,p)
            self.assertEqual(actor.check(target),[])
            name=next(iter(manifest['frozenLegacyFiles']));(target/name).write_text('new state owner')
            self.assertTrue(any('Legacy gameplay' in e for e in actor.check(target)))
            project=target/'runtime/Crisis.Runtime.csproj';project.write_text(project.read_text().replace('Include="Original/**/*.cs"','Include="Original/**/*.cs" Exclude="Original/CharacterBody.Full.cs"'))
            self.assertTrue(any('without exclusions' in e for e in actor.check(target)))
            extra=target/'runtime/Original/Weapons/NewTransportState.cs'
            extra.parent.mkdir(parents=True,exist_ok=True)
            extra.write_text('namespace Crisis.Runtime.Weapons; public partial class CharacterBody { public float health; }')
            self.assertTrue(any('New legacy gameplay owner' in e for e in actor.check(target)))

    def test_nested_foundation_guard_cannot_hide_production_transport(self):
        source='#if CRISIS_FOUNDATION_TRANSPORT\ntransport fixture\n#if UNKNOWN\nnested fixture\n#endif\n#else\nproduction binding\n#endif\nouter binding'
        active='\n'.join(line for _,line in active_full_runtime_lines(source))
        self.assertNotIn('fixture',active)
        self.assertIn('production binding',active)
        self.assertIn('outer binding',active)

    def test_inventory_covers_every_pinned_missing_name(self):
        report=inventory.generate()
        baseline=json.loads((inventory.ROOT/'docs/audit/compiler-baseline.json').read_text())
        found={s['name'] for s in report['symbols'] if s['compilerMissing']}
        self.assertEqual(found,set(baseline['missingSymbols']))
        self.assertEqual(report['archivedSourceCount'],1384)
        self.assertFalse(any(s['category']=='currently unavailable' for s in report['symbols'] if s['compilerMissing']))
        self.assertEqual(report['baselineSha'],baseline['sourceSha'])
        self.assertIn('runtime/Original/FullSource/Mission.cs',report['reachableFiles'])
        self.assertIn('CharacterBody',report['ambiguousNames'])

    def test_comment_literals_do_not_create_dependency_edges(self):
        clean=inventory.NOISE.sub(' ', '// class Fake {}\nstring s="class Other"; class Real {}')
        self.assertEqual(inventory.DECL.findall(clean),['Real'])


if __name__=='__main__':unittest.main()
