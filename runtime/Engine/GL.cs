using System.Runtime.CompilerServices;

namespace UnityEngine;

// Original immediate-mode wound/decal triangles and quads require a real
// graphics backend. No-op drawing would silently remove original injuries.
public interface IGLDriver {
    void PushMatrix();
    void PopMatrix();
    void LoadPixelMatrix(float left,float right,float bottom,float top);
    void Begin(int mode);
    void End();
    void Color(Color color);
    void Vertex3(float x,float y,float z);
    void LoadOrtho()=>throw new NotSupportedException("GL.LoadOrtho is not implemented by the bound driver");
    void TexCoord2(float x,float y)=>throw new NotSupportedException("GL.TexCoord2 is not implemented by the bound driver");
    void MultiTexCoord2(int unit,float x,float y)=>throw new NotSupportedException("GL.MultiTexCoord2 is not implemented by the bound driver");
    Matrix4x4 GetGPUProjectionMatrix(Matrix4x4 matrix,bool renderIntoTexture)=>throw new NotSupportedException("GPU projection conversion is not implemented by the bound driver");
}
public static class GL {
    private static readonly ConditionalWeakTable<RuntimeWorld, IGLDriver> drivers=new();
    public static void Bind(RuntimeWorld world,IGLDriver driver){
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(driver);
        drivers.Remove(world);
        drivers.Add(world,driver);
    }
    private static IGLDriver Driver {
        get {
            if(drivers.TryGetValue(RuntimeWorld.Current,out var driver))return driver;
            throw new NotSupportedException("Original limb-decal GL driver is not bound");
        }
    }
    public static void PushMatrix()=>Driver.PushMatrix();
    public static void PopMatrix()=>Driver.PopMatrix();
    public static void LoadPixelMatrix(float left,float right,float bottom,float top)=>Driver.LoadPixelMatrix(left,right,bottom,top);
    public static void Begin(int mode){
        if(mode!=4 && mode!=7)throw new NotSupportedException($"Original limb-decal GL primitive mode {mode} is unsupported");
        Driver.Begin(mode);
    }
    public static void End()=>Driver.End();
    public static void Color(Color value)=>Driver.Color(value);
    public static void Vertex3(float x,float y,float z)=>Driver.Vertex3(x,y,z);
    public static void LoadOrtho()=>Driver.LoadOrtho();
    public static void TexCoord2(float x,float y)=>Driver.TexCoord2(x,y);
    public static void MultiTexCoord2(int unit,float x,float y)=>Driver.MultiTexCoord2(unit,x,y);
    public static Matrix4x4 GetGPUProjectionMatrix(Matrix4x4 matrix,bool renderIntoTexture)=>Driver.GetGPUProjectionMatrix(matrix,renderIntoTexture);
}
