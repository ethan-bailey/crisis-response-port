import test from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
const physics=JSON.parse(fs.readFileSync(new URL('../game/assets/original-arm-physics.json',import.meta.url),'utf8'));
const components=JSON.parse(fs.readFileSync(new URL('../game/assets/original-character-limb-components.json',import.meta.url),'utf8'));
test('all original 19 actor limbs and 18 hinge parents preserve source prefab identities',()=>{
 assert.equal(components.format,'crisis-original-character-limb-components-v1');
 assert.equal(components.archiveSha256,'cc94b34da675969f881df10158d6c1b9c161cbcf620b5c0aea71c0cea4bc3a1d');
 for(const name of ['Operator','Occupant']){
  const prefab=physics.prefabs.find(p=>p.name===name);
  const source=components.characters.find(p=>p.name===name);
  assert.equal(source.prefabSha256,prefab.sha256);
  const id=new Set();
  assert.equal(source.limbs.length,19);
  const records=new Map(prefab.records.map(x=>[x.fileID,x]));
  const rbByGo=new Map(prefab.records.filter(x=>x.classID===54).map(x=>[x.fields.m_GameObject.fileID,x]));
  const joints=prefab.records.filter(x=>x.classID===59);
  assert.equal(joints.length,18);
  for(const limb of source.limbs){
   const limbId=Number(limb.sourceFields.Limb.id);
   assert.ok(limbId>=0&&limbId<19&&!id.has(limbId));
   id.add(limbId);
   const transform=records.get(limb.transform);
   assert.equal(transform.classID,4);
   assert.equal(transform.fields.m_GameObject.fileID,limb.gameObject);
   assert.ok(rbByGo.has(limb.gameObject),'original limb has no Rigidbody: '+limb.name);
   assert.ok(limb.sourceFields.Limb && limb.componentIds.Limb);
  }
  assert.equal(id.size,19);
  assert.equal(source.limbs.filter(x=>x.sourceFields.LimbPhysiology).length,17);
  assert.equal(source.limbs.filter(x=>x.sourceFields.LimbMuscle).length,16);
  assert.equal(source.limbs.filter(x=>x.sourceFields.VitalFunctions).length,9);
  for(const limb of source.limbs.filter(x=>/hand$/.test(x.name)))
    assert.equal(limb.sourceFields.LimbPhysiology,undefined,'Do not invent hand physiology');
  for(const j of joints){
   assert.ok(rbByGo.has(j.fields.m_GameObject.fileID));
   const connected=records.get(j.fields.m_ConnectedBody.fileID);
   assert.equal(connected?.classID,54,'missing original rigidbody attachment');
  }
 }
});
