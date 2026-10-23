// Browser device adapter only. Gameplay and long-press interpretation belong to
// the recovered InputHandler / PlayerController, not to this module.
export class BrowserInput {
  constructor({ onChange = () => {}, onPress = () => {} } = {}) {
    this.sources = new Map();
    this.onChange = onChange;
    this.onPress = onPress;
    this.cleanup = [];
    this.movementStickAxis = 0;
    this.movementStickAim = 0;
    this.movementStickOvertravel = false;
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
    this.resetMovementStick?.();
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

  // Owner-approved iPhone enhancement: preserve proportional joystick magnitude.
  // The existing digital left/right actions still feed original InputHandler.
  // Only the adapter's target walking/sprinting speed is scaled by this value.
  movementMagnitude(direction) {
    const action = direction < 0 ? 'left' : direction > 0 ? 'right' : null;
    const sources = action && this.sources.get(action);
    if (!sources?.size) return 0;
    // Keyboard, external buttons and accessibility actions retain full speed.
    if ([...sources].some(source => source !== 'stick:movement')) return 1;
    return Math.abs(this.movementStickAxis);
  }

  // Owner-approved thumb joystick for this side-scrolling game. The stick has
  // two-axis visual tracking; only horizontal displacement changes movement.
  // A spring return, dead zone, per-pointer capture and lifecycle reset prevent
  // ghost locomotion. Sprint is inferred from stick travel, never a touch button.
  bindMovementStick(surface, thumb, { deadZone = 0.16, onAim = () => {} } = {}) {
    let pointer = null;
    const source = 'stick:movement';
    const reset = () => {
      const released = pointer;
      pointer = null;
      this.movementStickAxis = 0;
      this.movementStickAim = 0;
      this.movementStickOvertravel = false;
      this.set('left', source, false);
      this.set('right', source, false);
      this.set('sprint', source, false);
      if (thumb) thumb.style.transform = '';
      surface.style?.setProperty?.('--stick-power', '0');
      onAim(0);
      surface.classList.remove('active', 'sprinting');
      if (released !== null && surface.hasPointerCapture?.(released)) surface.releasePointerCapture(released);
    };
    const move = event => {
      if (pointer !== event.pointerId) return;
      const rect = surface.getBoundingClientRect();
      const thumbWidth = thumb?.offsetWidth || 48;
      const thumbHeight = thumb?.offsetHeight || 48;
      const travelX = Math.max(1, (rect.width - thumbWidth) / 2 - 4);
      const travelY = Math.max(1, (rect.height - thumbHeight) / 2 - 4);
      const rawX = (event.clientX - (rect.left + rect.width / 2)) / travelX;
      const rawY = (event.clientY - (rect.top + rect.height / 2)) / travelY;
      const length = Math.hypot(rawX, rawY);
      const scale = length > 1 ? 1 / length : 1;
      const horizontal = rawX * scale;
      const vertical = rawY * scale;
      const magnitude = Math.max(0, (Math.abs(horizontal) - deadZone) / (1 - deadZone));
      const overtravel = Math.abs(rawX) > 1;
      this.movementStickOvertravel = overtravel;
      this.movementStickAxis = Math.sign(rawX) * (overtravel ? 1 : magnitude);
      const aimPower = Math.max(0, (Math.abs(vertical) - deadZone) / (1 - deadZone));
      this.movementStickAim = -Math.sign(vertical) * aimPower;
      onAim(this.movementStickAim);
      this.set('left', source, this.movementStickAxis < 0);
      this.set('right', source, this.movementStickAxis > 0);
      const sprinting = overtravel;
      this.set('sprint', source, sprinting);
      surface.classList.toggle('sprinting', sprinting);
      surface.style?.setProperty?.('--stick-power', String(overtravel ? 1 : magnitude));
      if (thumb) {
        thumb.style.transform = `translate3d(${horizontal * travelX}px,${vertical * travelY}px,0)`;
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
    this.resetMovementStick = reset;
    this.cleanup.push(reset);
    this.listen(surface, 'contextmenu', event => event.preventDefault());
  }

  // Preserve the known-good Site behavior: stick travel scales acceleration;
  // only pushing beyond the stick rim selects the sprint cap.
  movementProfile(direction, walkSpeed, sprintSpeed, keyboardSprint = false) {
    const action = direction < 0 ? 'left' : direction > 0 ? 'right' : null;
    const sources = action && this.sources.get(action);
    if (!sources?.size) return { magnitude: 0, sprint: false };
    if ([...sources].some(source => source !== 'stick:movement'))
      return { magnitude: 1, sprint: keyboardSprint };
    const power = Math.abs(this.movementStickAxis);
    return { magnitude: power, sprint: keyboardSprint || this.movementStickOvertravel };
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
