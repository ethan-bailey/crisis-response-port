import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const smoke=fs.readFileSync(new URL('./mission_grounding_browser_smoke.mjs',import.meta.url),'utf8');
const workflow=fs.readFileSync(new URL('../.github/workflows/source-contracts.yml',import.meta.url),'utf8');

test('packaged reload acceptance runs independently after locomotion failure',()=>{
 assert.match(smoke,/const reloadOnly=process\.argv\.includes\('--reload-only'\)/);
 assert.match(smoke,/if\(!reloadOnly\)\{[\s\S]*movementStick[\s\S]*KeyA[\s\S]*movementReport=/,
  'only joystick/aim/locomotion interactions may be skipped by reload-only mode');
 assert.match(smoke,/const reloadBefore = await page\.evaluate/,
  'source reload lifecycle must remain unconditional');
 assert.match(smoke,/const ammunitionBefore = \{ magazine: reloadBefore\.weapon\.magazine, reserve: reloadBefore\.weapon\.reserve \}/,
  'packaged reload acceptance must capture the source magazine and reserve counts before firing');
 assert.match(smoke,/const fireDeadline = Date\.now\(\) \+ 20000;[\s\S]*while \(Date\.now\(\) < fireDeadline\)[\s\S]*fireButton\.click\(\{ delay: 80 \}\)/,
  'the production fire control must receive repeated real browser clicks until two rounds are spent');
 assert.match(smoke,/ammunitionSpent\.magazine <= ammunitionBefore\.magazine - 2/,
  'reload-only acceptance must assert a two-round magazine decrease before reload');
 assert.match(smoke,/assert\.ok\(reloadAfter\.weapon\.magazine > ammunitionSpent\.magazine/,
  'reload acceptance must prove magazine rounds increase after firing');
 assert.match(smoke,/assert\.ok\(reloadAfter\.weapon\.reserve < ammunitionSpent\.reserve/,
  'reload acceptance must prove reserve rounds decrease after firing');
 assert.match(smoke,/reloadAudio: \{ before: reloadBefore, ammunitionBefore: ammunitionBefore, afterSpending: ammunitionSpent, afterReload:/,
  'reload-only report must retain before/spent/reloaded counts with source arm and audio evidence');
 assert.match(workflow,/mission_grounding_browser_smoke\.mjs[^\n]*cpr-reload-browser[^\n]*--reload-only/,
  'hosted package must run an independent reload-only browser invocation');
 assert.match(workflow,/grounding_status[\s\S]*reload_status[\s\S]*exit 1/,
  'workflow must run both invocations before failing either acceptance');
});
