using N = System.Numerics;
namespace UnityEngine;

// Serialized Unity value types consumed by the recovered Behavior Designer
// readers. These are data, not replacements for original game decisions.
public struct Vector4 {
    public float x,y,z,w;
    public static Vector4 zero=>new(0,0,0,0);
    public static Vector4 one=>new(1,1,1,1);
    public Vector4(float x,float y,float z,float w){this.x=x;this.y=y;this.z=z;this.w=w;}
    public static implicit operator Vector4(Vector3 value)=>new(value.x,value.y,value.z,0);
    public static Vector4 operator *(Vector4 value,float scale)=>new(value.x*scale,value.y*scale,value.z*scale,value.w*scale);
    public static Vector4 operator *(float scale,Vector4 value)=>value*scale;
    public static Vector4 operator +(Vector4 a,Vector4 b)=>new(a.x+b.x,a.y+b.y,a.z+b.z,a.w+b.w);
    public float this[int i] {get=>i switch{0=>x,1=>y,2=>z,3=>w,_=>throw new IndexOutOfRangeException()};set{switch(i){case 0:x=value;break;case 1:y=value;break;case 2:z=value;break;case 3:w=value;break;default:throw new IndexOutOfRangeException();}}}
}
public struct Matrix4x4 {
    N.Matrix4x4 data;
    public static Matrix4x4 identity=>new(){data=N.Matrix4x4.Identity};
    public float m00 {get=>this[0,0];set=>this[0,0]=value;}
    public float m01 {get=>this[0,1];set=>this[0,1]=value;}
    public float m02 {get=>this[0,2];set=>this[0,2]=value;}
    public float m03 {get=>this[0,3];set=>this[0,3]=value;}
    public float m10 {get=>this[1,0];set=>this[1,0]=value;}
    public float m11 {get=>this[1,1];set=>this[1,1]=value;}
    public float m12 {get=>this[1,2];set=>this[1,2]=value;}
    public float m13 {get=>this[1,3];set=>this[1,3]=value;}
    public float m20 {get=>this[2,0];set=>this[2,0]=value;}
    public float m21 {get=>this[2,1];set=>this[2,1]=value;}
    public float m22 {get=>this[2,2];set=>this[2,2]=value;}
    public float m23 {get=>this[2,3];set=>this[2,3]=value;}
    public float m30 {get=>this[3,0];set=>this[3,0]=value;}
    public float m31 {get=>this[3,1];set=>this[3,1]=value;}
    public float m32 {get=>this[3,2];set=>this[3,2]=value;}
    public float m33 {get=>this[3,3];set=>this[3,3]=value;}

    public float this[int row,int column] {get=>data[row,column];set=>data[row,column]=value;}
    // Unity serializes the one-dimensional index in column-major order.
    public float this[int index] {get=>this[index%4,index/4];set=>this[index%4,index/4]=value;}
    public static Matrix4x4 Inverse(Matrix4x4 value){if(!N.Matrix4x4.Invert(value.data,out var result))throw new InvalidOperationException("Matrix is singular");return new(){data=result};}
    public static Matrix4x4 operator *(Matrix4x4 a,Matrix4x4 b)=>new(){data=a.data*b.data};
    public static Vector4 operator *(Matrix4x4 matrix,Vector4 value){var result=N.Vector4.Transform(new N.Vector4(value.x,value.y,value.z,value.w),matrix.data);return new(result.X,result.Y,result.Z,result.W);}
    public void SetTRS(Vector3 position,Quaternion rotation,Vector3 scale)=>data=N.Matrix4x4.CreateScale(scale.Native)*N.Matrix4x4.CreateFromQuaternion(rotation.Native)*N.Matrix4x4.CreateTranslation(position.Native);
}
public struct LayerMask {
    static readonly string[] layerNames={
        "Default","TransparentFX","Ignore Raycast","", "Water","UI","","",
        "Doors","Characters","Obstacle geometry","Exterior geometry","Physical props","Projectiles","Limbs","Casings",
        "Fog of war","Triggers","Weapons","AimQuad","Gore","Interactables","Rooms","Grenades",
        "Limbs arms","Render to texture","Wall geometry","Windows","Smoke","Ethereal props","",""
    };
    public int value;
    public static implicit operator int(LayerMask mask)=>mask.value;
    public static implicit operator LayerMask(int value)=>new(){value=value};
    public static int NameToLayer(string name)=>Array.IndexOf(layerNames,name);
    public static int GetMask(params string[] layerNames){
        ArgumentNullException.ThrowIfNull(layerNames);int mask=0;
        foreach(var name in layerNames){var layer=NameToLayer(name);if(layer>=0)mask|=1<<layer;}
        return mask;
    }
}
[AttributeUsage(AttributeTargets.Field)] public sealed class HideInInspector:Attribute {}
[AttributeUsage(AttributeTargets.Class)] public sealed class AddComponentMenu(string name):Attribute {public string componentMenu=name;}
[AttributeUsage(AttributeTargets.Field,AllowMultiple=false)] public sealed class TextAreaAttribute(int minLines=3,int maxLines=3):Attribute {public int minLines {get;}=minLines;public int maxLines {get;}=maxLines;}
public enum TextureWrapMode {Repeat=0,Clamp=1,Mirror=2,MirrorOnce=3}
public class Texture:Object {
    public FilterMode filterMode {get;set;}=FilterMode.Bilinear;
    public TextureWrapMode wrapMode {get;set;}=TextureWrapMode.Repeat;
}
public enum RenderMode { ScreenSpaceOverlay,ScreenSpaceCamera,WorldSpace }
 [Flags] public enum DepthTextureMode {None=0,Depth=1,DepthNormals=2,MotionVectors=4}
public sealed class Canvas:Behaviour {public RenderMode renderMode;public Camera worldCamera;public float scaleFactor {get;set;}=1f;}
public class ScriptableObject:Object {
    public static T CreateInstance<T>() where T:ScriptableObject,new()=> (T)CreateInstance(typeof(T));
    public static ScriptableObject CreateInstance(Type type){
        if(!typeof(ScriptableObject).IsAssignableFrom(type)||type.IsAbstract)throw new ArgumentException(nameof(type));
        var asset=(ScriptableObject)Activator.CreateInstance(type);asset.world=RuntimeWorld.Current;asset.world.Register(asset);return asset;
    }
}
public static class Application {
    // This host executes play mode; no editor import loop runs here.
    public static bool isPlaying=>true;
    public static string dataPath=>AppContext.BaseDirectory;
    public static string persistentDataPath=>Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"CrisisResponsePort");
    public static SystemLanguage systemLanguage=>SystemLanguage.English;
    public static Action<string> OpenUrlHandler;
    public static Action QuitHandler;
    public static void OpenURL(string url){if(OpenUrlHandler==null)throw new NotSupportedException("Application URL handler is not bound");OpenUrlHandler(url);}
    public static void Quit(){if(QuitHandler==null)throw new NotSupportedException("Application lifecycle handler is not bound");QuitHandler();}
    public static void SetStackTraceLogType(LogType logType,StackTraceLogType stackTraceType){ }
}
public enum SystemLanguage { Unknown,English,French,German,Italian,Spanish,Japanese,Korean,Chinese }
public enum LogType { Error,Assert,Warning,Log,Exception }
public enum StackTraceLogType { None,ScriptOnly,Full }
public enum CursorLockMode { None,Locked,Confined }
public static class Cursor {
    public static bool visible {get;set;}=true;
    public static CursorLockMode lockState {get;set;}=CursorLockMode.None;
}

public enum GraphicsDeviceType { Direct3D11,Direct3D12,Metal,OpenGLCore,OpenGLES2,OpenGLES3,Vulkan,Null,WebGPU }
public static class SystemInfo {
    public static string deviceModel=>Environment.MachineName;
    public static string deviceUniqueIdentifier=>string.Empty;
    public static string graphicsDeviceName=>"Unknown";
    public static GraphicsDeviceType graphicsDeviceType=>GraphicsDeviceType.Null;
    public static string graphicsDeviceVendor=>"Unknown";
    public static string graphicsDeviceVersion=>"Unknown";
    public static int graphicsMemorySize=>0;
    public static int graphicsShaderLevel=>0;
    public static string operatingSystem=>Environment.OSVersion.ToString();
    public static int processorCount=>Environment.ProcessorCount;
    public static int processorFrequency=>0;
    public static string processorType=>Environment.GetEnvironmentVariable("PROCESSOR_IDENTIFIER")??"Unknown";
    public static int systemMemorySize=>0;
    public static bool supportsComputeShaders=>false;
    public static bool supportsImageEffects=>false;
    public static bool supportsRenderTextures=>false;
    public static bool SupportsRenderTextureFormat(RenderTextureFormat format)=>false;
}

// The game is a 3D physics scene. These payloads preserve the plugin's 2D
// callback signatures; they do not claim a 2D simulation implementation.
public abstract class Collider2D:Behaviour {}
public sealed class Collision2D {
    public Collider2D collider {get;}
    public GameObject gameObject=>collider.gameObject;
    public Vector2 relativeVelocity {get;}
    public Collision2D(Collider2D collider,Vector2 relativeVelocity){this.collider=collider??throw new ArgumentNullException(nameof(collider));this.relativeVelocity=relativeVelocity;}
}
public sealed class ControllerColliderHit {
    public Collider collider {get;}
    public GameObject gameObject=>collider.gameObject;
    public Vector3 point {get;}
    public Vector3 normal {get;}
    public ControllerColliderHit(Collider collider,Vector3 point,Vector3 normal){this.collider=collider??throw new ArgumentNullException(nameof(collider));this.point=point;this.normal=normal;}
}

public static class Gizmos {
    public static Action<Vector3,string> IconSink;
    public static void DrawIcon(Vector3 position,string name){if(IconSink==null)throw new NotSupportedException("Editor gizmo renderer is not bound");IconSink(position,name);}
}
