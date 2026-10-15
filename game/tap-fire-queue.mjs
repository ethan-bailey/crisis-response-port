// Browser-to-C# input adapter. Only raw trigger state and edges live here;
// firing cadence, selector, ammunition and recoil belong to recovered C# code.
export class TapFireQueue {
  constructor(maxQueued = 8, maxAgeMs = 500) {
    if (!Number.isInteger(maxQueued) || maxQueued < 1)
      throw new RangeError('maxQueued must be a positive integer');
    if (!Number.isFinite(maxAgeMs) || maxAgeMs <= 0)
      throw new RangeError('maxAgeMs must be positive');
    this.maxQueued = maxQueued;
    this.maxAgeMs = maxAgeMs;
    this.total = 0;
    this.dropped = 0;
    this.reset();
  }
  get pending() { return this.taps.length; }
  get held() { return this.holders.size > 0; }
  tap(now = Date.now()) {
    this.total++;
    if (this.taps.length < this.maxQueued) this.taps.push(now);
    else this.dropped++;
  }
  hold(source, pressed) {
    if (pressed) this.holders.add(source);
    else {
      const hadSource = this.holders.delete(source);
      if (hadSource && this.holders.size === 0 && this.lastOutput) {
        // A release is mandatory even if the same finger presses again before
        // the next async original InputHandler tick.
        this.forceRelease = true;
      }
    }
  }
  advance(now = Date.now()) {
    while (this.taps.length && now - this.taps[0] > this.maxAgeMs) {
      this.taps.shift();
      this.dropped++;
    }
    if (this.forceRelease || this.releaseNext) {
      this.forceRelease = false;
      this.releaseNext = false;
      this.lastOutput = false;
      return false;
    }
    // Sustain already asserted original fire input while any source is held.
    if (this.held) {
      this.lastOutput = true;
      return true;
    }
    if (this.pending) {
      this.taps.shift();
      this.releaseNext = true;
      this.lastOutput = true;
      return true;
    }
    this.lastOutput = false;
    return false;
  }
  reset() {
    this.taps = [];
    this.holders = new Set();
    this.lastOutput = false;
    this.releaseNext = false;
    this.forceRelease = false;
  }
}
