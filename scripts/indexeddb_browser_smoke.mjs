import assert from "node:assert/strict";
import { createHash } from "node:crypto";
import { gzipSync } from "node:zlib";
import { createServer } from "node:http";
import { chromium } from "../game/node_modules/playwright/index.mjs";

const sha256 = bytes => createHash("sha256").update(bytes).digest("hex");
const sourceHash1 = "a".repeat(64);
const sourceHash2 = "b".repeat(64);
const assetPath = "Assets/Resources/fixture.prefab";
const assetName = "assets/0123456789abcdef01234567.json.gz";
const rawAsset = Buffer.from(JSON.stringify({ fixture: "verified IndexedDB cache" }));
const compressedAsset = gzipSync(rawAsset);
const metadataBytes = Buffer.from(JSON.stringify({
  schemaVersion: 1,
  guidPaths: {},
  assetManifest: {
    [assetPath]: {
      bytes: rawAsset.byteLength,
      sha256: sha256(rawAsset),
      compressedBytes: compressedAsset.byteLength,
      compressedSha256: sha256(compressedAsset),
    },
  },
}));
const metadataGzip = gzipSync(metadataBytes);
const manifest = sourceHash => ({
  schemaVersion: 1,
  profile: "browser-indexeddb-fixture",
  encoding: "gzip-json-assets",
  sourcePackageSha256: sourceHash,
  rootAssets: [assetPath],
  assetCount: 1,
  compressedAssetBytes: compressedAsset.byteLength,
  uncompressedAssetBytes: rawAsset.byteLength,
  metadata: {
    file: "metadata.json.gz",
    bytes: metadataBytes.byteLength,
    sha256: sha256(metadataBytes),
    compressedBytes: metadataGzip.byteLength,
    compressedSha256: sha256(metadataGzip),
  },
  assets: [{
    path: assetPath,
    file: assetName,
    bytes: rawAsset.byteLength,
    sha256: sha256(rawAsset),
    compressedBytes: compressedAsset.byteLength,
    compressedSha256: sha256(compressedAsset),
  }],
});

const counts = { manifest: 0, metadata: 0, asset: 0 };
const server = createServer(async (request, response) => {
  const url = new URL(request.url, "http://localhost");
  const send = (status, type, body) => {
    response.writeHead(status, { "Content-Type": type, "Cache-Control": "no-store" });
    response.end(body);
  };
  if (url.pathname === "/") {
    send(200, "text/html", `<!doctype html><meta charset="utf-8"><script type="module">
      import { clearIncrementalSourceCache, deliverIncrementalSourceProfile }
        from "/scene-data/scene-package-loader.mjs";
      window.runFixtureLoad = async (version = 1) => {
        const progress = [];
        const bridge = { invokeMethodAsync: async (method, ...args) => {
          if (method === "InitializeIncrementalSourcePackage")
            return JSON.stringify({ expectedAssets: 1, receivedAssets: 0, receivedBytes: 0, receivedCompressedBytes: 0 });
          if (method !== "AddIncrementalSourceGzipAssetBatch") throw new Error("Unexpected bridge method: " + method);
          const decoded = atob(args[2]);
          const bytes = Uint8Array.from(decoded, value => value.charCodeAt(0));
          if (bytes.byteLength !== ${compressedAsset.byteLength}) throw new Error("Bridge payload byte count mismatch");
          return JSON.stringify({ items: [{ added: true, expectedAssets: 1, receivedAssets: 1,
            receivedBytes: ${rawAsset.byteLength}, receivedCompressedBytes: ${compressedAsset.byteLength} }] });
        } };
        const result = await deliverIncrementalSourceProfile(bridge,
          location.origin + "/manifest.json?version=" + version,
          { onProgress: value => progress.push(value) });
        return { result, progress };
      };
      window.clearFixtureCache = () => clearIncrementalSourceCache(location.origin + "/manifest.json?version=1");
    </script>`);
    return;
  }
  if (url.pathname === "/manifest.json") {
    counts.manifest++;
    send(200, "application/json", JSON.stringify(manifest(url.searchParams.get("version") === "2" ? sourceHash2 : sourceHash1)));
    return;
  }
  if (url.pathname === "/metadata.json.gz") {
    counts.metadata++;
    send(200, "application/gzip", metadataGzip);
    return;
  }
  if (url.pathname === "/" + assetName) {
    counts.asset++;
    send(200, "application/gzip", compressedAsset);
    return;
  }
  if (url.pathname === "/scene-data/scene-package-loader.mjs") {
    const { readFile } = await import("node:fs/promises");
    send(200, "text/javascript", await readFile(new URL("../scene-data/scene-package-loader.mjs", import.meta.url)));
    return;
  }
  send(404, "text/plain", "not found");
});

await new Promise(resolve => server.listen(0, "127.0.0.1", resolve));
const { port } = server.address();
const origin = `http://127.0.0.1:${port}`;
let browser;
try {
  browser = await chromium.launch({ headless: true });
  const context = await browser.newContext();
  const page = await context.newPage();
  let pageError = null;
  page.on("pageerror", error => { pageError = error; });
  await page.goto(`${origin}/`);
  await page.waitForFunction(() => typeof window.runFixtureLoad === "function");

  const first = await page.evaluate(() => window.runFixtureLoad(1));
  assert.equal(first.progress[0].backend, "IndexedDB");
  assert.equal(first.progress.at(-1).asset.wasCached, false);
  assert.equal(first.progress.at(-1).asset.cachedLocally, true);
  assert.equal(counts.asset, 1, "first launch fetches and stores the verified asset");

  const second = await page.evaluate(() => window.runFixtureLoad(1));
  assert.equal(second.progress.at(-1).asset.wasCached, true, "second launch reuses the verified IndexedDB record");
  assert.equal(counts.asset, 1, "a valid local hit avoids the asset request");

  await page.evaluate(async ({ sourceHash, assetUrl }) => {
    const database = await new Promise((resolve, reject) => {
      const request = indexedDB.open("crisis-response-mission-assets", 1);
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
    await new Promise((resolve, reject) => {
      const transaction = database.transaction("assets", "readwrite");
      transaction.objectStore("assets").put({ key: sourceHash + ":" + assetUrl,
        compressed: new TextEncoder().encode("invalid cached bytes").buffer });
      transaction.oncomplete = resolve;
      transaction.onerror = () => reject(transaction.error);
      transaction.onabort = () => reject(transaction.error);
    });
    database.close();
  }, { sourceHash: sourceHash1, assetUrl: `${origin}/${assetName}` });
  const recovered = await page.evaluate(() => window.runFixtureLoad(1));
  assert.equal(recovered.progress.at(-1).asset.wasCached, false, "corrupt record is rejected");
  assert.equal(recovered.progress.at(-1).asset.cachedLocally, true, "verified network bytes replace the bad record");
  assert.equal(counts.asset, 2, "one network recovery request repairs the corrupt cache entry");

  const changedManifest = await page.evaluate(() => window.runFixtureLoad(2));
  assert.equal(changedManifest.progress.at(-1).asset.wasCached, false, "a different source hash cannot reuse the prior version");
  assert.equal(counts.asset, 3);

  const cleared = await page.evaluate(() => window.clearFixtureCache());
  assert.deepEqual(cleared, { cleared: true, available: true });
  const otherVersionSurvived = await page.evaluate(async ({ sourceHash, assetUrl }) => {
    const database = await new Promise((resolve, reject) => {
      const request = indexedDB.open("crisis-response-mission-assets", 1);
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
    const record = await new Promise((resolve, reject) => {
      const transaction = database.transaction("assets", "readonly");
      const request = transaction.objectStore("assets").get(sourceHash + ":" + assetUrl);
      request.onsuccess = () => resolve(request.result);
      request.onerror = () => reject(request.error);
    });
    database.close();
    return !!record;
  }, { sourceHash: sourceHash2, assetUrl: `${origin}/${assetName}` });
  assert.equal(otherVersionSurvived, true, "clearing one manifest leaves other source versions intact");
  const afterClear = await page.evaluate(() => window.runFixtureLoad(1));
  assert.equal(afterClear.progress.at(-1).asset.wasCached, false, "clearing the selected source version forces a refetch");
  assert.equal(counts.asset, 4);
  assert.equal(pageError, null, pageError?.stack);

  await context.close();
  console.log(JSON.stringify({
    status: "passed",
    browser: "Chromium",
    actualIndexedDb: true,
    assertions: [
      "first launch stores a manifest-verified asset",
      "second launch reuses the persistent record without a network asset request",
      "corrupt cache data is rejected and repaired from verified network bytes",
      "manifest source hash isolates cache versions",
      "clear removes only the selected manifest version",
    ],
    requests: counts,
  }, null, 2));
} finally {
  await browser?.close();
  await new Promise(resolve => server.close(resolve));
}
