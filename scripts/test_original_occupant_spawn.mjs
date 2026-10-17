import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {createSourceOccupantSpawner} from '../game/source-occupant-spawner.mjs';
const source=JSON.parse(readFileSync('game/assets/occupant/source-visual-spawn.json','utf8'));
const calls=[],rand=()=>.5;
const setting={legacyGroup:{members:[{enabled:true,bodyPrefab:'Occupant',name:'Source test subject'}]}};
const base={random:rand,raycast:()=>false,spawnBody:x=>(calls.push(x),x),
 settings:setting,spawnData:{specificGroups:[],randomGroups:[]},campaign:{active:false},tutorial:false};
const scene=createSourceOccupantSpawner(source,'Marksman range',base);
assert.equal(scene.areas.length,7);
assert(Math.abs(scene.areas.reduce((sum,a)=>sum+a.probability,0)-1)<1e-5);
let actors=scene.spawn();
assert.equal(actors.length,1);
assert.equal(actors[0].sourceData.bodyPrefab,'Occupant');
assert(actors[0].position[0]>=actors[0].patrolAreaLeft);
assert.equal(actors[0].sourceWeight,80);
const mission=createSourceOccupantSpawner(source,'Mission 01',base);
assert.equal(mission.areas.length,1);
assert.equal(mission.areas[0].probability,0);
assert.equal(mission.spawn().length,1);
assert.throws(()=>createSourceOccupantSpawner(source,'Mission 01',{}),/requires original/);
const empty=createSourceOccupantSpawner(source,'Mission 01',{...base,tutorial:true});
assert.equal(empty.spawn().length,0);
console.log('PASS source serialized scene spawner paths, original group, area and strict dependency checks');

// Source draw order: area choice, X and Z position, obstacle sign,
// body weight, then non-Marksman patrol span.
const draws=[];
const testMission=createSourceOccupantSpawner(source,'Mission 01',{
 ...base,random:()=>{draws.push(1);return .5}
}).spawn();
assert.equal(testMission[0].sourceWeight,80);
assert.equal(draws.length,6);
const broken=structuredClone(source);
broken.sceneSpawners['Marksman range'].spawner.fields.areas=[];
assert.throws(()=>createSourceOccupantSpawner(broken,'Marksman range',base),/authored spawn areas/);

// A source RNG or ballistic collision adapter failure cannot silently place a body.
let rejectedSpawns=0;
const rejects={...base,spawnBody:()=>{rejectedSpawns++;}};
assert.throws(()=>createSourceOccupantSpawner(source,'Marksman range',{
 ...rejects,random:()=>NaN}).spawn(),/Unity random draw/);
assert.throws(()=>createSourceOccupantSpawner(source,'Marksman range',{
 ...rejects,random:()=>1.4}).spawn(),/Unity random draw/);
assert.throws(()=>createSourceOccupantSpawner(source,'Marksman range',{
 ...rejects,raycast:()=>undefined}).spawn(),/collision query must return a boolean/);
assert.equal(rejectedSpawns,0,'invalid source queries may not instantiate native actors');
// Incomplete later group cannot leave earlier valid actors partially registered.
const invalidSpawn={...base,spawnBody:()=>{rejectedSpawns++;},settings:{
 legacyGroup:{members:[{enabled:true,bodyPrefab:'Occupant'}, {enabled:true}]}
}};
assert.throws(()=>createSourceOccupantSpawner(source,'Marksman range',invalidSpawn).spawn(),/bodyPrefab missing/);
assert.equal(rejectedSpawns,0,'source group preflight must precede all native registrations');
const badConfig=structuredClone(source);
badConfig.sceneSpawners['Marksman range'].spawner.fields.bodyClearRadius=-1;
assert.throws(()=>createSourceOccupantSpawner(badConfig,'Marksman range',base),/obstacle configuration invalid/);
console.log('PASS source RNG, collision-mask response, full group preflight, and obstacle constraints');
