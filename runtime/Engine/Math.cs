using N = System.Numerics;

namespace UnityEngine;

public struct Vector2 {
    public float x, y;
    public Vector2(float x, float y) { this.x=x; this.y=y; }
    public static implicit operator Vector3(Vector2 v) => new(v.x,v.y,0);
    public static Vector2 operator *(Vector2 v,float n) => new(v.x*n,v.y*n);
}
public struct Vector3 {
    public float x,y,z;
    public Vector3(float x,float y,float z) { this.x=x;this.y=y;this.z=z; }
    internal N.Vector3 Native => new(x,y,z);
    internal static Vector3 From(N.Vector3 v) => new(v.X,v.Y,v.Z);
    public static Vector3 zero => new(0,0,0);
    public static Vector3 one => new(1,1,1);
    public static Vector3 right => new(1,0,0);
    public static Vector3 up => new(0,1,0);
    public static Vector3 forward => new(0,0,1);
    public float sqrMagnitude => x*x+y*y+z*z;
    public float magnitude => MathF.Sqrt(sqrMagnitude);
    public Vector3 normalized => magnitude>1e-5f ? this/magnitude : zero;
    public static Vector3 operator +(Vector3 a,Vector3 b) => From(a.Native+b.Native);
    public static Vector3 operator -(Vector3 a,Vector3 b) => From(a.Native-b.Native);
    public static Vector3 operator -(Vector3 a) => From(-a.Native);
    public static Vector3 operator *(Vector3 a,float n) => From(a.Native*n);
    public static Vector3 operator *(float n,Vector3 a) => a*n;
    public static Vector3 operator /(Vector3 a,float n) => From(a.Native/n);
    public static Vector3 Lerp(Vector3 a,Vector3 b,float t)=>a+(b-a)*Mathf.Clamp01(t);
    public static float Dot(Vector3 a,Vector3 b) => N.Vector3.Dot(a.Native,b.Native);
    public static Vector3 Cross(Vector3 a,Vector3 b) => From(N.Vector3.Cross(a.Native,b.Native));
    // UnityEngine.Vector3.Project: remove normal-to-plane component from an
    // arbitrary vector, preserving the original projectile's depth alignment.
    public static Vector3 Project(Vector3 vector,Vector3 onNormal){
        float dot=Dot(onNormal,onNormal);
        return dot<1e-15f?zero:onNormal*(Dot(vector,onNormal)/dot);
    }
    public static float Angle(Vector3 a,Vector3 b){float n=a.magnitude*b.magnitude;return n<1e-15f?0:MathF.Acos(Math.Clamp(Dot(a,b)/n,-1,1))*180/MathF.PI;}
    public static Vector3 Reflect(Vector3 direction,Vector3 normal)=>direction-2*Dot(direction,normal)*normal;
    public static Vector3 Scale(Vector3 a,Vector3 b) => From(a.Native*b.Native);
    public override string ToString() => $"({x}, {y}, {z})";
}
public struct Quaternion {
    public float x,y,z,w;
    public Quaternion(float x,float y,float z,float w) { this.x=x;this.y=y;this.z=z;this.w=w; }
    internal N.Quaternion Native => new(x,y,z,w);
    internal static Quaternion From(N.Quaternion q) => new(q.X,q.Y,q.Z,q.W);
    public static Quaternion identity => new(0,0,0,1);
    public static Quaternion AngleAxis(float degrees,Vector3 axis) => axis.sqrMagnitude<1e-10f ? identity : From(N.Quaternion.CreateFromAxisAngle(axis.normalized.Native,degrees*MathF.PI/180));
    public static Quaternion LookRotation(Vector3 forward)=>LookRotation(forward,Vector3.up);
    public static Quaternion LookRotation(Vector3 forward,Vector3 up){var z=forward.normalized;if(z.sqrMagnitude==0)return identity;var x=Vector3.Cross(up,z).normalized;if(x.sqrMagnitude<1e-12f)x=(Vector3.right-z*Vector3.Dot(Vector3.right,z)).normalized;var y=Vector3.Cross(z,x);var m=new N.Matrix4x4(x.x,x.y,x.z,0,y.x,y.y,y.z,0,z.x,z.y,z.z,0,0,0,0,1);return From(N.Quaternion.CreateFromRotationMatrix(m));}
    public static Quaternion Inverse(Quaternion q) => From(N.Quaternion.Inverse(q.Native));
    public static Quaternion operator *(Quaternion a,Quaternion b) => From(a.Native*b.Native);
    public static Vector3 operator *(Quaternion q,Vector3 v) => Vector3.From(N.Vector3.Transform(v.Native,q.Native));
}
public static class Mathf {
    public static int FloorToInt(float f)=>(int)MathF.Floor(f);
    public static float Sign(float f) => f>=0?1:-1;
    public static float Abs(float f) => MathF.Abs(f);
    public static float Min(float a,float b) => MathF.Min(a,b);
    public static float Max(float a,float b) => MathF.Max(a,b);
    public static float Clamp(float f,float min,float max) => Math.Clamp(f,min,max);
    public static float Clamp01(float f) => Math.Clamp(f,0,1);
    public static float Lerp(float a,float b,float t)=>a+(b-a)*Clamp01(t);
    public static float Pow(float value,float power)=>MathF.Pow(value,power);
    public static float Sqrt(float value)=>MathF.Sqrt(value);
    public static float Tan(float value)=>MathF.Tan(value);
    public static float Cos(float value)=>MathF.Cos(value);
    public static float LerpAngle(float a,float b,float t){float delta=(b-a)%360;if(delta<0)delta+=360;if(delta>180)delta-=360;return a+delta*Clamp01(t);}
    public static float Sin(float value)=>MathF.Sin(value);
    public static float Acos(float value)=>MathF.Acos(value);
    public static float Atan2(float y,float x)=>MathF.Atan2(y,x);
    public static float Atan(float value)=>MathF.Atan(value);
}
[AttributeUsage(AttributeTargets.Field)] public sealed class TooltipAttribute(string text):Attribute { }
[AttributeUsage(AttributeTargets.Field)] public sealed class HeaderAttribute(string text):Attribute { }
[AttributeUsage(AttributeTargets.Field)] public sealed class SerializeField:Attribute { }

public static class Random {
 static readonly System.Random source=new();
 public static Vector2 insideUnitCircle {get{float x,y;do{x=Range(-1f,1f);y=Range(-1f,1f);}while(x*x+y*y>1);return new(x,y);}}
 public static Vector3 onUnitSphere {get{float z=Range(-1f,1f),a=Range(0f,2*MathF.PI),r=MathF.Sqrt(1-z*z);return new(r*MathF.Cos(a),r*MathF.Sin(a),z);}}
 public static float value=>(float)source.NextDouble();
 public static float Range(float a,float b)=>a+(b-a)*value;
 public static int Range(int a,int b)=>source.Next(a,b);
}
public static class Debug {public static void LogWarningFormat(string format,params object[] args)=>Console.Error.WriteLine(format,args);}
