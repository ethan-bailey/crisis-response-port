import test from 'node:test';
import assert from 'node:assert/strict';
import { AimFireGate } from '../game/aim-fire-gate.mjs';

test('slow aim corrections keep sustained fire enabled', () => {
  const gate = new AimFireGate();
  assert.equal(gate.update(0, 0, 0), true);
  assert.equal(gate.update(0.02, 0, 100), true);
  assert.equal(gate.update(0.04, 0, 200), true);
});

test('rapid retarget blocks fire until aim settles, then unlocks', () => {
  const gate = new AimFireGate();
  gate.update(0, 0, 0);
  assert.equal(gate.update(0, 1, 50, false), false, 'A fast sky-to-floor sweep blocks fire');
  assert.equal(gate.update(0, 1, 100, false), false);
  assert.equal(gate.update(0, 1, 200, false), false, 'Fire stays blocked while source aim has not converged');
  assert.equal(gate.update(0, 1, 350, false), false, 'Stable pointer alone cannot release the source aim gate');
  assert.equal(gate.update(0, 1, 400, true), true, 'Fire resumes once stable and original weapon aim has converged');
});

test('fast reposition across a long frame gap is still treated as rapid', () => {
  const gate = new AimFireGate();
  gate.update(0, 0, 0);
  assert.equal(gate.update(1.5, 0, 1000), false, 'Long frame gaps cannot disguise a sweep');
});

test('slow adjustment after a rapid sweep does not unlock until nearly settled', () => {
  const gate = new AimFireGate();
  gate.update(0, 0, 0);
  gate.update(0, 1, 50);
  assert.equal(gate.update(0, 1.02, 100), false);
  assert.equal(gate.update(0, 1.04, 200), false);
  assert.equal(gate.update(0, 1.04, 350, true), true);
});

test('reset allows a fresh first aim sample', () => {
  const gate = new AimFireGate();
  gate.update(0, 0, 0);
  assert.equal(gate.update(1, 0, 20), false);
  gate.reset();
  assert.equal(gate.update(-1, 0, 30), true);
});
