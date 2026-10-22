using UnityEngine;
namespace UnityEngine {
 public struct Color {
  public static Color black=>new(0,0,0,1);public static Color white=>new(1,1,1,1);public static Color clear=>new(0,0,0,0);
  public static Color red=>new(1,0,0,1);public static Color green=>new(0,1,0,1);public static Color blue=>new(0,0,1,1);
  public static Color yellow=>new(1,0.9215686f,0.01568628f,1);public static Color cyan=>new(0,1,1,1);public static Color magenta=>new(1,0,1,1);public static Color gray=>new(0.5f,0.5f,0.5f,1);public static Color grey=>gray;
  public float r,g,b,a;public Color(float r,float g,float b,float a=1){this.r=r;this.g=g;this.b=b;this.a=a;}
  public static Color Lerp(Color from,Color to,float t)=>LerpUnclamped(from,to,Mathf.Clamp01(t));
  public static Color LerpUnclamped(Color from,Color to,float t)=>new(Mathf.LerpUnclamped(from.r,to.r,t),Mathf.LerpUnclamped(from.g,to.g,t),Mathf.LerpUnclamped(from.b,to.b,t),Mathf.LerpUnclamped(from.a,to.a,t));
 }
 public struct Rect {
  public float x,y,width,height;
  public Rect(float x,float y,float w,float h){this.x=x;this.y=y;width=w;height=h;}
  public float xMin {get=>x;set{float end=xMax;x=value;width=end-x;}}
  public float yMin {get=>y;set{float end=yMax;y=value;height=end-y;}}
  public float xMax {get=>x+width;set=>width=value-x;}
  public float yMax {get=>y+height;set=>height=value-y;}
  public Vector2 center {get=>new(x+width*0.5f,y+height*0.5f);set{x=value.x-width*0.5f;y=value.y-height*0.5f;}}
  public Vector2 size {get=>new(width,height);set{width=value.x;height=value.y;}}
  public Vector2 min=>new(x,y);public Vector2 max=>new(x+width,y+height);
 }
 // Read/write CPU texture adapter for the original glass alpha decisions. Pixels
 // are stored bottom-left first, as Unity Texture2D.GetPixels expects.
 public sealed class Material:Object {
  public Material(){}
  public Material(Shader shader){this.shader=shader??throw new ArgumentNullException(nameof(shader));}
  public Shader shader {get;set;}
  public int renderQueue {get;set;}=-1;
  public bool enableInstancing {get;set;}
  public bool doubleSidedGI {get;set;}
  public int lightmapFlags {get;set;}
  public HideFlags hideFlags {get;set;}
  public int passCount {get;set;}
  public bool SetPass(int pass){if(pass<0||pass>=passCount)throw new ArgumentOutOfRangeException(nameof(pass));return false;}
  Texture2D texture;
  readonly Dictionary<string,Color> colors=new();
  readonly Dictionary<string,float> floats=new();
  readonly Dictionary<string,Vector4> vectors=new();
  readonly Dictionary<string,Matrix4x4> matrices=new();
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
  public Color color {get=>GetColor("_Color");set=>SetColor("_Color",value);}
  public void SetFloat(string property,float value){ArgumentException.ThrowIfNullOrEmpty(property);floats[property]=value;}
  public float GetFloat(string property){ArgumentException.ThrowIfNullOrEmpty(property);return floats.GetValueOrDefault(property);}
  public void SetVector(string property,Vector4 value){ArgumentException.ThrowIfNullOrEmpty(property);vectors[property]=value;}
  public Vector4 GetVector(string property){ArgumentException.ThrowIfNullOrEmpty(property);return vectors.GetValueOrDefault(property);}
  public void SetMatrix(string property,Matrix4x4 value){ArgumentException.ThrowIfNullOrEmpty(property);matrices[property]=value;}
  public void SetBuffer(string property,ComputeBuffer value){ArgumentException.ThrowIfNullOrEmpty(property);ArgumentNullException.ThrowIfNull(value);throw new NotSupportedException("Shader buffer binding requires a GPU graphics driver");}
  public Matrix4x4 GetMatrix(string property){ArgumentException.ThrowIfNullOrEmpty(property);return matrices.GetValueOrDefault(property);}
  public Texture2D mainTexture {
   get=>texture;
   set {texture=value;OnMainTextureAssigned?.Invoke(value);}
  }
 }
 public class Renderer:Behaviour {
  public bool castShadows=true,receiveShadows=true,dynamicOccludee=true,preserveUVs,ignoreNormalsForChartDetection,importantGI,stitchLightmapSeams;
  public int motionVectors,lightProbeUsage,reflectionProbeUsage,selectedEditorRenderState,minimumChartSize,sortingLayerID,sortingLayer,sortingOrder;
  public float scaleInLightmap=1,autoUVMaxDistance,autoUVMaxAngle;
  public StaticBatchInfo staticBatchInfo;
  public new Transform staticBatchRoot,probeAnchor,lightProbeVolumeOverride;
  public Object lightmapParameters;
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

    materialInstances=(Material[])value.Clone();
    if(materialInstances.Length>0)singleMaterial=materialInstances[0];
    OnMaterialsAssigned?.Invoke((Material[])materialInstances.Clone());
   }
  }
 }
 public sealed class MeshRenderer:Renderer {}
public sealed class ParticleSystemRenderer:Renderer {
    public int renderMode { get; set; }
    public int sortMode { get; set; }
    public float lengthScale { get; set; }
    public int renderAlignment { get; set; }
    public bool useCustomVertexStreams { get; set; }
    public int vertexStreams { get; set; }
    public Vector3 pivot { get; set; }
    public Vector3 flip { get; set; }
    public float cameraVelocityScale { get; set; }
    public float velocityScale { get; set; }
    public float sortingFudge { get; set; }
    public float minParticleSize { get; set; }
    public float maxParticleSize { get; set; }
    public Mesh mesh { get; set; }
    public Mesh mesh1 { get; set; }
    public Mesh mesh2 { get; set; }
    public Mesh mesh3 { get; set; }
    public int maskInteraction { get; set; }
    public int normalDirection { get; set; }
    public Transform? staticBatchRoot { get; set; }
}

 public struct StaticBatchInfo { public uint firstSubMesh,subMeshCount; }
 // Imported source vertices. Convex planes are the hull of those vertices; this
 // is not a claim about matching Unity's PhysX cooking/reduction algorithm.
 public sealed class MeshCollider:Collider {
  public Vector3[] vertices;public int[][] triangles;public float[][] planes;public bool convex;public Mesh sharedMesh {get;set;} public bool inflateMesh {get;set;} public float skinWidth {get;set;}
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
