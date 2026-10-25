// Engine adapter: actual PhysX contact solving, using original extracted data.
// PhysX 5.6.1 is not Unity 2017's PhysX build: full solver parity is unverified.
import PhysX from './vendor/physx-js-webidl.mjs';
import { advanceOriginalKneelRatio } from './source-character-step.mjs';
export async function createContactPhysics(data,{x=-3,y=1.5,z=1.5}={},options={}) {
 const P=await PhysX(options), version=P.PHYSICS_VERSION;
 const allocator=new P.PxDefaultAllocator(),errors=new P.PxDefaultErrorCallback();
 const foundation=P.CreateFoundation(version,allocator,errors),scale=new P.PxTolerancesScale();
 const physics=P.CreatePhysics(version,foundation,scale),desc=new P.PxSceneDesc(scale);
 const vec=(a)=>new P.PxVec3(...a), gravity=data.settings.m_Gravity;
 const g=vec([gravity.x,gravity.y,gravity.z]);desc.set_gravity(g);
 const dispatcher=P.DefaultCpuDispatcherCreate(0);desc.set_cpuDispatcher(dispatcher);desc.set_filterShader(P.DefaultFilterShader());
 desc.set_bounceThresholdVelocity(data.settings.m_BounceThreshold);
 // Keep the legacy PGS solver / patch friction, rather than selecting newer TGS.
 desc.set_solverType(P.PxSolverTypeEnum.ePGS);desc.set_frictionType(P.PxFrictionTypeEnum.ePATCH);
 const scene=physics.createScene(desc),materials=new Map();
 const combine=[P.PxCombineModeEnum.eAVERAGE,P.PxCombineModeEnum.eMULTIPLY,P.PxCombineModeEnum.eMIN,P.PxCombineModeEnum.eMAX];
 for(const [id,m] of Object.entries(data.materials)){
  const mat=physics.createMaterial(m.staticFriction,m.dynamicFriction,m.bounciness);
  mat.setFrictionCombineMode(combine[m.frictionCombine]);mat.setRestitutionCombineMode(combine[m.bounceCombine]);materials.set(id,mat);
 }
 const casingMaterial=physics.createMaterial(0.1,0.1,0.35);
 casingMaterial.setFrictionCombineMode(P.PxCombineModeEnum.eAVERAGE);
 casingMaterial.setRestitutionCombineMode(P.PxCombineModeEnum.eAVERAGE);
 const flags=new P.PxShapeFlags(P.PxShapeFlagEnum.eSCENE_QUERY_SHAPE|P.PxShapeFlagEnum.eSIMULATION_SHAPE);
 const filter=new P.PxFilterData(1,1,0,0);
 function pose(position,rotation=[0,0,0,1]){
  const v=vec(position),q=new P.PxQuat(...rotation),t=new P.PxTransform(v,q);P.destroy(v);P.destroy(q);return t;
 }
 function attach(actor,geometry,mat,local=null){
  const shape=physics.createShape(geometry,materials.get(mat),true,flags);
  shape.setSimulationFilterData(filter);shape.setContactOffset(data.settings.m_DefaultContactOffset);
  if(local)shape.setLocalPose(local);actor.attachShape(shape);shape.release();P.destroy(geometry);return shape;
 }
 const statics=[],dynamics=new Set();
 for(const box of data.boxes){
  const t=pose(box.position,box.rotation),actor=physics.createRigidStatic(t);P.destroy(t);
  attach(actor,new P.PxBoxGeometry(...box.halfExtents),box.material);scene.addActor(actor);statics.push(actor);
 }
 const t=pose([x,y,z]),body=physics.createRigidDynamic(t);P.destroy(t);
 const original=data.character.body,capsule=data.character.capsule;
 // CharacterBody.UpdateAnimation changes the prefab capsule's center and height.
 const offset=data.character.movement.colliderVerticalOffset;
 let shape, kneelRatio=0, previousKneeling=null,previousFacing=null;
 const dimensions=(kneeling,facing)=>{
  const height=kneeling?1.25:1,center=[(kneeling?-.15:0)*facing,offset,0];
  const local=pose(center);if(shape){body.detachShape(shape);shape.release();}
  const geom=new P.PxCapsuleGeometry(capsule.m_Radius,Math.max(0,height/2-capsule.m_Radius));
  // PhysX capsules use local X, matching the original m_Direction: 0.
  shape=physics.createShape(geom,materials.get(data.character.material),true,flags);
  shape.setSimulationFilterData(filter);shape.setContactOffset(data.settings.m_DefaultContactOffset);shape.setLocalPose(local);body.attachShape(shape);
  P.destroy(local);P.destroy(geom);previousKneeling=kneeling;previousFacing=facing;
 };
 if(capsule.m_Direction!==0)throw new Error('Unsupported original capsule axis');
 dimensions(false,1);
 P.PxRigidBodyExt.prototype.setMassAndUpdateInertia(body,original.m_Mass);
 body.setLinearDamping(original.m_Drag);body.setAngularDamping(original.m_AngularDrag);
 body.setSleepThreshold(data.settings.m_SleepThreshold);
 body.setSolverIterationCounts(data.settings.m_DefaultSolverIterations,data.settings.m_DefaultSolverVelocityIterations);
 // Original Unity constraints 120: freeze Z position and all rotation.
 if(original.m_Constraints!==120)throw new Error('Unexpected original Rigidbody constraints');
 for(const key of ['eLOCK_LINEAR_Z','eLOCK_ANGULAR_X','eLOCK_ANGULAR_Y','eLOCK_ANGULAR_Z'])body.setRigidDynamicLockFlag(P.PxRigidDynamicLockFlagEnum[key],true);
 scene.addActor(body);
 function dynamicCheck(value){if(!dynamics.has(value))throw new Error('Foreign or disposed dynamic actor');}
 function forceMode(mode){
  const modes={Force:P.PxForceModeEnum.eFORCE,Acceleration:P.PxForceModeEnum.eACCELERATION,
   Impulse:P.PxForceModeEnum.eIMPULSE,VelocityChange:P.PxForceModeEnum.eVELOCITY_CHANGE};
  if(!(mode in modes))throw new Error('Unsupported original ForceMode: '+mode);
  return modes[mode];
 }
 function casingBody({position,rotation,mass,halfExtents,center,linearDamping,angularDamping}){
  if(![...position,...rotation,...halfExtents,...center,mass,linearDamping,angularDamping].every(Number.isFinite)||mass<=0||halfExtents.some(x=>x<=0))
   throw new TypeError('Invalid original casing Rigidbody/BoxCollider');
  const t=pose(position,rotation),actor=physics.createRigidDynamic(t);P.destroy(t);
  const shape=physics.createShape(new P.PxBoxGeometry(...halfExtents),casingMaterial,true,flags);
  const local=pose(center);shape.setLocalPose(local);shape.setSimulationFilterData(filter);
  shape.setContactOffset(data.settings.m_DefaultContactOffset);actor.attachShape(shape);
  P.destroy(local);P.PxRigidBodyExt.prototype.setMassAndUpdateInertia(actor,mass);
  actor.setLinearDamping(linearDamping);actor.setAngularDamping(angularDamping);
  actor.setSleepThreshold(data.settings.m_SleepThreshold);
  actor.setSolverIterationCounts(data.settings.m_DefaultSolverIterations,data.settings.m_DefaultSolverVelocityIterations);
  actor.setRigidBodyFlag(P.PxRigidBodyFlagEnum.eENABLE_CCD,true);
  actor.setMaxAngularVelocity(3.402823466e38);
  scene.addActor(actor);const result={actor,shape,mass};dynamics.add(result);return result;
 }
 P.destroy(g);P.destroy(desc);P.destroy(scale);
 const force=vec([0,0,0]);
 const api={
  data,backend:'PhysX '+[(version>>>24)&255,(version>>>16)&255,(version>>>8)&255].join('.'),
  setPoseState(kneeling,facing,dt){
   kneelRatio=advanceOriginalKneelRatio(kneelRatio,kneeling,dt);
   const low=kneelRatio>=.5;
   if(low!==previousKneeling||facing!==previousFacing)dimensions(low,facing);
  },
  synchronizeActorState({x,y,z,velocity,verticalVelocity=0,depthVelocity=0}){
   const values=[x,y,z,velocity,verticalVelocity,depthVelocity];
   if(values.some(value=>!Number.isFinite(value)))throw new TypeError('FullSource actor physics snapshot must contain finite position and velocity values');
   const nextPose=pose([x,y,z]);
   body.setGlobalPose(nextPose,true);
   P.destroy(nextPose);
   const nextVelocity=vec([velocity,verticalVelocity,depthVelocity]);
   body.setLinearVelocity(nextVelocity,true);
   P.destroy(nextVelocity);
  },
  addAcceleration(ax){force.set_x(ax);body.addForce(force,P.PxForceModeEnum.eACCELERATION,true);},
  createOriginalCasing(options){return casingBody(options);},
  addDynamicForce(value,forceValue,point,mode){
   dynamicCheck(value);if(forceValue.length!==3||point.length!==3||![...forceValue,...point].every(Number.isFinite))throw new TypeError('Invalid original casing force');
   // PxRigidBodyExt::addForceAtPos accepts only FORCE or IMPULSE. Convert Unity's
   // acceleration modes through the original Rigidbody mass first.
   const convert=mode==='Acceleration'||mode==='VelocityChange';
   const mapped=mode==='Acceleration'?'Force':mode==='VelocityChange'?'Impulse':mode;
   const f=vec(forceValue.map(component=>component*(convert?value.actor.getMass():1))),p=vec(point);
   try{P.PxRigidBodyExt.prototype.addForceAtPos(value.actor,f,p,forceMode(mapped),true);}finally{P.destroy(f);P.destroy(p);}
  },
  addDynamicTorque(value,torque,mode){
   dynamicCheck(value);if(torque.length!==3||!torque.every(Number.isFinite))throw new TypeError('Invalid original casing torque');
   const v=vec(torque);try{value.actor.addTorque(v,forceMode(mode),true);}finally{P.destroy(v);}
  },
  dynamicSnapshot(value){
   dynamicCheck(value);const t=value.actor.getGlobalPose(),p=t.get_p(),q=t.get_q(),v=value.actor.getLinearVelocity(),w=value.actor.getAngularVelocity();
   return {position:[p.get_x(),p.get_y(),p.get_z()],rotation:[q.get_x(),q.get_y(),q.get_z(),q.get_w()],
    velocity:[v.get_x(),v.get_y(),v.get_z()],angularVelocity:[w.get_x(),w.get_y(),w.get_z()],
    mass:value.actor.getMass(),sleeping:value.actor.isSleeping()};
  },
  synchronizeDynamic(value,state){
   dynamicCheck(value);if(!state||![...state.position,...state.rotation,...state.velocity,...state.angularVelocity].every(Number.isFinite))throw new TypeError('Invalid source casing state');
   const t=pose(state.position,state.rotation),v=vec(state.velocity),w=vec(state.angularVelocity);
   try{value.actor.setGlobalPose(t,true);value.actor.setLinearVelocity(v,true);value.actor.setAngularVelocity(w,true);}finally{P.destroy(t);P.destroy(v);P.destroy(w);}
  },
  removeDynamic(value){
   dynamicCheck(value);dynamics.delete(value);scene.removeActor(value.actor);value.actor.release();value.shape.release();
  },
  step(dt){scene.simulate(dt);scene.fetchResults(true);return api.snapshot();},
  snapshot(){const p=body.getGlobalPose().get_p(),v=body.getLinearVelocity();return {x:p.get_x(),y:p.get_y(),z:p.get_z(),velocity:v.get_x(),verticalVelocity:v.get_y(),kneelRatio,sleeping:body.isSleeping(),boxes:statics.length,backend:api.backend};},
  dispose(){for(const value of [...dynamics])api.removeDynamic(value);scene.removeActor(body);body.release();shape.release();for(const a of statics)a.release();scene.release();for(const m of materials.values())m.release();casingMaterial.release();physics.release();P.destroy(dispatcher);foundation.release();P.destroy(flags);P.destroy(filter);P.destroy(force);P.destroy(allocator);P.destroy(errors);}
 };
 return api;
}
