// Mission NPC runtime boundary: no placeholder positions, AI, or collision results.
// Call only after original Global.Context/OccupantSettings and Unity-compatible
// random/collision queries have been recovered and installed.
import {createSourceOccupantSpawner} from './source-occupant-spawner.mjs';
import {createSourceOccupantSandboxSettings} from './source-occupant-sandbox-settings.mjs';

export async function spawnOriginalMissionOccupants({
  visualSpawnSource,sceneName,random,raycast,settings,spawnData,
  campaign,tutorial=false,actorBridge
}={}) {
  if(!actorBridge || typeof actorBridge.spawnFromOriginalRecord!=='function')
    throw Error('Original mission NPC visual/physics bridge missing');
  // Never interpret an absent campaign or global occupant configuration as
  // an empty mission. Missing source configuration is an integration error.
  const sourceSpawner=createSourceOccupantSpawner(visualSpawnSource,sceneName,{
    random,raycast,settings,spawnData,campaign,tutorial,
    spawnBody:record=>{
      if(!record?.sourceData?.bodyPrefab ||
         !Array.isArray(record.position) || record.position.length!==3 ||
         !record.position.every(Number.isFinite))
        throw Error('Original mission NPC record lacks a valid source prefab and position');
      return actorBridge.spawnFromOriginalRecord(record);
    }
  });
  // The recovered spawner returns actor creation calls in source group order.
  // Browser actor construction is asynchronous; await every registration before
  // reporting completion, rather than treating pending promises as actors.
  const pending=sourceSpawner.spawn();
  if(!Array.isArray(pending))throw Error('Original source spawner did not return actors');
  return Promise.all(pending);
}

// Assemble sandbox-only spawn dependencies from the recovered scene and exact
// user OccupantSettings/EquipmentList. Caller owns Unity RNG and ballistic
// collision queries. Campaign actors use a different original source path.
export async function spawnOriginalSandboxMissionOccupants({
  visualSpawnSource,sceneName,occupantSettings,availableWeapons,
  randomFloat,randomInt,raycast,spawnData,campaign,tutorial=false,actorBridge
}={}) {
  if(!campaign || campaign.active!==false)
    throw Error('Original sandbox mission requires explicit inactive campaign state');
  const scene=visualSpawnSource?.sceneSpawners?.[sceneName]?.spawner?.fields;
  if(!scene)throw Error('Original serialized OccupantSpawner scene missing: '+sceneName);
  // Source consumes this conversion only for enabled legacy groups; random
  // groups are converted individually by the original spawner.
  const settings=createSourceOccupantSandboxSettings({
    occupantSettings,sceneSpawner:scene,availableWeapons,randomFloat,randomInt,
    createLegacyGroup:!!scene.spawnLegacyGroup && !tutorial
  });
  return spawnOriginalMissionOccupants({visualSpawnSource,sceneName,
    random:()=>randomFloat(0,1),raycast,settings,spawnData,campaign,tutorial,actorBridge});
}
