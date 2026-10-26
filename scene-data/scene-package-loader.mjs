export async function loadOriginalScenePackage() {
  const base = new URL("./", import.meta.url);
  const manifestResponse = await fetch(new URL("crisis-response-scenes.manifest.json", base));
  if (!manifestResponse.ok) {
    throw new Error("Original scene package manifest unavailable: HTTP " + manifestResponse.status);
  }
  const manifest = await manifestResponse.json();
  if (manifest.schemaVersion !== 1 || manifest.encoding !== "independent-gzip-chunks"
      || !Array.isArray(manifest.chunks) || manifest.chunks.length === 0) {
    throw new Error("Original scene package manifest is invalid");
  }
  if (typeof DecompressionStream === "undefined") {
    throw new Error("Browser cannot decompress the original scene package");
  }

  const parts = [];
  let byteLength = 0;
  for (const chunk of manifest.chunks) {
    if (typeof chunk.path !== "string" || !/^crisis-response-scenes-\d{3}\.json\.gz$/.test(chunk.path)) {
      throw new Error("Original scene package manifest contains an invalid chunk path");
    }
    const response = await fetch(new URL(chunk.path, base));
    if (!response.ok) throw new Error("Original scene package chunk unavailable: HTTP " + response.status);
    const bytes = await new Response(response.body.pipeThrough(new DecompressionStream("gzip"))).arrayBuffer();
    if (bytes.byteLength !== chunk.bytes) throw new Error("Original scene package chunk size mismatch: " + chunk.path);
    parts.push(new Uint8Array(bytes));
    byteLength += bytes.byteLength;
  }
  if (byteLength !== manifest.uncompressedBytes) throw new Error("Original scene package size mismatch");

  const joined = new Uint8Array(byteLength);
  let offset = 0;
  for (const part of parts) {
    joined.set(part, offset);
    offset += part.byteLength;
  }
  return new TextDecoder("utf-8", { fatal: true }).decode(joined);
}

export async function loadFullSourceOperatorPackage() {
  const base = new URL("./fullsource/", import.meta.url);
  const manifestResponse = await fetch(new URL("operator-prefab.manifest.json", base));
  if (!manifestResponse.ok) throw new Error("FullSource Operator manifest unavailable: HTTP " + manifestResponse.status);
  const manifest = await manifestResponse.json();
  if (manifest.schemaVersion !== 1 || manifest.profile !== "operator-prefab"
      || manifest.encoding !== "gzip-json" || manifest.rootAssets?.[0] !== "Assets/Resources/characters/Operator.prefab"
      || !Number.isSafeInteger(manifest.compressedBytes) || manifest.compressedBytes > 12 * 1024 * 1024
      || !Number.isSafeInteger(manifest.uncompressedBytes) || manifest.uncompressedBytes > 32 * 1024 * 1024
      || !/^[0-9a-f]{64}$/.test(manifest.packageSha256 ?? "")) {
    throw new Error("FullSource Operator manifest is invalid or exceeds the delivery budget");
  }
  if (typeof DecompressionStream === "undefined") throw new Error("Browser cannot decompress the FullSource Operator package");
  const response = await fetch(new URL("operator-prefab.json.gz", base));
  if (!response.ok) throw new Error("FullSource Operator package unavailable: HTTP " + response.status);
  const compressed = await response.arrayBuffer();
  if (compressed.byteLength !== manifest.compressedBytes) throw new Error("FullSource Operator package size mismatch");
  const digest = new Uint8Array(await crypto.subtle.digest("SHA-256", compressed));
  const actualHash = Array.from(digest, value => value.toString(16).padStart(2, "0")).join("");
  if (actualHash !== manifest.packageSha256) throw new Error("FullSource Operator package checksum mismatch");
  const raw = await new Response(new Blob([compressed]).stream().pipeThrough(new DecompressionStream("gzip"))).arrayBuffer();
  if (raw.byteLength !== manifest.uncompressedBytes) throw new Error("FullSource Operator source size mismatch");
  const source = new TextDecoder("utf-8", { fatal: true }).decode(raw);
  const packageData = JSON.parse(source);
  if (!packageData.assets?.[manifest.rootAssets[0]] || !Array.isArray(packageData.objectReferences)) {
    throw new Error("FullSource Operator package is missing its original root or dependency references");
  }
  return source;
}

async function sha256(bytes) {
  const digest = new Uint8Array(await crypto.subtle.digest("SHA-256", bytes));
  return Array.from(digest, value => value.toString(16).padStart(2, "0")).join("");
}

function throwIfAborted(signal) {
  if (signal?.aborted) throw new DOMException("Mission source download cancelled", "AbortError");
}

function monotonicNow() {
  return globalThis.performance?.now?.() ?? Date.now();
}

async function getStorageStatus() {
  const storage = globalThis.navigator?.storage;
  let persistent = null;
  let estimate = null;
  try { if (typeof storage?.persist === "function") persistent = await storage.persist(); }
  catch { /* Persistent storage remains optional; report browser-managed retention. */ }
  try {
    if (typeof storage?.estimate === "function") {
      const result = await storage.estimate();
      const usage = Number.isFinite(result?.usage) && result.usage >= 0 ? result.usage : null;
      const quota = Number.isFinite(result?.quota) && result.quota >= 0 ? result.quota : null;
      estimate = {usage, quota, available:usage !== null && quota !== null ? Math.max(0, quota - usage) : null};
    }
  } catch { /* Unsupported and denied estimates do not prevent verified delivery. */ }
  return {persistent, estimate};
}

async function fetchGzipJson(url, expected, label, signal) {
  const response = await fetch(url, {signal});
  if (!response.ok) throw new Error(`${label} unavailable: HTTP ${response.status}`);
  const compressed = await response.arrayBuffer();
  if (compressed.byteLength !== expected.compressedBytes || await sha256(compressed) !== expected.compressedSha256)
    throw new Error(`${label} compressed size or checksum mismatch`);
  if (typeof DecompressionStream === "undefined") throw new Error("Browser cannot decompress incremental source assets");
  const raw = await new Response(new Blob([compressed]).stream().pipeThrough(new DecompressionStream("gzip"))).arrayBuffer();
  if (raw.byteLength !== expected.bytes || await sha256(raw) !== expected.sha256)
    throw new Error(`${label} source size or checksum mismatch`);
  return { compressed, raw, text: new TextDecoder("utf-8", { fatal: true }).decode(raw) };
}

function openMissionAssetDatabase() {
  return new Promise((resolve,reject)=>{
    if(!globalThis.indexedDB?.open){reject(new Error("IndexedDB unavailable"));return;}
    const request=globalThis.indexedDB.open("crisis-response-mission-assets",1);
    request.onupgradeneeded=()=>{
      const database=request.result;
      if(!database.objectStoreNames.contains("assets"))database.createObjectStore("assets",{keyPath:"key"});
    };
    request.onsuccess=()=>{request.result.onversionchange=()=>request.result.close();resolve(request.result);};
    request.onerror=()=>reject(request.error||new Error("IndexedDB open failed"));
    request.onblocked=()=>reject(new Error("IndexedDB upgrade is blocked by another open page"));
  });
}

function makeIndexedDbAssetStore(database,sourceHash) {
  const prefix=`${sourceHash}:`;
  const keyFor=url=>prefix+String(url);
  const transaction=(mode,action)=>new Promise((resolve,reject)=>{
    let result;
    let tx;
    try{tx=database.transaction("assets",mode);result=action(tx.objectStore("assets"));}
    catch(error){reject(error);return;}
    tx.oncomplete=()=>resolve(result&&"result" in result?result.result:result);
    tx.onerror=()=>reject(tx.error||result?.error||new Error("IndexedDB transaction failed"));
    tx.onabort=()=>reject(tx.error||new Error("IndexedDB transaction aborted"));
  });
  return {
    backend:"IndexedDB",
    match:async url=>{
      const record=await transaction("readonly",store=>store.get(keyFor(url)));
      return record?.compressed instanceof ArrayBuffer?new Response(record.compressed,{headers:{"Content-Type":"application/gzip"}}):null;
    },
    put:async(url,response)=>{
      const compressed=await response.arrayBuffer();
      await transaction("readwrite",store=>store.put({key:keyFor(url),compressed}));
    },
    delete:async url=>transaction("readwrite",store=>store.delete(keyFor(url))),
    clear:async()=>{
      const state={deleted:0};
      await transaction("readwrite",store=>{
        const request=store.openCursor();
        request.onsuccess=()=>{const cursor=request.result;if(!cursor)return;if(typeof cursor.key==="string"&&cursor.key.startsWith(prefix)){cursor.delete();state.deleted++;}cursor.continue();};
        return state;
      });
      return state.deleted>0;
    },
    close:()=>database.close()
  };
}

async function openMissionAssetStore(sourceHash) {
  if(globalThis.isSecureContext&&globalThis.indexedDB?.open){
    try{return makeIndexedDbAssetStore(await openMissionAssetDatabase(),sourceHash);}
    catch(error){
      if(!(globalThis.caches?.open))return {backend:"unavailable",error,match:async()=>null,put:async()=>{throw error;},delete:async()=>false,clear:async()=>false};
    }
  }
  if(globalThis.isSecureContext&&globalThis.caches?.open){
    try{
      const cache=await globalThis.caches.open(`crisis-source-${sourceHash}`);
      return {backend:"CacheStorage",match:url=>cache.match(url),put:(url,response)=>cache.put(url,response),delete:url=>cache.delete(url),clear:()=>globalThis.caches.delete(`crisis-source-${sourceHash}`)};
    }catch(error){return {backend:"unavailable",error,match:async()=>null,put:async()=>{throw error;},delete:async()=>false,clear:async()=>false};}
  }
  return {backend:"unavailable",match:async()=>null,put:async()=>{throw new Error("Persistent browser storage is unavailable");},delete:async()=>false,clear:async()=>false};
}

async function fetchCompressedAsset(url, expected, label, signal, persistentCache) {
  const started = monotonicNow();
  let response = null;
  try { response = persistentCache ? await persistentCache.match(url) : null; }
  catch { /* A damaged local store is treated as a miss; network bytes are verified below. */ }
  let wasCached = !!response;
  let compressed = null;
  if (response) {
    try {
      compressed = await response.arrayBuffer();
      if (compressed.byteLength !== expected.compressedBytes || await sha256(compressed) !== expected.compressedSha256)
        throw new Error("Cached source asset failed its manifest integrity check");
    } catch {
      // A bad or unreadable cached chunk must not poison every later launch.
      // Drop it and fetch this one immutable asset again from the verified source.
      try { await persistentCache.delete?.(url); } catch { /* Network recovery is still attempted. */ }
      response = null;
      compressed = null;
      wasCached = false;
    }
  }
  if (!response) {
    response = await fetch(url, {signal});
    if (!response.ok) throw new Error(`${label} unavailable: HTTP ${response.status}`);
    compressed = await response.arrayBuffer();
    if (compressed.byteLength !== expected.compressedBytes || await sha256(compressed) !== expected.compressedSha256)
      throw new Error(`${label} compressed size or checksum mismatch`);
  }
  let cachedLocally = wasCached;
  let cacheWriteError = null;
  if (!wasCached && persistentCache) {
    try {
      await persistentCache.put(url, new Response(compressed.slice(0), {headers:{'Content-Type':'application/gzip'}}));
      cachedLocally = true;
    } catch (error) {
      // Storage is optional for this launch, but the caller must be able to
      // distinguish quota exhaustion from an unavailable/private cache.
      cacheWriteError = {name:error?.name || "Error", message:String(error?.message || error)};
    }
  }
  return {compressed, fetchAndHashMs: monotonicNow() - started, wasCached, cachedLocally, cacheWriteError};
}

// Blazor's byte[] JS interop path is version-sensitive and the hosted .NET 8
// runtime rejects Uint8Array arguments as an unexpected JSON byte array.
// .NET 8 expects byte[] arguments as base64 text. Encoding the bounded batch
// also avoids retaining base64 copies for the entire source profile at once.
function bytesToBase64(bytes) {
  let binary = "";
  const chunkSize = 0x8000;
  for (let offset = 0; offset < bytes.length; offset += chunkSize)
    binary += String.fromCharCode(...bytes.subarray(offset, offset + chunkSize));
  return btoa(binary);
}

/** Delete only the immutable source cache selected by the current manifest. */
export async function clearIncrementalSourceCache(manifestUrl) {
  if (!globalThis.isSecureContext || (!globalThis.indexedDB?.open&&!globalThis.caches?.delete))
    return {cleared:false,available:false};
  const response=await fetch(manifestUrl);
  if(!response.ok)throw new Error(`Incremental source manifest unavailable: HTTP ${response.status}`);
  const manifest=await response.json();
  if(manifest.schemaVersion!==1||manifest.encoding!=="gzip-json-assets"
      ||! /^[0-9a-f]{64}$/.test(manifest.sourcePackageSha256??""))
    throw new Error("Incremental source profile manifest is invalid");
  const cacheName=`crisis-source-${manifest.sourcePackageSha256}`;
  let cleared=false;
  if(globalThis.indexedDB?.open){
    try{const store=makeIndexedDbAssetStore(await openMissionAssetDatabase(),manifest.sourcePackageSha256);cleared=await store.clear();store.close();}
    catch(error){if(!globalThis.caches?.delete)throw error;}
  }
  if(globalThis.caches?.delete)cleared=(await globalThis.caches.delete(cacheName))||cleared;
  return {cleared,available:true};
}

/** Fetch a small bounded batch in parallel, then deliver verified assets in manifest order. */
export async function deliverIncrementalSourceProfile(bridge, manifestUrl, {onProgress,signal} = {}) {
  const profileStarted = monotonicNow();
  if (!bridge || typeof bridge.invokeMethodAsync !== "function") throw new TypeError("A Blazor DotNet bridge is required");
  throwIfAborted(signal);
  const manifestResponse = await fetch(manifestUrl, {signal});
  if (!manifestResponse.ok) throw new Error(`Incremental source manifest unavailable: HTTP ${manifestResponse.status}`);
  const manifest = await manifestResponse.json();
  if (manifest.schemaVersion !== 1 || manifest.encoding !== "gzip-json-assets"
      || !Array.isArray(manifest.rootAssets) || !manifest.rootAssets.length
      || !Array.isArray(manifest.assets) || manifest.assets.length !== manifest.assetCount
      || !Number.isSafeInteger(manifest.assetCount) || manifest.assetCount < 1
      || !manifest.metadata || manifest.metadata.file !== "metadata.json.gz"
      || !/^[0-9a-f]{64}$/.test(manifest.sourcePackageSha256 ?? ""))
    throw new Error("Incremental source profile manifest is invalid");

  // Store only immutable compressed assets in a namespace bound to the source
  // profile hash; every hit still passes the manifest size and SHA.
  const persistentCache=await openMissionAssetStore(manifest.sourcePackageSha256);
  // Storage persistence and quota estimates are advisory. A browser can still
  // evict local data, so verified network delivery never depends on either.
  const storageStatus = await getStorageStatus();
  if (onProgress) await onProgress({stage:"storage",backend:persistentCache.backend,...storageStatus});

  const base = new URL("./", new URL(manifestUrl, import.meta.url));
  const metadata = await fetchGzipJson(new URL(manifest.metadata.file, base), manifest.metadata, "Incremental source metadata",signal);
  const metadataObject = JSON.parse(metadata.text);
  if (!metadataObject.assetManifest || Object.keys(metadataObject.assetManifest).length !== manifest.assets.length)
    throw new Error("Incremental source metadata and delivery manifest disagree");
  const assetPaths = new Set(manifest.assets.map(asset => asset.path));
  if (manifest.rootAssets.some(path => !assetPaths.has(path)))
    throw new Error("Incremental source profile omits a declared root asset");
  const initialized = JSON.parse(await bridge.invokeMethodAsync("InitializeIncrementalSourcePackage", metadata.text));
  throwIfAborted(signal);
  if (initialized.expectedAssets !== manifest.assetCount || initialized.receivedAssets !== 0 || initialized.receivedBytes !== 0)
    throw new Error("WASM source store initialized with unexpected manifest counts");
  let compressedBytes = metadata.compressed.byteLength;
  let sourceBytes = metadata.raw.byteLength;
  let compressedAssetBytes = 0;
  let sourceAssetBytes = 0;
  let maximumAssetBytes = 0;
  let deliveredAssetBytes = 0;
  const slowestFetches = [];
  const slowestBridgeCalls = [];
  const rememberSlowest = (items, sample) => {
    items.push(sample);
    items.sort((a, b) => b.durationMs - a.durationMs);
    if (items.length > 10) items.length = 10;
  };
  const seenPaths = new Set();
  const downloadConcurrency = 6;
  // The canonical manifest is path-sorted, which leaves multi-megabyte assets
  // at the end of delivery. Fetch large verified assets first so the load does
  // not appear stalled near completion. Ties remain deterministic by path.
  const deliveryAssets=[...manifest.assets].sort((a,b)=>b.compressedBytes-a.compressedBytes||a.path.localeCompare(b.path));
  for (let batchStart = 0; batchStart < deliveryAssets.length; batchStart += downloadConcurrency) {
    const batchStarted = monotonicNow();
    const batch = deliveryAssets.slice(batchStart, batchStart + downloadConcurrency);
    for (const asset of batch) {
      if (typeof asset.path !== "string" || !/^assets\/[0-9a-f]{24}\.json\.gz$/.test(asset.file)
          || seenPaths.has(asset.path) || !/^[0-9a-f]{64}$/.test(asset.sha256 ?? "")
          || !/^[0-9a-f]{64}$/.test(asset.compressedSha256 ?? "")
          || !Number.isSafeInteger(asset.bytes) || asset.bytes < 0
          || !metadataObject.assetManifest[asset.path]
          || metadataObject.assetManifest[asset.path].bytes !== asset.bytes
          || metadataObject.assetManifest[asset.path].sha256 !== asset.sha256
          || metadataObject.assetManifest[asset.path].compressedBytes !== asset.compressedBytes
          || metadataObject.assetManifest[asset.path].compressedSha256 !== asset.compressedSha256
          || !Number.isSafeInteger(asset.compressedBytes) || asset.compressedBytes > 25 * 1024 * 1024)
        throw new Error("Incremental source profile contains an invalid asset entry");
      seenPaths.add(asset.path);
    }
    throwIfAborted(signal);
    if (onProgress) await onProgress({stage:"fetching",receivedAssets:batchStart,
      expectedAssets:manifest.assets.length,compressedAssetBytes,sourceAssetBytes,
      totalCompressedAssetBytes:manifest.compressedAssetBytes,totalSourceAssetBytes:manifest.uncompressedAssetBytes,
      pendingAssetPaths:batch.map(asset=>asset.path),profileElapsedMs:monotonicNow()-profileStarted});
    const fetchedBatch = await Promise.all(batch.map(asset =>
      fetchCompressedAsset(new URL(asset.file, base), asset, `Source asset ${asset.path}`,signal,persistentCache)));
    // Keep each interop payload small while amortizing Blazor/.NET dispatch over
    // several files. A single large file is still transported by itself.
    let deliveryOffset=0;
    while(deliveryOffset<batch.length){
      throwIfAborted(signal);
      const deliveryStart=deliveryOffset;
      let deliveryBytes=0;
      while(deliveryOffset<batch.length&&deliveryOffset-deliveryStart<16){
        const nextBytes=fetchedBatch[deliveryOffset].compressed.byteLength;
        if(deliveryOffset>deliveryStart&&deliveryBytes+nextBytes>1024*1024)break;
        deliveryBytes+=nextBytes;deliveryOffset++;
      }
      const assets=batch.slice(deliveryStart,deliveryOffset);
      const fetched=fetchedBatch.slice(deliveryStart,deliveryOffset);
      const prepareStarted=monotonicNow();
      const compressedLengths=fetched.map(item=>item.compressed.byteLength);
      const compressedBatch=new Uint8Array(deliveryBytes);
      let compressedOffset=0;
      for(const item of fetched){
        compressedBatch.set(new Uint8Array(item.compressed),compressedOffset);
        compressedOffset+=item.compressed.byteLength;
      }
      const transportPrepareMs=monotonicNow()-prepareStarted;
      if(onProgress)await onProgress({stage:"adding",receivedAssets:batchStart+deliveryStart,
        expectedAssets:manifest.assets.length,compressedAssetBytes,sourceAssetBytes,
        totalCompressedAssetBytes:manifest.compressedAssetBytes,totalSourceAssetBytes:manifest.uncompressedAssetBytes,
        pendingAssetPaths:assets.map(asset=>asset.path),batchCompressedBytes:deliveryBytes,
        profileElapsedMs:monotonicNow()-profileStarted,batchElapsedMs:monotonicNow()-batchStarted});
      const bridgeStarted=monotonicNow();
      const result=JSON.parse(await bridge.invokeMethodAsync("AddIncrementalSourceGzipAssetBatch",
        assets.map(asset=>asset.path),compressedLengths,bytesToBase64(compressedBatch)));
      const bridgeBatchMs=monotonicNow()-bridgeStarted;
      throwIfAborted(signal);
      if(!Array.isArray(result.items)||result.items.length!==assets.length)
        throw new Error("WASM source store returned an invalid incremental asset batch");
      for(let offset=0;offset<assets.length;offset++){
        const index=batchStart+deliveryStart+offset;
        const asset=assets[offset];
        const {compressed,fetchAndHashMs,cachedLocally}=fetched[offset];
        const item=result.items[offset];
        deliveredAssetBytes+=asset.bytes;
        if(!item.added||item.expectedAssets!==manifest.assets.length||item.receivedAssets!==index+1
            ||item.receivedBytes!==deliveredAssetBytes
            ||item.receivedCompressedBytes!==compressedAssetBytes+compressed.byteLength)
          throw new Error(`WASM source store rejected asset ${asset.path}`);
        compressedBytes+=compressed.byteLength;
        sourceBytes+=asset.bytes;
        compressedAssetBytes+=compressed.byteLength;
        sourceAssetBytes+=asset.bytes;
        maximumAssetBytes=Math.max(maximumAssetBytes,asset.bytes);
        rememberSlowest(slowestFetches,{path:asset.path,durationMs:fetchAndHashMs,compressedBytes:compressed.byteLength});
        if(onProgress)await onProgress({receivedAssets:index+1,expectedAssets:manifest.assets.length,
          stage:"delivered",compressedAssetBytes,sourceAssetBytes,maximumAssetBytes,
          totalCompressedAssetBytes:manifest.compressedAssetBytes,totalSourceAssetBytes:manifest.uncompressedAssetBytes,
          asset:{path:asset.path,compressedBytes:compressed.byteLength,sourceBytes:asset.bytes,
            fetchAndHashMs,wasCached:fetched[offset].wasCached,cachedLocally,cacheWriteError:fetched[offset].cacheWriteError,transportPrepareMs:transportPrepareMs/assets.length,bridgeMs:bridgeBatchMs/assets.length,
            bridgeBatchMs,bridgeBatchAssets:assets.length},
          profileElapsedMs:monotonicNow()-profileStarted,batchElapsedMs:monotonicNow()-batchStarted});
      }
      rememberSlowest(slowestBridgeCalls,{path:assets.map(asset=>asset.path).join(" | "),
        durationMs:bridgeBatchMs,assets:assets.length,compressedBytes:deliveryBytes});
    }
  }
  if (compressedAssetBytes !== manifest.compressedAssetBytes || sourceAssetBytes !== manifest.uncompressedAssetBytes)
    throw new Error("Incremental source profile byte totals do not match its manifest");
  return {profile: manifest.profile, rootAssets: manifest.rootAssets, assetCount: manifest.assetCount,
    compressedBytes, sourceBytes, maximumAssetBytes,totalCompressedAssetBytes:manifest.compressedAssetBytes,
    totalSourceAssetBytes:manifest.uncompressedAssetBytes,downloadConcurrency,
    elapsedMs:monotonicNow()-profileStarted,slowestFetches,slowestBridgeCalls};
}
