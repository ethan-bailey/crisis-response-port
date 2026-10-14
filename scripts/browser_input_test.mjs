import test from 'node:test';
import assert from 'node:assert/strict';
import { BrowserInput } from '../game/browser-input.mjs';

class Surface extends EventTarget {
  captures = new Set();
  setPointerCapture(id) { this.captures.add(id); }
  hasPointerCapture(id) { return this.captures.has(id); }
  releasePointerCapture(id) { this.captures.delete(id); }
  send(type, fields = {}) {
    const e = new Event(type, { cancelable: true });
    Object.assign(e, { button: 0, pointerType: 'touch', ...fields });
    this.dispatchEvent(e);
  }
}

test('two fingers on one action release independently', () => {
  const presses = [], input = new BrowserInput({ onPress: a => presses.push(a) });
  const button = new Surface(); input.bindButton(button, 'left');
  button.send('pointerdown', { pointerId: 1 });
  button.send('pointerdown', { pointerId: 2 });
  assert.equal(button.hasPointerCapture(1), true);
  button.send('pointerup', { pointerId: 1 });
  assert.equal(input.pressed('left'), true);
  button.send('pointercancel', { pointerId: 2 });
  assert.equal(input.pressed('left'), false);
  assert.deepEqual(presses, ['left']);
});

test('touch release does not clear a keyboard action; repeats do not retrigger', () => {
  const presses = [], input = new BrowserInput({ onPress: a => presses.push(a) });
  const button = new Surface(), keyboard = new Surface();
  input.bindButton(button, 'left'); input.bindKeyboard(keyboard, { KeyA: 'left', ArrowLeft: 'left' });
  keyboard.send('keydown', { code: 'KeyA' });
  keyboard.send('keydown', { code: 'KeyA', repeat: true });
  keyboard.send('keydown', { code: 'ArrowLeft' });
  button.send('pointerdown', { pointerId: 1 });
  button.send('pointerup', { pointerId: 1 });
  keyboard.send('keyup', { code: 'KeyA' });
  assert.equal(input.pressed('left'), true);
  keyboard.send('keyup', { code: 'ArrowLeft' });
  assert.equal(input.pressed('left'), false);
  assert.deepEqual(presses, ['left']);
});

test('movement finger release does not stop aiming/firing', () => {
  const input = new BrowserInput(), aim = new Surface(), left = new Surface(), moves = [];
  input.bindAim(aim, e => moves.push(e.pointerId)); input.bindButton(left, 'left');
  aim.send('pointerdown', { pointerId: 7 });
  left.send('pointerdown', { pointerId: 8 });
  left.send('pointerup', { pointerId: 8 });
  aim.send('pointerup', { pointerId: 8 });
  aim.send('pointermove', { pointerId: 8 });
  assert.equal(input.pressed('fire'), true);
  assert.deepEqual(moves, [7]);
  aim.send('pointercancel', { pointerId: 7 });
  assert.equal(input.pressed('fire'), false);
});

test('losing capture releases only that control', () => {
  const input = new BrowserInput(), left = new Surface(), sprint = new Surface();
  input.bindButton(left, 'left'); input.bindButton(sprint, 'sprint');
  left.send('pointerdown', { pointerId: 1 }); sprint.send('pointerdown', { pointerId: 2 });
  left.send('lostpointercapture', { pointerId: 1 });
  assert.equal(input.pressed('left'), false); assert.equal(input.pressed('sprint'), true);
});

for (const event of ['blur', 'pagehide', 'resize', 'visibilitychange']) {
  test(`${event} releases actions and lets a new aim finger take over`, () => {
    const input = new BrowserInput(), aim = new Surface(), win = new Surface(), doc = new Surface();
    input.bindAim(aim, () => {}); input.bindLifecycle(win, doc);
    aim.send('pointerdown', { pointerId: 1 }); input.set('sprint', 'touch:2', true);
    doc.hidden = true;
    (event === 'visibilitychange' ? doc : win).send(event);
    assert.equal(input.pressed('fire'), false); assert.equal(input.pressed('sprint'), false);
    aim.send('pointerdown', { pointerId: 3 }); assert.equal(input.pressed('fire'), true);
    input.dispose(); assert.equal(input.pressed('fire'), false);
    aim.send('pointerdown', { pointerId: 4 }); assert.equal(input.pressed('fire'), false);
  });
}

test('right mouse button does not fire', () => {
  const input = new BrowserInput(), aim = new Surface(); input.bindAim(aim, () => {});
  aim.send('pointerdown', { pointerId: 1, pointerType: 'mouse', button: 2 });
  assert.equal(input.pressed('fire'), false);
});
