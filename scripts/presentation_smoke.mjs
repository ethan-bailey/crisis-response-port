import {chromium,webkit} from 'playwright';
import fs from 'node:fs';import assert from 'node:assert/strict';
const reports=[];
for(const [engine,type] of [['chromium',chromium],['webkit',webkit]]){
 const browser=await type.launch({headless:true,...(engine==='chromium'?{args:['--no-sandbox','--enable-webgl','--ignore-gpu-blocklist','--use-gl=angle','--use-angle=swiftshader']}: {})});
 const page=await browser.newPage({viewport:{width:852,height:393},hasTouch:true});const errors=[];page.on('pageerror',e=>errors.push(e.message));
 try{
  await page.goto('http://127.0.0.1:8000/game/',{waitUntil:'domcontentloaded',timeout:120000});
  await page.waitForFunction(()=>window.__gameState?.().presentation.active||document.querySelector('#error').style.display==='block',null,{timeout:120000});
  assert.equal(await page.locator('#error').isVisible(),false,await page.locator('#error').textContent());
  const initial=await page.evaluate(()=>window.__gameState());
  assert.equal(initial.projection.type,'PerspectiveCamera');assert.equal(initial.projection.near,5);assert.equal(initial.projection.far,20);assert.ok(Math.abs(initial.projection.fov-20)<.001);
  assert.ok(Math.abs(initial.projection.position[1]-1.2)<.001);assert.ok(Math.abs(initial.projection.position[2]-9.8)<.001);
  await page.mouse.move(710,80);await page.mouse.down();await page.mouse.move(740,75,{steps:3});
  await page.waitForFunction(a=>window.__gameState().presentation.pose.weaponAngle>a+3,initial.presentation.pose.weaponAngle,{timeout:30000});
  await page.mouse.up();
  const up=await page.evaluate(()=>window.__gameState());
  assert.equal(up.aimPointerActive,false);
  await page.mouse.move(720,195); // Unpressed mouse must not steer toward an invisible cursor.
  await page.waitForFunction(t=>window.__gameState().physicsTime>t+.12,up.physicsTime,{timeout:30000});
  assert.deepEqual((await page.evaluate(()=>window.__gameState())).aimLocalPoint,up.aimLocalPoint,'Body-local aim stays latched after release');
  await page.mouse.move(610,220);await page.mouse.down();await page.mouse.move(600,235,{steps:3});
  await page.waitForFunction(a=>window.__gameState().presentation.pose.weaponAngle<a-5,up.presentation.pose.weaponAngle,{timeout:30000});
  await page.waitForFunction(()=>{
   const s=window.__gameState().presentation,d=s.worldHeadDirection;
   const angle=Math.atan2(d[1],Math.abs(d[0]))*180/Math.PI;
   let expected=(s.pose.head%360+360)%360;if(expected>180)expected-=360;
   return Math.abs(angle-expected)<3;
  },null,{timeout:30000});
  await page.mouse.up();
  const down=await page.evaluate(()=>window.__gameState());
  assert.equal(down.presentation.arms.length,2);assert.ok(down.presentation.arms.every(a=>Number.isFinite(a.upper)&&Number.isFinite(a.lower)));
  assert.ok(Math.abs(up.presentation.pose.head-down.presentation.pose.head)>.01,'Head follows original pose logic');
  // Dragging (not hovering) to the left turns camera look-ahead and IK together.
  await page.mouse.move(80,195);await page.mouse.down();await page.mouse.move(40,195,{steps:3});await page.waitForFunction(()=>window.__gameState().facingDirection===-1,null,{timeout:30000});
  await page.mouse.up();
  const left=await page.evaluate(()=>window.__gameState());
  assert.ok(left.presentation.arms.every(a=>Number.isFinite(a.upper)&&Number.isFinite(a.lower)));
  await page.screenshot({path:`presentation-${engine}.png`});
  assert.deepEqual(errors,[]);reports.push({engine,passed:true,initial,up,down,left});
 }catch(error){reports.push({engine,passed:false,error:String(error),pageErrors:errors,game:await page.evaluate(()=>window.__gameState?.()||null)});process.exitCode=1;}
 finally{await browser.close();}
}
fs.writeFileSync('presentation-smoke.json',JSON.stringify(reports,null,2));console.log(JSON.stringify(reports,null,2));
