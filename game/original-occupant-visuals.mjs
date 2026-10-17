import * as THREE from 'three';
import {sourceCutoutMaterial} from './source-cutout-materials.mjs';
export async function loadOriginalOccupantMaterials(){
 const res=await fetch('./assets/occupant/source-materials.json');
 if(!res.ok)throw Error('Original Occupant materials missing');
 const source=await res.json();
 if(source.format!=='original-occupant-source-materials-v1'||
    Object.keys(source.materials||{}).length!==13)
  throw Error('Original Occupant renderer material inventory invalid');
 for(const [guid,m] of Object.entries(source.materials)){
  if(!/^[0-9a-f]{32}$/.test(guid)||!m.texturePath?.startsWith('./assets/occupant/skins/')||
     !/^[0-9a-f]{64}$/.test(m.sha256)||m.cutoff!==0.5||
     m.shaderGuid!=='dfda7bb4fc1132e4b8c3293964087ef2')
   throw Error('Unverified original Occupant material: '+guid);
 }
 return source.materials;
}


// Import the real Occupant prefab's authored Unity hierarchy, 20 MeshRenderers,
// and LimbRenderer texture GUIDs. Spawn position, prefab skin RNG, AI and
// hit callbacks are deliberate requirements of the caller, never guessed here.
export async function createOriginalOccupantVisual(source, materialRecords, {
  selectOriginalSkin, loadOriginalTexture
}={}) {
 if(source?.format!=='crisis-source-occupant-visual-spawn-v1')
  throw Error('Expected recovered original Occupant prefab visual manifest');
 if(source.nodes.length!==56||source.renderers.length!==20||
    source.dynamicLimbSkins.length!==16)
  throw Error('Incomplete original Occupant render graph');
 if(typeof selectOriginalSkin!=='function'||typeof loadOriginalTexture!=='function')
  throw Error('Original skin RNG selection and original PNG texture loader must be bound');
 const roots=[],byTransform=new Map(),byGameObject=new Map(),sourceMaterials=new Map();
 const skinByGO=new Map(source.dynamicLimbSkins.map(x=>[x.gameObjectFileID,x]));
 for(const node of source.nodes){
  const v=new THREE.Group();v.name=node.name;
  const p=node.localPosition,q=node.localRotation,s=node.localScale;
  v.position.set(p.x,p.y,-p.z);v.quaternion.set(-q.x,-q.y,q.z,q.w);
  v.scale.set(s.x,s.y,s.z);
  byTransform.set(node.transformFileID,v);
  byGameObject.set(node.gameObjectFileID,v);
 }
 for(const node of source.nodes){
  const v=byTransform.get(node.transformFileID);
  if(node.parentTransformFileID==='0')roots.push(v);
  else {
   const parent=byTransform.get(node.parentTransformFileID);
   if(!parent)throw Error('Original Occupant parent Transform missing: '+node.parentTransformFileID);
   parent.add(v);
  }
 }
 if(roots.length!==1)throw Error('Expected one original Occupant prefab root');
 const skinBindings=new Map();
 const renderers=[];
 for(const renderer of source.renderers){
  const m=renderer.mesh;
  if(m.fileID!==10210||m.guid!=='0000000000000000e000000000000000')
   throw Error('Non-quad original Occupant mesh requires authentic source geometry');
  const skin=skinByGO.get(renderer.gameObjectFileID);
  if(!skin) {
   // Static variants include hands without LimbRenderer; texture must be
   // resolved via their original serialized source material, not invented.
   const key=renderer.materials[0]?.guid;
   const record=materialRecords?.[key];
   if(!record)throw Error('Original static hand material unresolved: '+key);
   const tex=await loadOriginalTexture(record.texturePath);
   const material=sourceCutoutMaterial(tex,record);
   const mesh=new THREE.Mesh(new THREE.PlaneGeometry(1,1),material);
   mesh.name='Original Occupant source renderer '+renderer.componentFileID;
   mesh.visible=renderer.enabled;
   byGameObject.get(renderer.gameObjectFileID).add(mesh);
   renderers.push(mesh);continue;
  }
  const index=selectOriginalSkin(skin.skinCandidates.length,skin.componentFileID);
  if(!Number.isInteger(index)||index<0||index>=skin.skinCandidates.length)
   throw Error('Original skin selection outside authored candidate range');
  const guid=skin.skinCandidates[index].guid;
  const sourcePath=source.guidPaths[guid];
  if(!sourcePath||!sourcePath.startsWith('Texture2D/'))
   throw Error('Original Occupant skin GUID missing: '+guid);
  const texture=await loadOriginalTexture('./assets/occupant/skins/'+sourcePath.slice(10));
  if(!texture)throw Error('Missing original Occupant skin texture '+guid);
  const matGuid=renderer.materials[0]?.guid;
  const sourceMaterial=materialRecords?.[matGuid];
  if(!sourceMaterial)throw Error('Original Occupant renderer source material unavailable: '+matGuid);
  const mesh=new THREE.Mesh(new THREE.PlaneGeometry(1,1),sourceCutoutMaterial(texture,sourceMaterial));
  mesh.name='Original Occupant source renderer '+renderer.componentFileID;
  mesh.visible=renderer.enabled;
  byGameObject.get(renderer.gameObjectFileID).add(mesh);
  renderers.push(mesh);
  skinBindings.set(renderer.gameObjectFileID,{guid,limbRendererComponentFileID:skin.componentFileID,
    dynamicMaterialGuid:skin.dynamicMaterial.guid,mesh});
 }
 return {root:roots[0],byTransform,byGameObject,renderers,skinBindings,
   sourcePrefabSha256:source.sourcePrefabSha256,
   dispose(){for(const mesh of renderers){mesh.geometry.dispose();mesh.material.dispose();}}};
}
