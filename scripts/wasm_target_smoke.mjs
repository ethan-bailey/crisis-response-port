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
  const result = await page.evaluate(() => ({
    text: document.querySelector("#result")?.textContent ?? "",
    scope: document.querySelector("#scope")?.textContent ?? "",
    title: document.querySelector("h1")?.textContent ?? ""
  }));
  fs.writeFileSync("work/wasm-target-smoke.json", JSON.stringify({ ...result, errors }, null, 2));
  await page.screenshot({ path: "work/wasm-target-smoke.png", fullPage: true });
  console.log(JSON.stringify({ ...result, errors }, null, 2));
  if (!result.text.startsWith("PASS") || errors.length) process.exitCode = 1;
} finally {
  await browser.close();
}
