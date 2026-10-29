import assert from 'node:assert/strict';
import {test} from 'node:test';
import {readFileSync} from 'node:fs';
import {gunzipSync} from 'node:zlib';
import {createHash} from 'node:crypto';

const compressed=readFileSync(new URL('../docs/history/mission-particle-dependency-inventory-2026-10-25.json.gz',import.meta.url));
const raw=gunzipSync(compressed);
const inventory=JSON.parse(raw.toString('utf8'));

function activeClampVelocityModules(){
  const active=[];
  for(const [path,prefab] of Object.entries(inventory.prefabs)){
    for(const record of prefab.records){
      if(record.classID!==198)continue;
      const module=record.fields.ClampVelocityModule;
      if(module?.enabled)active.push({path,fileID:record.fileID,module});
    }
  }
  return active;
}
function tally(values){
  return Object.fromEntries([...new Set(values)].sort((a,b)=>a-b).map(value=>[value,values.filter(x=>x===value).length]));
}

test('active source ClampVelocity modules retain their exact authored limits and space',()=>{
  assert.equal(createHash('sha256').update(raw).digest('hex'),'636935d6bf545dd0bf3610008bbc8759fae1fc765553852b8110da9eacc82407');
  assert.equal(inventory.coverage.activeModules.ClampVelocityModule,31);
  const active=activeClampVelocityModules();
  assert.equal(active.length,31);
  assert.ok(active.every(({module})=>module.separateAxis===0),'source modules use the overall magnitude limit');
  assert.ok(active.every(({module})=>module.magnitude.minMaxState===0),'magnitude records share serialized minMaxState');
  assert.deepEqual(tally(active.map(({module})=>module.magnitude.scalar)),{0:26,5:5});
  assert.deepEqual(tally(active.map(({module})=>module.inWorldSpace)),{0:25,1:6});
  assert.ok(active.every(({module})=>module.drag.minMaxState===0&&module.drag.scalar===0));
  assert.ok(active.every(({module})=>module.multiplyDragByParticleSize===1&&module.multiplyDragByParticleVelocity===1));
  assert.deepEqual(tally(active.map(({module})=>module.dampen)),{
    0.5:2,0.6:1,0.65:3,0.7:1,0.75:1,0.95:1,0.97:1,0.98:11,0.99:1,0.9975:1,0.999:5,0.9995:3
  });
});

test('inactive per-axis triples remain preserved and are not promoted to active limits',()=>{
  const active=activeClampVelocityModules();
  assert.deepEqual(tally(active.map(({module})=>module.x.scalar)),{0.01:6,1:25});
  assert.deepEqual(tally(active.map(({module})=>module.y.scalar)),{0.01:6,1:25});
  assert.deepEqual(tally(active.map(({module})=>module.z.scalar)),{0.01:6,1:25});
  assert.ok(active.every(({module})=>module.x.minMaxState===0&&module.y.minMaxState===0&&module.z.minMaxState===0));
});
