const finite3=value=>Array.isArray(value)&&value.length===3&&value.every(Number.isFinite);
const finite4=value=>Array.isArray(value)&&value.length===4&&value.every(Number.isFinite);
const sourceQuaternionToThree=q=>[-q[0],-q[1],q[2],q[3]];

/** Bind recovered casing prefab visuals to source Rigidbody requests and the shared Mission PhysX scene. */
export function createOriginalCasingRuntime({THREE,source,physics,scene,normalTexture,occlusionTexture}){
 if(source?.format!=='original-unity-casing-mesh-v1'||!source.source?.hashes||!physics||!scene)
  throw new TypeError('Recovered original casing source, PhysX scene, and renderer are required');
 const meshData=source.mesh,collider=source.collider,materialData=source.material;
 if(!Array.isArray(meshData.positions)||!meshData.positions.length||meshData.positions.some(v=>!finite3(v))||
    !Array.isArray(meshData.normals)||meshData.normals.length!==meshData.positions.length||meshData.normals.some(v=>!finite3(v))||
    !Array.isArray(meshData.uv)||meshData.uv.length!==meshData.positions.length||meshData.uv.some(v=>!Array.isArray(v)||v.length!==2||!v.every(Number.isFinite))||
    !Array.isArray(meshData.indices)||meshData.indices.length%3||meshData.indices.some(i=>!Number.isInteger(i)||i<0||i>=meshData.positions.length))
  throw new Error('Recovered original casing mesh channels or indices are invalid');
 const geometry=new THREE.BufferGeometry();
 geometry.setAttribute('position',new THREE.Float32BufferAttribute(meshData.positions.flat(),3));
 geometry.setAttribute('normal',new THREE.Float32BufferAttribute(meshData.normals.flat(),3));
 geometry.setAttribute('uv',new THREE.Float32BufferAttribute(meshData.uv.flat(),2));
 geometry.setAttribute('uv2',new THREE.Float32BufferAttribute(meshData.uv.flat(),2));
 geometry.setIndex(meshData.indices);geometry.computeBoundingSphere();
 const brass=new THREE.MeshStandardMaterial({metalness:materialData.metallic,roughness:1-materialData.smoothness,
  normalMap:normalTexture,normalScale:new THREE.Vector2(materialData.normalScale,materialData.normalScale),
  aoMap:occlusionTexture,aoMapIntensity:materialData.occlusionStrength,side:THREE.DoubleSide});
 brass.color.setRGB(...materialData.colorLinear,THREE.LinearSRGBColorSpace);
 const meshTemplate=new THREE.Mesh(geometry,brass);meshTemplate.name='Original Casing 5.56x45mm MeshRenderer';
 const casings=new Map(),settled=new Map();
 function remove(id,{keepVisual=false}={}){
  const value=casings.get(id);if(!value)return;
  physics.removeDynamic(value.body);casings.delete(id);
  if(keepVisual){settled.set(id,value.visual);value.visual.userData.settled=true;}
  else scene.remove(value.visual);
 }
 function create(command){
  const id=command.body;if(!Number.isSafeInteger(id)||!finite3(command.sourcePosition)||!finite4(command.sourceRotation)||
     !Number.isFinite(command.mass)||Math.abs(command.mass-collider.massKg)>1e-6)
   throw new Error('Source casing Rigidbody identity, pose, or authored 2 g mass is invalid');
  if(casings.has(id)||settled.has(id))throw new Error('Duplicate source casing Rigidbody: '+id);
  const body=physics.createOriginalCasing({position:command.sourcePosition,rotation:command.sourceRotation,
   mass:command.mass,halfExtents:collider.size.map(x=>x/2),center:collider.center,
   linearDamping:collider.drag,angularDamping:collider.angularDrag});
  const visual=meshTemplate.clone();visual.material=brass;visual.name=command.name||'Casing 5.56x45mm';
  visual.userData={source:'FullSource.Casing',sourceBodyId:id,sourceMeshGuid:source.source.meshGuid};scene.add(visual);
  const value={id,body,visual};casings.set(id,value);return value;
 }
 function consume(commands=[]){
  if(!Array.isArray(commands))throw new TypeError('FullSource world commands must be an array');
  for(const command of commands){
   if(command.source!=='Casing')continue;
   let value=casings.get(command.body);
   if(command.operation==='force'){
    if(value)throw new Error('Original casing force spawned the same Rigidbody twice');
    value=create(command);
    physics.addDynamicForce(value.body,command.force,command.position,command.mode);
   }else if(command.operation==='torque'){
    if(!value)throw new Error('Original casing torque arrived before its source Rigidbody force');
    physics.addDynamicTorque(value.body,command.torque,command.mode);
   }else throw new Error('Unsupported source casing world command: '+command.operation);
  }
 }
 function syncSourceStates(sourceStates=[]){
  if(!Array.isArray(sourceStates))throw new TypeError('FullSource casing state list must be an array');
  const active=new Set();
  for(const sourceState of sourceStates){
   if(!Number.isSafeInteger(sourceState.body))throw new Error('Source casing state is missing its Rigidbody ID');
   const value=casings.get(sourceState.body);
   if(!value){if(settled.has(sourceState.body))continue;throw new Error('Source casing state has no browser PhysX actor: '+sourceState.body);}
   active.add(sourceState.body);
   const state={position:sourceState.position,rotation:sourceState.rotation,velocity:sourceState.velocity,angularVelocity:sourceState.angularVelocity};
   if(!finite3(state.position)||!finite4(state.rotation)||!finite3(state.velocity)||!finite3(state.angularVelocity))
    throw new Error('FullSource casing state contains invalid pose/velocity');
   physics.synchronizeDynamic(value.body,state);
   value.sourceSettled=sourceState.settled===true;
  }
  for(const id of casings.keys())if(!active.has(id)&&!sourceStates.some(v=>v.body===id))remove(id);
 }
 function stepVisuals(){
  const states=[];
  for(const [id,value] of casings){
   const state=physics.dynamicSnapshot(value.body),q=sourceQuaternionToThree(state.rotation);
   value.visual.position.set(state.position[0],state.position[1],-state.position[2]);
   value.visual.quaternion.set(...q);
   states.push({body:id,position:state.position,rotation:state.rotation,velocity:state.velocity,
    angularVelocity:state.angularVelocity,sleeping:state.sleeping});
  }
  return states;
 }
 function reconcile(sourceStates=[]){
  const sourceById=new Map(sourceStates.map(value=>[value.body,value]));
  for(const [id,value] of casings){
   const state=sourceById.get(id);
   if(!state)remove(id);
   else if(state.settled)remove(id,{keepVisual:true});
  }
  for(const id of settled.keys())if(!sourceById.has(id)){
   // Source destroyed the casing GameObject at its authored below-floor or
   // settled-in-container condition; remove the matching visual at the same time.
   const visual=settled.get(id);scene.remove(visual);settled.delete(id);
  }
 }
 return {consume,stepVisuals,syncSourceStates,reconcile,
  snapshot(){return {active:casings.size,settled:settled.size,bodyIds:[...casings.keys()]};},
  dispose(){for(const id of [...casings.keys()])remove(id);for(const visual of settled.values())scene.remove(visual);settled.clear();geometry.dispose();brass.dispose();}};
}
