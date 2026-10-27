import assert from 'node:assert/strict';
import {test} from 'node:test';
import fs from 'node:fs';
import {sourceAnimatorParameters} from '../game/source-character-step.mjs';
import {SourceCharacterAnimator} from '../game/source-character-animator.mjs';

const controller=JSON.parse(fs.readFileSync(new URL('../game/assets/operator/character-animator-controller.json',import.meta.url),'utf8'));
const bridge=fs.readFileSync(new URL('../runtime/Engine/FullSourceActorBridge.cs',import.meta.url),'utf8');
const page=fs.readFileSync(new URL('../game/index.html',import.meta.url),'utf8');
const smoke=fs.readFileSync(new URL('./mission_grounding_browser_smoke.mjs',import.meta.url),'utf8');

test('FullSource Animator parameters preserve original Update values across the post-contact render step',()=>{
 const source={moving:true,backwards:true,moveSpeed:0,kneelRatio:.25,crippledRatio:0};
 const inputs=sourceAnimatorParameters({animationParameters:source});
 assert.deepEqual(inputs,{Moving:true,Backwards:true,MoveSpeed:0,KneelRatio:.25,CrippledRatio:0});
 const animator=new SourceCharacterAnimator(controller);
 const result=animator.step(.02,inputs);
 assert.equal(result.transition?.to,'Backward locomotion');
});

test('FullSource animation ignores stale post-contact velocity when original Animator reports stationary',()=>{
 const inputs=sourceAnimatorParameters({animationParameters:{moving:false,backwards:false,moveSpeed:0,kneelRatio:0,crippledRatio:0}});
 const animator=new SourceCharacterAnimator(controller);
 const result=animator.step(.02,inputs);
 assert.equal(result.state,'Stationary');
 assert.deepEqual(result.clips.map(clip=>clip.clip),['Body stand']);
});

test('bridge exports the source Animator parameters and FullSource sampler uses them before the legacy fallback',()=>{
 assert.match(bridge,/sourceAnimator\.GetBool\("Moving"\)/);
 assert.match(bridge,/sourceAnimator\.GetBool\("Backwards"\)/);
 assert.match(bridge,/sourceAnimator\.GetFloat\("MoveSpeed"\)/);
 assert.match(bridge,/sourceAnimator\.GetFloat\("KneelRatio"\)/);
 assert.match(bridge,/sourceAnimator\.GetFloat\("CrippledRatio"\)/);
 const start=page.indexOf('applyAnimation=async function(dt,preserveSourcePose=false)');
 const end=page.indexOf('rightUpperArm=nodeByPath',start);
 const sampler=page.slice(start,end);
 assert.ok(start>=0&&end>start);
 assert.match(sampler,/sourceAnimatorParameters\(sourceActorSnapshot\)/);
 assert.ok(sampler.indexOf('sourceAnimatorParameters(sourceActorSnapshot)')<sampler.indexOf('sourceAnimator.step(dt,animationInputs)'));
});

test('packaged Mission acceptance requires an authored locomotion clip while source Animator Moving is true',()=>{
 assert.match(smoke,/animationDiagnostics\?\.animationInputs\?\.Moving\s*===\s*true/,
  'cumulative contact-position drift must not satisfy the authored locomotion acceptance');
 assert.match(smoke,/\['Body walk', 'Body walk backward', 'Body sprint'\]\.includes\(current\.animation\)/,
  'the acceptance must observe one of the recovered source clips');
});
