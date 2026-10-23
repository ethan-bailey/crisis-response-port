import assert from "node:assert/strict";
import { gzipSync } from "node:zlib";
import { test } from "node:test";
import { loadOriginalScenePackage } from "../scene-data/scene-package-loader.mjs";

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
