import { loadSourceCutoutMaterials, sourceCutoutMaterial } from './source-cutout-materials.mjs';
import * as THREE from 'three';
import { createOriginalImpactMeshRenderer } from './original-impact-mesh.mjs';
import { sourceEffectAge } from './source-effect-clock.mjs';
import { createOriginalWindowVisuals } from './original-window-visuals.mjs';
import { createOriginalParticleEventBinding } from './original-particle-event-binding.mjs';
import { createOriginalParticleRenderer } from './original-particle-renderer.mjs';
import { createWeaponAudioRuntime, createSourceWeaponAudioDispatcher, preloadWeaponAudio } from './weapon-audio-runtime.mjs';
import { createWeaponAudioStats } from './weapon-audio-stats.mjs';
export async function loadWeaponData(){
 const r=await fetch('./assets/weapon-runtime.json.gz');if(!r.ok)throw new Error('Original weapon settings unavailable');
 const data=await new Response(r.body.pipeThrough(new DecompressionStream('gzip'))).json();
 // Start downloading and decoding the original gunshot clip as soon as the
 // small weapon manifest is available. Waiting until the full character,
 // physics, and presentation graph is ready made the first shots silent.
 Object.defineProperty(data,'audioRuntime',{value:createWeaponAudioRuntime(data.audio),enumerable:false});
 const [,renderMaterials]=await Promise.all([preloadWeaponAudio(data.audio,data.audioRuntime),loadSourceCutoutMaterials()]);
 data.renderMaterials=renderMaterials;return data;
}
export function createWeaponEffects(data,scene,ballisticsData,sourceWindows,particleSources,runtimeCall){
 const audio=data.audioRuntime||createWeaponAudioRuntime(data.audio);
 for(const info of Object.values(ballisticsData?.effects||{}))for(const url of info.audio)audio.load(url,url);
 const audioStats=createWeaponAudioStats(audio);
 const fetchOriginalSourceAudio=async path=>JSON.parse(await runtimeCall('GetOriginalAudioAssetData',path));
 const reportOriginalAudioState=async(path,state)=>JSON.parse(await runtimeCall('SetOriginalAudioLoadState',path,state));
 const reportOriginalAudioPlaying=async(sourceId,isPlaying)=>JSON.parse(await runtimeCall('SetBrowserAudioSourcePlaying',sourceId,isPlaying));
 const dispatchSourceAudio=createSourceWeaponAudioDispatcher(data.audio,audio,fetchOriginalSourceAudio,reportOriginalAudioState,reportOriginalAudioPlaying);
 const originalImpactMeshes=createOriginalImpactMeshRenderer(scene);
 const originalWindows=createOriginalWindowVisuals(scene,ballisticsData,sourceWindows);
 const originalParticleEvents=createOriginalParticleEventBinding(particleSources.particles,particleSources.materials,particleSources.controllers);
 const originalParticleRenderer=createOriginalParticleRenderer(scene,particleSources,originalParticleEvents,{
  particlesLifetime:ballisticsData.settings.particlesLifetime,emissionsModifier:1,chunkSizeMultiplier:1});
 // Preload and retain the exact original SpriteAtlas texture. The old code
 // deferred assigning a clone using Promise.then for EVERY shot. Because its
 // short original animation is timed by C# Time.time, the new flash could age
 // out before the next WASM/browser frame applied its very first sprite.
 let loadedFlashTexture=null,flashTextureError=null;
 const texturePromise=new THREE.TextureLoader().loadAsync(Object.values(data.flash.textures)[0].url)
  .then(t=>{t.colorSpace=THREE.SRGBColorSpace;loadedFlashTexture=t;return t;})
  .catch(error=>{flashTextureError=String(error?.message||error);return null;});
 const flashNodes=data.flash.prefab.filter(r=>r.classID===4),clip=data.flash.animation[0].fields,curve=clip.m_PPtrCurves[0].curve;
 const magRoot=new THREE.Group();scene.add(magRoot);const magNodes=new Map();
 for(const n of data.magazineVisual.nodes){const g=new THREE.Group();g.position.fromArray(n.position);g.quaternion.fromArray(n.rotation);g.scale.fromArray(n.scale);magNodes.set(n.gameObjectFileID,g);}
 for(const n of data.magazineVisual.nodes){const g=magNodes.get(n.gameObjectFileID);(magNodes.get(n.parentGameObjectFileID)||magRoot).add(g);}
 for(const r of data.magazineVisual.renderers)new THREE.TextureLoader().loadAsync('./assets/weapon-runtime/M4-magazine/'+r.texture).then(t=>{t.colorSpace=THREE.SRGBColorSpace;const m=sourceCutoutMaterial(t,data.renderMaterials.magazine);const mesh=new THREE.Mesh(new THREE.PlaneGeometry(1,1),m);magNodes.get(r.gameObjectFileID).add(mesh);});
 const duration=clip.m_AnimationClipSettings.m_StopTime,active=[];
 let shotsReceived=0,sourceFramesApplied=0,firstFramesApplied=0,maxDrawable=0;
 function attachOriginalTexture(item,t) {
  if(item.disposed||!t)return;
  item.texture=t.clone();
  item.texture.needsUpdate=true;
  item.material.map=item.texture;
  item.material.needsUpdate=true;
 }
 function applyOriginalSpriteFrame(item,time) {
  if(!item.texture)return;
  let frame=curve[0];for(const key of curve)if(key.time<=time)frame=key;
  const sprite=data.flash.sprites[frame.value.guid][0].fields,
        rect=sprite.m_RD.textureRect,ppu=sprite.m_PixelsToUnits,
        pivot=sprite.m_Pivot,tex=item.texture;
  tex.repeat.set(rect.width/tex.image.width,rect.height/tex.image.height);
  tex.offset.set(rect.x/tex.image.width,rect.y/tex.image.height);
  item.mesh.scale.set(rect.width/ppu,rect.height/ppu,1);
  item.mesh.position.set((.5-pivot.x)*rect.width/ppu,(.5-pivot.y)*rect.height/ppu,0);
  if(!item.firstFrameApplied){item.firstFrameApplied=true;firstFramesApplied++;}
  item.originalSpriteGuid=frame.value.guid;
  sourceFramesApplied++;
 }
 function flash(event,sourceTime){
  const group=new THREE.Group(),root=flashNodes.find(n=>n.fields.m_Father.fileID==='0'),child=flashNodes.find(n=>n!==root),f=child.fields;
  const holder=new THREE.Group(),q=f.m_LocalRotation;holder.quaternion.set(-q.x,-q.y,q.z,q.w);holder.scale.set(f.m_LocalScale.x,f.m_LocalScale.y,f.m_LocalScale.z).multiplyScalar(data.weapon.muzzleFlashScale);group.add(holder);
  const m=new THREE.MeshBasicMaterial({transparent:true,depthWrite:false,side:THREE.DoubleSide,alphaTest:.001});const mesh=new THREE.Mesh(new THREE.PlaneGeometry(1,1),m);holder.add(mesh);
  const p=event.muzzle,r=event.rotation;group.position.set(p[0],p[1],-p[2]);group.quaternion.set(-r[0],-r[1],r[2],r[3]);scene.add(group);
  group.name='Original M4 source muzzle-flash animation';
  mesh.name='Original muzzle-flash atlas sprite';
  const item={group,mesh,material:m,start:sourceTime,texture:null,disposed:false,firstFrameApplied:false,originalSpriteGuid:null};
  active.push(item);shotsReceived++;
  // Texture preloaded already: apply the first original sprite synchronously
  // THIS frame, before THREE renders. No invented duration or replacement art.
  if(loadedFlashTexture)attachOriginalTexture(item,loadedFlashTexture);
  else texturePromise.then(t=>{if(!item.disposed)attachOriginalTexture(item,t);});
 }
 function update(events,magazinePose,sourceTime,camera){
  if(magazinePose){const {p,q,visible}=magazinePose;magRoot.visible=visible;magRoot.position.set(p[0],p[1],-p[2]);magRoot.quaternion.set(-q[0],-q[1],q[2],q[3]);}
  for(const e of events){
   const eventAge=sourceTime-Number(e.time);
   if(e.type==='audio')audio.play(e.value,(data.audio[e.value]?.volume??1)*(e.value==='audioFire'?0.6:1),eventAge);
   if(e.type==='materialEffect')audio.play(e.value.clip,e.value.volume,eventAge);
   if(e.type==='shot')flash(e.value,e.time);
  }
  originalImpactMeshes.update(events);
  originalWindows.update(events);
  originalParticleEvents.observe(events,sourceTime);
  originalParticleRenderer.update(events,sourceTime,camera);
  for(let i=active.length-1;i>=0;i--){const item=active[i],time=sourceEffectAge(sourceTime,item.start);
   if(time>=duration){item.disposed=true;scene.remove(item.group);item.mesh.geometry.dispose();item.material.dispose();item.texture?.dispose();active.splice(i,1);continue;}
   applyOriginalSpriteFrame(item,time);
  }
  maxDrawable=Math.max(maxDrawable,active.filter(item=>item.firstFrameApplied).length);
 }
 function flashSnapshot(){
  return {textureLoaded:!!loadedFlashTexture,textureError:flashTextureError,
    sourceAnimationDuration:duration,shotsReceived,sourceFramesApplied,
    firstFramesApplied,maxDrawable,active:active.length,
    activeDrawable:active.filter(item=>item.firstFrameApplied).length,
    currentSourceSpriteGuid:active.find(item=>item.firstFrameApplied)?.originalSpriteGuid||null};
 }
 function audioSnapshot(){return audioStats.snapshot();}
 return {update,playSourceAudioEvents:dispatchSourceAudio,
  updateSourceAudioMixer:(configuration,events,sourceTime)=>audioStats.updateSourceAudioMixer(configuration,events,sourceTime),
  updateSourceAudioStates:(states,sourceTime)=>audioStats.updateSourceAudioStates(states,sourceTime),
  observeSourceParticleCommands:(events,sourceTime,dropped)=>{
   originalParticleEvents.observeSourceCommands(events,sourceTime,dropped);
   originalParticleRenderer.observeCommands(events);
  },
  flashSnapshot,audioSnapshot,impactMeshSnapshot:originalImpactMeshes.snapshot,windowSnapshot:originalWindows.snapshot,
  particleSourceSnapshot:()=>({...originalParticleEvents.snapshot(),renderer:originalParticleRenderer.snapshot()}),
  dispose:originalParticleRenderer.dispose};
}
