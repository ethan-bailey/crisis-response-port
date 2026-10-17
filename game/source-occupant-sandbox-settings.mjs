// Recovered OccupantSpawner.CreateRandomizedGroup_TEMPORARY,
// CreateSpecificGroup and CreateEquipment_TEMPORARY, sandbox path only.
// Callers supply the actual serialized OccupantSettings and recovered
// EquipmentList.OccupantWeapons; no substitute weapon or configuration.
export function createSourceOccupantSandboxSettings({
 occupantSettings, sceneSpawner, availableWeapons, randomFloat, randomInt,
 createLegacyGroup=true
}={}) {
 if(!occupantSettings || !sceneSpawner ||
    !Array.isArray(availableWeapons) ||
    typeof randomFloat!=='function'||typeof randomInt!=='function')
  throw Error('Original Occupant sandbox settings, weapons and Unity RNG required');
 const s=occupantSettings, f=randomFloat, i=randomInt;
 for(const key of ['morale','aggression','fear','shockResistance','weaponSkill'])
  if(!Number.isFinite(s[key]?.low)||!Number.isFinite(s[key]?.high))
   throw Error('Original Occupant settings range missing: '+key);
 if(!Number.isInteger(s.minCount)||!Number.isInteger(s.maxCount)||
    s.minCount<0||s.maxCount<s.minCount)
  throw Error('Original Occupant count settings invalid');
 if(!Number.isFinite(sceneSpawner.startCoordinate)||!Number.isFinite(sceneSpawner.endCoordinate))
  throw Error('Original OccupantSpawner legacy coordinates missing');
 // Exactly the filtering categories in OccupantSpawner.WeaponTypeEnabled.
 const weaponToggle={Pistol:'handguns',SMG:'smgs',Shotgun:'shotguns',Rifle:'rifles'};
 const weapons=availableWeapons.filter(w=>{
  const toggle=weaponToggle[w?.weaponType];
  if(!toggle) return false;
  if(typeof s[toggle]!=='boolean'||typeof w.name!=='string'||!w.name)
   throw Error('Original occupant weapon category or prefab name missing');
  return s[toggle];
 });
 const range=(a,b)=>{
  if(!Number.isFinite(a)||!Number.isFinite(b))throw Error('Invalid original occupant random range');
  const n=f(a,b);
  if(!Number.isFinite(n))throw Error('Original float RNG returned invalid value');
  return n;
 };
 const integer=(a,b)=>{
  const n=i(a,b);
  if(!Number.isInteger(n)||n<a||n>=b)throw Error('Original integer RNG out of range');
  return n;
 };
 const randomized={
  name:'Occupant',enabled:true,bodyPrefabs:['Occupant'],
  minCount:s.minCount,maxCount:s.maxCount,
  minCoord:sceneSpawner.startCoordinate,maxCoord:sceneSpawner.endCoordinate,
  minMorale:s.morale.low,maxMorale:s.morale.high,
  minAggression:s.aggression.low,maxAggression:s.aggression.high,
  minFear:s.fear.low,maxFear:s.fear.high,
  minFitness:0,maxFitness:1,
  minShockResistance:s.shockResistance.low,maxShockResistance:s.shockResistance.high,
  minBlindFireChance:0,maxBlindFireChance:s.aggression.high,
  minBlindFireDistance:0,maxBlindFireDistance:10*s.aggression.high
 };
 function convertRandomizedGroup(g){
  if(!g?.enabled)return {members:[]};
  if(!Number.isInteger(g.minCount)||!Number.isInteger(g.maxCount)||g.maxCount<g.minCount)
   throw Error('Original randomized group count invalid');
  const count=g.maxCount===g.minCount?g.minCount:integer(g.minCount,g.maxCount);
  const members=[];
  for(let n=0;n<count;n++){
   const member={
    enabled:true,bodyPrefab:'Occupant',
    name:count!==1?`${g.name} ${n+1}`:g.name,
    position:range(g.minCoord,g.maxCoord),
    morale:range(g.minMorale,g.maxMorale),
    aggression:range(g.minAggression,g.maxAggression),
    fear:range(g.minFear,g.maxFear),
    negotiable:false,
    fitness:range(g.minFitness,g.maxFitness),
    shockResistance:range(g.minShockResistance,g.maxShockResistance),
    blindFireChance:range(g.minBlindFireChance,g.maxBlindFireChance),
    blindFireDistance:range(g.minBlindFireDistance,g.maxBlindFireDistance),
    equipment:{primary:weapons.length?{prefabName:weapons[integer(0,weapons.length)].name}:null,
      armor:'None',items:{itemPrefabNames:['Primary magazine','Primary magazine']}}
   };
   members.push(member);
  }
  return {enabled:true,members};
 }
 return {
  legacyGroup:createLegacyGroup?convertRandomizedGroup(randomized):null,
  convertRandomizedGroup
 };
}
