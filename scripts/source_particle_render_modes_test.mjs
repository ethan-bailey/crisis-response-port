import assert from 'node:assert/strict';
import {test} from 'node:test';
import {readFileSync} from 'node:fs';
import {gunzipSync} from 'node:zlib';
import {stretchedBillboardParameters} from '../game/source-particle-render-modes.mjs';

const inventory=JSON.parse(gunzipSync(readFileSync(new URL('../docs/mission-particle-dependency-inventory-2026-10-25.json.gz',import.meta.url))));

test('all six source stretched-billboard renderers retain their authored sizing inputs',()=>{
  let count=0;
  for(const prefab of Object.values(inventory.prefabs)){
    const renderers=new Map(prefab.records.filter(r=>r.classID===199).map(r=>[String(r.fields.m_GameObject.fileID),r.fields]));
    for(const system of prefab.records.filter(r=>r.classID===198)){
      const renderer=renderers.get(String(system.fields.m_GameObject.fileID));
      if(renderer.m_RenderMode!==1)continue;
      count++;
      assert.equal(renderer.m_CameraVelocityScale,0);
      assert.ok(Number.isFinite(renderer.m_LengthScale));
      assert.ok(Number.isFinite(renderer.m_VelocityScale));
    }
  }
  assert.equal(count,6);
});

test('stretched-billboard geometry follows camera-plane velocity and source scale fields',()=>{
  const renderer={m_LengthScale:1,m_VelocityScale:.01};
  const horizontal=stretchedBillboardParameters([100,0,0],.2,renderer);
  assert.ok(Math.abs(horizontal.length-1.2)<1e-12);
  assert.ok(Math.abs(horizontal.rotation+Math.PI/2)<1e-12);
  const vertical=stretchedBillboardParameters([0,100,0],.2,renderer);
  assert.ok(Math.abs(vertical.rotation)<1e-12);
  assert.equal(vertical.length,1.2);
  const stationary=stretchedBillboardParameters([0,0,0],.2,{m_LengthScale:0,m_VelocityScale:0});
  assert.equal(stationary.length,.001);
});

test('renderer consumes native Stretch settings instead of constructing a mesh without geometry',()=>{
  const renderer=readFileSync(new URL('../game/original-particle-renderer.mjs',import.meta.url),'utf8');
  const binding=readFileSync(new URL('../game/original-particle-event-binding.mjs',import.meta.url),'utf8');
  assert.match(renderer,/mode===0\|\|mode===1\?new THREE\.Sprite/);
  assert.match(renderer,/stretchedBillboardParameters\(particle\.velocity\.toArray\(\),size,particle\.renderer,basis\)/);
  assert.match(renderer,/evaluateSourceInheritedVelocity\(emitter,0,random\.inheritVelocity\)/);
  assert.match(renderer,/attachedRigidbodyVelocity\).*multiplyScalar\(inheritance\)/);
  assert.match(binding,/new Set\(\[0,1,4\]\)/);
});
