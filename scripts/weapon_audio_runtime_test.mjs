import test from 'node:test';
import assert from 'node:assert/strict';
import {
  MAX_WEAPON_OUTPUT_GAIN,
  classifyWeaponPlayback,
  clampWeaponEventGain,
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
