// Suppress a held trigger during fast retargeting, while allowing controlled
// aim corrections to keep firing. Coordinates use normalized device coordinates.
export class AimFireGate {
  constructor({ rapidSpeed = 1.8, settledSpeed = 0.35, settleMs = 140 } = {}) {
    this.rapidSpeed = rapidSpeed;
    this.settledSpeed = settledSpeed;
    this.settleMs = settleMs;
    this.last = null;
    this.locked = false;
    this.slowSince = null;
  }

  update(x, y, nowMs, aimConverged = true) {
    if (this.last === null) {
      this.last = { x, y, nowMs };
      return true;
    }
    const elapsed = Math.max(1, Math.min(100, nowMs - this.last.nowMs));
    const speed = Math.hypot(x - this.last.x, y - this.last.y) / (elapsed / 1000);
    this.last = { x, y, nowMs };

    if (speed >= this.rapidSpeed) {
      this.locked = true;
      this.slowSince = null;
    } else if (this.locked && speed <= this.settledSpeed) {
      if (this.slowSince === null) this.slowSince = nowMs;
      if (nowMs - this.slowSince >= this.settleMs && aimConverged) this.locked = false;
    } else if (this.locked) {
      this.slowSince = null;
    }
    return !this.locked;
  }

  reset() { this.last = null; this.locked = false; this.slowSince = null; }
}

// A held trigger follows gradual aim changes. One-shot tap fire waits until
// the new aim has been applied by a source update.
export function resolveFireInput({ heldFire, tapRequested, aimReady, fireAimReady }) {
  if (!fireAimReady) return false;
  return heldFire || (tapRequested && aimReady);
}
