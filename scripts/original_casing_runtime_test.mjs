import assert from 'node:assert/strict';
import {test} from 'node:test';
import {readFile} from 'node:fs/promises';
import {createOriginalCasingRuntime} from '../game/original-casing-runtime.mjs';

const source=JSON.parse(await readFile(new URL('../game/assets/casing-556.json',import.meta.url),'utf8'));
class Geometry{setAttribute(key,value){(this.attributes??={})[key]=value}setIndex(value){this.index=value}computeBoundingSphere(){}dispose(){}}
class Vector2{constructor(x,y){this.x=x;this.y=y}}
class Material{constructor(options){Object.assign(this,options);this.color={setRGB:(...value)=>this.rgb=value}}dispose(){}}
class Mesh{constructor(geometry,material){this.geometry=geometry;this.material=material;this.position={set:(...v)=>this.p=v};this.quaternion={set:(...v)=>this.q=v};this.userData={};this.children=[]}clone(){return new Mesh(this.geometry,this.material)}}
class Group{constructor(){this.children=[]}add(value){this.children.push(value)}remove(value){this.children=this.children.filter(item=>item!==value)}}
const THREE={BufferGeometry:Geometry,Float32BufferAttribute:class{constructor(array,itemSize){this.array=array;this.itemSize=itemSize}},
 Vector2,MeshStandardMaterial:Material,Mesh,Group,DoubleSide:2,LinearSRGBColorSpace:'linear-srgb'};

test('original casing force and torque bind to authored visual, collider, mass and source state',()=>{
 const calls=[],scene=new Group();let nextState;
 const physics={createOriginalCasing(options){calls.push(['create',options]);return {id:options.mass}},
  addDynamicForce(body,force,point,mode){calls.push(['force',body,force,point,mode])},
  addDynamicTorque(body,torque,mode){calls.push(['torque',body,torque,mode])},
  synchronizeDynamic(body,state){calls.push(['sync',body,state])},
  dynamicSnapshot(){return nextState},removeDynamic(body){calls.push(['remove',body])}};
 const normalTexture={},occlusionTexture={};
 const runtime=createOriginalCasingRuntime({THREE,source,physics,scene,normalTexture,occlusionTexture});
 try{
  const rotation=[.1,.2,.3,.9],commands=[
   {source:'Casing',operation:'force',body:42,name:'Casing 5.56x45mm',mass:.002,
    sourcePosition:[-3,.75,1.5],sourceRotation:rotation,force:[1,2,3],position:[-3,.75,1.5],mode:'VelocityChange'},
   {source:'Casing',operation:'torque',body:42,mass:.002,torque:[0,0,.01],mode:'Impulse'}];
  runtime.consume(commands);
  assert.deepEqual(calls[0][1],{position:[-3,.75,1.5],rotation,mass:.002,
   halfExtents:source.collider.size.map(value=>value/2),center:source.collider.center,
   linearDamping:.1,angularDamping:.1});
  assert.equal(calls[1][4],'VelocityChange');assert.equal(calls[2][3],'Impulse');
  assert.equal(scene.children.length,1);
  const visual=scene.children[0];assert.equal(visual.userData.sourceBodyId,42);
  nextState={position:[-2,.5,1.25],rotation,velocity:[1,2,3],angularVelocity:[4,5,6],mass:.002,sleeping:false};
  const states=runtime.stepVisuals();
  assert.deepEqual(visual.p,[-2,.5,-1.25]);assert.deepEqual(visual.q,[-.1,-.2,.3,.9]);
  assert.deepEqual(states[0],{body:42,position:nextState.position,rotation,velocity:nextState.velocity,
   angularVelocity:nextState.angularVelocity,sleeping:false});
  runtime.syncSourceStates([{body:42,position:[-2,.5,1.25],rotation,velocity:[1,2,3],angularVelocity:[4,5,6],settled:false}]);
  runtime.reconcile([{body:42,settled:true}]);
  assert.deepEqual(runtime.snapshot(),{active:0,settled:1,bodyIds:[]});assert.equal(scene.children.length,1);
  runtime.reconcile([]);assert.equal(scene.children.length,0);
  assert.ok(calls.some(call=>call[0]==='remove'));
 }finally{runtime.dispose()}
});

test('original casing rejects torque without creation force, bad mass and malformed source state',()=>{
 const physics={createOriginalCasing(){return {}},addDynamicForce(){},addDynamicTorque(){},
  synchronizeDynamic(){},dynamicSnapshot(){return {}},removeDynamic(){}};
 const runtime=createOriginalCasingRuntime({THREE,source,physics,scene:new Group(),normalTexture:{},occlusionTexture:{}});
 try{
  assert.throws(()=>runtime.consume([{source:'Casing',operation:'torque',body:99,torque:[0,0,1],mode:'Impulse'}]),/before its source Rigidbody force/);
  assert.throws(()=>runtime.consume([{source:'Casing',operation:'force',body:100,mass:.005,
   sourcePosition:[0,0,0],sourceRotation:[0,0,0,1],force:[1,0,0],position:[0,0,0],mode:'VelocityChange'}]),/authored 2 g mass/);
  assert.throws(()=>runtime.syncSourceStates([{body:101,position:[0,0,0],rotation:[0,0,0,1],velocity:[0,0,0],angularVelocity:[0,0,0]}]),/no browser PhysX actor/);
 }finally{runtime.dispose()}
});
