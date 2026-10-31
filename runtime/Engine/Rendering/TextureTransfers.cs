using System.Runtime.CompilerServices;

namespace UnityEngine;

public enum RenderTextureFormat { ARGB32=0,Depth=1,ARGBHalf=2,RGHalf=15 }

// Data transfers are distinct from GL rasterization. A backend must supply
// actual target storage and readback, never an empty success callback.
public interface ITextureTransferDriver {
    bool Create(RenderTexture target);
    void Release(RenderTexture target);
    void Blit(Texture source,RenderTexture destination);
    Color[] ReadPixels(RenderTexture source,int x,int y,int width,int height);
}
public interface ITexturePostProcessDriver:ITextureTransferDriver {
    void Blit(Texture source,RenderTexture destination,Material material,int pass);
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
    public static void Blit(Texture source,RenderTexture destination,Material material,int pass){
        RequireWorld(source);RequireWorld(destination);ArgumentNullException.ThrowIfNull(material);
        if(pass<0||pass>=material.passCount)throw new ArgumentOutOfRangeException(nameof(pass));
        if(!destination.Create())throw new InvalidOperationException("Render target allocation failed");
        if(RequiredDriver(RuntimeWorld.Current) is not ITexturePostProcessDriver driver)
            throw new NotSupportedException("Shader-backed texture blit driver is not bound");
        driver.Blit(source,destination,material,pass);
    }
    public static void SetRenderTarget(RenderTexture target){RenderTexture.active=target;}
    public static void SetRandomWriteTarget(int index,ComputeBuffer buffer)=>throw new NotSupportedException("Random write render targets require a GPU graphics driver");
    public static void ClearRandomWriteTargets()=>throw new NotSupportedException("Random write render targets require a GPU graphics driver");
    public static void DrawProceduralIndirect(MeshTopology topology,ComputeBuffer buffer,int argsOffset=0)=>throw new NotSupportedException("Indirect procedural rendering requires a GPU graphics driver");
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
public sealed class CpuTextureTransferDriver:IRenderTargetDriver,ITextureTransferDriver,IGLDriver {
    sealed class Storage(int count) {public readonly Color[] Pixels=new Color[count];}
    readonly record struct GlVertex(float X,float Y,Color Color);
    readonly ConditionalWeakTable<RenderTexture,Storage> storage=new();
    readonly RuntimeWorld world;
    readonly List<GlVertex> glVertices=new();
    int primitiveMode,matrixDepth;
    float pixelLeft,pixelRight,pixelBottom,pixelTop;
    bool pixelMatrixLoaded;
    Color currentGlColor=UnityEngine.Color.white;
    public RenderTexture ActiveTarget {get;private set;}
    public CpuTextureTransferDriver(RuntimeWorld world) {this.world=world??throw new ArgumentNullException(nameof(world));}
    public void Bind() {RenderTexture.Bind(world,this);Graphics.Bind(world,this);GL.Bind(world,this);}

    public void PushMatrix()=>matrixDepth++;
    public void PopMatrix() {
        if(matrixDepth<=0)throw new InvalidOperationException("GL matrix stack underflow");
        matrixDepth--;
    }
    public void LoadPixelMatrix(float left,float right,float bottom,float top) {
        if(!float.IsFinite(left)||!float.IsFinite(right)||!float.IsFinite(bottom)||!float.IsFinite(top)||left==right||bottom==top)
            throw new ArgumentOutOfRangeException(nameof(left),"GL pixel matrix bounds must be finite and nondegenerate");
        pixelLeft=left;pixelRight=right;pixelBottom=bottom;pixelTop=top;pixelMatrixLoaded=true;
    }
    public void Begin(int mode) {
        if(mode is not (4 or 7))throw new NotSupportedException($"CPU limb-mask GL mode {mode} is unsupported");
        if(primitiveMode!=0)throw new InvalidOperationException("A GL primitive is already open");
        if(ActiveTarget==null)throw new InvalidOperationException("A GL limb-mask primitive requires an active render texture");
        if(!pixelMatrixLoaded)throw new InvalidOperationException("A GL limb-mask primitive requires a pixel matrix");
        primitiveMode=mode;glVertices.Clear();
    }
    public void End() {
        if(primitiveMode==0)throw new InvalidOperationException("No GL primitive is open");
        try {
            if(glVertices.Count!=0)throw new InvalidOperationException($"GL primitive mode {primitiveMode} has an incomplete vertex group");
        } finally {
            primitiveMode=0;glVertices.Clear();
        }
    }
    public void Color(Color color) {
        if(primitiveMode==0)throw new InvalidOperationException("GL color requires an open primitive");
        ValidateColor(color);currentGlColor=color;
    }
    public void Vertex3(float x,float y,float z) {
        if(primitiveMode==0)throw new InvalidOperationException("GL vertex requires an open primitive");
        if(!float.IsFinite(x)||!float.IsFinite(y)||!float.IsFinite(z))throw new ArgumentOutOfRangeException(nameof(x),"GL vertices must be finite");
        glVertices.Add(new GlVertex(MapX(x),MapY(y),currentGlColor));
        int stride=primitiveMode==4?3:4;
        if(glVertices.Count==stride) {
            Rasterize(glVertices[0],glVertices[1],glVertices[2]);
            if(primitiveMode==7)Rasterize(glVertices[0],glVertices[2],glVertices[3]);
            glVertices.Clear();
        }
    }
    float MapX(float x)=>(x-pixelLeft)/(pixelRight-pixelLeft)*ActiveTarget.width;
    float MapY(float y)=>(y-pixelBottom)/(pixelTop-pixelBottom)*ActiveTarget.height;
    static double Edge(float ax,float ay,float bx,float by,float px,float py)=>(double)(px-ax)*(by-ay)-(double)(py-ay)*(bx-ax);
    static void ValidateColor(Color color) {
        if(!float.IsFinite(color.r)||!float.IsFinite(color.g)||!float.IsFinite(color.b)||!float.IsFinite(color.a))
            throw new ArgumentOutOfRangeException(nameof(color),"GL color channels must be finite");
    }
    void Rasterize(GlVertex first,GlVertex second,GlVertex third) {
        var target=ActiveTarget??throw new InvalidOperationException("GL draw lost its active render texture");
        var pixels=Get(target).Pixels;
        double area=Edge(first.X,first.Y,second.X,second.Y,third.X,third.Y);
        if(Math.Abs(area)<1e-8)return;
        int minX=Math.Max(0,(int)MathF.Floor(MathF.Min(first.X,MathF.Min(second.X,third.X))));
        int maxX=Math.Min(target.width-1,(int)MathF.Ceiling(MathF.Max(first.X,MathF.Max(second.X,third.X)))-1);
        int minY=Math.Max(0,(int)MathF.Floor(MathF.Min(first.Y,MathF.Min(second.Y,third.Y))));
        int maxY=Math.Min(target.height-1,(int)MathF.Ceiling(MathF.Max(first.Y,MathF.Max(second.Y,third.Y)))-1);
        for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++) {
            float px=x+0.5f,py=y+0.5f;
            double wa=Edge(second.X,second.Y,third.X,third.Y,px,py)/area;
            double wb=Edge(third.X,third.Y,first.X,first.Y,px,py)/area;
            double wc=1-wa-wb;
            if(wa < -1e-7||wb < -1e-7||wc < -1e-7)continue;
            var source=new Color(
                (float)(first.Color.r*wa+second.Color.r*wb+third.Color.r*wc),
                (float)(first.Color.g*wa+second.Color.g*wb+third.Color.g*wc),
                (float)(first.Color.b*wa+second.Color.b*wb+third.Color.b*wc),
                (float)(first.Color.a*wa+second.Color.a*wb+third.Color.a*wc));
            int index=y*target.width+x;
            var destination=pixels[index];
            float alpha=Math.Clamp(source.a,0f,1f),inverse=1-alpha;
            pixels[index]=new Color(
                Math.Clamp(source.r,0f,1f)*alpha+destination.r*inverse,
                Math.Clamp(source.g,0f,1f)*alpha+destination.g*inverse,
                Math.Clamp(source.b,0f,1f)*alpha+destination.b*inverse,
                alpha+destination.a*inverse);
        }
    }
    void RequireOwner(Texture texture) {
        if(!ReferenceEquals(texture.world,world))throw new InvalidOperationException("Texture is owned by another world");
    }
    public bool Create(RenderTexture target) {
        RequireOwner(target);
        if(target.width<=0||target.height<=0)throw new ArgumentOutOfRangeException(nameof(target));
        if(target.format!=RenderTextureFormat.ARGB32)throw new NotSupportedException($"CPU texture transfer does not support {target.format}");
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
