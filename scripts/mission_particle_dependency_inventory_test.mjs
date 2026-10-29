import assert from 'node:assert/strict';
import {test} from 'node:test';
import {readFileSync} from 'node:fs';
import {gunzipSync} from 'node:zlib';
import {createHash} from 'node:crypto';

const file=new URL('../docs/history/mission-particle-dependency-inventory-2026-10-25.json.gz',import.meta.url);
const compressed=readFileSync(file);
const raw=gunzipSync(compressed);
const inventory=JSON.parse(raw.toString('utf8'));
const sha=value=>createHash('sha256').update(value).digest('hex');

test('Mission particle inventory is bound to the verified package source',()=>{
  assert.equal(sha(raw),'636935d6bf545dd0bf3610008bbc8759fae1fc765553852b8110da9eacc82407');
  assert.equal(inventory.schemaVersion,1);
  assert.equal(inventory.source.profile,'global-ui-mission-operator');
  assert.equal(inventory.source.manifestSha256,'2acba185c3c14a1b2c60d08ed72b1aeb8c2f3006b977956b26fe70ad95bbbcc2');
  assert.equal(inventory.source.sourcePackageSha256,'92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb');
  assert.equal(inventory.source.assetCount,1919);
});

test('all source particle prefabs and serialized dependencies are inventoried',()=>{
  const c=inventory.coverage;
  assert.equal(c.prefabs,27);
  assert.equal(c.systems,51);
  assert.equal(c.renderers,51);
  assert.equal(c.materials,15);
  assert.equal(c.customMeshes,5);
  assert.equal(c.missionClosurePrefabs,18);
  assert.equal(c.missionClosureSystems,32);
  assert.deepEqual(c.renderModes,{'0':26,'1':6,'4':19});
  assert.equal(c.emissionBurstEntries,9);
  assert.equal(c.nativeSubEmitterReferences,0);
  assert.equal(Object.keys(inventory.dependencies.textures).length,12);
  assert.equal(Object.keys(inventory.dependencies.shaders).length,3);
  assert.equal(Object.keys(inventory.dependencies.meshes).length,5);
  assert.equal(inventory.dependencies.shaders['0000000000000000f000000000000000'].behaviorVerified,false);
});

test('every system retains its exact source component, renderer, hierarchy and owner edges',()=>{
  let systems=0,renderers=0,outsideClosure=0;
  for(const [path,prefab] of Object.entries(inventory.prefabs)){
    assert.match(prefab.guid[0],/^[a-f0-9]{32}$/);
    assert.match(prefab.sha256,/^[a-f0-9]{64}$/);
    assert.equal(prefab.systemCount,prefab.rendererCount,path);
    const byGo=new Map();
    for(const record of prefab.records){
      if(record.classID===199){
        renderers++;
        const go=String(record.fields.m_GameObject.fileID);
        assert.ok(!byGo.has(go),'duplicate particle renderer '+path+':'+go);
        byGo.set(go,record);
      }
    }
    for(const record of prefab.records){
      if(record.classID!==198)continue;
      systems++;
      const go=String(record.fields.m_GameObject.fileID);
      assert.ok(byGo.has(go),'unpaired source ParticleSystem '+path+':'+go);
      assert.ok(record.fields.InitialModule&&record.fields.EmissionModule&&record.fields.ShapeModule);
      assert.ok(record.fields.m_GameObject);
    }
    if(!prefab.missionSceneGuidClosure){
      outsideClosure++;
      assert.ok(prefab.incomingReferences.length>0,'dynamic prefab lacks serialized owner edge: '+path);
    }
  }
  assert.equal(systems,51);
  assert.equal(renderers,51);
  assert.equal(outsideClosure,9);
});

test('inventory marks native gaps explicitly without claiming rendered parity',()=>{
  assert.equal(inventory.unresolved.length,1);
  assert.ok(inventory.unresolved.some(x=>x.guid==='0000000000000000e000000000000000'));
  assert.ok(inventory.limitations.some(x=>x.includes('no browser ParticleSystem driver')));
  assert.ok(inventory.limitations.some(x=>x.includes('built-in shader and mesh')));
});
