using UnityEngine;
namespace UnityEngine {
 public struct Color {public static Color black=>new(0,0,0,1);public float r,g,b,a;public Color(float r,float g,float b,float a){this.r=r;this.g=g;this.b=b;this.a=a;}}
 public struct Rect {public float x,y,width,height;public Rect(float x,float y,float w,float h){this.x=x;this.y=y;width=w;height=h;}public Vector2 min=>new(x,y);public Vector2 max=>new(x+width,y+height);}
 // Read/write CPU texture adapter for the original glass alpha decisions. Pixels
 // are stored bottom-left first, as Unity Texture2D.GetPixels expects.
 public sealed class Texture2D:Texture {
  public int width,height;byte[] rgba;public string SourceID;public Action<Texture2D,int,int,int,int,byte[]> Changed;
  public Texture2D(int w,int h,byte[] bytes,RuntimeWorld owner=null){world=owner??RuntimeWorld.Current;width=w;height=h;rgba=(byte[])bytes.Clone();world.Register(this);}
  public Texture2D CloneTexture(RuntimeWorld owner)=>new(width,height,rgba,owner){SourceID=SourceID,Changed=Changed};
  public Color GetPixel(int x,int y){int i=(y*width+x)*4;return new(rgba[i]/255f,rgba[i+1]/255f,rgba[i+2]/255f,rgba[i+3]/255f);}
  public Color[] GetPixels(int x,int y,int w,int h,int mip=0){var result=new Color[w*h];for(int row=0;row<h;row++)for(int col=0;col<w;col++)result[row*w+col]=GetPixel(x+col,y+row);return result;}
  public void SetPixels(int x,int y,int w,int h,Color[] values){var patch=new byte[w*h*4];for(int row=0;row<h;row++)for(int col=0;col<w;col++){var c=values[row*w+col];float[] v=[c.r,c.g,c.b,c.a];for(int channel=0;channel<4;channel++){byte b=(byte)Math.Clamp((int)MathF.Round(v[channel]*255),0,255);rgba[((y+row)*width+x+col)*4+channel]=b;patch[(row*w+col)*4+channel]=b;}}Changed?.Invoke(this,x,y,w,h,patch);}
  public void Apply(){} // CPU writes are immediate; there is no native GPU copy in this adapter.
 }
 public sealed class Material:Object {
  Texture2D texture;
  // Presentation observer only: source BallisticWindow.Reset assigns mainTexture
  // on a freshly cloned original CPU texture. Rebind browser stamp forwarding
  // without changing the recovered source Reset or StampAlpha algorithms.
  // Source limb skin material properties require an attached graphics driver.
  // This records no fictitious pixels or GPU state when the driver is absent.
  public Action<string,Texture> OnTextureAssigned;
  public void SetTexture(string property,Texture value){
   ArgumentException.ThrowIfNullOrEmpty(property);
   if(OnTextureAssigned==null)throw new NotSupportedException("Source material texture driver is not bound");
   OnTextureAssigned(property,value);
   if(property=="_MainTex" && value is Texture2D image)mainTexture=image;
  }
  public Action<Texture2D> OnMainTextureAssigned;
  public Texture2D mainTexture {
   get=>texture;
   set {texture=value;OnMainTextureAssigned?.Invoke(value);}
  }
 }
 public class Renderer:Component {public Material material=new();}
 public sealed class MeshRenderer:Renderer {}
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
