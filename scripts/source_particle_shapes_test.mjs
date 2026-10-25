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

test('unsupported shapes and invalid authored inputs fail explicitly',()=>{
  assert.throws(()=>sampleRecoveredParticleShape({type:2}),/Unsupported/);
  assert.throws(()=>sampleRecoveredParticleShape({type:4,radius:{value:-1},arc:{value:360}}),/invalid radius or arc/);
  assert.throws(()=>sampleRecoveredParticleShape({type:4,radius:{value:1},arc:{value:360}},{arc:2}),/outside/);
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
