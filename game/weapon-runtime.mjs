import * as THREE from 'three';
export async function loadWeaponData(){
 const r=await fetch('./assets/weapon-runtime.json.gz');if(!r.ok)throw new Error('Original weapon settings unavailable');
 return new Response(r.body.pipeThrough(new DecompressionStream('gzip'))).json();
}
export function createWeaponEffects(data,scene){
 // Original sampled audio, never synthesized substitutes. Unlock Web Audio on the device gesture.
 const AudioContext=window.AudioContext||window.webkitAudioContext,context=AudioContext?new AudioContext():null,buffers=new Map();
 if(context)for(const [name,info] of Object.entries(data.audio))fetch(info.url).then(r=>r.arrayBuffer()).then(b=>context.decodeAudioData(b)).then(b=>buffers.set(name,b)).catch(()=>{});
 addEventListener('pointerdown',()=>context?.resume().catch(()=>{}),{passive:true});
 const texturePromise=new THREE.TextureLoader().loadAsync(Object.values(data.flash.textures)[0].url).then(t=>{t.colorSpace=THREE.SRGBColorSpace;return t;});
 const flashNodes=data.flash.prefab.filter(r=>r.classID===4),clip=data.flash.animation[0].fields,curve=clip.m_PPtrCurves[0].curve;
 const duration=clip.m_AnimationClipSettings.m_StopTime,active=[];
 function flash(event){
  const group=new THREE.Group(),root=flashNodes.find(n=>n.fields.m_Father.fileID==='0'),child=flashNodes.find(n=>n!==root),f=child.fields;
  const holder=new THREE.Group(),q=f.m_LocalRotation;holder.quaternion.set(-q.x,-q.y,q.z,q.w);holder.scale.set(f.m_LocalScale.x,f.m_LocalScale.y,f.m_LocalScale.z).multiplyScalar(data.weapon.muzzleFlashScale);group.add(holder);
  const m=new THREE.MeshBasicMaterial({transparent:true,depthWrite:false,side:THREE.DoubleSide,alphaTest:.001});const mesh=new THREE.Mesh(new THREE.PlaneGeometry(1,1),m);holder.add(mesh);
  const p=event.muzzle,r=event.rotation;group.position.set(p[0],p[1],-p[2]);group.quaternion.set(-r[0],-r[1],r[2],r[3]);scene.add(group);
  const item={group,mesh,material:m,start:performance.now(),texture:null};active.push(item);texturePromise.then(t=>{item.texture=t.clone();item.texture.needsUpdate=true;m.map=item.texture;m.needsUpdate=true;});
 }
 function update(events){
  for(const e of events){
   if(e.type==='audio'&&context&&buffers.has(e.value)){const src=context.createBufferSource(),gain=context.createGain();src.buffer=buffers.get(e.value);gain.gain.value=data.audio[e.value].volume;src.connect(gain).connect(context.destination);src.onended=()=>{src.disconnect();gain.disconnect();};src.start();}
   if(e.type==='shot')flash(e.value);
  }
  for(let i=active.length-1;i>=0;i--){const item=active[i],time=(performance.now()-item.start)/1000;
   if(time>=duration){scene.remove(item.group);item.mesh.geometry.dispose();item.material.dispose();item.texture?.dispose();active.splice(i,1);continue;}
   if(!item.texture)continue;
   let frame=curve[0];for(const key of curve)if(key.time<=time)frame=key;
   const sprite=data.flash.sprites[frame.value.guid][0].fields,rect=sprite.m_RD.textureRect,ppu=sprite.m_PixelsToUnits,pivot=sprite.m_Pivot,tex=item.texture;
   tex.repeat.set(rect.width/tex.image.width,rect.height/tex.image.height);tex.offset.set(rect.x/tex.image.width,rect.y/tex.image.height);
   item.mesh.scale.set(rect.width/ppu,rect.height/ppu,1);item.mesh.position.set((.5-pivot.x)*rect.width/ppu,(.5-pivot.y)*rect.height/ppu,0);
  }
 }
 return {update};
}
