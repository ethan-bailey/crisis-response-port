import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import { compareSourceArmPoses, extractSourceArmPoses } from '../game/source-arm-pose.mjs';

const page = fs.readFileSync(new URL('../game/index.html', import.meta.url), 'utf8');
const program = fs.readFileSync(new URL('../runtime/Browser/Program.cs', import.meta.url), 'utf8');
const entry = fs.readFileSync(new URL('../runtime/Browser/BrowserEntryTeamMission.cs', import.meta.url), 'utf8');
const smoke = fs.readFileSync(new URL('./mission_grounding_browser_smoke.mjs', import.meta.url), 'utf8');

test('source arm sampler retains only recovered Operator arm and hand transforms', () => {
  const snapshot = { poses: [
    { path: 'Operator 1/Pelvis/Right upper arm', p: [0, 0, 0], q: [0, 0, 0, 1] },
    { path: 'Operator 1/Pelvis/Right upper arm/Right lower arm', p: [1, 0, 0], q: [0, 0, 0, 1] },
    { path: 'Operator 1/Pelvis/Right upper arm/Right lower arm/Right hand', p: [2, 0, 0], q: [0, 0, 0, 1] },
    { path: 'Operator 1/Pelvis', p: [0, 0, 0], q: [0, 0, 0, 1] },
  ] };
  assert.deepEqual(extractSourceArmPoses(snapshot).map(pose => pose.path), snapshot.poses.slice(0, 3).map(pose => pose.path));
  assert.deepEqual(extractSourceArmPoses(null), []);
});

test('quaternion comparison treats q and -q as the same rotation and detects source movement', () => {
  const before = [{ path: 'Operator/Left hand', position: [0, 0, 0], rotation: [0, 0, 0, 1] }];
  const equivalent = [{ path: 'Operator/Left hand', position: [0, 0, 0], rotation: [0, 0, 0, -1] }];
  const moved = [{ path: 'Operator/Left hand', position: [0.25, 0, 0], rotation: [0, 0, 0, 1] }];
  assert.equal(compareSourceArmPoses(before, equivalent).maximum, 0);
  assert.equal(compareSourceArmPoses(before, moved).maximum, 0.25);
});

test('packaged Mission reload HUD and arm observation follow the original player task lifecycle', () => {
  assert.match(program, /BrowserEntryTeamMission\.Start\(source,world,enablePlayerBehavior:true\)/,
    'production EntryTeam initialization must enable the original player task tree');
  assert.match(entry, /if \(!enablePlayerBehavior \|\| body == null \|\| !body\.IsPlayer\) tree\.enabled = false/,
    'nonplayer behavior trees must remain suppressed in the browser bridge');
  const frameStart = page.indexOf('if(sourceActorMode){');
  const frameEnd = page.indexOf('const fireAimReady=', frameStart);
  const frame = page.slice(frameStart, frameEnd);
  assert.ok(frame.indexOf('sourceActorSnapshot=JSON.parse') >= 0 &&
    frame.indexOf('weaponState=sourceActorSnapshot.weaponState') > frame.indexOf('sourceActorSnapshot=JSON.parse'),
    'the HUD state must come from the post-tick original actor snapshot');
  assert.ok(frame.indexOf('magazineHud.render(weaponState)') > frame.indexOf('weaponState=sourceActorSnapshot.weaponState'),
    'FullSource tick must refresh magazine gesture readiness from source ammo each frame');
  assert.match(page, /sourceArmPoses:extractSourceArmPoses\(sourceActorSnapshot\)/,
    'diagnostics must preserve original source poses separately from the visual arm rig');
  assert.match(smoke, /compareSourceArmPoses\(reloadBefore\.sourceArmPoses, sample\)/,
    'browser reload acceptance must compare the live authored arm/hand poses to an idle source baseline');
  assert.match(smoke, /reloadPoseChange\.maximum > idleArmDrift\.maximum/,
    'pose movement must exceed measured idle drift rather than an invented fixed threshold');
  assert.match(smoke, /settled: compareSourceArmPoses\(reloadBefore\.sourceArmPoses, reloadAfter\.sourceArmPoses\)/,
    'browser evidence must retain post-reload source pose alongside idle and active samples');
});
