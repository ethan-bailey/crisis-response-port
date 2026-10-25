import assert from "node:assert/strict";
import { gzipSync } from "node:zlib";
import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";
import { test } from "node:test";
import { deliverIncrementalSourceProfile, loadOriginalScenePackage } from "../scene-data/scene-package-loader.mjs";

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

test("delivers verified source assets sequentially to the WASM package bridge", async () => {
  const hash = bytes => createHash("sha256").update(bytes).digest("hex");
  const assetPath = "Assets/Resources/Global context.prefab";
  const assetText = '{"records":[]}';
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
  const files = new Map([["manifest.json", encoder.encode(JSON.stringify(manifest))],
    ["metadata.json.gz", metadataGzip], ["0123456789abcdef01234567.json.gz", assetGzip]]);
  globalThis.fetch = async url => {
    const name = new URL(url).pathname.split("/").at(-1);
    return files.has(name) ? new Response(files.get(name), {status: 200}) : new Response("missing", {status: 404});
  };
  const calls = [];
  const bridge = {invokeMethodAsync: async (method, ...args) => {
    calls.push([method, ...args]);
    if (method === "InitializeIncrementalSourcePackage")
      return JSON.stringify({expectedAssets: 1, receivedAssets: 0, receivedBytes: 0, receivedCompressedBytes: 0});
    return JSON.stringify({expectedAssets: 1, receivedAssets: 1, receivedBytes: assetRaw.byteLength,
      receivedCompressedBytes: assetGzip.byteLength});
  }};
  const progress = [];
  try {
    const result = await deliverIncrementalSourceProfile(bridge, "https://example.test/source/manifest.json", {onProgress: value => progress.push(value)});
    assert.equal(result.assetCount, 1);
    assert.equal(result.sourceBytes, metadataRaw.byteLength + assetRaw.byteLength);
    assert.equal(result.maximumAssetBytes, assetRaw.byteLength);
    assert.deepEqual(calls.map(call => call[0]), ["InitializeIncrementalSourcePackage", "AddIncrementalSourceGzipAsset"]);
    assert.equal(calls[1][0], "AddIncrementalSourceGzipAsset");
    assert.equal(calls[1][1], assetPath);
    assert.deepEqual(new Uint8Array(gzipSync(assetRaw)), calls[1][2]);
    assert.deepEqual(progress, [{receivedAssets:1,expectedAssets:1,compressedAssetBytes:assetGzip.byteLength,sourceAssetBytes:assetRaw.byteLength,maximumAssetBytes:assetRaw.byteLength}]);
  } finally {
    globalThis.fetch = originalFetch;
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
