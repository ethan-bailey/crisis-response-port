import assert from 'node:assert/strict';
import {test} from 'node:test';
import {readFileSync} from 'node:fs';
import {gunzipSync} from 'node:zlib';
import {sampleRecoveredParticleShape} from '../game/source-particle-shapes.mjs';

test('recovered cone base uses source radius, angle, arc and unit direction',()=>{
  const shape={type:4,radius:{value:2},angle:30,arc:{value:180},radiusThickness:1,randomDirectionAmount:0};
  const p=sampleRecoveredParticleShape(shape,{radius:.25,arc:.5});
  assert.ok(Math.abs(Math.hypot(...p.position)-1)<1e-9);
  assert.ok(Math.abs(p.position[1]-1)<1e-9);
  assert.ok(Math.abs(p.direction[0])<1e-9);
  assert.ok(Math.abs(p.direction[2]-Math.cos(Math.PI/6))<1e-9);
  assert.ok(Math.abs(Math.hypot(...p.direction)-1)<1e-9);
});

test('recovered circle edge emits on authored radius and arc',()=>{
  const shape={type:10,radius:{value:3},arc:{value:90},randomDirectionAmount:0};
  const p=sampleRecoveredParticleShape(shape,{arc:1});
  assert.ok(Math.abs(Math.hypot(...p.position)-3)<1e-9);
  assert.ok(Math.abs(p.position[1]-3)<1e-9);
  assert.deepEqual(p.direction,[0,0,1]);
});

test('shape random-direction amount blends and normalizes',()=>{
  const base={type:4,radius:{value:1},angle:0,arc:{value:360},randomDirectionAmount:1};
  const p=sampleRecoveredParticleShape(base,{arc:.5,directionZ:.5,directionArc:0});
  assert.ok(Math.abs(p.direction[0]-1)<1e-9);
  assert.ok(Math.abs(Math.hypot(...p.direction)-1)<1e-9);
});

test('recovered hemisphere samples the upper half-sphere and authored shell thickness',()=>{
  const shape={type:2,radius:{value:2},arc:{value:360},radiusThickness:1,randomDirectionAmount:0};
  const surface=sampleRecoveredParticleShape(shape,{radius:1,hemisphereZ:.5,hemisphereArc:.25});
  assert.ok(Math.abs(Math.hypot(...surface.position)-2)<1e-9);
  assert.ok(Math.abs(surface.position[0])<1e-9);
  assert.ok(surface.position[1]>0&&surface.position[2]>0);
  assert.deepEqual(surface.direction,surface.position.map(x=>x/2));
  const inner=sampleRecoveredParticleShape({...shape,radiusThickness:.5},
    {radius:0,hemisphereZ:1,hemisphereArc:0});
  assert.ok(Math.abs(inner.position[2]-1)<1e-9);
});

test('unsupported shapes and invalid authored inputs fail explicitly',()=>{
  assert.throws(()=>sampleRecoveredParticleShape({type:3}),/Unsupported/);
  assert.throws(()=>sampleRecoveredParticleShape({type:4,radius:{value:-1},arc:{value:360}}),/invalid radius or arc/);
  assert.throws(()=>sampleRecoveredParticleShape({type:4,radius:{value:1},arc:{value:360}},{arc:2}),/outside/);
  assert.throws(()=>sampleRecoveredParticleShape({type:2,radius:{value:1},arc:{value:360},randomPositionAmount:.1}),/unsupported position/);
});

test('all 26 impact emitters use the recovered cone-base or circle-edge shape paths',()=>{
  const particles=JSON.parse(gunzipSync(readFileSync(new URL('../game/assets/ballistics/original-particles-source.json.gz',import.meta.url))));
  const emitters=particles.effects.flatMap(effect=>effect.emitters);
  assert.equal(emitters.length,26);
  assert.ok(emitters.every(emitter=>[4,10].includes(emitter.enabledModules.ShapeModule.type)));
  assert.equal(emitters.filter(emitter=>emitter.enabledModules.ShapeModule.type===10).length,1);
  for(const emitter of emitters){
    const sample=sampleRecoveredParticleShape(emitter.enabledModules.ShapeModule,
      {radius:.37,arc:.61,directionZ:.27,directionArc:.82});
    assert.ok(sample.position.every(Number.isFinite));
    assert.ok(sample.direction.every(Number.isFinite));
    assert.ok(Math.abs(Math.hypot(...sample.direction)-1)<1e-9);
  }
});

test('all seven full-profile hemisphere systems retain supported source settings',()=>{
  const inventory=JSON.parse(gunzipSync(readFileSync(new URL('../docs/history/mission-particle-dependency-inventory-2026-10-25.json.gz',import.meta.url))));
  const hemisphereSystems=[];
  for(const [path,prefab] of Object.entries(inventory.prefabs)){
    const records=new Map(prefab.records.map(record=>[String(record.fileID),record]));
    for(const record of prefab.records){
      if(record.classID!==198||record.fields?.ShapeModule?.type!==2)continue;
      const gameObject=records.get(String(record.fields.m_GameObject.fileID));
      const shape=record.fields.ShapeModule;
      hemisphereSystems.push({path,name:gameObject.fields.m_Name,shape});
    }
  }
  assert.equal(hemisphereSystems.length,7);
  for(const {path,name,shape} of hemisphereSystems){
    assert.ok(shape.enabled,`${path}/${name} shape is disabled`);
    assert.equal(shape.radiusThickness,1,`${path}/${name} radius thickness differs`);
    assert.equal(shape.randomPositionAmount,0,`${path}/${name} uses unsupported position randomization`);
    assert.equal(shape.sphericalDirectionAmount,0,`${path}/${name} uses unsupported spherical direction randomization`);
    const sample=sampleRecoveredParticleShape(shape,{radius:.37,arc:.61,hemisphereZ:.27,hemisphereArc:.82,directionZ:.73,directionArc:.16});
    assert.ok(sample.position.every(Number.isFinite));
    assert.ok(sample.position[2]>=0,`${path}/${name} emitted outside the authored hemisphere`);
    assert.ok(Math.abs(Math.hypot(...sample.direction)-1)<1e-9);
  }
});
