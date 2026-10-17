import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {spawnOriginalMissionOccupants,spawnOriginalSandboxMissionOccupants} from '../game/source-occupant-mission-integration.mjs';

const source=JSON.parse(readFileSync('game/assets/occupant/source-visual-spawn.json','utf8'));
const calls=[];
const base={
 visualSpawnSource:source,sceneName:'Marksman range',
 random:()=>.5,raycast:()=>false,
 settings:{legacyGroup:{members:[{enabled:true,bodyPrefab:'Occupant'}]}},
 spawnData:{specificGroups:[],randomGroups:[]},campaign:{active:false},
 actorBridge:{async spawnFromOriginalRecord(record){
  calls.push(record);
  await Promise.resolve();
  return {actor:record.sourceData.bodyPrefab,position:record.position};
 }}
};
const result=await spawnOriginalMissionOccupants(base);
assert.equal(result.length,1);
assert.equal(result[0].actor,'Occupant');
assert.equal(calls.length,1);
assert.equal(calls[0].sourceScene,'Marksman range');
assert(calls[0].position.every(Number.isFinite));
assert.equal((await spawnOriginalMissionOccupants({...base,tutorial:true})).length,0);
await assert.rejects(spawnOriginalMissionOccupants({...base,actorBridge:null}),/bridge missing/);
await assert.rejects(spawnOriginalMissionOccupants({...base,settings:null}),/requires original/);
await assert.rejects(spawnOriginalMissionOccupants({...base,raycast:null}),/requires original/);
await assert.rejects(spawnOriginalMissionOccupants({...base,campaign:{active:true,occupants:[{}]}}),/valid source prefab and position/);
// An initialization adapter must only add source state; it cannot relocate
// recovered mission characters or substitute a different prefab.
for(const prepareSourceRecord of [
 record=>({...record,position:[record.position[0]+1,...record.position.slice(1)]}),
 record=>({...record,sourceScene:'Mission 01'}),
 record=>({...record,sourceData:{...record.sourceData,bodyPrefab:'Operator'}}),
 async record=>record,
 record=>{record.position[0]+=1;return record;},
 record=>{record.sourceData.bodyPrefab='Operator';return record;}
]) {
 let actorCalls=0;
 await assert.rejects(spawnOriginalMissionOccupants({...base,prepareSourceRecord,
  actorBridge:{spawnFromOriginalRecord(){actorCalls++;throw Error('actor must not spawn');}}
 }),/preparation changed source|must return a synchronous source record/);
 assert.equal(actorCalls,0);
}
console.log('PASS original mission spawn-to-WASM actor bridge and fail-closed source contracts');

const originalSettings={minCount:3,maxCount:6,morale:{low:0,high:1},
 aggression:{low:0,high:1},fear:{low:0,high:1},
 shockResistance:{low:0,high:.5},weaponSkill:{low:0,high:1},
 handguns:true,smgs:true,shotguns:true,rifles:true};
let floatCalls=0;
const sandbox=await spawnOriginalSandboxMissionOccupants({
 visualSpawnSource:source,sceneName:'Marksman range',
 occupantSettings:originalSettings,availableWeapons:[],
 randomFloat:(a,b)=>(floatCalls++,a+(b-a)*.5),randomInt:(a,b)=>a,
 raycast:()=>false,spawnData:{specificGroups:[],randomGroups:[]},
 campaign:{active:false},actorBridge:base.actorBridge
});
assert.equal(sandbox.length,3);
assert(sandbox.every(x=>x.actor==='Occupant'));
assert(floatCalls>0);
await assert.rejects(spawnOriginalSandboxMissionOccupants({
 visualSpawnSource:source,sceneName:'Marksman range',campaign:{active:true}
}),/inactive campaign/);

let initialized=0;
const integrated=await spawnOriginalSandboxMissionOccupants({
 visualSpawnSource:source,sceneName:'Marksman range',
 occupantSettings:originalSettings,availableWeapons:[],
 randomFloat:(a,b)=>a+(b-a)*.5,randomInt:(a,b)=>a,
 raycast:()=>false,spawnData:{specificGroups:[],randomGroups:[]},
 campaign:{active:false},
 prepareSourceRecord:record=>{initialized++;return {...record,sourceWeaponSkill:.5}},
 actorBridge:{spawnFromOriginalRecord:record=>({weaponSkill:record.sourceWeaponSkill})}
});
assert.equal(integrated.length,3);
assert.equal(initialized,3);
assert(integrated.every(x=>x.weaponSkill===.5));

const nativeEvents=[];
const sourceInit=await spawnOriginalSandboxMissionOccupants({
 visualSpawnSource:source,sceneName:'Marksman range',
 occupantSettings:originalSettings,availableWeapons:[],
 randomFloat:(a,b)=>a+(b-a)*.5,randomInt:(a,b)=>a,
 raycast:()=>false,spawnData:{specificGroups:[],randomGroups:[]},
 campaign:{active:false},
 nativeSourceStateBindings:record=>({
  applyBlackboard:x=>nativeEvents.push(['blackboard',x]),
  applyPhysiology:x=>nativeEvents.push(['physiology',x]),
  applyEquipment:x=>nativeEvents.push(['equipment',x]),
  registerBody:x=>nativeEvents.push(['register',x])
 }),
 actorBridge:{spawnFromOriginalRecord:record=>({
   skill:record.sourceWeaponSkill,blackboard:record.sourceBlackboard,
   physiology:record.sourcePhysiology
 })}
});
assert.equal(sourceInit.length,3);
assert.equal(nativeEvents.length,12);
assert.deepEqual(nativeEvents.slice(0,4).map(x=>x[0]),
 ['blackboard','physiology','equipment','register']);
assert(sourceInit.every(x=>x.skill===.5));
assert(sourceInit.every(x=>x.blackboard.morale===.5));
assert(sourceInit.every(x=>x.blackboard.willBlindFire===false));

assert(sourceInit.every(x=>x.physiology.fitness===.5));
assert(sourceInit.every(x=>x.physiology.shockResistance===.25));
assert(sourceInit.every(x=>x.physiology.shockRecovery===.55));
