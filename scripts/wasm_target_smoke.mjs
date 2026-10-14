import { chromium } from "playwright";
import fs from "node:fs";

const browser = await chromium.launch({ headless: true, args: ["--no-sandbox", "--enable-webgl", "--ignore-gpu-blocklist", "--use-gl=angle", "--use-angle=swiftshader"] });
const page = await browser.newPage({ viewport: { width: 1024, height: 720 } });
const errors = [];
page.on("pageerror", error => errors.push(error.message));
page.on("console", message => { if (message.type() === "error") errors.push(message.text()); });
try {
  await page.goto("http://127.0.0.1:8001/", { waitUntil: "networkidle", timeout: 90000 });
  await page.waitForFunction(() => {
    const el = document.querySelector("#result");
    return el && (el.textContent.startsWith("PASS") || el.textContent.startsWith("FAIL"));
  }, { timeout: 90000 });
  await page.locator("#runShot").click();
  await page.waitForFunction(() => document.querySelector("#shotResult")?.textContent?.includes("Original HandleProjectile returned"));
  const penetrableShot = await page.locator("#shotResult").textContent();
  await page.locator("#thickness").focus();
  await page.keyboard.press("ArrowRight");
  await page.keyboard.press("ArrowRight");
  await page.keyboard.press("ArrowRight");
  await page.locator("#runShot").click();
  await page.waitForFunction(() => document.querySelector("#shotResult")?.textContent?.includes("Original HandleProjectile returned"));
  const thickerSurfaceShot = await page.locator("#shotResult").textContent();
  await page.locator("#impenetrable").check();
  await page.locator("#runShot").click();
  await page.waitForFunction(() => document.querySelector("#shotResult")?.textContent?.includes("Original HandleProjectile returned"));
  const impenetrableShot = await page.locator("#shotResult").textContent();
  const result = await page.evaluate(() => ({
    text: document.querySelector("#result")?.textContent ?? "",
    scope: document.querySelector("#scope")?.textContent ?? "",
    title: document.querySelector("h1")?.textContent ?? "",
    shot: document.querySelector("#shotResult")?.textContent ?? ""
  }));
  result.penetrableShot = penetrableShot;
  result.thickerSurfaceShot = thickerSurfaceShot;
  result.impenetrableShot = impenetrableShot;
  fs.writeFileSync("work/wasm-target-smoke.json", JSON.stringify({ ...result, errors }, null, 2));
  await page.screenshot({ path: "work/wasm-target-smoke.png", fullPage: true });
  console.log(JSON.stringify({ ...result, errors }, null, 2));
  const energyAfter = shot => Number(shot.match(/energy after: ([0-9.]+)/i)?.[1] ?? NaN);
  if (!result.text.startsWith("PASS") ||
      !result.penetrableShot.includes("Original HandleProjectile returned") ||
      !(energyAfter(result.penetrableShot) > 0) ||
      !result.thickerSurfaceShot.includes("Original HandleProjectile returned") ||
      !(energyAfter(result.thickerSurfaceShot) > 0) ||
      !(energyAfter(result.thickerSurfaceShot) < energyAfter(result.penetrableShot)) ||
      !result.impenetrableShot.includes("Original HandleProjectile returned Stop") ||
      energyAfter(result.impenetrableShot) !== 0 ||
      errors.length) process.exitCode = 1;
} finally {
  await browser.close();
}
