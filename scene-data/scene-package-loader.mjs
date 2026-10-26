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

async function fetchCompressedAsset(url, expected, label, signal, persistentCache) {
  const started = monotonicNow();
  let response = persistentCache ? await persistentCache.match(url) : null;
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
  if (!wasCached && persistentCache) {
    try {
      await persistentCache.put(url, new Response(compressed.slice(0), {headers:{'Content-Type':'application/gzip'}}));
      cachedLocally = true;
    } catch { /* Safari quota or private browsing: this run still uses verified network bytes. */ }
  }
  return {compressed, fetchAndHashMs: monotonicNow() - started, wasCached, cachedLocally};
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

  // Cache only the immutable compressed assets. The cache name is bound to
  // the profile hash; every cache hit still passes the manifest size and SHA.
  let persistentCache = null;
  if (globalThis.isSecureContext && globalThis.caches?.open) {
    try { persistentCache = await globalThis.caches.open(`crisis-source-${manifest.sourcePackageSha256}`); }
    catch { /* Storage is optional; continue with verified network delivery. */ }
  }
  // Storage persistence is a best-effort request (Safari may decline it); it
  // never blocks the Mission or changes the verified network fallback.
  try { await globalThis.navigator?.storage?.persist?.(); } catch { /* Browser-managed quota remains valid. */ }

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
  for (let batchStart = 0; batchStart < manifest.assets.length; batchStart += downloadConcurrency) {
    const batchStarted = monotonicNow();
    const batch = manifest.assets.slice(batchStart, batchStart + downloadConcurrency);
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
            fetchAndHashMs,wasCached:fetched[offset].wasCached,cachedLocally,transportPrepareMs:transportPrepareMs/assets.length,bridgeMs:bridgeBatchMs/assets.length,
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
