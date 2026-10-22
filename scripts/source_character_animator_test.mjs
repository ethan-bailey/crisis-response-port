import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import {SourceCharacterAnimator} from '../game/source-character-animator.mjs';
const json=JSON.parse(fs.readFileSync(new URL('../game/assets/operator/character-animator-controller.json',import.meta.url),'utf8'));
const basic={Moving:false,Backwards:false,KneelRatio:0,CrippledRatio:0,MoveSpeed:0};
test('original Character AnimatorController identity, 4 states and 11 authored transitions',()=>{
 assert.equal(Object.keys(json.states).length,4);
 assert.equal(Object.values(json.states).reduce((n,s)=>n+s.transitions.length,0),11);
 const a=new SourceCharacterAnimator(json);
 let result=a.step(0.02,basic);
 assert.equal(result.state,'Stationary');
 assert.deepEqual(result.clips.map(c=>c.clip),['Body stand']);
 result=a.step(0.02,{KneelRatio:.4});
 assert.equal(result.clips.length,2);
 assert.ok(Math.abs(result.clips.find(c=>c.clip==='Body kneel').weight-.4)<1e-9);
});
test('original state transition duration and forward walk/sprint blend thresholds',()=>{
 const a=new SourceCharacterAnimator(json);
 let r=a.step(.1,{...basic,Moving:true});
 assert.equal(r.transition?.to,'Forward locomotion');
 assert.equal(r.transition?.duration,.5693313);
 r=a.step(.4693313,{...basic,Moving:true,MoveSpeed:.25});
 assert.equal(r.transition,null);
 assert.equal(r.state,'Forward locomotion');
 assert.deepEqual(r.clips.map(c=>[c.clip,c.weight]),[['Body walk',.5],['Body sprint',.5]]);
 r=a.step(.02,{...basic,Moving:true,MoveSpeed:.5});
 assert.equal(r.clips[0].clip,'Body sprint');
 assert.equal(r.clips[0].weight,1);
});
test('original moving/backwards, limp and stationary transitions follow source conditions',()=>{
 const a=new SourceCharacterAnimator(json);
 let r=a.step(.2317524,{...basic,Moving:true,Backwards:true});
 assert.equal(r.state,'Backward locomotion');
 r=a.step(.25,{...basic,Moving:true,Backwards:false,CrippledRatio:.5});
 assert.equal(r.state,'One leg limp');
 assert.deepEqual(r.clips.map(c=>c.clip),['One leg limp']);
 r=a.step(.25,{...basic,Moving:false,Backwards:false});
 assert.equal(r.state,'Stationary');
});
test('no interrupted transitions or invented clip graph',()=>{
 const a=new SourceCharacterAnimator(json);
 let r=a.step(.01,{...basic,Moving:true});
 assert.equal(r.transition.to,'Forward locomotion');
 r=a.step(.1,{...basic,Moving:false,Backwards:true});
 assert.equal(r.transition.to,'Forward locomotion');
 assert.throws(()=>new SourceCharacterAnimator({...json,sourceSha256:'wrong'}));
 assert.throws(()=>a.step(-1,basic));
});
