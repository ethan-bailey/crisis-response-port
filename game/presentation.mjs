import * as THREE from 'three';
const radians=Math.PI/180;
const worldPosition=node=>node.getWorldPosition(new THREE.Vector3());
const unityVector=v=>[v.x,v.y,-v.z];
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
export function createUpperBody(data,manifest,nodeById,character,weapon,runtimeCall){
 const byTransform=new Map(manifest.nodes.map(n=>[n.transformFileID,nodeById.get(n.gameObjectFileID)]));
 const nodeForRef=ref=>nodeById.get(data.componentGameObjects[ref.fileID])||byTransform.get(ref.fileID);
 const torso=nodeForRef(data.body.Torso),head=nodeForRef(data.body.Head),neck=nodeForRef(data.body.Neck),abdomen=nodeForRef(data.body.Abdomen),eye=nodeForRef(data.body.Eye);
 for(const [id,active] of Object.entries(data.objectActive||{}))if(nodeById.has(id))nodeById.get(id).visible=active;
 const arms=data.arms.map(a=>{
  const upper=nodeById.get(a.bones.upperArm),lower=nodeById.get(a.bones.lowerArm),hand=nodeById.get(a.bones.hand);
  const length1=worldPosition(lower).distanceTo(worldPosition(upper)),length2=worldPosition(hand).distanceTo(worldPosition(lower))+.08;
  const right=a.bones.upperArm===data.componentGameObjects[data.body.RightUpperArm.fileID],mode=right?data.armConfiguration.rightHandMode:data.armConfiguration.leftHandMode;
  const states=mode===0?[['forwardAssist',false],['fist',false],['trigger',true]]:mode===1?[['forwardAssist',false],['trigger',false],['fist',true]]:[['fist',false],['trigger',false],['forwardAssist',true]];
  for(const [key,enabled] of states)nodeById.get(a.handModes[key]).visible=enabled;
  return {...a,upper,lower,hand,length1,length2,right};
 });
 const muzzle=weapon.nodesByPath.get('Transform/Slots/Muzzle');
 const anchors=[weapon.nodesByPath.get('Transform/Right hand anchor'),weapon.nodesByPath.get('Transform/Left hand anchor')];
 const ac=data.armConfiguration;
 anchors[0].position.set(ac.rightHandOffset.x,ac.rightHandOffset.y,0);anchors[1].position.set(ac.leftHandOffset.x,ac.leftHandOffset.y,0);
 let result=null,solutions=[];
 const api={
  shoulder:arms.find(a=>a.right).upper,
  async update({position,velocity,direction,moving,sprinting,kneeling,aim,aspect},camera){
   character.updateWorldMatrix(true,true);
   const frame={position:unityVector(position),velocity:[velocity,0,0],direction,eye:unityVector(worldPosition(eye)),torso:unityVector(worldPosition(torso)),muzzle:unityVector(worldPosition(muzzle)),shoulder:unityVector(worldPosition(api.shoulder)),
    weaponRight:unityVector(new THREE.Vector3(1,0,0).applyQuaternion(weapon.root.getWorldQuaternion(new THREE.Quaternion()))),moving,sprinting,kneeling,aim:unityVector(aim),aspect,arms:[]};
   result=JSON.parse(await runtimeCall('StepPresentation',JSON.stringify(frame)));
   const yaw=direction===1?0:180;
   worldEuler(torso,yaw,result.pose.torso);worldEuler(abdomen,yaw,0);worldEuler(head,yaw,result.pose.head);worldEuler(neck,yaw,result.pose.head);
   worldEuler(weapon.root,yaw,result.pose.weaponAngle);character.updateWorldMatrix(true,true);
   const inputs=arms.map(a=>{
    const p=worldPosition(a.upper),target=worldPosition(anchors[a.right?0:1]);
    return {length1:a.length1,length2:a.length2,x:(target.x-p.x)*direction,y:target.y-p.y,flip:!!a.settings.flip};
   });
   solutions=JSON.parse(await runtimeCall('SolvePresentationArms',JSON.stringify(inputs)));
   arms.forEach((a,i)=>{
    worldEuler(a.upper,yaw,solutions[i].upper);worldEuler(a.lower,yaw,solutions[i].lower);
    a.hand.quaternion.setFromAxisAngle(new THREE.Vector3(0,0,1),(a.right?ac.rightHandAngle:ac.leftHandAngle)*radians);
   });
   const [x,y,z]=result.camera.position;camera.position.set(x,y,-z);camera.fov=result.camera.fov;camera.updateProjectionMatrix();
  },
  snapshot(){return {active:!!result,camera:result?.camera,pose:result?.pose,arms:solutions,worldShoulder:worldPosition(api.shoulder).toArray(),worldMuzzle:worldPosition(muzzle).toArray(),armTargets:anchors.map(a=>worldPosition(a).toArray())};}
 };
 return api;
}
