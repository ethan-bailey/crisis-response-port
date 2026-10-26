import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { readFile, stat } from 'node:fs/promises';
import { createReadStream } from 'node:fs';
import { resolve, sep, extname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { chromium } from '../game/node_modules/playwright/index.mjs';

const gameRoot = resolve(fileURLToPath(new URL('../game', import.meta.url)));
const mime = new Map([
  ['.html', 'text/html; charset=utf-8'],
  ['.mjs', 'text/javascript; charset=utf-8'],
  ['.js', 'text/javascript; charset=utf-8'],
  ['.json', 'application/json'],
  ['.gz', 'application/gzip'],
  ['.wav', 'audio/wav'],
]);
let browser;
const server = createServer(async (request, response) => {
  try {
    const pathname = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
    if (pathname === '/') {
      response.writeHead(200, { 'content-type': 'text/html; charset=utf-8' });
      response.end('<!doctype html><meta name="viewport" content="width=device-width,initial-scale=1"><title>audio acceptance fixture</title><main>Weapon audio acceptance</main>');
      return;
    }
    const target = resolve(gameRoot, '.' + pathname);
    if (!target.startsWith(gameRoot + sep)) {
      response.writeHead(403).end('forbidden');
      return;
    }
    const metadata = await stat(target);
    if (!metadata.isFile()) {
      response.writeHead(404).end('not found');
      return;
    }
    response.writeHead(200, {
      'content-type': mime.get(extname(target)) || 'application/octet-stream',
      'content-length': metadata.size,
      'cache-control': 'no-store',
    });
    createReadStream(target).pipe(response);
  } catch {
    response.writeHead(404).end('not found');
  }
});
await new Promise(resolveListen => server.listen(0, '127.0.0.1', resolveListen));
const address = server.address();
const origin = 'http://127.0.0.1:' + address.port;
const expectedFiles = {
  audioFire: 'audioFire.wav',
  audioExtractMagazine: 'audioExtractMagazine.wav',
  audioInsertMagazine: 'audioInsertMagazine.wav',
  audioBoltReturn: 'audioBoltReturn.wav',
};
let pageError = null;
try {
  browser = await chromium.launch({ headless: true });
  const page = await browser.newPage();
  page.on('pageerror', error => { pageError = String(error?.stack || error); });
  page.on('console', message => {
    if (message.type() === 'error') pageError ??= message.text();
  });
  await page.goto(origin + '/', { waitUntil: 'load' });
  await page.setContent(`<!doctype html><button id="unlock" type="button">Enable weapon audio</button><output id="ready">loading</output><script type="module">
    import { createWeaponAudioRuntime, createSourceWeaponAudioDispatcher, preloadWeaponAudio } from '/weapon-audio-runtime.mjs';
    const bytes = await fetch('/assets/weapon-runtime.json.gz');
    if (!bytes.ok) throw new Error('Weapon manifest request failed: ' + bytes.status);
    const weaponData = await new Response(bytes.body.pipeThrough(new DecompressionStream('gzip'))).json();
    const NativeAudioContext = window.AudioContext;
    let activeContext = null;
    let analyser = null;
    class InstrumentedAudioContext extends NativeAudioContext {
      constructor() { super(); activeContext = this; }
    }
    const connect = AudioNode.prototype.connect;
    AudioNode.prototype.connect = function(destination, ...args) {
      const result = connect.call(this, destination, ...args);
      if (activeContext && destination === activeContext.destination && !analyser) {
        analyser = activeContext.createAnalyser();
        analyser.fftSize = 2048;
        connect.call(this, analyser);
      }
      return result;
    };
    const audio = createWeaponAudioRuntime(weaponData.audio, { AudioContextClass: InstrumentedAudioContext });
    await preloadWeaponAudio(weaponData.audio, audio);
    const dispatch = createSourceWeaponAudioDispatcher(weaponData.audio, audio);
    window.audioFixture = { weaponData, audio, dispatch, context: activeContext, analyser };
    document.querySelector('#ready').value = 'ready';
    window.audioFixtureReady = true;
  </script>`);
  await page.waitForFunction(() => window.audioFixtureReady === true, null, { timeout: 15000 });
  const manifest = await page.evaluate(() => {
    const keys = ['audioFire', 'audioExtractMagazine', 'audioInsertMagazine', 'audioBoltReturn'];
    const entries = Object.fromEntries(keys.map(key => {
      const info = window.audioFixture.weaponData.audio[key];
      return [key, { source: info?.source, url: info?.url, basename: new URL(info?.url, location.href).pathname.split('/').pop() }];
    }));
    const snapshot = window.audioFixture.audio.snapshot();
    return { entries, snapshot, analyzerInstalled: !!window.audioFixture.analyser,
      urlCount: Object.values(window.audioFixture.weaponData.audio).filter(info => info?.source && info?.url).length };
  });
  for (const [key, basename] of Object.entries(expectedFiles)) {
    assert.ok(manifest.entries[key]?.source, key + ' has an authored source event path');
    assert.equal(manifest.entries[key].basename, basename, key + ' maps to its original WAV file');
  }
  assert.ok(manifest.analyzerInstalled, 'analyser is connected to the production output edge');
  assert.equal(manifest.snapshot.contextState, 'suspended', 'runtime is created before the user gesture');
  assert.equal(manifest.snapshot.decodeErrors, 0, 'all authored clip files decode');
  assert.equal(manifest.snapshot.loadedBuffers, manifest.urlCount, 'every source clip has a decoded buffer');
  assert.equal(manifest.snapshot.outputGain, 0.16, 'production master gain ceiling is retained');

  await page.locator('#unlock').click();
  await page.waitForFunction(() => window.audioFixture.context.state === 'running', null, { timeout: 5000 });
  const events = await page.evaluate(keys => {
    const fixture = window.audioFixture;
    return keys.map(key => {
      const info = fixture.weaponData.audio[key];
      const before = fixture.audio.snapshot().played;
      fixture.dispatch([{ sourcePath: info.source, action: 'oneShot', time: 20, volume: 0.8 }], 20);
      return { key, source: info.source, played: fixture.audio.snapshot().played - before };
    });
  }, Object.keys(expectedFiles));
  for (const event of events) assert.equal(event.played, 1, event.key + ' source event plays exactly once');
  await page.waitForTimeout(80);
  const rms = await page.evaluate(async () => {
    const analyser = window.audioFixture.analyser;
    const samples = new Float32Array(analyser.fftSize);
    let peakRms = 0;
    for (let frame = 0; frame < 30; frame++) {
      analyser.getFloatTimeDomainData(samples);
      let sum = 0;
      for (const sample of samples) sum += sample * sample;
      peakRms = Math.max(peakRms, Math.sqrt(sum / samples.length));
      await new Promise(resolveFrame => setTimeout(resolveFrame, 20));
    }
    return peakRms;
  });
  const final = await page.evaluate(() => window.audioFixture.audio.snapshot());
  assert.ok(rms > 0.0001, 'decoded original audio produces non-silent samples at the production output');
  assert.equal(final.played, Object.keys(expectedFiles).length);
  assert.equal(final.droppedStale, 0);
  assert.equal(final.droppedSuspended, 0);
  assert.equal(final.droppedUnloaded, 0);
  assert.equal(final.unmappedSourceEvents, 0);
  assert.equal(final.decodeErrors, 0);
  assert.equal(final.unlockFailures, 0);
  assert.equal(final.contextState, 'running');
  assert.equal(pageError, null, pageError || 'browser page completed without errors');

  console.log(JSON.stringify({
    status: 'passed',
    browser: 'Chromium',
    evidence: 'production WebAudio graph with original decoded WAV buffers and trusted pointer click',
    decodedSourceClipCount: final.loadedBuffers,
    requiredOriginalClips: events,
    contextState: final.contextState,
    outputGain: final.outputGain,
    maxOutputGain: final.maxOutputGain,
    playedEvents: final.played,
    droppedStale: final.droppedStale,
    droppedSuspended: final.droppedSuspended,
    droppedUnloaded: final.droppedUnloaded,
    unmappedSourceEvents: final.unmappedSourceEvents,
    peakAnalyserRms: rms,
    boundary: 'headless graph playback only; physical device audibility is not claimed',
  }, null, 2));
  await page.close();
} finally {
  await browser?.close();
  await new Promise(resolveClose => server.close(resolveClose));
}
