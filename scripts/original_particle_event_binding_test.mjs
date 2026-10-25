import assert from 'node:assert/strict';
import {test} from 'node:test';
import {readFileSync} from 'node:fs';
import {gunzipSync} from 'node:zlib';
import {createOriginalParticleEventBinding} from '../game/original-particle-event-binding.mjs';

const read=path=>JSON.parse(gunzipSync(readFileSync(new URL(path,import.meta.url))).toString('utf8'));
const particles=read('../game/assets/ballistics/original-particles-source.json.gz');
const materials=read('../game/assets/ballistics/original-particle-materials-source.json.gz');
const nativeEvent=(source,phase,time=5)=>({
  type:'materialEffect',time,
  value:{source,phase,position:[1,2,-3],direction:[0,0,1],energy:123,Velocity:15}
});

test('native source map resolves all 14 original effect prefabs and 26 component identities',()=>{
  const binder=createOriginalParticleEventBinding(particles,materials);
  const initial=binder.snapshot();
  assert.equal(initial.sourcePrefabs,14);
  assert.equal(initial.sourceEmitters,26);
  assert.equal(initial.billboardEmitters,14);
  assert.equal(initial.meshEmitters,12);
  assert.equal(initial.renderedParticles,0,'No fabricated emission/rendering');
  assert.equal(initial.sourceVisualParity,false);
  for(const effect of particles.effects){
    const event=nativeEvent(effect.guid,effect.roles[0]==='entryEffect'?'entry':'exit');
    const emitters=binder.resolveEmitters(event);
    assert.equal(emitters.length,effect.emitters.length);
    assert.deepEqual(emitters.map(x=>x.sourceEmitterFileID),effect.emitters.map(x=>x.sourceEmitterFileID));
    assert.deepEqual(emitters.map(x=>x.renderer.m_RenderMode),effect.emitters.map(x=>x.renderer.m_RenderMode));
  }
});

test('real C# event timing and original prefab role are preserved, nothing is drawn',()=>{
  const binder=createOriginalParticleEventBinding(particles,materials);
  const e=particles.effects[0];
  const event=nativeEvent(e.guid,e.roles[0]==='entryEffect'?'entry':'exit',11.75);
  binder.observe([event,{type:'flight',time:11.75,value:{}}],11.75);
  const state=binder.snapshot();
  assert.equal(state.receivedEvents,1);
  assert.equal(state.unrenderedEmitterRequests,e.emitters.length);
  assert.equal(state.renderedParticles,0);
  assert.deepEqual(state.recent,[{source:e.guid,phase:event.value.phase,
    sourceTime:11.75,emitterCount:e.emitters.length}]);
  assert.equal(binder.resolveEmitters({type:'shot'}),null);
});

test('unknown native GUID, mismatched phase and invalid clock fail visibly',()=>{
  const source=particles.effects[0];
  const phase=source.roles[0]==='entryEffect'?'entry':'exit';
  const binder=createOriginalParticleEventBinding(particles,materials);
  assert.throws(()=>binder.observe([nativeEvent('0'.repeat(32),phase)],5),/unknown recovered prefab/);
  assert.throws(()=>binder.observe([nativeEvent(source.guid,'unknown')],5),/phase/);
  assert.throws(()=>binder.observe([nativeEvent(source.guid,phase,6)],5),/after simulation-clock/);
  assert.throws(()=>binder.observe([nativeEvent(source.guid,phase,Number.NaN)],5),/Time.time/);
  assert.throws(()=>binder.observe([nativeEvent(source.guid,phase)],Number.NaN),/simulation-clock/);
});

test('source observer bounds diagnostics across repeated native impact events',()=>{
  const source=particles.effects[0],phase=source.roles[0]==='entryEffect'?'entry':'exit';
  const binder=createOriginalParticleEventBinding(particles,materials);
  const events=Array.from({length:35},(_,n)=>nativeEvent(source.guid,phase,n/10));
  binder.observe(events,3.5);
  const state=binder.snapshot();
  assert.equal(state.receivedEvents,35);
  assert.equal(state.unrenderedEmitterRequests,35*source.emitters.length);
  assert.equal(state.recent.length,32);
  assert.equal(state.recent[0].sourceTime,.3);
});

test('FullSource particle command transport validates provenance, actions and source clock',()=>{
  const binder=createOriginalParticleEventBinding(particles,materials);
  const effect=particles.effects[0],emitter=effect.emitters[0];
  const command={type:'particleCommand',time:4.5,systemId:12,sourceGuid:effect.guid,
    sourceFileId:emitter.sourceEmitterFileID,sourcePath:'Assets/Resources/effects/ballistic hits/Concrete entry.prefab',
    sourceAssetSha256:'a'.repeat(64),hierarchyPath:'Root/Emitter',
    position:[1,2,3],rotation:[0,0,0,1],command:{action:'emit',count:3,maxParticles:512,
      startSpeed:[2,4],radius:.2,angle:25}};
  binder.observeSourceCommands([command],5,0);
  binder.observeSourceCommands([{...command,time:5,command:{action:'simulate',simulationTime:.02,
    withChildren:true,restart:true,fixedTimeStep:false}}],5,2);
  binder.observeSourceCommands([{...command,time:5,command:{action:'play'}}],5,2);
  const state=binder.snapshot();
  assert.equal(state.sourceCommands,3);
  assert.equal(state.sourceCommandDrops,2);
  assert.deepEqual(state.sourceCommandActions,{emit:1,simulate:1,play:1});
  assert.equal(state.recentSourceCommands.length,3);
  assert.equal(state.renderedParticles,0);
  assert.equal(state.sourceVisualParity,false);
});

test('FullSource particle commands reject missing source identity and malformed action payloads',()=>{
  const binder=createOriginalParticleEventBinding(particles,materials);
  const effect=particles.effects[0],emitter=effect.emitters[0];
  const command={type:'particleCommand',time:4.5,sourceGuid:effect.guid,
    sourceFileId:emitter.sourceEmitterFileID,sourcePath:'Assets/Effects/Test.prefab',
    sourceAssetSha256:'a'.repeat(64),hierarchyPath:'Root/Emitter',
    position:[1,2,3],rotation:[0,0,0,1],command:{action:'play'}};
  assert.throws(()=>binder.observeSourceCommands([{...command,sourceAssetSha256:'bad'}],5),/provenance/);
  assert.throws(()=>binder.observeSourceCommands([{...command,time:6}],5),/after simulation-clock/);
  assert.throws(()=>binder.observeSourceCommands([{...command,command:{action:'emit',count:-1,startSpeed:[1,2]}}],5),/invalid count/);
  assert.throws(()=>binder.observeSourceCommands([{...command,command:{action:'restart'}}],5),/unknown/);
});

test('normal page forwards the FullSource actor particle snapshot into the source observer',()=>{
  const html=readFileSync(new URL('../game/index.html',import.meta.url),'utf8');
  const presentation=readFileSync(new URL('../game/presentation.mjs',import.meta.url),'utf8');
  const weaponRuntime=readFileSync(new URL('../game/weapon-runtime.mjs',import.meta.url),'utf8');
  assert.match(html,/observeSourceParticleCommands\(sourceActorSnapshot\.particleEvents,sourceActorSnapshot\.sourceTime,sourceActorSnapshot\.droppedParticleEvents\)/);
  assert.match(presentation,/observeSourceParticleCommands\(events,sourceTime,dropped\)\{effects\.observeSourceParticleCommands/);
  assert.match(weaponRuntime,/observeSourceParticleCommands:\(events,sourceTime,dropped\)=>originalParticleEvents\.observeSourceCommands/);
  const queue=readFileSync(new URL('../runtime/Engine/BrowserParticleEventQueue.cs',import.meta.url),'utf8');
  const loader=readFileSync(new URL('../runtime/Engine/SceneLoader.cs',import.meta.url),'utf8');
  assert.match(queue,/sourceAssetSha256 = system\.OriginalAssetSha256/);
  assert.match(loader,/component\.OriginalAssetSha256=package\.GetAssetSha256\(assetPath\)/);
});

test('missing or corrupted native material/particle manifest cannot fall back to invented assets',()=>{
  assert.throws(()=>createOriginalParticleEventBinding({...particles,effects:particles.effects.slice(1)},materials),
    /expected 14/);
  const invalid={...materials,effectsByMaterial:{...materials.effectsByMaterial}};
  invalid.effectsByMaterial[Object.keys(invalid.effectsByMaterial)[0]]=['unverified'];
  assert.throws(()=>createOriginalParticleEventBinding(particles,invalid),/linkage mismatch/);
});
