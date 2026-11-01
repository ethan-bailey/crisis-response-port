import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { readFile, stat, mkdir } from 'node:fs/promises';
import { resolve, sep, extname } from 'node:path';
import { chromium } from '../game/node_modules/playwright/index.mjs';
import { compareSourceArmPoses } from '../game/source-arm-pose.mjs';

const packageRoot = resolve(process.argv[2] ?? '');
const outputRoot = resolve(process.argv[3] ?? '');
const reloadOnly=process.argv.includes('--reload-only');
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
// Keep the recovered camera FOV and scene untouched; the 20:9 frame brings the authored target just inside the view.
const page = await browser.newPage({ viewport: { width: 1600, height: 720 }, deviceScaleFactor: 1, hasTouch: true });
const pageErrors = [];
page.on('pageerror', error => pageErrors.push(String(error)));
let report;
let combatEvidence=null;
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
  const cameraFraming = {source:state.sourceCamera,rendered:state.projection};
  assert.ok(cameraFraming.source,'packaged FullSource state must expose the original InteriorCamera projection');
  assert.equal(typeof state.weaponState?.aimConverged,'boolean','packaged FullSource state must expose original actor aim convergence');
  assert.equal(cameraFraming.source.fov,75,'original Mission InteriorCamera must use the actor-framing FOV');
  assert.equal(cameraFraming.source.centerOffset,0.49,'original Mission camera must preserve its recovered default subject framing while pointer rays share its projection');
  assert.equal(cameraFraming.rendered.fov,cameraFraming.source.fov,'packaged renderer must use the original camera FOV so pointer rays match');
  assert.equal(cameraFraming.rendered.near,cameraFraming.source.near,'packaged renderer must share the original camera near plane');
  assert.equal(cameraFraming.rendered.far,cameraFraming.source.far,'packaged renderer must share the original camera far plane');
  assert.ok(Math.abs(cameraFraming.rendered.aspect-cameraFraming.source.aspect)<1e-5,'packaged renderer and original camera must share the viewport aspect');
  assert.ok(Math.abs(cameraFraming.rendered.position[0]-cameraFraming.source.position[0])<1e-4,'packaged renderer must follow the original camera X');
  assert.ok(Math.abs(cameraFraming.rendered.position[1]-cameraFraming.source.position[1])<1e-4,'packaged renderer must follow the original camera Y');
  assert.ok(Math.abs(cameraFraming.rendered.position[2]+cameraFraming.source.position[2])<1e-4,'packaged renderer must mirror the original camera Z into the browser world');
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
  let movementReport={skipped:true,reason:'independent reload-only acceptance'};
  if(!reloadOnly){
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
  const movementTrace = [];
  await page.keyboard.down('KeyA');
  const movementDeadline = Date.now() + 30000;
  let moving = null;
  while (Date.now() < movementDeadline) {
    const sample = await page.evaluate(() => {
      const current = window.__gameState?.();
      return current && {
        elapsedMs: Math.round(performance.now()), playerX: current.playerX, velocity: current.velocity,
        animation: current.animation, facingDirection: current.facingDirection,
        originalLeftPressed: current.originalInput?.left?.Pressed,
        sourceMovement: current.sourceMovement, sourcePhysicsCommands: current.sourcePhysicsCommands,
        physics: current.physics,
      };
    });
    if (sample && movementTrace.length < 8) movementTrace.push(sample);
    else if (sample) {
      movementTrace.push(sample);
      if (movementTrace.length > 24) movementTrace.splice(8, 1);
    }
    if (sample?.originalLeftPressed && sample.playerX < walkStart - 0.1) {
      moving = await page.evaluate(() => window.__gameState());
      break;
    }
    await page.waitForTimeout(250);
  }
  assert.ok(moving, `original Mission input did not produce leftward movement; start=${walkStart}; last source/controller/physics samples=${JSON.stringify(movementTrace)}`);
  try {
    await page.waitForFunction(() =>
      (() => {
        const current = window.__gameState?.();
        return current && current.animationDiagnostics?.animationInputs?.Moving === true &&
          ['Body walk', 'Body walk backward', 'Body sprint'].includes(current.animation);
      })(),
      null, { timeout: 30000 });
    moving = await page.evaluate(() => window.__gameState());
  } catch (error) {
    const trace = await page.evaluate(() => {
      const state = window.__gameState?.();
      return state && {
        sourceInput: state.originalInput,
        currentAnimation: state.animation,
        currentAnimationDiagnostics: state.animationDiagnostics,
        sourcePhysicsCommands: state.sourcePhysicsCommands,
        sourceAnimationTrace: state.sourceAnimationTrace,
      };
    });
    throw new Error(`${error}; leftStart=${walkStart}; displacementSample=${JSON.stringify(moving && {
      playerX: moving.playerX, velocity: moving.velocity, animation: moving.animation,
      sourceMovement: moving.sourceMovement, animationDiagnostics: moving.animationDiagnostics,
    })}; animationTrace=${JSON.stringify(trace)}`);
  }
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
  movementReport={
    before:{x:walkStart,animation:state.animation},
    whileHeld:{x:moving.playerX,velocity:moving.velocity,animation:moving.animation,sourceInput:moving.originalInput?.left?.Pressed,sourceMovement:moving.sourceMovement},
    released:{x:stopped.playerX,velocity:stopped.velocity,animation:stopped.animation},
  };
  }

  const reloadBefore = await page.evaluate(() => ({
    weapon: window.__gameState().weaponState,
    sourceArmPoses: window.__gameState().sourceArmPoses,
    audio: window.__gameState().presentation.weaponAudio,
    eventCount: window.__missionSourceDiagnostics?.sourceAudioEvents?.length ?? 0,
    unavailable: document.querySelector('#magazineTouch')?.classList.contains('mag-unavailable'),
  }));
  const reloadAmmunition = (({weaponId,magazine,magazineCapacity,chamberLive,reserve,reloading}) =>
    ({weaponId,magazine,magazineCapacity,chamberLive,reserve,reloading}))(reloadBefore.weapon);
  assert.deepEqual(reloadAmmunition, {
    weaponId: 'M4A1', magazine: 30, magazineCapacity: 30, chamberLive: true, reserve: 30, reloading: false,
  }, 'FullSource HUD must use the recovered active M4A1 and reserve magazine graph');
  assert.ok(Number.isFinite(reloadBefore.weapon.aimAlignment),'FullSource weapon snapshot must report finite original aim alignment');
  assert.equal(typeof reloadBefore.weapon.aimConverged,'boolean','FullSource weapon snapshot must report original arm/camera convergence');
  assert.ok(reloadBefore.sourceArmPoses.length >= 6, 'FullSource Mission must expose both original upper/lower-arm and hand pose chains');
  assert.equal(reloadBefore.unavailable, false, 'the source magazine control must accept its authored drag gesture');
  const fireButton = page.locator('#fire');
  const ammunitionBefore = { magazine: reloadBefore.weapon.magazine, reserve: reloadBefore.weapon.reserve };
  const fireBox = await fireButton.boundingBox();
  assert.ok(fireBox, 'the production fire control must be visible for the source trigger test');
  await page.mouse.move(fireBox.x + fireBox.width / 2, fireBox.y + fireBox.height / 2);
  let ammunitionSpent = { magazine: ammunitionBefore.magazine, reserve: ammunitionBefore.reserve, chamberLive: reloadBefore.weapon.chamberLive };
  const fireDeadline = Date.now() + 20000;
  while (Date.now() < fireDeadline) {
    await fireButton.click({ delay: 80 });
    await page.waitForTimeout(350);
    ammunitionSpent = await page.evaluate(() => ({
      magazine: window.__gameState().weaponState.magazine,
      reserve: window.__gameState().weaponState.reserve,
      chamberLive: window.__gameState().weaponState.chamberLive,
    }));
    if (ammunitionBefore.magazine - ammunitionSpent.magazine >= 2) break;
  }
  assert.ok(ammunitionSpent.magazine <= ammunitionBefore.magazine - 2,
    `repeated production fire-control presses must spend at least two magazine rounds; before=${JSON.stringify(ammunitionBefore)} after=${JSON.stringify(ammunitionSpent)}`);
  assert.equal(ammunitionSpent.reserve, ammunitionBefore.reserve,
    'firing from the loaded magazine must not consume reserve ammunition');
  const idleArmTrace = [];
  for (let sample = 0; sample < 6; sample++) {
    await page.waitForTimeout(50);
    idleArmTrace.push(await page.evaluate(() => window.__gameState().sourceArmPoses));
  }
  const idleArmDrift = idleArmTrace.map(sample => compareSourceArmPoses(reloadBefore.sourceArmPoses, sample))
    .reduce((best, value) => value.maximum > best.maximum ? value : best, { maximum: 0, matchedPaths: [], deltas: [] });
  const magazineTouch = page.locator('#magazineTouch');
  const magazineBox = await magazineTouch.boundingBox();
  assert.ok(magazineBox, 'the source magazine drag control must be visible');
  const reloadBaseline = await page.evaluate(() => {
    const state = window.__gameState();
    return {
      played: state.presentation.weaponAudio.played,
      eventCount: window.__missionSourceDiagnostics?.sourceAudioEvents?.length ?? 0,
      droppedUnloaded: state.presentation.weaponAudio.droppedUnloaded,
      droppedSuspended: state.presentation.weaponAudio.droppedSuspended,
      unmapped: state.presentation.weaponAudio.unmappedSourceEvents,
    };
  });
  const magazineStart = { x: magazineBox.x + magazineBox.width / 2, y: magazineBox.y + 16 };
  await page.mouse.move(magazineStart.x, magazineStart.y);
  await page.mouse.down();
  await page.mouse.move(magazineStart.x, magazineStart.y + 55, { steps: 5 });
  await page.mouse.up();
  const reloadArmTrace = [];
  const reloadDeadline = Date.now() + 30000;
  let reloadPoseStarted = false;
  while (Date.now() < reloadDeadline) {
    const sample = await page.evaluate(() => {
      const state = window.__gameState();
      return { reloading: state.weaponState?.reloading === true, sourceArmPoses: state.sourceArmPoses };
    });
    if (sample.reloading) {
      reloadPoseStarted = true;
      reloadArmTrace.push(sample.sourceArmPoses);
    } else if (reloadPoseStarted) break;
    await page.waitForTimeout(25);
  }
  assert.ok(reloadPoseStarted && reloadArmTrace.length > 0,
    `real magazine drag did not enter the source reload task; arm trace=${JSON.stringify(reloadArmTrace)}`);
  const reloadPoseChange = reloadArmTrace.map(sample => compareSourceArmPoses(reloadBefore.sourceArmPoses, sample))
    .reduce((best, value) => value.maximum > best.maximum ? value : best, { maximum: 0, matchedPaths: [], deltas: [] });
  assert.ok(reloadPoseChange.maximum > idleArmDrift.maximum,
    `original reload task did not move a source arm/hand beyond observed idle drift; idle=${JSON.stringify(idleArmDrift)} reload=${JSON.stringify(reloadPoseChange)}`);
  if (reloadOnly) {
    await page.waitForFunction(({ played, eventCount, droppedUnloaded, droppedSuspended, unmapped }) => {
      const state = window.__gameState();
      const audio = state.presentation.weaponAudio;
      const emitted = window.__missionSourceDiagnostics?.sourceAudioEvents?.slice(eventCount) ?? [];
      return emitted.some(event => typeof event?.sourcePath === 'string') &&
        audio.contextState === 'running' && audio.played > played &&
        audio.droppedUnloaded <= droppedUnloaded && audio.droppedSuspended <= droppedSuspended &&
        audio.unmappedSourceEvents <= unmapped;
    }, reloadBaseline, { timeout: 30000 });
  }
  await page.waitForFunction(() => window.__gameState()?.weaponState?.reloading === false, null, { timeout: 30000 });
  const reloadAfter = await page.evaluate(() => ({
    weapon: window.__gameState().weaponState,
    sourceArmPoses: window.__gameState().sourceArmPoses,
    audio: window.__gameState().presentation.weaponAudio,
    events: window.__missionSourceDiagnostics?.sourceAudioEvents ?? [],
  }));
  assert.equal(reloadAfter.weapon.weaponId, 'M4A1');
  assert.equal(reloadAfter.weapon.magazine, 30, 'the recovered active source magazine must be replenished after reload');
  assert.ok(reloadAfter.weapon.magazine > ammunitionSpent.magazine,
    `reload must increase the loaded magazine after rounds were spent; before=${JSON.stringify(ammunitionSpent)} after=${JSON.stringify({magazine:reloadAfter.weapon.magazine,reserve:reloadAfter.weapon.reserve})}`);
  assert.ok(reloadAfter.weapon.reserve < ammunitionSpent.reserve,
    `reload must consume reserve ammunition after rounds were spent; before=${JSON.stringify(ammunitionSpent)} after=${JSON.stringify({magazine:reloadAfter.weapon.magazine,reserve:reloadAfter.weapon.reserve})}`);
  assert.equal(reloadAfter.weapon.chamberLive, true, 'the original live chamber must remain reflected in the HUD state');
  if (reloadOnly) {
    assert.ok(reloadAfter.audio.played > reloadBaseline.played, 'the trusted magazine drag must play original source reload audio');
    assert.ok(reloadAfter.events.length > reloadBaseline.eventCount, 'the browser must receive original source audio callbacks during reload');
  }
  assert.deepEqual(pageErrors, [], 'source reload and audio callbacks must not throw browser runtime errors');
  let combatReport={skipped:true,reason:'reload-only acceptance'};
  if(!reloadOnly){
    let combatBefore=await page.evaluate(()=>window.__sourceCombatSnapshot());
    assert.ok(combatBefore.shotsFired>=ammunitionBefore.magazine-ammunitionSpent.magazine,
      `combat snapshot must observe the source Operator after its real fire-control presses: ${JSON.stringify(combatBefore)}`);
    const exteriorVisibility=await page.evaluate(()=>window.__setSourceExteriorWallsHidden(true));
    assert.equal(exteriorVisibility.disableExteriorWalls,true,
      'the combat view must use the original Mission Extra that disables exterior walls');
    assert.ok(exteriorVisibility.matchingRoomExteriorHierarchies>0,
      `the source Room event must deactivate its authored Exterior hierarchies: ${JSON.stringify(exteriorVisibility)}`);
    const viewport=await page.evaluate(()=>({width:innerWidth,height:innerHeight}));
    const operatorFrame=await page.evaluate(()=>{
      const root=window.__gameState().grounding.sourceRoot;
      return window.__sourceCombatScreenPoint([root[0],root[1]+0.8,root[2]]);
    });
    const occupantTargets=combatBefore.occupants.filter(victim=>victim.alive)
      .flatMap(victim=>victim.limbs.map(limb=>({body:victim.body,limb:limb.limb,aimPoint:limb.aimPoint})));
    const occupantFrames=await page.evaluate(targets=>targets.map(target=>({
      body:target.body,limb:target.limb,...window.__sourceCombatScreenPoint(target.aimPoint),
    })),occupantTargets);
    const visibleOccupantFrames=occupantFrames.filter(point=>point.inside&&point.visible);
    visibleOccupantFrames.sort((a,b)=>Math.hypot(a.x-viewport.width/2,a.y-viewport.height/2)-
      Math.hypot(b.x-viewport.width/2,b.y-viewport.height/2));
    const cameraFrame={operator:operatorFrame,occupant:visibleOccupantFrames[0]??null,
      visibleOccupants:visibleOccupantFrames.length,limbFrames:occupantFrames.length};
    assert.ok(cameraFrame.operator.inside,
      `the synchronized source camera must frame the original Operator: ${JSON.stringify(cameraFrame)}`);
    // Initial AI/scene motion can leave every un-staged Occupant behind the combined mesh.
    // The staged combat loop below independently requires a visible Torso point before each tap.
    // Projectile traces showed the starting muzzle overlaps the entrance fixture
    // cluster. Clear it in short production joystick pulses and sample after each
    // release so the smoke cannot keep driving the source actor past the doorway.
    const combatLane={attempted:false,reason:'move the source actor into the contract firing lane',pulses:[]};
    const laneStart=await page.evaluate(()=>window.__gameState().grounding.sourceRoot);
    if(laneStart[0]<-0.1){
      combatLane.attempted=true;
      Object.assign(combatLane,{startX:laneStart[0],targetX:4,control:'production movement joystick sprint',direction:1});
      const movementStick=page.locator('#movementStick');
      const stickBox=await movementStick.boundingBox();
      assert.ok(stickBox,'combat lane clearance requires the production movement joystick');
      const geometry=await movementStick.evaluate(surface=>{
        const thumb=surface.querySelector('.stick-thumb');
        const rect=surface.getBoundingClientRect();
        return {center:{x:rect.left+rect.width/2,y:rect.top+rect.height/2},
          travelX:(rect.width-thumb.offsetWidth)/2-4};
      });
      let position=laneStart;
      for(let pulse=0;pulse<18&&position[0]<combatLane.targetX;pulse++){
        await page.mouse.move(geometry.center.x,geometry.center.y);
        await page.mouse.down();
        try{
          await page.mouse.move(geometry.center.x+geometry.travelX*1.8,geometry.center.y,{steps:2});
          await page.waitForFunction(()=>document.querySelector('#movementStick')?.classList.contains('sprinting'),
            null,{timeout:5000});
          await page.waitForTimeout(180);
        }finally{
          await page.mouse.up();
        }
        await page.waitForTimeout(250);
        position=await page.evaluate(()=>window.__gameState().grounding.sourceRoot);
        combatLane.pulses.push({pulse: pulse+1,sourceX:position[0],sourceY:position[1]});
        combatLane.endX=position[0];
        if(position[0]>4.5)break;
      }
      assert.ok(combatLane.endX>=combatLane.targetX-0.2,
        `production movement must clear the entrance fixture cluster before source firing: ${JSON.stringify(combatLane)}`);
      const released=await page.evaluate(()=>window.__gameState());
      assert.equal(released.originalInput?.right?.Pressed,false,
        'the production sprint pulse must release the original source movement key before firing');
      combatLane.released={x:released.playerX,y:released.playerY,velocity:released.velocity,
        animation:released.animation,rightPressed:released.originalInput?.right?.Pressed};
    }
    assert.ok(combatBefore.occupants.some(victim=>victim.alive&&victim.limbs.some(limb=>limb.limb==='Torso')),
      'packaged FullSource Mission must expose live serialized Occupant Torso limbs');
    const attempts=[];
    const acceptedHits=[];
    const failures=[];
    for(let shotNumber=1;shotNumber<=2;shotNumber++){
      const scenario=await page.evaluate(number=>window.__stageSourceProjectileContractScenario(number),shotNumber);
      await page.waitForTimeout(500);
      const beforeSnapshot=await page.evaluate(()=>window.__sourceCombatSnapshot());
      const beforeVictim=beforeSnapshot.occupants.find(victim=>victim.body===scenario.target);
      const beforeTorso=beforeVictim?.limbs.find(limb=>limb.limb==='Torso');
      const shotsBefore=beforeSnapshot.shotsFired;
      let point=null,projection=null,aimStateAtFire=null,lastAimState=null,aimDistance=Infinity,fireInputObserved=false,shotObserved=false;
      if(beforeVictim?.alive&&beforeTorso){
        point=await page.evaluate(position=>window.__sourceCombatScreenPoint(position),beforeTorso.aimPoint);
        const aimGenerationBefore=await page.evaluate(()=>window.__gameState().aimGeneration);
        const aimDeadline=Date.now()+5000;
        while(Date.now()<aimDeadline&&point?.inside&&point.visible){
          projection=await page.evaluate(position=>({point:window.__sourceCombatScreenPoint(position),
            camera:window.__gameState().sourceCamera}),beforeTorso.aimPoint);
          point=projection.point;
          if(!point.inside||!point.visible)break;
          await page.mouse.move(point.x,point.y);
          const state=await page.evaluate(()=>window.__gameState());
          const sourcePoint=state.weaponState?.sourceAimPoint;
          aimDistance=sourcePoint?Math.hypot(sourcePoint[0]-beforeTorso.aimPoint[0],
            sourcePoint[1]-beforeTorso.aimPoint[1],sourcePoint[2]-beforeTorso.aimPoint[2]):Infinity;
          lastAimState={aimGeneration:state.aimGeneration,sourceAimAppliedGeneration:state.sourceAimAppliedGeneration,
            aimPoint:state.aimPoint,sourceAimPoint:sourcePoint,sourcePointer:state.weaponState?.sourcePointer,
            sourceCamera:state.sourceCamera,aimAlignment:state.weaponState?.aimAlignment,
            aimConverged:state.weaponState?.aimConverged,aimDistance};
          if(state.aimGeneration>aimGenerationBefore&&state.sourceAimAppliedGeneration===state.aimGeneration&&
            state.weaponState?.aimConverged===true&&aimDistance<=0.55){aimStateAtFire={...lastAimState};break;}
          await page.waitForTimeout(50);
        }
      }
      if(aimStateAtFire){
        await page.mouse.move(point.x,point.y);
        await page.mouse.down();
        try{
          await page.waitForFunction(()=>window.__gameState().originalInput?.fire?.Pressed===true,
            null,{timeout:2000});
          fireInputObserved=true;
        }catch{}
        finally{await page.mouse.up();}
        const shotDeadline=Date.now()+3000;
        while(Date.now()<shotDeadline){
          await page.waitForTimeout(50);
          const current=await page.evaluate(()=>window.__sourceCombatSnapshot());
          if(current.shotsFired>shotsBefore){shotObserved=true;break;}
        }
      }
      let afterSnapshot=await page.evaluate(()=>window.__sourceCombatSnapshot());
      let changedLimb=null;
      const impactDeadline=Date.now()+3000;
      while(shotObserved&&Date.now()<impactDeadline){
        afterSnapshot=await page.evaluate(()=>window.__sourceCombatSnapshot());
        const afterVictim=afterSnapshot.occupants.find(victim=>victim.body===scenario.target);
        changedLimb=afterVictim?.limbs.find(limb=>limb.limb==='Torso'&&
          (limb.wounds>beforeTorso.wounds||limb.woundCavityVolume>beforeTorso.woundCavityVolume||
           limb.damagedTissueVolume>beforeTorso.damagedTissueVolume))??null;
        if(changedLimb)break;
        await page.waitForTimeout(100);
      }
      const afterVictim=afterSnapshot.occupants.find(victim=>victim.body===scenario.target);
      const attempt={shotNumber,stagedScenario:scenario,victim:scenario.target,aimedLimb:'Torso',
        aimPoint:beforeTorso?.aimPoint??scenario.aimPoint,screenPoint:point,cameraAtProjection:projection?.camera,
        aimStateAtFire,lastAimState,aimDistance,fireInputObserved,shotObserved,shotsBefore,shotsAfter:afterSnapshot.shotsFired,
        lastHitLimb:afterVictim?.lastHitLimb??null,damagedLimb:changedLimb,
        projectiles:afterSnapshot.projectiles,spawnedProjectiles:afterSnapshot.spawnedProjectiles,
        projectileInteractions:afterSnapshot.projectileInteractions};
      attempts.push(attempt);
      if(!(scenario.shooterPosition?.[0]>3.5&&scenario.shooterPosition?.[0]<4.6))failures.push(`shot ${shotNumber}: production-moved shooter X expected near source contract lane 4, observed ${scenario.shooterPosition?.[0]}`);
      if(Math.abs(scenario.shooterPosition?.[1]-0.94)>0.01)failures.push(`shot ${shotNumber}: grounded shooter Y expected near 0.94 within solver tolerance, observed ${scenario.shooterPosition?.[1]}`);
      if(scenario.kneeling!==true)failures.push(`shot ${shotNumber}: staged shooter kneeling expected true, observed ${scenario.kneeling}`);
      if(Math.abs(scenario.targetPosition?.[0]-17.74)>0.1)failures.push(`shot ${shotNumber}: visible Torso lane X expected 17.74, observed ${scenario.targetPosition?.[0]}`);
      if(scenario.aimedLimb!=='Torso')failures.push(`shot ${shotNumber}: aimed limb expected Torso, observed ${scenario.aimedLimb}`);
      if(!shotObserved)failures.push(`shot ${shotNumber}: no production trigger round observed for staged Torso ${scenario.target}`);
      if(attempt.shotsAfter-attempt.shotsBefore!==1)failures.push(`shot ${shotNumber}: exactly one production round expected, observed ${attempt.shotsAfter-attempt.shotsBefore}`);
      if(attempt.damagedLimb?.limb!=='Torso')failures.push(`shot ${shotNumber}: callback limb expected Torso, observed ${attempt.damagedLimb?.limb??'none'}`);
      if(changedLimb)acceptedHits.push({body:scenario.target,limb:'Torso',aimedLimb:'Torso',
        damageBefore:beforeTorso,damageAfter:changedLimb,physiologyBefore:beforeVictim?.physiology,
        physiologyAfter:afterVictim?.physiology,sourceDeltaTime:afterSnapshot.sourceDeltaTime,
        shotsBefore,shotsAfter:afterSnapshot.shotsFired});
    }
    const distinctHitBodies=new Set(acceptedHits.map(hit=>hit.body));
    if(distinctHitBodies.size!==2)failures.push(`two distinct Occupant Torso callbacks expected, observed ${distinctHitBodies.size}`);
    for(const hit of acceptedHits){
      if(!(hit.damageAfter.wounds>hit.damageBefore.wounds))failures.push(`${hit.body} Torso wound count expected to increase, observed ${hit.damageBefore.wounds}→${hit.damageAfter.wounds}`);
      if(!(hit.damageAfter.woundCavityVolume>hit.damageBefore.woundCavityVolume))failures.push(`${hit.body} Torso cavity expected to increase, observed ${hit.damageBefore.woundCavityVolume}→${hit.damageAfter.woundCavityVolume}`);
      if(!(hit.damageAfter.damagedTissueVolume>hit.damageBefore.damagedTissueVolume))failures.push(`${hit.body} Torso damaged tissue expected to increase, observed ${hit.damageBefore.damagedTissueVolume}→${hit.damageAfter.damagedTissueVolume}`);
      if(!(hit.physiologyAfter?.shock>hit.physiologyBefore?.shock))failures.push(`${hit.body} shock expected to increase, observed ${hit.physiologyBefore?.shock}→${hit.physiologyAfter?.shock}`);
    }
    const postHitRuntimeUi = await page.evaluate(() => ({
      errorVisible: Boolean(document.querySelector('#error') && !document.querySelector('#error').hidden),
      errorText: document.querySelector('#errorDetails')?.textContent ?? '',
    }));
    if(postHitRuntimeUi.errorVisible)failures.push(`runtime error dialog visible: ${postHitRuntimeUi.errorText.slice(0,1200)}`);
    if(!attempts.at(-1)?.projectiles?.every(projectile => projectile.status !== 'Error'))failures.push(`last original projectile entered Error status: ${JSON.stringify(attempts.at(-1)?.projectiles)}`);
    const projectileCleanupDeadline=Date.now()+5000;
    let cleanupSnapshot=await page.evaluate(()=>window.__sourceCombatSnapshot());
    while(Date.now()<projectileCleanupDeadline&&cleanupSnapshot.projectiles.some(projectile=>projectile.owner===cleanupSnapshot.shooter)){
      await page.waitForTimeout(100);
      cleanupSnapshot=await page.evaluate(()=>window.__sourceCombatSnapshot());
    }
    const ownedProjectilesAfterCleanup=cleanupSnapshot.projectiles.filter(projectile=>projectile.owner===cleanupSnapshot.shooter);
    if(ownedProjectilesAfterCleanup.length!==0)failures.push(`source projectile cleanup expected 0 active owned projectiles, observed ${ownedProjectilesAfterCleanup.length}`);
    if(pageErrors.length)failures.push(`browser page errors expected none, observed ${JSON.stringify(pageErrors)}`);
    const firstDivergingLink=attempts.some(attempt=>!attempt.shotObserved||attempt.shotsAfter-attempt.shotsBefore!==1||attempt.damagedLimb?.limb!=='Torso')?'hitCallback':
      acceptedHits.some(hit=>!(hit.damageAfter.wounds>hit.damageBefore.wounds))?'woundState':
      acceptedHits.some(hit=>!(hit.damageAfter.damagedTissueVolume>hit.damageBefore.damagedTissueVolume))?'tissue':
      acceptedHits.some(hit=>!(hit.physiologyAfter?.shock>hit.physiologyBefore?.shock))?'shock':
      ownedProjectilesAfterCleanup.length!==0?'cleanup':failures.length?'assertion':null;
    combatReport={status:failures.length?'failed':'passed',failures,firstDivergingLink,shotsBefore:combatBefore.shotsFired,shotsAfter:attempts.at(-1)?.shotsAfter??combatBefore.shotsFired,occupantsBefore:combatBefore.occupants.length,exteriorVisibility,cameraFrame,combatLane,attempts,hits:acceptedHits,chainComparison:{
      hitCallback:{contractExpected:'one production trigger call per staged target; original projectile callback must hit the selected Occupant Torso',packagedObserved:attempts.map(attempt=>({body:attempt.stagedScenario?.target,aimedLimb:attempt.stagedScenario?.aimedLimb,callbackLimb:attempt.damagedLimb?.limb??null,shots:attempt.shotsAfter-attempt.shotsBefore}))},
      woundState:{contractExpected:'source projectile contract: each hit limb wound count increases; torso-profile acceptance requires +1 per Torso hit',packagedObserved:acceptedHits.map(hit=>({body:hit.body,limb:hit.limb,before:hit.damageBefore.wounds,after:hit.damageAfter.wounds}))},
      tissue:{contractExpected:'source projectile contract: each hit limb damaged tissue increases',packagedObserved:acceptedHits.map(hit=>({body:hit.body,limb:hit.limb,before:hit.damageBefore.damagedTissueVolume,after:hit.damageAfter.damagedTissueVolume}))},
      shock:{contractExpected:'source projectile contract captures body shock; torso-profile acceptance requires shock to increase on each hit',packagedObserved:acceptedHits.map(hit=>({body:hit.body,before:hit.physiologyBefore?.shock,after:hit.physiologyAfter?.shock}))},
      bleeding:{contractExpected:{torsoHit:'injury contract asserts increased cavity/tissue, not a fixed hemorrhage amount',controlledPhysiologyFixture:{cavityCapMl:500,pressure:.06503155,deltaTimeSeconds:.1,internalBleedMlPerTick:3.2515774,stopHemorrhage:{cavityMl:0,internalBloodMl:0,hemorrhagingMlPerSecond:0}}},packagedObserved:acceptedHits.map(hit=>({body:hit.body,limb:hit.limb,cavityMl:hit.damageAfter.woundCavityVolume,internalBloodMl:hit.damageAfter.internalBloodVolume,internalHemorrhagingMlPerSecond:hit.damageAfter.internalHemorrhaging,hemorrhagingMlPerSecond:hit.damageAfter.hemorrhaging,localPressure:hit.damageAfter.localPressure,sourceDeltaTime:hit.sourceDeltaTime,expectedCurrentTickMl:hit.damageAfter.woundCavityVolume*hit.damageAfter.localPressure*hit.sourceDeltaTime}))},
      oxygenAndDeath:{contractExpected:{controlledHypoxiaFixture:{drainedBloodMl:4852,bloodOxygenAt100ms:'<=0.5',conscious:false,ragdoll:true,brainTraumaAt3200ms:'>0',aliveAt30200ms:false,eventOrder:['ragdoll','unconscious','death']}},packagedObserved:acceptedHits.map(hit=>({body:hit.body,bloodVolumeMl:hit.physiologyAfter?.bloodVolume,bloodOxygen:hit.physiologyAfter?.bloodOxygen,alive:hit.physiologyAfter?.alive,conscious:hit.physiologyAfter?.conscious,ragdoll:hit.physiologyAfter?.ragdoll})),exercisedByTorsoScenario:false},
      missionOutcome:{contractExpected:'MissionOutcomeContract asserts ended=true and success=true after all rooms are checked and all Occupants are restrained',packagedObserved:cleanupSnapshot.missionOutcome,exercisedByTorsoScenario:false},
      cleanup:{contractExpected:'zero source-owned projectiles after original source termination',packagedObserved:{activeOwnedProjectiles:ownedProjectilesAfterCleanup.length,shotsAfter:cleanupSnapshot.shotsFired}},
      nativePhysXIntegration:{status:'open',evidenceRequired:'source ragdoll Rigidbody/HingeJoint actors step in the packaged runtime and match a Unity 2017.2 reference',packagedObservedBackend:'browser PhysX 5.6.1; nativeUnityPhysXVerified=false',blocker:'the package has a browser PhysX 5.6.1 adapter, but no Unity 2017.2 native solver or reference trajectory/contact callback is exercised'},
      woundPixels:{status:'open',evidenceRequired:'before/after affected Torso UV or pixel-region comparison on the packaged renderer',packagedObserved:'injury state and full-page screenshot are captured; no Torso UV/pixel delta is asserted',blocker:'the current harness lacks a serialized-UV wound-pixel readback/comparison; source state change alone cannot establish rendered wound pixels'}
    },sourceProjectileAndPhysiologyCallbacks:acceptedHits.length>0,nativePhysXIntegrationVerified:false,woundPixelsVerified:false};
    combatEvidence=combatReport;
    assert.equal(ownedProjectilesAfterCleanup.length,0,`source projectile cleanup expected 0 active owned projectiles, observed ${ownedProjectilesAfterCleanup.length}`);
    assert.deepEqual(pageErrors, [], 'packaged FullSource projectile hit and injury callbacks must not throw browser runtime errors');
    assert.deepEqual(failures,[],`packaged torso scenario diverged from source contract: ${JSON.stringify(failures)}`);
  }
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
    cameraFraming,
    reloadAudio: { before: reloadBefore, ammunitionBefore: ammunitionBefore, afterSpending: ammunitionSpent, reloadAudioBaseline: reloadBaseline, afterReload: { magazine: reloadAfter.weapon.magazine, reserve: reloadAfter.weapon.reserve, chamberLive: reloadAfter.weapon.chamberLive }, after: reloadAfter, sourceEvents: reloadAfter.events.slice(reloadBaseline.eventCount), armPose: { idleDrift: idleArmDrift, activeReload: reloadPoseChange, settled: compareSourceArmPoses(reloadBefore.sourceArmPoses, reloadAfter.sourceArmPoses), sampleCount: reloadArmTrace.length } },
    combat: combatReport,
    movement: movementReport,
    mobileZoom: { before: mobileZoom.before, afterRapidGameSurfaceTaps: mobileZoomAfter, browser: 'Chromium mobile touch emulation; not iPhone Safari' },
    ...runtimeReport,
    pageErrors,
    visualAcceptance: 'untested; screenshot retained for inspection; numeric hierarchy diagnostics do not prove pixel parity or correct foot contact',
  };
  await page.screenshot({ path: resolve(outputRoot, 'mission-grounding.png'), fullPage: true });
} catch (error) {
  report = {
    status: 'failed', sourceCommit: build.sourceCommit, error: String(error), pageErrors,
    combat:combatEvidence,
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
