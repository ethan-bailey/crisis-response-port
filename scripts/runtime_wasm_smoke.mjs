import { chromium, webkit } from 'playwright';
import fs from 'node:fs';
import assert from 'node:assert/strict';
import { gunzipSync } from 'node:zlib';

const reports=[];
const packageData=JSON.parse(fs.readFileSync('work/runtime/scene-package.json','utf8'));
const weaponPackage=JSON.parse(gunzipSync(fs.readFileSync('game/assets/weapon-runtime.json.gz')).toString('utf8'));
const ammoPackageData=weaponPackage.ammoPackage;
const packageAssets={...packageData.assets,...ammoPackageData.assets};
const packageGuidPaths={...packageData.guidPaths,...ammoPackageData.guidPaths};
const packageGuidMetadata={...packageData.guidMetadata,...ammoPackageData.guidMetadata};
const referencesBySource=new Map();
for(const edge of packageData.objectReferences??[]){if(!referencesBySource.has(edge.source))referencesBySource.set(edge.source,[]);referencesBySource.get(edge.source).push(edge)}
const originalAmmoRecords=packageData.assets['Assets/Resources/weapons/ammunition/5.56-JHP.prefab'].records;
assert.equal(originalAmmoRecords.find(r=>r.classID===114).fields.projectile.fileID,'114703437598541787','Unity reference identity must survive JS parsing');
const packageFor=path=>{
  const has=(object,key)=>Object.prototype.hasOwnProperty.call(object,key);
  if(!has(packageAssets,path))throw new Error('Original fixture asset is absent: '+path);
  const settingsPath='ProjectSettings/TimeManager.asset';
  if(!has(packageAssets,settingsPath))throw new Error('Original TimeManager asset is absent');
  const included=new Set([path,settingsPath]);
  const pending=[...included];
  while(pending.length){
    const source=pending.pop();
    for(const edge of referencesBySource.get(source)||[]){
      if(edge.resolution!=='package')continue;
      const dependency=packageGuidPaths[edge.guid];
      if(!dependency||!has(packageAssets,dependency))throw new Error('Original package dependency is absent: '+source+':'+edge.fieldPath+' -> '+edge.guid+' -> '+dependency);
      if(!included.has(dependency)){included.add(dependency);pending.push(dependency)}
    }
  }
  return JSON.stringify({
    guidPaths:packageGuidPaths,
    guidMetadata:packageGuidMetadata,
    scriptTypes:packageData.scriptTypes,
    scriptAssemblies:packageData.scriptAssemblies,
    builtinObjects:packageData.builtinObjects,
    assets:Object.fromEntries([...included].map(asset=>[asset,packageAssets[asset]])),
    objectReferences:(packageData.objectReferences??[]).filter(edge=>included.has(edge.source)),
    unresolvedGuids:packageData.unresolvedGuids
  });
};
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
   const query=await page.evaluate(async()=>JSON.parse(await window.runtimeCall('VerifySceneRay')));
   assert.ok(query.nearestDistance>=0&&query.nearestDistance<=query.directDistance+.001);
   scenes.push({...scene,query});
  }
  const ammunition=[];
  for(const path of ['Assets/Resources/weapons/ammunition/5.56-JHP.prefab','Assets/Resources/weapons/ammunition/308-Win-BSP.prefab']){
   stage='ammunition '+path;const sample=await page.evaluate(async([text,path])=>JSON.parse(await window.runtimeCall('VerifyAmmunition',text,path)),[packageFor(path),path]);
   assert.ok(sample.energyAfter<sample.energyBefore&&sample.energyAfter>0);ammunition.push(sample);
  }
  stage='original resource lookup and prefab clone';
  const resource=await page.evaluate(async text=>JSON.parse(await window.runtimeCall('VerifyResource',text,'5.56-JHP')),packageFor('Assets/Resources/weapons/ammunition/5.56-JHP.prefab'));
  assert.ok(resource.independentInstance&&resource.cachedResource&&resource.clockUnchanged&&resource.originalGetAmmunitionExecuted);
  assert.deepEqual(errors,[]);
  await page.screenshot({path:`runtime-${name}.png`});
  reports.push({browser:name,passed:true,contracts:await page.evaluate(()=>window.__runtimeContracts),inputSnapshot,scenes,ammunition,resource,physicalIPhoneVerified:false});
 }catch(error){reports.push({browser:name,passed:false,stage,error:String(error),pageErrors:errors,runtimeError:await page.evaluate(()=>window.__runtimeError??null)});process.exitCode=1}
 finally{await browser.close()}
}
fs.writeFileSync('runtime-wasm-smoke.json',JSON.stringify(reports,null,2));console.log(JSON.stringify(reports,null,2));
