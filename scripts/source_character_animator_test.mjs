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


const fullSourceBridge=fs.readFileSync(new URL('../runtime/Engine/FullSourceActorBridge.cs',import.meta.url),'utf8');
const originalHand=fs.readFileSync(new URL('../runtime/Original/FullSource/Hand.cs',import.meta.url),'utf8');
const handRenderers=fs.readFileSync(new URL('../game/original-hand-renderers.mjs',import.meta.url),'utf8');
const page=fs.readFileSync(new URL('../game/index.html',import.meta.url),'utf8');
const operator=JSON.parse(fs.readFileSync(new URL('../game/assets/operator/operator.json',import.meta.url),'utf8'));
const animations=JSON.parse(fs.readFileSync(new URL('../game/assets/operator/animations.json',import.meta.url),'utf8'));
test('FullSource Mission frame samples original body clips after source pose projection without resetting source IK pose',()=>{
 const frameStart=page.indexOf('async function frame(');
 const branchStart=page.indexOf('if(sourceActorMode){',frameStart);
 const legacyStart=page.indexOf('const fireAimReady=',branchStart);
 assert.ok(frameStart>=0&&branchStart>frameStart&&legacyStart>branchStart,'source actor frame branch must exist');
 const sourceFrame=page.slice(branchStart,legacyStart);
 assert.match(sourceFrame,/projectFullSourceSnapshot\(sourceActorSnapshot\);await applyAnimation\(dt,true\);/,
  'source snapshot must be projected before the authored AnimatorController pose is sampled');
 const applyStart=page.indexOf('applyAnimation=async function(dt,preserveSourcePose=false)');
 const applyEnd=page.indexOf('rightUpperArm=nodeByPath',applyStart);
 assert.ok(applyStart>=0&&applyEnd>applyStart,'authored pose sampler must be available to the source frame');
 const sampler=page.slice(applyStart,applyEnd);
 assert.match(sampler,/if\(!preserveSourcePose\)resetPose\(\);/,
  'source pose mode must preserve projected transforms outside the authored clip tracks');
 const nodePaths=new Set(operator.nodes.map(node=>node.path));
 const trackPaths=new Set(Object.values(animations.clips).flatMap(clip=>clip.tracks.map(track=>track.path)));
 assert.ok(trackPaths.has('Pelvis')&&trackPaths.has('Pelvis/Left upper leg/Left lower leg'),
  'the original clips must continue to animate pelvis and leg transforms');
 for(const path of trackPaths)assert.ok(nodePaths.has(path),'original clip target is absent from Operator prefab: '+path);
 assert.ok(![...trackPaths].some(path=>/Right upper arm|Left upper arm|Right hand|Left hand/.test(path)),
  'the body clip sampler must not overwrite the source IK hand and arm transforms');
});

test('FullSource aim target helper is visible to the top-level Mission frame',()=>{
 const loadStart=page.indexOf('async function load(){');
 const frameStart=page.indexOf('async function frame(');
 const aimStart=page.indexOf('async function aimQuadTarget(){');
 const aimEnd=page.indexOf('\n}\nasync function load(){',aimStart);
 assert.ok(aimStart>=0&&aimStart<loadStart&&loadStart<frameStart,
  'the aim helper must live at script scope so the later top-level frame can call it');
 assert.ok(aimEnd>aimStart,'the shared aim helper must have a complete function body before load begins');
 assert.match(page.slice(aimStart,aimEnd),/SetFullSourceActorAimTarget/,
  'the shared helper must write the source actor-owned WeaponPointer target');
 const sourceFrame=page.slice(page.indexOf('if(sourceActorMode){',frameStart),page.indexOf('const fireAimReady=',frameStart));
 assert.ok(sourceFrame.indexOf('await aimQuadTarget()')>=0&&sourceFrame.indexOf('await aimQuadTarget()')<sourceFrame.indexOf('TickFullSourceActorBridge'),
  'FullSource frame must update its actor-owned aim target before the original actor tick');
});


test('FullSource projection follows original GameObject activation for authored hand layers',()=>{
 assert.match(originalHand,/forwardAssist\.SetActive\(false\)/);
 assert.match(originalHand,/fist\.SetActive\(true\)/);
 assert.match(originalHand,/trigger\.SetActive\(true\)/);
 assert.match(fullSourceBridge,/activeSelf\s*=\s*transform\.gameObject\.activeSelf/,
  'source snapshots must transport each original transform GameObject activeSelf state');
 const projectStart=page.indexOf('projectFullSourceSnapshot=snapshot=>');
 const projectEnd=page.indexOf('let textured=0;',projectStart);
 const projection=page.slice(projectStart,projectEnd);
 assert.match(projection,/group\.visible=pose\.activeSelf;/,
  'the renderer must apply source GameObject activation before drawing descendants');
 assert.match(handRenderers,/node\.add\(mesh\)/,
  'recovered hand layer meshes must remain children of their source GameObjects');
});
