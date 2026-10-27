import { rankSourceAudioVoices } from './source-audio-voice-arbitration.mjs';
import { createSourceAudioMixer } from './source-audio-mixer.mjs';

export const MAX_WEAPON_OUTPUT_GAIN = 0.16;
export const MAX_DECODED_SOURCE_AUDIO_BYTES = 48 * 1024 * 1024;
export const UNITY_REAL_VOICE_COUNT = 64;
export const UNITY_VIRTUAL_VOICE_COUNT = 512;

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

export function createSourceWeaponAudioDispatcher(audioData, audioRuntime, fetchOriginalSourceAudio, reportOriginalAudioState, reportPlaybackState) {
  const keys = new Map(Object.entries(audioData || {}).filter(([, info]) => info.source)
    .map(([key, info]) => [info.source, key]));
  return function dispatch(events, sourceTime) {
    for (const event of events || []) {
      const key = keys.get(event.sourcePath);
      if (key && event.action === 'oneShot') {
        const volume = (Number.isFinite(event.volume) ? event.volume : 0) * (key === 'audioFire' ? 0.6 : 1);
        if(audioRuntime.playMappedSourceEvent)audioRuntime.playMappedSourceEvent({...event,volume},sourceTime,key,reportPlaybackState);
        else audioRuntime.play(key, volume, sourceTime - Number(event.time));
        continue;
      }
      if (key && audioRuntime.playLoadedSourceEvent && event.action !== 'stop') {
        audioRuntime.playLoadedSourceEvent(event, sourceTime, key,reportPlaybackState);
        continue;
      }
      if (event?.action === 'stop' && audioRuntime.playOriginalSourceEvent) {
        audioRuntime.playOriginalSourceEvent(event, sourceTime,fetchOriginalSourceAudio,reportOriginalAudioState,reportPlaybackState);
        continue;
      }
      if (typeof event.sourcePath === 'string' && fetchOriginalSourceAudio && audioRuntime.playOriginalSourceEvent) {
        audioRuntime.playOriginalSourceEvent(event, sourceTime, fetchOriginalSourceAudio, reportOriginalAudioState,reportPlaybackState);
        continue;
      }
      audioRuntime.noteUnmappedSourceEvent();
    }
  };
}

export function createWeaponAudioRuntime(audioData, { AudioContextClass, fetchAudio,
  maxRealVoices=UNITY_REAL_VOICE_COUNT,maxVirtualVoices=UNITY_VIRTUAL_VOICE_COUNT } = {}) {
  if(!Number.isInteger(maxRealVoices)||maxRealVoices<0)throw new RangeError('Real voice limit must be a non-negative integer');
  if(!Number.isInteger(maxVirtualVoices)||maxVirtualVoices<0)throw new RangeError('Virtual voice limit must be a non-negative integer');
  const AudioContext = AudioContextClass ?? globalThis.AudioContext ?? globalThis.webkitAudioContext;
  const request = fetchAudio ?? globalThis.fetch?.bind(globalThis);
  const context = AudioContext ? new AudioContext() : null;
  const buffers = new Map();
  const loadingAudio = new Map();
  const decodedBytes = new Map(),lastUsed = new Map(),pendingSourceEvents=new Map(),activeSources=new Map(),sourceReporters=new Map(),activeVoiceRecords=new Map();
  let nextVoiceId=1,nextVoiceSequence=1;
  let latestSourceAudioStates = new Map(),latestSourceAudioStateTime=null,currentSourceAudioTime=0;
  const sourceMixer=createSourceAudioMixer();
  const stats = { played: 0, droppedSuspended: 0, droppedUnloaded: 0, droppedStale: 0,
    decodeErrors: 0, unlockFailures: 0, unmappedSourceEvents: 0,sourceDecodeErrors:0,
    sourceDecodeRequests:0,sourceDecodeCoalesced:0,sourceEventsDropped:0,sourceEventsPending:0,
    sourceStateFrames:0,sourceStateErrors:0,realVoices:0,virtualVoices:0,voicesRejected:0,
    voicesPromoted:0,voicesVirtualized:0,virtualNaturalEnds:0,
    sourceBytesDecoded:0,peakDecodedSourceBytes:0,sourceStops:0,sourceLoopEvents:0 };
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

  function audioBufferBytes(buffer) {
    const length=Number.isFinite(buffer?.length)?buffer.length:0,
      channels=Number.isFinite(buffer?.numberOfChannels)?buffer.numberOfChannels:0;
    // AudioBuffer.length is already sample frames; multiply by channels and
    // Float32 bytes/sample. Multiplying by sampleRate again overcounts by Hz.
    return Math.max(0,Math.ceil(length*channels*4));
  }

  function trimDecodedAudio() {
    let total=[...decodedBytes.values()].reduce((sum,value)=>sum+value,0);
    while(total>MAX_DECODED_SOURCE_AUDIO_BYTES) {
      const oldest=[...lastUsed.entries()].filter(([key])=>String(key).startsWith('source:'))
        .sort((a,b)=>a[1]-b[1])[0];
      if(!oldest)break;
      const [key]=oldest;total-=decodedBytes.get(key)||0;
      buffers.delete(key);decodedBytes.delete(key);lastUsed.delete(key);
      const path=String(key).startsWith('source:')?String(key).slice(7):null;
      if(path)notifySourceState(path,0,sourceReporters.get(path));
    }
  }

  function notifySourceState(path,state,report) {
    try { Promise.resolve(report?.(path,state)).catch(()=>{}); } catch { /* decoder state telemetry cannot block play */ }
  }

  function decodeSource(path,fetchSource,reportState) {
    const key=`source:${path}`;
    if(!context||typeof fetchSource!=='function')return Promise.resolve(null);
    if(buffers.has(key)){lastUsed.set(key,performanceNow());return Promise.resolve(buffers.get(key));}
    if(loadingAudio.has(key)){stats.sourceDecodeCoalesced++;return loadingAudio.get(key);}
    if(reportState)sourceReporters.set(path,reportState);
    stats.sourceDecodeRequests++;
    const promise=Promise.resolve().then(()=>fetchSource(path)).then(async payload=>{
      if(!payload||payload.assetPath!==path||typeof payload.dataBase64!=='string'||typeof payload.sourceSha256!=='string')
        throw new Error(`Verified source audio bridge returned invalid data for ${path}`);
      const binary=base64ToBytes(payload.dataBase64);
      const actual=await sha256Hex(binary);
      if(actual!==payload.sourceSha256)throw new Error(`Original source audio hash mismatch for ${path}`);
      const buffer=await context.decodeAudioData(binary.buffer.slice(binary.byteOffset,binary.byteOffset+binary.byteLength));
      const bytes=audioBufferBytes(buffer);
      if(bytes>MAX_DECODED_SOURCE_AUDIO_BYTES)throw new Error(`Decoded original source audio exceeds the ${MAX_DECODED_SOURCE_AUDIO_BYTES}-byte retained-cache limit: ${path}`);
      buffers.set(key,buffer);decodedBytes.set(key,bytes);
      lastUsed.set(key,performanceNow());stats.sourceBytesDecoded+=binary.byteLength;
      stats.peakDecodedSourceBytes=Math.max(stats.peakDecodedSourceBytes,[...decodedBytes.values()].reduce((a,b)=>a+b,0));
      notifySourceState(path,2,reportState);trimDecodedAudio();return buffer;
    }).catch(error=>{
      stats.sourceDecodeErrors++;notifySourceState(path,3,reportState);return null;
    }).finally(()=>loadingAudio.delete(key));
    loadingAudio.set(key,promise);return promise;
  }

  function reportSourcePlaying(report,sourceId){
    try{Promise.resolve(report?.(sourceId,[...activeVoiceRecords.values()].some(voice=>voice.sourceId===sourceId))).catch(()=>{});}catch{/* playback-state diagnostics cannot block audio */}
  }

  function voiceOffset(voice,now=context?.currentTime??0){
    if(voice.node&&voice.realSince!=null)return voice.offset+(now-voice.realSince)*voice.playbackRate;
    if(voice.virtualSince!=null)return voice.offset+(now-voice.virtualSince)*voice.playbackRate;
    return voice.offset;
  }

  function detachNode(voice,virtualizing=false){
    if(!voice.node)return;
    const source=voice.node,gain=voice.gainNode,now=context?.currentTime??0;voice.offset=voiceOffset(voice,now);voice.node=null;voice.gainNode=null;voice.realSince=null;
    voice.virtualSince=virtualizing?now:null;
    voice.suppressEnd=true;
    try{source.stop();}catch{}
    try{source.disconnect();gain?.disconnect();}catch{}
    const items=activeSources.get(voice.sourceId);items?.delete(source);if(items?.size===0)activeSources.delete(voice.sourceId);
    voice.suppressEnd=false;
    if(virtualizing)stats.voicesVirtualized++;
  }

  function finishVoice(voice,{virtualEnd=false}={}){
    if(!activeVoiceRecords.has(voice.id))return;
    detachNode(voice,false);activeVoiceRecords.delete(voice.id);
    if(virtualEnd)stats.virtualNaturalEnds++;
    reportSourcePlaying(voice.reportPlaybackState,voice.sourceId);
  }

  function currentVoicePriority(voice){
    const state=latestSourceAudioStates.get(voice.sourceId)||voice.sourceState;
    return Number.isInteger(state?.priority)&&state.priority>=0&&state.priority<=255?state.priority:voice.priority;
  }
  function currentVoiceGain(voice){
    const state=latestSourceAudioStates.get(voice.sourceId)||voice.sourceState;
    if(voice.event.muted===true||state?.muted===true||state?.enabled===false||state?.activeInHierarchy===false)return 0;
    // SpatialBlend and distance attenuation remain absent, but recovered source
    // group and snapshot gains are applied before ranking and physical output.
    const baseVolume=Number.isFinite(voice.sourceState?.volume)?voice.sourceState.volume:1;
    const eventScale=baseVolume>0?voice.eventVolume/baseVolume:voice.eventVolume;
    const sourceVolume=Number.isFinite(state?.volume)?state.volume:baseVolume;
    const group=sourceMixer.gainFor(state?.outputAudioMixerGroupGuid??voice.sourceState?.outputAudioMixerGroupGuid,
      state?.outputAudioMixerGroupFileID??voice.sourceState?.outputAudioMixerGroupFileID,currentSourceAudioTime);
    return Math.max(0,eventScale*sourceVolume*(group?.gain??1));
  }
  function currentVoiceAudibility(voice){
    return clampWeaponEventGain(currentVoiceGain(voice));
  }
  function currentVoicePitch(voice){
    const state=latestSourceAudioStates.get(voice.sourceId)||voice.sourceState;
    const sourcePitch=Number.isFinite(state?.pitch)?state.pitch:(Number.isFinite(voice.event.pitch)?voice.event.pitch:1);
    const group=sourceMixer.gainFor(state?.outputAudioMixerGroupGuid??voice.sourceState?.outputAudioMixerGroupGuid,
      state?.outputAudioMixerGroupFileID??voice.sourceState?.outputAudioMixerGroupFileID,currentSourceAudioTime);
    return Math.max(.01,Math.min(3,Math.abs(sourcePitch)*(group?.pitch??1)));
  }

  function startVoice(voice){
    if(!context||context.state!=='running'||voice.node)return false;
    const promotedFromVirtual=voice.virtualSince!=null;
    const offset=voiceOffset(voice),duration=Number.isFinite(voice.buffer?.duration)?voice.buffer.duration:
      (Number.isFinite(voice.buffer?.length)&&Number.isFinite(voice.buffer?.sampleRate)&&voice.buffer.sampleRate>0?voice.buffer.length/voice.buffer.sampleRate:Infinity);
    if(!voice.loop&&offset>=duration){finishVoice(voice,{virtualEnd:true});return false;}
    const source=context.createBufferSource(),gain=context.createGain();source.buffer=voice.buffer;
    voice.playbackRate=currentVoicePitch(voice);
    source.loop=voice.loop;source.playbackRate.value=voice.playbackRate;
    gain.gain.value=currentVoiceGain(voice);source.connect(gain).connect(outputGain);
    voice.node=source;voice.gainNode=gain;voice.realSince=context.currentTime??0;voice.virtualSince=null;
    const cleanup=()=>{
      if(voice.suppressEnd||voice.node!==source)return;
      voice.offset=voiceOffset(voice);voice.node=null;voice.gainNode=null;voice.realSince=null;
      try{source.disconnect();gain.disconnect();}catch{}
      const items=activeSources.get(voice.sourceId);items?.delete(source);if(items?.size===0)activeSources.delete(voice.sourceId);
      finishVoice(voice,{virtualEnd:false});
      rebalanceVoices();
    };
    source.onended=cleanup;
    if(!activeSources.has(voice.sourceId))activeSources.set(voice.sourceId,new Set());
    activeSources.get(voice.sourceId).add(source);
    try{source.start(0,offset);}catch{detachNode(voice);return false;}
    if(promotedFromVirtual)stats.voicesPromoted++;
    return true;
  }

  function rebalanceVoices(){
    const now=context?.currentTime??0;
    for(const voice of [...activeVoiceRecords.values()]){
      const duration=Number.isFinite(voice.buffer?.duration)?voice.buffer.duration:
        (Number.isFinite(voice.buffer?.length)&&Number.isFinite(voice.buffer?.sampleRate)&&voice.buffer.sampleRate>0?voice.buffer.length/voice.buffer.sampleRate:Infinity);
      if(!voice.loop&&voiceOffset(voice,now)>=duration){finishVoice(voice,{virtualEnd:!voice.node});}
    }
    const candidates=[...activeVoiceRecords.values()].map(voice=>({voice,priority:currentVoicePriority(voice),
      audibility:currentVoiceAudibility(voice),sequence:voice.sequence}));
    const ranked=rankSourceAudioVoices(candidates),real=new Set(ranked.slice(0,maxRealVoices).map(item=>item.voice.id));
    const retained=new Set(ranked.slice(0,maxRealVoices+maxVirtualVoices).map(item=>item.voice.id));
    for(const voice of [...activeVoiceRecords.values()])if(!retained.has(voice.id)){
      stats.voicesRejected++;stats.sourceEventsDropped++;finishVoice(voice);
    }
    for(const voice of activeVoiceRecords.values()){
      if(real.has(voice.id)){
        if(!voice.node)startVoice(voice);
        if(voice.node&&voice.gainNode){
          const offset=voiceOffset(voice),now=context?.currentTime??0;
          voice.offset=offset;voice.realSince=now;voice.playbackRate=currentVoicePitch(voice);
          voice.node.playbackRate.value=voice.playbackRate;
          voice.gainNode.gain.value=currentVoiceGain(voice);
        }
      }else if(voice.node)detachNode(voice,true);
      else {
        if(voice.virtualSince==null)voice.virtualSince=now;
        const offset=voiceOffset(voice);voice.offset=offset;voice.virtualSince=now;voice.playbackRate=currentVoicePitch(voice);
      }
    }
    stats.realVoices=[...activeVoiceRecords.values()].filter(voice=>!!voice.node).length;
    stats.virtualVoices=activeVoiceRecords.size-stats.realVoices;
  }

  function playDecodedSource(event,sourceTime,keyOverride,reportPlaybackState) {
    const key=keyOverride||`source:${event.sourcePath}`,buffer=buffers.get(key),id=Number.isInteger(event.sourceId)?event.sourceId:0;
    const eventAge=sourceTime-Number(event.time);
    const disposition=classifyWeaponPlayback({eventAgeSeconds:eventAge,contextState:context?.state??'unavailable',bufferLoaded:!!buffer});
    if(disposition==='stale'){stats.droppedStale++;stats.sourceEventsDropped++;reportSourcePlaying(reportPlaybackState,id);return disposition;}
    if(disposition==='suspended'){stats.droppedSuspended++;stats.sourceEventsDropped++;reportSourcePlaying(reportPlaybackState,id);return disposition;}
    if(disposition==='unloaded'){stats.droppedUnloaded++;stats.sourceEventsDropped++;reportSourcePlaying(reportPlaybackState,id);return disposition;}
    if(event.action==='play')for(const prior of [...activeVoiceRecords.values()])
      if(prior.sourceId===id&&prior.kind==='play')finishVoice(prior);
    const sourceState=event.sourceState||latestSourceAudioStates.get(id)||null;
    const voice={id:nextVoiceId++,sourceId:id,key,buffer,event,eventVolume:clampWeaponEventGain(event.volume),
      sourceState,priority:Number.isInteger(sourceState?.priority)&&sourceState.priority>=0&&sourceState.priority<=255?sourceState.priority:
        (Number.isInteger(event.priority)&&event.priority>=0&&event.priority<=255?event.priority:128),
      sequence:nextVoiceSequence++,kind:event.action==='play'?'play':'oneShot',loop:event.action==='play'&&event.loop===true,
      playbackRate:Number.isFinite(event.pitch)?Math.max(.01,Math.min(3,Math.abs(event.pitch))):1,offset:0,realSince:null,virtualSince:null,node:null,gainNode:null,
      reportPlaybackState};
    activeVoiceRecords.set(voice.id,voice);
    if(voice.loop)stats.sourceLoopEvents++;
    lastUsed.set(key,performanceNow());stats.played++;
    rebalanceVoices();reportSourcePlaying(reportPlaybackState,id);return 'play';
  }

  function stopSource(sourceId,reportPlaybackState) {
    const id=Number.isInteger(sourceId)?sourceId:0;
    const cancelled=[];
    for(const [path,events] of pendingSourceEvents){
      const kept=events.filter(item=>id!==0&&item.event.sourceId!==id);
      const removed=events.length-kept.length;
      if(removed){stats.sourceEventsPending=Math.max(0,stats.sourceEventsPending-removed);stats.sourceEventsDropped+=removed;cancelled.push(...events.filter(item=>id===0||item.event.sourceId===id).map(item=>item.event.sourceId).filter(Number.isInteger));}
      if(kept.length)pendingSourceEvents.set(path,kept);else pendingSourceEvents.delete(path);
    }
    const keys=id===0?[...new Set([...activeSources.keys(),...[...activeVoiceRecords.values()].map(voice=>voice.sourceId)])]:[id];
    for(const voice of [...activeVoiceRecords.values()])if(id===0||voice.sourceId===id)finishVoice(voice);
    for(const sourceId of new Set([...keys,...cancelled]))reportSourcePlaying(reportPlaybackState,sourceId);
    stats.sourceStops++;
  }
  function playLoadedSourceEvent(event,sourceTime,key,reportPlaybackState){return playDecodedSource(event,sourceTime,key,reportPlaybackState);}
  function playMappedSourceEvent(event,sourceTime,key,reportPlaybackState){return playDecodedSource(event,sourceTime,key,reportPlaybackState);}

  function playOriginalSourceEvent(event,sourceTime,fetchSource,reportState,reportPlaybackState) {
    if(event?.action==='stop'){stopSource(event.sourceId,reportPlaybackState);return 'stop';}
    if(!event||!['oneShot','play'].includes(event.action)||typeof event.sourcePath!=='string'||
      !/^Assets\/AudioClip\/.+\.(wav|ogg)$/i.test(event.sourcePath)){stats.unmappedSourceEvents++;return 'unmapped';}
    const key=`source:${event.sourcePath}`;
    if(buffers.has(key))return playDecodedSource(event,sourceTime,key,reportPlaybackState);
    const eventAge=sourceTime-Number(event.time);
    if(!Number.isFinite(eventAge)||eventAge<-.05||eventAge>.12){stats.droppedStale++;stats.sourceEventsDropped++;}
    else {
      const pending=pendingSourceEvents.get(event.sourcePath)||[];
      if(pending.length>=16){stats.sourceEventsDropped++;stats.droppedUnloaded++;}
      else {
        pending.push({event:{...event},sourceTime,startedAt:performanceNow(),reportState});
        pendingSourceEvents.set(event.sourcePath,pending);stats.sourceEventsPending++;
      }
    }
    decodeSource(event.sourcePath,fetchSource,reportState).then(buffer=>{
      const queued=pendingSourceEvents.get(event.sourcePath)||[];pendingSourceEvents.delete(event.sourcePath);
      for(const item of queued){
        stats.sourceEventsPending=Math.max(0,stats.sourceEventsPending-1);
        const elapsed=(performanceNow()-item.startedAt)/1000;
        if(buffer&&item.sourceTime-Number(item.event.time)+elapsed<=.12)
          playDecodedSource(item.event,item.sourceTime+elapsed,key,reportPlaybackState);
        else {stats.sourceEventsDropped++;stats.droppedStale++;reportSourcePlaying(reportPlaybackState,item.event.sourceId);}
      }
    });
    return 'loading';
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
      loadedBuffers: buffers.size, pendingBuffers: loadingAudio.size,
      decodedAudioBytes:[...decodedBytes.values()].reduce((a,b)=>a+b,0),maxDecodedAudioBytes:MAX_DECODED_SOURCE_AUDIO_BYTES,
      sourceStateFrames:stats.sourceStateFrames,sourceStateErrors:stats.sourceStateErrors,
      activeSourceStateCount:latestSourceAudioStates.size,sourceStateTime:latestSourceAudioStateTime,
      mixer:sourceMixer.snapshot(currentSourceAudioTime),
      realVoices:stats.realVoices,virtualVoices:stats.virtualVoices,maxRealVoices,maxVirtualVoices };
  }

  function noteUnmappedSourceEvent() { stats.unmappedSourceEvents++; }

  function updateSourceAudioStates(states,sourceTime){
    if(!Array.isArray(states)||!Number.isFinite(sourceTime)){stats.sourceStateErrors++;return false;}
    const next=new Map();
    for(const state of states){
      const position=state?.sourceTransform?.position;
      const motion=state?.sourceTransform?.motion;
      const motionVelocity=motion?.velocity;
      const motionAngularVelocity=motion?.angularVelocity;
      const validMotion=motion==null||
        ((motion.kind==='rigidbody'||motion.kind==='projectileFlight')&&
         Array.isArray(motionVelocity)&&motionVelocity.length===3&&motionVelocity.every(Number.isFinite)&&
         (motionAngularVelocity==null||(Array.isArray(motionAngularVelocity)&&motionAngularVelocity.length===3&&motionAngularVelocity.every(Number.isFinite))));
      if(!Number.isInteger(state?.sourceId)||!Number.isInteger(state?.priority)||state.priority<0||state.priority>255||
        !Array.isArray(position)||position.length!==3||position.some(value=>!Number.isFinite(value))||!validMotion){stats.sourceStateErrors++;return false;}
      next.set(state.sourceId,state);
    }
    latestSourceAudioStates=next;latestSourceAudioStateTime=sourceTime;currentSourceAudioTime=sourceTime;stats.sourceStateFrames++;rebalanceVoices();return true;
  }

  function updateSourceAudioMixer(configuration,events,sourceTime){
    if(!Number.isFinite(sourceTime)){stats.sourceStateErrors++;return false;}
    sourceMixer.update(configuration,events,sourceTime);currentSourceAudioTime=sourceTime;rebalanceVoices();return true;
  }

  return { context, load, play, snapshot, unlock, noteUnmappedSourceEvent,
    loadOriginalSource:(path,fetchSource,reportState)=>decodeSource(path,fetchSource,reportState),
    preloadOriginalSources:async(paths,fetchSource,reportState,{concurrency=3}={})=>{
      const queue=[...new Set(paths||[])],decoded=[],failed=[];
      const workers=Array.from({length:Math.max(1,Math.min(8,concurrency))},async()=>{
        while(queue.length){const path=queue.shift();const buffer=await decodeSource(path,fetchSource,reportState);
          (buffer?decoded:failed).push(path);}
      });
      await Promise.all(workers);
      const decodedPaths=[...new Set(decoded)],resident=decodedPaths.filter(path=>buffers.has(`source:${path}`));
      const residentSet=new Set(resident);
      return {loaded:resident,decoded:decodedPaths,evicted:decodedPaths.filter(path=>!residentSet.has(path)),failed:[...new Set(failed)],...snapshot()};
    },
    playOriginalSourceEvent,playLoadedSourceEvent,playMappedSourceEvent,stopSource,updateSourceAudioStates,updateSourceAudioMixer };
}

function performanceNow(){return globalThis.performance?.now?.()??Date.now();}
function base64ToBytes(value){
  const binary=globalThis.atob(value),bytes=new Uint8Array(binary.length);
  for(let i=0;i<binary.length;i++)bytes[i]=binary.charCodeAt(i);
  return bytes;
}
async function sha256Hex(bytes){
  if(!globalThis.crypto?.subtle)throw new Error('WebCrypto is required to verify original source audio bytes');
  const digest=await globalThis.crypto.subtle.digest('SHA-256',bytes);
  return [...new Uint8Array(digest)].map(value=>value.toString(16).padStart(2,'0')).join('');
}
