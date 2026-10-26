import * as THREE from 'three';
import {evaluateSourceParticleFields} from './source-particle-values.mjs';
import {sampleRecoveredParticleShape} from './source-particle-shapes.mjs';

const TEXTURES='./assets/ballistics/';
const MESHES='./assets/ballistics/particle-meshes/particle-mesh-';
const MAX_ACTIVE=2048,MAX_PENDING=32;
const v3=v=>new THREE.Vector3(v[0],v[1],-v[2]);
function visualRandom(event,emitter,index,sourceSeed=null){
  let x=((sourceSeed===null?0:sourceSeed>>>0))^Math.imul(index+1,0x9e3779b1);
  if(sourceSeed===null)x^=(Math.trunc(Number(event.time)*1e6))^Number(String(emitter.sourceEmitterFileID).slice(-8));
  if(sourceSeed===null)for(const c of String(event.value.source))x=Math.imul(x^c.charCodeAt(0),16777619);
  x^=x>>>16;x=Math.imul(x,0x7feb352d);x^=x>>>15;x=Math.imul(x,0x846ca68b);x^=x>>>16;
  const next=()=>{x=(Math.imul(x,1664525)+1013904223)|0;return (x>>>0)/4294967296;};
  return {radius:next(),arc:next(),hemisphereZ:next(),hemisphereArc:next(),
    directionZ:next(),directionArc:next(),
    startSize:next(),startSpeed:next(),startLifetime:next(),startRotation:next(),color:next(),
    sizeOverLifetime:next(),rotationOverLifetime:next()};
}

// Displays source-linked impact particle assets in WebGL. The original asset
// mappings and CPU fields are used; native Unity shader equivalence is not
// claimed until the recovered shader source is available.
export function createOriginalParticleRenderer(scene,sources,binder,settings={}) {
  const effects=new Map(sources.particles.effects.map(effect=>[effect.guid,effect]));
  const textureLoader=new THREE.TextureLoader(),textures=new Map(),geometries=new Map(),active=[];
  let disposed=false,loadError=null,assetsReady=false,emitted=0,dropped=0,sourceSeededEmitters=0,seedFallbackEmitters=0,lastSourceTime=0;
  const pending=[];
  const sourceEmissions=new Map();

  const referencedTextures=new Set();
  for(const material of Object.values(sources.materials.materials)){
    const tex=material.originalSavedProperties?.m_TexEnvs?._MainTex?.m_Texture?.guid;
    if(tex&&sources.materials.textures[tex])referencedTextures.add(tex);
  }
  const referencedMeshes=new Set();
  for(const effect of sources.particles.effects)for(const emitter of effect.emitters){
    const mesh=emitter.renderer?.m_Mesh?.guid;if(mesh)referencedMeshes.add(mesh);
  }
  const loading=Promise.all([
    ...[...referencedTextures].map(async guid=>{
      const source=sources.materials.textures[guid];
      const texture=await textureLoader.loadAsync(source.url.startsWith('./')?source.url:TEXTURES+source.url);
      texture.colorSpace=THREE.SRGBColorSpace;textures.set(guid,texture);
    }),
    ...[...referencedMeshes].map(async guid=>{
      const response=await fetch(MESHES+guid+'.json');if(!response.ok)throw new Error(`Original particle mesh ${guid} HTTP ${response.status}`);
      const data=await response.json();
      if(data.sourceGuid!==guid||!data.mesh?.positions?.length||!data.mesh?.indices?.length)
        throw new Error('Original particle mesh metadata or buffers are invalid: '+guid);
      const geometry=new THREE.BufferGeometry();
      geometry.setAttribute('position',new THREE.Float32BufferAttribute(data.mesh.positions.flat(),3));
      geometry.setIndex(data.mesh.indices);
      if(data.mesh.normals?.length)geometry.setAttribute('normal',new THREE.Float32BufferAttribute(data.mesh.normals.flat(),3));
      else geometry.computeVertexNormals();
      if(data.mesh.uv?.length)geometry.setAttribute('uv',new THREE.Float32BufferAttribute(data.mesh.uv.flat(),2));
      geometries.set(guid,geometry);
    })
  ]).then(()=>{
    if(disposed)return;
    assetsReady=true;
    for(const event of pending.splice(0))spawnImpact(event,lastSourceTime);
  }).catch(error=>{loadError=String(error?.message||error);console.warn('Original particle assets unavailable:',loadError);});

  function materialFor(guid,renderer) {
    const source=sources.materials.materials[guid];
    if(!source)throw new Error('Original particle material is missing: '+guid);
    const saved=source.originalSavedProperties||{},colors=saved.m_Colors||{},floats=saved.m_Floats||{};
    const tint=colors._TintColor||colors._Color||{r:1,g:1,b:1,a:1};
    const textureGuid=saved.m_TexEnvs?._MainTex?.m_Texture?.guid;
    const texture=textures.get(textureGuid);
    const opacity=Math.max(0,Math.min(1,tint.a??1));
    const base={color:new THREE.Color().setRGB(tint.r,tint.g,tint.b),transparent:true,
      opacity,depthWrite:false,side:THREE.DoubleSide,
      userData:{sourceOpacity:opacity,sourceMaterialGuid:guid}};
    if(renderer.m_RenderMode===0)return new THREE.SpriteMaterial({...base,map:texture||null,rotation:0});
    return new THREE.MeshBasicMaterial({...base,map:texture||null});
  }
  function disposeParticle(particle){
    scene.remove(particle.object);particle.material.dispose();particle.object=null;
  }
  function spawnImpact(event,sourceTime){
    const value=event.value,source=effects.get(value.source);
    if(!source)return;
    const plans=binder.evaluateImpactEffect(event,settings);
    for(let index=0;index<plans.length;index++){
      const plan=plans[index],emitter=source.emitters.find(e=>String(e.sourceEmitterFileID)===String(plan.sourceEmitterFileID));
      const max=emitter.enabledModules.InitialModule.maxNumParticles||1000;
      const commandRecord=sourceEmissions.get(`${source.guid}:${emitter.sourceEmitterFileID}`);
      const hasSourceCommand=!!commandRecord&&Math.abs(event.time-commandRecord.time)<=.5;
      const requested=hasSourceCommand?commandRecord.count:plan.emission.count;
      const count=Math.min(requested,max,MAX_ACTIVE-active.length);
      if(count<requested)dropped+=requested-count;
      const ref=(emitter.renderer.m_Materials||[]).find(r=>r.guid)?.guid;
      if(!ref||!plan.emission.count||!count)continue;
      // Impact renderers currently have recovered controllers only. Their
      // authored ShapeModule remains in the source manifest, but until its
      // source sampler is implemented, count each origin-only particle as a
      // known approximation instead of reporting false shape coverage.
      const mode=emitter.renderer.m_RenderMode,meshGuid=emitter.renderer.m_Mesh?.guid;
      const geometry=mode===4?geometries.get(meshGuid):null;
      if(mode===4&&!geometry)throw new Error('Original source particle mesh is unavailable: '+meshGuid);
      const material=materialFor(ref,emitter.renderer);
      const base=v3(value.position),direction=v3(value.direction).normalize();
      const rootRotation=new THREE.Quaternion().setFromUnitVectors(new THREE.Vector3(0,0,1),direction);
      const local=emitter.localTransform;
      const emitterRotation=new THREE.Quaternion(-local.rotation.x,-local.rotation.y,local.rotation.z,local.rotation.w);
      const emitterWorldRotation=rootRotation.clone().multiply(emitterRotation);
      const emitterOffset=v3([local.position.x,local.position.y,local.position.z]).applyQuaternion(rootRotation);
      const shape=emitter.enabledModules.ShapeModule;
      const shapeRotation=new THREE.Quaternion().setFromEuler(new THREE.Euler(
        -shape.m_Rotation.x*Math.PI/180,-shape.m_Rotation.y*Math.PI/180,shape.m_Rotation.z*Math.PI/180,'XYZ'));
      const fullShapeRotation=emitterWorldRotation.clone().multiply(shapeRotation);
      const shapeScale=shape.m_Scale;
      const shapeOrigin=v3([shape.m_Position.x,shape.m_Position.y,shape.m_Position.z]).applyQuaternion(emitterWorldRotation);
      base.add(emitterOffset).add(shapeOrigin);
      const sourceSeed=hasSourceCommand?commandRecord.randomSeed:null;
      const speedRange=hasSourceCommand?commandRecord.startSpeed:plan.emission.speedRange;
      const lifetimeRange=hasSourceCommand?commandRecord.startLifetime:null;
      const sizeMultiplier=hasSourceCommand?commandRecord.startSizeMultiplier:plan.emission.sizeMultiplier;
      if(hasSourceCommand)sourceSeededEmitters++;else seedFallbackEmitters++;
      for(let i=0;i<count;i++){
        const random=visualRandom(event,emitter,i,sourceSeed);
        const shapeSample=sampleRecoveredParticleShape(shape,random);
        const particleBase=base.clone().add(v3(shapeSample.position.map((n,axis)=>n*shapeScale[['x','y','z'][axis]])).applyQuaternion(fullShapeRotation));
        const particleDirection=v3(shapeSample.direction).applyQuaternion(fullShapeRotation).normalize();
        const authored=evaluateSourceParticleFields(emitter,0,random);
        const speed=speedRange[0]+(speedRange[1]-speedRange[0])*random.startSpeed;
        const lifetime=lifetimeRange
          ?lifetimeRange[0]+(lifetimeRange[1]-lifetimeRange[0])*random.startLifetime
          :plan.emission.lifetime;
        const velocity=particleDirection.multiplyScalar(speed);
        const object=mode===0?new THREE.Sprite(material.clone()):new THREE.Mesh(geometry,material.clone());
        const particle={object,material:object.material,geometry,emitter,sourceGuid:source.guid,
          start:event.time,lifetime,base:particleBase,velocity,
          initialSize:Math.max(.001,authored.size*sizeMultiplier),rotation:authored.rotation,
          random};
        object.name=`Original ${mode===0?'billboard':'mesh'} particle ${plan.sourceEmitterFileID}`;
        object.userData.originalParticleSource={prefab:source.guid,emitter:plan.sourceEmitterFileID,
          material:ref,mode,sourceHash:source.sourcePrefabSha256};
        scene.add(object);active.push(particle);emitted++;
        updateParticle(particle,sourceTime);
      }
    }
  }
  function updateParticle(particle,sourceTime){
    const age=Math.max(0,sourceTime-particle.start),life=Math.max(.001,particle.lifetime),t=Math.min(1,age/life);
    const sample=evaluateSourceParticleFields(particle.emitter,t,particle.random);
    particle.object.position.copy(particle.base).addScaledVector(particle.velocity,age);
    const size=Math.max(.001,particle.initialSize*sample.size/Math.max(.001,evaluateSourceParticleFields(particle.emitter,0,
      particle.random).size));
    particle.object.scale.set(size,size,size);
    particle.object.rotation.z=sample.rotation;
    particle.material.color.setRGB(sample.color.r,sample.color.g,sample.color.b);
    particle.material.opacity=(particle.material.userData?.sourceOpacity??1)*sample.color.a;
  }
  function update(events,sourceTime){
    if(disposed)return;
    if(!Number.isFinite(sourceTime))throw new Error('Original particle renderer requires source Unity Time.time');
    lastSourceTime=sourceTime;
    if(!assetsReady){
      for(const event of events)if(event.type==='materialEffect'){
        if(pending.length>=MAX_PENDING){pending.shift();dropped++;}
        pending.push(event);
      }
    }else for(const event of events)if(event.type==='materialEffect')spawnImpact(event,sourceTime);
    for(let i=active.length-1;i>=0;i--){const particle=active[i];
      if(sourceTime-particle.start>=particle.lifetime){disposeParticle(particle);active.splice(i,1);continue;}
      updateParticle(particle,sourceTime);
    }
  }
  function observeCommands(events){
    for(const event of events||[])if(event?.type==='particleCommand'&&event.command?.action==='emit'&&
      Number.isInteger(event.command.randomSeed)&&event.command.randomSeed>=0&&
      /^\d+$/.test(String(event.sourceFileId||'')))
      sourceEmissions.set(`${event.sourceGuid}:${event.sourceFileId}`,{
        randomSeed:event.command.randomSeed>>>0,count:event.command.count,time:event.time,
        startSpeed:event.command.startSpeed,startLifetime:event.command.startLifetime,
        startSizeMultiplier:event.command.startSizeMultiplier});
  }
  function snapshot(){return {loaded:loadError===null&&textures.size===referencedTextures.size&&
      [...referencedMeshes].every(guid=>geometries.has(guid)),assetError:loadError,
    sourceMaterialCount:Object.keys(sources.materials.materials).length,sourceTextureCount:referencedTextures.size,
    sourceMeshCount:referencedMeshes.size,sourceEmitters:effects.size?26:0,
    renderedParticles:active.length,emittedParticles:emitted,droppedParticles:dropped,
    shapeSampledParticles:emitted,sourceSeededEmitters,seedFallbackEmitters,sourceVisualParity:false,
    blocker:'The emitter seed is transported when available, but Unity random-stream reproduction, native shader behavior and browser acceptance are not yet verified'};}
  function dispose(){disposed=true;for(const p of active)disposeParticle(p);active.length=0;
    for(const texture of textures.values())texture.dispose();for(const geometry of geometries.values())geometry.dispose();}
  return {loading,update,observeCommands,snapshot,dispose};
}
