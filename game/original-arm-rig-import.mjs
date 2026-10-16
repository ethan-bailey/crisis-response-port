// Serialized Unity source -> pinned PhysX engine boundary. No replacement arm
// solver, controller, force rules, or authored masses.
import * as THREE from 'three';
import {createArmPhysicsKernel} from './arm-physics-kernel.mjs';
export const originalArmRigFormat='crisis-original-arm-physics-runtime-v1';
const V=p=>new THREE.Vector3(p.x,p.y,p.z);
const Q=q=>new THREE.Quaternion(q.x,q.y,q.z,q.w);
const xyz=v=>v.toArray();
const xyzw=q=>q.toArray();
const LOCAL_X_TO_UNITY_HINGE_Z=new THREE.Quaternion().setFromUnitVectors(
  new THREE.Vector3(1,0,0),new THREE.Vector3(0,0,1));
const unityPosition=node=>{
 const p=node.getWorldPosition(new THREE.Vector3());
 return new THREE.Vector3(p.x,p.y,-p.z);
};
const unityRotation=node=>{
 const q=node.getWorldQuaternion(new THREE.Quaternion());
 return new THREE.Quaternion(-q.x,-q.y,q.z,q.w);
};
export async function createOriginalArmRig(source,nodeById,character,{moduleOptions={}}={}){
 if(source?.format!==originalArmRigFormat)throw Error('Original arm source required');
 const prefab=source.prefabs.find(p=>p.name==='Operator');
 if(!prefab)throw Error('Original Operator physics not recovered');
 const records=new Map(prefab.records.map(r=>[r.fileID,r]));
 const goByTransform=new Map(prefab.records.filter(r=>r.classID===4)
   .map(r=>[r.fileID,r.fields.m_GameObject.fileID]));
 const transformByGo=new Map([...goByTransform].map(([t,g])=>[g,t]));
 const sourceNode=transform=>nodeById.get(goByTransform.get(transform));
 const physics=source.physics[0];
 const kernel=await createArmPhysicsKernel({moduleOptions,gravity:
   [physics.m_Gravity.x,physics.m_Gravity.y,physics.m_Gravity.z]});
 const bodies=new Map(),joints=new Map(),arms=[];
 const pose=transform=>{
   const node=sourceNode(transform);
   if(!node)throw Error('Source physics Transform missing from character: '+transform);
   node.updateWorldMatrix(true,false);
   return {position:xyz(unityPosition(node)),rotation:xyzw(unityRotation(node))};
 };
 function bodyFromId(rigidbodyId){
   if(bodies.has(rigidbodyId))return bodies.get(rigidbodyId);
   const rigidbody=records.get(rigidbodyId);
   if(rigidbody?.classID!==54)throw Error('Missing original Rigidbody: '+rigidbodyId);
   const go=rigidbody.fields.m_GameObject.fileID;
   const collider=prefab.records.find(r=>r.classID===136&&r.fields.m_GameObject.fileID===go);
   if(!collider||!collider.fields.m_Enabled||collider.fields.m_IsTrigger)
     throw Error('Original active capsule collider required for '+go);
   const transform=transformByGo.get(go),w=pose(transform),c=collider.fields,b=rigidbody.fields;
   const actor=kernel.createCapsule({...w,mass:b.m_Mass,
     linearDamping:b.m_Drag,angularDamping:b.m_AngularDrag,
     isKinematic:!!b.m_IsKinematic,useGravity:!!b.m_UseGravity,
     constraints:b.m_Constraints,radius:c.m_Radius,height:c.m_Height,
     direction:c.m_Direction,center:xyz(V(c.m_Center))});
   const value={actor,transform,gameObject:go,rigidbodyId,colliderId:collider.fileID};
   bodies.set(rigidbodyId,value);
   return value;
 }
 try{
   // Every original prefab Rigidbody is present in the source, not just the
   // six hand/arm bones. The Operator ROOT Rigidbody is already simulated by
   // the live contact-physics actor, so it is not duplicated in this PhysX
   // sub-scene. All 19 limb actors plus source helmet enter this rig.
   const sourceBodyRecords=prefab.records.filter(r=>r.classID===54);
   const sourceRoot=sourceBodyRecords.find(r=>{
     const go=prefab.records.find(x=>x.classID===1&&x.fileID===r.fields.m_GameObject.fileID);
     return go?.fields.m_Name===prefab.name;
   });
   if(!sourceRoot)throw Error('Original Operator root contact body is missing');
   const sourceRootId=sourceRoot.fileID;
   for(const rb of sourceBodyRecords)if(rb.fileID!==sourceRootId)bodyFromId(rb.fileID);
   const sourceHinges=prefab.records.filter(r=>r.classID===59);
   if(bodies.size!==20||sourceHinges.length!==18)
     throw Error('Incomplete original Operator limb/helmet PhysX actor graph');
   // Preserve the two original ArmController chains as interface views over
   // the same full original body/joint graph.
   for(const [side,arm] of Object.entries(prefab.arms)){
     const chain=arm.limbs.map(limb=>({limb,...bodyFromId(limb.rigidbody)}));
     arms.push({side,chain,mode:arm.controller.mode});
   }
   for(const record of sourceHinges){
     const j=record.fields,go=j.m_GameObject.fileID;
     const transform=transformByGo.get(go);
     if(!transform)throw Error('Original hinge GameObject has no Transform');
     const connectedId=j.m_ConnectedBody?.fileID;
     const movingRb=sourceBodyRecords.find(x=>x.fields.m_GameObject.fileID===go);
     if(!movingRb||!connectedId)
       throw Error('Original hinge body connection is missing');
     const moving=bodyFromId(movingRb.fileID),connected=bodyFromId(connectedId);
     const movingNode=sourceNode(transform),parentNode=sourceNode(connected.transform);
     // Derive authored Unity hinge frames before the PhysX coordinate reflection.
     const movingQ=unityRotation(movingNode),connectedQ=unityRotation(parentNode);
     const worldQ=movingQ.clone().multiply(LOCAL_X_TO_UNITY_HINGE_Z);
     const localMoving=LOCAL_X_TO_UNITY_HINGE_Z.clone();
     const localParent=connectedQ.clone().invert().multiply(worldQ);
     const anchor=V(j.m_Anchor);
     const pivot=unityPosition(movingNode).add(anchor.clone().applyQuaternion(movingQ));
     const connectedAnchor=j.m_AutoConfigureConnectedAnchor
       ?pivot.sub(unityPosition(parentNode)).applyQuaternion(connectedQ.invert())
       :V(j.m_ConnectedAnchor);
     const hinge=kernel.createHinge(moving.actor,connected.actor,{
       anchor:xyz(anchor),connectedAnchor:xyz(connectedAnchor),
       rotation:xyzw(localMoving),connectedRotation:xyzw(localParent),
       limits:j.m_UseLimits?j.m_Limits:null,enableCollision:!!j.m_EnableCollision});
     hinge.setSpring(j.m_Spring,!!j.m_UseSpring);
     joints.set(record.fileID,hinge);
   }
   for(const arm of arms)for(const entry of arm.chain){
     entry.joint=joints.get(entry.limb.joint);
     if(!entry.joint)throw Error('Original source arm joint absent in complete graph');
   }
   return {
     kernel,arms,bodies,joints,
     syncKinematic(){
       // Only the original IK output determines the kinematic arm poses.
       for(const body of bodies.values())if(body.actor.isKinematic){
         const w=pose(body.transform);
         kernel.syncKinematicPose(body.actor,w.position,w.rotation);
       }
     },
     angles(){return Object.fromEntries([...joints].map(([key,joint])=>[key,joint.angle()]));},
     applySourceCommands(commands){
       for(const command of commands){
         if(command.type==='kinematic') {
           const body=bodies.get(command.body);
           if(!body)throw Error('Original arm kinematic command references unknown body');
           kernel.setKinematic(body.actor,command.value);
         } else if(command.type==='gravity'){
           const body=bodies.get(command.body);
           if(!body)throw Error('Original limb gravity command references unknown actor');
           kernel.setGravity(body.actor,command.value);
         } else if(command.type==='drag'){
           const body=bodies.get(command.body);
           if(!body)throw Error('Original limb drag command references unknown actor');
           kernel.setLinearDamping(body.actor,command.value);
         } else if(command.type==='force'){
           const body=bodies.get(command.body);
           if(!body)throw Error('Original arm force references unknown body');
           kernel.addForceAtPosition(body.actor,command.force,command.position,command.mode);
         } else if(command.type==='spring'){
           const joint=joints.get(command.joint);
           if(!joint)throw Error('Original arm spring references unknown hinge');
           joint.setSpring({spring:command.spring,damper:command.damper,
             targetPosition:command.targetPosition},command.enabled);
         } else throw Error('Unexpected original physical arm command');
       }
     },
     setDynamic(bodyId,dynamic){
       const body=bodies.get(bodyId);
       if(!body)throw Error('Unknown original limb Rigidbody');
       kernel.setKinematic(body.actor,!dynamic);
     },
     addForce(bodyId,force,position,mode){
       const body=bodies.get(bodyId);
       if(!body)throw Error('Unknown original force recipient');
       kernel.addForceAtPosition(body.actor,force,position,mode);
     },
     step(dt){kernel.step(dt);},
     applyDynamicToCharacter(){
       for(const body of bodies.values()){
         if(body.actor.isKinematic)continue;
         const node=sourceNode(body.transform),parent=node.parent;
         const state=kernel.snapshot(body.actor);
         const worldPosition=new THREE.Vector3(state.position[0],
           state.position[1],-state.position[2]);
         const worldRotation=new THREE.Quaternion(-state.rotation[0],
           -state.rotation[1],state.rotation[2],state.rotation[3]);
         if(parent) {
           parent.updateWorldMatrix(true,false);
           node.position.copy(parent.worldToLocal(worldPosition));
           node.quaternion.copy(parent.getWorldQuaternion(
             new THREE.Quaternion()).invert().multiply(worldRotation));
         }else{node.position.copy(worldPosition);node.quaternion.copy(worldRotation);}
         node.updateWorldMatrix(false,true);
       }
     },
     sourceGraph(){return {sourceBodyCount:sourceBodyRecords.length,
        actorCount:bodies.size,hingeCount:joints.size,rootActorAuthority:'contact-physics',
        boneIds:[...bodies.keys()]};},
     snapshot(){
       return arms.map(arm=>({side:arm.side,chain:arm.chain.map(item=>({
         transform:item.transform,body:item.rigidbodyId,
         ...kernel.snapshot(item.actor)
       }))}));
     },
     dispose(){kernel.dispose();}
   };
 }catch(error){kernel.dispose();throw error;}
}
