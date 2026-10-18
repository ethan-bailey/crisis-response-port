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
var vertices=mesh.vertices;vertices[0]=Vector3.up;
Check(mesh.vertices[0].sqrMagnitude==0,"mesh caller cannot mutate source vertices");
Throws<ArgumentException>(()=>mesh.triangles=new[]{0,1,3});
var second=new RuntimeWorld();second.MakeCurrent();
Throws<NotSupportedException>(()=>GL.End());
Throws<NotSupportedException>(()=>{var value=RenderTexture.active;});
first.MakeCurrent();Check(RenderTexture.active==null,"world target restored");
// Verify new Unity-compatible API surfaces without claiming live rendering.
var eventOrder=new List<int>();
var typedEvent=new UnityEngine.Events.UnityEvent<int>();
Action<int> firstListener=value=>eventOrder.Add(value);
Action<int> secondListener=value=>eventOrder.Add(value+1);
typedEvent.AddListener(firstListener);typedEvent.AddListener(secondListener);typedEvent.Invoke(3);
Check(eventOrder.SequenceEqual(new[]{3,4}),"typed UnityEvent listeners execute in registration order");
typedEvent.RemoveListener(firstListener);typedEvent.Invoke(5);
Check(eventOrder.SequenceEqual(new[]{3,4,6}),"removing a typed listener preserves other callbacks");
typedEvent.RemoveAllListeners();typedEvent.Invoke(7);
Check(eventOrder.Count==3,"removed typed listeners do not execute");
var emitterGo=new GameObject("original emitter module",first);
var emitter=emitterGo.AddComponent<ParticleSystem>();
var mainModule=emitter.main;var shapeModule=emitter.shape;
mainModule.maxParticles=48;mainModule.startSpeed=new ParticleSystem.MinMaxCurve(2f,5f);
shapeModule.radius=.4f;shapeModule.angle=21f;
Check(emitter.main.maxParticles==48 && emitter.main.startSpeed.constantMax==5f &&
      emitter.shape.radius==.4f && emitter.shape.angle==21f,"particle module value handles mutate shared state");
Throws<ArgumentOutOfRangeException>(()=>{mainModule.maxParticles=-1;});
Throws<NotSupportedException>(()=>emitter.Emit(1));
var probe=emitterGo.AddComponent<ReflectionProbe>();
Throws<NotSupportedException>(()=>probe.RenderProbe());
probe.timeSlicingMode=UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces;
Check(probe.timeSlicingMode==UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces,
      "original reflection probe slicing value round-trips");
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
