import { chromium, webkit } from 'playwright';
import fs from 'node:fs';
import assert from 'node:assert/strict';

const reports=[];
const packageText=fs.readFileSync('work/runtime/scene-package.json','utf8');
for(const [name,type] of [['chromium',chromium],['webkit',webkit]]){
 const browser=await type.launch({headless:true,...(name==='chromium'?{args:['--no-sandbox']}: {})});
 const page=await browser.newPage({viewport:{width:852,height:393},hasTouch:true});const errors=[];
 page.on('pageerror',e=>errors.push(e.message));
 try{
  await page.goto('http://127.0.0.1:8000/',{waitUntil:'networkidle',timeout:120000});
  await page.waitForFunction(()=>window.__runtimeContracts?.status==='PASS'||window.__runtimeError,null,{timeout:120000});
  assert.equal(await page.evaluate(()=>window.__runtimeContracts?.status),'PASS',await page.locator('#contracts').textContent());
  await page.keyboard.down('KeyD');
  await page.waitForFunction(()=>window.__originalInput?.right.Pressed);
  await page.keyboard.up('KeyD');
  await page.waitForFunction(()=>!window.__originalInput?.right.Pressed);
  const fire=page.locator('[data-action="fire"]'),box=await fire.boundingBox();
  await page.mouse.move(box.x+box.width/2,box.y+box.height/2);await page.mouse.down();
  await page.waitForFunction(()=>window.__originalInput?.fire.PressedLong);
  await page.mouse.move(800,350);await page.mouse.up();
  await page.waitForFunction(()=>!window.__originalInput?.fire.Pressed);
  const inputSnapshot=await page.evaluate(()=>window.__originalInput);
  const scenes=[];
  for(const path of ['Assets/Scenes/Marksman range.unity','Assets/Scenes/Mission 01.unity']){
   const scene=await page.evaluate(async([text,path])=>JSON.parse(await window.runtimeCall('LoadScene',text,path)),[packageText,path]);
   assert.ok(scene.gameObjects>600);assert.ok(scene.boxColliders>300);assert.equal(scene.unresolvedReferences.length,0);
   // Ask a loaded original box for a geometrically guaranteed entering ray.
   scenes.push(scene);
  }
  assert.deepEqual(errors,[]);
  await page.screenshot({path:`runtime-${name}.png`});
  reports.push({browser:name,passed:true,contracts:await page.evaluate(()=>window.__runtimeContracts),inputSnapshot,scenes,physicalIPhoneVerified:false});
 }catch(error){reports.push({browser:name,passed:false,error:String(error),pageErrors:errors});process.exitCode=1}
 finally{await browser.close()}
}
fs.writeFileSync('runtime-wasm-smoke.json',JSON.stringify(reports,null,2));console.log(JSON.stringify(reports,null,2));
