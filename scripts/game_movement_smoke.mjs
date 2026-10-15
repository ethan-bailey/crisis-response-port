import { chromium } from "playwright";
import fs from "node:fs";

const browser = await chromium.launch({ headless: true, args: ["--no-sandbox", "--enable-webgl", "--ignore-gpu-blocklist", "--use-gl=angle", "--use-angle=swiftshader"] });
const page = await browser.newPage({ viewport: { width: 1280, height: 800 }, deviceScaleFactor: 1 });
const errors = [];
page.on("pageerror", e => errors.push(e.message));
page.on("console", m => { if (m.type() === "error") errors.push(m.text()); });
try {
  await page.goto("http://127.0.0.1:8000/game/", { waitUntil: "networkidle", timeout: 90000 });
  await page.waitForFunction(() => window.__gameState?.().loaded && window.__gameState?.().weaponsLoaded || document.querySelector("#error")?.style.display === "block", { timeout: 90000 });
  const before = await page.evaluate(() => window.__gameState?.() ?? null);
  if (!before?.loaded) throw new Error("Game runtime did not finish loading: " + (await page.locator("#error").textContent()));
  await page.mouse.move(1000,220);
  await page.waitForFunction(()=>window.__gameState().facingDirection===1);
  const rightBox = await page.locator("#right").boundingBox();
  await page.mouse.move(rightBox.x + rightBox.width / 2, rightBox.y + rightBox.height / 2);
  await page.evaluate(() => {
    window.__poseTrace=[];
    function capture(){const s=window.__gameState?.();if(s?.loaded)window.__poseTrace.push({x:s.playerX,pelvis:s.pelvisLocalPosition,animation:s.animation});if(window.__poseTrace.length<500)requestAnimationFrame(capture);}
    requestAnimationFrame(capture);
  });
  await page.mouse.down();
  // Software-rendered CI may produce only a few frames in 700ms. Wait for the
  // observed movement threshold instead of assuming a wall-clock frame rate.
  await page.waitForFunction(x => window.__gameState().playerX > x + 0.05, before.playerX, { timeout: 30000 });
  const whileHeld = await page.evaluate(() => window.__gameState());
  await page.mouse.up();
  await page.waitForFunction(() => window.__gameState?.().originalInput?.right.Pressed === false);
  const afterMove = await page.evaluate(() => window.__gameState?.() ?? null);
  const moved = !!afterMove && afterMove.playerX > before.playerX + 0.05;
  await page.waitForFunction(() => Math.abs(window.__gameState().velocity) < .01, null, { timeout: 90000 });
  const stopped = await page.evaluate(() => window.__gameState());
  await page.waitForFunction(t => window.__gameState().physicsTime > t + .5, stopped.physicsTime, { timeout: 90000 });
  const poseTrace=await page.evaluate(() => window.__poseTrace);
  const walkSamples=poseTrace.filter(s=>s.animation==='Body walk');
  const poseTravelStable=walkSamples.length>0&&walkSamples.every(s=>Math.abs(s.pelvis[0])<1e-6);
  const after = await page.evaluate(() => window.__gameState?.() ?? null);
  const originalInputActive = !!after?.originalInputActive;
  const weaponActionsPending = await page.locator("#fire").isDisabled();
  await page.screenshot({ path: "mission01-game-smoke.png", fullPage: true });
  const result = { before, afterMove, after, moved, originalInputActive, weaponActionsPending, stopped, whileHeld, poseTrace, poseTravelStable, errors };
  fs.writeFileSync("mission01-game-smoke.json", JSON.stringify(result, null, 2));
  console.log(JSON.stringify(result, null, 2));
  if (!before.sceneMesh || !before.weaponsLoaded || before.limbCount < 10 || !moved || whileHeld.animation !== "Body walk" || Math.abs(after.playerX-stopped.playerX) > .005 || Math.abs(after.velocity) > .01 || !poseTravelStable || !originalInputActive || !weaponActionsPending || errors.length) process.exitCode = 1;
} finally {
  await browser.close();
}

