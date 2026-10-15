using System.Text.Json;
using System.Reflection;
using UnityEngine;
using Crisis.Runtime;
using Crisis.Runtime.Weapons;
using Object=UnityEngine.Object;
public sealed class BallisticRuntimeHost {
 public static BallisticRuntimeHost Current;
 readonly SceneLoadResult scene;readonly Dictionary<string,BallisticMaterialProperties> properties=[];
 readonly Dictionary<string,Texture2D> textures=[];readonly Dictionary<string,BallisticHitEffect> effectPrefabs=[];readonly Dictionary<string,BallisticHitMeshEffect> meshPrefabs=[];
 readonly HashSet<BallisticMaterial> started=[];readonly List<BallisticMaterial> materials=[];
 readonly List<(string objectID,string componentID,string textureID,bool hasHole,BallisticWindow component)> windowVisuals=[];
 readonly string sourceSceneHash;readonly List<object> interactions=[];readonly Dictionary<int,float> entryEnergy=[];
 readonly Dictionary<int,GameObject> meshes=[];readonly Action<Projectile> register;readonly Crisis.Runtime.Weapons.WeaponEffects effects;
 public readonly int particlesLifetime;int contactCount,fragmentCount;public int unsupportedColliders,loadedMeshColliders,windowCount;public RuntimeWorld World=>scene.world;
 static float F(JsonElement e,string key)=>e.GetProperty(key).GetSingle();static string Ref(JsonElement e)=>e.GetProperty("fileID").GetString();
 static Vector3 V(JsonElement e)=>new(F(e,"x"),F(e,"y"),F(e,"z"));
 static bool Flag(JsonElement e,string key)=>e.GetProperty(key).ValueKind==JsonValueKind.True||(e.GetProperty(key).ValueKind==JsonValueKind.Number&&e.GetProperty(key).GetInt32()!=0);
 public BallisticRuntimeHost(string json,SceneLoadResult scene,Crisis.Runtime.Weapons.WeaponEffects effects,Action<Projectile> register){
  this.scene=scene;this.effects=effects;this.register=register;Current=this;scene.world.MakeCurrent();using var document=JsonDocument.Parse(json);var c=document.RootElement;
  sourceSceneHash=c.GetProperty("sourceHashes").GetProperty("Assets/Scenes/Mission 01.unity").GetString();
  Masks.Instance.BallisticTargets=c.GetProperty("ballisticMask").GetInt32();Masks.Instance.Limbs=c.GetProperty("limbMask").GetInt32();Masks.Instance.BallisticTargetsSansLimbs=Masks.Instance.BallisticTargets&~Masks.Instance.Limbs;
  var settings=c.GetProperty("settings");particlesLifetime=settings.GetProperty("particlesLifetime").GetInt32();PerformanceManager.Instance=new(){MaxEnvironmentHitEffects=settings.GetProperty("maxEnvironmentHitEffects").GetInt32(),MaxEnvironmentHitMeshes=settings.GetProperty("maxEnvironmentHitMeshes").GetInt32()};MissionHelper.Instance.EffectsContainer=new GameObject("Original effects container",scene.world).transform;
  foreach(var pair in c.GetProperty("textures").EnumerateObject()){var t=pair.Value;textures[pair.Name]=new(t.GetProperty("width").GetInt32(),t.GetProperty("height").GetInt32(),Convert.FromBase64String(t.GetProperty("rgba").GetString()),scene.world){SourceID=pair.Name};}
  foreach(var pair in c.GetProperty("effects").EnumerateObject()){
   var records=pair.Value.GetProperty("records");var map=Hierarchy(records);var root=map.Values.First(t=>t.parent==null);root.gameObject.assetObject=true;
   foreach(var record in records.EnumerateArray()){
    if(record.GetProperty("classID").GetInt32()!=114)continue;var f=record.GetProperty("fields");var goid=Ref(f.GetProperty("m_GameObject"));var tr=records.EnumerateArray().First(r=>r.GetProperty("classID").GetInt32()==4&&Ref(r.GetProperty("fields").GetProperty("m_GameObject"))==goid);var target=map[tr.GetProperty("fileID").GetString()];
    if(f.TryGetProperty("audioClips",out _)){var fx=target.gameObject.AddComponent<BallisticHitEffect>();fx.SourceID=pair.Name;fx.offset=F(f,"offset");fx.audioClips=pair.Value.GetProperty("audio").EnumerateArray().Select(x=>x.GetString()).ToArray();effectPrefabs[pair.Name]=fx;}
    else if(f.TryGetProperty("scaleRatio",out _)){var fx=target.gameObject.AddComponent<BallisticHitMeshEffect>();fx.SourceID=pair.Name;fx.scaleRatio=F(f,"scaleRatio");fx.maxScale=F(f,"maxScale");meshPrefabs[pair.Name]=fx;}
   }
  }
  foreach(var pair in c.GetProperty("properties").EnumerateObject()){
   var f=pair.Value.GetProperty("fields");var p=new GameObject(pair.Value.GetProperty("source").GetString(),scene.world).AddComponent<BallisticMaterialProperties>();p.gameObject.assetObject=true;
   p.surfaceThickness=F(f,"surfaceThickness");p.surfaceDensity=F(f,"surfaceDensity");p.maximumDeflection=F(f,"maximumDeflection");p.density=F(f,"density");var range=f.GetProperty("densityRandomness");p.densityRandomness=new(F(range,"x"),F(range,"y"));
   string Key(string k)=>f.GetProperty(k).TryGetProperty("guid",out var g)?g.GetString():null;
   if(Key("entryEffect") is string e&&effectPrefabs.TryGetValue(e,out var ef))p.entryEffect=ef;if(Key("exitEffect") is string x&&effectPrefabs.TryGetValue(x,out var xf))p.exitEffect=xf;if(Key("exitHole") is string h&&meshPrefabs.TryGetValue(h,out var hf))p.exitHole=hf;properties[pair.Name]=p;
  }
  foreach(var record in c.GetProperty("materials").EnumerateArray()){
   var f=record.GetProperty("fields");string id=record.GetProperty("id").GetString(),goid=Ref(f.GetProperty("m_GameObject"));if(!scene.objects.TryGetValue(goid,out var obj)||obj is not GameObject go)continue;
   BallisticMaterial m;if(record.GetProperty("kind").GetString()=="window"){
    var w=go.AddComponent<BallisticWindow>();go.AddComponent<Renderer>();w.texture=textures[f.GetProperty("texture").GetProperty("guid").GetString()];if(f.GetProperty("holeTexture").TryGetProperty("guid",out var hole))w.holeTexture=textures[hole.GetString()];m=w;windowCount++;
   }else m=go.AddComponent<BallisticMaterial>();
   if(f.GetProperty("properties").TryGetProperty("guid",out var guid))m.properties=properties[guid.GetString()];m.width=F(f,"width");m.depth=F(f,"depth");m.impenetrable=Flag(f,"impenetrable");m.surfaceTestOnExit=Flag(f,"surfaceTestOnExit");m.enabled=Flag(f,"m_Enabled");
   // Loaded graph is deliberately not globally activated. Activate only recovered
   // material methods, preserving the existing input clock and scope.
   m.Awake();if(m.isActiveAndEnabled){m.Start();started.Add(m);}materials.Add(m);scene.objects[id]=m;
   if(m is BallisticWindow visual)windowVisuals.Add((goid,id,f.GetProperty("texture").GetProperty("guid").GetString(),f.GetProperty("holeTexture").TryGetProperty("guid",out _),visual));
   m.onInteractionBegin.AddListener(p=>{entryEnergy[p.GetInstanceID()]=p.Energy;contactCount++;});
   m.onInteractionEnd.AddListener(p=>{var info=new{sourceObject=goid,sourceComponent=id,name=go.name,material=m.properties?.name,entryEnergy=entryEnergy.GetValueOrDefault(p.GetInstanceID(),p.Energy),exitEnergy=p.Energy,status=p.Status.ToString(),p.PenetrationDistance,position=new[]{p.Position.x,p.Position.y,p.Position.z},projectile=p.Id,fragment=p.HasFragmented};if(interactions.Count>=32)interactions.RemoveAt(0);interactions.Add(info);Emit("materialInteraction",info);entryEnergy.Remove(p.GetInstanceID());});
   if(m is BallisticWindow window){
    Action<Texture2D,int,int,int,int,byte[]> onPixelsChanged=(t,x,y,w,h,pixels)=>
      Emit("windowStamp",new{sourceObject=goid,texture=t.SourceID,x,y,width=w,height=h,rgba=Convert.ToBase64String(pixels)});
    var renderer=window.GetComponent<Renderer>();
    bool installed=false;
    renderer.material.OnMainTextureAssigned=t=>{
      if(t!=null)t.Changed=onPixelsChanged;
      if(installed)Emit("windowReset",new{sourceObject=goid,texture=t?.SourceID});
      installed=true;
    };
    // Awake has already assigned the first original texture instance.
    renderer.material.OnMainTextureAssigned(renderer.material.mainTexture);
   }
  }
  var data=c.GetProperty("meshes");foreach(var record in c.GetProperty("meshColliders").EnumerateArray()){
   var f=record.GetProperty("fields");string goid=Ref(f.GetProperty("m_GameObject"));if(!scene.objects.TryGetValue(goid,out var obj)||obj is not GameObject go)continue;var source=data.GetProperty(record.GetProperty("mesh").GetString());var collider=go.AddComponent<MeshCollider>();collider.convex=Flag(f,"m_Convex");collider.enabled=Flag(f,"m_Enabled");collider.isTrigger=Flag(f,"m_IsTrigger");
   collider.vertices=source.GetProperty("vertices").EnumerateArray().Select(a=>new Vector3(a[0].GetSingle(),a[1].GetSingle(),a[2].GetSingle())).ToArray();collider.triangles=source.GetProperty("triangles").EnumerateArray().Select(a=>a.EnumerateArray().Select(v=>v.GetInt32()).ToArray()).ToArray();if(collider.convex)collider.planes=source.GetProperty("convexPlanes").EnumerateArray().Select(a=>a.EnumerateArray().Select(v=>v.GetSingle()).ToArray()).ToArray();scene.objects[record.GetProperty("id").GetString()]=collider;loadedMeshColliders++;
  }
  unsupportedColliders=c.GetProperty("unsupportedColliders").GetArrayLength();
 }
 Dictionary<string,Transform> Hierarchy(JsonElement records){var map=new Dictionary<string,Transform>();foreach(var r in records.EnumerateArray())if(r.GetProperty("classID").GetInt32()==4){var f=r.GetProperty("fields");var t=new GameObject("Original effect transform",scene.world).transform;map[r.GetProperty("fileID").GetString()]=t;}
  foreach(var r in records.EnumerateArray())if(r.GetProperty("classID").GetInt32()==4){var f=r.GetProperty("fields");var t=map[r.GetProperty("fileID").GetString()];if(map.TryGetValue(Ref(f.GetProperty("m_Father")),out var parent))t.SetParent(parent,false);t.localPosition=V(f.GetProperty("m_LocalPosition"));t.localScale=V(f.GetProperty("m_LocalScale"));var q=f.GetProperty("m_LocalRotation");t.localRotation=new(F(q,"x"),F(q,"y"),F(q,"z"),F(q,"w"));}return map;
 }
 public object WindowVisuals(){
  scene.world.MakeCurrent();
  return new {
   sceneSha256=sourceSceneHash,
   // Confirmed by the original Mission 01 MeshFilter audit, not substitute geometry.
   meshFileID="10210",meshGuid="0000000000000000e000000000000000",
   windows=windowVisuals.Select(w=>{
    var t=w.component.transform;var m=t.WorldMatrix;
    // System.Numerics uses row-vector matrices. Row-major serialization can
    // be imported by Three as column-major, then reflected on both Z axes.
    var matrix=new[]{m.M11,m.M12,m.M13,m.M14,m.M21,m.M22,m.M23,m.M24,
      m.M31,m.M32,m.M33,m.M34,m.M41,m.M42,m.M43,m.M44};
    return new {sourceObject=w.objectID,sourceComponent=w.componentID,texture=w.textureID,
      hasHoleTexture=w.hasHole,active=w.component.isActiveAndEnabled,matrix};
   }).ToArray()
  };
 }
 public BallisticMaterialProperties Property(string id)=>properties[id];
 public void Emit(string type,object value)=>effects.Emit(type,value);
 public void RegisterProjectile(Projectile p){fragmentCount++;register(p);Emit("fragmentSpawn",new{p.Id,p.Mass,p.BaseVelocity});}
 public void Step(){scene.world.MakeCurrent();
  // Impact energy is presentation-only telemetry. The original synchronous
  // onInteractionEnd consumes completed entries immediately. Deferred exits
  // never invoke onInteractionEnd, so their unused entries expire here rather
  // than retaining every suspended projectile for the life of the WASM app.
  entryEnergy.Clear();
  foreach(var m in materials)if(m.isActiveAndEnabled){if(started.Add(m))m.Start();m.Update();}foreach(var e in scene.world.Objects.OfType<BallisticHitEffect>().Where(e=>e.isActiveAndEnabled).ToArray())e.Step();foreach(var e in scene.world.Objects.OfType<BallisticHitMeshEffect>().Where(e=>e.isActiveAndEnabled).ToArray())e.StepPresentation();}
 public object Snapshot()=>new{contacts=contactCount,fragments=fragmentCount,materials=materials.Count,windows=windowCount,loadedMeshColliders,unsupportedColliders,last=interactions.LastOrDefault()};
}
