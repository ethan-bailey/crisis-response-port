import { chromium, webkit } from 'playwright';
import fs from 'node:fs';
import assert from 'node:assert/strict';

const reports=[];
const packageData=JSON.parse(fs.readFileSync('work/runtime/scene-execution.json','utf8'));
const packageFor=path=>JSON.stringify({guidPaths:packageData.guidPaths,assets:{
 [path]:packageData.assets[path],
 'ProjectSettings/TimeManager.asset':packageData.assets['ProjectSettings/TimeManager.asset']
}});
for(const [name,type] of [['chromium',chromium],['webkit',webkit]]){
 const browser=await type.launch({headless:true,...(name==='chromium'?{args:['--no-sandbox']}: {})});
 const page=await browser.newPage({viewport:{width:852,height:393},hasTouch:true});const errors=[];let stage='startup';
 page.on('pageerror',e=>errors.push(e.message));
 try{
  await page.goto('http://127.0.0.1:8000/',{waitUntil:'networkidle',timeout:120000});
  await page.waitForFunction(()=>window.__runtimeReady||window.__runtimeError,null,{timeout:120000});
  assert.equal(await page.evaluate(()=>window.__runtimeContracts?.status),'PASS',await page.locator('#contracts').textContent());
  stage='keyboard press';await page.keyboard.down('KeyD');
  await page.waitForFunction(()=>window.__originalInput?.right.Pressed);
  stage='keyboard release';await page.keyboard.up('KeyD');
  await page.waitForFunction(()=>!window.__originalInput?.right.Pressed);
  stage='long press';const fire=page.locator('[data-action="fire"]'),box=await fire.boundingBox();
  await page.mouse.move(box.x+box.width/2,box.y+box.height/2);await page.mouse.down();
  await page.waitForFunction(()=>window.__originalInput?.fire.PressedLong);
  stage='release outside';await page.mouse.move(800,350);await page.mouse.up();
  await page.waitForFunction(()=>!window.__originalInput?.fire.Pressed);
  const inputSnapshot=await page.evaluate(()=>window.__originalInput);
  const scenes=[];
  for(const path of ['Assets/Scenes/Marksman range.unity','Assets/Scenes/Mission 01.unity']){
   stage='scene '+path;const scene=await page.evaluate(async([text,path])=>JSON.parse(await window.runtimeCall('LoadScene',text,path)),[packageFor(path),path]);
   assert.ok(scene.gameObjects>600);assert.ok(scene.boxColliders>300);assert.equal(scene.unresolvedReferences.length,0);
   // Ask a loaded original box for a geometrically guaranteed entering ray.
   scenes.push(scene);
  }
  const ammunition=[];
  for(const path of ['Assets/Resources/weapons/ammunition/5.56-JHP.prefab','Assets/Resources/weapons/ammunition/308-Win-BSP.prefab']){
   stage='ammunition '+path;const sample=await page.evaluate(async([text,path])=>JSON.parse(await window.runtimeCall('VerifyAmmunition',text,path)),[packageFor(path),path]);
   assert.ok(sample.energyAfter<sample.energyBefore&&sample.energyAfter>0);ammunition.push(sample);
  }
  assert.deepEqual(errors,[]);
  await page.screenshot({path:`runtime-${name}.png`});
  reports.push({browser:name,passed:true,contracts:await page.evaluate(()=>window.__runtimeContracts),inputSnapshot,scenes,ammunition,physicalIPhoneVerified:false});
 }catch(error){reports.push({browser:name,passed:false,stage,error:String(error),pageErrors:errors,runtimeError:await page.evaluate(()=>window.__runtimeError??null)});process.exitCode=1}
 finally{await browser.close()}
}
fs.writeFileSync('runtime-wasm-smoke.json',JSON.stringify(reports,null,2));console.log(JSON.stringify(reports,null,2));
