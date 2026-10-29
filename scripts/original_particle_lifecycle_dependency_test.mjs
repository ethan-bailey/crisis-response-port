import assert from 'node:assert/strict';
import {test} from 'node:test';
import {existsSync,readdirSync,readFileSync} from 'node:fs';
import {gunzipSync} from 'node:zlib';
import {createHash} from 'node:crypto';

const compressed=readFileSync(new URL('../docs/history/mission-particle-dependency-inventory-2026-10-25.json.gz',import.meta.url));
const raw=gunzipSync(compressed);
const inventory=JSON.parse(raw.toString('utf8'));
const sourceRoot=new URL('../runtime/Original/FullSource/',import.meta.url);
const readSource=name=>readFileSync(new URL(name,sourceRoot),'utf8');
function methodBody(source,signature){
  const start=source.indexOf(signature);
  assert.notEqual(start,-1,'original method missing: '+signature);
  const open=source.indexOf('{',start);
  assert.notEqual(open,-1,'original method body missing: '+signature);
  let depth=0;
  for(let i=open;i<source.length;i++){
    if(source[i]==='{')depth++;
    else if(source[i]==='}'&&--depth===0)return source.slice(open+1,i);
  }
  assert.fail('unbalanced source method: '+signature);
}
function component(prefabPath,classID,scriptGuid){
  const prefab=inventory.prefabs[prefabPath];assert.ok(prefab,'serialized prefab missing: '+prefabPath);
  return prefab.records.find(record=>record.classID===classID&&record.fields?.m_Script?.guid===scriptGuid);
}
function ordered(source,needles,message){
  let cursor=-1;
  for(const needle of needles){const found=source.indexOf(needle,cursor+1);assert.ok(found>cursor,message+': '+needle);cursor=found;}
}

test('particle inventory locks the complete serialized WoundEmitter and ProjectileTrace asset closure',()=>{
  assert.equal(createHash('sha256').update(raw).digest('hex'),'636935d6bf545dd0bf3610008bbc8759fae1fc765553852b8110da9eacc82407');
  const woundPath='Assets/GameObject/Wound emitter.prefab';
  const wound=inventory.prefabs[woundPath];
  assert.equal(wound.sha256,'5331cf1fddcb0adc2a434ea0191f077925afe5ec0ab6a8d92c0f753932cf419f');
  assert.deepEqual(wound.incomingReferences.map(ref=>[ref.source,ref.fieldPath]).sort(),[
    ['Assets/Resources/characters/Occupant.prefab','woundEmitterPrefab'],
    ['Assets/Resources/characters/Operator.prefab','woundEmitterPrefab'],
    ['Assets/Scenes/Mission 01.unity','woundEmitterPrefab']
  ]);
  const emitter=component(woundPath,114,'7cc04c590272ba6bd67608c51dd6df33');
  assert.equal(emitter.fields.m_GameObject.fileID,'1709254077376921');
  assert.equal(emitter.fields.system.fileID,'198884547391421755');
  assert.equal(wound.systemCount,1);
  const tracePath='Assets/GameObject/Projectile trace.prefab';
  const trace=inventory.prefabs[tracePath];
  assert.equal(trace.sha256,'be9cd9699c39ee7190012e3b9eb33a297ca3898d7f467a2307bd193d4ce58099');
  assert.deepEqual(trace.incomingReferences.map(ref=>[ref.source,ref.fieldPath]).sort(),[
    ['Assets/Resources/weapons/ammunition/projectiles/Projectile 5.56mm FMJ Tracer.prefab','tracePrefab'],
    ['Assets/Resources/weapons/ammunition/projectiles/Projectile 7.62mm FMJ Tracer.prefab','tracePrefab'],
    ['Assets/Resources/weapons/ammunition/projectiles/Projectile 9mm FMJ Tracer.prefab','tracePrefab']
  ]);
  const traceController=component(tracePath,114,'691fc9e4f1bd53933eb96a1e0f9ad9d9');
  assert.equal(traceController.fields.traceFadeTime,0.03);
  assert.equal(traceController.fields.hitEffectPrefab.guid,'d1093d129ce22e94eb638ce311c4ac48');
  const line=trace.records.find(record=>record.classID===120);
  assert.ok(line);
  assert.equal(line.fields.m_UseWorldSpace,1);
  assert.equal(line.fields.m_Parameters.widthMultiplier,0.01);
  assert.equal(line.fields.m_Materials[0].guid,'b69974e8f5204bf45a1bc5df664959fd');
  const hit=inventory.prefabs['Assets/GameObject/Tracer hit.prefab'];
  assert.equal(hit.sha256,'7ba0c2dc98e1ecc5479f7d36ea662cbfac82bd670f7014788725619444d3ea91');
  assert.equal(hit.incomingReferences[0].source,tracePath);
  assert.equal(hit.incomingReferences[0].fieldPath,'hitEffectPrefab');
});

test('wound and tracer render dependencies are inventoried and recovery assets are distinguished from runtime bindings',()=>{
  const deps=inventory.dependencies;
  // Renderer records have no script GUID. Resolve by the serialized component fileID instead.
  const wound=inventory.prefabs['Assets/GameObject/Wound emitter.prefab'];
  const renderer=wound.records.find(record=>record.classID===199&&record.fileID==='199724804207875695');
  assert.ok(renderer);
  const bloodMaterial=renderer.fields.m_Materials[0].guid;
  const bloodMesh=renderer.fields.m_Mesh.guid;
  assert.equal(bloodMaterial,'2f18bd9e6a4de9e45b6395e6e325af12');
  assert.equal(bloodMesh,'f62208a372e889c4f8ae2dde522cce11');
  assert.deepEqual([deps.materials[bloodMaterial].path,deps.materials[bloodMaterial].sha256],
    ['Assets/Material/Blood particle new.mat','52ffba71d0fc041a2230376cddbbeb0c7884af26aa2a8e3a1f3fd7980bc113a1']);
  assert.deepEqual([deps.meshes[bloodMesh].path,deps.meshes[bloodMesh].sha256],
    ['Assets/Mesh/LowPolySphere.asset','68d32986c468bc15a1abf15edf3659ba168fc5225f5ec155c56c4fb332ed0e33']);
  assert.equal(deps.meshes[bloodMesh].sourceMetadata.nativeVertexCount,79);
  assert.equal(Object.hasOwn(deps.meshes[bloodMesh],'mesh'),false,
    'inventory metadata does not contain decoded LowPolySphere geometry');
  const recoveredMesh=JSON.parse(readFileSync(new URL('../game/assets/ballistics/particle-meshes/particle-mesh-'+bloodMesh+'.json',import.meta.url)));
  assert.equal(recoveredMesh.sourcePath,'Assets/Mesh/LowPolySphere.asset');
  assert.equal(recoveredMesh.sourceSha256,'6b07b7570773145d9e5676a32ee293274cd0dc4dca41fc767680615f7a0cad9a');
  assert.deepEqual(recoveredMesh.count,{vertices:79,triangles:112});
  assert.equal(recoveredMesh.mesh.positions.length,79);
  assert.equal(deps.materials[bloodMaterial].records[0].fields.m_Shader.guid,
    '0000000000000000f000000000000000');
  const materialManifest=JSON.parse(gunzipSync(readFileSync(new URL('../game/assets/ballistics/original-particle-materials-source.json.gz',import.meta.url))));
  assert.equal(Object.hasOwn(materialManifest.materials,bloodMaterial),false,
    'the runtime material map is impact-only and does not yet bind Blood particle new');
  assert.equal(existsSync(new URL('../game/assets/ballistics/particle-meshes/particle-mesh-'+bloodMesh+'.json',import.meta.url)),true,
    'the verified full-source snapshot supplied LowPolySphere geometry');

  const trace=inventory.prefabs['Assets/GameObject/Projectile trace.prefab'];
  const line=trace.records.find(record=>record.classID===120);
  const lineMaterial=line.fields.m_Materials[0].guid;
  assert.equal(lineMaterial,'b69974e8f5204bf45a1bc5df664959fd');
  assert.equal(Object.hasOwn(deps.materials,lineMaterial),false,
    'the LineRenderer material is outside the particle material closure');
  for(const ammo of trace.incomingReferences.map(ref=>ref.source))
    assert.equal(inventory.prefabs[ammo],undefined,
      'tracer ammunition owners are outside the bounded 27-prefab inventory: '+ammo);
});

test('all 27 inventoried particle prefabs reconcile against browser materials, textures, and custom meshes',()=>{
  const sourceMaterials=new Set(Object.keys(inventory.dependencies.materials));
  const sourceMeshes=new Set(Object.keys(inventory.dependencies.meshes));
  const referencedMaterials=new Set(),referencedMeshes=new Set();
  for(const prefab of Object.values(inventory.prefabs))for(const record of prefab.records){
    const fields=record.fields||{};
    if(record.classID===199){
      for(const reference of fields.m_Materials||[])if(reference.guid)referencedMaterials.add(reference.guid);
      const guid=fields.m_Mesh?.guid;
      if(guid&&guid!=='0000000000000000e000000000000000')referencedMeshes.add(guid);
    }
  }
  for(const guid of referencedMaterials)assert.ok(sourceMaterials.has(guid),'renderer material absent from source inventory: '+guid);
  for(const guid of referencedMeshes)assert.ok(sourceMeshes.has(guid),'renderer mesh absent from source inventory: '+guid);
  const browserMaterials=JSON.parse(gunzipSync(readFileSync(new URL('../game/assets/ballistics/original-particle-materials-source.json.gz',import.meta.url))));
  const browserTextures=new Set(Object.keys(browserMaterials.textures));
  assert.equal(sourceMaterials.size,15);
  assert.equal(Object.keys(browserMaterials.materials).length,8);
  assert.equal(Object.keys(inventory.dependencies.textures).length,12);
  assert.equal(browserTextures.size,4);
  assert.deepEqual([...sourceMaterials].filter(guid=>!Object.hasOwn(browserMaterials.materials,guid)).sort(),[
    '02cb3c12ddabcf842bd3a305d976b73b','166803ee771bf814cbffdb9dd564b612',
    '28219ea11a6740946a759c6c6cc2b9f8','2f18bd9e6a4de9e45b6395e6e325af12',
    '3ef6f72747746c04094e0dac76918064','78bd66f8e354a564287553acff2ab912',
    'f88e26bf467496741b4c28316201b48b'
  ]);
  assert.deepEqual(Object.keys(inventory.dependencies.textures).filter(guid=>!browserTextures.has(guid)).sort(),[
    '0698d19480d8616439566fcc492a6141','3f5678afea752fb4aa55ec3a6bb5ee0d',
    '43907091bffd53b478c985dca2762559','4ae3dad7f7820a14daaff571c7763543',
    '8d74c3abf30ef9342aae5c8a0fad8388','c3d5701bb477a2c47a733d4dd0c3cce3',
    'f0b75ecf96d6104489b28b07451a3db8','f8daee0cf4e406b4986d4843c0aa9d17'
  ]);
  const availableMeshFiles=new Set(readdirSync(new URL('../game/assets/ballistics/particle-meshes/',import.meta.url))
    .map(name=>name.match(/^particle-mesh-([0-9a-f]{32})\.json$/)?.[1]).filter(Boolean));
  assert.equal(sourceMeshes.size,5);
  assert.equal(availableMeshFiles.size,5);
  assert.deepEqual([...referencedMeshes].filter(guid=>!availableMeshFiles.has(guid)),[]);
  assert.equal(Object.hasOwn(inventory.dependencies.meshes,'0000000000000000e000000000000000'),false,
    'Unity built-in mesh remains an engine dependency, not a project asset');
});

test('wound emission keeps original settings, source state mutation, and callback order',()=>{
  const emitter=readSource('Emitter.cs');
  const start=methodBody(emitter,'private void Start()');
  ordered(start,['PerformanceManager.Instance','system.shape','system.main','perfMgr.MaxBloodParticles','GetComponent<Rigidbody>()',
    'Performance.onChanged.AddListener(OnPerformanceSettingsChanged)','Extras.onChanged.AddListener(OnExtraSettingsChanged)'],'Emitter Start dependency order changed');
  const extra=methodBody(emitter,'public void OnExtraSettingsChanged(');
  ordered(extra,['particleCountMultiplier = newSettings.emissionsModifier','mainModule.maxParticles = perfMgr.MaxBloodParticles'],'extra settings source order changed');
  const emit=methodBody(emitter,'public void Emit(int count)');
  ordered(emit,['Mathf.CeilToInt((float)count * particleCountMultiplier)','system.Emit(count2)'],'wound emission scaling/order changed');
  const bounce=methodBody(emitter,'public void BloodStreamBounceEmit(');
  ordered(bounce,['if (!float.IsNaN(position.x))','position + Vector3.up * 0.01f','Quaternion.LookRotation(Vector3.Reflect(velocity, Vector3.up))',
    'shapeModule.radius = radius','shapeModule.angle = angle','new ParticleSystem.MinMaxCurve(0f, velocity.magnitude)','system.Emit(count2)'],'blood bounce source order changed');
  const limb=methodBody(readSource('Limb.cs'),'public void WoundEmission(');
  ordered(limb,['WoundEmitter.SetTransform(worldPosition, Quaternion.LookRotation(worldDirection), Velocity)',
    'emitterMainModule.startSpeed = startVelocity','emitterShapeModule.radius = emissionRadiusMeters','emitterShapeModule.angle = 1f','WoundEmitter.Emit(count)'],'limb wound callback order changed');
});

test('blood stream lifecycle calls bounce and column emission in authored update paths',()=>{
  const stream=readSource('BloodStream.cs');
  const update=methodBody(stream,'private void UpdateParticles(float delta)');
  ordered(update,['bloodstreamParticle.velocity = new Vector3','if (bloodstreamParticle.prev != null','CreateParticleColumn(bloodstreamParticle, bloodstreamParticle.prev)',
    'if (bloodstreamParticle.position.y <= 0f)','FindSurface(bloodstreamParticle)','particleEmitter.BloodStreamBounceEmit('],'blood stream update lifecycle order changed');
  const column=methodBody(stream,'private void CreateParticleColumn(');
  assert.match(column,/particleEmitter\.EmitColumn\(trailer\.position, leader\.position, trailer\.radius, leader\.radius, trailer\.velocity, leader\.velocity, disperseAngle\)/);
  const generator=readSource('BloodStreamGenerator.cs');
  const generatorUpdate=methodBody(generator,'public bool Update(float pressure, Vector3 position, Vector3 velocity)');
  ordered(generatorUpdate,['if (pressure > 0.05f)','perfMgr.CreateBloodStream(bloodEmitter)','activeStream.Emit(pressure, position, velocity)',
    'activeStream.Terminate(position, velocity)','activeStream = null'],'blood stream threshold/termination order changed');
  const columnEmitter=methodBody(readSource('Emitter.cs'),'public void EmitColumn(');
  ordered(columnEmitter,['if (!float.IsNaN(start.x))','int num3 = Mathf.CeilToInt(num2)','int count = Mathf.CeilToInt(2f * particleCountMultiplier * Time.timeScale)',
    'for (int i = 0; i < num3; i++)','system.Emit(count)','base.transform.position += normalized * num4'],'source column particle callback order changed');
});

test('tracer projectile lifecycle retains source initialization, point update, fade, and hit calls',()=>{
  const projectile=readSource('Projectile.cs');
  const init=methodBody(projectile,'private void CommonInitialize()');
  ordered(init,['if (tracePrefab != null)','Trace = Object.Instantiate(tracePrefab)','Trace.Initialize(this)'],'projectile trace prefab initialization order changed');
  const trace=readSource('ProjectileTrace.cs');
  const initialize=methodBody(trace,'public void Initialize(Projectile projectile)');
  ordered(initialize,['timestamp = Time.time','GetComponent<ParticleSystem>()','GetComponent<LineRenderer>()','lineRenderer.endWidth = metricDiameter',
    'obj.startWidth = metricDiameter','SetScale(Time.timeScale)','SetParent(projectile.transform)','localPosition = Vector3.zero',
    'AddPosition(projectile.transform.position)','AddPosition(projectile.transform.position)'],'trace initialization order changed');
  const update=methodBody(trace,'private void Update()');
  ordered(update,['SetAlpha()','SetScale(Time.timeScale)','projectile.transform.position - lineRenderer.GetPosition(lineRendererIndex - 1)',
    'if (sqrMagnitude > 1f)','AddPosition(projectile.transform.position)','lineRenderer.SetPosition(lineRendererIndex - 1, projectile.transform.position)'],'trace point update order changed');
  const scale=methodBody(trace,'private void SetScale(float scale)');
  assert.match(scale,/startSpeed\.constantMin = 0f;[\s\S]*startSpeed\.constantMax = scale \* 15f;[\s\S]*mainModule\.startSpeed = startSpeed/);
  const alpha=methodBody(trace,'private void SetAlpha()');
  assert.match(alpha,/Mathf\.Clamp01\(\(Time\.time - timestamp\) \/ traceFadeTime\)/);
  assert.match(alpha,/Mathf\.Lerp\(1f \* Time\.timeScale, 0f, num\)/);
  assert.match(alpha,/if \(num >= 1f\)[\s\S]*Object\.Destroy\(lineRenderer\)/);
  const hit=methodBody(trace,'public void SpawnHitEffect(');
  ordered(hit,['Object.Instantiate(hitEffectPrefab)','SetParent(parent)','localScale = Vector3.one','transform.position = position','transform.LookAt(position + hitNormal)'],'tracer hit source pose changed');
  assert.match(projectile,/Trace\.AddPosition\(base\.transform\.position\)/);
  assert.match(projectile,/Trace\.SpawnHitEffect\(base\.transform\.position, hit\.transform, hit\.normal\)/);
});

test('mission weapon-smoke and head-bounce particle callbacks preserve original owner and order',()=>{
  const smoke=inventory.prefabs['Assets/Resources/effects/Weapon smoke emitter.prefab'];
  assert.equal(smoke.sha256,'a47a5d19fd2b3261c6e85d438bc464a9827fab5a7545fb743f82020bbf4437d2');
  assert.deepEqual(smoke.incomingReferences.map(ref=>[ref.source,ref.fieldPath]),[
    ['Assets/Scenes/Mission 01.unity','weaponSmokeEmitterPrefab']
  ]);
  const brain=inventory.prefabs['Assets/Resources/effects/Head bounce brain emission.prefab'];
  assert.equal(brain.sha256,'59a5b8b73f9d22264aaf12295d0c9ce7277bf5d55c920244f063fd18c21c9a00');
  assert.deepEqual(brain.incomingReferences.map(ref=>[ref.source,ref.fieldPath]).sort(),[
    ['Assets/Resources/characters/Occupant.prefab','headBrainBounce'],
    ['Assets/Resources/characters/Operator.prefab','headBrainBounce']
  ]);
  const mission=methodBody(readSource('Mission.cs'),'private void Awake()');
  ordered(mission,['CreateContainers()','WeaponSmokeEmitter = Object.Instantiate(weaponSmokeEmitterPrefab)',
    'WeaponSmokeEmitter.transform.SetParent(EmittersContainer)','MissionHelper.Instance.Initialize(this)',
    'perfMgr = PerformanceManager.Instance'],'mission particle owner initialization order changed');
  const muzzle=methodBody(readSource('WeaponBase.cs'),'private void EmitMuzzleSmoke()');
  ordered(muzzle,['if (emitSmoke)','muzzleSmokeEmitter.transform.position = Muzzle.position',
    'muzzleSmokeEmitter.transform.rotation = Muzzle.rotation','muzzleSmokeEmitter.Emit(1)',
    'if (ComponentSuppressor != null)','muzzleSmokeEmitter.transform.position = cyclingBolt.transform.position',
    'muzzleSmokeEmitter.transform.LookAt(cyclingBolt.transform.position + Vector3.back)',
    'muzzleSmokeEmitter.Emit(1)'],'muzzle smoke source callback order changed');
  const collision=methodBody(readSource('Limb.cs'),'private void OnCollisionEnter(Collision collision)');
  ordered(collision,['if (id != LimbId.Head || !useImpactEmissions || collision.contacts.Length == 0)',
    'LimbPhysiology.AddShock(num)','perfMgr.CreateGoreHitEffect(Body.headBrainBounce.GetComponent<BallisticHitEffect>())',
    'ballisticHitEffect.transform.SetParent(MissionHelper.Instance.EffectsContainer, worldPositionStays: false)',
    'ballisticHitEffect.transform.position = transform.TransformPoint(wound.LocalPosition)',
    'ballisticHitEffect.PlayBounce(count, Rigidbody.velocity.magnitude)'],'head bounce particle/source callback order changed');
  const bounce=methodBody(readSource('BallisticHitEffect.cs'),'public void PlayBounce(int count, float velocity)');
  ordered(bounce,['spawnTime = Time.time','Lifetime = Global.Context.Settings.Performance.particlesLifetime',
    'component.BounceEmission(count, velocity)','PlayAudio(null, 0.5f)'],'brain bounce emission source timing changed');
});
