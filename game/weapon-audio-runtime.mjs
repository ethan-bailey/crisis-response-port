export const MAX_WEAPON_OUTPUT_GAIN = 0.16;

export function classifyWeaponPlayback({ eventAgeSeconds, contextState, bufferLoaded }) {
  if (!Number.isFinite(eventAgeSeconds) || eventAgeSeconds < -0.05 || eventAgeSeconds > 0.12)
    return 'stale';
  if (contextState !== 'running') return 'suspended';
  if (!bufferLoaded) return 'unloaded';
  return 'play';
}

export function clampWeaponEventGain(value) {
  return Number.isFinite(value) ? Math.max(0, Math.min(1, value)) : 0;
}

export async function preloadWeaponAudio(audioData, audioRuntime) {
  const entries = Object.entries(audioData || {}).filter(([, info]) => info?.source && info?.url);
  await Promise.all(entries.map(([key, info]) => audioRuntime.load(key, info.url)));
  return entries.map(([key]) => key);
}

export function createSourceWeaponAudioDispatcher(audioData, audioRuntime) {
  const keys = new Map(Object.entries(audioData || {}).filter(([, info]) => info.source)
    .map(([key, info]) => [info.source, key]));
  return function dispatch(events, sourceTime) {
    for (const event of events || []) {
      const key = keys.get(event.sourcePath);
      if (!key || event.action !== 'oneShot') { audioRuntime.noteUnmappedSourceEvent(); continue; }
      const volume = (Number.isFinite(event.volume) ? event.volume : 0) * (key === 'audioFire' ? 0.6 : 1);
      audioRuntime.play(key, volume, sourceTime - Number(event.time));
    }
  };
}

export function createWeaponAudioRuntime(audioData, { AudioContextClass, fetchAudio } = {}) {
  const AudioContext = AudioContextClass ?? globalThis.AudioContext ?? globalThis.webkitAudioContext;
  const request = fetchAudio ?? globalThis.fetch?.bind(globalThis);
  const context = AudioContext ? new AudioContext() : null;
  const buffers = new Map();
  const loadingAudio = new Map();
  const stats = { played: 0, droppedSuspended: 0, droppedUnloaded: 0, droppedStale: 0,
    decodeErrors: 0, unlockFailures: 0, unmappedSourceEvents: 0 };
  const outputGain = context?.createGain() ?? null;
  const limiter = context?.createDynamicsCompressor() ?? null;
  if (outputGain && limiter) {
    outputGain.gain.value = MAX_WEAPON_OUTPUT_GAIN;
    limiter.threshold.value = -8;
    limiter.knee.value = 6;
    limiter.ratio.value = 10;
    limiter.attack.value = 0.003;
    limiter.release.value = 0.15;
    outputGain.connect(limiter).connect(context.destination);
  }

  function load(key, url) {
    if (!context || !request || !url) return Promise.resolve(null);
    if (buffers.has(key)) return Promise.resolve(buffers.get(key));
    if (loadingAudio.has(key)) return loadingAudio.get(key);
    const promise = request(url).then(response => {
      if (!response.ok) throw new Error(`Audio request failed: ${url} (${response.status})`);
      return response.arrayBuffer();
    }).then(bytes => context.decodeAudioData(bytes)).then(buffer => {
      buffers.set(key, buffer);
      return buffer;
    }).catch(() => {
      stats.decodeErrors++;
      return null;
    }).finally(() => loadingAudio.delete(key));
    loadingAudio.set(key, promise);
    return promise;
  }

  function unlock() {
    if (!context || context.state === 'running') return;
    context.resume().catch(() => { stats.unlockFailures++; });
  }
  const pointerDown = () => unlock();
  if (context && globalThis.addEventListener) globalThis.addEventListener('pointerdown', pointerDown, { capture: true, passive: true });

  function play(key, volume = 1, eventAgeSeconds = 0) {
    const disposition = classifyWeaponPlayback({
      eventAgeSeconds,
      contextState: context?.state ?? 'unavailable',
      bufferLoaded: buffers.has(key)
    });
    if (disposition === 'stale') { stats.droppedStale++; return disposition; }
    if (disposition === 'suspended') { stats.droppedSuspended++; return disposition; }
    if (disposition === 'unloaded') { stats.droppedUnloaded++; return disposition; }
    const source = context.createBufferSource();
    const gain = context.createGain();
    source.buffer = buffers.get(key);
    gain.gain.value = clampWeaponEventGain(volume);
    source.connect(gain).connect(outputGain);
    source.onended = () => { source.disconnect(); gain.disconnect(); };
    // Never schedule a source into a suspended context. A blocked one-shot is
    // discarded now and cannot emerge later, out of sync with the shot.
    source.start();
    stats.played++;
    return 'play';
  }

  function snapshot() {
    return { ...stats, contextState: context?.state ?? 'unavailable',
      outputGain: outputGain?.gain.value ?? 0, maxOutputGain: MAX_WEAPON_OUTPUT_GAIN,
      loadedBuffers: buffers.size, pendingBuffers: loadingAudio.size };
  }

  function noteUnmappedSourceEvent() { stats.unmappedSourceEvents++; }

  return { context, load, play, snapshot, unlock, noteUnmappedSourceEvent };
}
