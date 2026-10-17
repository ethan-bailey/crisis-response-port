import { chromium, webkit } from 'playwright';
import assert from 'node:assert/strict';
import fs from 'node:fs';
const evidence=[];
for (const [name,engine] of [['chromium',chromium],['webkit',webkit]]) {
 const browser=await engine.launch({headless:true});
 try {
  const page=await browser.newPage();const errors=[];
  page.on('pageerror',e=>errors.push(String(e)));
  await page.goto('http://127.0.0.1:8000/runtime-web/?bridge',{waitUntil:'networkidle'});
  await page.waitForFunction(()=>window.__runtimeReady||window.__runtimeError,null,{timeout:120000});
  const result=await page.evaluate(async()=>{
   if(window.__runtimeError)throw Error(window.__runtimeError);
   const source=await fetch('/game/assets/weapon-runtime.json.gz');
   if(!source.ok)throw Error(`Original weapon data: ${source.status}`);
   const weapon=await new Response(source.body.pipeThrough(new DecompressionStream('gzip'))).text();
   const material=await fetch('/game/assets/original-limb-ballistic-materials.json');
   if(!material.ok)throw Error(`Original tissue material: ${material.status}`);
   const skin=await fetch('/game/assets/operator/original-source-limb-damage-mask.json.gz');
   if(!skin.ok)throw Error(`Original skin mask: ${skin.status}`);
   const skinJson=await new Response(skin.body.pipeThrough(new DecompressionStream('gzip'))).text();
   return JSON.parse(await window.runtimeCall('VerifyProjectileInjuryChain',weapon,await material.text(),skinJson));
  });
  assert.equal(result.status,'PASS');assert.equal(result.contracts.length,7);
  assert.equal(result.originalTorsoSkin,true);assert.equal(result.liveNpcGameplay,false);assert.equal(result.sourceSkinFixture,true);
  assert.deepEqual(errors,[]);evidence.push({browser:name,...result});
 }finally{await browser.close();}
}
fs.mkdirSync('work/evidence',{recursive:true});
fs.writeFileSync('work/evidence/projectile-injury-browser.json',JSON.stringify(evidence,null,2)+'\n');
console.log(JSON.stringify(evidence));
