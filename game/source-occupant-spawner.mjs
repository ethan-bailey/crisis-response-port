// Source-port of OccupantSpawner.InitializeSpawnAreas, RandomSpawnArea,
// SpawnSandboxGroup and SpawnSandboxCharacter's position selection. A caller
// must provide real Settings/Global.Context data and source collision mask.
// Never create a fallback NPC, location, weapon, or behavior.
export function createSourceOccupantSpawner(source,sceneName,{random,raycast,spawnBody,settings,spawnData,campaign,tutorial}={}){
 const scene=source?.sceneSpawners?.[sceneName]?.spawner?.fields;
 if(!scene)throw Error('Original serialized OccupantSpawner scene missing: '+sceneName);
 if(tutorial)return {spawn:()=>[],areas:[],sourceScene:sceneName};
 if(typeof random!=='function'||typeof raycast!=='function'||typeof spawnBody!=='function'||
    !settings||!spawnData||campaign===undefined)
  throw Error('Source OccupantSpawner requires original Unity RNG, collision query, prefab instantiation and mission context');
 const areas=scene.areas.map(area=>({...area}));
 let accumulated=0;
 for(const area of areas){area.probabilityLow=accumulated;accumulated+=area.probability;area.probabilityHigh=accumulated;}
 const choose=()=>{const sample=random()*accumulated;
  return areas.find(area=>sample>=area.probabilityLow&&sample<area.probabilityHigh)||areas[0];};
 const adjust=(position,area)=>{
  let num=0,num2=0,flag=false;
  // Source consumes this UnityEngine.Random.Range(-1,1) call, even though num3
  // is unused by recovered OccupantSpawner.AdjustPositionForObstacles.
  Math.sign(random()*2-1);
  const clear=scene.bodyClearRadius;
  const occupied=()=>raycast({origin:[position[0]+num2-clear,position[1],position[2]],direction:[1,0,0],
    distance:clear*2,mask:'BallisticTargets'});
  while(occupied()&&num<scene.maxTries){
   if(flag){num2+=scene.collisionIncrement*Math.sign(num2);flag=false;}
   else {flag=true;num2*=-1;}
   num++;
  }
  position[0]+=num2;
  return num<scene.maxTries&&position[0]>=area.xMin&&position[0]<=area.xMax;
 };
 const positionFor=(data,randomize=true)=>{
  const area=choose();if(!area)throw Error('Original OccupantSpawner has no spawn areas');
  const position=randomize?[area.xMin+random()*(area.xMax-area.xMin),area.y,area.zMin+random()*(area.zMax-area.zMin)]
    :[data.position,1.25,-1.5];
  if(randomize&&!adjust(position,area))return null;
  return {position,area};
 };
 const spawnOne=(data,randomize=true)=>{
  if(!data?.enabled)return null;
  if(!data.bodyPrefab)throw Error('Original SerializableOccupantSpawn bodyPrefab missing');
  const info=positionFor(data,randomize);if(!info)return null;
  const patrolSpan=(info.area.xMax-info.area.xMin)*(sceneName==='Marksman range'?1:random());
  return spawnBody({sourceData:data,sourceScene:sceneName,position:info.position,
   patrolAreaLeft:Math.max(info.area.xMin,info.position[0]-patrolSpan*.5),
   patrolAreaRight:Math.min(info.area.xMax,info.position[0]+patrolSpan*.5),
   walkableAreaLeft:info.area.walkableMin,walkableAreaRight:info.area.walkableMax});
 };
 return {sourceScene:sceneName,areas,spawn(){
  if(campaign.active){
   if(!Array.isArray(campaign.occupants))throw Error('Original campaign occupant templates missing');
   return campaign.occupants.map(data=>spawnBody({sourceScene:sceneName,sourceCampaignTemplate:data})).filter(Boolean);
  }
  if(scene.spawnLegacyGroup&&!settings.legacyGroup)
   throw Error('Original OccupantSpawner legacy group Settings/Ocсupant settings unavailable');
  const groups=[];
  if(scene.spawnLegacyGroup)groups.push(settings.legacyGroup);
  for(const group of spawnData.specificGroups||[]){if(group.enabled)groups.push(group);}
  for(const group of spawnData.randomGroups||[]){
   if(group.enabled){
    if(typeof settings.convertRandomizedGroup!=='function')
     throw Error('Original CreateSpecificGroup group conversion binding unavailable');
    groups.push(settings.convertRandomizedGroup(group));
   }
  }
  const actors=[];for(const group of groups)for(const member of group.members||[]){
   if(member.enabled){const actor=spawnOne(member);if(actor)actors.push(actor);}
  }
  return actors;
 },spawnOne};
}
