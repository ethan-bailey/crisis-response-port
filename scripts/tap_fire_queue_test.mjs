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

test('stationary hold continuously asserts original trigger until released', () => {
  const queue=new TapFireQueue();
  queue.hold('aim:1',true);
  assert.deepEqual([queue.advance(),queue.advance(),queue.advance()],[true,true,true]);
  queue.hold('aim:1',false);
  assert.deepEqual([queue.advance(),queue.advance()],[false,false]);
});

test('hold re-press between async original ticks never hides a release edge', () => {
  const queue=new TapFireQueue();
  queue.hold('aim:1',true);
  assert.equal(queue.advance(),true);
  queue.hold('aim:1',false);
  queue.hold('aim:2',true);
  assert.deepEqual([queue.advance(),queue.advance()],[false,true]);
  queue.hold('aim:2',false);
  assert.equal(queue.advance(),false);
});

test('multiple holders cannot accidentally end someone else trigger', () => {
  const queue=new TapFireQueue();
  queue.hold('aim:1',true);
  queue.hold('button:2',true);
  assert.equal(queue.advance(),true);
  queue.hold('aim:1',false);
  assert.equal(queue.advance(),true);
  queue.hold('button:2',false);
  assert.equal(queue.advance(),false);
});

test('queued taps expire after slow or blocked runtime input ticks', () => {
  const queue=new TapFireQueue(8,500);
  queue.tap(1000);
  queue.tap(1050);
  assert.equal(queue.advance(1700),false);
  assert.equal(queue.dropped,2);
  assert.equal(queue.pending,0);
});

test('rapid same-spot presses yield distinct original trigger edges', () => {
  const queue=new TapFireQueue(32,4000);
  for(let i=0;i<20;i++) queue.tap(1000+i);
  const edges=Array.from({length:40},()=>queue.advance(1100));
  assert.deepEqual(edges,Array.from({length:40},(_,i)=>i%2===0));
  assert.equal(queue.pending,0);
});

test('pending short taps do not fabricate shots during held fire', () => {
  const queue=new TapFireQueue();
  queue.tap(1000);
  queue.hold('aim:2',true);
  assert.equal(queue.advance(1010),true);
  assert.equal(queue.advance(1020),true);
  queue.hold('aim:2',false);
  assert.equal(queue.advance(1030),false);
  assert.equal(queue.advance(1040),true);
  assert.equal(queue.advance(1050),false);
});

test('reset cancels both queued taps and any held trigger', () => {
  const queue=new TapFireQueue();
  queue.hold('aim:1',true);
  queue.tap();
  assert.equal(queue.advance(),true);
  queue.reset();
  assert.equal(queue.held,false);
  assert.equal(queue.pending,0);
  assert.equal(queue.advance(),false);
});
