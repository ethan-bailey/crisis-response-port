// Real headless browser check for the retained ORIGINAL Unity Marksman WebGL
// player. Never substitute gameplay; only observe Unity's loader, input and GPU
// canvas. Browser loading is not proof of complete gameplay or iPhone parity.
import { chromium, webkit } from 'playwright';
import { mkdirSync, writeFileSync } from 'node:fs';
const output='work/unity-spike/native-browser';
mkdirSync(output,{recursive:true});
const results=[];
for (const [name,engine] of [['chromium',chromium],['webkit',webkit]]) {
  let browser=null;
  const result={engine:name,resolved:false,canvas:null,errors:[],warnings:[],
    failedRequests:[],browserRuntimeTest:true,realIPhoneTest:false,
    originalMission01Test:false,originalGameplayParity:false};
  try {
    browser=await engine.launch({headless:true,...name==='chromium'?{
      args:['--no-sandbox','--enable-webgl','--ignore-gpu-blocklist',
        '--use-gl=angle','--use-angle=swiftshader']} : {}});
    const page=await browser.newPage({viewport:{width:960,height:540},deviceScaleFactor:1});
    page.on('pageerror',e=>result.errors.push(String(e.message).slice(0,1200)));
    page.on('console',e=>{if(e.type()==='error')result.warnings.push(e.text().slice(0,1200));});
    page.on('requestfailed',r=>result.failedRequests.push({url:r.url().slice(0,230),failure:r.failure()?.errorText}));
    page.on('dialog',async d=>{result.warnings.push('Unity dialog '+d.type()+': '+d.message().slice(0,1200));await d.dismiss();});
    // Observe the actual Unity-created instance promise without changing any
    // source game file or simulating a successful instance.
    await page.addInitScript(()=>{
      const diag={invoked:false,resolved:false,rejected:null,progress:0};
      window.__nativeUnityProof=diag;
      let original;
      Object.defineProperty(window,'createUnityInstance',{
        configurable:true,
        set(fn){original=fn;},
        get(){
          if(typeof original!=='function')return original;
          return function(...args) {
            diag.invoked=true;
            if(typeof args[2]==='function'){
              const callback=args[2];
              args[2]=function(progress){diag.progress=progress;return callback(progress);};
            }
            const promise=Reflect.apply(original,this,args);
            Promise.resolve(promise).then(
              instance=>{diag.resolved=Boolean(instance&&
                typeof instance.SendMessage==='function'&&
                typeof instance.Quit==='function');},
              error=>{diag.rejected=String(error).slice(0,1500);}
            );
            return promise;
          };
        }
      });
    });
    const response=await page.goto('http://127.0.0.1:8000/',{
      waitUntil:'domcontentloaded',timeout:120000});
    result.httpStatus=response?.status()??null;
    await page.waitForFunction(()=>{
      const d=window.__nativeUnityProof;
      return d?.resolved||d?.rejected||
        (d?.invoked&&document.querySelector('#unity-warning')?.textContent?.length>40);
    },null,{timeout:160000,polling:250});
    const diagnostics=await page.evaluate(()=>{
      const canvas=document.querySelector('canvas');
      const diag=window.__nativeUnityProof||{};
      const loading=document.querySelector('#unity-loading-bar');
      return {invoked:diag.invoked,resolved:diag.resolved,
        rejected:diag.rejected,progress:diag.progress,
        canvas:canvas?{width:canvas.width,height:canvas.height,
          clientWidth:canvas.clientWidth,clientHeight:canvas.clientHeight}:null,
        loadingBarDisplay:loading?getComputedStyle(loading).display:null,
        title:document.title,warning:(document.querySelector('#unity-warning')?.textContent||'').slice(0,900)};
    });
    result.diagnostics=diagnostics;
    result.resolved=diagnostics.resolved===true;
    result.canvas=diagnostics.canvas;
    if(!result.resolved)throw new Error('Original Unity createUnityInstance did not resolve: '+diagnostics.rejected);
    if(!result.canvas||result.canvas.width<100||result.canvas.height<100)
      throw new Error('Unity WebGL engine produced no usable canvas');
    await page.screenshot({path:output+'/original-'+name+'-loaded.png',fullPage:true});
    // Dispatch actual browser input; do not claim gameplay movement without
    // a native in-game observable to compare against.
    await page.mouse.click(490,265);
    await page.keyboard.down('ArrowRight');
    await page.waitForTimeout(1500);
    await page.keyboard.up('ArrowRight');
    await page.keyboard.press('Escape');
    await page.waitForTimeout(1000);
    result.nativeInputDispatched=true;
    await page.screenshot({path:output+'/original-'+name+'-after-input.png',fullPage:true});
  } catch (error) {
    result.failure=String(error).slice(0,2000);
  } finally {
    try{await browser?.close();}catch{}
    results.push(result);
    console.log(JSON.stringify({engine:name,resolved:result.resolved,
      failure:result.failure||null,canvas:result.canvas}));
  }
}
writeFileSync(output+'/native-browser-runtime.json',JSON.stringify(results,null,2)+'\n');
if(results.some(r=>!r.resolved))process.exitCode=1;
