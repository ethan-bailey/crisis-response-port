using UnityEngine;
namespace UnityEngine {
 public struct Color {public static Color black=>new(0,0,0,1);public static Color white=>new(1,1,1,1);public static Color clear=>new(0,0,0,0);public float r,g,b,a;public Color(float r,float g,float b,float a=1){this.r=r;this.g=g;this.b=b;this.a=a;}}
 public struct Rect {
  public float x,y,width,height;
  public Rect(float x,float y,float w,float h){this.x=x;this.y=y;width=w;height=h;}
  public float xMin {get=>x;set{float end=xMax;x=value;width=end-x;}}
  public float yMin {get=>y;set{float end=yMax;y=value;height=end-y;}}
  public float xMax {get=>x+width;set=>width=value-x;}
  public float yMax {get=>y+height;set=>height=value-y;}
  public Vector2 min=>new(x,y);public Vector2 max=>new(x+width,y+height);
 }
 // Read/write CPU texture adapter for the original glass alpha decisions. Pixels
 // are stored bottom-left first, as Unity Texture2D.GetPixels expects.
 public sealed class Material:Object {
  public Material(){}
  public Material(Shader shader){this.shader=shader??throw new ArgumentNullException(nameof(shader));}
  public Shader shader {get;set;}
  public HideFlags hideFlags {get;set;}
  public int passCount {get;set;}
  public bool SetPass(int pass){if(pass<0||pass>=passCount)throw new ArgumentOutOfRangeException(nameof(pass));return false;}
  Texture2D texture;
  readonly Dictionary<string,Color> colors=new();
  // Presentation observer only: source BallisticWindow.Reset assigns mainTexture
  // on a freshly cloned original CPU texture. Rebind browser stamp forwarding
  // without changing the recovered source Reset or StampAlpha algorithms.
  // Material property values are retained by the CPU adapter; an attached
  // observer may forward them to a renderer without claiming GPU execution.
  public Action<string,Texture> OnTextureAssigned;
  readonly Dictionary<string,Texture> textures=new();
  public void SetTexture(string property,Texture value){
   ArgumentException.ThrowIfNullOrEmpty(property);
   if(OnTextureAssigned==null && !(property=="_Bloodmask" && value is Texture2D))
    throw new NotSupportedException("Source material texture driver is not bound");
   OnTextureAssigned?.Invoke(property,value);
   textures[property]=value;
   if(property=="_MainTex" && value is Texture2D image)mainTexture=image;
  }
  public Texture GetTexture(string property){ArgumentException.ThrowIfNullOrEmpty(property);return textures.GetValueOrDefault(property);}
  public Action<Texture2D> OnMainTextureAssigned;
  // Material values retain exact CPU-side source settings. A renderer or
  // browser presentation host can observe changes without inventing pixels.
  public Action<string,Color> OnColorAssigned;
  public void SetColor(string property,Color value){ArgumentException.ThrowIfNullOrEmpty(property);colors[property]=value;OnColorAssigned?.Invoke(property,value);}
  public Color GetColor(string property){ArgumentException.ThrowIfNullOrEmpty(property);return colors.GetValueOrDefault(property);}
  public Texture2D mainTexture {
   get=>texture;
   set {texture=value;OnMainTextureAssigned?.Invoke(value);}
  }
 }
 public class Renderer:Component {
  Material singleMaterial=new();Material[] materialInstances;
  public Action<Material[]> OnMaterialsAssigned;
  public Material material {
   get=>materialInstances is {Length:>0}?materialInstances[0]:singleMaterial;
   set {singleMaterial=value??throw new ArgumentNullException(nameof(value));if(materialInstances is {Length:>0})materialInstances[0]=value;}
  }
  public Material[] materials {
   get=>materialInstances is null?new[]{singleMaterial}:(Material[])materialInstances.Clone();
   set {
    ArgumentNullException.ThrowIfNull(value);
    if(Array.Exists(value,item=>item==null))throw new ArgumentException("Renderer materials cannot contain null",nameof(value));
    materialInstances=(Material[])value.Clone();
    if(materialInstances.Length>0)singleMaterial=materialInstances[0];
    OnMaterialsAssigned?.Invoke((Material[])materialInstances.Clone());
   }
  }
 }
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
