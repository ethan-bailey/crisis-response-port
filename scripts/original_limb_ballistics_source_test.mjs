import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
const read=(name)=>JSON.parse(fs.readFileSync(new URL(name,import.meta.url),'utf8'));
const limb=read('../game/assets/original-character-limb-components.json');
const physical=read('../game/assets/original-full-character-physics.json');
const ballistic=read('../game/assets/original-limb-ballistic-materials.json');
const PIN='cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d';
test('exact source limb ballistic property identity, thickness, surface density and armor',()=>{
 assert.equal(ballistic.format,'crisis-original-limb-ballistic-materials-v1');
 assert.equal(ballistic.archiveSha256,PIN);
 assert.equal(limb.archiveSha256,PIN);
 assert.equal(physical.archiveSha256,PIN);
 const materials=Object.entries(ballistic.materials);
 assert.equal(materials.length,8);
 for(const [id,asset] of materials){
  assert.match(id,/^[0-9a-f]{32}$/);
  assert.match(asset.sourceSha256,/^[0-9a-f]{64}$/);
  assert.match(asset.metaSha256,/^[0-9a-f]{64}$/);
  assert.ok(asset.assetPath.startsWith('Assets/'));
  const f=asset.fields;
  for(const field of ['surfaceThickness','surfaceDensity','maximumDeflection','density'])
   assert.ok(Number.isFinite(f[field])&&f[field]>=0,field);
  assert.ok(f.densityRandomness.x>0 && f.densityRandomness.y>=f.densityRandomness.x);
  assert.equal(f.exitHole.fileID,'0');
 }
 assert.equal(ballistic.armorScriptGuid.length,32);
 const op=limb.characters.find(a=>a.name==='Operator'),occupied=limb.characters.find(a=>a.name==='Occupant');
 for(const source of [op,occupied]){
  const pref=physical.prefabs.find(p=>p.name===source.name);
  const go=new Map(pref.records.filter(r=>r.classID===1).map(r=>[r.fileID,r.fields]));
  const capsules=new Map(pref.records.filter(r=>r.classID===136).map(r=>[r.fields.m_GameObject.fileID,r.fields]));
  const sourceIds=new Set(source.limbs.map(l=>l.gameObject));
  assert.equal(source.limbs.length,19);
  assert.equal(capsules.size>=19,true);
  const observed=new Set();
  for(const item of source.limbs){
   let id=item.sourceFields.Limb.properties.guid;
   assert.ok(ballistic.materials[id],'Unresolved original flesh material '+item.name);
   observed.add(id);
   assert.ok(capsules.has(item.gameObject),'Missing source Collider '+item.name);
   let c=capsules.get(item.gameObject);
   assert.ok(c.m_Radius>0&&c.m_Height>0);
   assert.ok([14,24].includes(go.get(item.gameObject).m_Layer),'Unknown source limb physics layer');
  }
  assert.equal(observed.size,8);
  let armors=ballistic.armor[source.name];
  assert.ok(Object.keys(armors).every(id=>sourceIds.has(id)),
   'Source armor cannot be attached to an invented limb');
  if(source.name==='Operator'){
   assert.equal(Object.keys(armors).length,4);
   assert.deepEqual(Object.values(armors).map(v=>v.fields.type).sort(),[4,4,6,6]);
  }
 }
});
