import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const bridge=fs.readFileSync(new URL('../runtime/Engine/FullSourceActorBridge.cs',import.meta.url),'utf8');
const page=fs.readFileSync(new URL('../game/index.html',import.meta.url),'utf8');
const smoke=fs.readFileSync(new URL('./mission_grounding_browser_smoke.mjs',import.meta.url),'utf8');

test('FullSource locomotion failures retain the original emitted force vectors end to end',()=>{
  assert.match(bridge,/physicsCommands\.Add\(new\s*\{\s*force\s*=\s*new\[\]\s*\{\s*force\.x,\s*force\.y,\s*force\.z\s*\}/s,
    'the source bridge must retain the exact vector produced by CharacterBody.FixedUpdate');
  assert.match(bridge,/commands\s*=\s*physicsCommands\.ToArray\(\)/,
    'the actor snapshot must transport source force commands to the browser');
  assert.match(page,/sourcePhysicsCommands:sourceActorSnapshot\?\.physics\?\.commands\?\?\[\]/,
    'the playable source state must expose actual returned commands without recomputation');
  assert.match(page,/sourcePhysicsCommands:sourceActorSnapshot\.physics\?\.commands\?\?\[\]/,
    'the bounded per-frame source animation trace must retain the command vectors applied to contact physics');
  assert.match(smoke,/sourceMovement: current\.sourceMovement, sourcePhysicsCommands: current\.sourcePhysicsCommands/,
    'a movement timeout must retain source movement gates and the exact command values together');
  assert.match(smoke,/sourcePhysicsCommands: state\.sourcePhysicsCommands,\s*sourceAnimationTrace: state\.sourceAnimationTrace/,
    'an authored-animation timeout must retain the direct command snapshot and bounded frame trace');
});
