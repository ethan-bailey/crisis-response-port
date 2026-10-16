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
 const input=new BrowserInput(),stick=new Surface(),keyboard=new Surface();
 const thumb={style:{transform:''},offsetWidth:44};
 const classes=new Set(),properties=new Map();
 stick.classList={add:x=>classes.add(x),remove:(...xs)=>xs.forEach(x=>classes.delete(x)),toggle:(x,on)=>on?classes.add(x):classes.delete(x)};
 stick.style={setProperty:(key,value)=>properties.set(key,value)};
 stick.getBoundingClientRect=()=>({left:40,top:20,width:200,height:200});
 input.bindMovementStick(stick,thumb);
 input.bindKeyboard(keyboard,{KeyA:'left',KeyD:'right',ShiftLeft:'sprint'});
 const move=(type,x,y=120,pointerId=9)=>stick.send(type,{pointerId,clientX:x,clientY:y});
 return {input,stick,keyboard,thumb,classes,properties,move};
}

test('circular joystick snaps to center and tracks both axes without vertical locomotion',()=>{
 const {input,thumb,move,stick}=analogStickFixture();
 move('pointerdown',140,120);
 assert.equal(input.movementMagnitude(1),0);
 move('pointermove',140,20); // vertical-only gesture
 assert.equal(input.pressed('left'),false);assert.equal(input.pressed('right'),false);
 assert.match(thumb.style.transform,/translate3d\(0px,-/);
 move('pointermove',152,120); // 12% horizontal, inside 16% dead zone
 assert.equal(input.movementMagnitude(1),0);
 move('pointermove',190,120); // half right
 assert.ok(Math.abs(input.movementMagnitude(1)-(.5-.16)/.84)<1e-8);
 move('pointermove',240,120); // full right/sprint
 assert.equal(input.movementMagnitude(1),1);
 move('pointermove',40,120); // full left without releasing
 assert.equal(input.movementMagnitude(-1),1);
 assert.equal(input.pressed('right'),false);
 move('pointerup',40);
 assert.equal(input.movementMagnitude(-1),0);
 assert.equal(thumb.style.transform,'');
 assert.equal(stick.hasPointerCapture(9),false);
 input.dispose();
});

test('walk-to-sprint profile is continuous at transition and full pull reaches original sprint speed',()=>{
 const {input,move,classes}=analogStickFixture();
 const walk=1.5,sprint=4.2;
 move('pointerdown',140,120);
 for(const x of [150,180,194,195,196,210,230,240]){
  move('pointermove',x,120);
  const p=input.movementProfile(1,walk,sprint,false);
  const speed=(p.sprint?sprint:walk)*p.magnitude;
  assert.ok(speed>=0&&speed<=sprint);
 }
 const splitX=140+100*(.16+.84*.65);
 move('pointermove',splitX-.001,120);
 const below=input.movementProfile(1,walk,sprint);
 move('pointermove',splitX+.001,120);
 const above=input.movementProfile(1,walk,sprint);
 assert.equal(below.sprint,false);
 assert.equal(above.sprint,true);
 assert.ok(Math.abs(below.magnitude*walk-above.magnitude*sprint)<.001);
 move('pointermove',240,120);
 assert.deepEqual(input.movementProfile(1,walk,sprint),{magnitude:1,sprint:true});
 assert.equal(classes.has('sprinting'),true);
 assert.equal(input.pressed('sprint'),true,'full-pull sprint must reach original InputHandler');
 move('pointerup',240,120);
 assert.equal(input.pressed('sprint'),false,'spring release must reset source sprint action');
 assert.equal(classes.has('sprinting'),false);
});

test('keyboard Shift retains source sprint and digital input overrides thumb speed',()=>{
 const {input,keyboard,move}=analogStickFixture();
 move('pointerdown',185,120);
 assert.ok(input.movementMagnitude(1)<1);
 keyboard.send('keydown',{code:'KeyD'});
 assert.deepEqual(input.movementProfile(1,1.5,4.2),{magnitude:1,sprint:false});
 keyboard.send('keydown',{code:'ShiftLeft'});
 assert.deepEqual(input.movementProfile(1,1.5,4.2,true),{magnitude:1,sprint:true});
 keyboard.send('keyup',{code:'KeyD'});
 assert.ok(input.movementProfile(1,1.5,4.2).magnitude<1);
 keyboard.send('keydown',{code:'KeyA'});
 assert.equal(input.movementMagnitude(-1),1);
 move('pointerup',185,120);
 assert.equal(input.pressed('left'),true);
 keyboard.send('keyup',{code:'KeyA'});
 keyboard.send('keyup',{code:'ShiftLeft'});
 assert.equal(input.movementMagnitude(-1),0);
});

test('second finger, cancellation, capture loss and background reset safely release joystick',()=>{
 const {input,stick,thumb,classes,move}=analogStickFixture();
 const win=new Surface(),doc=new Surface();
 input.bindLifecycle(win,doc);
 move('pointerdown',240,120,9);
 move('pointerdown',40,120,10);
 move('pointerup',40,120,10);
 assert.equal(input.movementMagnitude(1),1);
 move('pointercancel',240,120,9);
 assert.equal(input.movementMagnitude(1),0);
 move('pointerdown',40,120,11);
 assert.equal(input.movementMagnitude(-1),1);
 stick.send('lostpointercapture',{pointerId:11});
 assert.equal(input.movementMagnitude(-1),0);
 move('pointerdown',240,120,12);
 win.send('blur');
 assert.equal(input.movementMagnitude(1),0);
 assert.equal(stick.hasPointerCapture(12),false);
 assert.equal(thumb.style.transform,'');
 assert.equal(classes.has('active'),false);
 move('pointerdown',240,120,13);
 input.dispose();
 assert.equal(input.pressed('right'),false);
 move('pointermove',40,120,13);
 assert.equal(input.pressed('left'),false);
});
