import {chromium,webkit} from 'playwright';
import fs from 'node:fs';import assert from 'node:assert/strict';
const results=[];
for(const [engine,type] of [['chromium',chromium],['webkit',webkit]]){
 const browser=await type.launch({headless:true,...(engine==='chromium'?{args:['--no-sandbox','--enable-webgl','--ignore-gpu-blocklist','--use-gl=angle','--use-angle=swiftshader']}: {})});
 const page=await browser.newPage({viewport:{width:852,height:393},hasTouch:true});const errors=[];page.on('pageerror',e=>errors.push(e.message));
 const state=()=>page.evaluate(()=>window.__gameState());
 const ready=()=>page.waitForFunction(()=>{const w=window.__gameState?.().weaponState;return w&&!w.reloading&&w.right.OnTarget&&w.left.OnTarget;},null,{timeout:30000});
 const pullOriginalMagazine=async()=>{
  const box=await page.locator('#magazineTouch').boundingBox();
  assert.ok(box,'Original magazine touch element must exist');
  const x=box.x+box.width/2,y=box.y+13;
  await page.mouse.move(x,y);await page.mouse.down();
  await page.mouse.move(x,y+60,{steps:5});await page.mouse.up();
 };
 const rounds=w=>w.magazine+w.reserve+(w.chamberLive?1:0);
 try{
  await page.goto('http://127.0.0.1:8000/game/',{waitUntil:'domcontentloaded',timeout:120000});
  await page.waitForFunction(()=>window.__gameState?.().weaponState||document.querySelector('#error').style.display==='block',null,{timeout:120000});
  assert.equal(await page.locator('#error').isVisible(),false,await page.locator('#error').textContent());await ready();
  await page.waitForFunction(()=>window.__gameState()?.originalHands?.drawnRenderers>=2,null,{timeout:30000});
  const handState=await state();
  assert.equal(handState.originalHands.sourceRenderers,4,'All four original Quad hand MeshRenderers must load');
  assert.equal(handState.originalHands.uniqueSourceTextures,3,'Only the three recovered original hand textures');
  assert.equal(handState.originalHands.visibleRenderers,2,'Original M4 hand modes select one source visual per hand');
  assert.ok(handState.originalHands.drawCalls>0,'Source hand geometry must actually reach WebGL rendering');
  assert.equal(handState.originalHands.sourceShaderParity,false,'Dummy exported shader is not native visual parity');
  assert.equal(handState.presentation.originalIKUpdate,true);
  assert.equal(handState.presentation.originalHandSetMode,true);
  assert.equal(await page.locator('#magazineTouch [data-mag-chamber]').count(),0);
  assert.equal(Number(await page.locator('#magazineTouch [data-mag-count]').textContent()),
   handState.weaponState.magazine+(handState.weaponState.chamberLive?1:0));
  assert.equal(handState.sourceCutouts?.depthWriting,19);
  for(const [i,side] of ['right','left'].entries()){
   const pose=handState.weaponState[side],v=handState.presentation.armTargets[i];
   assert.ok(Math.hypot(...pose.grip.p.map((x,k)=>x-(k===2?-v[k]:v[k])))<1e-4);
  }
  // Real recovered C# ArmController.Update and Unity component bridge are live.
  // The initial Operator authored arms are kinematic, so source IK still owns
  // their visible poses until an original physical-state transition occurs.
  assert.equal(handState.presentation.originalPhysicalArmController,true);
  assert.equal(handState.presentation.physicalArmState?.length,2);
  for(const arm of handState.presentation.physicalArmState){
   assert.equal(arm.chain.length,3,'Original three limb rigidbodies per arm');
   assert.deepEqual(arm.chain.map(limb=>limb.mass),[4,4,1]);
   assert.ok(arm.chain.every(limb=>limb.isKinematic),
    'Original Operator limb rigidity state, no invented ragdoll');
  }

  for(const side of ['right','left'])assert.equal(handState.weaponState[side].handObjects.filter(v=>v.active).length,1,'Original Hand.SetMode activation must reach renderer');
  await page.screenshot({path:`original-hands-${engine}.png`});
  const particles=await page.evaluate(()=>window.__gameState()?.presentation?.originalParticleSource);
  assert.equal(particles?.sourcePrefabs,14,'Recovered original impact prefab manifest must load');
  assert.equal(particles?.sourceEmitters,26,'26 original native ParticleSystems must be bound');
  assert.equal(particles?.billboardEmitters,14,'Keep original source billboard mode mapping');
  assert.equal(particles?.meshEmitters,12,'Keep original source mesh emitter mapping');
  assert.equal(particles?.renderedParticles,0,'Do not synthesize visually unverified particles');
  assert.equal(particles?.sourceVisualParity,false,'No native Unity particle visual parity proven');
  // Source-checked original Unity Quad windows: 56 glass textures have a
  // transport renderer; 12 custom mirror shaders remain deliberately unrendered.
  const windowVisual=await page.evaluate(async()=>{
    const [{createOriginalWindowVisuals},THREE]=await Promise.all([
      import('./original-window-visuals.mjs'),import('three')]);
    const response=await fetch('./assets/ballistics.json.gz');
    const data=await new Response(response.body.pipeThrough(new DecompressionStream('gzip'))).json();
    const wasm=document.querySelector('iframe[title="Original C# runtime"]')?.contentWindow;
    const info=JSON.parse(await wasm.runtimeCall('OriginalWindowVisuals'));
    const isolated=new THREE.Scene(),view=createOriginalWindowVisuals(isolated,data,info);
    const initial=view.snapshot(),[id,glass]=[...view.registered.entries()].find(([,v])=>!v.mirror);
    const descriptor=info.windows.find(w=>w.sourceObject===id);
    const offset=(glass.width+1)*4+3,initialAlpha=glass.pixels[offset];
    const patch=btoa(String.fromCharCode(30,60,90,0));
    view.update([{type:'windowStamp',value:{sourceObject:id,texture:descriptor.texture,
      x:1,y:1,width:1,height:1,rgba:patch}}]);
    const stamped=Array.from(glass.pixels.subarray(offset-3,offset+1));
    view.update([{type:'windowReset',value:{sourceObject:id,texture:descriptor.texture}}]);
    const resetAlpha=glass.pixels[offset];
    const result={initial,stamped,initialAlpha,resetAlpha,
      after:view.snapshot(),
      exactXY:Math.abs(glass.mesh.matrix.elements[12]-descriptor.matrix[12])<1e-5&&
        Math.abs(glass.mesh.matrix.elements[13]-descriptor.matrix[13])<1e-5,
      reflectedZ:Math.abs(glass.mesh.matrix.elements[14]+descriptor.matrix[14])<1e-5,
      sourceRect:glass.mesh.geometry.parameters.width===1&&glass.mesh.geometry.parameters.height===1,
      visible:isolated.children.length};
    view.dispose();
    result.disposed=isolated.children.length===0;
    return result;
  });
  assert.equal(windowVisual.initial.sourceWindows,68);
  assert.equal(windowVisual.initial.visibleGlass,56);
  assert.equal(windowVisual.initial.unrenderedMirrorShaders,12);
  assert.equal(windowVisual.visible,56);
  assert.equal(windowVisual.exactXY,true,'Recovered Unity window matrices must preserve world X/Y');
  assert.equal(windowVisual.reflectedZ,true,'Browser window must reflect original world Z');
  assert.equal(windowVisual.sourceRect,true,'Original built-in Unity Quad is 1×1');
  assert.deepEqual(windowVisual.stamped,[30,60,90,0],'C# window RGBA stamp pixel transport');
  assert.equal(windowVisual.resetAlpha,windowVisual.initialAlpha,'Original glass texture reset restores source pixels');
  assert.equal(windowVisual.after.stamps,1);
  assert.equal(windowVisual.after.resets,1);
  assert.equal(windowVisual.disposed,true);
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
   const raw=await (await fetch('./assets/ballistics/wood-exit-hole-source.json')).json();
   const points=mesh.geometry.getAttribute('position'),index=mesh.geometry.index;
   const near=(a,b)=>Math.abs(a-b)<1e-7;
   // The source extractor has already converted both raw and compressed
   // original Unity mesh buffers to right-handed THREE vertex coordinates.
   const convertedVertices=raw.mesh.positions.every(([x,y,z],i)=>
     near(points.getX(i),x)&&near(points.getY(i),y)&&near(points.getZ(i),z));
   const convertedWinding=raw.mesh.indices.every((v,i)=>index.getX(i)===v);
   const n=mesh.geometry.getAttribute('normal');
   const convertedNormals=!raw.mesh.normals.length||raw.mesh.normals.every(([x,y,z],i)=>
     near(n.getX(i),x)&&near(n.getY(i),y)&&near(n.getZ(i),z));
   const payload={ready,added:renderer.snapshot().active,
     vertices:points.count,triangles:index.count/3,
     convertedVertices,convertedWinding,convertedNormals,sourceFormat:raw.mesh.source_format,
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
  assert.equal(sourceVisual.sourceFormat,'Unity 2017 compressed mesh packed floats');
  assert.equal(sourceVisual.convertedVertices,true,'Exporter already negated Unity Z: renderer must not do it again');
  assert.equal(sourceVisual.convertedWinding,true,'Exporter already reversed triangle winding');
  assert.equal(sourceVisual.convertedNormals,true,'Exporter already converted original normals where present');
  assert.deepEqual(sourceVisual.position,[1,2,-3],'Unity-to-Three world position conversion');
  assert.deepEqual(sourceVisual.scale,[1,1,.2]);
  assert.equal(sourceVisual.added,1);
  assert.equal(sourceVisual.removed,true,'Source stops rendering when C# frame stops reporting its instance');
  assert.equal(sourceVisual.source,'c0bb8365c7bd54b4597570f085f5a153');
  // Assert a real recovered muzzle texture is ready before the first shot.
  await page.waitForFunction(()=>window.__gameState()?.presentation?.originalMuzzleFlash?.textureLoaded===true,null,{timeout:30000});
  const flashInitial=(await state()).presentation.originalMuzzleFlash;
  assert.ok(flashInitial.sourceAnimationDuration>0,'Recovered original muzzle flash clip must have a positive duration');
  assert.equal(flashInitial.shotsReceived,0,'No muzzle flash without an original C# shot');
  const initial=await state();
  const sourceMagazine=await page.locator('#magazineTouch').evaluate(el=>{
   const img=el.querySelector('[data-mag-image]');
   return {url:img?.getAttribute('src'),loaded:img?.complete&&img.naturalWidth>0,
     shown:el.querySelector('[data-mag-count]')?.textContent,rounds:el.querySelectorAll('.mag-round:not([hidden])').length};
  });
  assert.equal(sourceMagazine.url,'./assets/weapon-runtime/M4-magazine/textures/material_00.png');
  assert.equal(sourceMagazine.loaded,true,'Existing original M4 magazine texture must load in the game browser');
  assert.equal(initial.weaponState.weaponId,'M4A1','The actually executable original weapon host is M4A1');
  assert.equal(initial.weaponState.magazineCapacity,30,'HUD capacity comes from original magazine source');
  assert.equal(Number(sourceMagazine.shown),initial.weaponState.magazine+(initial.weaponState.chamberLive?1:0),
    'HUD must display actual available-to-fire rounds including the source chamber state');
  assert.equal(sourceMagazine.rounds,10,'The magazine window is full while the original magazine has at least ten rounds');
  const lastRoundsProjection=await page.evaluate(async()=>{
    const {magazinePresentation}=await import('./magazine-hud.mjs');
    return {
      three:magazinePresentation({weaponId:'M4A1',magazine:3,magazineCapacity:30,chamberLive:false}),
      last:magazinePresentation({weaponId:'M4A1',magazine:0,magazineCapacity:30,chamberLive:true}),
      glock:magazinePresentation({weaponId:'Glock-18',magazine:5,magazineCapacity:17,chamberLive:true})
    };
  });
  assert.equal(lastRoundsProjection.three.visible,3,'Exactly three last M4 magazine cartridges should be visible');
  assert.equal(lastRoundsProjection.last.ready,1,'The M4 chamber round must not disappear from the ammo number');
  assert.equal(lastRoundsProjection.last.visible,0,'A live chamber round is NOT inside the empty magazine');
  assert.equal(lastRoundsProjection.glock.art.magazineSprite,false,'Glock must not use the M4 cutaway sprite');
  assert.match(lastRoundsProjection.glock.art.texture,/Glock-18/,'Other equipped guns select their own recovered source art');
  assert.equal(initial.weaponState.ballistics.materials,386);assert.equal(initial.weaponState.ballistics.loadedMeshColliders,61);assert.equal(initial.weaponState.ballistics.unsupportedColliders,2);assert.equal(initial.weaponState.shots,0);assert.equal(rounds(initial.weaponState),61);
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
  const firstFlash=tapped.presentation.originalMuzzleFlash;
  assert.equal(firstFlash.textureLoaded,true,'Original source muzzle atlas texture must load');
  assert.equal(firstFlash.textureError,null,'Original flash source texture may not silently fail');
  assert.equal(firstFlash.shotsReceived,1,'One real original shot must trigger exactly one source flash');
  assert.ok(firstFlash.firstFramesApplied>=1,'At least one original sprite frame must be ready for rendering, not expire first');
  assert.ok(firstFlash.maxDrawable>=1,'A flash needs an actual textured drawable frame while alive');
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
  const preReload=await state();await pullOriginalMagazine();
  await page.waitForFunction(()=>window.__gameState().weaponState.reloading,null,{timeout:30000});
  await page.waitForFunction(()=>!window.__gameState().weaponState.reloading,null,{timeout:30000});await ready();
  const reloaded=await state();assert.equal(rounds(reloaded.weaponState),rounds(preReload.weaponState));assert.equal(reloaded.weaponState.magazine,30);assert.equal(reloaded.weaponState.reserve,29);
  const fireBox=await page.locator('#fire').boundingBox();await page.mouse.move(fireBox.x+fireBox.width/2,fireBox.y+fireBox.height/2);await page.mouse.down();
  // Capture the actual live muzzle flash in the original game's render loop.
  // The sprite is short-lived, so trigger a screenshot as soon as its source
  // C# event has produced a textured frame, rather than after fire is released.
  await page.waitForFunction(()=>window.__gameState()?.presentation?.originalMuzzleFlash?.activeDrawable>0,null,{timeout:30000});
  // Short original clips need visual evidence across several browser paints.
  // Keep holding the real fire control; capture consecutive original source
  // frames rather than mislabeling a late empty-weapon screenshot as a flash.
  for(let i=0;i<5;i++){
   await page.screenshot({path:`muzzle-flash-${engine}-${i}.png`});
  }
  await page.waitForFunction(()=>window.__gameState().weaponState.bolt==='Open'&&window.__gameState().weaponState.magazine===0,null,{timeout:30000});await page.mouse.up();
  const empty=await state();assert.equal(empty.weaponState.shots,32);
  assert.equal(empty.presentation.originalMuzzleFlash.shotsReceived,32,
    'Every original fired projectile produces a recovered muzzle effect');
  assert.ok(empty.presentation.originalMuzzleFlash.firstFramesApplied>=32,
    'Every shot must apply its own original sprite frame on its firing update');assert.equal(empty.weaponState.chamberLive,false);
  await page.waitForTimeout(300);assert.equal((await state()).weaponState.shots,32,'No extra shot after release/empty');
  await page.waitForFunction(()=>document.querySelector('#magazineTouch [data-mag-count]')?.textContent==='0',null,{timeout:30000});
  assert.equal(await page.locator('#magazineTouch .mag-round:not([hidden])').count(),0,'Empty original magazine must show no x-ray rounds');
  await pullOriginalMagazine();await page.waitForFunction(()=>window.__gameState().weaponState.reloading,null,{timeout:30000});await page.waitForFunction(()=>!window.__gameState().weaponState.reloading&&window.__gameState().weaponState.chamberLive,null,{timeout:30000});await ready();
  const final=await state();assert.equal(await page.locator('#magazineTouch [data-mag-count]').textContent(),String(final.weaponState.magazine+(final.weaponState.chamberLive?1:0)));
  assert.equal(await page.locator('#magazineTouch [data-mag-chamber]').isVisible(),true,'The live chamber cartridge gets a separate on-screen marker');
  assert.equal(final.weaponState.magazine,28);assert.equal(final.weaponState.reserve,0);assert.equal(final.projection.fov,20);assert.ok(final.presentation.arms.every(a=>Number.isFinite(a.upper)&&Number.isFinite(a.lower)));
  assert.ok(final.weaponState.ballistics.contacts>0,"Original scene material contacts must occur during live firing");assert.ok(Number.isFinite(final.weaponState.ballistics.last.exitEnergy));assert.deepEqual(errors,[]);await page.screenshot({path:`weapon-${engine}.png`});results.push({engine,passed:true,nativeTouchDrag,actualIPhoneTest:false,sourceMuzzleFlash:final.presentation.originalMuzzleFlash,initial,tapped,reloaded,empty,final});
 }catch(e){results.push({engine,passed:false,error:String(e),errors,state:await page.evaluate(()=>window.__gameState?.()||null),screenError:await page.locator('#error').textContent()});await page.screenshot({path:`weapon-${engine}.png`}).catch(()=>{});process.exitCode=1;}
 finally{await browser.close();}
}
fs.writeFileSync('weapon-smoke.json',JSON.stringify(results,null,2));console.log(JSON.stringify(results.map(({engine,passed,error})=>({engine,passed,error}))));

