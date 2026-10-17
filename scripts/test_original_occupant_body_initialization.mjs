import assert from 'node:assert/strict';
import {applyOriginalOccupantSpawnState} from '../game/source-occupant-body-initialization.mjs';
const calls=[],order=[];
const record={sourceData:{morale:.4,aggression:.7,fear:.2,blindFireChance:.6,
 blindFireDistance:4,negotiable:false,fitness:.8,shockResistance:.35,
 equipment:{primary:{prefabName:'Original pistol'}}}};
const result=applyOriginalOccupantSpawnState(record,{
 randomFloat:(a,b)=>(order.push([a,b]),a+(b-a)*.5),
 occupantSettings:{weaponSkill:{low:.1,high:.9}},
 applyBlackboard:x=>calls.push(['blackboard',x]),
 applyPhysiology:x=>calls.push(['physiology',x]),
 applyEquipment:x=>calls.push(['equipment',x]),
 registerBody:x=>calls.push(['register',x])
});
assert.deepEqual(order,[[.1,1],[0,1],[.1,.9],[0,1],[.1,.9]]);
assert.deepEqual(calls.map(x=>x[0]),['blackboard','physiology','equipment','register']);
assert.equal(result.blindFire,true);
assert.equal(result.weaponSkill,.5);
assert.equal(result.recoilCompensation[0],.75);
assert.equal(result.shockRecovery,.55);
assert.equal(calls[0][1].morale,.4);
assert.equal(calls[1][1].fitness,.8);
assert.equal(calls[2][1].primary.prefabName,'Original pistol');
assert.throws(()=>applyOriginalOccupantSpawnState(record,{}),/requires source/);
assert.throws(()=>applyOriginalOccupantSpawnState(record,{
 randomFloat:()=>NaN,occupantSettings:{weaponSkill:{low:0,high:1}},
 applyBlackboard(){},applyPhysiology(){},applyEquipment(){},registerBody(){}
}),/invalid original Occupant RNG/i);
console.log('PASS original Occupant spawn personality, source RNG order, physiology and equipment callbacks');
