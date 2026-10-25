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
