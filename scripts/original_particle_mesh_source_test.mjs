import assert from 'node:assert/strict';
import { test } from 'node:test';
import { readFileSync } from 'node:fs';

// Source-only original Unity mesh fidelity gate. No generated substitute meshes.
const expected = {
  '8ed663818ef63e741bd160cb133722fe': {source:'661b5a9d7f3f45d558e9f6f11864eacb449e031a38561fffe51b6bff3d4fbdb1',triangles:8,normals:true},
  'eedec0cf1e8d71a49b3949fbbc2cf3b4': {source:'906bd249eb17b8d5e2d41877b53a2eee335bb2590e55cd97de8263d09cc19c3f',triangles:4,normals:true},
  'f6fe08f20af97e149b26c77ded5823d7': {source:'37d2f797c49d4bb3055a65a25f50b059ebf41583638e6896fca76f75a4013876',triangles:4,normals:true},
};
for (const [guid,info] of Object.entries(expected)) test('exact original source particle mesh '+guid,()=>{
  const asset=JSON.parse(readFileSync(new URL('../game/assets/ballistics/particle-meshes/particle-mesh-'+guid+'.json', import.meta.url),'utf8'));
  assert.equal(asset.sourceGuid,guid);
  assert.equal(asset.sourceSha256,info.source);
  assert.ok(asset.sourceFormat.includes('already converted to right-handed'));
  assert.equal(asset.count.vertices,12);
  assert.equal(asset.count.triangles,info.triangles);
  const {positions,normals,uv,indices}=asset.mesh;
  assert.equal(positions.length,12);
  assert.equal(indices.length,info.triangles*3);
  assert.equal(normals.length,info.normals?12:0);
  assert.equal(asset.nativeNormalsPresent,true);
  assert.equal(asset.hasDecodedSourceNormals,info.normals);
  assert.equal(asset.nativeNormalsPresent,true);
  if (info.normals) for (const n of normals){
    const unit=Math.hypot(n[0],n[1],n[2]);
    assert.ok(Math.abs(unit-1)<0.0025,'Recovered native packed source normal must remain a unit direction');
  }
  assert.equal(uv.length,0,'Never invent absent source UV');
  let checked=0;
  for(let i=0;i<indices.length;i+=3){
    const [a,b,c]=indices.slice(i,i+3);
    for(const index of [a,b,c])assert.ok(Number.isInteger(index)&&index>=0&&index<positions.length);
    const v1=positions[b].map((val,k)=>val-positions[a][k]);
    const v2=positions[c].map((val,k)=>val-positions[a][k]);
    const cross=[v1[1]*v2[2]-v1[2]*v2[1],v1[2]*v2[0]-v1[0]*v2[2],v1[0]*v2[1]-v1[1]*v2[0]];
    assert.ok(cross.every(Number.isFinite));
    assert.ok(cross.reduce((sum,x)=>sum+x*x,0)>1e-14,'No degenerate source triangles');
    if(normals.length){
      const dot=cross.reduce((sum,val,k)=>sum+val*normals[a][k],0);
      assert.ok(dot>0,'Source winding must agree with source normals after Unity conversion');
    }
    checked++;
  }
  assert.equal(checked,info.triangles);
});
