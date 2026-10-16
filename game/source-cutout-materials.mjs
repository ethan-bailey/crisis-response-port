// Original alpha cutoff and depth writes; custom Unity shader shading is unported.
import * as THREE from 'three';
let pending;const bound=[];
export const sourceCutoutSnapshot=()=>({bound:bound.length,depthWriting:bound.filter(m=>m.depthWrite).length,alphaTested:bound.filter(m=>m.alphaTest===.5&&!m.transparent).length,nativeShaderParity:false});
export function loadSourceCutoutMaterials(){
 return pending??=fetch('./assets/source-cutout-materials.json').then(async r=>{
  if(!r.ok)throw new Error('Original depth material settings unavailable');
  const data=await r.json();if(data.format!=='original-source-cutout-v1')throw new Error('Unsupported original depth materials');return data;
 });
}
export function sourceCutoutMaterial(texture,source){
 if(!source||!Number.isFinite(source.cutoff)||typeof source.depthWrite!=='boolean')throw new Error('Original cutout material missing');
 const c=source.color;
 const material=new THREE.MeshBasicMaterial({map:texture,transparent:false,alphaTest:source.cutoff,depthWrite:source.depthWrite,side:THREE.DoubleSide,color:new THREE.Color(c.r,c.g,c.b),opacity:c.a});bound.push(material);return material;
}
