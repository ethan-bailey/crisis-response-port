// Mission NPC runtime boundary: no placeholder positions, AI, or collision results.
// Call only after original Global.Context/OccupantSettings and Unity-compatible
// random/collision queries have been recovered and installed.
import {createSourceOccupantSpawner} from './source-occupant-spawner.mjs';

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
