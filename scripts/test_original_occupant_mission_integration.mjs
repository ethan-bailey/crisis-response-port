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
