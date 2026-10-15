import test from 'node:test';
import assert from 'node:assert/strict';
import { BrowserInput } from '../game/browser-input.mjs';
import { TapFireQueue } from '../game/tap-fire-queue.mjs';

// Pure device adapter integration; no substituted weapon or ballistics systems.
class Surface extends EventTarget {
  captured = new Set();
  setPointerCapture(id) { this.captured.add(id); }
  hasPointerCapture(id) { return this.captured.has(id); }
  releasePointerCapture(id) { this.captured.delete(id); }
  send(type, fields={}) {
    const ev = new Event(type, { cancelable:true });
    Object.assign(ev, {button:0,clientX:100,clientY:100,pointerId:1,...fields});
    this.dispatchEvent(ev);
  }
}
class Clock {
  now=0;
  nextId=1;
  tasks=new Map();
  schedule=(fn, ms)=>{const id=this.nextId++;this.tasks.set(id,{at:this.now+ms,fn});return id;};
  cancel=(id)=>this.tasks.delete(id);
  go(ms) {
    this.now+=ms;
    for(const [id,task] of [...this.tasks])
      if(task.at<=this.now){this.tasks.delete(id);task.fn();}
  }
}
function setup() {
  const clock=new Clock(), surface=new Surface(), win=new Surface(), doc=new Surface();
  const input=new BrowserInput(), queue=new TapFireQueue(32,1000), aim=[], modes=[];
  input.bindAim(surface,e=>{if(e)aim.push([e.clientX,e.clientY]);},{
    dragThreshold:11,holdDelay:210,
    schedule:clock.schedule,cancelSchedule:clock.cancel,
    onTap:()=>queue.tap(clock.now),
    onHoldChange:(value,id)=>queue.hold('aim:'+id,value),
    onGesture:state=>modes.push(state)
  });
  input.bindLifecycle(win,doc);
  return {clock,surface,win,doc,input,queue,aim,modes};
}
test('20 repeated same-spot taps dispatch 20 discrete original input edges',()=>{
  const s=setup(), edges=[];
  for(let i=0;i<20;i++){
    s.surface.send('pointerdown',{pointerId:i+1,clientX:130,clientY:170});
    s.clock.go(45);
    s.surface.send('pointerup',{pointerId:i+1,clientX:133,clientY:172});
    edges.push(s.queue.advance(s.clock.now));
    edges.push(s.queue.advance(s.clock.now));
    s.clock.go(45);
  }
  assert.deepEqual(edges,Array.from({length:40},(_,i)=>i%2===0));
  assert.equal(s.queue.total,20);
  assert.equal(s.queue.pending,0);
  assert.deepEqual(s.aim,[]);
});
test('210ms stationary hold propagates sustained original trigger, then stops',()=>{
  const s=setup();
  s.surface.send('pointerdown',{pointerId:2});
  s.clock.go(209);assert.equal(s.queue.advance(s.clock.now),false);
  s.clock.go(1);assert.equal(s.queue.held,true);
  assert.deepEqual([s.queue.advance(s.clock.now),s.queue.advance(s.clock.now)],[true,true]);
  s.surface.send('pointerup',{pointerId:2});
  assert.deepEqual([s.queue.advance(s.clock.now),s.queue.advance(s.clock.now)],[false,false]);
  assert.equal(s.queue.total,0,'Hold is not misclassified as tap');
});
test('hold then drag continues original held input and changes aim',()=>{
  const s=setup();
  s.surface.send('pointerdown',{pointerId:1,clientX:200,clientY:100});
  s.clock.go(210);
  s.surface.send('pointermove',{pointerId:1,clientX:230,clientY:100});
  assert.equal(s.queue.advance(s.clock.now),true);
  s.surface.send('pointerup',{pointerId:1,clientX:250,clientY:110});
  assert.equal(s.queue.advance(s.clock.now),false);
  assert.deepEqual(s.aim,[[230,100],[250,110]]);
  assert.equal(s.queue.total,0);
});
test('intended swipe never creates trigger input, even without a move event',()=>{
  const s=setup();
  s.surface.send('pointerdown',{pointerId:1,clientX:120,clientY:100});
  s.clock.go(80);
  s.surface.send('pointerup',{pointerId:1,clientX:150,clientY:140});
  assert.equal(s.queue.advance(s.clock.now),false);
  assert.deepEqual(s.aim,[[150,140]]);
});
test('cancel and pagehide immediately end held fire and prevent stale shots',()=>{
  const s=setup();
  s.surface.send('pointerdown',{pointerId:1});
  s.clock.go(210);
  assert.equal(s.queue.advance(s.clock.now),true);
  s.surface.send('pointercancel',{pointerId:1});
  assert.equal(s.queue.advance(s.clock.now),false);
  s.surface.send('pointerdown',{pointerId:2});
  s.clock.go(210);
  assert.equal(s.queue.advance(s.clock.now),true);
  s.win.send('pagehide');
  s.queue.reset(); // Same reset invoked by game/index.html lifecycle hooks.
  assert.equal(s.queue.advance(s.clock.now),false);
  assert.equal(s.clock.tasks.size,0);
});
