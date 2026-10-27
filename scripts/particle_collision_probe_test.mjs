import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const probe=fs.readFileSync(new URL('./unity_2017/ParticleCollisionEventProbe.cs',import.meta.url),'utf8');
const closure=fs.readFileSync(new URL('../docs/particle-lifecycle-dependency-closure-2026-10-26.md',import.meta.url),'utf8');

test('Unity fixture captures the exact collision-event payload needed by SurfacePainter',()=>{
 assert.match(probe,/RequiredUnityVersion = "2017\.2\.0f3"/);
 assert.match(probe,/Application\.unityVersion != RequiredUnityVersion/);
 assert.match(probe,/collision\.sendCollisionMessages = true/);
 assert.match(probe,/collision\.type = ParticleSystemCollisionType\.World/);
 assert.match(probe,/collision\.mode = ParticleSystemCollisionMode\.Collision3D/);
 assert.match(probe,/GetCollisionEvents\(wall, collisionEvents\)/);
 assert.match(probe,/intersection = Vector\(first\.intersection\)/);
 assert.match(probe,/velocity = Vector\(first\.velocity\)/);
 assert.match(probe,/Temp\/crisis-response-particle-collision-event-probe\.json/);
 assert.match(closure,/ParticleCollisionEventProbe\.cs/);
 assert.match(closure,/must not be implemented until the version-locked report exists/);
});
