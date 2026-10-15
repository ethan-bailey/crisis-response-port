// Device event scheduler only. Each tap becomes one raw original InputHandler
// key-down tick followed by a distinct key-up tick. No weapon behavior lives here.
export class TapFireQueue {
  constructor(maxQueued = 8) {
    if (!Number.isInteger(maxQueued) || maxQueued < 1) throw new RangeError('maxQueued must be a positive integer');
    this.maxQueued = maxQueued;
    this.total = 0;
    this.reset();
  }
  tap() {
    this.total++;
    if (this.pending < this.maxQueued) this.pending++;
  }
  advance() {
    if (this.releaseNext) {
      this.releaseNext = false;
      return false;
    }
    if (this.pending === 0) return false;
    this.pending--;
    this.releaseNext = true;
    return true;
  }
  reset() {
    this.pending = 0;
    this.releaseNext = false;
  }
}
