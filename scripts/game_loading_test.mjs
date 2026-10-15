import {test} from 'node:test';
import assert from 'node:assert/strict';
import fs from 'node:fs';
import {promiseCache,decodeScene} from '../game/loading.mjs';
test('shared textures load once even for concurrent callers',async()=>{
 let calls=0;const get=promiseCache(async()=>{calls++;return {};});
 const items=await Promise.all([get('skin'),get('skin'),get('skin')]);
 assert.equal(calls,1);assert.equal(items[0],items[1]);assert.equal(await get('skin'),items[0]);
});
test('binary scene preserves every original OBJ triangle corner and attribute',()=>{
 const root=new URL('../game/assets/',import.meta.url),bytes=fs.readFileSync(new URL('mission01.crg',root));
 const d=decodeScene(bytes.buffer.slice(bytes.byteOffset,bytes.byteOffset+bytes.byteLength));
 const v=[],n=[],uv=[];let corner=0;
 for(const line of fs.readFileSync(new URL('mission01.obj',root),'utf8').split('\n')){
  const p=line.trim().split(/\s+/);if(p[0]==='v')v.push(p.slice(1).map(Number));else if(p[0]==='vn')n.push(p.slice(1).map(Number));else if(p[0]==='vt')uv.push(p.slice(1).map(Number));
  else if(p[0]==='f')for(const tuple of p.slice(1)){
   const [vi,ti,ni]=tuple.split('/').map(Number),index=d.indices[corner++];
   for(const [values,size,source] of [[d.positions,3,v[vi-1]],[d.normals,3,n[ni-1]],[d.uvs,2,uv[ti-1]]])
    for(let axis=0;axis<size;axis++)assert.equal(values[index*size+axis],Math.fround(source[axis]));
  }
 }
 assert.equal(corner,d.indices.length);assert.equal(corner/3,33666);
 assert.ok(bytes.length<fs.statSync(new URL('mission01.obj',root)).size*.4);
});
