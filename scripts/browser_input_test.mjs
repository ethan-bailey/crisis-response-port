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
  aim.send('pointerdown', { pointerId: 7, pointerType:'mouse' });
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
    aim.send('pointerdown', { pointerId: 1, pointerType:'mouse' }); input.set('sprint', 'touch:2', true);
    doc.hidden = true;
    (event === 'visibilitychange' ? doc : win).send(event);
    assert.equal(input.pressed('fire'), false); assert.equal(input.pressed('sprint'), false);
    aim.send('pointerdown', { pointerId: 3, pointerType:'mouse' }); assert.equal(input.pressed('fire'), true);
    input.dispose(); assert.equal(input.pressed('fire'), false);
    aim.send('pointerdown', { pointerId: 4 }); assert.equal(input.pressed('fire'), false);
  });
}

test('right mouse button does not fire', () => {
  const input = new BrowserInput(), aim = new Surface(); input.bindAim(aim, () => {});
  aim.send('pointerdown', { pointerId: 1, pointerType: 'mouse', button: 2 });
  assert.equal(input.pressed('fire'), false);
});

test('first touch aims; near-repeat taps fire; changing target aims without shooting',()=>{
 const input=new BrowserInput(),surface=new Surface(),aims=[],fires=[];
 input.bindAim(surface,e=>aims.push([e.clientX,e.clientY]),e=>fires.push([e.clientX,e.clientY]));
 const tap=(id,x,y)=>{surface.send('pointerdown',{pointerId:id,clientX:x,clientY:y});surface.send('pointerup',{pointerId:id,clientX:x,clientY:y});};
 tap(1,100,100);
 assert.deepEqual(aims,[[100,100]],'First touch must establish aim, not fire');
 assert.equal(fires.length,0);
 tap(2,115,112);
 assert.equal(fires.length,1,'Finger jitter inside 24 CSS pixels fires');
 assert.deepEqual(aims,[[100,100]],'Firing leaves last aim unchanged');
 tap(3,200,120);
 assert.deepEqual(aims,[[100,100],[200,120]],'New tap retargets without a shot');
 assert.equal(fires.length,1);
 tap(4,200,120);
 assert.deepEqual(fires,[[115,112],[200,120]],'Second tap on new aim fires');
 assert.equal(input.pressed('fire'),false,'Tap does not hold fire');
});

test('drag establishes new aim; subsequent tap near drag endpoint fires',()=>{
 const input=new BrowserInput(),surface=new Surface(),aims=[],fires=[];
 input.bindAim(surface,e=>aims.push([e.clientX,e.clientY]),()=>fires.push('fire'));
 surface.send('pointerdown',{pointerId:1,clientX:100,clientY:100});
 surface.send('pointermove',{pointerId:1,clientX:140,clientY:100});
 surface.send('pointerup',{pointerId:1,clientX:145,clientY:95});
 assert.deepEqual(aims,[[140,100],[145,95]]);
 assert.deepEqual(fires,[]);
 surface.send('pointerdown',{pointerId:2,clientX:151,clientY:100});
 surface.send('pointerup',{pointerId:2,clientX:151,clientY:100});
 assert.deepEqual(fires,['fire']);
 surface.send('pointerdown',{pointerId:3,clientX:240,clientY:80});
 surface.send('pointercancel',{pointerId:3,clientX:240,clientY:80});
 assert.equal(fires.length,1,'Cancelled gestures must not fire');
 surface.send('pointerdown',{pointerId:4,clientX:240,clientY:80});
 surface.send('pointerup',{pointerId:4,clientX:240,clientY:80});
 assert.deepEqual(aims.at(-1),[240,80],'Cancelled gesture does not change last aim');
 assert.equal(fires.length,1);
});

test('resize/background reset tap target; mouse press-to-fire behavior is retained',()=>{
 const input=new BrowserInput(),aim=new Surface(),win=new Surface(),doc=new Surface(),fires=[],aims=[];
 input.bindAim(aim,e=>aims.push([e.clientX,e.clientY]),()=>fires.push('fire'));
 input.bindLifecycle(win,doc);
 const tap=(id,x,y)=>{
  aim.send('pointerdown',{pointerId:id,clientX:x,clientY:y});
  aim.send('pointerup',{pointerId:id,clientX:x,clientY:y});
 };
 tap(1,100,100);tap(2,100,100);assert.equal(fires.length,1);
 win.send('resize');tap(3,100,100);
 assert.equal(fires.length,1,'Resize must reset same-spot comparison');
 assert.deepEqual(aims.at(-1),[100,100]);
 aim.send('pointerdown',{pointerId:4,pointerType:'mouse',clientX:200,clientY:100});
 assert.equal(input.pressed('fire'),true,'Mouse click still fires immediately');
 aim.send('pointerup',{pointerId:4,pointerType:'mouse',clientX:200,clientY:100});
 assert.equal(input.pressed('fire'),false);
 assert.equal(fires.length,1,'Mouse never enters touch tap-to-fire callback');
});


function analogStickFixture() {
  const input = new BrowserInput(), stick = new Surface(), keyboard = new Surface();
  const thumb = { style: { transform: '' }, offsetWidth: 44 };
  const classes = new Set();
  stick.classList = { add: x => classes.add(x), remove: x => classes.delete(x) };
  stick.getBoundingClientRect = () => ({ left: 40, width: 200 });
  input.bindMovementStick(stick, thumb);
  input.bindKeyboard(keyboard, { KeyA: 'left', KeyD: 'right' });
  const move = (type, x, pointerId = 9) => stick.send(type, { pointerId, clientX: x });
  return { input, stick, keyboard, thumb, classes, move };
}

test('analog joystick scales from dead zone to full speed in both directions', () => {
  const { input, stick, thumb, move } = analogStickFixture();
  move('pointerdown', 140); // center of 200px surface
  assert.equal(input.movementMagnitude(1), 0);
  move('pointermove', 158); // 18px: inside 22% dead zone
  assert.equal(input.pressed('left'), false);
  assert.equal(input.pressed('right'), false);
  move('pointermove', 190); // 50% right
  assert.equal(input.pressed('right'), true);
  assert.equal(input.pressed('left'), false);
  assert.ok(Math.abs(input.movementMagnitude(1) - (0.5 - 0.22) / 0.78) < 1e-10);
  assert.equal(input.movementMagnitude(-1), 0);
  assert.match(thumb.style.transform, /translateX\(/);
  move('pointermove', 240); // full right
  assert.equal(input.movementMagnitude(1), 1);
  move('pointermove', 90); // half left, reverse without a new gesture
  assert.equal(input.pressed('right'), false);
  assert.equal(input.pressed('left'), true);
  assert.ok(Math.abs(input.movementMagnitude(-1) - (0.5 - 0.22) / 0.78) < 1e-10);
  move('pointermove', 40); // full left
  assert.equal(input.movementMagnitude(-1), 1);
  move('pointerup', 40);
  assert.equal(input.pressed('left'), false);
  assert.equal(input.movementMagnitude(-1), 0);
  assert.equal(thumb.style.transform, '');
  assert.equal(stick.hasPointerCapture(9), false);
  input.dispose();
});

test('keyboard retains full original speed while touch movement is analog', () => {
  const { input, keyboard, move } = analogStickFixture();
  move('pointerdown', 190); // partial right
  assert.ok(input.movementMagnitude(1) < 1);
  keyboard.send('keydown', { code: 'KeyD' });
  assert.equal(input.movementMagnitude(1), 1, 'digital right input must override half-stick magnitude');
  keyboard.send('keyup', { code: 'KeyD' });
  assert.ok(input.movementMagnitude(1) < 1, 'return to analog magnitude');
  keyboard.send('keydown', { code: 'KeyA' });
  assert.equal(input.movementMagnitude(-1), 1, 'opposite digital direction still works');
  move('pointerup', 190);
  assert.equal(input.pressed('left'), true, 'touch release never clears keyboard');
  keyboard.send('keyup', { code: 'KeyA' });
  assert.equal(input.movementMagnitude(-1), 0);
  input.dispose();
});

test('analog movement cannot stick after second fingers, cancel, blur or disposal', () => {
  const { input, stick, thumb, classes, move } = analogStickFixture();
  const win = new Surface(), doc = new Surface();
  input.bindLifecycle(win, doc);
  move('pointerdown', 240, 9);
  move('pointerdown', 40, 10); // unrelated finger must not steal joystick
  assert.equal(input.movementMagnitude(1), 1);
  move('pointerup', 40, 10); // releasing unrelated finger does nothing
  assert.equal(input.movementMagnitude(1), 1);
  move('pointercancel', 240, 9);
  assert.equal(input.movementMagnitude(1), 0);
  assert.equal(input.pressed('right'), false);
  move('pointerdown', 40, 11);
  assert.equal(input.movementMagnitude(-1), 1);
  win.send('blur');
  assert.equal(input.movementMagnitude(-1), 0);
  assert.equal(input.pressed('left'), false);
  assert.equal(stick.hasPointerCapture(11), false);
  assert.equal(thumb.style.transform, '');
  assert.equal(classes.has('active'), false);
  move('pointerdown', 240, 12);
  input.dispose();
  assert.equal(input.movementMagnitude(1), 0);
  move('pointermove', 40, 12);
  assert.equal(input.pressed('left'), false);
});
