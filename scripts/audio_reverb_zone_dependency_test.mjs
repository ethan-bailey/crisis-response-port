import assert from 'node:assert/strict';
import {readFileSync} from 'node:fs';
import {test} from 'node:test';
import {gunzipSync} from 'node:zlib';

const root=new URL('../',import.meta.url);
const read=path=>readFileSync(new URL(path,root),'utf8');
const scenes=JSON.parse(gunzipSync(readFileSync(new URL('scene-data/crisis-response-scenes.json.gz',root))));
const mission=scenes.assets['Assets/Scenes/Mission 01.unity'];

test('all eight Mission reverb zones retain exact owners, transforms, and enabled state',()=>{
  const expected=[
    ['5584','27','Carpeted room reverb zone','1638',4,4.5],
    ['5585','156','Room reverb zone','2654',2.5,3],
    ['5586','179','Exterior reverb zone','2524',4.5,5],
    ['5587','181','Room reverb zone','2769',3,3.5],
    ['5588','445','Carpeted room reverb zone','2462',4,4.5],
    ['5589','594','Room reverb zone','1996',4,4.5],
    ['5590','744','Carpeted room reverb zone','2568',4,4.5],
    ['5591','1303','Room reverb zone','1897',4,4.5],
  ];
  const actual=mission.records.filter(record=>record.classID===167).map(zone=>{
    const gameObject=mission.records.find(record=>record.classID===1&&record.fileID===zone.fields.m_GameObject.fileID);
    const transform=mission.records.find(record=>(record.classID===4||record.classID===224)&&record.fields?.m_GameObject?.fileID===gameObject.fileID);
    assert.deepEqual(transform.fields.m_LocalScale,{x:1,y:1,z:1});
    assert.equal(transform.fields.m_LocalPosition.y,1.25);
    assert.equal(transform.fields.m_LocalPosition.z,-1.5);
    assert.equal(zone.fields.m_Enabled,1);
    return [zone.fileID,gameObject.fileID,gameObject.fields.m_Name,transform.fileID,zone.fields.m_MinDistance,zone.fields.m_MaxDistance];
  });
  assert.deepEqual(actual,expected);
});

test('Mission reverb fields preserve all authored acoustic values without normalization',()=>{
  const zones=mission.records.filter(record=>record.classID===167);
  assert.ok(zones.every(zone=>zone.fields.m_ReverbPreset===27));
  assert.deepEqual([...new Set(zones.map(zone=>zone.fields.m_Room))].sort((a,b)=>a-b),[-1000,0]);
  assert.deepEqual([...new Set(zones.map(zone=>zone.fields.m_RoomHF))].sort((a,b)=>a-b),[-3500,-1500,-1000,-300]);
  assert.deepEqual([...new Set(zones.map(zone=>zone.fields.m_DecayTime))].sort((a,b)=>a-b),[0.15,0.2,0.25,0.5]);
  assert.deepEqual([...new Set(zones.map(zone=>zone.fields.m_DecayHFRatio))].sort((a,b)=>a-b),[0.417,0.798]);
  assert.deepEqual([...new Set(zones.map(zone=>zone.fields.m_Reflections))].sort((a,b)=>a-b),[-4649,-1363]);
  assert.deepEqual([...new Set(zones.map(zone=>zone.fields.m_Reverb))].sort((a,b)=>a-b),[0,53]);
});

test('engine imports every serialized AudioReverbZone field as data only',()=>{
  const audio=read('runtime/Engine/Audio.cs');
  assert.match(audio,/sealed class AudioReverbZone:Behaviour/);
  const loader=read('runtime/Engine/SceneLoader.cs');
  assert.match(loader,/case 167:component=go\.AddComponent<AudioReverbZone>\(\)/);
  for(const field of ['m_MinDistance','m_MaxDistance','m_ReverbPreset','m_Room','m_RoomHF','m_DecayTime','m_DecayHFRatio',
    'm_Reflections','m_ReflectionsDelay','m_Reverb','m_ReverbDelay','m_HFReference','m_Diffusion','m_Density','m_LFReference','m_RoomLF'])
    assert.match(loader,new RegExp('SetField\\("'+field+'"'));
  assert.doesNotMatch(audio,/Convolver|impulse|process|render/i,'engine model must not invent a reverb algorithm');
});

test('browser bridge transports zone definitions without claiming spatial output',()=>{
  const bridge=read('runtime/Engine/BrowserAudioEventQueue.cs');
  assert.match(bridge,/world\.Objects\.OfType<AudioReverbZone>\(\)/);
  assert.match(bridge,/groups,snapshots,reverbZones/);
  const browser=read('game/weapon-audio-runtime.mjs');
  assert.doesNotMatch(browser,/AudioReverbZone|reverbZones|createConvolver|ConvolverNode/,
    'WebAudio reverb remains deferred until a source-backed implementation exists');
});
