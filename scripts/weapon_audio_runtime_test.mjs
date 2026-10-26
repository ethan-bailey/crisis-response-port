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

function methodBody(source, signature) {
  const start=source.indexOf(signature);
  assert.notEqual(start,-1,`source method missing: ${signature}`);
  const open=source.indexOf('{',start);
  assert.notEqual(open,-1,`method body missing: ${signature}`);
  let depth=0;
  for(let index=open;index<source.length;index++){
    if(source[index]==='{')depth++;
    else if(source[index]==='}'&&--depth===0)return source.slice(open,index+1);
  }
  assert.fail(`unterminated method body: ${signature}`);
}

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

test('Mission M4 ammunition has a complete original prefab dependency closure', async () => {
  const bytes = await readFile(new URL('../game/assets/weapon-runtime.json.gz', import.meta.url));
  const data = JSON.parse(gunzipSync(bytes));
  const ammo = data.ammoPackage;
  const audit = ammo.closureAudit;

  assert.equal(data.ammoPath, 'Assets/Resources/weapons/ammunition/5.56-FMJ.prefab');
  assert.equal(audit.complete, true);
  assert.deepEqual(audit.unresolved, []);
  assert.deepEqual([...audit.resolvedAssets].sort(), Object.keys(ammo.assets).sort());
  assert.equal(audit.resolvedAssets.length, 29, 'the known M4 FMJ graph has 29 packaged assets');

  const sourceEdges = {
    calibre: ['57cfc851e0e47374d98db0383b39cddf', 'Assets/Resources/weapons/calibers/Caliber 5.56x45mm.prefab'],
    projectile: ['cf5bca2bd1c0369439bb8fbe00c9320d', 'Assets/Resources/weapons/ammunition/projectiles/Projectile 5.56mm FMJ.prefab'],
    casing: ['9413de22ed594c44e8586fede9f5a355', 'Assets/Resources/weapons/ammunition/casings/Casing 5.56x45mm.prefab'],
    highPolyModel: ['1b3dc24381623284e82335a132ad5dad', 'Assets/Resources/weapons/ammunition/ui models/5.56x45mm FMJ.prefab'],
    sprite: ['8464cbc48daa246479808d64b86fb8c2', 'Assets/Sprite/Icon 556 FMJ.asset'],
  };
  for (const [field, [guid, sourcePath]] of Object.entries(sourceEdges)) {
    assert.equal(data.ammunition[field].guid, guid, `${field} keeps the source prefab identity`);
    assert.equal(ammo.guidPaths[guid], sourcePath, `${field} GUID resolves to its original source path`);
  }
  for (const sourcePath of Object.values(sourceEdges).map(([, sourcePath]) => sourcePath)) {
    assert.ok(ammo.assets[sourcePath], `transitive source dependency is packaged: ${sourcePath}`);
  }
});

test('the original firing callback chain emits source audio at the shot frame', async () => {
  const [weapon, host, queue, adapter] = await Promise.all([
    readFile(new URL('../runtime/Original/FullSource/WeaponBase.cs', import.meta.url), 'utf8'),
    readFile(new URL('../runtime/Engine/WeaponHost.cs', import.meta.url), 'utf8'),
    readFile(new URL('../runtime/Engine/BrowserAudioEventQueue.cs', import.meta.url), 'utf8'),
    readFile(new URL('../game/weapon-runtime.mjs', import.meta.url), 'utf8'),
  ]);
  const strike = methodBody(weapon, 'public void StrikeFiringPin()');
  const spawn = methodBody(weapon, 'private void SpawnProjectiles()');
  const emit = methodBody(weapon, 'private void EmitMuzzleAudio(bool superSonic = true)');

  assert.ok(strike.indexOf('audioFiringPin.Play') < strike.indexOf('DetonateRound()'),
    'the source firing-pin callback runs before detonation');
  assert.ok(spawn.indexOf('onSpawnProjectile.Invoke(projectile)') < spawn.indexOf('EmitMuzzleAudio(flag)'),
    'the source shot callback runs before its associated muzzle sound in the same tick');
  assert.ok(spawn.indexOf('EmitMuzzleAudio(flag)') < spawn.indexOf('LastWeaponFireTimestamp = Time.time'),
    'source fire timestamp is recorded only after the muzzle sound callback');
  assert.match(emit, /propagatingAudioSample\.PlayCustom\(Muzzle\.position\.x/);
  assert.match(emit, /audioSupersonic\.PlayCustom\(Muzzle\.position\.x/);

  assert.match(host, /weapon\.audioFiringPin=new\(effects,"audioFiringPin"\)/);
  assert.match(host, /weapon\.audioBoltExtract=new\(effects,"audioBoltExtract"\)/);
  assert.match(host, /weapon\.audioInsertMagazine=new\(effects,"audioInsertMagazine"\)/);
  assert.match(host, /weapon\.audioExtractMagazine=new\(effects,"audioExtractMagazine"\)/,
    'the source reload-removal event must be bound to its serialized extract clip');
  assert.match(host, /public void EmitMuzzleAudio\(bool supersonic\)\{Effects\.Emit\("audio","audioFire"\)/);
  assert.match(queue, /sourcePath = clip\.OriginalAssetPath/);
  assert.match(queue, /time = Time\.time/);
  assert.match(adapter, /const eventAge=sourceTime-Number\(e\.time\)/);
  assert.match(adapter, /audio\.play\(e\.value,\(data\.audio\[e\.value\]/);
});

test('original magazine reload, interrupt, and internal-magazine audio branches stay source-owned',async()=>{
  const weapon=await readFile(new URL('../runtime/Original/FullSource/WeaponBase.cs',import.meta.url),'utf8');
  const reload=await readFile(new URL('../runtime/Original/CharacterBehavior/CharacterBehavior/ChangeMagazine.cs',import.meta.url),'utf8');
  const insert=methodBody(weapon,'public bool InsertMagazine(Magazine magazine)');
  assert.ok(insert.indexOf('if (magazine == null)')<insert.indexOf('audioInsertMagazine.Play'),
    'null magazine insert exits before the original insert sound');
  assert.ok(insert.indexOf('onNewMagazine.Invoke()')<insert.indexOf('audioInsertMagazine.Play'),
    'the source magazine ownership event precedes its authored insert sound');

  const remove=methodBody(weapon,'public void RemoveMagazine(bool drop = false, bool audio = true)');
  assert.ok(remove.indexOf('if (fireFromMagazine)')<remove.indexOf('audioExtractMagazine.Play'),
    'internal magazine path selects its source extraction event');
  assert.ok(remove.indexOf('audioExtractMagazine.Play')<remove.indexOf('if (cyclingBolt.chamber.childCount <= 0)'),
    'fire-from-magazine source plays extraction before the empty-chamber early return');
  assert.match(remove,/if \(Magazine != null\)\s*\{\s*audioExtractMagazine\.Play/,
    'detachable magazine extraction sound is conditional on an owned magazine');
  assert.match(remove,/if \(!useInternalMagazine\)\s*\{\s*ComponentMagazine\.Drop\(\);\s*Magazine = null;/,
    'detachable magazine drop and ownership release remain source-controlled');

  const onUpdate=methodBody(reload,'public override TaskStatus OnUpdate()');
  assert.match(onUpdate,/self\.ActiveWeapon != weapon \|\| \(input\.Keys\.fire\.Down && state >= State\.State9\)/,
    'weapon switch and late reload fire interrupt the source task');
  assert.ok(onUpdate.indexOf('self.ActiveWeapon != weapon')<onUpdate.indexOf('magazineArm.SetOnWeapon'),
    'interrupt cleanup retargets the source arm without fabricating an audio event');
  const onEnd=methodBody(reload,'public override void OnEnd()');
  assert.match(onEnd,/self\.PendingAction = CharacterBody\.CharacterAction\.None;/);
  assert.match(onEnd,/self\.IsManipulatingEquipment = false;/);
  assert.doesNotMatch(onEnd,/audio(?:Extract|Insert)Magazine\.Play/,
    'cancelling the source task does not invent completion audio');
  assert.match(onUpdate,/weapon\.RemoveMagazine\(true\)/,'quick reload uses the authored removal path');
  assert.match(onUpdate,/self\.ActiveWeapon\.InsertMagazine\(fullestMagazineForWeapon\)/,'normal reload uses the authored insertion path');
  assert.match(onUpdate,/self\.ActiveWeapon\.ReturnMagazine\(\)/,'internal magazine closure uses its authored return event');
});
