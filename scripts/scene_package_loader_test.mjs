import assert from "node:assert/strict";
import { gzipSync } from "node:zlib";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { clearIncrementalSourceCache, deliverIncrementalSourceProfile, loadOriginalScenePackage } from "../scene-data/scene-package-loader.mjs";

const encoder = new TextEncoder();
const originalFetch = globalThis.fetch;

function mockPackage(source) {
  const data = encoder.encode(source);
  const chunks = [];
  for (let start = 0, index = 0; start < data.length; start += 7, index += 1) {
    const bytes = data.slice(start, start + 7);
    chunks.push({
      path: `crisis-response-scenes-${String(index).padStart(3, "0")}.json.gz`,
      bytes: bytes.length,
      gzip: gzipSync(bytes),
    });
  }
  const manifest = {
    schemaVersion: 1,
    encoding: "independent-gzip-chunks",
    uncompressedBytes: data.length,
    chunks: chunks.map(({ path, bytes }) => ({ path, bytes })),
  };
  globalThis.fetch = async (url) => {
    const name = new URL(url).pathname.split("/").at(-1);
    if (name === "crisis-response-scenes.manifest.json") {
      return new Response(JSON.stringify(manifest), { status: 200 });
    }
    const chunk = chunks.find((entry) => entry.path === name);
    return chunk ? new Response(chunk.gzip, { status: 200 }) : new Response("missing", { status: 404 });
  };
}

test("reassembles compressed scene chunks across UTF-8 boundaries", async () => {
  const source = '{"scene":"Mission 01 ☃ — recovery"}';
  mockPackage(source);
  try {
    assert.equal(await loadOriginalScenePackage(), source);
  } finally {
    globalThis.fetch = originalFetch;
  }
});

test("rejects an invalid chunk path before fetching it", async () => {
  const manifest = {
    schemaVersion: 1,
    encoding: "independent-gzip-chunks",
    uncompressedBytes: 0,
    chunks: [{ path: "../outside.json.gz", bytes: 0 }],
  };
  globalThis.fetch = async () => new Response(JSON.stringify(manifest), { status: 200 });
  try {
    await assert.rejects(loadOriginalScenePackage(), /invalid chunk path/);
  } finally {
    globalThis.fetch = originalFetch;
  }
});

test("delivers verified source assets through the WASM package bridge", async () => {
  const hash = bytes => createHash("sha256").update(bytes).digest("hex");
  const assetPath = "Assets/Resources/Global context.prefab";
  let state = 0x5eed;
  let payload = "";
  for (let index = 0; index < 96 * 1024; index++) {
    state = (Math.imul(state, 1664525) + 1013904223) >>> 0;
    payload += String.fromCharCode(65 + state % 26);
  }
  const assetText = `{"payload":"${payload}"}`;
  const assetRaw = encoder.encode(assetText);
  const assetGzip = gzipSync(assetRaw);
  const assetCompressedHash = hash(assetGzip);
  const metadataText = JSON.stringify({schemaVersion: 1, guidPaths: {},
    assetManifest: {[assetPath]: {bytes: assetRaw.byteLength, sha256: hash(assetRaw),
      compressedBytes: assetGzip.byteLength, compressedSha256: assetCompressedHash}}});
  const metadataRaw = encoder.encode(metadataText);
  const metadataGzip = gzipSync(metadataRaw);
  const manifest = {schemaVersion: 1, profile: "fixture", encoding: "gzip-json-assets",
    sourcePackageSha256: "a".repeat(64), rootAssets: [assetPath], assetCount: 1,
    compressedAssetBytes: assetGzip.byteLength, uncompressedAssetBytes: assetRaw.byteLength,
    metadata: {file: "metadata.json.gz", bytes: metadataRaw.byteLength, sha256: hash(metadataRaw),
      compressedBytes: metadataGzip.byteLength, compressedSha256: hash(metadataGzip)},
    assets: [{path: assetPath, file: "assets/0123456789abcdef01234567.json.gz", bytes: assetRaw.byteLength,
      sha256: hash(assetRaw), compressedBytes: assetGzip.byteLength, compressedSha256: assetCompressedHash}]};
  const originalFetch = globalThis.fetch;
  const originalCaches = globalThis.caches;
  const originalSecureContext = globalThis.isSecureContext;
  const files = new Map([["manifest.json", encoder.encode(JSON.stringify(manifest))],
    ["metadata.json.gz", metadataGzip], ["0123456789abcdef01234567.json.gz", assetGzip]]);
  const assetFetchCounts = new Map();
  globalThis.fetch = async url => {
    const name = new URL(url).pathname.split("/").at(-1);
    if (name === "0123456789abcdef01234567.json.gz")
      assetFetchCounts.set(name, (assetFetchCounts.get(name) ?? 0) + 1);
    return files.has(name) ? new Response(files.get(name), {status: 200}) : new Response("missing", {status: 404});
  };
  const cachedResponses = new Map();
  globalThis.isSecureContext = true;
  globalThis.caches = {open:async()=>({
    match:async url=>{
      const response=cachedResponses.get(String(url));
      return response?new Response(await response.arrayBuffer(),{headers:{"Content-Type":"application/gzip"}}):undefined;
    },
    delete:async url=>cachedResponses.delete(String(url)),
    put:async(url,response)=>cachedResponses.set(String(url),new Response(await response.arrayBuffer(),{headers:{"Content-Type":"application/gzip"}}))
  })};
  const calls = [];
  const bridge = {invokeMethodAsync: async (method, ...args) => {
    calls.push([method, ...args]);
    if (method === "InitializeIncrementalSourcePackage")
      return JSON.stringify({expectedAssets: 1, receivedAssets: 0, receivedBytes: 0, receivedCompressedBytes: 0});
    assert.equal(method,"AddIncrementalSourceGzipAssetBatch");
    assert.deepEqual(args[0],[assetPath]);
    assert.deepEqual(args[1],[assetGzip.byteLength]);
    assert.equal(typeof args[2],"string","Blazor byte[] interop accepts a base64 string, not Uint8Array JSON");
    assert.deepEqual(Buffer.from(args[2],"base64"),Buffer.from(assetGzip));
    return JSON.stringify({items:[{added:true,expectedAssets:1,receivedAssets:1,receivedBytes:assetRaw.byteLength,
      receivedCompressedBytes:assetGzip.byteLength}]});
  }};
  const progress = [];
  try {
    const result = await deliverIncrementalSourceProfile(bridge, "https://example.test/source/manifest.json", {onProgress: value => progress.push(value)});
    assert.equal(result.assetCount, 1);
    assert.equal(result.sourceBytes, metadataRaw.byteLength + assetRaw.byteLength);
    assert.equal(result.maximumAssetBytes, assetRaw.byteLength);
    assert.ok(Number.isFinite(result.elapsedMs) && result.elapsedMs >= 0);
    assert.equal(result.slowestFetches.length, 1);
    assert.equal(result.slowestFetches[0].path, assetPath);
    assert.equal(result.slowestBridgeCalls.length, 1);
    assert.equal(result.slowestBridgeCalls[0].path, assetPath);
    assert.deepEqual(calls.map(call => call[0]), ["InitializeIncrementalSourcePackage", "AddIncrementalSourceGzipAssetBatch"]);
    assert.equal(calls[1][0], "AddIncrementalSourceGzipAssetBatch");
    assert.deepEqual(calls[1][1],[assetPath]);
    assert.deepEqual(calls[1][2],[assetGzip.byteLength]);
    assert.equal(typeof calls[1][3],"string");
    assert.deepEqual(Buffer.from(calls[1][3],"base64"),Buffer.from(gzipSync(assetRaw)));
    assert.deepEqual(progress.map(value=>value.stage),["fetching","adding","delivered"]);
    assert.deepEqual(progress[0].pendingAssetPaths,[assetPath]);
    assert.deepEqual(progress[1].pendingAssetPaths,[assetPath]);
    assert.equal(progress[1].batchCompressedBytes,assetGzip.byteLength);
    const completed=progress.at(-1);
    assert.deepEqual({
      receivedAssets:completed.receivedAssets,expectedAssets:completed.expectedAssets,
      compressedAssetBytes:completed.compressedAssetBytes,sourceAssetBytes:completed.sourceAssetBytes,
      maximumAssetBytes:completed.maximumAssetBytes,totalCompressedAssetBytes:completed.totalCompressedAssetBytes,
      totalSourceAssetBytes:completed.totalSourceAssetBytes,asset:completed.asset
    }, {receivedAssets:1,expectedAssets:1,compressedAssetBytes:assetGzip.byteLength,sourceAssetBytes:assetRaw.byteLength,
      maximumAssetBytes:assetRaw.byteLength,totalCompressedAssetBytes:assetGzip.byteLength,totalSourceAssetBytes:assetRaw.byteLength,
      asset:{path:assetPath,compressedBytes:assetGzip.byteLength,sourceBytes:assetRaw.byteLength,
        fetchAndHashMs:completed.asset.fetchAndHashMs,wasCached:false,cachedLocally:true,cacheWriteError:null,transportPrepareMs:completed.asset.transportPrepareMs,
        bridgeMs:completed.asset.bridgeMs,bridgeBatchMs:completed.asset.bridgeBatchMs,bridgeBatchAssets:1}});
    for(const value of [completed.asset.fetchAndHashMs,completed.asset.transportPrepareMs,completed.asset.bridgeMs,
      completed.profileElapsedMs,completed.batchElapsedMs]) assert.ok(Number.isFinite(value) && value >= 0);

    const cachedProgress=[];
    const cachedBridge={invokeMethodAsync:async(method,...args)=>{
      if(method==="InitializeIncrementalSourcePackage")return JSON.stringify({expectedAssets:1,receivedAssets:0,receivedBytes:0});
      assert.equal(method,"AddIncrementalSourceGzipAssetBatch");
      assert.deepEqual(args[0],[assetPath]);
      return JSON.stringify({items:[{added:true,expectedAssets:1,receivedAssets:1,receivedBytes:assetRaw.byteLength,receivedCompressedBytes:assetGzip.byteLength}]});
    }};
    await deliverIncrementalSourceProfile(cachedBridge,"https://example.test/source/manifest.json",{onProgress:value=>cachedProgress.push(value)});
    assert.equal(assetFetchCounts.get("0123456789abcdef01234567.json.gz"),1,"second run reads the verified asset from persistent cache");
    assert.equal(cachedProgress.at(-1).asset.wasCached,true);
    assert.equal(cachedProgress.at(-1).asset.cachedLocally,true,"a verified cache hit is confirmed as stored on device");

    const assetUrl="https://example.test/source/assets/0123456789abcdef01234567.json.gz";
    cachedResponses.set(assetUrl,new Response(encoder.encode("corrupt cached bytes")));
    const recoveredProgress=[];
    await deliverIncrementalSourceProfile(cachedBridge,"https://example.test/source/manifest.json",{onProgress:value=>recoveredProgress.push(value)});
    assert.equal(assetFetchCounts.get("0123456789abcdef01234567.json.gz"),2,"a corrupt cached chunk is recovered from the network");
    assert.equal(recoveredProgress.at(-1).asset.wasCached,false,"network recovery is reported as a cache miss");
    assert.equal(recoveredProgress.at(-1).asset.cachedLocally,true,"the verified replacement is stored on device");
    assert.deepEqual(Buffer.from(await cachedResponses.get(assetUrl).arrayBuffer()),Buffer.from(assetGzip),"verified network bytes replace the corrupt cache entry");

    globalThis.caches={open:async()=>({match:async()=>undefined,delete:async()=>false,
      put:async()=>{const error=new Error("Storage quota reached");error.name="QuotaExceededError";throw error;}})};
    const quotaProgress=[];
    await deliverIncrementalSourceProfile(cachedBridge,"https://example.test/source/manifest.json",{onProgress:value=>quotaProgress.push(value)});
    assert.equal(quotaProgress.at(-1).asset.cachedLocally,false,"quota rejection must not be reported as saved locally");
    assert.equal(quotaProgress.at(-1).asset.cacheWriteError.name,"QuotaExceededError","the exact browser storage failure remains observable");
  } finally {
    globalThis.fetch = originalFetch;
    if(originalCaches===undefined)delete globalThis.caches;else globalThis.caches=originalCaches;
    if(originalSecureContext===undefined)delete globalThis.isSecureContext;else globalThis.isSecureContext=originalSecureContext;
  }
});

test("amortizes WASM interop over a bounded ordered batch of small source assets",async()=>{
  const hash=bytes=>createHash("sha256").update(bytes).digest("hex");
  const entries=["Assets/First.prefab","Assets/Second.prefab"].map((path,index)=>{
    const raw=encoder.encode(JSON.stringify({records:[{index}]}));
    const compressed=gzipSync(raw);
    return {path,raw,compressed,bytes:raw.byteLength,sha256:hash(raw),compressedBytes:compressed.byteLength,
      compressedSha256:hash(compressed),file:`assets/${String(index+1).padStart(24,"0")}.json.gz`};
  });
  const metadataText=JSON.stringify({schemaVersion:1,guidPaths:{},assetManifest:Object.fromEntries(entries.map(item=>[item.path,{
    bytes:item.bytes,sha256:item.sha256,compressedBytes:item.compressedBytes,compressedSha256:item.compressedSha256
  }]))});
  const metadataRaw=encoder.encode(metadataText),metadataGzip=gzipSync(metadataRaw);
  const manifest={schemaVersion:1,profile:"fixture",encoding:"gzip-json-assets",sourcePackageSha256:"a".repeat(64),
    rootAssets:[entries[0].path],assetCount:entries.length,
    compressedAssetBytes:entries.reduce((sum,item)=>sum+item.compressedBytes,0),
    uncompressedAssetBytes:entries.reduce((sum,item)=>sum+item.bytes,0),
    metadata:{file:"metadata.json.gz",bytes:metadataRaw.byteLength,sha256:hash(metadataRaw),
      compressedBytes:metadataGzip.byteLength,compressedSha256:hash(metadataGzip)},
    assets:entries.map(({path,file,bytes,sha256,compressedBytes,compressedSha256})=>({path,file,bytes,sha256,compressedBytes,compressedSha256}))};
  const files=new Map([["manifest.json",encoder.encode(JSON.stringify(manifest))],["metadata.json.gz",metadataGzip],
    ...entries.map(item=>[item.file.split("/").at(-1),item.compressed])]);
  const original=globalThis.fetch;
  globalThis.fetch=async url=>{const value=files.get(new URL(url).pathname.split("/").at(-1));
    return value?new Response(value,{status:200}):new Response("missing",{status:404});};
  let receivedAssets=0,receivedBytes=0,receivedCompressedBytes=0;
  const calls=[];
  const bridge={invokeMethodAsync:async(method,...args)=>{
    calls.push([method,...args]);
    if(method==="InitializeIncrementalSourcePackage")return JSON.stringify({expectedAssets:2,receivedAssets:0,receivedBytes:0,receivedCompressedBytes:0});
    assert.equal(method,"AddIncrementalSourceGzipAssetBatch");
    assert.deepEqual(args[0],entries.map(item=>item.path));
    assert.deepEqual(args[1],entries.map(item=>item.compressed.byteLength));
    assert.equal(typeof args[2],"string","batched Blazor byte[] payload must use base64 JSON encoding");
    assert.deepEqual(Buffer.from(args[2],"base64"),Buffer.concat(entries.map(item=>Buffer.from(item.compressed))));
    const items=entries.map(item=>{
      receivedAssets++;receivedBytes+=item.bytes;receivedCompressedBytes+=item.compressedBytes;
      return {added:true,expectedAssets:2,receivedAssets,receivedBytes,receivedCompressedBytes};
    });
    return JSON.stringify({items});
  }};
  try{
    const progress=[];
    const result=await deliverIncrementalSourceProfile(bridge,"https://example.test/source/manifest.json",{onProgress:value=>progress.push(value)});
    assert.equal(result.assetCount,2);
    assert.deepEqual(calls.map(item=>item[0]),["InitializeIncrementalSourcePackage","AddIncrementalSourceGzipAssetBatch"]);
    assert.deepEqual(progress.filter(item=>item.stage==="delivered").map(item=>item.receivedAssets),[1,2]);
  }finally{globalThis.fetch=original;}
});

test("stops source delivery when its abort signal is already cancelled", async () => {
  const controller = new AbortController();
  controller.abort();
  let requested = false;
  const original = globalThis.fetch;
  globalThis.fetch = async () => { requested = true; throw new Error("fetch should not run"); };
  try {
    await assert.rejects(
      deliverIncrementalSourceProfile({invokeMethodAsync:async()=>"{}"}, "https://example.test/source/manifest.json", {signal:controller.signal}),
      error => error.name === "AbortError",
    );
    assert.equal(requested, false);
  } finally {
    globalThis.fetch = original;
  }
});

test("clear control deletes only the manifest-versioned Mission source cache",async()=>{
  const originalFetch=globalThis.fetch,originalCaches=globalThis.caches,originalSecureContext=globalThis.isSecureContext;
  const sourceHash="b".repeat(64),deleted=[];
  globalThis.isSecureContext=true;
  globalThis.caches={delete:async name=>{deleted.push(name);return true;}};
  globalThis.fetch=async()=>new Response(JSON.stringify({schemaVersion:1,encoding:"gzip-json-assets",sourcePackageSha256:sourceHash}),{status:200});
  try{
    assert.deepEqual(await clearIncrementalSourceCache("https://example.test/source/manifest.json"),{cleared:true,available:true});
    assert.deepEqual(deleted,[`crisis-source-${sourceHash}`]);
  }finally{
    globalThis.fetch=originalFetch;
    if(originalCaches===undefined)delete globalThis.caches;else globalThis.caches=originalCaches;
    if(originalSecureContext===undefined)delete globalThis.isSecureContext;else globalThis.isSecureContext=originalSecureContext;
  }
});

test("defers the 220 MB original scene preflight unless scene-smoke is requested", async () => {
  const game = await readFile(new URL("../game/index.html", import.meta.url), "utf8");
  const runtime = await readFile(new URL("../runtime/Browser/wwwroot/index.html", import.meta.url), "utf8");
  for (const [name, html] of [["game", game], ["runtime", runtime]]) {
    const guard = html.indexOf('has("scene-smoke")');
    const load = html.indexOf("loadOriginalScenePackage())");
    assert.ok(guard >= 0 && load > guard, `${name}: heavy scene package must be behind the opt-in smoke flag`);
  }
});
