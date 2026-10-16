// Engine-only PhysX prerequisites. Not yet connected to original ArmController.
// Coordinates at this boundary are Unity world/local coordinates (reflect Z).
// No gameplay rules, source joint defaults, or approximate hinge springs here.
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
 const api={
  backend:'PhysX 5.6.1; engine prerequisite, native Unity parity unverified',
  // Box fixture API for numerical engine contracts; source capsule/hierarchy
  // import is intentionally not claimed by this prerequisite.
  createBox({position=[0,0,0],rotation=[0,0,0,1],halfExtents,mass,
             linearDamping=0,angularDamping=0,isKinematic=false,useGravity=false}) {
   alive();
   if(!finite(halfExtents)||halfExtents.some(x=>x<=0)||!Number.isFinite(mass)||mass<=0)throw new Error('Invalid body dimensions or mass');
   const t=pose(position,rotation),actor=physics.createRigidDynamic(t);P.destroy(t);
   const geom=new P.PxBoxGeometry(...halfExtents),shape=physics.createShape(geom,material,true,flags);
   actor.attachShape(shape);shape.release();P.destroy(geom);
   P.PxRigidBodyExt.prototype.setMassAndUpdateInertia(actor,mass);
   actor.setLinearDamping(linearDamping);actor.setAngularDamping(angularDamping);
   actor.setActorFlag(P.PxActorFlagEnum.eDISABLE_GRAVITY,!useGravity);
   actor.setRigidBodyFlag(P.PxRigidBodyFlagEnum.eKINEMATIC,isKinematic);
   scene.addActor(actor);const handle={actor,isKinematic};bodies.add(handle);return handle;
  },
  setKinematic(body,state){bodyCheck(body);body.actor.setRigidBodyFlag(P.PxRigidBodyFlagEnum.eKINEMATIC,!!state);body.isKinematic=!!state;},
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
   const joint=P.PxTopLevelFunctions.prototype.RevoluteJointCreate(physics,connected.actor,t0,body.actor,t1);
   P.destroy(t0);P.destroy(t1);
   joint.setConstraintFlag(P.PxConstraintFlagEnum.eCOLLISION_ENABLED,enableCollision);
   if(limits){
    const l=new P.PxJointAngularLimitPair(limits.min*Math.PI/180,limits.max*Math.PI/180);
    l.restitution=limits.bounciness;l.bounceThreshold=limits.bounceMinVelocity;
    joint.setLimit(l);P.destroy(l);joint.setRevoluteJointFlag(P.PxRevoluteJointFlagEnum.eLIMIT_ENABLED,true);
   }
   joints.add(joint);
   return {angle:()=>joint.getAngle()*180/Math.PI,
    setSpring(){throw new Error('Unity HingeJoint spring compatibility requires native reference; not implemented');}};
  },
  step(dt){alive();if(!Number.isFinite(dt)||dt<=0)throw new Error('Invalid fixed timestep');scene.simulate(dt);scene.fetchResults(true);},
  snapshot(body){
   bodyCheck(body);const t=body.actor.getGlobalPose(),q=t.get_q();
   // Angular velocity is an axial vector: reflection changes X/Y signs.
   const w=body.actor.getAngularVelocity();
   return {position:values(t.get_p()),rotation:[-q.get_x(),-q.get_y(),q.get_z(),q.get_w()],
    velocity:values(body.actor.getLinearVelocity()),angularVelocity:[-w.get_x(),-w.get_y(),w.get_z()],
    inertia:values(body.actor.getMassSpaceInertiaTensor()).map(Math.abs),mass:body.actor.getMass(),isKinematic:body.isKinematic};
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
