// Engine-only PhysX prerequisites. Not yet connected to original ArmController.
// Coordinates at this boundary are Unity world/local coordinates (reflect Z).
// No gameplay rules or source joint defaults. Springs use PhysX implicit drives.
import PhysX from './vendor/physx-js-webidl.mjs';

export async function createArmPhysicsKernel({moduleOptions={},gravity=[0,0,0]}={}) {
 const P=await PhysX(moduleOptions),allocator=new P.PxDefaultAllocator(),errors=new P.PxDefaultErrorCallback();
 const foundation=P.CreateFoundation(P.PHYSICS_VERSION,allocator,errors),scale=new P.PxTolerancesScale();
 const physics=P.CreatePhysics(P.PHYSICS_VERSION,foundation,scale),desc=new P.PxSceneDesc(scale);
 const dispatcher=P.DefaultCpuDispatcherCreate(0);
 const vector=a=>new P.PxVec3(a[0],a[1],-a[2]);
 const values=v=>[v.get_x(),v.get_y(),-v.get_z()];
 const finite=a=>Array.isArray(a)&&a.length===3&&a.every(Number.isFinite);
 function pose(position,rotation=[0,0,0,1]) {
  if(!finite(position)||rotation.length!==4||!rotation.every(Number.isFinite))throw new Error('Invalid pose');
  const v=vector(position),q=new P.PxQuat(-rotation[0],-rotation[1],rotation[2],rotation[3]);
  const t=new P.PxTransform(v,q);P.destroy(v);P.destroy(q);return t;
 }
 const g=vector(gravity);desc.set_gravity(g);P.destroy(g);
 desc.set_cpuDispatcher(dispatcher);desc.set_filterShader(P.DefaultFilterShader());
 desc.set_solverType(P.PxSolverTypeEnum.ePGS);
 const scene=physics.createScene(desc),material=physics.createMaterial(0,0,0);
 const flags=new P.PxShapeFlags(P.PxShapeFlagEnum.eSIMULATION_SHAPE|P.PxShapeFlagEnum.eSCENE_QUERY_SHAPE);
 const bodies=new Set(),joints=new Set();let disposed=false;
 function alive(){if(disposed)throw new Error('Physics kernel disposed');}
 function bodyCheck(body){alive();if(!bodies.has(body))throw new Error('Foreign or disposed body');}
 function createBody({position=[0,0,0],rotation=[0,0,0,1],mass,
                      linearDamping=0,angularDamping=0,isKinematic=false,useGravity=false,
                      center=[0,0,0],shapeRotation=[0,0,0,1],constraints=0},geometry){
  alive();
  if(!Number.isFinite(mass)||mass<=0||!finite(center))throw new Error('Invalid body mass/center');
  const t=pose(position,rotation),actor=physics.createRigidDynamic(t);P.destroy(t);
  const shape=physics.createShape(geometry,material,true,flags),local=pose(center,shapeRotation);
  shape.setLocalPose(local);P.destroy(local);actor.attachShape(shape);shape.release();P.destroy(geometry);
  P.PxRigidBodyExt.prototype.setMassAndUpdateInertia(actor,mass);
  actor.setLinearDamping(linearDamping);actor.setAngularDamping(angularDamping);
  actor.setActorFlag(P.PxActorFlagEnum.eDISABLE_GRAVITY,!useGravity);
  actor.setRigidBodyFlag(P.PxRigidBodyFlagEnum.eKINEMATIC,isKinematic);
  const locks=[[2,'eLOCK_LINEAR_X'],[4,'eLOCK_LINEAR_Y'],[8,'eLOCK_LINEAR_Z'],[16,'eLOCK_ANGULAR_X'],[32,'eLOCK_ANGULAR_Y'],[64,'eLOCK_ANGULAR_Z']];
  for(const [bit,name] of locks)actor.setRigidDynamicLockFlag(P.PxRigidDynamicLockFlagEnum[name],!!(constraints&bit));
  scene.addActor(actor);const handle={actor,isKinematic};bodies.add(handle);return handle;
 }
 const api={
  backend:'PhysX 5.6.1; engine prerequisite, native Unity parity unverified',
  createBox(options) {
   alive();
   const {halfExtents}=options;
   if(!finite(halfExtents)||halfExtents.some(x=>x<=0))throw new Error('Invalid body dimensions');
   return createBody(options,new P.PxBoxGeometry(...halfExtents));
  },
  createCapsule(options){
   alive();const {radius,height,direction}=options;
   if(!Number.isFinite(radius)||!Number.isFinite(height)||radius<=0||height<=0||![0,1,2].includes(direction))throw new Error('Invalid capsule dimensions/axis');
   const h=Math.SQRT1_2,shapeRotation=direction===0?[0,0,0,1]:direction===1?[0,0,h,h]:[0,-h,0,h];
   return createBody({...options,shapeRotation},new P.PxCapsuleGeometry(radius,Math.max(0,height/2-radius)));
  },
  setKinematic(body,state){bodyCheck(body);body.actor.setRigidBodyFlag(P.PxRigidBodyFlagEnum.eKINEMATIC,!!state);body.isKinematic=!!state;},
  // Original Unity arm joints begin kinematic. During normal source IK, the
  // authored limb world pose must be transported to the actual PhysX actor;
  // toggling eKINEMATIC alone leaves its collision/joint frame at the old pose.
  // This is an engine pose adapter, not a replacement IK solver or controller.
  syncKinematicPose(body,position,rotation){
   bodyCheck(body);
   if(!body.isKinematic)throw new Error('Cannot apply IK pose to dynamic source limb');
   const target=pose(position,rotation);
   try{body.actor.setKinematicTarget(target);}
   finally{P.destroy(target);}
  },
  addForceAtPosition(body,force,point,mode='Force') {
   bodyCheck(body);if(!finite(force)||!finite(point))throw new Error('Invalid force/point');
   if(!['Force','Acceleration','Impulse','VelocityChange'].includes(mode))throw new Error('Unsupported ForceMode: '+mode);
   if(body.isKinematic)return; // Unity does not apply forces to kinematic bodies.
   // PhysX Ext accepts FORCE/IMPULSE only. Scale by mass before Ext so the
   // moment uses the real center of mass and full rotated inertia tensor.
   // This mapping is numerically checked; native Unity trajectory comparison
   // is still required before enabling original controller playback.
   const byMass=mode==='Acceleration'||mode==='VelocityChange';
   const f=vector(force.map(x=>x*(byMass?body.actor.getMass():1))),p=vector(point);
   const impulse=mode==='Impulse'||mode==='VelocityChange';
   P.PxRigidBodyExt.prototype.addForceAtPos(body.actor,f,p,impulse?P.PxForceModeEnum.eIMPULSE:P.PxForceModeEnum.eFORCE,true);
   P.destroy(f);P.destroy(p);
  },
  createHinge(body,connected,{anchor,connectedAnchor,rotation,connectedRotation,
                            limits,enableCollision=false}) {
   bodyCheck(body);bodyCheck(connected);
   // Frames must be explicit: PhysX revolves about local X. Recovering the
   // source Unity Z axis does not by itself establish the reference angle.
   if(!rotation||!connectedRotation)throw new Error('Explicit authored hinge frames required');
   const t0=pose(connectedAnchor,connectedRotation),t1=pose(anchor,rotation);
   // A D6 joint with only twist free has the same hinge degrees of freedom,
   // and provides the implicit spring drive absent from PxRevoluteJoint.
   const joint=P.PxTopLevelFunctions.prototype.D6JointCreate(physics,connected.actor,t0,body.actor,t1);
   P.destroy(t0);P.destroy(t1);
   joint.setMotion(P.PxD6AxisEnum.eTWIST,limits?P.PxD6MotionEnum.eLIMITED:P.PxD6MotionEnum.eFREE);
   joint.setAngularDriveConfig(P.PxD6AngularDriveConfigEnum.eLEGACY);
   joint.setConstraintFlag(P.PxConstraintFlagEnum.eCOLLISION_ENABLED,enableCollision);
   if(limits){
    const l=new P.PxJointAngularLimitPair(limits.min*Math.PI/180,limits.max*Math.PI/180);
    l.restitution=limits.bounciness;l.bounceThreshold=limits.bounceMinVelocity;
    joint.setTwistLimit(l);P.destroy(l);
   }
   joints.add(joint);
   let springState={spring:0,damper:0,targetPosition:0,enabled:false};
   return {angle:()=>joint.getTwistAngle()*180/Math.PI,
    springState:()=>({...springState}),
    setSpring({spring,damper,targetPosition},enabled=false){
     bodyCheck(body);bodyCheck(connected);
     if(![spring,damper,targetPosition].every(Number.isFinite)||spring<0||damper<0)throw new Error('Invalid hinge spring');
     // Unity 2017 targets are degrees relative to the initial rest frame;
     // PhysX D6 targets are radians. Use a force drive, not acceleration.
     // Coefficient/native Unity trajectory parity still needs reference data.
     const drive=new P.PxD6JointDrive(enabled?spring:0,enabled?damper:0,3.402823466e38,false);
     joint.setDrive(P.PxD6DriveEnum.eTWIST,drive);P.destroy(drive);
     const radians=targetPosition*Math.PI/180;
     // This is the joint's native PhysX X-axis orientation, not Unity world X.
     const v=new P.PxVec3(0,0,0),q=new P.PxQuat(Math.sin(radians/2),0,0,Math.cos(radians/2)),target=new P.PxTransform(v,q);
     joint.setDrivePosition(target,true);P.destroy(target);P.destroy(q);P.destroy(v);
     springState={spring,damper,targetPosition,enabled:!!enabled};
    }};
  },
  step(dt){alive();if(!Number.isFinite(dt)||dt<=0)throw new Error('Invalid fixed timestep');scene.simulate(dt);scene.fetchResults(true);},
  snapshot(body){
   bodyCheck(body);const t=body.actor.getGlobalPose(),q=t.get_q();
   // Angular velocity is an axial vector: reflection changes X/Y signs.
   const w=body.actor.getAngularVelocity(),c=t.transform(body.actor.getCMassLocalPose().get_p()),worldCenterOfMass=values(c);
   return {position:values(t.get_p()),rotation:[-q.get_x(),-q.get_y(),q.get_z(),q.get_w()],
    velocity:values(body.actor.getLinearVelocity()),angularVelocity:[-w.get_x(),-w.get_y(),w.get_z()],
    inertia:values(body.actor.getMassSpaceInertiaTensor()).map(Math.abs),centerOfMass:values(body.actor.getCMassLocalPose().get_p()),worldCenterOfMass,mass:body.actor.getMass(),isKinematic:body.isKinematic};
  },
  dispose(){
   if(disposed)return;disposed=true;
   for(const joint of joints)joint.release();joints.clear();
   for(const b of bodies){scene.removeActor(b.actor);b.actor.release();}bodies.clear();
   scene.release();material.release();physics.release();P.destroy(dispatcher);foundation.release();
   P.destroy(desc);P.destroy(scale);P.destroy(flags);P.destroy(allocator);P.destroy(errors);
  }
 };
 return api;
}
