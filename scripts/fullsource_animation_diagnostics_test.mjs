import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const page=fs.readFileSync(new URL('../game/index.html',import.meta.url),'utf8');
const smoke=fs.readFileSync(new URL('./mission_grounding_browser_smoke.mjs',import.meta.url),'utf8');
const adapter=fs.readFileSync(new URL('../game/source-character-step.mjs',import.meta.url),'utf8');

test('FullSource animation evidence records actual adapter inputs, source/contact motion and selected authored clips',()=>{
 const applyStart=page.indexOf('applyAnimation=async function(dt,preserveSourcePose=false)');
 const applyEnd=page.indexOf('rightUpperArm=nodeByPath',applyStart);
 assert.ok(applyStart>=0&&applyEnd>applyStart,'source animation adapter must be present');
 const apply=page.slice(applyStart,applyEnd);
 assert.match(apply,/const animationInputs\s*=/,'diagnostics must capture the actual parameter object passed to the AnimatorController');
 for(const field of ['sourceAnimatorState','sourceAnimatorTransition','selectedClips','sourcePosition','contactPosition','sourceVelocity','contactVelocity','sourceMovement'])
  assert.match(apply,new RegExp(`\\b${field}\\s*:`),`animation diagnostics must record ${field}`);
 assert.match(apply,/const timeline=sourceAnimator\.step\(dt,animationInputs\)/,
  'the diagnostic inputs must be the same object consumed by the recovered AnimatorController');
 const frameStart=page.indexOf('async function frame(');
 const branchStart=page.indexOf('if(sourceActorMode){',frameStart);
 const legacyStart=page.indexOf('const fireAimReady=',branchStart);
 const sourceFrame=page.slice(branchStart,legacyStart);
 assert.ok(sourceFrame.indexOf('projectFullSourceSnapshot(sourceActorSnapshot);await applyAnimation(dt,true)')<sourceFrame.indexOf('sourceAnimationTrace.push'),
  'trace samples must be recorded after original source state synchronization and clip selection');
 assert.match(sourceFrame,/if\(sourceAnimationTrace\.length>96\)sourceAnimationTrace\.splice\(0,sourceAnimationTrace\.length-96\)/,
  'runtime diagnostics must remain bounded');
 assert.match(adapter,/const moving = Math\.abs\(velocity\) > 0\.15/,
  'the traced adapter retains its current movement threshold until evidence establishes a source correction');
});

test('packaged walk-animation timeout retains the actual bounded runtime trace',()=>{
 assert.match(page,/animationDiagnostics,sourceAnimationTrace:sourceAnimationTrace\.slice\(-96\)/,
  'browser snapshot must expose the bounded animation diagnostic trace');
 assert.match(smoke,/currentAnimationDiagnostics: state\.animationDiagnostics/,
  'the timeout report must capture the exact adapter inputs and selected clip');
 assert.match(smoke,/sourceAnimationTrace: state\.sourceAnimationTrace/,
  'the timeout report must retain the frame sequence after player displacement');
 assert.match(smoke,/Body walk backward/,
  'the failure wrapper must keep the original backward-locomotion acceptance');
});
