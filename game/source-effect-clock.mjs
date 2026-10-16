// Engine-to-renderer timing adapter, not a game rule. The recovered C# weapon
// runtime owns the scaled Unity Time.time clock, and each shot emits that clock.
// Do not advance one-shot effects using performance.now(): WASM frames, pauses,
// and Safari backgrounding can make browser wall time diverge from source time.
export function sourceEffectAge(sourceTime, eventTime) {
  if (!Number.isFinite(sourceTime) || !Number.isFinite(eventTime)) {
    throw new RangeError('Missing or invalid recovered source effect clock');
  }
  // Frame ordering and finite float quantization can put emission slightly
  // ahead of the most recent snapshot. Never play source animation backward.
  return Math.max(0, sourceTime - eventTime);
}
