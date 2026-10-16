export function promiseCache(loader){
 const entries=new Map();
 return key=>{if(!entries.has(key))entries.set(key,Promise.resolve().then(()=>loader(key)));return entries.get(key);};
}
export function decodeScene(buffer){
 const h=new DataView(buffer),vertices=h.getUint32(4,true),indices=h.getUint32(8,true);
 if(h.getUint32(0,true)!==0x31475243||h.getUint32(12,true)!==3||buffer.byteLength!==16+vertices*32+indices*4)throw new Error('Invalid original scene binary');
 let offset=16;const take=(n)=>{const a=new Float32Array(buffer,offset,n);offset+=n*4;return a;};
 return {positions:take(vertices*3),normals:take(vertices*3),uvs:take(vertices*2),indices:new Uint32Array(buffer,offset,indices)};
}
