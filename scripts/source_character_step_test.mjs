import assert from 'node:assert/strict';
import { test } from 'node:test';
import {
  advanceOriginalKneelRatio,
  stepBodyMovement,
  stepCharacterAnimation,
} from '../game/source-character-step.mjs';

test('source kneel ratio uses CharacterBody.UpdateAnimation smoothing', () => {
  assert.equal(advanceOriginalKneelRatio(0, true, 0.1), 0.5);
  assert.equal(advanceOriginalKneelRatio(0.99, true, 0.1), 1.0025);
  assert.equal(advanceOriginalKneelRatio(0.05, false, 0.1), 0.025);
});

test('source movement respects walk cap and original acceleration formula', () => {
  assert.deepEqual(stepBodyMovement({
    direction: 1, velocity: 0.2, step: 0.02, facing: 1,
    walkSpeed: 1.2, sprintSpeed: 3.2, analogMagnitude: 0.5,
  }), { forceX: 7.5, reset: true, speedLimit: 1.2 });
  assert.deepEqual(stepBodyMovement({
    direction: 1, velocity: 1.21, step: 0.02, facing: 1,
    walkSpeed: 1.2, sprintSpeed: 3.2,
  }), { forceX: 0, reset: false, speedLimit: 1.2 });
});

test('source movement prevents sprint while reversing or limping', () => {
  const base = {
    direction: -1, velocity: 0, step: 0.02, facing: 1,
    walkSpeed: 1.2, sprintSpeed: 3.2, sprint: true,
  };
  assert.equal(stepBodyMovement(base).speedLimit, 1.2);
  assert.equal(stepBodyMovement({ ...base, direction: 1, facing: 1, crippledRatio: 0.5,
    operator: false, noPlayerLimp: false }).speedLimit, 0.96);
});

test('source animator inputs match CharacterBody movement and speed thresholds', () => {
  const result = stepCharacterAnimation({
    kneelRatio: 0.4, movingDelta: -0.02, deltaTime: 0.1, velocity: -2.2,
    direction: 1, walkSpeed: 1.2, sprintSpeed: 3.2,
  });
  assert.deepEqual(result, {
    moving: true, backwards: true, moveSpeed: result.moveSpeed,
    KneelRatio: 0.4, crippled: 0, ragdoll: false,
  });
  assert.ok(Math.abs(result.moveSpeed - 0.25) < 1e-12);
});
