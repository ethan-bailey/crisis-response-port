export const MAX_WEAPON_OUTPUT_GAIN = 0.16;
export const MAX_DECODED_SOURCE_AUDIO_BYTES = 48 * 1024 * 1024;

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

export function createWeaponAudioRuntime(audioData, { AudioContextClass, fetchAudio } = {}) {
  const AudioContext = AudioContextClass ?? globalThis.AudioContext ?? globalThis.webkitAudioContext;
  const request = fetchAudio ?? globalThis.fetch?.bind(globalThis);
  const context = AudioContext ? new AudioContext() : null;
  const buffers = new Map();
  const loadingAudio = new Map();
  const decodedBytes = new Map(),lastUsed = new Map(),pendingSourceEvents=new Map(),activeSources=new Map(),activeSourceKinds=new WeakMap(),sourceReporters=new Map();
  let latestSourceAudioStates = new Map(),latestSourceAudioStateTime=null;
  const stats = { played: 0, droppedSuspended: 0, droppedUnloaded: 0, droppedStale: 0,
    decodeErrors: 0, unlockFailures: 0, unmappedSourceEvents: 0,sourceDecodeErrors:0,
    sourceDecodeRequests:0,sourceDecodeCoalesced:0,sourceEventsDropped:0,sourceEventsPending:0,
    sourceStateFrames:0,sourceStateErrors:0,
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
    try{Promise.resolve(report?.(sourceId,(activeSources.get(sourceId)?.size||0)>0)).catch(()=>{});}catch{/* playback-state diagnostics cannot block audio */}
  }

  function playDecodedSource(event,sourceTime,keyOverride,reportPlaybackState) {
    const key=keyOverride||`source:${event.sourcePath}`,buffer=buffers.get(key),id=Number.isInteger(event.sourceId)?event.sourceId:0;
    const eventAge=sourceTime-Number(event.time);
    const disposition=classifyWeaponPlayback({eventAgeSeconds:eventAge,contextState:context?.state??'unavailable',bufferLoaded:!!buffer});
    if(disposition==='stale'){stats.droppedStale++;stats.sourceEventsDropped++;reportSourcePlaying(reportPlaybackState,id);return disposition;}
    if(disposition==='suspended'){stats.droppedSuspended++;stats.sourceEventsDropped++;reportSourcePlaying(reportPlaybackState,id);return disposition;}
    if(disposition==='unloaded'){stats.droppedUnloaded++;stats.sourceEventsDropped++;reportSourcePlaying(reportPlaybackState,id);return disposition;}
    const source=context.createBufferSource(),gain=context.createGain();source.buffer=buffer;
    source.loop=event.action==='play'&&event.loop===true;
    source.playbackRate.value=Number.isFinite(event.pitch)?Math.max(0.01,Math.min(3,Math.abs(event.pitch))):1;
    gain.gain.value=event.muted===true?0:clampWeaponEventGain(event.volume);
    source.connect(gain).connect(outputGain);
    const activeKey=id;
    const cleanup=()=>{source.disconnect();gain.disconnect();const items=activeSources.get(activeKey);items?.delete(source);if(items?.size===0)activeSources.delete(activeKey);reportSourcePlaying(reportPlaybackState,activeKey);};
    source.onended=cleanup;
    if(event.action==='play')for(const prior of activeSources.get(activeKey)||[]){
      // A regular Play replaces/restarts this AudioSource's assigned clip,
      // while its PlayOneShot voices remain independent overlapping voices.
      if(activeSourceKinds.get(prior)==='play')try{prior.stop();}catch{}
    }
    if(!activeSources.has(activeKey))activeSources.set(activeKey,new Set());
    activeSourceKinds.set(source,event.action==='play'?'play':'oneShot');
    activeSources.get(activeKey).add(source);source.start();reportSourcePlaying(reportPlaybackState,id);stats.played++;
    if(source.loop)stats.sourceLoopEvents++;
    lastUsed.set(key,performanceNow());return 'play';
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
    const keys=id===0?[...activeSources.keys()]:[id];
    for(const key of keys)for(const source of activeSources.get(key)||[]){try{source.stop();}catch{}}
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
      activeSourceStateCount:latestSourceAudioStates.size,sourceStateTime:latestSourceAudioStateTime };
  }

  function noteUnmappedSourceEvent() { stats.unmappedSourceEvents++; }

  function updateSourceAudioStates(states,sourceTime){
    if(!Array.isArray(states)||!Number.isFinite(sourceTime)){stats.sourceStateErrors++;return false;}
    const next=new Map();
    for(const state of states){
      const position=state?.sourceTransform?.position;
      if(!Number.isInteger(state?.sourceId)||!Number.isInteger(state?.priority)||state.priority<0||state.priority>255||
        !Array.isArray(position)||position.length!==3||position.some(value=>!Number.isFinite(value))){stats.sourceStateErrors++;return false;}
      next.set(state.sourceId,state);
    }
    latestSourceAudioStates=next;latestSourceAudioStateTime=sourceTime;stats.sourceStateFrames++;return true;
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
    playOriginalSourceEvent,playLoadedSourceEvent,playMappedSourceEvent,stopSource,updateSourceAudioStates };
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
