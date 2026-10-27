import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const bridge=fs.readFileSync(new URL('../runtime/Engine/FullSourceActorBridge.cs',import.meta.url),'utf8');
const page=fs.readFileSync(new URL('../game/index.html',import.meta.url),'utf8');
const smoke=fs.readFileSync(new URL('./mission_grounding_browser_smoke.mjs',import.meta.url),'utf8');

test('FullSource Mission snapshots publish live original InputHandler action state to browser acceptance',()=>{
 const start=bridge.indexOf('object Snapshot(object[] audioEvents, object[] particleEvents)');
 const end=bridge.length;
 const snapshot=bridge.slice(start,end);
 assert.ok(start>=0&&end>start,'FullSource actor snapshot must have a bounded serializer');
 assert.match(snapshot,/originalInput\s*=\s*actions\.ToDictionary\(pair\s*=>\s*pair\.Key/,'snapshot must serialize original InputMapping actions');
 assert.match(snapshot,/Pressed\s*=\s*item\.Pressed/,'snapshot must report the held key consumed by PlayerController.Update');
 assert.match(snapshot,/Down\s*=\s*item\.Down/,'snapshot must preserve the original input edge state');
 const frameStart=page.indexOf('if(sourceActorMode){');
 const frameEnd=page.indexOf('const fireAimReady=',frameStart);
 const frame=page.slice(frameStart,frameEnd);
 const tick=frame.indexOf('TickFullSourceActorBridge');
 const input=frame.indexOf('originalInputState=sourceActorSnapshot.originalInput');
 assert.ok(tick>=0&&input>tick,'FullSource frame must refresh diagnostic input state from the original actor snapshot after source update');
 assert.match(smoke,/sample\?\.originalLeftPressed && sample\.playerX < walkStart - 0\.1/,'grounding smoke must observe original source input and resulting leftward movement together');
 assert.match(smoke,/movementTrace/,'grounding smoke must retain sampled controller and physics state when locomotion times out');
});
