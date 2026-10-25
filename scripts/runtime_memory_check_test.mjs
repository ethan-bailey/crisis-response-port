import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { test } from 'node:test';

const html = await readFile(new URL('../runtime/Browser/wwwroot/index.html', import.meta.url), 'utf8');

test('iPhone memory sampling waits until the incremental source store is initialized', () => {
  assert.match(html, /let sampling=false,sourceSnapshotAvailable=false/);
  assert.match(html, /if\(sampling\|\|!sourceSnapshotAvailable\)return/);
  assert.match(html, /sourceSnapshotAvailable=true;status\.textContent=.*?await sample\(\)/);
  assert.match(html, /await sample\(\);const timer=setInterval\(sample,1000\)/);
});

test('iPhone memory check downloads the same full Mission profile as the Start Mission action', () => {
  assert.match(html, /profile:'global-ui-mission-operator'/);
  assert.match(html, /fullsource\/global-ui-mission-operator\/manifest\.json/);
});
