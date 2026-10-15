using System.Text.Json;
using UnityEngine;
namespace UnityEngine {
 public struct Color {public float r,g,b,a;public Color(float r,float g,float b,float a){this.r=r;this.g=g;this.b=b;this.a=a;}}
 public struct Rect {public float x,y,width,height;public Rect(float x,float y,float w,float h){this.x=x;this.y=y;width=w;height=h;}public Vector2 min=>new(x,y);public Vector2 max=>new(x+width,y+height);}
 // Read/write CPU texture adapter for the original glass alpha decisions. Pixels
 // are stored bottom-left first, as Unity Texture2D.GetPixels expects.
 public sealed class Texture2D:Object {
  public int width,height;byte[] rgba;public string SourceID;public Action<Texture2D,int,int,int,int,byte[]> Changed;
  public Texture2D(int w,int h,byte[] bytes,RuntimeWorld owner=null){world=owner??RuntimeWorld.Current;width=w;height=h;rgba=(byte[])bytes.Clone();world.Register(this);}
  public Texture2D CloneTexture(RuntimeWorld owner)=>new(width,height,rgba,owner){SourceID=SourceID,Changed=Changed};
  public Color GetPixel(int x,int y){int i=(y*width+x)*4;return new(rgba[i]/255f,rgba[i+1]/255f,rgba[i+2]/255f,rgba[i+3]/255f);}
  public Color[] GetPixels(int x,int y,int w,int h,int mip=0){var result=new Color[w*h];for(int row=0;row<h;row++)for(int col=0;col<w;col++)result[row*w+col]=GetPixel(x+col,y+row);return result;}
  public void SetPixels(int x,int y,int w,int h,Color[] values){var patch=new byte[w*h*4];for(int row=0;row<h;row++)for(int col=0;col<w;col++){var c=values[row*w+col];float[] v=[c.r,c.g,c.b,c.a];for(int channel=0;channel<4;channel++){byte b=(byte)Math.Clamp((int)MathF.Round(v[channel]*255),0,255);rgba[((y+row)*width+x+col)*4+channel]=b;patch[(row*w+col)*4+channel]=b;}}Changed?.Invoke(this,x,y,w,h,patch);}
  public void Apply(){} // CPU writes are immediate; there is no native GPU copy in this adapter.
 }
 public sealed class Material:Object {public Texture2D mainTexture;}
 public sealed class Renderer:Component {public Material material=new();}
 // Imported source vertices. Convex planes are the hull of those vertices; this
 // is not a claim about matching Unity's PhysX cooking/reduction algorithm.
 public sealed class MeshCollider:Collider {
  public Vector3[] vertices;public int[][] triangles;public float[][] planes;public bool convex;
  public override bool Raycast(Ray ray,out RaycastHit hit,float distance){
   hit=default;if(!isActiveAndEnabled)return false;var o=transform.InverseTransformPoint(ray.origin);var d=transform.InverseTransformVector(ray.direction);
   float near=0,far=distance;Vector3 normal=Vector3.zero;
   if(convex){bool inside=true;foreach(var p in planes)if(p[0]*o.x+p[1]*o.y+p[2]*o.z+p[3]>1e-6f)inside=false;if(inside)return false;
    foreach(var p in planes){var n=new Vector3(p[0],p[1],p[2]);float a=Vector3.Dot(n,o)+p[3],b=Vector3.Dot(n,d);if(MathF.Abs(b)<1e-9f){if(a>0)return false;continue;}float t=-a/b;if(b<0){if(t>near){near=t;normal=n;}}else far=MathF.Min(far,t);if(near>far)return false;}
   }else{near=distance;bool found=false;foreach(var t in triangles){var a=vertices[t[0]];var e1=vertices[t[1]]-a;var e2=vertices[t[2]]-a;var p=Vector3.Cross(d,e2);float det=Vector3.Dot(e1,p);if(det<=1e-8f)continue;var s=o-a;float u=Vector3.Dot(s,p)/det;if(u<0||u>1)continue;var q=Vector3.Cross(s,e1);float v=Vector3.Dot(d,q)/det;if(v<0||u+v>1)continue;float parameter=Vector3.Dot(e2,q)/det;if(parameter>0&&parameter<near){near=parameter;normal=Vector3.Cross(e1,e2).normalized;found=true;}}if(!found)return false;}
   if(near<=0||near>distance||normal.sqrMagnitude==0)return false;hit=new(){collider=this,distance=near,point=ray.GetPoint(near),normal=transform.TransformNormal(normal)};return true;
  }
 }
}
[Serializable] public sealed class ProjectileEvent {event Action<Projectile> listeners;public void AddListener(Action<Projectile> action)=>listeners+=action;public void Invoke(Projectile p)=>listeners?.Invoke(p);}
public sealed class ProjectileTrace:Component {public void Initialize(Projectile p)=>throw new NotSupportedException("Source M4 trace prefab is null");public void AddPosition(Vector3 p)=>throw new NotSupportedException("Trace not configured");public void SpawnHitEffect(Vector3 p,Transform t,Vector3 n)=>throw new NotSupportedException("Trace not configured");}
public sealed class AudioInstigator:Component {}
public static class MarksmanMode {public static float DeflectionModifier=.15f;}
public sealed class Masks {public static Masks Instance=new();public int Limbs,BallisticTargets,BallisticTargetsSansLimbs;}
public sealed class MissionHelper {public static MissionHelper Instance=new();public Transform EffectsContainer;}
