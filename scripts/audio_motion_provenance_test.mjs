import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const queue=fs.readFileSync(new URL('../runtime/Engine/BrowserAudioEventQueue.cs',import.meta.url),'utf8');
const audioRuntime=fs.readFileSync(new URL('../game/weapon-audio-runtime.mjs',import.meta.url),'utf8');
const projectile=fs.readFileSync(new URL('../runtime/Original/FullSource/Projectile.cs',import.meta.url),'utf8');

test('audio snapshots preserve exact runtime motion provenance without applying guessed Doppler',()=>{
 assert.match(queue,/static object DescribeMotion\(Transform transform\)/,
  'audio transform snapshots must identify their runtime motion owner');
 assert.match(queue,/GetComponentInParent<Rigidbody>\(\)/,
  'runtime-added casing and authored body motion must come from the owning Rigidbody when present');
 assert.match(queue,/GetComponentInParent<Crisis\.Runtime\.FullSource\.Projectile>\(\)/,
  'transform-driven projectile audio must resolve the original Projectile owner explicitly');
 assert.match(queue,/projectile\.transform\.forward \* projectile\.Velocity \+ projectile\.GravityVelocityAtTime\(Time\.time\)/,
  'projectile motion must use the recovered flight equation at the same source time');
 assert.match(queue,/motion=DescribeMotion\(transform\)/,
  'callback-time and post-tick transform snapshots must carry the provenance record');
 assert.match(projectile,/NextPosition = base\.transform\.position \+ base\.transform\.forward \* num4 \+ GravityVelocityAtTime\(Time\.time \+ num3\) \* num3;/,
  'the transported projectile vector must remain tied to the recovered source integration formula');
 assert.match(audioRuntime,/const motion=state\?\.sourceTransform\?\.motion/,
  'the browser boundary must validate optional source motion before accepting a state frame');
 assert.doesNotMatch(audioRuntime,/createPanner\(|dopplerFactor|speedOfSound/,
  'provenance transport must not invent WebAudio spatial or Doppler output');
});
