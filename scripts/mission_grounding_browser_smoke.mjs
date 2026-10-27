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
let injectedAssetFailure = true;
let injectedFailurePath = null;
const server = createServer(async (request, response) => {
  try {
    const pathname = decodeURIComponent(new URL(request.url, 'http://localhost').pathname);
    if (injectedAssetFailure && pathname.startsWith('/scene-data/fullsource/global-ui-mission-operator/assets/')) {
      injectedAssetFailure = false;
      injectedFailurePath = pathname;
      response.writeHead(503, { 'Content-Type': 'text/plain; charset=utf-8', 'Cache-Control': 'no-store' });
      response.end('Intentional one-time Mission asset failure for retry acceptance');
      return;
    }
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
    const button = document.querySelector('#startOriginalMission');
    const progress = document.querySelector('#missionProgress');
    return button?.disabled && progress && !progress.hidden;
  }, null, { timeout: 10000 });
  const initialAttemptUi = await page.evaluate(() => ({
    startDisabled: document.querySelector('#startOriginalMission')?.disabled,
    progressVisible: !document.querySelector('#missionProgress')?.hidden,
    progressText: document.querySelector('#missionProgressText')?.textContent,
  }));
  assert.equal(initialAttemptUi.startDisabled, true);
  assert.equal(initialAttemptUi.progressVisible, true);
  await page.waitForFunction(() => {
    const error = document.querySelector('#error');
    const retry = document.querySelector('#retryError');
    const game = window.__missionSourceDiagnostics;
    return error && !error.hidden && retry && !retry.hidden && game?.status === 'failed';
  }, null, { timeout: 30000 });
  const failedAttemptUi = await page.evaluate(() => ({
    startDisabled: document.querySelector('#startOriginalMission')?.disabled,
    progressVisible: !document.querySelector('#missionProgress')?.hidden,
    retryVisible: !document.querySelector('#retryError')?.hidden,
    errorVisible: !document.querySelector('#error')?.hidden,
    copyVisible: !document.querySelector('#copyError')?.hidden,
    errorText: document.querySelector('#errorDetails')?.textContent,
    diagnostics: window.__missionSourceDiagnostics?.error,
  }));
  assert.equal(failedAttemptUi.startDisabled, false, 'failed source fetch must return the Mission action to a retryable state');
  assert.equal(failedAttemptUi.progressVisible, false, 'failed source fetch must close the progress panel');
  assert.equal(failedAttemptUi.retryVisible, true, 'failed source fetch must expose the retry action');
  assert.equal(failedAttemptUi.errorVisible, true, 'failed source fetch must leave an accessible error panel');
  assert.equal(failedAttemptUi.copyVisible, true, 'failed source fetch must expose the error copy action');
  assert.match(failedAttemptUi.errorText, /HTTP 503/, 'the accessible error must preserve the concrete asset-fetch status');
  assert.ok(injectedFailurePath, 'the package server must have injected exactly one source-asset failure');
  await page.context().grantPermissions(['clipboard-read', 'clipboard-write'], { origin: `http://127.0.0.1:${address.port}` });
  await page.locator('#copyError').click();
  await page.waitForFunction(() => document.querySelector('#copyErrorStatus')?.textContent.includes('Copied'));
  const copiedErrorText = await page.evaluate(() => navigator.clipboard.readText());
  assert.equal(copiedErrorText, failedAttemptUi.errorText, 'Copy details must place the full visible HTTP failure diagnostic on the clipboard');
  await page.locator('#retryError').click();
  await page.waitForFunction(() => {
    const button = document.querySelector('#startOriginalMission');
    const progress = document.querySelector('#missionProgress');
    return button?.disabled && progress && !progress.hidden && document.querySelector('#error')?.hidden;
  }, null, { timeout: 10000 });
  const loadingUi = await page.evaluate(() => ({
    startDisabled: document.querySelector('#startOriginalMission')?.disabled,
    progressVisible: !document.querySelector('#missionProgress')?.hidden,
    progressText: document.querySelector('#missionProgressText')?.textContent,
  }));
  const loadStartedAt = Date.now();
  let playableSnapshot = null;
  while (Date.now() - loadStartedAt < 300000) {
    const checkpoint = await page.evaluate(() => {
      const game = window.__gameState?.();
      const trace = window.__missionSourceDiagnostics;
      return {
        ui: {
          progress: document.querySelector('#missionProgressText')?.textContent,
          status: document.querySelector('#status')?.textContent,
          error: document.querySelector('#errorDetails')?.textContent,
          errorVisible: Boolean(document.querySelector('#error') && !document.querySelector('#error').hidden),
        },
        diagnostics: trace && {
          status: trace.status, stage: trace.stage, progress: trace.progress,
          audio: trace.audio, error: trace.error,
          slowestFetches: trace.slowestFetches, slowestBridgeCalls: trace.slowestBridgeCalls,
        },
        game: game && {
          loaded: game.loaded, originalInputActive: game.originalInputActive,
          groundingStages: game.grounding?.stages ?? null, animation: game.animation,
          loadingTimings: game.loadingTimings,
        },
      };
    });
    if (checkpoint.game?.loaded && checkpoint.game.groundingStages?.synchronizedSource?.length === 3) {
      playableSnapshot = checkpoint;
      break;
    }
    assert.equal(checkpoint.ui.errorVisible, false,
      `Mission startup reached its visible runtime error panel before the playable state: ${checkpoint.ui.error}`);
    if (Date.now() - loadStartedAt >= 15000) {
      console.log(`Mission playable-state checkpoint +${Date.now() - loadStartedAt}ms ${JSON.stringify(checkpoint)}`);
    }
    await page.waitForTimeout(500);
  }
  assert.ok(playableSnapshot, `FullSource Mission did not reach a playable grounding frame within 300000ms; last state: ${JSON.stringify(await page.evaluate(() => ({progress: document.querySelector('#missionProgressText')?.textContent,status: document.querySelector('#status')?.textContent,diagnostics: window.__missionSourceDiagnostics,game: window.__gameState?.()})))}`);
  const state = await page.evaluate(() => window.__gameState());
  const loadedUi = await page.evaluate(() => ({
    startHidden: document.querySelector('#startOriginalMission')?.hidden,
    progressHidden: document.querySelector('#missionProgress')?.hidden,
    status: document.querySelector('#status')?.textContent,
    sourceTransfer: window.__missionSourceDiagnostics?.status,
  }));
  assert.equal(loadedUi.startHidden, true, 'Start original Mission must leave the UI after successful initialization');
  assert.equal(loadedUi.progressHidden, true, 'download progress must close only after Mission initialization');
  assert.equal(loadedUi.sourceTransfer, 'verified', 'Mission UI completion must follow verified source delivery');
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
  const movementStick = page.locator('#movementStick');
  const stickBox = await movementStick.boundingBox();
  assert.ok(stickBox && stickBox.width >= 176 && stickBox.height >= 176, 'packaged Mission keeps the enlarged thumb joystick');
  const stickGeometry = await movementStick.evaluate(surface => {
    const thumb = surface.querySelector('.stick-thumb');
    const rect = surface.getBoundingClientRect();
    return { rect: { left: rect.left, top: rect.top, width: rect.width, height: rect.height }, thumbWidth: thumb.offsetWidth, thumbHeight: thumb.offsetHeight };
  });
  const stickCenter = { x: stickGeometry.rect.left + stickGeometry.rect.width / 2, y: stickGeometry.rect.top + stickGeometry.rect.height / 2 };
  // The world-space aim point is shoulder-relative and can move between
  // evaluations as the authored source animation advances. Assert the stable
  // absolute aim direction instead of comparing that animated world position.
  const aimBeforeStick = await page.evaluate(() => window.__gameState().aimDirection);
  await page.mouse.move(stickCenter.x, stickCenter.y);
  await page.mouse.down();
  const aimOnStickDown = await page.evaluate(() => window.__gameState().aimDirection);
  assert.ok(near(aimOnStickDown, aimBeforeStick), 'touch-down on the movement stick must preserve the current absolute aim direction');
  await page.mouse.move(stickCenter.x, stickCenter.y - 24, { steps: 4 });
  await page.waitForFunction(y => Math.abs(window.__gameState().aimDirection[1] - y) > 0.01, aimBeforeStick[1], { timeout: 5000 });
  const verticalAim = await page.evaluate(() => window.__gameState().aimDirection);
  await page.mouse.move(stickCenter.x + 32, stickCenter.y - 24, { steps: 4 });
  await page.waitForFunction(y => Math.abs(window.__gameState().aimDirection[1] - y) < 0.01, verticalAim[1], { timeout: 5000 });
  const horizontalAim = await page.evaluate(() => window.__gameState().aimDirection);
  assert.ok(Math.abs(horizontalAim[1] - verticalAim[1]) < 0.01, 'horizontal joystick travel must leave the absolute vertical aim direction unchanged');
  await page.mouse.up();
  const releasedAim = await page.evaluate(() => window.__gameState().aimDirection);
  assert.ok(Math.abs(releasedAim[1] - verticalAim[1]) < 0.01, 'releasing the joystick must preserve the last absolute vertical aim direction');
  const travelX = (stickGeometry.rect.width - stickGeometry.thumbWidth) / 2 - 4;
  await page.mouse.move(stickCenter.x, stickCenter.y);
  await page.mouse.down();
  await page.mouse.move(stickCenter.x + travelX * 1.5, stickCenter.y, { steps: 4 });
  assert.equal(await movementStick.evaluate(surface => surface.classList.contains('sprinting')), false, 'the deliberate rim resistance must hold below the sprint wall');
  await page.mouse.move(stickCenter.x + travelX * 1.8, stickCenter.y, { steps: 4 });
  assert.equal(await movementStick.evaluate(surface => surface.classList.contains('sprinting')), true, 'pushing through the resistance wall must activate sprint');
  const sprintPuck = await movementStick.evaluate(surface => {
    const thumb = surface.querySelector('.stick-thumb');
    const x = Number(thumb.style.transform.match(/translate3d\(([-\d.]+)px/)?.[1] ?? 0);
    return { x, rim: surface.getBoundingClientRect().width / 2, radius: thumb.offsetWidth / 2 };
  });
  assert.ok(sprintPuck.x > sprintPuck.rim + sprintPuck.radius, 'the sprint puck must fully break through the outer rim');
  await page.mouse.up();
  assert.equal(await movementStick.evaluate(surface => surface.classList.contains('sprinting')), false, 'releasing the stick must clear the sprint wall state');
  // The pre-interaction state snapshot predates the joystick sprint test.
  // Capture the actual actor after sprint release, then walk back through the
  // clear spawn-side corridor so the authored movement check is not already
  // satisfied by the prior sprint or stalled against the counter collision.
  const walkStart = await page.evaluate(() => window.__gameState().playerX);
  await page.keyboard.down('KeyA');
  await page.waitForFunction(start => {
    const current = window.__gameState?.();
    return current?.originalInput?.left?.Pressed && current.playerX < start - 0.1;
  }, walkStart, { timeout: 30000 });
  await page.waitForFunction(() =>
    ['Body walk', 'Body walk backward', 'Body sprint'].includes(window.__gameState?.().animation),
    null, { timeout: 30000 });
  const moving = await page.evaluate(() => window.__gameState());
  await page.keyboard.up('KeyA');
  await page.waitForFunction(() => {
    const current = window.__gameState?.();
    return current && Math.abs(current.velocity) < 0.05 && current.animation === 'Body stand';
  }, null, { timeout: 30000 });
  const stopped = await page.evaluate(() => window.__gameState());
  assert.ok(moving.playerX < walkStart - 0.1, 'original Mission input must move the source actor through contact synchronization in the clear spawn-side corridor');
  assert.ok(['Body walk', 'Body walk backward', 'Body sprint'].includes(moving.animation), 'source movement must select a recovered authored forward, backward, or sprint locomotion clip');
  assert.ok(Number.isFinite(stopped.playerX), 'released Mission actor must retain a finite source position');
  assert.equal(stopped.originalInput?.left?.Pressed, false, 'released browser input must reach the original source controller');
  assert.deepEqual(pageErrors, [], 'Mission walk/release must not throw browser runtime errors');

  const reloadBefore = await page.evaluate(() => ({
    weapon: window.__gameState().weaponState,
    audio: window.__gameState().presentation.weaponAudio,
    eventCount: window.__missionSourceDiagnostics?.sourceAudioEvents?.length ?? 0,
    unavailable: document.querySelector('#magazineTouch')?.classList.contains('mag-unavailable'),
  }));
  assert.deepEqual(reloadBefore.weapon, {
    weaponId: 'M4A1', magazine: 30, magazineCapacity: 30, chamberLive: true, reserve: 30, reloading: false,
  }, 'FullSource HUD must use the recovered active M4A1 and reserve magazine graph');
  assert.equal(reloadBefore.unavailable, false, 'the source magazine control must accept its authored drag gesture');
  const magazineTouch = page.locator('#magazineTouch');
  const magazineBox = await magazineTouch.boundingBox();
  assert.ok(magazineBox, 'the source magazine drag control must be visible');
  const magazineStart = { x: magazineBox.x + magazineBox.width / 2, y: magazineBox.y + 16 };
  await page.mouse.move(magazineStart.x, magazineStart.y);
  await page.mouse.down();
  await page.mouse.move(magazineStart.x, magazineStart.y + 55, { steps: 5 });
  await page.mouse.up();
  const reloadBaseline = {
    played: reloadBefore.audio.played, eventCount: reloadBefore.eventCount,
    droppedUnloaded: reloadBefore.audio.droppedUnloaded, droppedSuspended: reloadBefore.audio.droppedSuspended,
    unmapped: reloadBefore.audio.unmappedSourceEvents,
  };
  await page.waitForFunction(({ played, eventCount, droppedUnloaded, droppedSuspended, unmapped }) => {
    const state = window.__gameState();
    const audio = state.presentation.weaponAudio;
    const emitted = window.__missionSourceDiagnostics?.sourceAudioEvents?.slice(eventCount) ?? [];
    return state.weaponState?.reloading === true &&
      emitted.some(event => typeof event?.sourcePath === 'string') &&
      audio.contextState === 'running' && audio.played > played &&
      audio.droppedUnloaded <= droppedUnloaded && audio.droppedSuspended <= droppedSuspended &&
      audio.unmappedSourceEvents <= unmapped;
  }, reloadBaseline, { timeout: 30000 });
  await page.waitForFunction(() => window.__gameState()?.weaponState?.reloading === false, null, { timeout: 30000 });
  const reloadAfter = await page.evaluate(() => ({
    weapon: window.__gameState().weaponState,
    audio: window.__gameState().presentation.weaponAudio,
    events: window.__missionSourceDiagnostics?.sourceAudioEvents ?? [],
  }));
  assert.equal(reloadAfter.weapon.weaponId, 'M4A1');
  assert.equal(reloadAfter.weapon.magazine, 30, 'the recovered active source magazine must remain populated after reload');
  assert.equal(reloadAfter.weapon.chamberLive, true, 'the original live chamber must remain reflected in the HUD state');
  assert.ok(reloadAfter.audio.played > reloadBefore.audio.played, 'the trusted magazine drag must play original source reload audio');
  assert.ok(reloadAfter.events.length > reloadBefore.eventCount, 'the browser must receive original source audio callbacks during reload');
  assert.deepEqual(pageErrors, [], 'source reload and audio callbacks must not throw browser runtime errors');
  const mobilePage = await browser.newPage({ viewport: { width: 390, height: 844 }, deviceScaleFactor: 2, isMobile: true, hasTouch: true });
  mobilePage.on('pageerror', error => pageErrors.push(String(error)));
  await mobilePage.goto(`http://127.0.0.1:${address.port}/game/`, { waitUntil: 'domcontentloaded', timeout: 120000 });
  await mobilePage.waitForSelector('#stage', { state: 'visible', timeout: 30000 });
  const mobileZoom = await mobilePage.evaluate(async () => {
    const before = window.visualViewport?.scale ?? null;
    const stage = document.querySelector('#stage').getBoundingClientRect();
    const point = { x: stage.left + stage.width * 0.5, y: stage.top + stage.height * 0.5 };
    return { before, point };
  });
  assert.equal(mobileZoom.before, 1, 'mobile packaged page must start at its authored viewport scale');
  await mobilePage.touchscreen.tap(mobileZoom.point.x, mobileZoom.point.y);
  await mobilePage.touchscreen.tap(mobileZoom.point.x, mobileZoom.point.y);
  await mobilePage.waitForTimeout(150);
  const mobileZoomAfter = await mobilePage.evaluate(() => window.visualViewport?.scale ?? null);
  assert.equal(mobileZoomAfter, 1, 'rapid taps over the actual game surface must not zoom the mobile page');
  await mobilePage.close();
  const runtimeReport = await page.evaluate(() => ({
    sourceActors: window.__fullSourceMission?.session?.sourceActors ?? null,
    missionDiagnostics: window.__missionSourceDiagnostics?.summary ?? null,
  }));
  await page.screenshot({ path: resolve(outputRoot, 'mission-grounding-walk.png'), fullPage: true });
  report = {
    status: 'passed',
    sourceCommit: build.sourceCommit,
    runtimePath: 'packaged /game/ → Start original Mission → verified FullSource EntryTeam actor → browser contact solver → projected Three.js hierarchy',
    browser: 'Chromium with SwiftShader WebGL',
    missionLoadUi: { initialAttempt: initialAttemptUi, failedAttempt: failedAttemptUi, retryLoading: loadingUi, completed: loadedUi },
    injectedFailure: { path: injectedFailurePath, initialAttemptFailed: true, diagnosticCopied: copiedErrorText === failedAttemptUi.errorText, retrySucceeded: true },
    grounding: state.grounding,
    reloadAudio: { before: reloadBefore, after: reloadAfter, sourceEvents: reloadAfter.events.slice(reloadBefore.eventCount) },
    movement: {
      before: { x: walkStart, animation: state.animation },
      whileHeld: { x: moving.playerX, velocity: moving.velocity, animation: moving.animation, sourceInput: moving.originalInput?.right?.Pressed },
      released: { x: stopped.playerX, velocity: stopped.velocity, animation: stopped.animation },
    },
    mobileZoom: { before: mobileZoom.before, afterRapidGameSurfaceTaps: mobileZoomAfter, browser: 'Chromium mobile touch emulation; not iPhone Safari' },
    ...runtimeReport,
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
  console.error('Packaged Mission browser failure report:', JSON.stringify(report));
  throw error;
} finally {
  if (report) await import('node:fs/promises').then(fs => fs.writeFile(resolve(outputRoot, 'mission-grounding.json'), JSON.stringify(report, null, 2)));
  await browser.close();
  await new Promise(resolveClose => server.close(resolveClose));
}
console.log(JSON.stringify(report, null, 2));
