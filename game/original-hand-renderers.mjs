// Render only source-authored hand Quad MeshRenderers omitted by LimbRenderer export.
// Original custom shader was recovered as a dummy: shading parity is not claimed.
import * as THREE from 'three';
export async function createOriginalHandRenderers(data,nodeById,loadTexture){
 if(data.format!=='crisis-original-hand-renderers-v1')throw new Error('Unsupported original hand renderer data');
 const entries=[];
 for(const source of data.renderers){
  const node=nodeById.get(source.gameObjectFileID);
  if(!node)throw new Error('Original hand node missing: '+source.gameObjectFileID);
  if(source.mesh.fileID!=='10210'||source.mesh.guid!=='0000000000000000e000000000000000')throw new Error('Unsupported hand mesh; no substitute generated');
  const texture=(await loadTexture('./assets/operator/'+source.texture)).clone();
  const saved=source.material.m_SavedProperties,main=saved.m_TexEnvs._MainTex,color=saved.m_Colors._Color;
  texture.repeat.set(main.m_Scale.x,main.m_Scale.y);texture.offset.set(main.m_Offset.x,main.m_Offset.y);texture.needsUpdate=true;
  const material=new THREE.MeshBasicMaterial({map:texture,color:new THREE.Color(color.r,color.g,color.b),
   opacity:color.a,alphaTest:saved.m_Floats._Cutoff,side:THREE.DoubleSide,
   transparent:false,depthWrite:!!saved.m_Floats._ZWrite});
  const mesh=new THREE.Mesh(new THREE.PlaneGeometry(1,1),material);
  mesh.name='Original source hand: '+source.name;mesh.visible=!!source.renderer.m_Enabled;
  mesh.userData.originalHandRenderer=source.rendererFileID;
  mesh.userData.originalHandGameObject=source.gameObjectFileID;
  // Authored transform is already on the source node, including hand depth.
  // Use the same renderer ordering as existing source limb skin transport.
  mesh.renderOrder=1;node.add(mesh);const entry={node,mesh,source,draws:0};mesh.onAfterRender=()=>{entry.draws++;};entries.push(entry);
 }
 return {
  snapshot(){return {sourceRenderers:entries.length,loadedTextures:entries.length,uniqueSourceTextures:Object.keys(data.textures).length,drawnRenderers:entries.filter(e=>e.draws>0).length,drawCalls:entries.reduce((n,e)=>n+e.draws,0),
   visibleRenderers:entries.filter(({mesh})=>{let node=mesh;while(node){if(!node.visible)return false;node=node.parent;}return true;}).length,
   sourceShaderParity:false,objects:entries.map(({mesh,source,draws})=>({draws,gameObject:source.gameObjectFileID,renderer:source.rendererFileID,textureSha256:source.textureSha256,attached:!!mesh.parent}))};},
  dispose(){for(const {mesh} of entries){mesh.removeFromParent();mesh.geometry.dispose();mesh.material.map.dispose();mesh.material.dispose();}}
 };
}
