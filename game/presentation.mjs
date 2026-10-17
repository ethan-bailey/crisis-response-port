import * as THREE from 'three';
import { createWeaponEffects } from './weapon-runtime.mjs';
const radians=Math.PI/180;
const worldPosition=node=>node.getWorldPosition(new THREE.Vector3());
const unityVector=v=>[v.x,v.y,-v.z];
const worldZ=node=>{const q=node.getWorldQuaternion(new THREE.Quaternion()),r=new THREE.Vector3(1,0,0).applyQuaternion(q),u=new THREE.Vector3(0,1,0).applyQuaternion(q);return Math.atan2(r.y,u.y)*180/Math.PI;};
function worldEuler(node,yaw,z){
 const desired=new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0,1,0),-yaw*radians).multiply(new THREE.Quaternion().setFromAxisAngle(new THREE.Vector3(0,0,1),z*radians));
 const parent=node.parent?.getWorldQuaternion(new THREE.Quaternion())||new THREE.Quaternion();node.quaternion.copy(parent.invert().multiply(desired));node.updateWorldMatrix(false,true);
}
export async function loadPresentationData(){
 const response=await fetch('./assets/presentation.json.gz');if(!response.ok)throw new Error('Original presentation settings missing');
 return new Response(response.body.pipeThrough(new DecompressionStream('gzip'))).json();
}
export function configureCamera(camera,data){
 const p=data.cameraTransform.m_LocalPosition;camera.position.set(p.x,p.y,-p.z);camera.rotation.set(0,0,0);
 camera.fov=data.camera['field of view'];camera.near=data.camera['near clip plane'];camera.far=data.camera['far clip plane'];camera.aspect=innerWidth/innerHeight;camera.updateProjectionMatrix();
}
export function createUpperBody(data,manifest,nodeById,character,weapon,runtimeCall,weaponData,scene,ballisticsData,sourceWindows,particleSources,armRig=null){
 const byTransform=new Map(manifest.nodes.map(n=>[n.transformFileID,nodeById.get(n.gameObjectFileID)]));
 const nodeForRef=ref=>nodeById.get(data.componentGameObjects[ref.fileID])||byTransform.get(ref.fileID);
 const torso=nodeForRef(data.body.Torso),head=nodeForRef(data.body.Head),neck=nodeForRef(data.body.Neck),abdomen=nodeForRef(data.body.Abdomen),eye=nodeForRef(data.body.Eye);
 for(const [id,active] of Object.entries(data.objectActive||{}))if(nodeById.has(id))nodeById.get(id).visible=active;
 const arms=data.arms.map(a=>{
  const upper=nodeById.get(a.bones.upperArm),lower=nodeById.get(a.bones.lowerArm),hand=nodeById.get(a.bones.hand);
  const length1=worldPosition(lower).distanceTo(worldPosition(upper)),length2=worldPosition(hand).distanceTo(worldPosition(lower))+.08;
  const right=a.bones.upperArm===data.componentGameObjects[data.body.RightUpperArm.fileID],mode=right?data.armConfiguration.rightHandMode:data.armConfiguration.leftHandMode;
  return {...a,upper,lower,hand,length1,length2,right};
 });
 const muzzle=weapon.nodesByPath.get('Transform/Slots/Muzzle');
 const anchors=[weapon.nodesByPath.get('Transform/Right hand anchor'),weapon.nodesByPath.get('Transform/Left hand anchor')];
 const ac=data.armConfiguration;
 anchors[0].position.set(ac.rightHandOffset.x,ac.rightHandOffset.y,0);anchors[1].position.set(ac.leftHandOffset.x,ac.leftHandOffset.y,0);
 const effects=createWeaponEffects(weaponData,scene,ballisticsData,sourceWindows,particleSources);
 const weaponWorld=()=>{const p=worldPosition(weapon.root),q=weapon.root.getWorldQuaternion(new THREE.Quaternion());return {p:unityVector(p),q:[-q.x,-q.y,q.z,q.w]};};
 const sourceNodes=new Map(manifest.nodes.map(n=>[n.transformFileID,nodeById.get(n.gameObjectFileID)]));
 let result=null,solutions=[],weaponResult=null;
 const api={
  shoulder:arms.find(a=>a.right).upper,
  async update({position,velocity,direction,moving,sprinting,kneeling,aim,aspect,fire,reload,physicsStep=1/60},camera){
   character.updateWorldMatrix(true,true);
   const frame={position:unityVector(position),velocity:[velocity,0,0],direction,eye:unityVector(worldPosition(eye)),torso:unityVector(worldPosition(torso)),muzzle:unityVector(worldPosition(muzzle)),shoulder:unityVector(worldPosition(api.shoulder)),
    weaponRight:unityVector(new THREE.Vector3(1,0,0).applyQuaternion(weapon.root.getWorldQuaternion(new THREE.Quaternion()))),moving,sprinting,kneeling,aim:unityVector(aim),aspect,arms:[]};
   result=JSON.parse(await runtimeCall('StepPresentation',JSON.stringify(frame)));
   const yaw=direction===1?0:180;
   worldEuler(torso,yaw,result.pose.torso);worldEuler(abdomen,yaw,0);worldEuler(head,yaw,result.pose.head);worldEuler(neck,yaw,result.pose.head);
   worldEuler(weapon.root,yaw,result.pose.weaponAngle);character.updateWorldMatrix(true,true);
   const operatorTransforms={};
   for(const [id,node] of sourceNodes){const p=worldPosition(node),q=node.getWorldQuaternion(new THREE.Quaternion());operatorTransforms[id]={p:unityVector(p),q:[-q.x,-q.y,q.z,q.w]};}
   weaponResult=JSON.parse(await runtimeCall('StepWeapons',JSON.stringify({position:unityVector(position),velocity:[velocity,0,0],direction,weaponPose:weaponWorld(),operatorTransforms,fire,reload,armAngles:armRig?.angles()??{}})));
   effects.update(weaponResult.events,weaponResult.magazinePose,weaponResult.sourceTime);
   // Original ArmIKSolver.Update now runs against the source C# bone hierarchy.
   // Apply its actual world rotations and original Hand.SetMode object states.
   const applySourceRotation=(node,pose)=>{
    const q=new THREE.Quaternion(-pose.q[0],-pose.q[1],pose.q[2],pose.q[3]);
    const parent=node.parent?.getWorldQuaternion(new THREE.Quaternion())||new THREE.Quaternion();
    node.quaternion.copy(parent.invert().multiply(q));node.updateWorldMatrix(false,true);
   };
   solutions=arms.map(a=>{const state=weaponResult[a.right?'right':'left'];return {solved:state.HasSolution,upper:state.upper,lower:state.lower};});
   arms.forEach(a=>{
    const state=weaponResult[a.right?'right':'left'];
    applySourceRotation(a.upper,state.bones.upper);applySourceRotation(a.lower,state.bones.lower);applySourceRotation(a.hand,state.bones.hand);
    for(const visual of state.handObjects){const node=nodeById.get(visual.id);if(!node)throw new Error('Original hand visual missing: '+visual.id);node.visible=visual.active;}
   });
   if(armRig){
    armRig.applySourceCommands(weaponResult.physicalArms.commands);
    armRig.syncKinematic();
    armRig.step(physicsStep);
    armRig.applyDynamicToCharacter();
   }
   const [x,y,z]=result.camera.position;camera.position.set(x,y,-z);camera.fov=result.camera.fov;camera.updateProjectionMatrix();
  },
  weaponSnapshot(){return weaponResult;},
  snapshot(){return {active:!!result,camera:result?.camera,pose:result?.pose,arms:solutions,originalIKUpdate:true,originalHandSetMode:true,originalPhysicalArmController:weaponResult?.physicalArms?.sourceControllerExecuted===true,physicalArmState:armRig?.snapshot()??null,originalFullBodySource:armRig?.sourceGraph()??null,worldHeadDirection:new THREE.Vector3(1,0,0).applyQuaternion(head.getWorldQuaternion(new THREE.Quaternion())).toArray(),worldShoulder:worldPosition(api.shoulder).toArray(),worldMuzzle:worldPosition(muzzle).toArray(),armTargets:anchors.map(a=>worldPosition(a).toArray()),originalMuzzleFlash:effects.flashSnapshot(),originalImpactMeshes:effects.impactMeshSnapshot(),originalWindows:effects.windowSnapshot(),originalParticleSource:effects.particleSourceSnapshot()};}
 };
 return api;
}

