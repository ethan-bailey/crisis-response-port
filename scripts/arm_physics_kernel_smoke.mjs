import {chromium,webkit} from 'playwright';
import fs from 'node:fs';
import http from 'node:http';
import path from 'node:path';
import {fileURLToPath} from 'node:url';
const root=fileURLToPath(new URL('../',import.meta.url));
const server=http.createServer((req,res)=>{
 const target=path.resolve(root,'.'+decodeURIComponent(new URL(req.url,'http://localhost').pathname));
 if(target===root.slice(0,-1)){res.end('Arm physics engine contracts');return;}
 if(!target.startsWith(root)){res.writeHead(403);res.end();return;}
 try{res.setHeader('Content-Type',target.endsWith('.mjs')?'text/javascript':target.endsWith('.wasm')?'application/wasm':'application/json');res.end(fs.readFileSync(target));}
 catch{res.writeHead(404);res.end();}
});
await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
const reports=[];
try{
for(const [engine,type] of [['chromium',chromium],['webkit',webkit]]){
 let browser;
 try{
  browser=await type.launch({headless:true,...(engine==='chromium'?{
   args:['--no-sandbox','--disable-dev-shm-usage'],
   ...(process.env.ARM_PHYSICS_CHROMIUM_PATH?{executablePath:process.env.ARM_PHYSICS_CHROMIUM_PATH}: {})
  }: {})});
  const page=await browser.newPage();
  await page.goto('http://127.0.0.1:'+server.address().port+'/');
  const report=await page.evaluate(async()=>{
   const {runArmPhysicsKernelContracts}=await import('/scripts/arm_physics_kernel_contracts.mjs');
   return await runArmPhysicsKernelContracts();
  });
  reports.push({engine,...report});
 }catch(error){reports.push({engine,passed:false,error:String(error)});process.exitCode=1;}
 finally{if(browser)await browser.close();}
}
}finally{await new Promise(resolve=>server.close(resolve));}
fs.writeFileSync('arm-physics-kernel-smoke.json',JSON.stringify(reports,null,2)+'\n');
console.log(JSON.stringify(reports,null,2));
