import assert from 'node:assert/strict';
import {createSourceOccupantSandboxSettings} from '../game/source-occupant-sandbox-settings.mjs';
const s={minCount:3,maxCount:6,morale:{low:0,high:1},
 aggression:{low:0,high:1},fear:{low:0,high:1},
 shockResistance:{low:0,high:.5},weaponSkill:{low:0,high:1},
 handguns:true,smgs:false,shotguns:false,rifles:false};
let calls=0;
const args={occupantSettings:s,sceneSpawner:{startCoordinate:1,endCoordinate:30},
 availableWeapons:[{name:'Original pistol',weaponType:'Pistol'},{name:'Original rifle',weaponType:'Rifle'}],
 randomInt:(low,high)=>low,randomFloat:(low,high)=>(calls++,low)};
const settings=createSourceOccupantSandboxSettings(args);
assert.equal(settings.legacyGroup.members.length,3);
assert.equal(settings.legacyGroup.members[0].name,'Occupant 1');
assert.equal(settings.legacyGroup.members[0].position,1);
assert.equal(settings.legacyGroup.members[0].equipment.primary.prefabName,'Original pistol');
assert.deepEqual(settings.legacyGroup.members[0].equipment.items.itemPrefabNames,
 ['Primary magazine','Primary magazine']);
assert.equal(settings.legacyGroup.members[0].negotiable,false);
assert.equal(settings.legacyGroup.members[2].shockResistance,0);
assert(calls>0);
const unarmed=createSourceOccupantSandboxSettings({...args,availableWeapons:[]});
assert.equal(unarmed.legacyGroup.members[0].equipment.primary,null);
assert.throws(()=>createSourceOccupantSandboxSettings({...args,occupantSettings:null}),/required/);
assert.throws(()=>createSourceOccupantSandboxSettings({...args,randomInt:()=>99}),/out of range/);
console.log('PASS original default sandbox count, authored RNG attributes, equipment, weapons and fail-closed settings');
