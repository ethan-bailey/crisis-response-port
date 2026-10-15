import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import {posePosition} from '../game/animation-pose.mjs';
const data=JSON.parse(fs.readFileSync(new URL('../game/assets/operator/animations.json',import.meta.url)));
const pelvis=clip=>clip.tracks.find(t=>t.path==='Pelvis'&&t.property==='position');
test('recovered animator disables root motion',()=>assert.equal(data.animator.applyRootMotion,false));
for(const name of ['Body walk','Body walk backward','One leg limp'])test(name+' does not add looping travel to physics position',()=>{
 const clip=data.clips[name],track=pelvis(clip),first=track.keys[0],last=track.keys.at(-1);
 assert.ok(Math.abs(last.value[0]-first.value[0])>.5,'Actual recovered clip reproduces a large reset without extraction');
 for(let cycle=0;cycle<3;cycle++)for(const key of track.keys){
  const out=posePosition(track,key.value,clip,data.animator.applyRootMotion);
  assert.equal(out[0],first.value[0]);assert.equal(out[1],key.value[1]);assert.equal(out[2],key.value[2]);
 }
 const before=posePosition(track,last.value,clip,false),after=posePosition(track,first.value,clip,false);
 assert.ok(Math.abs(before[0]-after[0])<1e-6,'No horizontal snap at wrap');
 assert.deepEqual(posePosition(track,last.value,clip,true),last.value,'Enabling root motion retains original curve');
});
for(const name of ['Body stand','Body kneel','Body sprint'])test(name+' keeps original pose offsets',()=>{
 const clip=data.clips[name],track=pelvis(clip);
 for(const key of track.keys)assert.deepEqual(posePosition(track,key.value,clip,false),key.value);
});

test('original 0.5 threshold means full sprint, with full weight range',async()=>{
 const {locomotionBlend}=await import('../game/animation-pose.mjs');
 assert.deepEqual(data.locomotion.thresholds,[0,.5]);
 for(const [speed,weight] of [[1.5,0],[2.85,.5],[4.2,1],[5,1]])assert.ok(Math.abs(locomotionBlend(speed,1.5,4.2,data.locomotion.thresholds)-weight)<1e-6);
});
test('walk and sprint maintain common normalized gait phase as weight changes',async()=>{
 const {advanceGait}=await import('../game/animation-pose.mjs');let phase=.9;
 for(let i=0;i<240;i++){
  const g=advanceGait(phase,1/60,i/240,data.clips['Body walk'].duration,data.clips['Body sprint'].duration);
  assert.ok(Math.abs(g.walkTime/data.clips['Body walk'].duration-g.sprintTime/data.clips['Body sprint'].duration)<1e-8);
  assert.ok((g.phase-phase+1)%1<.03);phase=g.phase;
 }
});
