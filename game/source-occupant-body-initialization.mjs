// Original OccupantSpawner.SpawnSandboxCharacter personality and weapon-skill
// initialization. Never substitute missing native body callbacks.
export function applyOriginalOccupantSpawnState(record,{
  randomFloat,applyBlackboard,applyPhysiology,applyEquipment,registerBody,
  occupantSettings
}={}) {
 const data=record?.sourceData;
 if(!data||typeof randomFloat!=='function'||typeof applyBlackboard!=='function'||
    typeof applyPhysiology!=='function'||typeof applyEquipment!=='function'||
    typeof registerBody!=='function'||!occupantSettings?.weaponSkill)
  throw Error('Original Occupant state requires source spawn data and native body bindings');
 const f=(a,b)=>{
  if(!Number.isFinite(a)||!Number.isFinite(b))throw Error('Invalid source Occupant random range');
  const value=randomFloat(a,b);
  if(!Number.isFinite(value))throw Error('Invalid original Occupant RNG result');
  return value;
 };
 const weaponSkill=f(occupantSettings.weaponSkill.low,occupantSettings.weaponSkill.high);
 const compensationX=weaponSkill+(1-weaponSkill)*f(0,1);
 const compensationY=Math.min(compensationX+
   (1-f(occupantSettings.weaponSkill.low,occupantSettings.weaponSkill.high))*3,3);
 // Source order: shock recovery, blind fire chance, SetWeaponSkill.
 // Calculate before mutating the C# body so failed validation cannot partially
 // install source behavior.
 const shockRecovery=f(.1,1);
 const blindFire=f(0,1)<data.blindFireChance;
 applyBlackboard({
  morale:data.morale,aggression:data.aggression,fear:data.fear,
  willBlindFire:blindFire,blindFireDistance:data.blindFireDistance,
  negotiable:data.negotiable,weaponSkill,recoilCompensation:[compensationX,compensationY]
 });
 applyPhysiology({fitness:data.fitness,shockResistance:data.shockResistance,shockRecovery});
 applyEquipment(data.equipment);
 registerBody(record);
 return {weaponSkill,recoilCompensation:[compensationX,compensationY],shockRecovery,blindFire};
}
