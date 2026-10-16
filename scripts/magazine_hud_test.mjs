import test from "node:test";
import assert from "node:assert/strict";
import fs from "node:fs";
import {magazineView,isDropGesture,bindMagazineDrag,ORIGINAL_M4_MAGAZINE_TEXTURE} from "../game/magazine-hud.mjs";

test("uses original recovered M4 magazine texture, without generating images",()=>{
 assert.equal(ORIGINAL_M4_MAGAZINE_TEXTURE,"./assets/weapon-runtime/M4-magazine/textures/material_00.png");
 assert.ok(fs.statSync("game/assets/weapon-runtime/M4-magazine/textures/material_00.png").size>0);
 const original=JSON.parse(fs.readFileSync("game/assets/weapon-runtime/M4-magazine/weapon.json","utf8"));
 assert.equal(original.nodes[0].name,"Magazine AR 30 round");
 assert.ok(original.renderers.some(r=>r.texture==="textures/material_00.png"));
});
test("ammo display derives from source magazine count; unknown never guesses",()=>{
 assert.deepEqual(magazineView(null),{known:false,count:null,capacity:30,visible:0,label:"Magazine ammunition unavailable"});
 assert.equal(magazineView(30).visible,10);
 assert.equal(magazineView(18).visible,6);
 assert.equal(magazineView(1).visible,1);
 assert.equal(magazineView(0).visible,0);
 assert.equal(magazineView(29).label,"M4 magazine: 29 of 30 rounds");
 assert.equal(magazineView(31).known,false);
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
