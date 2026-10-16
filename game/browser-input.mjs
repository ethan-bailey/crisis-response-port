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


  // Touch-only movement surface. Produces the EXISTING left/right source input
  // actions; it adds no player movement or animation implementation.
  bindMovementStick(surface, thumb, {deadZone = 0.22} = {}) {
    let pointer = null;
    const source = 'stick:movement';
    const reset = () => {
      pointer = null;
      this.set('left', source, false);
      this.set('right', source, false);
      if (thumb) thumb.style.transform = '';
      surface.classList.remove('active');
    };
    const move = event => {
      if (pointer !== event.pointerId) return;
      const rect = surface.getBoundingClientRect();
      const radius = Math.max(1, rect.width / 2);
      const dx = (event.clientX - (rect.left + rect.width / 2)) / radius;
      const clamped = Math.max(-1, Math.min(1, dx));
      this.set('left', source, clamped < -deadZone);
      this.set('right', source, clamped > deadZone);
      if (thumb) {
        const travel = Math.max(0, (rect.width - thumb.offsetWidth) / 2 - 2);
        thumb.style.transform = `translateX(${clamped * travel}px)`;
      }
      event.preventDefault();
    };
    this.listen(surface, 'pointerdown', event => {
      if (pointer !== null || (event.pointerType === 'mouse' && event.button !== 0)) return;
      pointer = event.pointerId;
      surface.setPointerCapture?.(pointer);
      surface.classList.add('active');
      move(event);
    });
    this.listen(surface, 'pointermove', move);
    for (const type of ['pointerup', 'pointercancel', 'lostpointercapture']) {
      this.listen(surface, type, event => {
        if (event.pointerId !== pointer) return;
        reset();
      });
    }
    this.cleanup.push(reset);
    this.listen(surface, 'contextmenu', event => event.preventDefault());
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
    // CSS pixels, not physical pixels: allow ordinary finger placement error
    // without treating a different target as an attempt to shoot.
    const dragThreshold = 8, sameSpotRadius = 24;
    let pointer = null, start = null, dragging = false, lastTouchAim = null;
    const aimAtTouch = event => {
      onAim(event);
      lastTouchAim = { x: event.clientX, y: event.clientY };
    };
    const release = event => {
      if (event.pointerId !== pointer) return;
      const previous = pointer;
      const isTouch = start.touch;
      const duration = performance.now() - start.time;
      const moved = Math.hypot(event.clientX - start.x, event.clientY - start.y);
      const wasDrag = dragging || (event.type === 'pointerup' && moved > dragThreshold);
      pointer = null; start = null; dragging = false;
      this.set('fire', `aim:${previous}`, false);
      if (surface.hasPointerCapture?.(previous)) surface.releasePointerCapture(previous);
      if (event.type !== 'pointerup' || !isTouch) return;
      if (wasDrag) {
        // Ending a drag establishes its final screen-space aim location.
        aimAtTouch(event);
      } else if (duration <= 300) {
        if (lastTouchAim && Math.hypot(event.clientX - lastTouchAim.x,
                                      event.clientY - lastTouchAim.y) <= sameSpotRadius) {
          onTap(event); // fire only after the user taps the currently aimed spot
        } else {
          aimAtTouch(event); // a tap somewhere else redirects, never fires
        }
      }
    };
    this.listen(surface, 'pointerdown', event => {
      if (event.button !== 0 || pointer !== null) return;
      event.preventDefault(); pointer = event.pointerId;
      start = {x:event.clientX, y:event.clientY, time:performance.now(), touch:event.pointerType !== 'mouse'};
      dragging = false; surface.setPointerCapture(pointer);
      if (!start.touch) {onAim(event); this.set('fire', `aim:${pointer}`, true);}
    });
    this.listen(surface, 'pointermove', event => {
      if (pointer === null && event.pointerType === 'mouse') {onAim(event);return;}
      if (event.pointerId !== pointer) return;
      if (Math.hypot(event.clientX-start.x,event.clientY-start.y)>dragThreshold) dragging=true;
      if (start.touch) {
        if (dragging) aimAtTouch(event);
      } else onAim(event);
    });
    for (const type of ['pointerup', 'pointercancel', 'lostpointercapture']) this.listen(surface, type, release);
    this.listen(surface, 'contextmenu', event => event.preventDefault());
    this.resetAim = () => {
      const previous=pointer; pointer=null; start=null; dragging=false; lastTouchAim=null;
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
