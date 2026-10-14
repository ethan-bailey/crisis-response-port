import { chromium } from "playwright";
import fs from "node:fs";

const browser = await chromium.launch({ headless: true, args: ["--no-sandbox", "--enable-webgl", "--ignore-gpu-blocklist", "--use-gl=angle", "--use-angle=swiftshader"] });
const page = await browser.newPage({ viewport: { width: 1280, height: 800 }, deviceScaleFactor: 1 });
const errors = [];
page.on("pageerror", error => errors.push(error.message));
page.on("console", message => { if (message.type() === "error") errors.push(message.text()); });
try {
  await page.goto("http://127.0.0.1:8000/index.html", { waitUntil: "networkidle", timeout: 60000 });
  await page.waitForFunction(() => window.__viewerStatus === "loaded" || window.__viewerStatus === "error", { timeout: 60000 });
  const before = await page.evaluate(() => ({
    status: window.__viewerStatus,
    stats: window.__viewerStats,
    position: window.__playerPosition?.(),
    canvas: !!document.querySelector("canvas"),
    canvasWidth: document.querySelector("canvas")?.width ?? 0,
    canvasHeight: document.querySelector("canvas")?.height ?? 0
  }));
  if (before.status === "loaded") {
    await page.locator("#forward").click();
    await page.waitForTimeout(150);
  }
  const after = await page.evaluate(() => window.__playerPosition?.());
  const moved = !!before.position && !!after && Math.hypot(after[0]-before.position[0], after[1]-before.position[1], after[2]-before.position[2]) > 0.001;
  await page.screenshot({ path: "character-movement-smoke.png", fullPage: true });
  const result = { ...before, afterPosition: after, moved, errors };
  fs.writeFileSync("character-movement-smoke.json", JSON.stringify(result, null, 2));
  console.log(JSON.stringify(result, null, 2));
  if (before.status !== "loaded" || !before.stats || before.stats.vertices < 1 || before.stats.triangles < 1) process.exitCode = 1;
  if (!before.canvas || before.canvasWidth < 1 || before.canvasHeight < 1) process.exitCode = 1;
  if (!moved) process.exitCode = 1;
  if (errors.length) process.exitCode = 1;
} finally {
  await browser.close();
}
