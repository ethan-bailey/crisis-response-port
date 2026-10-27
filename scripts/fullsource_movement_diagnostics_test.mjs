import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const bridge=fs.readFileSync(new URL('../runtime/Engine/FullSourceActorBridge.cs',import.meta.url),'utf8');
const page=fs.readFileSync(new URL('../game/index.html',import.meta.url),'utf8');
const smoke=fs.readFileSync(new URL('./mission_grounding_browser_smoke.mjs',import.meta.url),'utf8');

test('FullSource movement failure snapshots expose original controller gates and physics inputs',()=>{
 const start=bridge.indexOf('object Snapshot(object[] audioEvents, object[] particleEvents)');
 assert.ok(start>=0,'FullSource actor bridge must serialize its original actor state');
 const snapshot=bridge.slice(start);
 for(const field of ['missionView','missionPaused','playerControllerActive','bodyRagdoll','accelerationMagnitude','sourceVelocity','walkSpeed','sprintSpeed','fixedSteps','emittedForceCommands'])
  assert.match(snapshot,new RegExp(`\\b${field}\\s*=`),`movement diagnostics must include ${field}`);
 assert.match(page,/sourceMovement:sourceActorSnapshot\?\.movementState\?\?null/,
  'the actual original movement gate state must reach the browser snapshot');
 assert.match(smoke,/current\.sourceMovement/,'the failing browser report must retain the original movement gate state');
 assert.match(smoke,/movementTrace/,'the browser smoke must retain a bounded trace when locomotion times out');
 assert.match(smoke,/sample\.playerX < walkStart - 0\.1/,'the existing acceptance must still require new leftward actor displacement');
});
