import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { createServer } from "node:http";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { chromium } from "../game/node_modules/playwright/index.mjs";

const profileDir = resolve(process.argv[2] ?? "");
assert.ok(process.argv[2], "Pass the verified incremental profile directory");
const manifest = JSON.parse(await readFile(resolve(profileDir, "manifest.json"), "utf8"));
assert.equal(manifest.profile, "global-ui-mission-operator");
assert.equal(manifest.assetCount, 1919);
const metadataBytes = await readFile(resolve(profileDir, manifest.metadata.file));
assert.equal(metadataBytes.byteLength, manifest.metadata.compressedBytes);
assert.equal(createHash("sha256").update(metadataBytes).digest("hex"), manifest.metadata.compressedSha256);
const assetsByFile = new Map(manifest.assets.map(asset => [asset.file, asset]));
const assetSummary = Object.fromEntries(manifest.assets.map(asset =>
  [asset.path, {bytes:asset.bytes, compressedBytes:asset.compressedBytes}]));
const requestCounts = { manifest: 0, metadata: 0, assets: 0, bad: 0 };

const server = createServer(async (request, response) => {
  const url = new URL(request.url, "http://localhost");
  const send = (status, type, body) => {
    response.writeHead(status, { "Content-Type": type, "Cache-Control": "no-store" });
    response.end(body);
  };
  try {
    if (url.pathname === "/") {
      send(200, "text/html", `<!doctype html><meta charset="utf-8"><script>
        window.fullProfileAssetSummary = ${JSON.stringify(assetSummary)};
      </script><script type="module">
        import { deliverIncrementalSourceProfile } from "/scene-data/scene-package-loader.mjs";
        window.runFullProfile = async () => {
          const progress = [];
          let expectedAssets = 0, receivedAssets = 0, receivedBytes = 0, receivedCompressedBytes = 0;
          let assetManifest;
          const bridge = { invokeMethodAsync: async (method, ...args) => {
            if (method === "InitializeIncrementalSourcePackage") {
              if (typeof args[0] !== "string") throw new Error("Source metadata bridge input is not text");
              assetManifest = window.fullProfileAssetSummary;
              expectedAssets = Object.keys(assetManifest).length;
              return JSON.stringify({expectedAssets, receivedAssets, receivedBytes, receivedCompressedBytes});
            }
            if (method !== "AddIncrementalSourceGzipAssetBatch") throw new Error("Unexpected interop method: " + method);
            const [paths, lengths, encoded] = args;
            const binary = atob(encoded);
            const payload = Uint8Array.from(binary, value => value.charCodeAt(0));
            if (lengths.reduce((sum, value) => sum + value, 0) !== payload.byteLength)
              throw new Error("Source batch byte lengths do not match payload");
            const items = [];
            for (let i = 0; i < paths.length; i++) {
              const expected = assetManifest[paths[i]];
              if (!expected || lengths[i] !== expected.compressedBytes)
                throw new Error("Unknown or invalid asset in interop batch: " + paths[i]);
              receivedAssets++;
              receivedBytes += expected.bytes;
              receivedCompressedBytes += lengths[i];
              items.push({added:true, expectedAssets, receivedAssets, receivedBytes, receivedCompressedBytes});
            }
            return JSON.stringify({items});
          } };
          const result = await deliverIncrementalSourceProfile(bridge, location.origin + "/manifest.json",
            {onProgress: value => progress.push(value)});
          return {result, progress};
        };
      </script>`);
      return;
    }
    if (url.pathname === "/manifest.json") {
      requestCounts.manifest++;
      send(200, "application/json", JSON.stringify(manifest));
      return;
    }
    if (url.pathname === "/" + manifest.metadata.file) {
      requestCounts.metadata++;
      send(200, "application/gzip", metadataBytes);
      return;
    }
    if (url.pathname.startsWith("/assets/")) {
      const file = url.pathname.slice(1);
      const entry = assetsByFile.get(file);
      if (!entry) { requestCounts.bad++; send(404, "text/plain", "Unknown source asset"); return; }
      requestCounts.assets++;
      const bytes = await readFile(resolve(profileDir, file));
      if (bytes.byteLength !== entry.compressedBytes
          || createHash("sha256").update(bytes).digest("hex") !== entry.compressedSha256) {
        requestCounts.bad++;
        send(500, "text/plain", "Source asset changed after profile verification");
        return;
      }
      send(200, "application/gzip", bytes);
      return;
    }
    if (url.pathname === "/scene-data/scene-package-loader.mjs") {
      send(200, "text/javascript", await readFile(new URL("../scene-data/scene-package-loader.mjs", import.meta.url)));
      return;
    }
    send(404, "text/plain", "Not found");
  } catch (error) {
    requestCounts.bad++;
    send(500, "text/plain", String(error));
  }
});

await new Promise(resolveListen => server.listen(0, "127.0.0.1", resolveListen));
const { port } = server.address();
let browser;
try {
  browser = await chromium.launch({headless:true});
  const context = await browser.newContext();
  const page = await context.newPage();
  let pageError = null;
  page.on("pageerror", error => { pageError = error; });
  await page.goto(`http://127.0.0.1:${port}/`);
  await page.waitForFunction(() => typeof window.runFullProfile === "function");
  const first = await page.evaluate(() => window.runFullProfile());
  assert.equal(first.progress[0].backend, "IndexedDB");
  assert.equal(first.result.assetCount, manifest.assetCount);
  assert.equal(first.result.sourceBytes, manifest.uncompressedAssetBytes + manifest.metadata.bytes);
  assert.equal(first.result.compressedBytes, manifest.compressedAssetBytes + manifest.metadata.compressedBytes);
  const firstDelivered = first.progress.filter(item => item.stage === "delivered");
  assert.equal(firstDelivered.length, manifest.assetCount);
  assert.ok(firstDelivered.every(item => item.asset.cachedLocally), "all verified source chunks are persisted on first load");
  assert.ok(firstDelivered.every(item => !item.asset.wasCached), "first load downloads every profile chunk");
  assert.equal(requestCounts.assets, manifest.assetCount);
  assert.equal(requestCounts.bad, 0);
  const downloadedRequests = requestCounts.assets;

  await page.reload();
  await page.waitForFunction(() => typeof window.runFullProfile === "function");
  const second = await page.evaluate(() => window.runFullProfile());
  assert.equal(second.progress[0].backend, "IndexedDB");
  assert.equal(second.result.assetCount, manifest.assetCount);
  const secondDelivered = second.progress.filter(item => item.stage === "delivered");
  assert.equal(secondDelivered.length, manifest.assetCount);
  assert.ok(secondDelivered.every(item => item.asset.wasCached && item.asset.cachedLocally),
    "a new page load reuses all verified chunks from IndexedDB");
  assert.equal(requestCounts.assets, downloadedRequests,
    "the repeat load makes no network request for profile chunks");
  assert.equal(requestCounts.bad, 0);
  assert.equal(pageError, null, pageError?.stack);

  console.log(JSON.stringify({
    status:"passed",
    browser:"Chromium",
    evidence:"production incremental loader over the complete verified Mission profile; Blazor bridge modeled in page",
    profile:manifest.profile,
    assetCount:manifest.assetCount,
    compressedAssetBytes:manifest.compressedAssetBytes,
    sourceAssetBytes:manifest.uncompressedAssetBytes,
    firstLoadMs:Math.round(first.result.elapsedMs),
    cachedReloadMs:Math.round(second.result.elapsedMs),
    firstLoadNetworkAssets:downloadedRequests,
    cachedReloadNetworkAssets:requestCounts.assets-downloadedRequests,
    firstLoadPersistedAssets:firstDelivered.filter(item=>item.asset.cachedLocally).length,
    cachedReloadHits:secondDelivered.filter(item=>item.asset.wasCached).length,
    firstLoadSlowestFetches:first.result.slowestFetches.slice(0,3),
    firstLoadSlowestBridgeBatches:first.result.slowestBridgeCalls.slice(0,3),
    backend:first.progress[0].backend,
    boundary:"real Chromium IndexedDB and production JS loader; Blazor bridge modeled; not iPhone Safari or device storage evidence"
  }));
  await context.close();
} finally {
  if (browser) await browser.close();
  await new Promise(resolveClose => server.close(resolveClose));
}
