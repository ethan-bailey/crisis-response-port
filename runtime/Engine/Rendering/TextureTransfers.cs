using System.Runtime.CompilerServices;

namespace UnityEngine;

public enum RenderTextureFormat { ARGB32=0 }

// Data transfers are distinct from GL rasterization. A backend must supply
// actual target storage and readback, never an empty success callback.
public interface ITextureTransferDriver {
    bool Create(RenderTexture target);
    void Release(RenderTexture target);
    void Blit(Texture source,RenderTexture destination);
    Color[] ReadPixels(RenderTexture source,int x,int y,int width,int height);
}
public static class Graphics {
    static readonly ConditionalWeakTable<RuntimeWorld,ITextureTransferDriver> drivers=new();
    public static void Bind(RuntimeWorld world,ITextureTransferDriver driver) {
        ArgumentNullException.ThrowIfNull(world);ArgumentNullException.ThrowIfNull(driver);
        drivers.Remove(world);drivers.Add(world,driver);
    }
    internal static ITextureTransferDriver RequiredDriver(RuntimeWorld world)=>drivers.TryGetValue(world,out var driver)?driver:
        throw new NotSupportedException("Texture transfer driver is not bound");
    static void RequireWorld(Texture texture) {
        ArgumentNullException.ThrowIfNull(texture);
        if(!ReferenceEquals(texture.world,RuntimeWorld.Current))throw new InvalidOperationException("Texture belongs to another world");
    }
    public static void Blit(Texture source,RenderTexture destination) {
        RequireWorld(source);RequireWorld(destination);
        if(ReferenceEquals(source,destination))throw new ArgumentException("Source and destination must be distinct textures");
        if(!destination.Create())throw new InvalidOperationException("Render target allocation failed");
        RequiredDriver(RuntimeWorld.Current).Blit(source,destination);
    }
    internal static Color[] ReadPixels(RenderTexture source,int x,int y,int width,int height) {
        RequireWorld(source);
        Texture2D.ValidateRegion(x,y,width,height,source.width,source.height);
        if(!source.IsCreated())throw new InvalidOperationException("Readback target has no storage");
        return RequiredDriver(RuntimeWorld.Current).ReadPixels(source,x,y,width,height);
    }
}

// An actual CPU storage/copy/readback backend for same-resolution RGBA masks.
// It deliberately does not implement a GL rasterizer, filtering, resizing,
// color-space conversion or claim native GPU/browser rendering parity.
public sealed class CpuTextureTransferDriver:IRenderTargetDriver,ITextureTransferDriver {
    sealed class Storage(int count) {public readonly Color[] Pixels=new Color[count];}
    readonly ConditionalWeakTable<RenderTexture,Storage> storage=new();
    readonly RuntimeWorld world;
    public RenderTexture ActiveTarget {get;private set;}
    public CpuTextureTransferDriver(RuntimeWorld world) {this.world=world??throw new ArgumentNullException(nameof(world));}
    public void Bind() {RenderTexture.Bind(world,this);Graphics.Bind(world,this);}
    void RequireOwner(Texture texture) {
        if(!ReferenceEquals(texture.world,world))throw new InvalidOperationException("Texture is owned by another world");
    }
    public bool Create(RenderTexture target) {
        RequireOwner(target);
        if(target.width<=0||target.height<=0)throw new ArgumentOutOfRangeException(nameof(target));
        storage.GetValue(target,t=>new Storage(checked(t.width*t.height)));return true;
    }
    public void Release(RenderTexture target) {
        RequireOwner(target);storage.Remove(target);
        if(ReferenceEquals(ActiveTarget,target))ActiveTarget=null;
    }
    Storage Get(RenderTexture target)=>storage.TryGetValue(target,out var pixels)?pixels:
        throw new InvalidOperationException("Render texture storage is unavailable on this backend");
    public void SetActive(RenderTexture target) {
        if(target!=null) {RequireOwner(target);if(!target.Create())throw new InvalidOperationException("Render target allocation failed");}
        ActiveTarget=target;
    }
    public void Blit(Texture source,RenderTexture destination) {
        RequireOwner(source);RequireOwner(destination);
        Color[] pixels;int width,height;
        if(source is Texture2D texture) {width=texture.width;height=texture.height;pixels=texture.GetPixels();}
        else if(source is RenderTexture target) {width=target.width;height=target.height;pixels=Get(target).Pixels;}
        else throw new NotSupportedException("CPU transfer source must be a CPU texture or render target");
        if(width!=destination.width||height!=destination.height)throw new NotSupportedException("CPU transfer does not approximate GPU resampling");
        Array.Copy(pixels,Get(destination).Pixels,pixels.Length);
    }
    public Color[] ReadPixels(RenderTexture source,int x,int y,int width,int height) {
        RequireOwner(source);Texture2D.ValidateRegion(x,y,width,height,source.width,source.height);
        var pixels=Get(source).Pixels;var result=new Color[checked(width*height)];
        for(int row=0;row<height;row++)Array.Copy(pixels,(y+row)*source.width+x,result,row*width,width);
        return result;
    }
}
