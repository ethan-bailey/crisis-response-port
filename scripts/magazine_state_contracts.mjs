import assert from 'node:assert/strict';
import {magazineView,magazinePresentation} from '../game/magazine-hud.mjs';
// Strict source state to HUD contracts; no ammo simulation is added here.
const source=(magazine,chamberLive)=>({weaponId:'M4A1',magazine,magazineCapacity:30,chamberLive});
for(const [magazine,chamberLive,ready,visible] of [
 [30,false,30,10],[29,true,30,10],[11,true,12,10],
 [10,true,11,10],[1,true,2,1],[0,true,1,0],[0,false,0,0],
 [1,false,1,1],[30,true,31,10]
]){
 const view=magazinePresentation(source(magazine,chamberLive));
 assert.equal(view.known,true);
 assert.equal(view.ready,ready);
 assert.equal(view.visible,visible);
 assert.equal(view.count,magazine);
 assert.equal(view.chamber,chamberLive?1:0);
 assert.equal(magazineView(magazine,30,10,chamberLive).ready,ready);
}
for(const bad of [-1,31,1.5,undefined]){
 assert.equal(magazinePresentation(source(bad,true)).known,false);
}
assert.equal(magazinePresentation({weaponId:'Unsupported',magazine:0,chamberLive:true}).art,null);
console.log('Original-source chamber-inclusive ammo UI contracts: 9 valid + 4 invalid + unsupported weapon passed');
