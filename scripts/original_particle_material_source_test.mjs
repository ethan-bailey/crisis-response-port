import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFileSync } from 'node:fs';
import { createHash } from 'node:crypto';
import { gunzipSync } from 'node:zlib';

// Metadata and immutable-source integrity only, not a replacement shader.
const sha256 = bytes => createHash('sha256').update(bytes).digest('hex');
const materialBinary = readFileSync(new URL('../game/assets/ballistics/original-particle-materials-source.json.gz',import.meta.url));
const particleBinary = readFileSync(new URL('../game/assets/ballistics/original-particles-source.json.gz',import.meta.url));
const material = JSON.parse(gunzipSync(materialBinary).toString('utf8'));
const particle = JSON.parse(gunzipSync(particleBinary).toString('utf8'));
const builtin = '0000000000000000f000000000000000';

test('every source-original material and copied native texture matches immutable provenance',()=>{
  assert.equal(sha256(materialBinary),'bce1860f49f8ab6887deb825bad56118261b4949477653b88c88f94f77d04aa0');
  assert.equal(material.particleManifestSha256,sha256(particleBinary));
  assert.equal(material.version,1);
  assert.match(material.provenanceSha256,/^[a-f0-9]{64}$/);
  assert.equal(Object.keys(material.materials).length,8);
  assert.equal(Object.keys(material.textures).length,4);
  assert.equal(Object.keys(material.shaders).length,2);
  assert.equal(material.shaders[builtin].unresolvedEngineShader,true);
  for(const [id,entry] of Object.entries(material.textures)){
    const src = new URL('../game/assets/ballistics/texture-'+id+'.png',import.meta.url);
    assert.equal(entry.url,'./assets/ballistics/texture-'+id+'.png');
    assert.equal(sha256(readFileSync(src)),entry.originalSha256,'Original PNG must remain byte-exact');
  }
  for(const [id,m] of Object.entries(material.materials)){
    assert.match(id,/^[a-f0-9]{32}$/);
    assert.match(m.originalSha256,/^[a-f0-9]{64}$/);
    assert.ok(m.originalSavedProperties.m_Colors);
    assert.ok(m.originalSavedProperties.m_Floats);
    assert.ok(m.originalSavedProperties.m_TexEnvs);
    assert.ok(Object.hasOwn(material.shaders,m.shaderGuid));
    assert.equal(m.shaderSourceAvailable,!Boolean(material.shaders[m.shaderGuid].unresolvedEngineShader));
    for(const env of Object.values(m.originalSavedProperties.m_TexEnvs)){
      const texture=env?.m_Texture?.guid;
      if(!texture)continue;
      assert.ok(Object.hasOwn(material.textures,texture));
      assert.ok(env.m_Scale&&env.m_Offset,'Do not discard original UV scale/offset');
    }
  }
});

test('native ParticleSystemRenderer material references are the complete source mapping',()=>{
  const actual=new Map();
  for(const effect of particle.effects) for(const emitter of effect.emitters)
    for(const ref of emitter.renderer.m_Materials) {
      if(!ref.guid)continue;
      const source=actual.get(ref.guid)||new Set();
      source.add(effect.guid);
      actual.set(ref.guid,source);
    }
  const actualObject=Object.fromEntries([...actual].map(([guid,set])=>[guid,[...set].sort()]));
  assert.deepEqual(actualObject,material.effectsByMaterial);
  assert.equal(Object.keys(actualObject).length,8);
});
