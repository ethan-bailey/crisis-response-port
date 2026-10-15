import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFileSync } from 'node:fs';
import { gunzipSync } from 'node:zlib';
import { createHash } from 'node:crypto';

// Source-only regression: never synthesize gameplay or particle emitter state.
const compressed = readFileSync(new URL('../game/assets/ballistics/original-particles-source.json.gz', import.meta.url));
const sourceSha256 = createHash('sha256').update(compressed).digest('hex');
const source = JSON.parse(gunzipSync(compressed).toString('utf8'));
const effects = source.effects;

test('exact original Unity source effect GUID and component coverage', () => {
  assert.equal(sourceSha256, '75bbdbeadf3595f9c8fa32563a689fc119d6747abe0efb6c5bd0c347cd70cf92');
  assert.equal(source.version, 1);
  assert.match(source.sourceAuditSha256, /^[a-f0-9]{64}$/);
  assert.equal(effects.length, 14);
  assert.equal(new Set(effects.map(x=>x.guid)).size, 14);
  assert.equal(effects.reduce((n,e)=>n+e.emitters.length,0), 26);
  for(const e of effects){
    assert.match(e.guid, /^[a-f0-9]{32}$/);
    assert.match(e.sourcePrefabSha256, /^[a-f0-9]{64}$/);
    assert.ok(e.roles.length > 0);
    assert.ok(e.emitters.length > 0);
    assert.equal(new Set(e.emitters.map(x=>x.sourceGameObject)).size,e.emitters.length);
  }
});

test('original native particle modules preserved without enabling disabled ones',()=>{
  let enabledCollisions=0, enabledColor=0;
  const modes={0:0,4:0};
  for (const effect of effects) for (const emitter of effect.emitters) {
    assert.match(emitter.originalEmitterFieldSha256, /^[a-f0-9]{64}$/);
    assert.match(emitter.originalRendererFieldSha256, /^[a-f0-9]{64}$/);
    assert.equal(String(emitter.sourceSettings.m_GameObject.fileID),emitter.sourceGameObject);
    assert.ok(emitter.localTransform.position);
    assert.ok(emitter.localTransform.rotation);
    assert.ok(emitter.localTransform.scale);
    assert.ok(emitter.localTransform.parentTransform);
    for (const name of ['InitialModule','EmissionModule','ShapeModule']) {
      assert.equal(emitter.enabledModules[name]?.enabled,1,'Source module absent: '+name);
    }
    for (const name of emitter.disabledModules){
      assert.ok(!Object.hasOwn(emitter.enabledModules,name),'Disabled module changed: '+name);
    }
    const mode=emitter.renderer.m_RenderMode;
    assert.ok(mode===0||mode===4,'Native renderer mode unsupported: '+mode);
    modes[mode]++;
    assert.ok(Array.isArray(emitter.renderer.m_Materials));
    assert.ok(Number.isFinite(emitter.sourceSettings.lengthInSec));
    if (emitter.enabledModules.CollisionModule) enabledCollisions++;
    if (emitter.enabledModules.ColorModule) enabledColor++;
  }
  assert.deepEqual(modes,{0:14,4:12});
  assert.equal(enabledCollisions,12);
  assert.equal(enabledColor,14);
});
