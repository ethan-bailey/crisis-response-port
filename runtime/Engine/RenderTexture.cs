using System.Runtime.CompilerServices;

namespace UnityEngine;

// Original RenderTexture.active selects the target for Unity GL skin-mask
// drawing. A native/browser graphics driver must actually bind the target;
// selecting one here never pretends that any pixels were drawn.
public interface IRenderTargetDriver {
    RenderTexture ActiveTarget { get; }
    void SetActive(RenderTexture target);
}
public sealed class RenderTexture : Texture {
    public int width {get;}
    public int height {get;}
    public int depth {get;}
    public RenderTextureFormat format {get;}
    ITextureTransferDriver allocationDriver;
    public RenderTexture() {world=RuntimeWorld.Current;world.Register(this);}
    public RenderTexture(int width,int height,int depth,RenderTextureFormat format) {
        if(width<=0||height<=0)throw new ArgumentOutOfRangeException(nameof(width));
        if(depth is not (0 or 16 or 24))throw new NotSupportedException("Unsupported render texture depth");
        if(format!=RenderTextureFormat.ARGB32)throw new NotSupportedException("Unsupported render texture color format");
        this.width=width;this.height=height;this.depth=depth;this.format=format;
        world=RuntimeWorld.Current;world.Register(this);
    }
    public bool IsCreated()=>allocationDriver!=null;
    public bool Create() {
        if(allocationDriver!=null)return true;
        if(width<=0||height<=0)throw new InvalidOperationException("Render texture dimensions are unset");
        var driver=Graphics.RequiredDriver(world);
        if(!driver.Create(this))return false;
        allocationDriver=driver;return true;
    }
    public void Release() {
        if(allocationDriver==null)return;
        allocationDriver.Release(this);allocationDriver=null;
    }
    private static readonly ConditionalWeakTable<RuntimeWorld, IRenderTargetDriver> drivers = new();
    public static void Bind(RuntimeWorld world, IRenderTargetDriver driver) {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(driver);
        drivers.Remove(world);
        drivers.Add(world, driver);
    }
    private static IRenderTargetDriver Driver {
        get {
            if (drivers.TryGetValue(RuntimeWorld.Current, out var driver)) return driver;
            throw new NotSupportedException("Original limb-skin render target driver is not bound");
        }
    }
    public static RenderTexture active {
        get => Driver.ActiveTarget;
        set {
            if(value!=null&&!ReferenceEquals(value.world,RuntimeWorld.Current))throw new InvalidOperationException("Render target belongs to another world");
            Driver.SetActive(value);
        }
    }
}
