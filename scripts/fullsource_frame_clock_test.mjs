import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const page=fs.readFileSync(new URL('../game/index.html',import.meta.url),'utf8');

test('browser runtime, contact physics, and animation share one capped frame interval',()=>{
 const frameStart=page.indexOf('async function frame(now)');
 const frameEnd=page.lastIndexOf('requestAnimationFrame(frame);');
 assert.ok(frameStart>=0&&frameEnd>frameStart,'production frame loop must be present');
 const frame=page.slice(frameStart,frameEnd);
 assert.match(frame,/const dt=Math\.min\(elapsed,\.05\)/,
  'production frame must retain the bounded 50ms browser simulation interval');
 assert.match(frame,/TickFullSourceActorBridge",dt,JSON\.stringify\(rawActions\)/,
  'FullSource RuntimeWorld must receive the same capped interval as contact integration and animation');
 assert.match(frame,/runtimeCall\("Tick",dt,JSON\.stringify\(rawActions\)\)/,
  'legacy RuntimeWorld must use the same capped interval instead of an unbounded wall-clock gap');
 assert.doesNotMatch(frame,/runtimeCall\("(?:TickFullSourceActorBridge|Tick)",elapsed,/,
  'no production runtime tick may receive the uncapped wall-clock interval');
});
