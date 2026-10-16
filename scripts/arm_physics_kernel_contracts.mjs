import {createArmPhysicsKernel} from '../game/arm-physics-kernel.mjs';
// Browser numerical engine fixtures; no game character substitutions.
const assert={
 ok(value,message){if(!value)throw new Error(message);},
 throws(callback,pattern){let thrown;try{callback();}catch(e){thrown=e;}if(!thrown||!pattern.test(String(thrown)))throw new Error('Expected unsupported operation to throw');}
};
export async function runArmPhysicsKernelContracts(){
const world=await createArmPhysicsKernel();
const near=(a,b,label,tolerance=1e-4)=>assert.ok(Math.abs(a-b)<tolerance,`${label}: ${a} vs ${b}`);
const dt=.02;
let checks=0;
try {
 for(const mass of [1,4])for(const mode of ['Force','Acceleration','Impulse','VelocityChange']){
  const body=world.createBox({position:[mass*10,checks*10,0],halfExtents:[.2,.3,.4],mass});
  const p=world.snapshot(body).position;
  world.addForceAtPosition(body,[0,3,0],[p[0]+.125,p[1],p[2]],mode);world.step(dt);
  const state=world.snapshot(body),impulse=['Impulse','VelocityChange'].includes(mode);
  const factor=(['Acceleration','VelocityChange'].includes(mode)?mass:1)*(impulse?1:dt);
  near(state.velocity[1],3*factor/mass,mode+' linear response');
  near(state.angularVelocity[2],.375*factor/state.inertia[2],mode+' off-center torque');checks+=2;
 }
 const body=world.createBox({position:[200,0,0],halfExtents:[.2,.3,.4],mass:4,isKinematic:true});
 world.addForceAtPosition(body,[5,0,0],[200,0,0],'Acceleration');world.step(dt);
 near(world.snapshot(body).position[0],200,'Kinematic body ignores force');checks++;
 world.setKinematic(body,false);world.addForceAtPosition(body,[0,0,5],[200,0,0],'Acceleration');world.step(dt);
 near(world.snapshot(body).velocity[2],5*dt,'Unity Z reflection');checks++;
 const axial=world.createBox({position:[250,0,0],halfExtents:[.2,.3,.4],mass:1});
 world.addForceAtPosition(axial,[0,0,2],[250,.1,0],'Force');world.step(dt);
 near(world.snapshot(axial).angularVelocity[0],.2*dt/world.snapshot(axial).inertia[0],'Axial X reflection');checks++;
 const fixed=world.createBox({position:[300,0,0],halfExtents:[.1,.1,.1],mass:1,isKinematic:true});
 const swinging=world.createBox({position:[300,-1,0],halfExtents:[.1,1,.1],mass:1});
 // These analytical fixture frames use PhysX X as the hinge axis.
 const hinge=world.createHinge(swinging,fixed,{anchor:[0,1,0],connectedAnchor:[0,0,0],rotation:[0,0,0,1],connectedRotation:[0,0,0,1]});
 assert.throws(()=>hinge.setSpring({spring:500,damper:10,targetPosition:0}),/not implemented/);checks++;
 world.addForceAtPosition(swinging,[0,0,1],[300,-1,0],'Force');
 for(let i=0;i<100;i++)world.step(dt);
 assert.ok(Math.abs(hinge.angle())>.1,'Real revolute joint moves');checks++;
 const s=world.snapshot(swinging),q=s.rotation;
 const transformedAnchor=[s.position[0],s.position[1]+1-2*(q[0]*q[0]+q[2]*q[2]),s.position[2]+2*(q[1]*q[2]+q[0]*q[3])];
 near(Math.hypot(transformedAnchor[0]-300,transformedAnchor[1],transformedAnchor[2]),0,'Hinge preserves connected anchor',.005);checks++;
 return {passed:true,checks,backend:world.backend,originalControllerExecuted:false,hingeSpringImplemented:false};
}finally{world.dispose();world.dispose();}

}
