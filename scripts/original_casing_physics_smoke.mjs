import assert from 'node:assert/strict';
import {readFile} from 'node:fs/promises';
import {createContactPhysics} from '../game/contact-physics.mjs';

const data=JSON.parse(await readFile(new URL('../game/assets/contact-physics.json',import.meta.url),'utf8'));
const source=JSON.parse(await readFile(new URL('../game/assets/casing-556.json',import.meta.url),'utf8'));
assert.equal(source.format,'original-unity-casing-mesh-v1');
const physics=await createContactPhysics(data,{x:-3,y:.75,z:1.5});
try{
 const collider=source.collider;
 const body=physics.createOriginalCasing({position:[-3,.75,1.5],rotation:[0,0,0,1],mass:collider.massKg,
  halfExtents:collider.size.map(value=>value/2),center:collider.center,
  linearDamping:collider.drag,angularDamping:collider.angularDrag});
 const before=physics.dynamicSnapshot(body);
 assert.ok(Math.abs(before.mass-.002)<1e-8,'Original 2 g casing mass reaches PhysX');
 physics.addDynamicForce(body,[1,0,0],[-3,.75,1.5],'VelocityChange');
 physics.addDynamicTorque(body,[0,0,.0001],'Impulse');
 physics.step(data.fixedDeltaTime);
 const first=physics.dynamicSnapshot(body);
 assert.ok(first.velocity[0]>.95,'Source VelocityChange force preserves the authored initial velocity increment');
 assert.ok(first.angularVelocity[2]>.1,'Source Impulse torque produces casing angular velocity');
 for(let i=0;i<500;i++)physics.step(data.fixedDeltaTime);
 const settled=physics.dynamicSnapshot(body);
 assert.ok(settled.position[1]>-.2&&settled.position[1]<.5,'Casing collides with the recovered Mission static floor');
 assert.ok(settled.position[0]>-10&&settled.position[0]<1,'Casing remains in the source floor collider bounds');
 console.log(JSON.stringify({passed:true,backend:physics.backend,source:source.source,
  mass:before.mass,forceMode:'VelocityChange',torqueMode:'Impulse',initialVelocity:first.velocity,
  settled:{position:settled.position,velocity:settled.velocity,sleeping:settled.sleeping},
  originalStaticColliders:data.boxes.length}));
}finally{physics.dispose();}
