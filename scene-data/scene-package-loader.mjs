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
