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
  await page.mouse.move(740,75);
  await page.waitForFunction(a=>window.__gameState().presentation.pose.weaponAngle>a+3,initial.presentation.pose.weaponAngle,{timeout:30000});
  const up=await page.evaluate(()=>window.__gameState());
  await page.mouse.move(600,235);
  await page.waitForFunction(a=>window.__gameState().presentation.pose.weaponAngle<a-5,up.presentation.pose.weaponAngle,{timeout:30000});
  const down=await page.evaluate(()=>window.__gameState());
  assert.equal(down.presentation.arms.length,2);assert.ok(down.presentation.arms.every(a=>Number.isFinite(a.upper)&&Number.isFinite(a.lower)));
  assert.ok(Math.abs(up.presentation.pose.head-down.presentation.pose.head)>.01,'Head follows original pose logic');
  // Moving the mouse to the left of the body must turn camera look-ahead and IK together.
  await page.mouse.move(40,195);await page.waitForFunction(()=>window.__gameState().facingDirection===-1,null,{timeout:30000});
  const left=await page.evaluate(()=>window.__gameState());
  assert.ok(left.presentation.arms.every(a=>Number.isFinite(a.upper)&&Number.isFinite(a.lower)));
  await page.screenshot({path:`presentation-${engine}.png`});
  assert.deepEqual(errors,[]);reports.push({engine,passed:true,initial,up,down,left});
 }catch(error){reports.push({engine,passed:false,error:String(error),pageErrors:errors,game:await page.evaluate(()=>window.__gameState?.()||null)});process.exitCode=1;}
 finally{await browser.close();}
}
fs.writeFileSync('presentation-smoke.json',JSON.stringify(reports,null,2));console.log(JSON.stringify(reports,null,2));
