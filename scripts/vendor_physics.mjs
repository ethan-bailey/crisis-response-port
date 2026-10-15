// Run after npm ci --prefix game. Serve WASM from the same origin on iOS.
import {mkdir,copyFile} from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
const game=fileURLToPath(new URL('../game/',import.meta.url));
await mkdir(game+'vendor',{recursive:true});
for(const file of ['physx-js-webidl.mjs','physx-js-webidl.wasm'])
 await copyFile(game+'node_modules/physx-js-webidl/'+file,game+'vendor/'+file);
