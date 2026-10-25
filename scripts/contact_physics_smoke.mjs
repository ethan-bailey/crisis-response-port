import {chromium,webkit} from 'playwright';
import fs from 'node:fs';
import assert from 'node:assert/strict';
const reports=[];
for(const [engine,type] of [['chromium',chromium],['webkit',webkit]]){
 const browser=await type.launch({headless:true,...(engine==='chromium'?{args:['--no-sandbox']}: {})});
 const page=await browser.newPage();
 try{
  await page.goto('http://127.0.0.1:8000/game/assets/contact-physics.json');
  const result=await page.evaluate(async()=>{
   const {createContactPhysics}=await import('/game/contact-physics.mjs');
   const data=await(await fetch('/game/assets/contact-physics.json')).json(),dt=data.fixedDeltaTime;
   const p=await createContactPhysics(data);let elapsed=0;
   function step(direction=0){
    const speed=Math.abs(p.snapshot().velocity),target=data.character.movement.walkSpeed;
    if(direction&&speed<=target)p.addAcceleration((target-speed)*direction*dt*750);
    p.step(dt);elapsed+=dt;
   }
   for(let i=0;i<100;i++)step();const settled=p.snapshot();
   for(let i=0;i<45;i++)step(1);const right=p.snapshot();
   for(let i=0;i<100;i++)step();const stopped=p.snapshot();
   for(let i=0;i<100;i++)step();const later=p.snapshot();
   for(let i=0;i<45;i++)step(-1);const left=p.snapshot();
   for(let i=0;i<45;i++)step(1);const reverse=p.snapshot();
   p.synchronizeActorState({x:4.25,y:2.75,z:1.5,velocity:-.75,verticalVelocity:.2});const synchronized=p.snapshot();
   p.dispose();
   // With contacts absent and drag still zero, horizontal velocity must persist.
   // Proves stopping comes from contacts, rather than a release-only velocity clamp.
   const air=await createContactPhysics({...data,boxes:[]},{x:-3,y:100,z:1.5});
   air.addAcceleration(10);air.step(dt);const airBefore=air.snapshot();
   for(let i=0;i<100;i++)air.step(dt);const airAfter=air.snapshot();air.dispose();
   return {settled,right,stopped,later,left,reverse,synchronized,airBefore,airAfter,elapsed,material:data.materials[data.character.material],originalDrag:data.character.body.m_Drag};
  });
  assert.equal(result.originalDrag,0);assert.equal(result.material.dynamicFriction,.5);
  assert.ok(result.settled.y>0&&result.settled.y<2,'Recovered floor supports original capsule');
  assert.ok(result.right.velocity>.2&&result.right.x>result.settled.x+.2,'Hold right moves');
  assert.ok(Math.abs(result.stopped.velocity)<.01,'Release stops');
  assert.ok(Math.abs(result.later.x-result.stopped.x)<.005,'No indefinite drift after release');
  assert.ok(result.left.velocity<-.2,'Left moves after stopping');
  assert.ok(result.reverse.velocity>.2,'Direction reverses while moving');
  assert.ok(Math.abs(result.synchronized.x-4.25)<.001&&Math.abs(result.synchronized.y-2.75)<.001&&Math.abs(result.synchronized.z-1.5)<.001,'FullSource Transform pose reaches the browser PhysX actor');
  assert.ok(Math.abs(result.synchronized.velocity+.75)<.001&&Math.abs(result.synchronized.verticalVelocity-.2)<.001,'FullSource Rigidbody velocity reaches the browser PhysX actor');
  assert.ok(Math.abs(result.airAfter.velocity-result.airBefore.velocity)<.001,'No invented airborne damping');
  reports.push({engine,passed:true,...result});
 }catch(error){reports.push({engine,passed:false,error:String(error)});process.exitCode=1;}
 finally{await browser.close();}
}
fs.writeFileSync('contact-physics-smoke.json',JSON.stringify(reports,null,2));console.log(JSON.stringify(reports,null,2));
