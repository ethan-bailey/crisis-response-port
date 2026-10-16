import test from "node:test";
import assert from "node:assert/strict";
import fs from "node:fs";
import {magazineView,magazinePresentation,isDropGesture,bindMagazineDrag,ORIGINAL_M4_MAGAZINE_TEXTURE,SOURCE_WEAPON_VISUALS} from "../game/magazine-hud.mjs";

test("uses original recovered M4 magazine texture, without generating images",()=>{
 assert.equal(ORIGINAL_M4_MAGAZINE_TEXTURE,"./assets/weapon-runtime/M4-magazine/textures/material_00.png");
 assert.ok(fs.statSync("game/assets/weapon-runtime/M4-magazine/textures/material_00.png").size>0);
 const original=JSON.parse(fs.readFileSync("game/assets/weapon-runtime/M4-magazine/weapon.json","utf8"));
 assert.equal(original.nodes[0].name,"Magazine AR 30 round");
 assert.ok(original.renderers.some(r=>r.texture==="textures/material_00.png"));
});
test("final ten real magazine rounds each appear one-for-one in HUD",()=>{
 const unknown=magazineView(null);
 assert.equal(unknown.known,false);
 assert.equal(unknown.ready,null);
 assert.equal(magazineView(30).visible,10);
 assert.equal(magazineView(18).visible,10);
 for(let count=0;count<=10;count++){
  const state=magazineView(count);
  assert.equal(state.visible,count,`Exact last-ten cartridges at ${count} must be one-for-one`);
  assert.equal(state.ready,count);
 }
 assert.equal(magazineView(11).visible,10);
 assert.equal(magazineView(31).known,false);
});
test("chambered round stays visible and counted when original magazine is empty",()=>{
 const three=magazineView(3,30,10,true);
 assert.equal(three.count,3); assert.equal(three.chamber,1);
 assert.equal(three.visible,3); assert.equal(three.ready,4);
 assert.match(three.label,/3 in magazine, 1 chambered; 4 ready/);
 const last=magazineView(0,30,10,true);
 assert.equal(last.visible,0);
 assert.equal(last.chamber,1);
 assert.equal(last.ready,1);
 const spent=magazineView(0,30,10,false);
 assert.equal(spent.visible,0); assert.equal(spent.ready,0);
});
test("equipped gun determines art, calibre, and source-owned capacity, never reuses M4 asset for another weapon",()=>{
 assert.equal(SOURCE_WEAPON_VISUALS.M4A1.caliber,"5.56");
 assert.equal(SOURCE_WEAPON_VISUALS["Glock-18"].caliber,"9mm");
 const rifle=magazinePresentation({weaponId:"M4A1",magazine:3,chamberLive:true,magazineCapacity:30});
 assert.equal(rifle.ready,4); assert.equal(rifle.visible,3);
 assert.equal(rifle.art.texture,ORIGINAL_M4_MAGAZINE_TEXTURE);
 assert.equal(rifle.art.magazineSprite,true);
 const pistol=magazinePresentation({weaponId:"Glock-18",magazine:4,chamberLive:true,magazineCapacity:17});
 assert.equal(pistol.capacity,17);
 assert.equal(pistol.ready,5);
 assert.equal(pistol.art.magazineSprite,false,"Recovered Glock has gun texture but no verified standalone magazine texture");
 assert.match(pistol.art.texture,/Glock-18\/textures\/material_00.png/);
 assert.equal(fs.statSync("game/assets/weapons/Glock-18/textures/material_00.png").size>0,true);
 assert.ok(!pistol.art.texture.includes("M4-magazine"));
 const unknown=magazinePresentation({weaponId:"Other",magazine:5,magazineCapacity:14});
 assert.equal(unknown.art,null);assert.equal(unknown.ready,5);
 const absent=magazinePresentation({magazine:30});
 assert.equal(absent.known,false);
 assert.equal(absent.art,null);
});
test("reload gesture requires a real pull downward, not taps or side swipes",()=>{
 assert.equal(isDropGesture(0,45),true);
 assert.equal(isDropGesture(0,44),false);
 assert.equal(isDropGesture(60,46),false);
 assert.equal(isDropGesture(-15,60),true);
 assert.equal(isDropGesture(0,-60),false);
});
class Surface extends EventTarget {
 captures=new Set();
 setPointerCapture(id){this.captures.add(id)}
 hasPointerCapture(id){return this.captures.has(id)}
 releasePointerCapture(id){this.captures.delete(id)}
 send(type,{pointerId=1,clientX=0,clientY=0,pointerType="touch",button=0}={}){
  const ev=new Event(type,{cancelable:true});
  Object.assign(ev,{pointerId,clientX,clientY,pointerType,button});
  this.dispatchEvent(ev);
 }
}
test("releases on completed drag exactly once; ignores second finger, tap and cancel",()=>{
 const el=new Surface(), pulls=[],fires=[];
 const binding=bindMagazineDrag(el,()=>fires.push("reload"),{onPull:v=>pulls.push(v)});
 el.send("pointerdown",{pointerId:5});
 el.send("pointerdown",{pointerId:6});
 el.send("pointermove",{pointerId:6,clientY:90});
 el.send("pointermove",{pointerId:5,clientY:56});
 el.send("pointerup",{pointerId:6,clientY:90});
 assert.deepEqual(fires,[]);
 el.send("pointerup",{pointerId:5,clientY:56});
 assert.deepEqual(fires,["reload"]);
 el.send("pointerdown",{pointerId:7});el.send("pointerup",{pointerId:7});
 el.send("pointerdown",{pointerId:8});el.send("pointermove",{pointerId:8,clientY:80});el.send("pointercancel",{pointerId:8,clientY:80});
 assert.deepEqual(fires,["reload"]);
 assert.equal(pulls.at(-1),0);
 binding.dispose();
 el.send("pointerdown",{pointerId:9});el.send("pointerup",{pointerId:9,clientY:80});
 assert.deepEqual(fires,["reload"]);
});
test("magazine gesture cannot start when reload state is active",()=>{
 const el=new Surface();let blocked=true,calls=0;
 const binding=bindMagazineDrag(el,()=>calls++,{canDrop:()=>!blocked});
 el.send("pointerdown",{pointerId:1});el.send("pointerup",{pointerId:1,clientY:90});
 assert.equal(calls,0);
 blocked=false;
 el.send("pointerdown",{pointerId:2});el.send("pointerup",{pointerId:2,clientY:90});
 assert.equal(calls,1);
 binding.dispose();
});

test("game HUD uses original magazine texture, original StepWeapons state and magazine input",()=>{
 const src=fs.readFileSync("game/index.html","utf8");
 assert.match(src,/createOriginalM4MagazineHud\(document.querySelector\("#magazineTouch"\)/);
 assert.match(src,/magazineHud\.render\(weaponState\)/);
 assert.match(src,/onReload:\(\)=>\{reloadPending=true;/);
 assert.match(src,/data-mag-image/);
 assert.match(src,/data-mag-rounds/);
 assert.match(src,/data-mag-chamber/);
 assert.match(src,/magazineHud\.render\(weaponState\)/);
 assert.match(src,/weaponState\.weaponId/);
 const host=fs.readFileSync("runtime/Engine/WeaponHost.cs","utf8");
 assert.match(host,/weaponId="M4A1",magazineCapacity=weapon\.Magazine\?\.capacity/);
 assert.match(host,/chamberLive=weapon\.cyclingBolt\.Live/);
 assert.doesNotMatch(src, /id="reload"\s+data-action="magazine"/,"Avoid duplicate reload trigger over the magazine");
 assert.match(src, /KeyR:"magazine"/,"Original keyboard reload retained");
});
