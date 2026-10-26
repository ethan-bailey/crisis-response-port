import assert from 'node:assert/strict';
import {test} from 'node:test';
import {readFileSync} from 'node:fs';
import {gunzipSync} from 'node:zlib';
import {createHash} from 'node:crypto';
import {createOriginalParticleEventBinding} from '../game/original-particle-event-binding.mjs';

const read=path=>JSON.parse(gunzipSync(readFileSync(new URL(path,import.meta.url))).toString('utf8'));
const particles=read('../game/assets/ballistics/original-particles-source.json.gz');
const materials=read('../game/assets/ballistics/original-particle-materials-source.json.gz');
const controllers=read('../game/assets/ballistics/original-particle-controllers-source.json.gz');
const nativeEvent=(source,phase,time=5)=>({
  type:'materialEffect',time,
  value:{source,phase,position:[1,2,-3],direction:[0,0,1],energy:123,Velocity:15}
});

test('native source map resolves all 14 original effect prefabs and 26 component identities',()=>{
  const binder=createOriginalParticleEventBinding(particles,materials,controllers);
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

test('particle source binding samples authored curve fields while preserving emitter and material identity',()=>{
  const binder=createOriginalParticleEventBinding(particles,materials,controllers);
  const source=particles.effects[0],sampled=binder.sampleSourceEffect(source.guid,.5,{startSpeed:.25,color:.75});
  assert.equal(sampled.length,source.emitters.length);
  assert.deepEqual(sampled.map(x=>x.sourceEmitterFileID),source.emitters.map(x=>x.sourceEmitterFileID));
  for(let i=0;i<sampled.length;i++){
    assert.equal(sampled[i].renderMode,source.emitters[i].renderer.m_RenderMode);
    assert.deepEqual(sampled[i].materialGuids,source.emitters[i].renderer.m_Materials.map(x=>x.guid).filter(Boolean));
    assert.ok(Number.isFinite(sampled[i].fields.speed));
    assert.ok(Number.isFinite(sampled[i].fields.size));
    assert.ok(Number.isFinite(sampled[i].fields.lifetime));
    assert.ok(Number.isFinite(sampled[i].fields.rotation));
    assert.equal(sampled[i].controller.m_Script.guid,'7f9bb5f5ef1d9fb3ca6dbc9296820005');
  }
  assert.throws(()=>binder.sampleSourceEffect('0'.repeat(32),.5),/unrecovered original prefab/);
});

test('impact events resolve the complete source prefab controller graph and authored emission commands',()=>{
  const binder=createOriginalParticleEventBinding(particles,materials,controllers);
  const source=particles.effects.find(e=>e.source.endsWith('Concrete entry.prefab'));
  const event=nativeEvent(source.guid,'entry',2);
  Object.assign(event.value,{Velocity:1200,energy:500,metricDiameter:.009,projectileDiameter:9,
    displacedMaterialVolume:1.25,particleCountScaler:1});
  const plans=binder.evaluateImpactEffect(event,{particlesLifetime:20,emissionsModifier:1,chunkSizeMultiplier:1});
  assert.equal(plans.length,source.emitters.length);
  assert.equal(plans.reduce((sum,p)=>sum+p.emission.count,0),8);
  assert.deepEqual(plans.map(p=>p.hierarchyPath),['Concrete entry/Smoke emitter','Concrete entry/Chunk emitter']);
  assert.ok(plans.every(p=>p.sourceGuid===source.guid&&p.renderer));
  assert.throws(()=>binder.evaluateImpactEffect(nativeEvent(source.guid,'unknown')),/phase/);
});

test('source binding observer preserves native event timing and prefab role',()=>{
  const binder=createOriginalParticleEventBinding(particles,materials,controllers);
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
  const binder=createOriginalParticleEventBinding(particles,materials,controllers);
  assert.throws(()=>binder.observe([nativeEvent('0'.repeat(32),phase)],5),/unknown recovered prefab/);
  assert.throws(()=>binder.observe([nativeEvent(source.guid,'unknown')],5),/phase/);
  assert.throws(()=>binder.observe([nativeEvent(source.guid,phase,6)],5),/after simulation-clock/);
  assert.throws(()=>binder.observe([nativeEvent(source.guid,phase,Number.NaN)],5),/Time.time/);
  assert.throws(()=>binder.observe([nativeEvent(source.guid,phase)],Number.NaN),/simulation-clock/);
});

test('source observer bounds diagnostics across repeated native impact events',()=>{
  const source=particles.effects[0],phase=source.roles[0]==='entryEffect'?'entry':'exit';
  const binder=createOriginalParticleEventBinding(particles,materials,controllers);
  const events=Array.from({length:35},(_,n)=>nativeEvent(source.guid,phase,n/10));
  binder.observe(events,3.5);
  const state=binder.snapshot();
  assert.equal(state.receivedEvents,35);
  assert.equal(state.unrenderedEmitterRequests,35*source.emitters.length);
  assert.equal(state.recent.length,32);
  assert.equal(state.recent[0].sourceTime,.3);
});

test('FullSource particle command transport validates provenance, actions and source clock',()=>{
  const binder=createOriginalParticleEventBinding(particles,materials,controllers);
  const effect=particles.effects[0],emitter=effect.emitters[0];
  const command={type:'particleCommand',time:4.5,systemId:12,sourceGuid:effect.guid,
    sourceFileId:emitter.sourceEmitterFileID,sourcePath:'Assets/Resources/effects/ballistic hits/Concrete entry.prefab',
    sourceAssetSha256:'a'.repeat(64),hierarchyPath:'Root/Emitter',
    sourceFields:{m_ParticleSystem:{serializedVersion:6}},
    renderer:{sourceGuid:'0123456789abcdef0123456789abcdef',sourceFileId:'19900000',
      sourcePath:'Assets/Resources/effects/ballistic hits/Concrete entry.prefab',sourceAssetSha256:'a'.repeat(64),
      fields:{m_RenderMode:0}},
    position:[1,2,3],rotation:[0,0,0,1],attachedRigidbodyVelocity:[.5,0,-1],command:{action:'emit',count:3,maxParticles:512,
      startSpeed:[2,4],startLifetime:[3,3],startSizeMultiplier:1.25,randomSeed:123456789,
      radius:.2,angle:25}};
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
  const binder=createOriginalParticleEventBinding(particles,materials,controllers);
  const effect=particles.effects[0],emitter=effect.emitters[0];
  const command={type:'particleCommand',time:4.5,sourceGuid:effect.guid,
    sourceFileId:emitter.sourceEmitterFileID,sourcePath:'Assets/Effects/Test.prefab',
    sourceAssetSha256:'a'.repeat(64),hierarchyPath:'Root/Emitter',
    sourceFields:{m_ParticleSystem:{serializedVersion:6}},
    renderer:{sourceGuid:'0123456789abcdef0123456789abcdef',sourceFileId:'19900000',
      sourcePath:'Assets/Effects/Test.prefab',sourceAssetSha256:'a'.repeat(64),fields:{m_RenderMode:0}},
    position:[1,2,3],rotation:[0,0,0,1],attachedRigidbodyVelocity:[0,0,0],command:{action:'play'}};
  assert.throws(()=>binder.observeSourceCommands([{...command,sourceAssetSha256:'bad'}],5),/provenance/);
  assert.throws(()=>binder.observeSourceCommands([{...command,sourceFields:{}}],5),/ParticleSystem module fields/);
  assert.throws(()=>binder.observeSourceCommands([{...command,renderer:{...command.renderer,fields:{}}}],5),/ParticleSystemRenderer fields/);
  assert.throws(()=>binder.observeSourceCommands([{...command,time:6}],5),/after simulation-clock/);
  assert.throws(()=>binder.observeSourceCommands([{...command,command:{...command.command,action:'emit',count:-1}}],5),/invalid count/);
  assert.throws(()=>binder.observeSourceCommands([{...command,command:{action:'restart'}}],5),/unknown/);
  assert.throws(()=>binder.observeSourceCommands([{...command,attachedRigidbodyVelocity:[0,NaN,0]}],5),/world pose/);
});

test('normal page forwards the FullSource actor particle snapshot into the source observer',()=>{
  const html=readFileSync(new URL('../game/index.html',import.meta.url),'utf8');
  const presentation=readFileSync(new URL('../game/presentation.mjs',import.meta.url),'utf8');
  const weaponRuntime=readFileSync(new URL('../game/weapon-runtime.mjs',import.meta.url),'utf8');
  assert.match(html,/observeSourceParticleCommands\(sourceActorSnapshot\.particleEvents,sourceActorSnapshot\.sourceTime,sourceActorSnapshot\.droppedParticleEvents\)/);
  assert.match(presentation,/observeSourceParticleCommands\(events,sourceTime,dropped\)\{effects\.observeSourceParticleCommands/);
  assert.match(weaponRuntime,/originalParticleEvents\.observeSourceCommands\(events,sourceTime,dropped\)/);
  assert.match(weaponRuntime,/originalParticleRenderer\.observeCommands\(events\)/);
  const queue=readFileSync(new URL('../runtime/Engine/BrowserParticleEventQueue.cs',import.meta.url),'utf8');
  const loader=readFileSync(new URL('../runtime/Engine/SceneLoader.cs',import.meta.url),'utf8');
  assert.match(queue,/sourceAssetSha256 = system\.OriginalAssetSha256/);
  assert.match(queue,/randomSeed = state\.randomSeed/);
  assert.match(queue,/attachedRigidbodyVelocity = attachedRigidbody == null/);
  assert.match(queue,/startLifetime = new\[\] \{ state\.startLifetime\.constantMin, state\.startLifetime\.constantMax \}/);
  assert.match(queue,/sourceFields = system\.OriginalSerializedFields/);
  assert.match(queue,/fields = sourceRenderer\.OriginalSerializedFields/);
  assert.match(loader,/component\.OriginalAssetSha256=package\.GetAssetSha256\(assetPath\)/);
  assert.match(weaponRuntime,/originalParticleRenderer\.update\(events,sourceTime,camera\)/);
  const renderer=readFileSync(new URL('../game/original-particle-renderer.mjs',import.meta.url),'utf8');
  assert.match(renderer,/binder\.evaluateImpactEffect\(event,settings\)/);
  assert.match(renderer,/renderedParticles:active\.length/);
});

test('impact events preserve the original Rigidbody velocity when no Emit command is available',()=>{
  const binding=readFileSync(new URL('../game/original-particle-event-binding.mjs',import.meta.url),'utf8');
  const renderer=readFileSync(new URL('../game/original-particle-renderer.mjs',import.meta.url),'utf8');
  const adapter=readFileSync(new URL('../runtime/Engine/BallisticEffects.cs',import.meta.url),'utf8');
  assert.match(adapter,/PlayExit\(Projectile p,Vector3 parentVelocity,AudioInstigator instigator\)\{Play\(p,"exit",parentVelocity\);\}/);
  assert.match(adapter,/sourceRigidbodyVelocity=new\[\]\{sourceRigidbodyVelocity\.x,sourceRigidbodyVelocity\.y,sourceRigidbodyVelocity\.z\}/);
  assert.match(binding,/invalid source Rigidbody velocity input/);
  assert.match(renderer,/commandRecord\.attachedRigidbodyVelocity:value\.sourceRigidbodyVelocity/);
  assert.match(renderer,/Original particle InheritVelocity requires its source Rigidbody velocity input/);
});

test('missing or corrupted native material/particle manifest cannot fall back to invented assets',()=>{
  assert.throws(()=>createOriginalParticleEventBinding({...particles,effects:particles.effects.slice(1)},materials,controllers),
    /expected 14/);
  const invalid={...materials,effectsByMaterial:{...materials.effectsByMaterial}};
  invalid.effectsByMaterial[Object.keys(invalid.effectsByMaterial)[0]]=['unverified'];
  assert.throws(()=>createOriginalParticleEventBinding(particles,invalid,controllers),/linkage mismatch/);
});

test('particle binding refuses partial or mismatched source controller closure',()=>{
  assert.throws(()=>createOriginalParticleEventBinding(particles,materials),/controller closure/);
  const partial={...controllers,effects:controllers.effects.slice(1)};
  assert.throws(()=>createOriginalParticleEventBinding(particles,materials,partial),/controller/);
  const bad={...controllers,effects:controllers.effects.map((effect,i)=>i?effect:{...effect,sourcePrefabSha256:'f'.repeat(64)})};
  assert.throws(()=>createOriginalParticleEventBinding(particles,materials,bad),/provenance differs/);
});

test('controller closure is source-hash bound to all effect prefabs and recovered controller implementations',()=>{
  const bytes=readFileSync(new URL('../game/assets/ballistics/original-particle-controllers-source.json.gz',import.meta.url));
  assert.equal(createHash('sha256').update(bytes).digest('hex'),'0dec7b3eb1f7a20d95aa5ee58befbf0dd8226232ce4a0a795d41f5dc579382ce');
  assert.equal(controllers.sourcePackageSha256,'92e84fff24b79ae9b482cfe50dc33346fe8d5d7f2afb948e7bb4cacf5fb7dafb');
  assert.deepEqual(controllers.counts,{effects:14,BallisticHitEffect:14,SingleEmissionEffect:26});
  for(const [name,path] of Object.entries({BallisticHitEffect:'../runtime/Original/FullSource/BallisticHitEffect.cs',
    SingleEmissionEffect:'../runtime/Original/FullSource/SingleEmissionEffect.cs'})){
    const hash=createHash('sha256').update(readFileSync(new URL(path,import.meta.url))).digest('hex');
    assert.equal(hash,controllers.sourceFilesSha256[name]);
  }
  for(const effect of controllers.effects){
    const source=particles.effects.find(e=>e.guid===effect.guid);
    assert.equal(effect.sourcePrefabSha256,source.sourcePrefabSha256);
    const emitters=effect.controllers.filter(c=>c.scriptGuid==='7f9bb5f5ef1d9fb3ca6dbc9296820005');
    assert.equal(emitters.length,source.emitters.length);
    assert.ok(emitters.every(c=>c.particleSystemFileID&&c.particleRendererFileID&&c.serializedFields.entryRatio!==undefined));
  }
});
