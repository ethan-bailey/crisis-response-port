import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {test} from 'node:test';
import {gunzipSync} from 'node:zlib';

const root=new URL('../',import.meta.url);
const read=path=>readFileSync(new URL(path,root),'utf8');
const scenePackage=JSON.parse(gunzipSync(readFileSync(new URL('scene-data/crisis-response-scenes.json.gz',root))));
const timePath='ProjectSettings/TimeManager.asset';

test('recovered TimeManager identity and all authored fields remain exact',()=>{
  const asset=scenePackage.assets[timePath];
  assert.equal(asset.sha256,'1a83e54adbbda7c9f4851103a0c6ab7f6448a3343d4ea5b7620452fa08416ecd');
  assert.equal(asset.records.length,1);
  assert.equal(asset.records[0].classID,5);
  assert.deepEqual(asset.records[0].fields,{
    m_ObjectHideFlags:0,
    'Fixed Timestep':0.02,
    'Maximum Allowed Timestep':0.33333334,
    m_TimeScale:1,
    'Maximum Particle Timestep':0.03,
  });
});

test('both bounded profile builders root code-driven TimeManager lookup explicitly',()=>{
  const incremental=read('scripts/package_incremental_source.py');
  const site=read('scripts/package_wasm_site.py');
  for(const source of [incremental,site]){
    const manager=source.indexOf('"ProjectSettings/TimeManager.asset"');
    const context=source.indexOf('"Assets/Resources/Global context.prefab"');
    assert.ok(manager>=0&&manager<context,'TimeManager must be an explicit root before scene/prefab roots');
    assert.match(source,/SceneLoader reads this explicit project-settings path/);
  }
});

test('workflow and full-profile browser acceptance require the seven-root profile totals',()=>{
  const workflow=read('.github/workflows/source-contracts.yml');
  assert.match(workflow,/"ProjectSettings\/TimeManager\.asset"[\s\S]*"Assets\/Scenes\/Mission 01\.unity"/);
  for(const expected of [
    /profile\.get\("assetCount"\) == 1920/,
    /profile\.get\("recordCount"\) == 22366/,
    /profile\.get\("compressedAssetBytes"\) == 97767924/,
    /profile\.get\("uncompressedAssetBytes"\) == 205515099/,
  ])assert.match(workflow,expected);
  const browser=read('scripts/indexeddb_fullprofile_browser_smoke.mjs');
  assert.match(browser,/assert\.equal\(manifest\.assetCount, 1920\)/);
  assert.match(browser,/assert\.deepEqual\(manifest\.rootAssets,\[[\s\S]*ProjectSettings\/TimeManager\.asset/);
});

test('scene loader and exact EntryTeam contract apply only proven runtime clock fields',()=>{
  const loader=read('runtime/Engine/SceneLoader.cs');
  assert.match(loader,/TryGetAsset\("ProjectSettings\/TimeManager\.asset",out var timeAsset\)/);
  assert.match(loader,/Time\.fixedDeltaTime=Number\(settings,"Fixed Timestep",\.02f\)/);
  assert.match(loader,/Time\.maximumDeltaTime=Number\(settings,"Maximum Allowed Timestep",\.33333334f\)/);
  assert.match(loader,/Time\.timeScale=Number\(settings,"m_TimeScale",1\)/);
  assert.doesNotMatch(loader,/Maximum Particle Timestep/,
    'Maximum Particle Timestep has no proven runtime consumer and must stay explicit');
  const contract=read('runtime/Checks/Program.cs');
  assert.match(contract,/authoredFixedDelta[\s\S]*authoredMaximumDelta[\s\S]*authoredTimeScale[\s\S]*authoredMaximumParticleTimestep/);
  assert.match(contract,/maximumParticleTimestepRuntimeConsumer=false/);
});
