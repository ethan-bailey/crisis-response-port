import test from 'node:test';import assert from 'node:assert/strict';import {AimDirection} from '../game/aim-direction.mjs';
test('released aim follows character translation without turning',()=>{const aim=new AimDirection();aim.set([10,3,1.5],[0,1,1.5]);assert.deepEqual(aim.target([100,4,1.5]),[110,6,1.5]);assert.deepEqual(aim.target([-100,4,1.5]),[-90,6,1.5]);assert.deepEqual(aim.vector,[10,2,0]);});
test('zero-length drag preserves direction',()=>{const aim=new AimDirection();aim.set([0,0,0],[0,0,0]);assert.deepEqual(aim.vector,[2,0,0]);});
