import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';

const html = await readFile(new URL('../runtime/Browser/wwwroot/index.html', import.meta.url), 'utf8');
const app = await readFile(new URL('../runtime/Browser/App.razor', import.meta.url), 'utf8');

test('iPhone memory sampling waits until the incremental source store is initialized', () => {
  assert.match(html, /let sampling=false,sourceSnapshotAvailable=false,lastSampleAt=-Infinity/);
  assert.match(html, /const sample=async\(force=false\)=>/);
  assert.match(html, /!force&&now-lastSampleAt<1000/);
  assert.match(html, /sourceSnapshotAvailable=true;status\.textContent=.*?await sample\(\)/);
  assert.match(html, /await sample\(\);const timer=setInterval\(sample,1000\)/);
  assert.match(html, /await sample\(true\);clearInterval\(timer\);renderReport\('complete'/);
  assert.match(html, /clearInterval\(timer\);await sample\(true\);renderReport\('failed'/);
});

test('iPhone memory check downloads the same full Mission profile as the Start Mission action', () => {
  assert.match(html, /profile:'global-ui-mission-operator'/);
  assert.match(html, /fullsource\/global-ui-mission-operator\/manifest\.json/);
});

test('Blazor starts the runtime exactly once through the first-render callback', () => {
  assert.match(app, /if\(firstRender\)await JS\.InvokeVoidAsync\("startOriginalRuntime"\)/);
  assert.doesNotMatch(html, /window\.startOriginalRuntime\(\);/);
});
