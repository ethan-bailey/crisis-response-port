import { chromium } from "playwright";
import fs from "node:fs";

const browser = await chromium.launch({ headless: true, args: ["--no-sandbox", "--enable-webgl", "--ignore-gpu-blocklist", "--use-gl=angle", "--use-angle=swiftshader"] });
const page = await browser.newPage({ viewport: { width: 1280, height: 800 }, deviceScaleFactor: 1 });
const errors = [];
page.on("pageerror", error => errors.push(error.message));
page.on("console", message => { if (message.type() === "error") errors.push(message.text()); });
try {
  await page.goto("http://127.0.0.1:8000/work/mesh-viewer/index.html", { waitUntil: "networkidle", timeout: 60000 });
  await page.waitForFunction(() => window.__viewerStatus === "loaded" || window.__viewerStatus === "error", { timeout: 60000 });
  const result = await page.evaluate(() => ({
    status: window.__viewerStatus,
    stats: window.__viewerStats,
    canvas: !!document.querySelector("canvas"),
    canvasWidth: document.querySelector("canvas")?.width ?? 0,
    canvasHeight: document.querySelector("canvas")?.height ?? 0
  }));
  await page.screenshot({ path: "work/mesh-viewer-smoke.png", fullPage: true });
  fs.writeFileSync("work/mesh-viewer-smoke.json", JSON.stringify({ ...result, errors }, null, 2));
  console.log(JSON.stringify({ ...result, errors }, null, 2));
  if (result.status !== "loaded" || !result.stats || result.stats.vertices < 1 || result.stats.triangles < 1) process.exitCode = 1;
  if (!result.canvas || result.canvasWidth < 1 || result.canvasHeight < 1) process.exitCode = 1;
  if (errors.length) process.exitCode = 1;
} finally {
  await browser.close();
}
