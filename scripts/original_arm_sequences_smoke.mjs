import { chromium, webkit } from 'playwright';
import assert from 'node:assert/strict';
import fs from 'node:fs';

const results = [];
for (const [name, engine] of [['chromium', chromium], ['webkit', webkit]]) {
  const browser = await engine.launch({ headless: true });
  try {
    const page = await browser.newPage();
    const errors = [];
    page.on('pageerror', e => errors.push(String(e)));
    await page.goto('http://127.0.0.1:8000/runtime-web/?bridge', { waitUntil: 'networkidle' });
    await page.waitForFunction(() => window.__runtimeReady || window.__runtimeError, null, { timeout: 120000 });
    const result = await page.evaluate(async () => {
      if (window.__runtimeError) throw Error(window.__runtimeError);
      const response = await fetch('/game/assets/original-arm-sequences.json');
      if (!response.ok) throw Error(`Original sequence fetch: ${response.status}`);
      const bundle = await response.text();
      const source = JSON.parse(bundle);
      const compositions = [];
      for (const names of [source.sequences.map(s => s.name), ['Holster pistol', 'Unholster pistol'], ['Lower weapon', 'Raise shoulder weapon']]) {
        compositions.push({ names, output: JSON.parse(await window.runtimeCall('ComposeOriginalArmSequences', bundle, JSON.stringify(names))) });
      }
      return { source, compositions };
    });
    let checked = 0;
    for (const { names, output } of result.compositions) {
      assert.equal(output.playbackImplemented, false);
      for (const arm of ['right', 'left']) {
        const expected = names.flatMap(n => result.source.sequences.find(s => s.name === n)[arm]);
        assert.equal(output[arm].length, expected.length);
        expected.forEach((frame, i) => {
          const actual = output[arm][i];
          for (const key of ['name', 'action', 'transform', 'callback', 'waitForSiblingFrame']) assert.equal(actual[key], frame[key]);
          assert.ok(Math.abs(actual.time - frame.time) < 1e-6);
          ['x', 'y', 'z'].forEach((axis, j) => assert.ok(Math.abs(actual.offset[j] - frame.offset[axis]) < 1e-6));
          checked++;
        });
      }
    }
    assert.deepEqual(errors, []);
    results.push({ browser: name, status: 'pass', originalPrefabs: result.source.sequences.length, checkedFramesAcrossCompositions: checked, playbackImplemented: false });
    await page.screenshot({ path: `work/evidence/arm-sequences-${name}.png` });
  } finally { await browser.close(); }
}
fs.writeFileSync('work/evidence/arm-sequences-browser.json', JSON.stringify(results, null, 2) + '\n');
console.log(JSON.stringify(results));
