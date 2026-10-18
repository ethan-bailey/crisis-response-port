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
}
