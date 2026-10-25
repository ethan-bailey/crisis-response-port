import test from 'node:test';
import assert from 'node:assert/strict';
import {
  MAX_WEAPON_OUTPUT_GAIN,
  classifyWeaponPlayback,
  clampWeaponEventGain,
  createSourceWeaponAudioDispatcher,
  createWeaponAudioRuntime,
} from '../game/weapon-audio-runtime.mjs';

class FakeNode {
  constructor() { this.gain = { value: 1 }; }
  connect(next) { return next; }
  disconnect() {}
}

let activeContext;
class FakeAudioContext {
  constructor() {
    activeContext = this;
    this.state = 'suspended';
    this.destination = {};
    this.started = 0;
    this.master = null;
  }
  createGain() {
    const node = new FakeNode();
    if (!this.master) this.master = node;
    return node;
  }
  createDynamicsCompressor() {
    return Object.assign(new FakeNode(), Object.fromEntries(
      ['threshold', 'knee', 'ratio', 'attack', 'release'].map(k => [k, { value: 0 }])));
  }
  createBufferSource() {
    const source = new FakeNode();
    source.start = () => { this.started++; };
    return source;
  }
  decodeAudioData() { return Promise.resolve({ decoded: true }); }
  resume() { this.state = 'running'; return Promise.resolve(); }
}

test('one-frame gun audio rejects stale, future, suspended, and unloaded events', () => {
  assert.equal(classifyWeaponPlayback({ eventAgeSeconds: 0.121, contextState: 'running', bufferLoaded: true }), 'stale');
  assert.equal(classifyWeaponPlayback({ eventAgeSeconds: -0.051, contextState: 'running', bufferLoaded: true }), 'stale');
  assert.equal(classifyWeaponPlayback({ eventAgeSeconds: 0, contextState: 'suspended', bufferLoaded: true }), 'suspended');
  assert.equal(classifyWeaponPlayback({ eventAgeSeconds: 0, contextState: 'running', bufferLoaded: false }), 'unloaded');
  assert.equal(classifyWeaponPlayback({ eventAgeSeconds: 0.12, contextState: 'running', bufferLoaded: true }), 'play');
});

test('a blocked suspended shot is dropped and cannot play after audio unlock', async () => {
  const runtime = createWeaponAudioRuntime({}, { AudioContextClass: FakeAudioContext,
    fetchAudio: async () => ({ ok: true, arrayBuffer: async () => new ArrayBuffer(0) }) });
  await runtime.load('audioFire', '/audioFire.ogg');
  assert.equal(runtime.play('audioFire', 0.6, 0), 'suspended');
  assert.equal(activeContext.started, 0);
  runtime.unlock();
  await Promise.resolve();
  assert.equal(runtime.play('audioFire', 0.6, 0), 'play');
  assert.equal(activeContext.started, 1);
  assert.equal(runtime.snapshot().droppedSuspended, 1);
});

test('gun output has a fixed master ceiling and per-event gain is bounded', async () => {
  const runtime = createWeaponAudioRuntime({}, { AudioContextClass: FakeAudioContext,
    fetchAudio: async () => ({ ok: true, arrayBuffer: async () => new ArrayBuffer(0) }) });
  await runtime.load('audioFire', '/audioFire.ogg');
  assert.equal(runtime.snapshot().outputGain, MAX_WEAPON_OUTPUT_GAIN);
  assert.equal(MAX_WEAPON_OUTPUT_GAIN, 0.16);
  assert.equal(clampWeaponEventGain(4), 1);
  assert.equal(clampWeaponEventGain(-1), 0);
  assert.equal(clampWeaponEventGain(Number.NaN), 0);
});

test('FullSource gun events resolve by original clip path and retain source age', () => {
  const played = [], unmapped = [];
  const dispatcher = createSourceWeaponAudioDispatcher({
    audioFire: { source: 'Assets/AudioClip/5.56x45mm-01-unsuppressed.wav' },
    audioSupersonic: { source: 'Assets/AudioClip/5.56x45mm-crack-interior.wav' },
    audioFiringPin: { source: 'Assets/Audio/M4-pin.wav' },
  }, {
    play: (...args) => played.push(args),
    noteUnmappedSourceEvent: () => unmapped.push(true),
  });
  dispatcher([
    { action: 'oneShot', sourcePath: 'Assets/AudioClip/5.56x45mm-01-unsuppressed.wav', volume: 0.5, time: 10.04 },
    { action: 'oneShot', sourcePath: 'Assets/AudioClip/5.56x45mm-crack-interior.wav', volume: 0.8, time: 10.08 },
    { action: 'play', sourcePath: 'Assets/Audio/M4-pin.wav', volume: 0.8, time: 10.05 },
    { action: 'oneShot', sourcePath: 'Assets/Audio/not-mapped.wav', volume: 1, time: 10.06 },
  ], 10.1);
  assert.equal(played.length, 2);
  assert.equal(played[0][0], 'audioFire');
  assert.ok(Math.abs(played[0][1] - 0.3) < 1e-12);
  assert.ok(Math.abs(played[0][2] - 0.06) < 1e-12);
  assert.equal(played[1][0], 'audioSupersonic');
  assert.ok(Math.abs(played[1][1] - 0.8) < 1e-12);
  assert.ok(Math.abs(played[1][2] - 0.02) < 1e-12);
  assert.equal(unmapped.length, 2, 'unsupported loops and unknown source clips are counted rather than guessed');
});
