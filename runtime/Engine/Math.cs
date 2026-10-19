using N = System.Numerics;

namespace UnityEngine;
public enum Space { World,Self }

public struct Vector2 {
    public float x, y;
    public static Vector2 zero=>new(0,0);
    public float sqrMagnitude=>x*x+y*y;
    public float magnitude=>MathF.Sqrt(sqrMagnitude);
    public Vector2 normalized=>magnitude>1e-5f?this/magnitude:zero;
    public static bool operator ==(Vector2 a,Vector2 b)=>(a.x-b.x)*(a.x-b.x)+(a.y-b.y)*(a.y-b.y)<1e-10f;
    public static bool operator !=(Vector2 a,Vector2 b)=>!(a==b);
    public override bool Equals(object o)=>o is Vector2 b&&x.Equals(b.x)&&y.Equals(b.y);
    public override int GetHashCode()=>HashCode.Combine(x,y);
    public Vector2(float x, float y) { this.x=x; this.y=y; }
    public static implicit operator Vector2(Vector3 v)=>new(v.x,v.y);
    public static implicit operator Vector3(Vector2 v) => new(v.x,v.y,0);
    public static Vector2 operator +(Vector2 a,Vector2 b)=>new(a.x+b.x,a.y+b.y);
    public static Vector2 operator -(Vector2 a,Vector2 b)=>new(a.x-b.x,a.y-b.y);
    public static Vector2 operator *(Vector2 v,float n) => new(v.x*n,v.y*n);
    public static Vector2 operator *(float n,Vector2 v)=>v*n;
    public static Vector2 operator /(Vector2 v,float n)=>new(v.x/n,v.y/n);
}
public struct Vector3 {
    public float x,y,z;
    public Vector3(float x,float y):this(x,y,0){}
    public Vector3(float x,float y,float z) { this.x=x;this.y=y;this.z=z; }
    internal N.Vector3 Native => new(x,y,z);
    internal static Vector3 From(N.Vector3 v) => new(v.X,v.Y,v.Z);
    public static Vector3 zero => new(0,0,0);
    public static Vector3 one => new(1,1,1);
    public static Vector3 right => new(1,0,0);
    public static Vector3 left => new(-1,0,0);
    public static Vector3 up => new(0,1,0);
    public static Vector3 down => new(0,-1,0);
    public static Vector3 forward => new(0,0,1);
    public static Vector3 back => new(0,0,-1);
    public float sqrMagnitude => x*x+y*y+z*z;
    public float magnitude => MathF.Sqrt(sqrMagnitude);
    public Vector3 normalized => magnitude>1e-5f ? this/magnitude : zero;
    public static Vector3 operator +(Vector3 a,Vector3 b) => From(a.Native+b.Native);
    public static Vector3 operator -(Vector3 a,Vector3 b) => From(a.Native-b.Native);
    public static Vector3 operator -(Vector3 a) => From(-a.Native);
    public static Vector3 operator *(Vector3 a,float n) => From(a.Native*n);
    public static Vector3 operator *(float n,Vector3 a) => a*n;
    public static Vector3 operator /(Vector3 a,float n) => From(a.Native/n);
    public static bool operator ==(Vector3 a,Vector3 b)=>(a-b).sqrMagnitude<1e-10f;
    public static bool operator !=(Vector3 a,Vector3 b)=>!(a==b);
    public void Normalize()=>this=normalized;
    public static Vector3 Lerp(Vector3 a,Vector3 b,float t)=>a+(b-a)*Mathf.Clamp01(t);
    public static Vector3 Slerp(Vector3 a,Vector3 b,float t){
        t=Mathf.Clamp01(t);float ma=a.magnitude,mb=b.magnitude;
        if(ma<1e-8f||mb<1e-8f)return Lerp(a,b,t);
        Vector3 an=a/ma,bn=b/mb;float angle=MathF.Acos(Math.Clamp(Dot(an,bn),-1f,1f));
        if(angle<1e-5f)return Lerp(a,b,t);
        float sin=MathF.Sin(angle);Vector3 direction=an*(MathF.Sin((1f-t)*angle)/sin)+bn*(MathF.Sin(t*angle)/sin);
        return direction*(ma+(mb-ma)*t);
    }
    public static float Distance(Vector3 a,Vector3 b)=>(a-b).magnitude;
    public static Vector3 Project(Vector3 vector,Vector3 onNormal){float n=onNormal.sqrMagnitude;return n<1e-12f?zero:onNormal*(Dot(vector,onNormal)/n);}
    public static float Dot(Vector3 a,Vector3 b) => N.Vector3.Dot(a.Native,b.Native);
    public static Vector3 Cross(Vector3 a,Vector3 b) => From(N.Vector3.Cross(a.Native,b.Native));
    public static float Angle(Vector3 a,Vector3 b){float n=a.magnitude*b.magnitude;return n<1e-15f?0:MathF.Acos(Math.Clamp(Dot(a,b)/n,-1,1))*180/MathF.PI;}
    public static Vector3 Reflect(Vector3 direction,Vector3 normal)=>direction-2*Dot(direction,normal)*normal;
    public static Vector3 Scale(Vector3 a,Vector3 b) => From(a.Native*b.Native);
    public override string ToString() => $"({x}, {y}, {z})";
    public override bool Equals(object o)=>o is Vector3 v&&this==v;
    public override int GetHashCode()=>HashCode.Combine(x,y,z);
}
public struct Quaternion {
    public float x,y,z,w;
    public Quaternion(float x,float y,float z,float w) { this.x=x;this.y=y;this.z=z;this.w=w; }
    internal N.Quaternion Native => new(x,y,z,w);
    internal static Quaternion From(N.Quaternion q) => new(q.X,q.Y,q.Z,q.W);
    public static Quaternion identity => new(0,0,0,1);
    public static Quaternion AngleAxis(float degrees,Vector3 axis) => axis.sqrMagnitude<1e-10f ? identity : From(N.Quaternion.CreateFromAxisAngle(axis.normalized.Native,degrees*MathF.PI/180));
    public static Quaternion Euler(float x,float y,float z)=>AngleAxis(z,Vector3.forward)*AngleAxis(x,Vector3.right)*AngleAxis(y,Vector3.up);
    public static Quaternion Euler(Vector3 euler)=>Euler(euler.x,euler.y,euler.z);
    public static Quaternion LookRotation(Vector3 forward)=>LookRotation(forward,Vector3.up);
    public static Quaternion LookRotation(Vector3 forward,Vector3 up){var z=forward.normalized;if(z.sqrMagnitude==0)return identity;var x=Vector3.Cross(up,z).normalized;if(x.sqrMagnitude<1e-12f)x=(Vector3.right-z*Vector3.Dot(Vector3.right,z)).normalized;var y=Vector3.Cross(z,x);var m=new N.Matrix4x4(x.x,x.y,x.z,0,y.x,y.y,y.z,0,z.x,z.y,z.z,0,0,0,0,1);return From(N.Quaternion.CreateFromRotationMatrix(m));}
    public static Quaternion Inverse(Quaternion q) => From(N.Quaternion.Inverse(q.Native));
    public static Quaternion Lerp(Quaternion a,Quaternion b,float t)=>From(N.Quaternion.Slerp(a.Native,b.Native,Mathf.Clamp01(t)));
    public static Quaternion Slerp(Quaternion a,Quaternion b,float t)=>Lerp(a,b,t);
    public static float Angle(Quaternion a,Quaternion b){float dot=MathF.Abs(N.Quaternion.Dot(a.Native,b.Native));return 2f*MathF.Acos(Math.Clamp(dot,-1f,1f))*180f/MathF.PI;}
    public Vector3 eulerAngles {
        get {var f=this*Vector3.forward;var r=this*Vector3.right;var u=this*Vector3.up;return new(MathF.Asin(Math.Clamp(-f.y,-1,1))*180/MathF.PI,MathF.Atan2(f.x,f.z)*180/MathF.PI,MathF.Atan2(r.y,u.y)*180/MathF.PI);}
    }
    public static Quaternion operator *(Quaternion a,Quaternion b) => From(a.Native*b.Native);
    public static Vector3 operator *(Quaternion q,Vector3 v) => Vector3.From(N.Vector3.Transform(v.Native,q.Native));
}
public static class Mathf {
    public const float Epsilon=float.Epsilon;
    public static int FloorToInt(float f)=>(int)MathF.Floor(f);
    public static int CeilToInt(float f)=>(int)MathF.Ceiling(f);
    public static int RoundToInt(float f)=>(int)MathF.Round(f,MidpointRounding.ToEven);
    public static int NextPowerOfTwo(int value){if(value<=1)return 1;uint v=(uint)value-1;v|=v>>1;v|=v>>2;v|=v>>4;v|=v>>8;v|=v>>16;return checked((int)(v+1));}
    public static float Floor(float f)=>MathF.Floor(f);
    public static float Ceil(float f)=>MathF.Ceiling(f);
    public static float Round(float f)=>MathF.Round(f,MidpointRounding.ToEven);
    public static float Sign(float f) => f>=0?1:-1;
    public static float Abs(float f) => MathF.Abs(f);
    public static float Min(float a,float b) => MathF.Min(a,b);
    public static int Min(int a,int b)=>Math.Min(a,b);
    public static float Max(float a,float b) => MathF.Max(a,b);
    public static int Max(int a,int b)=>Math.Max(a,b);
    public static float Clamp(float f,float min,float max) => Math.Clamp(f,min,max);
    public static int Clamp(int value,int min,int max)=>Math.Clamp(value,min,max);
    public static float Clamp01(float f) => Math.Clamp(f,0,1);
    public static float Lerp(float a,float b,float t)=>a+(b-a)*Clamp01(t);
    public static float LerpUnclamped(float a,float b,float t)=>a+(b-a)*t;
    public static float Pow(float value,float power)=>MathF.Pow(value,power);
    public static float Sqrt(float value)=>MathF.Sqrt(value);
    public static float Tan(float value)=>MathF.Tan(value);
    public static float Cos(float value)=>MathF.Cos(value);
    public static float LerpAngle(float a,float b,float t){float delta=(b-a)%360;if(delta<0)delta+=360;if(delta>180)delta-=360;return a+delta*Clamp01(t);}
    public static float Sin(float value)=>MathF.Sin(value);
    public static float Acos(float value)=>MathF.Acos(value);
    public static float Atan2(float y,float x)=>MathF.Atan2(y,x);
    public static float Atan(float value)=>MathF.Atan(value);
    public static float Max(float a,float b,float c)=>MathF.Max(a,MathF.Max(b,c));
    public static int Max(int a,int b,int c)=>Math.Max(a,Math.Max(b,c));
}
[AttributeUsage(AttributeTargets.Field)] public sealed class TooltipAttribute(string text):Attribute { }
[AttributeUsage(AttributeTargets.Field)] public sealed class HeaderAttribute(string text):Attribute { }
[AttributeUsage(AttributeTargets.Field)] public sealed class SerializeField:Attribute { }

public static class Random {
 static readonly System.Random source=new();
 public static Vector2 insideUnitCircle {get{float x,y;do{x=Range(-1f,1f);y=Range(-1f,1f);}while(x*x+y*y>1);return new(x,y);}}
 public static Vector3 insideUnitSphere {get{Vector3 value;do{value=new(Range(-1f,1f),Range(-1f,1f),Range(-1f,1f));}while(value.sqrMagnitude>1f);return value;}}
 public static Vector3 onUnitSphere {get{float z=Range(-1f,1f),a=Range(0f,2*MathF.PI),r=MathF.Sqrt(1-z*z);return new(r*MathF.Cos(a),r*MathF.Sin(a),z);}}
 public static float value=>(float)source.NextDouble();
 public static float Range(float a,float b)=>a+(b-a)*value;
 public static int Range(int a,int b)=>source.Next(a,b);
}
public static class Debug {
 public static Action<Vector3,Vector3,Color,float,bool> DrawLineSink;
 public static void Log(object message)=>Console.WriteLine(message);
 public static void LogError(object message)=>Console.Error.WriteLine(message);
 public static void LogWarning(object message)=>Console.Error.WriteLine(message);
 public static void LogWarning(object message,Object context)=>Console.Error.WriteLine(message);
 public static void LogFormat(string format,params object[] args)=>Console.WriteLine(format,args);
 public static void LogErrorFormat(string format,params object[] args)=>Console.Error.WriteLine(format,args);
 public static void LogWarningFormat(string format,params object[] args)=>Console.Error.WriteLine(format,args);
 public static void DrawLine(Vector3 start,Vector3 end,Color color,float duration=0f,bool depthTest=true)=>DrawLineSink?.Invoke(start,end,color,duration,depthTest);
}

