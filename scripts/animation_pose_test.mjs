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
