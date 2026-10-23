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
 const statics=[];
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
 P.destroy(g);P.destroy(desc);P.destroy(scale);
 const force=vec([0,0,0]);
 const api={
  data,backend:'PhysX '+[(version>>>24)&255,(version>>>16)&255,(version>>>8)&255].join('.'),
  setPoseState(kneeling,facing,dt){
   kneelRatio=advanceOriginalKneelRatio(kneelRatio,kneeling,dt);
   const low=kneelRatio>=.5;
   if(low!==previousKneeling||facing!==previousFacing)dimensions(low,facing);
  },
  addAcceleration(ax){force.set_x(ax);body.addForce(force,P.PxForceModeEnum.eACCELERATION,true);},
  step(dt){scene.simulate(dt);scene.fetchResults(true);return api.snapshot();},
  snapshot(){const p=body.getGlobalPose().get_p(),v=body.getLinearVelocity();return {x:p.get_x(),y:p.get_y(),z:p.get_z(),velocity:v.get_x(),verticalVelocity:v.get_y(),kneelRatio,sleeping:body.isSleeping(),boxes:statics.length,backend:api.backend};},
  dispose(){scene.removeActor(body);body.release();shape.release();for(const a of statics)a.release();scene.release();for(const m of materials.values())m.release();physics.release();P.destroy(dispatcher);foundation.release();P.destroy(flags);P.destroy(filter);P.destroy(force);P.destroy(allocator);P.destroy(errors);}
 };
 return api;
}
