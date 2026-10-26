// Source-backed event-to-prefab binding for recovered original Unity 2017
// ballistic ParticleSystems, plus validation of source ParticleSystem driver
// commands delivered by the FullSource actor. This module NEVER simulates or
// draws replacements.
// This source/provenance binder does not itself simulate or draw particles.
// Original C# materialEffect events and Time.time are authoritative; the
// separate renderer consumes its recovered controller and module mappings.
import {evaluateOriginalImpactEmission,evaluateSourceParticleFields} from './source-particle-values.mjs';
const GUID=/^[0-9a-f]{32}$/;
const SHA=/^[0-9a-f]{64}$/;
const MAX_DIAGNOSTICS=32;
const KNOWN_MESH_RENDER_MODES=new Set([0,1,4]);

function requireSource(ok,message) {
  if(!ok)throw new Error('Original particle source binding: '+message);
}
async function loadGzip(url) {
  const r=await fetch(url);
  if(!r.ok)throw new Error('Original particle source unavailable ('+r.status+'): '+url);
  return new Response(r.body.pipeThrough(new DecompressionStream('gzip'))).json();
}
export async function loadOriginalParticleSources() {
  const [particles,materials,controllers]=await Promise.all([
    loadGzip('./assets/ballistics/original-particles-source.json.gz'),
    loadGzip('./assets/ballistics/original-particle-materials-source.json.gz'),
    loadGzip('./assets/ballistics/original-particle-controllers-source.json.gz')
  ]);
  return {particles,materials,controllers};
}

export function createOriginalParticleEventBinding(particles,materials,controllers) {
  requireSource(particles?.version===1&&particles.effects?.length===14,
    'expected 14 recovered impact effect prefabs');
  requireSource(materials?.version===1&&
    Object.keys(materials.materials||{}).length===8,
    'expected eight recovered native particle materials');
  requireSource(Object.keys(materials.textures||{}).length===4,
    'expected four original particle textures');
  requireSource(materials.shaders&&
    Object.keys(materials.shaders).length===2,
    'source original particle shader provenance is absent');
  requireSource(SHA.test(materials.particleManifestSha256||''),
    'original particle manifest hash is absent');
  requireSource(controllers?.schemaVersion===1&&controllers.kind==='original-impact-particle-controller-closure'&&
    controllers.counts?.effects===14&&controllers.counts?.BallisticHitEffect===14&&
    controllers.counts?.SingleEmissionEffect===26,
    'original impact emission controller closure is missing or incomplete');
  requireSource(SHA.test(controllers.sourcePackageSha256||''),
    'original particle controller closure is not bound to its verified source package');

  const sourceEffects=new Map(),controllerEffects=new Map(),byMaterial=new Map();
  for(const effect of controllers.effects||[]) {
    requireSource(GUID.test(effect.guid)&&SHA.test(effect.sourcePrefabSha256||'')&&
      !controllerEffects.has(effect.guid),'duplicate or malformed original emission controller prefab');
    const root=effect.controllers?.filter(c=>c.scriptGuid==='f9f9f8d0f023b657c5276ea277044f46')||[];
    const emitters=effect.controllers?.filter(c=>c.scriptGuid==='7f9bb5f5ef1d9fb3ca6dbc9296820005')||[];
    requireSource(root.length===1&&emitters.length>0&&emitters.every(c=>c.particleSystemFileID&&c.particleRendererFileID),
      'original emission controller is not paired to its particle system/renderer: '+effect.guid);
    controllerEffects.set(effect.guid,effect);
  }
  let systemCount=0,billboards=0,meshes=0;
  for(const source of particles.effects) {
    requireSource(GUID.test(source.guid)&&!sourceEffects.has(source.guid),
      'duplicate or malformed original particle prefab GUID');
    requireSource(SHA.test(source.sourcePrefabSha256||'')&&
      source.emitters?.length>0, 'unverified prefab or empty emitters: '+source.guid);
    const controllerEffect=controllerEffects.get(source.guid);
    requireSource(controllerEffect&&controllerEffect.sourcePrefabSha256===source.sourcePrefabSha256,
      'original particle and spawn-controller prefab provenance differs: '+source.guid);
    requireSource(Array.isArray(source.roles)&&
      source.roles.every(role=>role==='entryEffect'||role==='exitEffect'),
      'unsupported original effect phase: '+source.guid);
    const unique=new Set();
    for(const e of source.emitters) {
      systemCount++;
      requireSource(!unique.has(e.sourceGameObject),
        'duplicate native emitter transform in prefab: '+source.guid);
      unique.add(e.sourceGameObject);
      requireSource(SHA.test(e.originalEmitterFieldSha256||'')&&
        SHA.test(e.originalRendererFieldSha256||''),
        'missing original emitter/renderer file provenance: '+source.guid);
      requireSource(e.enabledModules?.InitialModule?.enabled===1&&
        e.enabledModules?.EmissionModule?.enabled===1&&
        e.enabledModules?.ShapeModule?.enabled===1,
        'original required emission modules changed: '+source.guid);
      requireSource(Array.isArray(e.disabledModules)&&e.disabledModules.every(
        key=>!Object.hasOwn(e.enabledModules,key)),
        'disabled original particle module was incorrectly enabled');
      const mode=e.renderer?.m_RenderMode;
      requireSource(KNOWN_MESH_RENDER_MODES.has(mode),
        'unknown native ParticleSystemRenderer mode: '+mode);
      if(mode===0||mode===1)billboards++;else meshes++;
      for(const ref of (e.renderer?.m_Materials||[])) {
        const material=ref.guid;
        if(!material)continue;
        requireSource(GUID.test(material)&&Object.hasOwn(materials.materials,material),
          'unrecovered native ParticleSystemRenderer material GUID');
        if(!byMaterial.has(material))byMaterial.set(material,new Set());
        byMaterial.get(material).add(source.guid);
      }
    }
    const controllerEmitters=controllerEffect.controllers.filter(c=>c.scriptGuid==='7f9bb5f5ef1d9fb3ca6dbc9296820005');
    requireSource(controllerEmitters.length===source.emitters.length&&source.emitters.every(e=>
      controllerEmitters.some(c=>String(c.particleSystemFileID)===String(e.sourceEmitterFileID)&&
        String(c.particleRendererFileID)===String(e.sourceRendererFileID))),
      'original spawn-controller component mapping differs from particle manifest: '+source.guid);
    sourceEffects.set(source.guid,source);
  }
  requireSource(systemCount===26&&billboards===14&&meshes===12,
    'recovered Unity effect component/mode coverage differs');
  for(const [guid,sourceEffectsExpected] of Object.entries(materials.effectsByMaterial||{})) {
    const actual=byMaterial.get(guid);
    requireSource(actual&&actual.size===sourceEffectsExpected.length&&
      sourceEffectsExpected.every(id=>actual.has(id)),
      'native material-to-prefab linkage mismatch: '+guid);
  }
  requireSource(byMaterial.size===8, 'not all original effect materials are bound');

  let received=0,unrendered=0,sourceCommands=0,sourceCommandDrops=0;
  const sourceCommandActions={emit:0,simulate:0,play:0};
  const recent=[];
  const recentCommands=[];
  function resolveEmitters(event) {
    if(event?.type!=='materialEffect')return null;
    const value=event.value;
    const source=sourceEffects.get(value?.source);
    requireSource(source,'C# materialEffect references unknown recovered prefab: '+value?.source);
    const requiredRole=value?.phase==='entry'?'entryEffect':
      value?.phase==='exit'?'exitEffect':null;
    requireSource(requiredRole&&source.roles.includes(requiredRole),
      'C# source event phase does not match prefab roles: '+source.guid);
    requireSource(Number.isFinite(event.time),
      'C# impact event lacks authoritative Unity Time.time');
    requireSource(Array.isArray(value.position)&&value.position.length===3&&
      value.position.every(Number.isFinite),'invalid native effect world position');
    requireSource(Array.isArray(value.direction)&&value.direction.length===3&&
      value.direction.every(Number.isFinite),'invalid native effect world direction');
    return source.emitters;
  }
  function sampleSourceEffect(sourceGuid,normalizedAge,random={}) {
    const source=sourceEffects.get(sourceGuid);
    requireSource(source,'cannot sample an unrecovered original prefab: '+sourceGuid);
    return source.emitters.map(emitter=>({
      sourceEmitterFileID:emitter.sourceEmitterFileID,
      sourceRendererFileID:emitter.sourceRendererFileID,
      renderMode:emitter.renderer.m_RenderMode,
      materialGuids:(emitter.renderer.m_Materials||[]).map(ref=>ref.guid).filter(Boolean),
      controller:controllerEffects.get(sourceGuid).controllers.find(c=>c.scriptGuid==='7f9bb5f5ef1d9fb3ca6dbc9296820005'&&
        String(c.particleSystemFileID)===String(emitter.sourceEmitterFileID)).serializedFields,
      fields:evaluateSourceParticleFields(emitter,normalizedAge,random)
    }));
  }
  function evaluateImpactEffect(event,settings={}) {
    const emitters=resolveEmitters(event);
    if(!emitters)return [];
    const source=sourceEffects.get(event.value.source);
    const controllers=controllerEffects.get(source.guid).controllers;
    return emitters.map(emitter=>{
      const controller=controllers.find(c=>c.scriptGuid==='7f9bb5f5ef1d9fb3ca6dbc9296820005'&&
        String(c.particleSystemFileID)===String(emitter.sourceEmitterFileID));
      requireSource(controller,'original SingleEmissionEffect controller is missing for '+emitter.sourceEmitterFileID);
      return {sourceGuid:source.guid,sourceEmitterFileID:emitter.sourceEmitterFileID,
        hierarchyPath:controller.hierarchyPath,renderer:emitter.renderer,
        emission:evaluateOriginalImpactEmission(controller,event.value,event.value.phase,settings)};
    });
  }
  function observe(events,sourceTime) {
    for(const event of events) {
      const emitters=resolveEmitters(event);
      if(!emitters)continue;
      requireSource(Number.isFinite(sourceTime)&&event.time<=sourceTime+0.0001,
        'original event occurs after simulation-clock snapshot');
      received++;
      // Explicitly record missing renderer support. Never fake emitted sprites.
      unrendered+=emitters.length;
      recent.push({source:event.value.source,phase:event.value.phase,
        sourceTime:event.time,emitterCount:emitters.length});
      if(recent.length>MAX_DIAGNOSTICS)recent.shift();
    }
  }
  function observeSourceCommands(events,sourceTime,droppedEvents=0) {
    requireSource(Array.isArray(events),'FullSource particle command snapshot is not an array');
    requireSource(Number.isFinite(sourceTime),'FullSource particle command snapshot lacks Unity Time.time');
    requireSource(Number.isInteger(droppedEvents)&&droppedEvents>=0,
      'FullSource particle dropped-event count is invalid');
    sourceCommandDrops=Math.max(sourceCommandDrops,droppedEvents);
    for(const event of events) {
      requireSource(event?.type==='particleCommand','unexpected FullSource particle command type');
      requireSource(Number.isFinite(event.time)&&event.time<=sourceTime+0.0001,
        'FullSource particle command occurs after simulation-clock snapshot');
      requireSource(GUID.test(event.sourceGuid||'')&&/^\d+$/.test(String(event.sourceFileId||''))&&
        typeof event.sourcePath==='string'&&event.sourcePath.startsWith('Assets/')&&
        SHA.test(event.sourceAssetSha256||''),
        'FullSource particle command is missing original component provenance');
      requireSource(event.sourceFields&&typeof event.sourceFields==='object'&&
        Object.keys(event.sourceFields).length>0,
        'FullSource particle command is missing the original ParticleSystem module fields');
      const renderer=event.renderer;
      requireSource(renderer&&GUID.test(renderer.sourceGuid||'')&&
        /^\d+$/.test(String(renderer.sourceFileId||''))&&
        typeof renderer.sourcePath==='string'&&renderer.sourcePath===event.sourcePath&&
        SHA.test(renderer.sourceAssetSha256||'')&&renderer.sourceAssetSha256===event.sourceAssetSha256&&
        renderer.fields&&typeof renderer.fields==='object'&&Object.keys(renderer.fields).length>0,
        'FullSource particle command is missing the original ParticleSystemRenderer fields');
      requireSource(typeof event.hierarchyPath==='string'&&event.hierarchyPath.length>0&&
        Array.isArray(event.position)&&event.position.length===3&&event.position.every(Number.isFinite)&&
        Array.isArray(event.rotation)&&event.rotation.length===4&&event.rotation.every(Number.isFinite),
        'FullSource particle command has invalid hierarchy or world pose');
      const command=event.command;
      requireSource(command&&Object.hasOwn(sourceCommandActions,command.action),
        'unknown FullSource particle command action');
      if(command.action==='emit') requireSource(Number.isInteger(command.count)&&command.count>=0&&
        Number.isInteger(command.maxParticles)&&command.maxParticles>=0&&command.count<=command.maxParticles&&
        Array.isArray(command.startSpeed)&&command.startSpeed.length===2&&command.startSpeed.every(Number.isFinite)&&
        Array.isArray(command.startLifetime)&&command.startLifetime.length===2&&command.startLifetime.every(Number.isFinite)&&
        Number.isFinite(command.startSizeMultiplier)&&Number.isInteger(command.randomSeed)&&
        command.randomSeed>=0&&command.randomSeed<=0xffffffff,
        'FullSource Emit command has invalid count, source state, or random seed');
      if(command.action==='simulate') requireSource(Number.isFinite(command.simulationTime)&&
        typeof command.withChildren==='boolean'&&typeof command.restart==='boolean'&&
        typeof command.fixedTimeStep==='boolean','FullSource Simulate command is malformed');
      sourceCommands++;
      sourceCommandActions[command.action]++;
      recentCommands.push({action:command.action,source:event.sourceGuid,
        component:event.sourceFileId,path:event.sourcePath,time:event.time});
      if(recentCommands.length>MAX_DIAGNOSTICS)recentCommands.shift();
    }
  }
  function snapshot() {
    return {sourcePrefabs:sourceEffects.size,sourceEmitters:systemCount,
      sourceEmissionControllers:controllers.counts.SingleEmissionEffect,
      billboardEmitters:billboards,meshEmitters:meshes,
      receivedEvents:received,unrenderedEmitterRequests:unrendered,
      sourceCommands,sourceCommandDrops,sourceCommandActions:{...sourceCommandActions},
      recentSourceCommands:recentCommands.slice(),
      renderedParticles:0,sourceVisualParity:false,
      blocker:'Native Unity particle modules and shaders not yet executed',
      recent:recent.slice()};
  }
  return {resolveEmitters,sampleSourceEffect,evaluateImpactEffect,observe,observeSourceCommands,snapshot};
}
