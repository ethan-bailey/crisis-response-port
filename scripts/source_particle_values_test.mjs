import assert from 'node:assert/strict';
import {test} from 'node:test';
import {readFileSync} from 'node:fs';
import {gunzipSync} from 'node:zlib';
import {evaluateOriginalImpactEmission,evaluateSourceInheritedVelocity,evaluateSourceParticleFields,evaluateUnityCurve,evaluateUnityGradient,evaluateUnityMinMaxCurve,evaluateUnityMinMaxGradient} from '../game/source-particle-values.mjs';
const controllers=JSON.parse(gunzipSync(readFileSync(new URL('../game/assets/ballistics/original-particle-controllers-source.json.gz',import.meta.url))));

const source=JSON.parse(gunzipSync(readFileSync(new URL('../game/assets/ballistics/original-particles-source.json.gz',import.meta.url))));
const curve=(keys,pre=1,post=1)=>({m_Curve:keys,m_PreInfinity:pre,m_PostInfinity:post});

test('Unity source curve evaluates authored cubic tangents and preserves endpoint keys',()=>{
  const c=curve([{time:0,value:0,inSlope:0,outSlope:2},{time:1,value:1,inSlope:0,outSlope:0}]);
  assert.equal(evaluateUnityCurve(c,0),0);
  assert.equal(evaluateUnityCurve(c,1),1);
  assert.equal(evaluateUnityCurve(c,.5),.75);
});

test('Unity source curve honors Loop and PingPong wrap modes',()=>{
  const loop=curve([{time:0,value:0,inSlope:0,outSlope:2},{time:1,value:1,inSlope:0,outSlope:0}],2,2);
  const pingPong=curve([{time:0,value:0,inSlope:0,outSlope:2},{time:1,value:1,inSlope:0,outSlope:0}],4,4);
  assert.equal(evaluateUnityCurve(loop,1.25),.4375);
  assert.equal(evaluateUnityCurve(loop,-.25),.9375);
  assert.equal(evaluateUnityCurve(pingPong,1.25),.9375);
  assert.equal(evaluateUnityCurve(pingPong,1.75),.4375);
});

test('all serialized Unity MinMaxCurve modes use their authored scalar or curves',()=>{
  const twoConstants={minMaxState:3,minScalar:2,scalar:6};
  const oneCurve={minMaxState:1,scalar:2,maxCurve:curve([{time:0,value:1,inSlope:0,outSlope:0},{time:1,value:3,inSlope:0,outSlope:0}])};
  const twoCurves={minMaxState:2,minCurve:curve([{time:0,value:1,inSlope:0,outSlope:0}]),maxCurve:curve([{time:0,value:5,inSlope:0,outSlope:0}])};
  assert.equal(evaluateUnityMinMaxCurve({minMaxState:0,scalar:7},.5,.25),7);
  assert.equal(evaluateUnityMinMaxCurve(oneCurve,.5,.25),4);
  assert.equal(evaluateUnityMinMaxCurve(twoCurves,0,.25),2);
  assert.equal(evaluateUnityMinMaxCurve(twoConstants,0,.25),3);
  assert.throws(()=>evaluateUnityMinMaxCurve({minMaxState:8},0),/Unsupported/);
});

test('Unity Gradient evaluates authored color and alpha keys',()=>{
  const g={m_NumColorKeys:2,m_NumAlphaKeys:2,ctime0:0,ctime1:65535,atime0:0,atime1:65535,
    key0:{r:1,g:0,b:.25,a:1},key1:{r:0,g:1,b:.75,a:0},key2:{a:1},key3:{a:0}};
  assert.deepEqual(evaluateUnityGradient(g,.5),{r:.5,g:.5,b:.5,a:.5});
  const mm={minMaxState:2,minGradient:g,maxGradient:{...g,key0:{r:0,g:0,b:0,a:0},key1:{r:1,g:1,b:1,a:1}}};
  assert.deepEqual(evaluateUnityMinMaxGradient(mm,.5,.5),{r:.5,g:.5,b:.5,a:.5});
});

test('every recovered original impact emitter curve and gradient is evaluable from source data',()=>{
  let curves=0,gradients=0;
  const walk=(value,path='source')=>{
    if(!value||typeof value!=='object')return;
    if(Array.isArray(value)){for(let i=0;i<value.length;i++)walk(value[i],`${path}[${i}]`);return;}
    if(Number.isInteger(value.minMaxState)&&value.maxCurve&&value.minCurve){
      for(const t of [0,.25,.5,.75,1]) for(const r of [0,.5,1])
        assert.ok(Number.isFinite(evaluateUnityMinMaxCurve(value,t,r)),`${path} at ${t}`);
      curves++;
    }
    if(Number.isInteger(value.m_NumColorKeys)&&Number.isInteger(value.m_NumAlphaKeys)){
      for(const t of [0,.25,.5,.75,1]){
        const color=evaluateUnityGradient(value,t);
        for(const channel of ['r','g','b','a'])assert.ok(Number.isFinite(color[channel]),`${path}.${channel}`);
      }
      gradients++;
    }
    for(const [key,child] of Object.entries(value))walk(child,`${path}.${key}`);
  };
  walk(source.effects);
  assert.ok(curves>100,`source curves visited: ${curves}`);
  assert.ok(gradients>10,`source gradients visited: ${gradients}`);
});

test('source particle field sampling evaluates authored initial and over-lifetime fields for every impact emitter',()=>{
  let evaluated=0;
  for(const effect of source.effects)for(const emitter of effect.emitters){
    for(const age of [0,.25,.5,.75,1]){
      const fields=evaluateSourceParticleFields(emitter,age,{startSize:.2,startSpeed:.7,
        startLifetime:.6,startRotation:.3,color:.5,sizeOverLifetime:.4,rotationOverLifetime:.6});
      for(const key of ['speed','lifetime','size','rotation'])assert.ok(Number.isFinite(fields[key]),`${effect.guid}:${key}`);
      for(const key of ['r','g','b','a'])assert.ok(Number.isFinite(fields.color[key]),`${effect.guid}:color.${key}`);
      evaluated++;
    }
  }
  assert.equal(evaluated,130);
});

test('all active impact InheritVelocity modules use source initial mode and retain the owning Rigidbody multiplier',()=>{
  let count=0;
  for(const effect of source.effects)for(const emitter of effect.emitters){
    const module=emitter.enabledModules.InheritVelocityModule;
    if(!module)continue;
    count++;
    assert.equal(module.enabled,1);
    assert.equal(module.m_Mode,0);
    for(const age of [0,.25,.5,.75,1])
      assert.equal(evaluateSourceInheritedVelocity(emitter,age,.37),1);
  }
  assert.equal(count,14);
  assert.equal(evaluateSourceInheritedVelocity({enabledModules:{}},0),0);
  const unsupported={enabledModules:{InheritVelocityModule:{enabled:1,m_Mode:1,m_Curve:{minMaxState:0,scalar:1}}}};
  assert.throws(()=>evaluateSourceInheritedVelocity(unsupported,0),/Unsupported Unity InheritVelocity mode/);
});

test('serialized SingleEmissionEffect fields reproduce original impact count, speed and lifetime formulas',()=>{
  const effect=controllers.effects.find(e=>e.source.endsWith('Concrete entry.prefab'));
  const root=effect.controllers.find(c=>c.scriptGuid==='f9f9f8d0f023b657c5276ea277044f46');
  const controller=effect.controllers.find(c=>c.scriptGuid==='7f9bb5f5ef1d9fb3ca6dbc9296820005');
  const impact={Velocity:1200,energy:500,metricDiameter:.009,projectileDiameter:9,
    displacedMaterialVolume:1.25,particleCountScaler:1};
  const entry=evaluateOriginalImpactEmission(controller,impact,'entry',{particlesLifetime:20});
  assert.equal(entry.count,4);
  assert.deepEqual(entry.speedRange,[1.5,6]);
  assert.equal(entry.lifetime,1);
  assert.equal(root.serializedFields.offset,.005);
  const exitEffect=controllers.effects.find(e=>e.source.endsWith('Concrete exit.prefab'));
  const exitController=exitEffect.controllers.find(c=>c.scriptGuid==='7f9bb5f5ef1d9fb3ca6dbc9296820005');
  const exit=evaluateOriginalImpactEmission(exitController,impact,'exit',{particlesLifetime:20});
  assert.equal(exit.count,1);
  assert.deepEqual(exit.speedRange,[0,120]);
  assert.equal(exit.lifetime,3);
  assert.throws(()=>evaluateOriginalImpactEmission(controller,impact,'bounce'),/phase/);
  assert.throws(()=>evaluateOriginalImpactEmission(controller,{...impact,energy:NaN},'entry'),/invalid/);
});
