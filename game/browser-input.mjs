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

  // Browser gesture classification only. The original InputHandler remains the
  // authority for interpreting a fire press; this adapter never fires weapons.
  bindAim(surface, onAim, { onTap = () => {}, dragThreshold = 8, maxTapDuration = 350 } = {}) {
    let gesture = null;
    const coords = event => [event.clientX ?? 0, event.clientY ?? 0];
    const release = event => {
      if (event.pointerId !== gesture?.id) return;
      const finished = gesture;
      gesture = null;
      onAim(null); // Freeze the final character-local aim direction.
      if (surface.hasPointerCapture?.(event.pointerId)) surface.releasePointerCapture(event.pointerId);
      // Cancelled/capture-lost gestures and long holds must never shoot.
      if (event.type === 'pointerup' && !finished.dragged &&
          event.timeStamp - finished.startedAt <= maxTapDuration) onTap(event);
    };
    this.listen(surface, 'pointerdown', event => {
      if (event.button !== 0 || gesture !== null) return;
      event.preventDefault();
      const [x, y] = coords(event);
      gesture = { id: event.pointerId, x, y, startedAt: event.timeStamp, dragged: false };
      surface.setPointerCapture(event.pointerId);
    });
    this.listen(surface, 'pointermove', event => {
      if (event.pointerId !== gesture?.id) return;
      const [x, y] = coords(event);
      if (!gesture.dragged &&
          (x - gesture.x) ** 2 + (y - gesture.y) ** 2 >= dragThreshold ** 2) {
        gesture.dragged = true;
      }
      if (gesture.dragged) onAim(event);
    });
    for (const type of ['pointerup', 'pointercancel', 'lostpointercapture'])
      this.listen(surface, type, release);
    this.listen(surface, 'contextmenu', event => event.preventDefault());
    // Safari can suspend or resize without dispatching pointerup.
    this.resetAim = () => {
      const previous = gesture;
      gesture = null;
      if (previous) onAim(null);
      if (previous && surface.hasPointerCapture?.(previous.id))
        surface.releasePointerCapture(previous.id);
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
