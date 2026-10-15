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

  // Touch interaction only. Original C# InputHandler and weapon logic decide
  // shots, fire modes and recoil. A gesture may hold the raw trigger, but this
  // adapter never computes a fire rate or simulates a projectile.
  bindAim(surface, onAim, {
    onTap = () => {}, onHoldChange = () => {}, onGesture = () => {},
    dragThreshold = 11, holdDelay = 210, maxTapDuration = 350,
    schedule = (fn, delay) => setTimeout(fn, delay),
    cancelSchedule = token => clearTimeout(token)
  } = {}) {
    let gesture = null;
    const coords = event => [event.clientX ?? 0, event.clientY ?? 0];
    const isDrag = (g, x, y) =>
      (x - g.x) ** 2 + (y - g.y) ** 2 >= dragThreshold ** 2;
    const stopTimer = g => {
      if (g?.timer !== null && g?.timer !== undefined) {
        cancelSchedule(g.timer);
        g.timer = null;
      }
    };
    const stopHold = g => {
      if (g.state === 'held') onHoldChange(false, g.id);
    };
    const endGesture = (event, cancelled = false) => {
      const g = gesture;
      if (!g || (event && event.pointerId !== g.id)) return;
      gesture = null;
      stopTimer(g);
      if (!cancelled && event?.type === 'pointerup' &&
          Number.isFinite(event.clientX) && Number.isFinite(event.clientY)) {
        const [x, y] = coords(event);
        if (isDrag(g, x, y) && g.state === 'pending') g.state = 'aiming';
        if ((g.state === 'aiming' || (g.state === 'held' && isDrag(g,x,y))) &&
            (x !== g.lastAimX || y !== g.lastAimY))
          onAim(event);
      }
      const wasPending = g.state === 'pending';
      stopHold(g); // Release a held original trigger on every exit path.
      onAim(null); // Retain final character-local aim.
      onGesture('idle', g.id);
      if (surface.hasPointerCapture?.(g.id)) surface.releasePointerCapture(g.id);
      // No browser click/double-click delay and no fake weapon timing.
      if (!cancelled && event?.type === 'pointerup' && wasPending &&
          event.timeStamp - g.startedAt <= maxTapDuration) onTap(event);
    };
    this.listen(surface, 'pointerdown', event => {
      if (event.button !== 0 || gesture !== null) return;
      event.preventDefault();
      const [x, y] = coords(event);
      const g = {id:event.pointerId,x,y,startedAt:event.timeStamp,
        state:'pending',timer:null,lastAimX:x,lastAimY:y};
      gesture = g;
      surface.setPointerCapture(g.id);
      onGesture('pending',g.id);
      g.timer = schedule(() => {
        if (gesture !== g || g.state !== 'pending') return;
        g.timer = null;
        g.state = 'held';
        onGesture('held',g.id);
        onHoldChange(true,g.id);
      }, holdDelay);
    });
    this.listen(surface, 'pointermove', event => {
      const g = gesture;
      if (!g || event.pointerId !== g.id) return;
      const [x, y] = coords(event);
      if (g.state === 'pending' && isDrag(g, x, y)) {
        g.state = 'aiming';
        stopTimer(g);
        onGesture('aiming',g.id);
      }
      if (g.state === 'aiming' ||
          (g.state === 'held' && isDrag(g,x,y))) {
        g.lastAimX = x;g.lastAimY = y;
        onAim(event);
      }
    });
    this.listen(surface, 'pointerup', event => endGesture(event));
    this.listen(surface, 'pointercancel', event => endGesture(event,true));
    this.listen(surface, 'lostpointercapture', event => endGesture(event,true));
    this.listen(surface, 'contextmenu', event => event.preventDefault());
    // Safari may suspend, rotate or abandon an active captured touch.
    this.resetAim = () => endGesture(null,true);
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
