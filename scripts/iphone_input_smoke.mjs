import { chromium, webkit } from 'playwright';
import assert from 'node:assert/strict';
import fs from 'node:fs';

// iPhone 16 landscape CSS viewport. WebKit on Linux is not an actual iPhone test.
const results = [];
for (const [engine, type] of [['chromium', chromium], ['webkit', webkit]]) {
  const browser = await type.launch({ headless: true, ...(engine === 'chromium' ? {
    args: ['--no-sandbox', '--enable-webgl', '--ignore-gpu-blocklist', '--use-gl=angle', '--use-angle=swiftshader']
  } : {}) });
  const page = await browser.newPage({ viewport: { width: 852, height: 393 }, deviceScaleFactor: 3, hasTouch: true });
  const errors = [];
  page.on('pageerror', e => errors.push(e.message));
  try {
    await page.goto('http://127.0.0.1:8000/game/', { waitUntil: 'networkidle', timeout: 90000 });
    await page.waitForFunction(() => window.__gameState?.().loaded || document.querySelector('#error').style.display === 'block', null, { timeout: 90000 });
    assert.equal(await page.evaluate(() => window.__gameState?.().loaded), true, await page.locator('#error').textContent());
    const buttons = page.locator('#controls button');
    for (const button of await buttons.all()) {
      const box = await button.boundingBox();
      assert.ok(box && box.width >= 44 && box.height >= 44 && box.x >= 0 && box.y >= 0 && box.x + box.width <= 852 && box.y + box.height <= 393, 'Touch target is usable in landscape');
    }
    const right = page.locator('#right'), box = await right.boundingBox();
    const start = await page.evaluate(() => window.__gameState().playerX);
    await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2);
    await page.mouse.down();
    await page.waitForTimeout(250);
    await page.mouse.move(450, 160);
    await page.mouse.up();
    assert.equal(await right.evaluate(el => el.classList.contains('active')), false, 'Release outside button clears input');
    assert.ok(await page.evaluate(() => window.__gameState().playerX) > start, 'Movement input reaches prototype');

    if (engine === 'chromium') {
      const cdp = await page.context().newCDPSession(page);
      const sprintBox = await page.locator('#sprint').boundingBox();
      const movement = { x: box.x + box.width / 2, y: box.y + box.height / 2, id: 1 };
      const sprint = { x: sprintBox.x + sprintBox.width / 2, y: sprintBox.y + sprintBox.height / 2, id: 2 };
      await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [movement] });
      await cdp.send('Input.dispatchTouchEvent', { type: 'touchStart', touchPoints: [sprint] });
      assert.equal(await right.evaluate(el => el.classList.contains('active')), true);
      assert.equal(await page.locator('#sprint').evaluate(el => el.classList.contains('active')), true);
      // Chromium's default dispatch path ends the listed changed points, not
      // the omitted points. Release sprint while movement remains held.
      // https://issues.chromium.org/issues/569778218
      await cdp.send('Input.dispatchTouchEvent', { type: 'touchEnd', touchPoints: [sprint] });
      assert.equal(await right.evaluate(el => el.classList.contains('active')), true, 'Other finger remains held');
      assert.equal(await page.locator('#sprint').evaluate(el => el.classList.contains('active')), false);
      await cdp.send('Input.dispatchTouchEvent', { type: 'touchCancel', touchPoints: [] });
      assert.equal(await right.evaluate(el => el.classList.contains('active')), false);
    }
    await page.keyboard.down('KeyD');
    await page.evaluate(() => window.dispatchEvent(new Event('pagehide')));
    assert.equal(await right.evaluate(el => el.classList.contains('active')), false);
    await page.keyboard.up('KeyD');
    assert.deepEqual(errors, []);
    await page.screenshot({ path: `iphone-input-${engine}.png` });
    results.push({ engine, viewport: '852x393', passed: true, actualIPhoneTest: false });
  } catch (error) {
    results.push({ engine, passed: false, error: String(error), pageErrors: errors });
    process.exitCode = 1;
  } finally { await browser.close(); }
}
fs.writeFileSync('iphone-input-smoke.json', JSON.stringify(results, null, 2));
console.log(JSON.stringify(results, null, 2));
