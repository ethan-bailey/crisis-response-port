import test from 'node:test';
import assert from 'node:assert/strict';
import { TapFireQueue } from '../game/tap-fire-queue.mjs';

test('one tap sends one InputHandler press followed by release', () => {
  const queue = new TapFireQueue();
  queue.tap();
  assert.deepEqual([queue.advance(),queue.advance(),queue.advance()],[true,false,false]);
  assert.equal(queue.total,1);
});

test('rapid separate taps never become one continuous held-fire state', () => {
  const queue = new TapFireQueue();
  queue.tap();queue.tap();queue.tap();
  assert.deepEqual(Array.from({length:7},()=>queue.advance()),
                   [true,false,true,false,true,false,false]);
});

test('a tap during the release frame is not lost', () => {
  const queue = new TapFireQueue();
  queue.tap();
  assert.equal(queue.advance(),true);
  queue.tap();
  assert.deepEqual([queue.advance(),queue.advance(),queue.advance()],[false,true,false]);
});

test('pagehide reset cancels queued shots and pending key release', () => {
  const queue = new TapFireQueue();
  queue.tap();queue.tap();
  assert.equal(queue.advance(),true);
  queue.reset();
  assert.deepEqual([queue.advance(),queue.advance()],[false,false]);
  queue.tap();
  assert.deepEqual([queue.advance(),queue.advance()],[true,false]);
});

test('queued taps are bounded and invalid queue sizes are rejected', () => {
  const queue = new TapFireQueue(2);
  queue.tap();queue.tap();queue.tap();
  assert.equal(queue.total,3);
  assert.equal(queue.pending,2);
  assert.deepEqual(Array.from({length:5},()=>queue.advance()),
                   [true,false,true,false,false]);
  assert.throws(()=>new TapFireQueue(0),RangeError);
});
