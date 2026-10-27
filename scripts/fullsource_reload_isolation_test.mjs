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
 assert.match(smoke,/reloadAudio: \{ before: reloadBefore, after: reloadAfter/,
  'reload-only report must retain source arm and audio evidence');
 assert.match(workflow,/mission_grounding_browser_smoke\.mjs[^\n]*cpr-reload-browser[^\n]*--reload-only/,
  'hosted package must run an independent reload-only browser invocation');
 assert.match(workflow,/grounding_status[\s\S]*reload_status[\s\S]*exit 1/,
  'workflow must run both invocations before failing either acceptance');
});
