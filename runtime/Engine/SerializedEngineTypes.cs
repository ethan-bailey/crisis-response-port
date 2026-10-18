using N = System.Numerics;
namespace UnityEngine;

// Serialized Unity value types consumed by the recovered Behavior Designer
// readers. These are data, not replacements for original game decisions.
public struct Vector4 {
    public float x,y,z,w;
    public static Vector4 zero=>new(0,0,0,0);
    public Vector4(float x,float y,float z,float w){this.x=x;this.y=y;this.z=z;this.w=w;}
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
}
public struct LayerMask {
    public int value;
    public static implicit operator int(LayerMask mask)=>mask.value;
    public static implicit operator LayerMask(int value)=>new(){value=value};
}
[AttributeUsage(AttributeTargets.Field)] public sealed class HideInInspector:Attribute {}
[AttributeUsage(AttributeTargets.Class)] public sealed class AddComponentMenu(string name):Attribute {public string componentMenu=name;}
public class Texture:Object {}
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
