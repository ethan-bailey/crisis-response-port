using UnityEngine;

int assertions=0;
void Check(bool condition,string message){if(!condition)throw new Exception(message);assertions++;}
void Throws<T>(Action action) where T:Exception {try{action();}catch(T){assertions++;return;}throw new Exception($"Expected {typeof(T).Name}");}

// One coherent graphics transaction: clone original CPU pixels, bind a wound
// mask target/material, emit GL geometry, restore target, and retain isolation.
var first=new RuntimeWorld();first.MakeCurrent();
var original=new Texture2D(2,1,new byte[]{255,0,0,255,0,255,0,255});
var clone=UnityEngine.Object.Instantiate(original);
byte[] patch=null;clone.Changed=(t,x,y,w,h,pixels)=>patch=pixels;
clone.SetPixels(1,0,1,1,new[]{new Color(0,0,1,.5f)});clone.Apply();
Check(original.GetPixel(1,0).g==1,"cloned writes must not modify original texture");
Check(clone.GetPixel(1,0).b==1&&patch.SequenceEqual(new byte[]{0,0,255,128}),"pixel patch reaches observer in original RGBA order");
var go=new GameObject("source limb renderer",first);var renderer=go.AddComponent<MeshRenderer>();
var bloodMask=new Texture2D(1,1,new byte[4]);
renderer.material.SetTexture("_Bloodmask",bloodMask);
Check(ReferenceEquals(renderer.material.GetTexture("_Bloodmask"),bloodMask),"CPU material retains the source blood-mask texture binding");
Throws<NotSupportedException>(()=>renderer.material.SetTexture("_DamageMask",null));
Throws<NotSupportedException>(()=>GL.PushMatrix());
Throws<NotSupportedException>(()=>{var value=RenderTexture.active;});
var graphics=new GraphicsProbe();RenderTexture.Bind(first,graphics);GL.Bind(first,graphics);
renderer.material.OnTextureAssigned=(property,texture)=>graphics.Ops.Add(property);
var target=new RenderTexture();RenderTexture.active=target;
Check(ReferenceEquals(RenderTexture.active,target),"render target identity retained");
renderer.material.SetTexture("_DamageMask",target);
GL.PushMatrix();GL.LoadPixelMatrix(0,2,1,0);GL.Begin(4);GL.Color(new Color(1,0,0,1));GL.Vertex3(0,0,0);GL.End();GL.PopMatrix();RenderTexture.active=null;
Check(graphics.Ops.SequenceEqual(new[]{"target","_DamageMask","push","matrix","begin4","color","vertex","end","pop","restore"}),"target/material/GL transaction order");
Throws<NotSupportedException>(()=>GL.Begin(9));
var mesh=new Mesh();mesh.vertices=new[]{Vector3.zero,Vector3.right,Vector3.up};mesh.triangles=new[]{0,1,2};
mesh.uv=new[]{Vector2.zero,new Vector2(1,0),new Vector2(0,1)};mesh.RecalculateNormals();
var vertices=mesh.vertices;vertices[0]=Vector3.up;
Check(mesh.vertices[0].sqrMagnitude==0,"mesh caller cannot mutate source vertices");
Check(mesh.uv.Length==3&&mesh.normals.Length==3&&Math.Abs(mesh.normals[0].z-1)<1e-6,
      "mesh UVs and recalculated normals follow source vertex and triangle data");
var meshOwner=new GameObject("original source mesh filter",first);var meshFilter=meshOwner.AddComponent<MeshFilter>();meshFilter.mesh=mesh;
Check(ReferenceEquals(meshFilter.sharedMesh,mesh),"MeshFilter retains the source-assigned mesh identity");
Throws<ArgumentException>(()=>mesh.triangles=new[]{0,1,3});
mesh.Clear();Check(mesh.vertexCount==0&&mesh.uv.Length==0&&mesh.normals.Length==0,"Mesh.Clear resets source mesh buffers");
var second=new RuntimeWorld();second.MakeCurrent();
Throws<NotSupportedException>(()=>GL.End());
Throws<NotSupportedException>(()=>{var value=RenderTexture.active;});
first.MakeCurrent();Check(RenderTexture.active==null,"world target restored");
// Verify new Unity-compatible API surfaces without claiming live rendering.
var eventOrder=new List<int>();
var typedEvent=new UnityEngine.Events.UnityEvent<int>();
UnityEngine.Events.UnityAction<int> firstListener=value=>eventOrder.Add(value);
UnityEngine.Events.UnityAction<int> secondListener=value=>eventOrder.Add(value+1);
typedEvent.AddListener(firstListener);typedEvent.AddListener(secondListener);typedEvent.Invoke(3);
Check(eventOrder.SequenceEqual(new[]{3,4}),"typed UnityEvent listeners execute in registration order");
typedEvent.RemoveListener(firstListener);typedEvent.Invoke(5);
Check(eventOrder.SequenceEqual(new[]{3,4,6}),"removing a typed listener preserves other callbacks");
typedEvent.RemoveAllListeners();typedEvent.Invoke(7);
Check(eventOrder.Count==3,"removed typed listeners do not execute");
var emitterGo=new GameObject("original emitter module",first);
var emitterBody=emitterGo.AddComponent<Rigidbody>();emitterBody.velocity=new Vector3(.5f,0f,-1f);
var emitterChild=new GameObject("particle system child",first);emitterChild.transform.SetParent(emitterGo.transform);
var emitter=emitterChild.AddComponent<ParticleSystem>();
var mainModule=emitter.main;var shapeModule=emitter.shape;
mainModule.maxParticles=48;mainModule.startSpeed=new ParticleSystem.MinMaxCurve(2f,5f);
mainModule.startLifetime=new ParticleSystem.MinMaxCurve(4f,7f);mainModule.startSizeMultiplier=1.5f;emitter.randomSeed=123456789;
shapeModule.radius=.4f;shapeModule.angle=21f;
Check(emitter.main.maxParticles==48 && emitter.main.startSpeed.constantMax==5f &&
      emitter.shape.radius==.4f && emitter.shape.angle==21f,"particle module value handles mutate shared state");
Throws<ArgumentOutOfRangeException>(()=>{mainModule.maxParticles=-1;});
Throws<NotSupportedException>(()=>emitter.Emit(1));
var particleQueue=new Crisis.Runtime.BrowserParticleEventQueue();first.ParticleSystemDriver=particleQueue;emitter.Driver=particleQueue;
emitter.OriginalSerializedFields["m_ParticleSystem"]=System.Text.Json.JsonDocument.Parse("{\"serializedVersion\":6}").RootElement.Clone();
emitter.OriginalAssetGuid="0123456789abcdef0123456789abcdef";emitter.OriginalAssetFileID="19800000";emitter.OriginalAssetPath="Assets/Effects/Original.prefab";emitter.OriginalAssetSha256=new string('a',64);
var particleRenderer=emitterChild.AddComponent<ParticleSystemRenderer>();particleRenderer.OriginalAssetGuid=emitter.OriginalAssetGuid;particleRenderer.OriginalAssetFileID="19900000";particleRenderer.OriginalAssetPath=emitter.OriginalAssetPath;particleRenderer.OriginalAssetSha256=emitter.OriginalAssetSha256;particleRenderer.OriginalSerializedFields["m_RenderMode"]=System.Text.Json.JsonDocument.Parse("0").RootElement.Clone();
emitter.Emit(3);emitter.Simulate(.25f,false,true,false);emitter.Play();
var particleCommands=particleQueue.Drain();
Check(particleCommands.Length==3&&particleQueue.DroppedEvents==0,"source particle emit/simulate/play callbacks drain as bounded browser events");
var particleJson=System.Text.Json.JsonSerializer.Serialize(particleCommands);
Check(particleJson.Contains("Assets/Effects/Original.prefab")&&particleJson.Contains("19800000")&&particleJson.Contains("19900000")&&particleJson.Contains(new string('a',64))&&particleJson.Contains("sourceAssetSha256")&&particleJson.Contains("sourceFields")&&particleJson.Contains("m_ParticleSystem")&&particleJson.Contains("m_RenderMode")&&particleJson.Contains("attachedRigidbodyVelocity")&&particleJson.Contains("-1")&&particleJson.Contains("emit")&&particleJson.Contains("simulate")&&particleJson.Contains("play"),"particle events retain original component and renderer fields, asset identity, Rigidbody velocity, and command payloads");
Check(particleJson.Contains("123456789")&&particleJson.Contains("startLifetime")&&particleJson.Contains("startSizeMultiplier"),"particle Emit callbacks preserve the authored lifetime, size multiplier, and source random seed");
var emitterClone=UnityEngine.Object.Instantiate(emitterChild);
var clonedEmitter=emitterClone.GetComponent<ParticleSystem>()!;
Check(clonedEmitter.OriginalAssetGuid==emitter.OriginalAssetGuid&&clonedEmitter.OriginalAssetFileID==emitter.OriginalAssetFileID&&clonedEmitter.OriginalAssetPath==emitter.OriginalAssetPath&&clonedEmitter.OriginalAssetSha256==emitter.OriginalAssetSha256,"instantiated particle components retain original component and source-asset provenance");
var clonedParticleRenderer=emitterClone.GetComponent<ParticleSystemRenderer>()!;
Check(clonedParticleRenderer.OriginalAssetSha256==particleRenderer.OriginalAssetSha256&&clonedParticleRenderer.OriginalSerializedFields.ContainsKey("m_RenderMode"),"instantiated particle renderers retain source identity and authored renderer fields");
for(var i=0;i<257;i++)emitter.Play();
Check(particleQueue.Drain().Length==256&&particleQueue.DroppedEvents==1,"particle event transport bounds per-tick queue growth and counts dropped requests");
first.ParticleSystemDriver=null;emitter.Driver=null;
var probe=emitterGo.AddComponent<ReflectionProbe>();
Throws<NotSupportedException>(()=>probe.RenderProbe());
probe.timeSlicingMode=UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces;
Check(probe.timeSlicingMode==UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces,
      "original reflection probe slicing value round-trips");
var shaderObject=new Shader{name="original image effect shader",isSupported=true};Shader.Resolver=name=>name=="original image effect shader"?shaderObject:null;
Check(ReferenceEquals(Shader.Find("original image effect shader"),shaderObject)&&Shader.Find("missing") is null,
      "shader lookup resolves only assets supplied by the host");
var compute=new ComputeBuffer(2,4);var sourceValues=new[]{4,7};var targetValues=new int[2];compute.SetData(sourceValues);compute.GetData(targetValues);
Check(targetValues.SequenceEqual(sourceValues),"compute buffer CPU transfer preserves typed source values");compute.Release();
Throws<ObjectDisposedException>(()=>compute.SetData(sourceValues));
Check(typeof(MetadataProbe).GetCustomAttributes(typeof(RequireComponentAttribute),true).Length==1,
      "Unity required component metadata is retained");
var movingGraph = new AnimatorGraphDriver("Idle", new[] {
    new AnimatorGraphTransition("Idle", "Run", new[] { new AnimatorGraphCondition("Moving", 3, 0.01f) })
});
movingGraph.SetFloat("Moving", 1f);
movingGraph.Evaluate(1f / 60f, 1f);
Check(movingGraph.CurrentState == "Run", "Animator graph takes a condition-matched transition");
var timedGraph = new AnimatorGraphDriver("Run", new[] {
    new AnimatorGraphTransition("Run", "Idle", Array.Empty<AnimatorGraphCondition>(), true, 0.5f)
}, new Dictionary<string, float> { ["Run"] = 0.5f });
timedGraph.Evaluate(0.2f, 1f);
Check(timedGraph.CurrentState == "Run", "Animator graph waits for normalized exit time");
timedGraph.Evaluate(0.05f, 2f);
Check(timedGraph.CurrentState == "Idle", "Animator graph applies playback speed to exit-time progression");
var triggerGraph = new AnimatorGraphDriver("Idle", new[] {
    new AnimatorGraphTransition("Idle", "Run", new[] { new AnimatorGraphCondition("Fire", 1, 0f) }),
    new AnimatorGraphTransition("Run", "Idle", new[] { new AnimatorGraphCondition("Fire", 1, 0f) })
});
triggerGraph.SetTrigger("Fire");
triggerGraph.Evaluate(1f / 60f, 1f);
triggerGraph.Evaluate(1f / 60f, 1f);
Check(triggerGraph.CurrentState == "Run", "Animator graph consumes trigger parameters once");
var componentGraph = new AnimatorGraphDriver("Idle", new[] {
    new AnimatorGraphTransition("Idle", "Run", new[] { new AnimatorGraphCondition("Fire", 1, 0f) }),
    new AnimatorGraphTransition("Run", "Idle", new[] { new AnimatorGraphCondition("Fire", 1, 0f) })
});
var animatorOwner = new GameObject("source animator trigger api", first);
var sourceAnimator = animatorOwner.AddComponent<Animator>();
sourceAnimator.Driver = componentGraph;
sourceAnimator.SetTrigger("Fire");
sourceAnimator.Update(1f / 60f);
Check(componentGraph.CurrentState == "Run", "Animator.SetTrigger forwards original named trigger to the graph driver");
sourceAnimator.Update(1f / 60f);
Check(componentGraph.CurrentState == "Run", "Animator trigger remains one-shot through component update");
sourceAnimator.SetTrigger("Fire");
sourceAnimator.Update(1f / 60f);
Check(componentGraph.CurrentState == "Idle", "Animator.SetTrigger can trigger the next graph edge");
sourceAnimator.SetTrigger("Fire");
sourceAnimator.ResetTrigger("Fire");
sourceAnimator.Update(1f / 60f);
Check(componentGraph.CurrentState == "Idle", "Animator.ResetTrigger cancels a pending trigger");
Throws<InvalidOperationException>(() => sourceAnimator.GetBool("Fire"));
assertions+=TextureTransferContracts.Run();
Console.WriteLine($"PASS: {assertions} isolated engine graphics and source API assertions; no gameplay or full CharacterBody compiled.");

sealed class GraphicsProbe:IRenderTargetDriver,IGLDriver {
 public readonly List<string> Ops=new();
 public RenderTexture ActiveTarget{get;private set;}
 public void SetActive(RenderTexture value){ActiveTarget=value;Ops.Add(value==null?"restore":"target");}
 public void PushMatrix()=>Ops.Add("push");public void PopMatrix()=>Ops.Add("pop");
 public void LoadPixelMatrix(float l,float r,float b,float t)=>Ops.Add("matrix");
 public void Begin(int mode)=>Ops.Add("begin"+mode);public void End()=>Ops.Add("end");
 public void Color(Color color)=>Ops.Add("color");public void Vertex3(float x,float y,float z)=>Ops.Add("vertex");
}

[RequireComponent(typeof(Camera))]
sealed class MetadataProbe:MonoBehaviour { }
