import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { gunzipSync } from 'node:zlib';
import {
  MAX_WEAPON_OUTPUT_GAIN,
  classifyWeaponPlayback,
  clampWeaponEventGain,
  createSourceWeaponAudioDispatcher,
  createWeaponAudioRuntime,
  preloadWeaponAudio,
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

test('weapon readiness waits until every source audio clip has finished loading', async () => {
  const audioData = { audioFire:{source:'fire.wav',url:'fire.ogg'}, audioInsertMagazine:{source:'in.wav',url:'in.ogg'}, unused:{url:'unused.ogg'} };
  const pending = new Map(), calls = [];
  const runtime = {load:(key,url)=>{calls.push([key,url]);return new Promise(resolve=>pending.set(key,resolve));}};
  let settled = false;
  const ready = preloadWeaponAudio(audioData,runtime).then(keys=>{settled=true;return keys;});
  await Promise.resolve();
  assert.deepEqual(calls,[['audioFire','fire.ogg'],['audioInsertMagazine','in.ogg']]);
  assert.equal(settled,false,'weapons cannot become ready with a source clip still decoding');
  pending.get('audioFire')({});
  await Promise.resolve();
  assert.equal(settled,false);
  pending.get('audioInsertMagazine')({});
  assert.deepEqual(await ready,['audioFire','audioInsertMagazine']);
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

test('original M4 reload and bolt audio clips are packaged and preloaded by the live weapon adapter', async () => {
  const bytes = await readFile(new URL('../game/assets/weapon-runtime.json.gz', import.meta.url));
  const data = JSON.parse(gunzipSync(bytes));
  const expected = {
    audioExtractMagazine: 'Assets/AudioClip/magazine-out-02.wav',
    audioInsertMagazine: 'Assets/AudioClip/magazine-in-01.wav',
    audioBoltExtract: 'Assets/AudioClip/bolt-extract-01.wav',
    audioBoltReturn: 'Assets/AudioClip/bolt-return-02.wav',
  };
  for (const [key, source] of Object.entries(expected)) {
    assert.equal(data.audio[key]?.source, source, `${key} must retain its original serialized clip`);
    const path = new URL(`../game/${data.audio[key].url.slice(2)}`, import.meta.url);
    assert.ok((await readFile(path)).byteLength > 0, `${key} audio bytes must be in the Site package`);
  }
  const adapter = await readFile(new URL('../game/weapon-runtime.mjs', import.meta.url), 'utf8');
  assert.match(adapter, /preloadWeaponAudio\(data\.audio,data\.audioRuntime\)/);
  assert.match(adapter, /if\(e\.type==='audio'\)audio\.play\(e\.value/);
});
