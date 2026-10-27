import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { readFile, stat, mkdir } from 'node:fs/promises';
import { resolve, sep, extname } from 'node:path';
import { chromium } from '../game/node_modules/playwright/index.mjs';

const packageRoot = resolve(process.argv[2] ?? '');
const outputRoot = resolve(process.argv[3] ?? '');
assert.ok(process.argv[2], 'Pass the complete, integrity-verified Site package directory');
assert.ok(process.argv[3], 'Pass a directory for the browser report and screenshot');
await mkdir(outputRoot, { recursive: true });

const build = JSON.parse(await readFile(resolve(packageRoot, 'build-manifest.json'), 'utf8'));
assert.equal(build.sourceCommit, process.env.GITHUB_SHA, 'browser page package must match this exact workflow source SHA');
const mimeTypes = new Map([
  ['.html', 'text/html; charset=utf-8'], ['.js', 'text/javascript; charset=utf-8'],
  ['.mjs', 'text/javascript; charset=utf-8'], ['.json', 'application/json'],
  ['.wasm', 'application/wasm'], ['.gz', 'application/gzip'], ['.crg', 'application/octet-stream'],
  ['.png', 'image/png'], ['.jpg', 'image/jpeg'], ['.jpeg', 'image/jpeg'],
  ['.css', 'text/css; charset=utf-8'], ['.wav', 'audio/wav'], ['.mp3', 'audio/mpeg'],
]);
const server = createServer(async (request, response) => {
  try {
    const pathname = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
    const requested = resolve(packageRoot, `.${pathname}`);
    if (requested !== packageRoot && !requested.startsWith(packageRoot + sep)) {
      response.writeHead(403).end('forbidden');
      return;
    }
    const file = (await stat(requested)).isDirectory() ? resolve(requested, 'index.html') : requested;
    const body = await readFile(file);
    response.writeHead(200, {
      'Content-Type': mimeTypes.get(extname(file).toLowerCase()) ?? 'application/octet-stream',
      'Cache-Control': 'no-store',
      'Content-Length': body.byteLength,
    });
    response.end(body);
  } catch (error) {
    response.writeHead(error.code === 'ENOENT' ? 404 : 500).end(String(error));
  }
});
await new Promise((resolveListen, reject) => {
  server.once('error', reject);
  server.listen(0, '127.0.0.1', resolveListen);
});
const address = server.address();
const browser = await chromium.launch({ headless: true, args: ['--no-sandbox', '--enable-webgl', '--ignore-gpu-blocklist', '--use-gl=angle', '--use-angle=swiftshader'] });
const page = await browser.newPage({ viewport: { width: 1280, height: 720 }, deviceScaleFactor: 1 });
const pageErrors = [];
page.on('pageerror', error => pageErrors.push(String(error)));
let report;
try {
  await page.goto(`http://127.0.0.1:${address.port}/game/`, { waitUntil: 'domcontentloaded', timeout: 120000 });
  await page.waitForFunction(() => {
    const start = document.querySelector('#startOriginalMission');
    return (start && !start.disabled) || (!document.querySelector('#error')?.hidden);
  }, null, { timeout: 180000 });
  const startupError = await page.locator('#error').isVisible().catch(() => false);
  assert.equal(startupError, false, await page.locator('#errorDetails').textContent().catch(() => 'Mission page failed before the source load button became available'));
  await page.locator('#startOriginalMission').click();
  await page.waitForFunction(() => {
    const game = window.__gameState?.();
    return game?.loaded && game?.grounding?.stages?.synchronizedSource?.length === 3;
  }, null, { timeout: 900000 });
  const state = await page.evaluate(() => window.__gameState());
  assert.equal(state.originalInputActive, true, 'normal Mission must be ticking the original FullSource actor');
  assert.ok(state.grounding, 'production Mission frame must expose grounding diagnostics');
  const { stages, sourceRoot, contactRoot, projectedRoot, capsule, floorCandidates } = state.grounding;
  for (const [name, vector] of Object.entries({
    sourceBeforeContact: stages.sourceBeforeContact,
    contactBefore: stages.contactBefore,
    contactAfter: stages.contactAfter,
    synchronizedSource: stages.synchronizedSource,
    sourceRoot, contactRoot, projectedRoot,
  })) {
    assert.ok(Array.isArray(vector) && vector.length === 3 && vector.every(Number.isFinite), `finite three-axis Mission grounding sample required: ${name}`);
  }
  const near = (a, b) => a.every((value, index) => Math.abs(value - b[index]) < 1e-4);
  assert.ok(near(sourceRoot, stages.synchronizedSource), 'source root must report the post-contact synchronized source pose');
  assert.ok(near(contactRoot, stages.contactAfter), 'contact root must report the final fixed-step contact state');
  assert.ok(near(projectedRoot, sourceRoot), 'render hierarchy root must match the source pose projected for this frame');
  assert.ok(Number.isFinite(capsule.bottom) && Number.isFinite(capsule.verticalHalfExtent), 'authored capsule axis and lower bound must be reported');
  assert.ok(Array.isArray(floorCandidates), 'authored nearby floor candidates must be reported');
  assert.deepEqual(pageErrors, [], 'packaged Mission page must not throw browser runtime errors');
  const walkStart = state.playerX;
  await page.keyboard.down('KeyD');
  await page.waitForFunction(start => {
    const current = window.__gameState?.();
    return current?.originalInput?.right?.Pressed && current.playerX > start + 0.1;
  }, walkStart, { timeout: 30000 });
  await page.waitForFunction(() => ['Body walk', 'Body sprint'].includes(window.__gameState?.().animation), null, { timeout: 30000 });
  const moving = await page.evaluate(() => window.__gameState());
  await page.keyboard.up('KeyD');
  await page.waitForFunction(() => {
    const current = window.__gameState?.();
    return current && Math.abs(current.velocity) < 0.05 && current.animation === 'Body stand';
  }, null, { timeout: 30000 });
  const stopped = await page.evaluate(() => window.__gameState());
  assert.ok(moving.playerX > walkStart + 0.1, 'original Mission input must move the source actor through contact synchronization');
  assert.ok(['Body walk', 'Body sprint'].includes(moving.animation), 'source movement must select a recovered authored locomotion clip');
  assert.ok(Number.isFinite(stopped.playerX), 'released Mission actor must retain a finite source position');
  assert.equal(stopped.originalInput?.right?.Pressed, false, 'released browser input must reach the original source controller');
  assert.deepEqual(pageErrors, [], 'Mission walk/release must not throw browser runtime errors');
  await page.screenshot({ path: resolve(outputRoot, 'mission-grounding-walk.png'), fullPage: true });
  report = {
    status: 'passed',
    sourceCommit: build.sourceCommit,
    runtimePath: 'packaged /game/ → Start original Mission → verified FullSource EntryTeam actor → browser contact solver → projected Three.js hierarchy',
    browser: 'Chromium with SwiftShader WebGL',
    grounding: state.grounding,
    movement: {
      before: { x: walkStart, animation: state.animation },
      whileHeld: { x: moving.playerX, velocity: moving.velocity, animation: moving.animation, sourceInput: moving.originalInput?.right?.Pressed },
      released: { x: stopped.playerX, velocity: stopped.velocity, animation: stopped.animation },
    },
    sourceActors: window.__fullSourceMission?.session?.sourceActors ?? null,
    missionDiagnostics: window.__missionSourceDiagnostics?.summary ?? null,
    pageErrors,
    visualAcceptance: 'untested; screenshot retained for inspection; numeric hierarchy diagnostics do not prove pixel parity or correct foot contact',
  };
  await page.screenshot({ path: resolve(outputRoot, 'mission-grounding.png'), fullPage: true });
} catch (error) {
  report = {
    status: 'failed', sourceCommit: build.sourceCommit, error: String(error), pageErrors,
    pageError: await page.locator('#errorDetails').textContent().catch(() => null),
    missionDiagnostics: await page.evaluate(() => window.__missionSourceDiagnostics ?? null).catch(() => null),
    game: await page.evaluate(() => window.__gameState?.() ?? null).catch(() => null),
  };
  await page.screenshot({ path: resolve(outputRoot, 'mission-grounding-failure.png'), fullPage: true }).catch(() => {});
  throw error;
} finally {
  if (report) await import('node:fs/promises').then(fs => fs.writeFile(resolve(outputRoot, 'mission-grounding.json'), JSON.stringify(report, null, 2)));
  await browser.close();
  await new Promise(resolveClose => server.close(resolveClose));
}
console.log(JSON.stringify(report, null, 2));
