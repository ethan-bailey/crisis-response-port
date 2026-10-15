// Browser device adapter only. Gameplay and long-press interpretation belong to
// the recovered InputHandler / PlayerController, not to this module.
export class BrowserInput {
  constructor({ onChange = () => {}, onPress = () => {} } = {}) {
    this.sources = new Map();
    this.onChange = onChange;
    this.onPress = onPress;
    this.cleanup = [];
  }

  pressed(action) { return (this.sources.get(action)?.size || 0) > 0; }

  set(action, source, pressed) {
    const before = this.pressed(action);
    const sources = this.sources.get(action) || new Set();
    if (pressed) sources.add(source); else sources.delete(source);
    if (sources.size) this.sources.set(action, sources); else this.sources.delete(action);
    const after = this.pressed(action);
    if (before !== after) {
      this.onChange(action, after);
      if (after) this.onPress(action);
    }
  }

  clear() {
    const held = [...this.sources.keys()];
    this.sources.clear();
    for (const action of held) this.onChange(action, false);
  }

  listen(target, type, handler) {
    target.addEventListener(type, handler);
    this.cleanup.push(() => target.removeEventListener(type, handler));
  }

  bindButton(button, action) {
    const source = id => `button:${action}:${id}`;
    const release = event => {
      event.preventDefault();
      this.set(action, source(event.pointerId), false);
      if (button.hasPointerCapture?.(event.pointerId)) button.releasePointerCapture(event.pointerId);
    };
    this.listen(button, 'pointerdown', event => {
      if (event.button !== 0) return;
      event.preventDefault();
      button.setPointerCapture(event.pointerId);
      this.set(action, source(event.pointerId), true);
    });
    for (const type of ['pointerup', 'pointercancel', 'lostpointercapture']) this.listen(button, type, release);
    this.listen(button, 'contextmenu', event => event.preventDefault());
  }

  bindKeyboard(target, bindings) {
    this.listen(target, 'keydown', event => {
      const action = bindings[event.code];
      if (!action) return;
      event.preventDefault();
      // Auto-repeat after blur must not re-arm an action before a fresh press.
      if (!event.repeat) this.set(action, `key:${event.code}`, true);
    });
    this.listen(target, 'keyup', event => {
      const action = bindings[event.code];
      if (!action) return;
      event.preventDefault();
      this.set(action, `key:${event.code}`, false);
    });
  }

  bindAim(surface, onAim, onTap = () => {}) {
    let pointer = null, start = null, dragging = false;
    const release = event => {
      if (event.pointerId !== pointer) return;
      const tap = event.type === 'pointerup' && !dragging && performance.now()-start.time <= 300;
      const previous = pointer; pointer = null;
      this.set('fire', `aim:${previous}`, false);
      if (surface.hasPointerCapture?.(previous)) surface.releasePointerCapture(previous);
      if (tap && start.touch) onTap(event);
    };
    this.listen(surface, 'pointerdown', event => {
      if (event.button !== 0 || pointer !== null) return;
      event.preventDefault(); pointer = event.pointerId;
      start = {x:event.clientX, y:event.clientY, time:performance.now(), touch:event.pointerType !== 'mouse'};
      dragging = false; surface.setPointerCapture(pointer);
      if (!start.touch) {onAim(event); this.set('fire', `aim:${pointer}`, true);}
    });
    this.listen(surface, 'pointermove', event => {
      if(pointer===null&&event.pointerType==='mouse'){onAim(event);return;}
      if (event.pointerId !== pointer) return;
      if (Math.hypot(event.clientX-start.x,event.clientY-start.y)>8) dragging=true;
      if (dragging || !start.touch) onAim(event);
    });
    for (const type of ['pointerup', 'pointercancel', 'lostpointercapture']) this.listen(surface, type, release);
    this.listen(surface, 'contextmenu', event => event.preventDefault());
    this.resetAim = () => {
      const previous=pointer; pointer=null; start=null; dragging=false;
      if (previous !== null && surface.hasPointerCapture?.(previous)) surface.releasePointerCapture(previous);
    };
  }

  bindLifecycle(windowTarget, documentTarget) {
    const reset = () => { this.resetAim?.(); this.clear(); };
    this.listen(windowTarget, 'blur', reset);
    this.listen(windowTarget, 'pagehide', reset);
    this.listen(windowTarget, 'resize', reset);
    this.listen(documentTarget, 'visibilitychange', () => { if (documentTarget.hidden) reset(); });
  }

  dispose() { this.resetAim?.(); this.clear(); for (const remove of this.cleanup.splice(0)) remove(); }
}
