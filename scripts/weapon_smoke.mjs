import {chromium,webkit} from 'playwright';
import fs from 'node:fs';import assert from 'node:assert/strict';
const results=[];
for(const [engine,type] of [['chromium',chromium],['webkit',webkit]]){
 const browser=await type.launch({headless:true,...(engine==='chromium'?{args:['--no-sandbox','--enable-webgl','--ignore-gpu-blocklist','--use-gl=angle','--use-angle=swiftshader']}: {})});
 const page=await browser.newPage({viewport:{width:852,height:393},hasTouch:true});const errors=[];page.on('pageerror',e=>errors.push(e.message));
 const state=()=>page.evaluate(()=>window.__gameState());
 const ready=()=>page.waitForFunction(()=>{const w=window.__gameState?.().weaponState;return w&&!w.reloading&&w.right.OnTarget&&w.left.OnTarget;},null,{timeout:30000});
 const rounds=w=>w.magazine+w.reserve+(w.chamberLive?1:0);
 try{
  await page.goto('http://127.0.0.1:8000/game/',{waitUntil:'domcontentloaded',timeout:120000});
  await page.waitForFunction(()=>window.__gameState?.().weaponState||document.querySelector('#error').style.display==='block',null,{timeout:120000});
  assert.equal(await page.locator('#error').isVisible(),false,await page.locator('#error').textContent());await ready();
  // Validate rendering only the recovered Wood exit-hole Circle.asset from its
  // original source GUID/material, without inventing a particle or physics effect.
  const sourceVisual=await page.evaluate(async()=>{
   const {createOriginalImpactMeshRenderer}=await import('./original-impact-mesh.mjs');
   const THREE=await import('three');
   const isolated=new THREE.Scene(),renderer=createOriginalImpactMeshRenderer(isolated);
   await renderer.loading;
   const ready=renderer.snapshot();
   if(!ready.loaded)return {ready};
   renderer.update([{type:'impactMesh',value:{
     source:'c0bb8365c7bd54b4597570f085f5a153',instance:1234567,
     position:[1,2,3],rotation:[0,0,0,1],scale:[1,1,.2]
   }}]);
   const mesh=isolated.children[0];
   const payload={ready,added:renderer.snapshot().active,
     vertices:mesh.geometry.getAttribute('position').count,
     triangles:mesh.geometry.index.count/3,
     position:mesh.position.toArray(),scale:mesh.scale.toArray(),
     source:mesh.userData.originalEffectSource};
   renderer.update([]);
   payload.removed=renderer.snapshot().active===0&&isolated.children.length===0;
   renderer.dispose();
   return payload;
  });
  assert.equal(sourceVisual.ready?.assetError,null,'No original impact geometry load errors');
  assert.equal(sourceVisual.ready?.loaded,true,'Original exit-hole source mesh loads');
  assert.equal(sourceVisual.vertices,186);
  assert.equal(sourceVisual.triangles,100);
  assert.deepEqual(sourceVisual.position,[1,2,-3],'Unity-to-Three world position conversion');
  assert.deepEqual(sourceVisual.scale,[1,1,.2]);
  assert.equal(sourceVisual.added,1);
  assert.equal(sourceVisual.removed,true,'Source stops rendering when C# frame stops reporting its instance');
  assert.equal(sourceVisual.source,'c0bb8365c7bd54b4597570f085f5a153');
  const initial=await state();assert.equal(initial.weaponState.ballistics.materials,386);assert.equal(initial.weaponState.ballistics.loadedMeshColliders,61);assert.equal(initial.weaponState.ballistics.unsupportedColliders,2);assert.equal(initial.weaponState.shots,0);assert.equal(rounds(initial.weaponState),61);
  await page.mouse.move(730,90);await page.waitForFunction(()=>window.__gameState().aimDirection[1]>.05,null,{timeout:30000});
  const aimed=await state(),direction=aimed.aimDirection;
  await page.keyboard.down('KeyD');await page.waitForFunction(x=>window.__gameState().playerX>x+.3,aimed.playerX,{timeout:30000});await page.keyboard.up('KeyD');
  const walking=await state();assert.deepEqual(walking.aimDirection,direction);assert.equal(walking.facingDirection,1);
  await page.keyboard.down('KeyA');await page.waitForFunction(x=>window.__gameState().playerX<x-.3,walking.playerX,{timeout:30000});await page.keyboard.up('KeyA');
  const back=await state();assert.deepEqual(back.aimDirection,direction);assert.equal(back.facingDirection,1);
  // A tap away from the last aimed screen location adjusts direction, not firing.
  await page.touchscreen.tap(630,130);
  await page.waitForFunction(previous=>window.__gameState().aimDirection.some((v,i)=>Math.abs(v-previous[i])>.015),direction,{timeout:30000});
  const touchAimed=await state();
  assert.equal(touchAimed.weaponState.shots,0,'New touch location must only aim');
  assert.notDeepEqual(touchAimed.aimDirection,direction);
  // A repeated tap near that location should fire exactly one original weapon shot.
  await ready();
  await page.touchscreen.tap(637,136);
  await page.waitForFunction(()=>window.__gameState().weaponState.shots===1,null,{timeout:30000});
  await page.waitForTimeout(400);
  const tapped=await state();
  assert.equal(tapped.weaponState.shots,1);
  assert.deepEqual(tapped.aimDirection,touchAimed.aimDirection,'Shooting must retain new touch aim');
  assert.equal(rounds(tapped.weaponState),60);
  // A new distant tap should re-aim without firing another round.
  await page.touchscreen.tap(780,80);
  await page.waitForFunction(previous=>window.__gameState().aimDirection.some((v,i)=>Math.abs(v-previous[i])>.015),tapped.aimDirection,{timeout:30000});
  assert.equal((await state()).weaponState.shots,1,'Different tap cannot fire');
  let nativeTouchDrag=false;
  if(engine==='chromium'){
   const cdp=await page.context().newCDPSession(page);
   await cdp.send('Input.dispatchTouchEvent',{type:'touchStart',touchPoints:[{x:600,y:180,id:1}]});
   await cdp.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x:670,y:120,id:1}]});
   await cdp.send('Input.dispatchTouchEvent',{type:'touchMove',touchPoints:[{x:740,y:80,id:1}]});
   await cdp.send('Input.dispatchTouchEvent',{type:'touchEnd',touchPoints:[]});
   await page.waitForTimeout(300);const dragged=await state();assert.equal(dragged.weaponState.shots,1,'Dragging must never fire');assert.notDeepEqual(dragged.aimDirection,direction);nativeTouchDrag=true;
  }
  const preReload=await state();await page.locator('#reload').tap();
  await page.waitForFunction(()=>window.__gameState().weaponState.reloading,null,{timeout:30000});
  await page.waitForFunction(()=>!window.__gameState().weaponState.reloading,null,{timeout:30000});await ready();
  const reloaded=await state();assert.equal(rounds(reloaded.weaponState),rounds(preReload.weaponState));assert.equal(reloaded.weaponState.magazine,30);assert.equal(reloaded.weaponState.reserve,29);
  const fireBox=await page.locator('#fire').boundingBox();await page.mouse.move(fireBox.x+fireBox.width/2,fireBox.y+fireBox.height/2);await page.mouse.down();
  await page.waitForFunction(()=>window.__gameState().weaponState.bolt==='Open'&&window.__gameState().weaponState.magazine===0,null,{timeout:30000});await page.mouse.up();
  const empty=await state();assert.equal(empty.weaponState.shots,32);assert.equal(empty.weaponState.chamberLive,false);
  await page.waitForTimeout(300);assert.equal((await state()).weaponState.shots,32,'No extra shot after release/empty');
  await page.locator('#reload').tap();await page.waitForFunction(()=>window.__gameState().weaponState.reloading,null,{timeout:30000});await page.waitForFunction(()=>!window.__gameState().weaponState.reloading&&window.__gameState().weaponState.chamberLive,null,{timeout:30000});await ready();
  const final=await state();assert.equal(final.weaponState.magazine,28);assert.equal(final.weaponState.reserve,0);assert.equal(final.projection.fov,20);assert.ok(final.presentation.arms.every(a=>Number.isFinite(a.upper)&&Number.isFinite(a.lower)));
  assert.ok(final.weaponState.ballistics.contacts>0,"Original scene material contacts must occur during live firing");assert.ok(Number.isFinite(final.weaponState.ballistics.last.exitEnergy));assert.deepEqual(errors,[]);await page.screenshot({path:`weapon-${engine}.png`});results.push({engine,passed:true,nativeTouchDrag,actualIPhoneTest:false,initial,tapped,reloaded,empty,final});
 }catch(e){results.push({engine,passed:false,error:String(e),errors,state:await page.evaluate(()=>window.__gameState?.()||null),screenError:await page.locator('#error').textContent()});await page.screenshot({path:`weapon-${engine}.png`}).catch(()=>{});process.exitCode=1;}
 finally{await browser.close();}
}
fs.writeFileSync('weapon-smoke.json',JSON.stringify(results,null,2));console.log(JSON.stringify(results.map(({engine,passed,error})=>({engine,passed,error}))));
