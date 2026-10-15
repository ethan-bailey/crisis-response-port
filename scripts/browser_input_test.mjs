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

test('movement finger release does not interrupt drag or trigger fire', () => {
  const input = new BrowserInput(), aim = new Surface(), left = new Surface(), moves = [], taps = [];
  input.bindAim(aim, e => { if (e) moves.push(e.pointerId); }, { onTap: e => taps.push(e.pointerId) });
  input.bindButton(left, 'left');
  aim.send('pointerdown', { pointerId: 7, clientX: 10 });
  left.send('pointerdown', { pointerId: 8 });
  left.send('pointerup', { pointerId: 8 });
  aim.send('pointermove', { pointerId: 8, clientX: 400 }); // Not the aim owner
  aim.send('pointermove', { pointerId: 7, clientX: 25 });
  assert.deepEqual(moves, [7]);
  assert.equal(input.pressed('fire'), false, 'Dragging cannot hold original fire action');
  aim.send('pointercancel', { pointerId: 7 });
  assert.deepEqual(taps, [], 'Cancellation cannot fire');
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
    aim.send('pointerdown', { pointerId: 3, clientX: 10 });
    aim.send('pointermove', { pointerId: 3, clientX: 40 });
    input.dispose(); assert.equal(input.pressed('fire'), false);
    aim.send('pointerdown', { pointerId: 4 }); assert.equal(input.pressed('fire'), false);
  });
}

test('right mouse button does not fire', () => {
  const input = new BrowserInput(), aim = new Surface(); input.bindAim(aim, () => {});
  aim.send('pointerdown', { pointerId: 1, pointerType: 'mouse', button: 2 });
  assert.equal(input.pressed('fire'), false);
});

test('aim only follows the captured pointer and remains latched after release', () => {
  const callbacks=[],input=new BrowserInput(),surface=new Surface();
  input.bindAim(surface,event=>callbacks.push(event ? [event.pointerId,event.clientX] : 'released'));
  surface.send('pointermove',{pointerId:9,pointerType:'mouse',clientX:100}); // Hover must not steer the character.
  surface.send('pointerdown',{pointerId:1,clientX:25});
  surface.send('pointermove',{pointerId:2,clientX:400}); // A second finger cannot aim.
  surface.send('pointermove',{pointerId:1,clientX:50});
  surface.send('pointerup',{pointerId:1});
  surface.send('pointermove',{pointerId:1,pointerType:'mouse',clientX:700}); // No invisible pointer.
  assert.deepEqual(callbacks,[[1,50],'released']);
  assert.equal(input.pressed('fire'),false);
});

test('page suspension releases active aim and ignores subsequent hover', () => {
  const callbacks=[],input=new BrowserInput(),aim=new Surface(),win=new Surface(),doc=new Surface();
  input.bindAim(aim,event=>callbacks.push(event?'aim':'released'));
  input.bindLifecycle(win,doc);
  aim.send('pointerdown',{pointerId:11,clientX:10});
  aim.send('pointermove',{pointerId:11,clientX:40});
  win.send('pagehide');
  aim.send('pointermove',{pointerId:11,pointerType:'mouse'});
  assert.deepEqual(callbacks,['aim','released']);
  assert.equal(input.pressed('fire'),false);
});

test('tap requests fire only on pointerup, not on aim drag or pointerdown', () => {
  const input = new BrowserInput(), surface = new Surface(), aim = [], taps = [];
  input.bindAim(surface, e => aim.push(e ? 'aim' : 'released'), { onTap: e => taps.push(e.pointerId) });
  surface.send('pointerdown', { pointerId: 1, clientX: 10, clientY: 20 });
  assert.deepEqual(taps, []);
  assert.deepEqual(aim, [], 'A tap must not move the last aimed direction');
  surface.send('pointerup', { pointerId: 1, clientX: 10, clientY: 20 });
  assert.deepEqual(taps, [1]);
  assert.deepEqual(aim, ['released']);
  surface.send('pointerdown', { pointerId: 2, clientX: 0, clientY: 0 });
  surface.send('pointermove', { pointerId: 2, clientX: 30, clientY: 0 });
  surface.send('pointerup', { pointerId: 2, clientX: 30, clientY: 0 });
  assert.deepEqual(taps, [1], 'Dragging to aim must not trigger a shot');
  assert.equal(input.pressed('fire'), false, 'Adapter only emits intent');
});

test('cancel, capture loss and long hold cannot accidentally tap to fire', () => {
  const surface = new Surface(), taps = [];
  const input = new BrowserInput();
  input.bindAim(surface, () => {}, { onTap: e => taps.push(e.pointerId), maxTapDuration: -1 });
  surface.send('pointerdown', { pointerId: 1 }); surface.send('pointerup', { pointerId: 1 });
  surface.send('pointerdown', { pointerId: 2 }); surface.send('pointercancel', { pointerId: 2 });
  surface.send('pointerdown', { pointerId: 3 }); surface.send('lostpointercapture', { pointerId: 3 });
  assert.deepEqual(taps, []);
});

test('a second pointer cannot convert an active drag into a tap', () => {
  const aim = new Surface(), taps = [], changes = [];
  const input = new BrowserInput();
  input.bindAim(aim, e => { if (e) changes.push(e.pointerId); }, { onTap: e => taps.push(e.pointerId) });
  aim.send('pointerdown', { pointerId: 1, clientX: 0 });
  aim.send('pointerdown', { pointerId: 2, clientX: 100 });
  aim.send('pointermove', { pointerId: 2, clientX: 120 });
  aim.send('pointerup', { pointerId: 2 });
  aim.send('pointermove', { pointerId: 1, clientX: 20 });
  aim.send('pointerup', { pointerId: 1 });
  assert.deepEqual(changes, [1]);
  assert.deepEqual(taps, []);
});

test('swipe with coalesced missing pointermove never misfires on release', () => {
  const input = new BrowserInput(), surface = new Surface(), aim = [], taps = [];
  input.bindAim(surface, event => aim.push(event ? [event.clientX, event.clientY] : 'released'),
                { onTap: () => taps.push('fire'), dragThreshold: 10 });
  surface.send('pointerdown', { pointerId: 1, clientX: 100, clientY: 100 });
  surface.send('pointerup', { pointerId: 1, clientX: 170, clientY: 110 });
  assert.deepEqual(taps, []);
  assert.deepEqual(aim, [[170, 110], 'released']);
});

test('last drag endpoint is captured without a duplicate final update', () => {
  const input = new BrowserInput(), surface = new Surface(), aim = [];
  input.bindAim(surface, event => aim.push(event ? event.clientX : 'released'));
  surface.send('pointerdown', { pointerId: 1, clientX: 10, clientY: 20 });
  surface.send('pointermove', { pointerId: 1, clientX: 35, clientY: 20 });
  surface.send('pointerup', { pointerId: 1, clientX: 39, clientY: 20 });
  assert.deepEqual(aim, [35, 39, 'released']);
  aim.length = 0;
  surface.send('pointerdown', { pointerId: 2, clientX: 10, clientY: 20 });
  surface.send('pointermove', { pointerId: 2, clientX: 35, clientY: 20 });
  surface.send('pointerup', { pointerId: 2, clientX: 35, clientY: 20 });
  assert.deepEqual(aim, [35, 'released']);
});

class GestureClock {
  now = 0;
  next = 0;
  tasks = new Map();
  schedule = (fn, delay) => {
    const id = ++this.next;
    this.tasks.set(id, { fn, at: this.now + delay });
    return id;
  };
  cancelSchedule = id => { this.tasks.delete(id); };
  advance(ms) {
    this.now += ms;
    for (const [id, task] of [...this.tasks])
      if (task.at <= this.now) { this.tasks.delete(id); task.fn(); }
  }
}
const withClock = clock => ({ schedule: clock.schedule, cancelSchedule: clock.cancelSchedule,
                            dragThreshold: 11, holdDelay: 210 });

test('stationary 210ms hold emits one held trigger down and releases on up', () => {
  const clock = new GestureClock(), surface = new Surface(), holds = [], taps = [], phases = [];
  const input = new BrowserInput();
  input.bindAim(surface, () => {}, { ...withClock(clock),
    onTap: () => taps.push('tap'),
    onHoldChange: (pressed,id) => holds.push([pressed,id]),
    onGesture: phase => phases.push(phase) });
  surface.send('pointerdown',{pointerId:12,clientX:200,clientY:110});
  clock.advance(209); assert.deepEqual(holds,[]);
  clock.advance(1); assert.deepEqual(holds,[[true,12]]);
  surface.send('pointerup',{pointerId:12,clientX:200,clientY:110});
  assert.deepEqual(holds,[[true,12],[false,12]]);
  assert.deepEqual(taps,[]);
  assert.deepEqual(phases,['pending','held','idle']);
});

test('quick repeated taps are separate and never wait for double tap', () => {
  const clock = new GestureClock(), surface = new Surface(), taps = [], holds = [];
  const input = new BrowserInput();
  input.bindAim(surface, () => {}, { ...withClock(clock), onTap: e => taps.push(e.pointerId),
    onHoldChange: value => holds.push(value) });
  for (let i=1;i<=20;i++) {
    surface.send('pointerdown',{pointerId:i,clientX:100,clientY:120});
    clock.advance(60);
    surface.send('pointerup',{pointerId:i,clientX:104,clientY:124});
    clock.advance(40);
  }
  assert.equal(taps.length,20);
  assert.deepEqual(holds,[]);
  assert.equal(clock.tasks.size,0);
});

test('moving before hold threshold cancels firing and tracks final aim', () => {
  const clock = new GestureClock(), surface = new Surface(), holds = [], taps = [], aiming = [];
  const input = new BrowserInput();
  input.bindAim(surface, e => { if(e) aiming.push(e.clientX); },
    { ...withClock(clock), onTap:()=>taps.push('tap'),onHoldChange:v=>holds.push(v) });
  surface.send('pointerdown',{pointerId:4,clientX:50,clientY:80});
  clock.advance(90);
  surface.send('pointermove',{pointerId:4,clientX:63,clientY:80});
  clock.advance(300);
  surface.send('pointerup',{pointerId:4,clientX:70,clientY:80});
  assert.deepEqual(holds,[]);
  assert.deepEqual(taps,[]);
  assert.deepEqual(aiming,[63,70]);
});

test('hold then drag retains trigger until release without an extra tap', () => {
  const clock = new GestureClock(), surface = new Surface(), holds = [], taps = [], aiming = [];
  const input = new BrowserInput();
  input.bindAim(surface,e=>{if(e)aiming.push(e.clientX);},{
    ...withClock(clock), onTap:()=>taps.push('tap'),onHoldChange:(pressed)=>holds.push(pressed)});
  surface.send('pointerdown',{pointerId:7,clientX:60,clientY:80});
  clock.advance(210);
  surface.send('pointermove',{pointerId:7,clientX:65,clientY:80}); // Jitter cannot steer.
  surface.send('pointermove',{pointerId:7,clientX:100,clientY:80});
  surface.send('pointerup',{pointerId:7,clientX:110,clientY:80});
  assert.deepEqual(holds,[true,false]);
  assert.deepEqual(taps,[]);
  assert.deepEqual(aiming,[100,110]);
});

test('pagehide and cancellation release held trigger, cancel pending timer', () => {
  const clock = new GestureClock(), surface = new Surface(), win = new Surface(), doc = new Surface();
  const holds = [], taps = [], input = new BrowserInput();
  input.bindAim(surface,()=>{},{...withClock(clock),
    onTap:()=>taps.push('tap'),onHoldChange:pressed=>holds.push(pressed)});
  input.bindLifecycle(win,doc);
  surface.send('pointerdown',{pointerId:10});
  win.send('pagehide');
  clock.advance(300);
  assert.deepEqual(holds,[]);
  surface.send('pointerdown',{pointerId:11});
  clock.advance(210);
  surface.send('pointercancel',{pointerId:11});
  assert.deepEqual(holds,[true,false]);
  assert.deepEqual(taps,[]);
  assert.equal(clock.tasks.size,0);
});

test('second pointer cannot steal active held fire or restart its timer', () => {
  const clock = new GestureClock(), surface = new Surface(), holds = [], taps = [];
  const input = new BrowserInput();
  input.bindAim(surface,()=>{},{...withClock(clock),
    onTap:e=>taps.push(e.pointerId),onHoldChange:(v,id)=>holds.push([v,id])});
  surface.send('pointerdown',{pointerId:1});
  surface.send('pointerdown',{pointerId:2});
  clock.advance(210);
  surface.send('pointerup',{pointerId:2});
  assert.deepEqual(holds,[[true,1]]);
  surface.send('pointerup',{pointerId:1});
  assert.deepEqual(holds,[[true,1],[false,1]]);
  assert.deepEqual(taps,[]);
});
