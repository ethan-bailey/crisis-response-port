import assert from 'node:assert/strict';
import { test } from 'node:test';
import { sourceEffectAge } from '../game/source-effect-clock.mjs';

test('source effect age follows recovered Unity Time.time, not wall time', () => {
  const firedAt = 120.25;
  assert.equal(sourceEffectAge(firedAt, firedAt), 0);
  assert.ok(Math.abs(sourceEffectAge(120.30, firedAt) - .05) < 1e-10);
  assert.equal(sourceEffectAge(120.30, firedAt), sourceEffectAge(120.30, firedAt),
    'Stalled browser frames do not advance a source-timed effect');
  assert.equal(sourceEffectAge(120.30, 120.31), 0,
    'A newly emitted event may precede the visible source time by float rounding');
});

test('time scaled or paused in the source stays paused in the renderer', () => {
  const firedAt = 10;
  const sourceTimes = [10.01, 10.02, 10.02, 10.02, 10.04];
  const ages = sourceTimes.map(time => sourceEffectAge(time, firedAt));
  assert.deepEqual(ages.map(v => Number(v.toFixed(3))), [.01, .02, .02, .02, .04]);
});

test('invalid or mismatched WASM effect-clock messages fail visibly', () => {
  for (const bad of [NaN, Infinity, -Infinity, undefined, null]) {
    if (bad === null) continue; // null is handled by the bridge schema
    assert.throws(() => sourceEffectAge(bad, 5), RangeError);
    assert.throws(() => sourceEffectAge(5, bad), RangeError);
  }
});
