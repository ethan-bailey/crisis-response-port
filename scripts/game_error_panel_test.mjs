import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { test } from "node:test";

const html = await readFile(new URL("../game/index.html", import.meta.url), "utf8");

test("game errors are readable, selectable, copyable, and no longer covered by Mission controls", () => {
  assert.match(html, /<section id="error" role="alert"[^>]*hidden>/);
  assert.match(html, /<button id="copyError" type="button">Copy details<\/button>/);
  assert.match(html, /<pre id="errorDetails" tabindex="0"/);
  assert.match(html, /#error:not\(\[hidden\]\)\{[^}]*z-index:12/);
  assert.match(html, /#errorDetails\{[^}]*overflow:auto/);
  assert.match(html, /user-select:text;-webkit-user-select:text/);
  assert.match(html, /navigator\.clipboard\?\.writeText/);
  assert.match(html, /navigator\.clipboard\.writeText\(text\)/);
  assert.match(html, /range\.selectNodeContents\(errorDetails\)/);
  assert.match(html, /showError\(error,\{hint:[\s\S]*?retry:/);
  assert.match(html, /id="missionProgress" role="status" aria-live="polite"/);
  assert.match(html, /id="cancelMissionLoad" type="button">Cancel/);
  assert.match(html, /signal:sourceLoadController\.signal/);
  assert.match(html, /event\.preventDefault\(\);lastStageTouchEnd=now/);
});
