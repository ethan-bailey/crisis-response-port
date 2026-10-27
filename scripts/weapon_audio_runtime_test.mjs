import test from 'node:test';
import assert from 'node:assert/strict';
import { webcrypto } from 'node:crypto';
import { readFile } from 'node:fs/promises';
import { gunzipSync } from 'node:zlib';
import {
  MAX_WEAPON_OUTPUT_GAIN,
  MAX_DECODED_SOURCE_AUDIO_BYTES,
  UNITY_REAL_VOICE_COUNT,
  UNITY_VIRTUAL_VOICE_COUNT,
  classifyWeaponPlayback,
  clampWeaponEventGain,
  createSourceWeaponAudioDispatcher,
  createWeaponAudioRuntime,
  preloadWeaponAudio,
} from '../game/weapon-audio-runtime.mjs';
import { planSourceAudioVoices, rankSourceAudioVoices } from '../game/source-audio-voice-arbitration.mjs';

test('Unity 2018.4 audio arbitration ranks priority first, then audibility, with a stable tie order', () => {
  const candidates = [
    { id:'quiet-important', priority:5, audibility:.01, sequence:4 },
    { id:'loud-ordinary', priority:40, audibility:1, sequence:1 },
    { id:'quiet-ordinary', priority:40, audibility:.1, sequence:2 },
    { id:'same-audibility-first', priority:128, audibility:.5, sequence:3 },
    { id:'same-audibility-second', priority:128, audibility:.5, sequence:5 },
  ];
  assert.deepEqual(rankSourceAudioVoices(candidates).map(item=>item.id), [
    'quiet-important','loud-ordinary','quiet-ordinary','same-audibility-first','same-audibility-second',
  ]);
  const allocation=planSourceAudioVoices(candidates,2);
  assert.deepEqual(allocation.real.map(item=>item.id),['quiet-important','loud-ordinary']);
  assert.deepEqual(allocation.virtual.map(item=>item.id),['quiet-ordinary','same-audibility-first','same-audibility-second']);
});

test('active source snapshots reach the audio runtime each frame and reject malformed updates atomically', async () => {
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext});
  const state={sourceId:19,priority:5,sourceTransform:{position:[1,2,3]}};
  assert.equal(runtime.updateSourceAudioStates([state],3.5),true);
  assert.equal(runtime.snapshot().sourceStateFrames,1);
  assert.equal(runtime.snapshot().sourceStateErrors,0);
  assert.equal(runtime.snapshot().activeSourceStateCount,1);
  assert.equal(runtime.snapshot().sourceStateTime,3.5);
  assert.equal(runtime.updateSourceAudioStates([{...state,sourceTransform:{position:[1,2]}}],4),false);
  assert.equal(runtime.snapshot().activeSourceStateCount,1,'malformed frame must not replace the last complete snapshot');
  assert.equal(runtime.snapshot().sourceStateErrors,1);
  assert.equal(runtime.updateSourceAudioStates([],4.5),true);
  assert.equal(runtime.snapshot().activeSourceStateCount,0,'an empty active set clears stale source poses');
});

test('full-source frame snapshots are forwarded through the production game audio adapter', async () => {
  const [page,presentation,weapon]=await Promise.all([
    readFile(new URL('../game/index.html',import.meta.url),'utf8'),
    readFile(new URL('../game/presentation.mjs',import.meta.url),'utf8'),
    readFile(new URL('../game/weapon-runtime.mjs',import.meta.url),'utf8'),
  ]);
  assert.match(page,/updateSourceAudioStates\(sourceActorSnapshot\.audioSourceStates,sourceActorSnapshot\.sourceTime\)/);
  assert.match(page,/updateSourceAudioMixer\(sourceActorSnapshot\.audioMixerConfiguration,sourceActorSnapshot\.audioMixerEvents,sourceActorSnapshot\.audioMixerTime\)/);
  const startupSnapshot=page.indexOf('sourceActorSnapshot=session.snapshot;');
  const startupMixer=page.indexOf('upperBody.updateSourceAudioMixer(sourceActorSnapshot.audioMixerConfiguration,sourceActorSnapshot.audioMixerEvents,sourceActorSnapshot.audioMixerTime);',startupSnapshot);
  const sourceModeEnabled=page.indexOf('sourceActorMode=true;',startupSnapshot);
  assert.ok(startupSnapshot>=0&&startupMixer>startupSnapshot&&sourceModeEnabled>startupMixer,
    'consume the one-time mixer graph and startup transitions before enabling the Mission frame loop');
  assert.match(presentation,/updateSourceAudioStates\(states,sourceTime\)\{effects\.updateSourceAudioStates\(states,sourceTime\);\}/);
  assert.match(presentation,/updateSourceAudioMixer\(configuration,events,sourceTime\)\{effects\.updateSourceAudioMixer\(configuration,events,sourceTime\);\}/);
  assert.match(weapon,/updateSourceAudioStates:\(states,sourceTime\)=>audioStats\.updateSourceAudioStates\(states,sourceTime\)/);
  assert.match(weapon,/updateSourceAudioMixer:\(configuration,events,sourceTime\)=>audioStats\.updateSourceAudioMixer\(configuration,events,sourceTime\)/);
});

test('voice arbitration selects exactly 64 real voices from a 513-candidate deterministic profile', () => {
  const candidates=Array.from({length:513},(_,index)=>({id:index,priority:index%256,
    audibility:(index%101)/100,sequence:index}));
  const allocation=planSourceAudioVoices(candidates,64);
  const ranked=rankSourceAudioVoices(candidates);
  assert.equal(allocation.real.length,64);
  assert.equal(allocation.virtual.length,449);
  assert.ok(allocation.real.every((voice,index)=>voice===ranked[index]));
  assert.equal(allocation.real[0].priority,0);
});

test('source voice ranking rejects invalid priority and incomplete audibility estimates', () => {
  assert.throws(()=>rankSourceAudioVoices([{priority:256,audibility:1}]),/priority/);
  assert.throws(()=>rankSourceAudioVoices([{priority:1,audibility:Number.NaN}]),/audibility/);
  assert.throws(()=>planSourceAudioVoices([], -1),/Real voice count/);
});

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
  constructor() { this.gain = { value: 1 }; this.playbackRate={value:1};this.stopped=0; }
  connect(next) { return next; }
  disconnect() {}
  stop() { if(this.stopped)return;this.stopped++;this.onended?.(); }
  end() { this.onended?.(); }
}

let activeContext;
class FakeAudioContext {
  constructor() {
    activeContext = this;
    this.state = 'suspended';
    this.currentTime = 0;
    this.destination = {};
    this.started = 0;
    this.master = null;
    this.sources=[];
    this.gains=[];
  }
  createGain() {
    const node = new FakeNode();
    if (!this.master) this.master = node;
    this.gains.push(node);
    return node;
  }
  createDynamicsCompressor() {
    return Object.assign(new FakeNode(), Object.fromEntries(
      ['threshold', 'knee', 'ratio', 'attack', 'release'].map(k => [k, { value: 0 }])));
  }
  createBufferSource() {
    const source = new FakeNode();
    source.start = (when=0,offset=0) => { source.startOffset=offset;this.started++; };
    this.sources.push(source);
    return source;
  }
  decodeAudioData() { return Promise.resolve({ decoded: true,length:this.decodedBufferLength??4800,numberOfChannels:1,sampleRate:48000 }); }
  resume() { this.state = 'running'; return Promise.resolve(); }
}

test('source audio applies authored mixer group gain and snapshot pitch to active output',async()=>{
  const sourcePath='Assets/AudioClip/mixer-route.ogg',bytes=Uint8Array.from([1,2,7]);
  const sourceSha256=[...new Uint8Array(await webcrypto.subtle.digest('SHA-256',bytes))].map(x=>x.toString(16).padStart(2,'0')).join('');
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext});runtime.unlock();await Promise.resolve();
  const fetchSource=async path=>({assetPath:path,dataBase64:Buffer.from(bytes).toString('base64'),sourceSha256});
  await runtime.loadOriginalSource(sourcePath,fetchSource);
  const mixerGuid='mixer-guid';
  const configuration={mixers:[{guid:mixerGuid,fields:{m_StartSnapshot:{fileID:'24500001'},m_TargetSnapshot:{fileID:'24500001'}}}],
    groups:[{guid:mixerGuid,fileID:'24300000',fields:{m_GroupID:'master',m_Volume:'master-volume',m_Pitch:'master-pitch',m_Children:[{fileID:'24300004'}],m_Mute:0}},
      {guid:mixerGuid,fileID:'24300004',fields:{m_GroupID:'effects',m_Volume:'effects-volume',m_Pitch:'effects-pitch',m_Children:[],m_Mute:0}}],
    snapshots:[{guid:mixerGuid,fileID:'24500001',fields:{m_FloatValues:{'master-volume':0,'effects-volume':0,'master-pitch':1,'effects-pitch':1}}},
      {guid:mixerGuid,fileID:'24500002',fields:{m_FloatValues:{'master-volume':-6,'effects-volume':-6,'master-pitch':2,'effects-pitch':2}}}]};
  const transition={action:'transition',time:1,duration:1,snapshotFileID:'24500002',snapshotFields:configuration.snapshots[1].fields};
  assert.equal(runtime.updateSourceAudioMixer(configuration,[transition],1),true);
  const sourceState={sourceId:991,priority:80,volume:.8,pitch:1,enabled:true,activeInHierarchy:true,muted:false,
    outputAudioMixerGroupGuid:mixerGuid,outputAudioMixerGroupFileID:'24300004',sourceTransform:{position:[0,0,0]}};
  const dispatch=createSourceWeaponAudioDispatcher({},runtime,fetchSource);
  dispatch([{action:'oneShot',sourcePath,sourceId:991,time:1.01,volume:.8,pitch:1,sourceState}],1.02);
  assert.equal(runtime.updateSourceAudioStates([sourceState],1.5),true);
  const outputGain=activeContext.gains.at(-1);
  assert.ok(Math.abs(outputGain.gain.value-.8*10**(-6/20))<1e-9,`expected interpolated group/master gain, got ${outputGain.gain.value}`);
  assert.equal(activeContext.sources.at(-1).playbackRate.value,2.25,'parent and child snapshot pitch multiply at the active buffer source');
  runtime.updateSourceAudioMixer(null,[],2);
  assert.ok(Math.abs(outputGain.gain.value-.8*10**(-12/20))<1e-9);
});

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
    audioExtractMagazine: { source: 'Assets/AudioClip/magazine-out-02.wav' },
    audioInsertMagazine: { source: 'Assets/AudioClip/magazine-in-01.wav' },
  }, {
    play: (...args) => played.push(args),
    noteUnmappedSourceEvent: () => unmapped.push(true),
  });
  dispatcher([
    { action: 'oneShot', sourcePath: 'Assets/AudioClip/5.56x45mm-01-unsuppressed.wav', volume: 0.5, time: 10.04 },
    { action: 'oneShot', sourcePath: 'Assets/AudioClip/5.56x45mm-crack-interior.wav', volume: 0.8, time: 10.08 },
    { action: 'play', sourcePath: 'Assets/Audio/M4-pin.wav', volume: 0.8, time: 10.05 },
    { action: 'oneShot', sourcePath: 'Assets/AudioClip/magazine-out-02.wav', volume: 0.7, time: 10.07 },
    { action: 'oneShot', sourcePath: 'Assets/AudioClip/magazine-in-01.wav', volume: 0.6, time: 10.09 },
    { action: 'oneShot', sourcePath: 'Assets/Audio/not-mapped.wav', volume: 1, time: 10.06 },
  ], 10.1);
  assert.equal(played.length, 4);
  assert.equal(played[0][0], 'audioFire');
  assert.ok(Math.abs(played[0][1] - 0.3) < 1e-12);
  assert.ok(Math.abs(played[0][2] - 0.06) < 1e-12);
  assert.equal(played[1][0], 'audioSupersonic');
  assert.ok(Math.abs(played[1][1] - 0.8) < 1e-12);
  assert.ok(Math.abs(played[1][2] - 0.02) < 1e-12);
  assert.equal(played[2][0], 'audioExtractMagazine');
  assert.ok(Math.abs(played[2][1] - 0.7) < 1e-12);
  assert.ok(Math.abs(played[2][2] - 0.03) < 1e-12);
  assert.equal(played[3][0], 'audioInsertMagazine');
  assert.ok(Math.abs(played[3][1] - 0.6) < 1e-12);
  assert.ok(Math.abs(played[3][2] - 0.01) < 1e-12);
  assert.equal(unmapped.length, 2, 'unsupported loops and unknown source clips are counted rather than guessed');
});

test('dynamic Mission source WAV events coalesce verified decode and preserve per-source stop state', async () => {
  const bytes=Uint8Array.from([1,2,3,4]),sourcePath='Assets/AudioClip/hurt-07.ogg';
  const sourceSha256=[...new Uint8Array(await webcrypto.subtle.digest('SHA-256',bytes))].map(x=>x.toString(16).padStart(2,'0')).join('');
  const encoded=Buffer.from(bytes).toString('base64');let fetches=0;const states=[],sourcePlaying=[],played=[],unmapped=[];
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext});runtime.unlock();await Promise.resolve();
  const fetchSource=async path=>{assert.equal(path,sourcePath);fetches++;return {assetPath:path,dataBase64:encoded,sourceSha256};};
  const report=(path,state)=>states.push([path,state]);
  const dispatch=createSourceWeaponAudioDispatcher({},runtime,fetchSource,report,(id,isPlaying)=>sourcePlaying.push([id,isPlaying]));
  dispatch([
    {action:'oneShot',sourcePath,sourceId:22,time:4,volume:.7},
    {action:'oneShot',sourcePath,sourceId:22,time:4.01,volume:.5},
  ],4.02);
  await runtime.loadOriginalSource(sourcePath,fetchSource,report);
  assert.equal(fetches,1,'same source path shares the in-flight verified package read and decode');
  assert.equal(runtime.snapshot().sourceDecodeCoalesced,2);
  assert.equal(runtime.snapshot().played,2,JSON.stringify(runtime.snapshot()));
  assert.deepEqual(states,[[sourcePath,2]],'Loaded is reported only after source hash verification and decode');
  dispatch([{action:'play',sourcePath,sourceId:22,time:4.03,volume:.4,pitch:.8,loop:true}],4.04);
  assert.equal(runtime.snapshot().sourceLoopEvents,1);
  dispatch([{action:'stop',sourceId:22,time:4.05}],4.06);
  assert.equal(runtime.snapshot().sourceStops,1);
  assert.deepEqual(sourcePlaying.at(-1),[22,false],'source-scoped Stop reports the ended state to FullSource');
  assert.equal(runtime.snapshot().decodedAudioBytes,19200);
  assert.equal(runtime.snapshot().maxDecodedAudioBytes,MAX_DECODED_SOURCE_AUDIO_BYTES);
  assert.equal(runtime.snapshot().unmappedSourceEvents,0);
  assert.equal(runtime.snapshot().sourceEventsDropped,0);
  assert.equal(unmapped.length,0);
});

test('production voice allocation enforces 64 real and 512 virtual slots using source priority and known gain', async () => {
  const bytes=Uint8Array.from([2,4,6]),sourcePath='Assets/AudioClip/voice-pool.ogg';
  const sourceSha256=[...new Uint8Array(await webcrypto.subtle.digest('SHA-256',bytes))].map(x=>x.toString(16).padStart(2,'0')).join('');
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext,maxRealVoices:1,maxVirtualVoices:2});runtime.unlock();await Promise.resolve();
  const fetchSource=async path=>({assetPath:path,dataBase64:Buffer.from(bytes).toString('base64'),sourceSha256});
  await runtime.loadOriginalSource(sourcePath,fetchSource);
  const dispatch=createSourceWeaponAudioDispatcher({},runtime,fetchSource);
  const events=[
    {sourceId:10,priority:40,volume:.2},
    {sourceId:11,priority:5,volume:.1},
    {sourceId:12,priority:40,volume:1},
    {sourceId:13,priority:150,volume:.8},
  ].map((item,index)=>({action:'oneShot',sourcePath,time:3+index*.001,...item,
    sourceState:{sourceId:item.sourceId,priority:item.priority,volume:item.volume,enabled:true,activeInHierarchy:true,muted:false}}));
  dispatch(events,3.01);
  const state=runtime.snapshot();
  assert.equal(state.realVoices,1);
  assert.equal(state.virtualVoices,2);
  assert.equal(state.maxRealVoices,1);
  assert.equal(state.maxVirtualVoices,2);
  assert.equal(state.voicesRejected,1);
  assert.equal(activeContext.started,2,'the first physical voice is demoted when the highest priority source arrives');
  assert.ok(activeContext.sources.at(-1).stopped===0,'highest-priority source owns the physical voice');
  assert.equal(runtime.updateSourceAudioStates([
    {sourceId:10,priority:255,volume:.2,enabled:true,activeInHierarchy:true,muted:false,sourceTransform:{position:[0,0,0]}},
    {sourceId:11,priority:254,volume:.1,enabled:true,activeInHierarchy:true,muted:false,sourceTransform:{position:[0,0,0]}},
    {sourceId:12,priority:0,volume:1,enabled:true,activeInHierarchy:true,muted:false,sourceTransform:{position:[0,0,0]}},
  ],3.1),true);
  assert.equal(runtime.snapshot().voicesPromoted,1,'updated source priorities trigger a real-slot promotion');
  assert.equal(activeContext.started,3);
  assert.equal(runtime.snapshot().realVoices,1);
});

test('production allocator applies recovered 64-real and 512-virtual defaults at the boundary', async () => {
  const bytes=Uint8Array.from([1,3,5]),sourcePath='Assets/AudioClip/default-voice-pool.ogg';
  const sourceSha256=[...new Uint8Array(await webcrypto.subtle.digest('SHA-256',bytes))].map(x=>x.toString(16).padStart(2,'0')).join('');
  assert.equal(UNITY_REAL_VOICE_COUNT,64);
  assert.equal(UNITY_VIRTUAL_VOICE_COUNT,512);
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext});runtime.unlock();await Promise.resolve();
  const fetchSource=async path=>({assetPath:path,dataBase64:Buffer.from(bytes).toString('base64'),sourceSha256});
  await runtime.loadOriginalSource(sourcePath,fetchSource);
  const dispatch=createSourceWeaponAudioDispatcher({},runtime,fetchSource);
  const candidates=Array.from({length:UNITY_REAL_VOICE_COUNT+UNITY_VIRTUAL_VOICE_COUNT+1},(_,index)=>({
    action:'oneShot',sourcePath,sourceId:1000+index,time:1+index*.00001,volume:.5,priority:128,
  }));
  dispatch(candidates,1.02);
  const snapshot=runtime.snapshot();
  assert.equal(snapshot.realVoices,64);
  assert.equal(snapshot.virtualVoices,512);
  assert.equal(snapshot.voicesRejected,1);
  assert.equal(snapshot.sourceEventsDropped,1);
  assert.equal(activeContext.started,64);
});

test('virtual source playback preserves offset when promoted and ends naturally while virtual', async () => {
  const bytes=Uint8Array.from([7,8,9]),sourcePath='Assets/AudioClip/virtual.ogg';
  const sourceSha256=[...new Uint8Array(await webcrypto.subtle.digest('SHA-256',bytes))].map(x=>x.toString(16).padStart(2,'0')).join('');
  const fetchSource=async path=>({assetPath:path,dataBase64:Buffer.from(bytes).toString('base64'),sourceSha256});
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext,maxRealVoices:1,maxVirtualVoices:1});runtime.unlock();await Promise.resolve();
  await runtime.loadOriginalSource(sourcePath,fetchSource);
  const playing=[],dispatch=createSourceWeaponAudioDispatcher({},runtime,fetchSource,null,(id,isPlaying)=>playing.push([id,isPlaying]));
  const sourceState=(sourceId,priority)=>({sourceId,priority,volume:.8,enabled:true,activeInHierarchy:true,muted:false});
  dispatch([{action:'oneShot',sourcePath,sourceId:21,time:5,volume:.8,sourceState:sourceState(21,100)}],5.01);
  const first=activeContext.sources.at(-1);
  activeContext.currentTime=.04;
  dispatch([{action:'oneShot',sourcePath,sourceId:22,time:5.02,volume:.8,sourceState:sourceState(22,5)}],5.03);
  const higher=activeContext.sources.at(-1);
  assert.equal(first.stopped,1,'lower-ranked active node is virtualized');
  assert.equal(runtime.snapshot().virtualVoices,1);
  higher.end();
  const promoted=activeContext.sources.at(-1);
  assert.notEqual(promoted,first,'promotion creates a new physical node for the surviving logical voice');
  assert.ok(Math.abs(promoted.startOffset-.04)<1e-9,'virtual playback position is retained during promotion');
  assert.equal(runtime.snapshot().realVoices,1);

  const natural=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext,maxRealVoices:0,maxVirtualVoices:1});natural.unlock();await Promise.resolve();
  await natural.loadOriginalSource(sourcePath,fetchSource);
  const naturalStates=[],naturalDispatch=createSourceWeaponAudioDispatcher({},natural,fetchSource,null,(id,isPlaying)=>naturalStates.push([id,isPlaying]));
  naturalDispatch([{action:'oneShot',sourcePath,sourceId:23,time:8,volume:.5,sourceState:sourceState(23,50)}],8.01);
  assert.equal(natural.snapshot().virtualVoices,1);
  activeContext.currentTime=.11;
  natural.updateSourceAudioStates([],8.12);
  assert.equal(natural.snapshot().virtualVoices,0,'a virtual non-looping voice expires at its decoded duration');
  assert.equal(natural.snapshot().virtualNaturalEnds,1);
  assert.deepEqual(naturalStates.at(-1),[23,false]);
  assert.deepEqual(playing.at(-1),[22,false]);
});

test('PlayOneShot overlaps regular Play and one-shots; regular Play replaces only the prior regular voice', async () => {
  const bytes=Uint8Array.from([1,4,9]),sourcePath='Assets/AudioClip/hurt-07.ogg';
  const sourceSha256=[...new Uint8Array(await webcrypto.subtle.digest('SHA-256',bytes))].map(x=>x.toString(16).padStart(2,'0')).join('');
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext});runtime.unlock();await Promise.resolve();
  const fetchSource=async path=>({assetPath:path,dataBase64:Buffer.from(bytes).toString('base64'),sourceSha256});
  await runtime.loadOriginalSource(sourcePath,fetchSource);
  const states=[],dispatch=createSourceWeaponAudioDispatcher({},runtime,fetchSource,null,(id,isPlaying)=>states.push([id,isPlaying]));
  dispatch([{action:'play',sourcePath,sourceId:42,time:2,volume:.8,loop:true}],2.01);
  const firstPlay=activeContext.sources.at(-1);
  dispatch([{action:'oneShot',sourcePath,sourceId:42,time:2.02,volume:.6}],2.03);
  const oneShot=activeContext.sources.at(-1);
  assert.equal(firstPlay.stopped,0,'PlayOneShot must not interrupt the assigned Play voice');
  oneShot.end();
  assert.deepEqual(states.at(-1),[42,true],'natural one-shot completion leaves the assigned Play active');
  dispatch([{action:'play',sourcePath,sourceId:42,time:2.04,volume:.5,loop:true}],2.05);
  const replacementPlay=activeContext.sources.at(-1);
  assert.equal(firstPlay.stopped,1,'a new Play restarts/replaces an earlier assigned-clip Play');
  assert.equal(replacementPlay.stopped,0);
  assert.deepEqual(states.at(-1),[42,true]);
  replacementPlay.end();
  assert.deepEqual(states.at(-1),[42,false],'natural completion clears the final active voice');
  assert.equal(runtime.snapshot().played,3);
});

test('overlapping PlayOneShot voices keep isPlaying true until the last natural end', async () => {
  const bytes=Uint8Array.from([2,5,8]),sourcePath='Assets/AudioClip/impact.ogg';
  const sourceSha256=[...new Uint8Array(await webcrypto.subtle.digest('SHA-256',bytes))].map(x=>x.toString(16).padStart(2,'0')).join('');
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext});runtime.unlock();await Promise.resolve();
  const fetchSource=async path=>({assetPath:path,dataBase64:Buffer.from(bytes).toString('base64'),sourceSha256});
  await runtime.loadOriginalSource(sourcePath,fetchSource);
  const states=[],dispatch=createSourceWeaponAudioDispatcher({},runtime,fetchSource,null,(id,isPlaying)=>states.push([id,isPlaying]));
  dispatch([
    {action:'oneShot',sourcePath,sourceId:8,time:7,volume:.4},
    {action:'oneShot',sourcePath,sourceId:8,time:7.01,volume:.5},
  ],7.02);
  const [first,second]=activeContext.sources.slice(-2);
  assert.deepEqual(states.at(-1),[8,true]);
  first.end();
  assert.deepEqual(states.at(-1),[8,true]);
  second.end();
  assert.deepEqual(states.at(-1),[8,false]);
});

test('source decode queue accepts 16 same-clip events and records later events as dropped', async () => {
  const bytes=Uint8Array.from([3,6,9]),sourcePath='Assets/AudioClip/burst.ogg';
  const sourceSha256=[...new Uint8Array(await webcrypto.subtle.digest('SHA-256',bytes))].map(x=>x.toString(16).padStart(2,'0')).join('');
  let finishFetch;
  const fetchSource=path=>new Promise(resolve=>{finishFetch=()=>resolve({assetPath:path,dataBase64:Buffer.from(bytes).toString('base64'),sourceSha256});});
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext});runtime.unlock();await Promise.resolve();
  const dispatch=createSourceWeaponAudioDispatcher({},runtime,fetchSource);
  dispatch(Array.from({length:17},(_,index)=>({action:'oneShot',sourcePath,sourceId:99,time:10+index*.001,volume:.5})),10.02);
  await Promise.resolve();
  assert.equal(runtime.snapshot().sourceEventsPending,16);
  assert.equal(runtime.snapshot().sourceEventsDropped,1);
  assert.equal(runtime.snapshot().droppedUnloaded,1);
  finishFetch();
  await runtime.loadOriginalSource(sourcePath,fetchSource);
  await Promise.resolve();
  await Promise.resolve();
  assert.equal(runtime.snapshot().sourceEventsPending,0);
  assert.equal(runtime.snapshot().played,16);
  assert.equal(runtime.snapshot().sourceEventsDropped,1);
});

test('source decode failure and stale event are recorded without late playback', async () => {
  const sourcePath='Assets/AudioClip/hurt-07.ogg',states=[];
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext});runtime.unlock();await Promise.resolve();
  const fetchBad=async path=>({assetPath:path,dataBase64:Buffer.from([1,2]).toString('base64'),sourceSha256:'0'.repeat(64)});
  const report=(path,state)=>states.push([path,state]);
  const dispatch=createSourceWeaponAudioDispatcher({},runtime,fetchBad,report);
  dispatch([{action:'oneShot',sourcePath,sourceId:1,time:1,volume:1}],1.2);
  await runtime.loadOriginalSource(sourcePath,fetchBad,report);
  assert.equal(runtime.snapshot().sourceDecodeErrors,1);
  assert.equal(runtime.snapshot().sourceEventsDropped,1);
  assert.deepEqual(states,[[sourcePath,3]]);
  const staleRuntime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext});staleRuntime.unlock();await Promise.resolve();
  let fetches=0;const fetchStale=async()=>{fetches++;throw new Error('should still be caught');},stale=createSourceWeaponAudioDispatcher({},staleRuntime,fetchStale);
  stale([{action:'oneShot',sourcePath,sourceId:2,time:2,volume:1}],2.2);
  await staleRuntime.loadOriginalSource(sourcePath,fetchStale);
  assert.equal(staleRuntime.snapshot().droppedStale,1);
  assert.equal(staleRuntime.snapshot().played,0);
  assert.equal(fetches,1,'stale events still warm the clip cache but cannot play late');
});

test('AudioSource.Stop cancels source events that are waiting for original decode', async () => {
  const bytes=Uint8Array.from([5,6,7]),sourcePath='Assets/AudioClip/hurt-07.ogg';
  const sourceSha256=[...new Uint8Array(await webcrypto.subtle.digest('SHA-256',bytes))].map(x=>x.toString(16).padStart(2,'0')).join('');
  let finishFetch;const states=[];
  const runtime=createWeaponAudioRuntime({}, {AudioContextClass:FakeAudioContext});runtime.unlock();await Promise.resolve();
  const fetchSource=()=>new Promise(resolve=>{finishFetch=()=>resolve({assetPath:sourcePath,dataBase64:Buffer.from(bytes).toString('base64'),sourceSha256});});
  const dispatch=createSourceWeaponAudioDispatcher({},runtime,fetchSource,null,(id,isPlaying)=>states.push([id,isPlaying]));
  dispatch([{action:'oneShot',sourcePath,sourceId:77,time:9,volume:1}],9.01);
  await Promise.resolve();
  dispatch([{action:'stop',sourceId:77,time:9.02}],9.02);
  finishFetch();
  await runtime.loadOriginalSource(sourcePath,fetchSource);
  assert.equal(runtime.snapshot().played,0,'pending one-shot must not resurrect after source Stop');
  assert.equal(runtime.snapshot().sourceEventsPending,0);
  assert.equal(runtime.snapshot().sourceEventsDropped,1);
  assert.deepEqual(states.at(-1),[77,false]);
});

test('a single decoded source larger than the retained-memory ceiling fails explicitly', async () => {
  class OversizedAudioContext extends FakeAudioContext { constructor(){super();this.decodedBufferLength=Math.floor(MAX_DECODED_SOURCE_AUDIO_BYTES/4)+1;} }
  const bytes=Uint8Array.of(8),sourcePath='Assets/AudioClip/large.wav';
  const sourceSha256=[...new Uint8Array(await webcrypto.subtle.digest('SHA-256',bytes))].map(x=>x.toString(16).padStart(2,'0')).join('');
  const states=[],runtime=createWeaponAudioRuntime({}, {AudioContextClass:OversizedAudioContext});
  const result=await runtime.loadOriginalSource(sourcePath,async()=>({assetPath:sourcePath,dataBase64:Buffer.from(bytes).toString('base64'),sourceSha256}),
    (path,state)=>states.push([path,state]));
  assert.equal(result,null);
  assert.equal(runtime.snapshot().sourceDecodeErrors,1);
  assert.equal(runtime.snapshot().decodedAudioBytes,0);
  assert.deepEqual(states,[[sourcePath,3]]);
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
