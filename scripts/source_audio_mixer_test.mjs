import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { createSourceAudioMixer } from '../game/source-audio-mixer.mjs';

const mixerGuid='mixer-guid';
const group=(fileID,name,volume,pitch,children=[])=>({guid:mixerGuid,fileID,name,fields:{m_GroupID:`group-${fileID}`,m_Volume:volume,m_Pitch:pitch,m_Children:children.map(fileID=>({fileID})),m_Mute:0}});
const snapshot=(fileID,name,values)=>({guid:mixerGuid,fileID,name,fields:{m_FloatValues:values}});
const configuration={
  mixers:[{guid:mixerGuid,fields:{m_StartSnapshot:{fileID:'24500001'},m_TargetSnapshot:{fileID:'24500001'}}}],
  groups:[group('24300000','Master','master-volume','master-pitch',['24300004']),group('24300004','Effects','effects-volume','effects-pitch')],
  snapshots:[snapshot('24500001','Mission',{'master-volume':0,'effects-volume':0,'master-pitch':1,'effects-pitch':1}),
    snapshot('24500002','Paused',{'master-volume':-6,'effects-volume':-6,'master-pitch':2,'effects-pitch':2})]
};

test('authored snapshot transitions interpolate mixer gains and pitch through the recovered parent chain',()=>{
  const mixer=createSourceAudioMixer();
  const transition={action:'transition',time:4,duration:2,snapshotFileID:'24500002',snapshotFields:configuration.snapshots[1].fields};
  assert.deepEqual(mixer.update(configuration,[transition],4),{loaded:true,transitions:1,configurationErrors:0});
  const halfway=mixer.gainFor(mixerGuid,'24300004',5);
  assert.ok(Math.abs(halfway.gain-10**(-6/20))<1e-12);
  assert.equal(halfway.pitch,2.25);
  assert.equal(mixer.snapshot(6).values['master-volume'],-6);
});

test('unknown group identities fail open to neutral output without fabricating a route',()=>{
  const mixer=createSourceAudioMixer();mixer.update(configuration,[],0);
  assert.equal(mixer.gainFor(mixerGuid,'24399999',0),null);
  assert.equal(mixer.snapshot().unknownGroups,1);
});

test('mixer gains above unity are retained for the downstream master limiter',()=>{
  const mixer=createSourceAudioMixer();
  const boosted={...configuration,snapshots:configuration.snapshots.map((item,index)=>index===0?{
    ...item,fields:{m_FloatValues:{...item.fields.m_FloatValues,'master-volume':6,'effects-volume':6}}}:item)};
  mixer.update(boosted,[],0);
  assert.ok(mixer.gainFor(mixerGuid,'24300004',0).gain>1.9);
});

test('malformed mixer configuration is rejected atomically',()=>{
  const mixer=createSourceAudioMixer();
  assert.deepEqual(mixer.update({groups:[{}],snapshots:[],mixers:[]},[],0),{loaded:false,transitions:0,configurationErrors:1});
  assert.equal(mixer.snapshot().groupCount,0);
});

test('all five authored snapshots route the recovered Mission music group through the complete mixer tree',async()=>{
  const source=await readFile(new URL('../source-reference/Original/Assets/AudioMixerController/Master mixer.mixer',import.meta.url),'utf8');
  const headers=[...source.matchAll(/^--- !u!(\d+) &(\d+)[^\n]*\n/gm)];
  const records=headers.map((match,index)=>({classId:match[1],fileID:match[2],
    body:source.slice(match.index+match[0].length,headers[index+1]?.index??source.length)}));
  const mixerRecord=records.find(item=>item.classId==='241');
  const groups=records.filter(item=>item.classId==='243').map(item=>{
    const childrenBlock=item.body.match(/^  m_Children:\s*([\s\S]*?)^  m_Volume:/m)?.[1]??'';
    return {guid:mixerGuid,fileID:item.fileID,name:item.body.match(/^  m_Name: (.*)$/m)?.[1],fields:{
      m_GroupID:item.body.match(/^  m_GroupID: (.*)$/m)?.[1],
      m_Volume:item.body.match(/^  m_Volume: (.*)$/m)?.[1],
      m_Pitch:item.body.match(/^  m_Pitch: (.*)$/m)?.[1],
      m_Children:[...childrenBlock.matchAll(/fileID: (\d+)/g)].map(match=>({fileID:match[1]})),
      m_Mute:Number(item.body.match(/^  m_Mute: (\d+)$/m)?.[1]??0)}};
  });
  const snapshots=records.filter(item=>item.classId==='245').map(item=>{
    const valueBlock=item.body.match(/^  m_FloatValues:[ \t]*\r?\n([\s\S]*?)^  m_TransitionOverrides:/m)?.[1]??'';
    return {guid:mixerGuid,fileID:item.fileID,name:item.body.match(/^  m_Name: (.*)$/m)?.[1],fields:{m_FloatValues:Object.fromEntries(
      [...valueBlock.matchAll(/^    ([0-9a-f]{32}): (-?[0-9.]+)$/gm)].map(match=>[match[1],Number(match[2])]))}};
  });
  const mixerFields={m_StartSnapshot:{fileID:source.match(/^  m_StartSnapshot: \{fileID: (\d+)\}$/m)?.[1]},
    m_TargetSnapshot:{fileID:source.match(/^  m_TargetSnapshot: \{fileID: (\d+)\}$/m)?.[1]}};
  assert.equal(groups.length,19);assert.equal(snapshots.length,5);
  assert.ok(snapshots.every(item=>Object.keys(item.fields.m_FloatValues).length===47));
  const configuration={mixers:[{guid:mixerGuid,fileID:'24100000',fields:mixerFields}],groups,snapshots,expectedGroupCount:19,expectedSnapshotCount:5};
  const mixer=createSourceAudioMixer();
  const initial=snapshots.find(item=>item.fileID===mixerFields.m_StartSnapshot.fileID);
  assert.equal(mixer.update(configuration,[],0).loaded,true);
  const groupId='24300060';
  const groupGainFor=(snapshot)=>{
    const byId=new Map(groups.map(item=>[item.fileID,item]));let id=groupId,db=0;
    while(id){const item=byId.get(id);assert.ok(item,`missing parent group ${id}`);db+=snapshot.fields.m_FloatValues[item.fields.m_Volume];
      id=groups.find(parent=>parent.fields.m_Children.some(child=>child.fileID===id))?.fileID??'';}
    return db<=-80?0:10**(db/20);
  };
  assert.ok(Math.abs(mixer.gainFor(mixerGuid,groupId,0).gain-groupGainFor(initial))<1e-12,
    `expected StartSnapshot ${initial?.name} gain ${groupGainFor(initial)}, got ${JSON.stringify(mixer.gainFor(mixerGuid,groupId,0))}`);
  for(const [index,target] of snapshots.entries()){
    mixer.update(null,[{action:'transition',time:index+1,duration:0,snapshotFileID:target.fileID,snapshotFields:target.fields}],index+1);
    assert.ok(Math.abs(mixer.gainFor(mixerGuid,groupId,index+1).gain-groupGainFor(target))<1e-12,
      `source Mission music route must follow ${target.name} authored volume values`);
  }
  assert.equal(mixer.snapshot().groupCount,19);assert.equal(mixer.snapshot().snapshotCount,5);
});
