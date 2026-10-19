namespace UnityEngine;

public enum TextureFormat { RGBA32=4 }

// CPU texture data is bottom-left first. Rendering/upload observers remain
// separate; no GPU completion is claimed by changing these pixels.
public sealed class Texture2D:Texture {
    public int width {get;}
    public int height {get;}
    readonly byte[] rgba;
    public string SourceID;
    public Action<Texture2D,int,int,int,int,byte[]> Changed;

    static int ByteCount(int w,int h) {
        if(w<=0||h<=0)throw new ArgumentOutOfRangeException(nameof(w));
        return checked(w*h*4);
    }
    public Texture2D(int w,int h,byte[] bytes,RuntimeWorld owner=null) {
        ArgumentNullException.ThrowIfNull(bytes);
        if(bytes.Length!=ByteCount(w,h))throw new ArgumentException("RGBA32 byte count does not match texture dimensions",nameof(bytes));
        world=owner??RuntimeWorld.Current;width=w;height=h;rgba=(byte[])bytes.Clone();world.Register(this);
    }
    public Texture2D(int w,int h):this(w,h,TextureFormat.RGBA32,false) { }
    public bool LoadImage(byte[] imageData,bool markNonReadable=false)=>throw new NotSupportedException("Image decoding requires the pinned asset decoder");
    static byte[] Allocate(int w,int h,TextureFormat format,bool mipChain) {
        if(format!=TextureFormat.RGBA32||mipChain)throw new NotSupportedException("This CPU texture adapter supports RGBA32 level-zero textures without mip chains");
        return new byte[ByteCount(w,h)];
    }
    public Texture2D(int w,int h,TextureFormat format,bool mipChain):this(w,h,Allocate(w,h,format,mipChain)) { }
    public Texture2D CloneTexture(RuntimeWorld owner)=>new(width,height,rgba,owner){SourceID=SourceID,Changed=Changed,wrapMode=wrapMode};
    static int Address(int value,int size,TextureWrapMode mode) => mode switch {
        TextureWrapMode.Clamp=>Math.Clamp(value,0,size-1),
        TextureWrapMode.Repeat=>((value%size)+size)%size,
        TextureWrapMode.Mirror=>Mirror(value,size),
        TextureWrapMode.MirrorOnce=>(int)Math.Clamp(value<0?-(long)value-1:(long)value,0,(long)size-1),
        _=>throw new NotSupportedException("Unknown texture wrap mode")
    };
    static int Mirror(int value,int size) {long period=(long)size*2;long p=((long)value%period+period)%period;return (int)(p<size?p:period-p-1);}
    public Color GetPixel(int x,int y) {
        int i=(Address(y,height,wrapMode)*width+Address(x,width,wrapMode))*4;
        return new(rgba[i]/255f,rgba[i+1]/255f,rgba[i+2]/255f,rgba[i+3]/255f);
    }
    internal static void ValidateRegion(int x,int y,int w,int h,int width,int height) {
        if(x<0||y<0||w<=0||h<=0||(long)x+w>width||(long)y+h>height)throw new ArgumentOutOfRangeException(nameof(x),"Pixel region is outside the texture");
    }
    public Color[] GetPixels()=>GetPixels(0,0,width,height);
    public Color[] GetPixels(int x,int y,int w,int h,int mip=0) {
        if(mip!=0)throw new NotSupportedException("Only level-zero CPU pixels are available");
        ValidateRegion(x,y,w,h,width,height);var result=new Color[checked(w*h)];
        for(int row=0;row<h;row++)for(int col=0;col<w;col++)result[row*w+col]=GetPixel(x+col,y+row);
        return result;
    }
    public void SetPixels(Color[] values)=>SetPixels(0,0,width,height,values);
    public void SetPixel(int x,int y,Color value)=>SetPixels(x,y,1,1,new[]{value});
    public void SetPixels(int x,int y,int w,int h,Color[] values) {
        ArgumentNullException.ThrowIfNull(values);ValidateRegion(x,y,w,h,width,height);
        if(values.Length!=checked(w*h))throw new ArgumentException("Color count does not match the pixel region",nameof(values));
        var patch=new byte[checked(w*h*4)];
        for(int i=0;i<values.Length;i++) {
            var c=values[i];patch[i*4]=Channel(c.r);patch[i*4+1]=Channel(c.g);patch[i*4+2]=Channel(c.b);patch[i*4+3]=Channel(c.a);
        }
        for(int row=0;row<h;row++)Array.Copy(patch,row*w*4,rgba,((y+row)*width+x)*4,w*4);
        Changed?.Invoke(this,x,y,w,h,(byte[])patch.Clone());
    }
    static byte Channel(float value) {
        if(!float.IsFinite(value))throw new ArgumentOutOfRangeException(nameof(value));
        return (byte)Math.Clamp((int)MathF.Round(Math.Clamp(value,0,1)*255),0,255);
    }
    public void ReadPixels(Rect source,int destX,int destY) {
        if(source.x!=MathF.Floor(source.x)||source.y!=MathF.Floor(source.y)||source.width!=MathF.Floor(source.width)||source.height!=MathF.Floor(source.height))
            throw new NotSupportedException("CPU readback requires integral pixel coordinates");
        int w=checked((int)source.width),h=checked((int)source.height);
        ValidateRegion(destX,destY,w,h,width,height);
        if(!ReferenceEquals(world,RuntimeWorld.Current))throw new InvalidOperationException("Readback destination belongs to another world");
        var active=RenderTexture.active??throw new InvalidOperationException("No active render texture for readback");
        SetPixels(destX,destY,w,h,Graphics.ReadPixels(active,checked((int)source.x),checked((int)source.y),w,h));
    }
    public void Apply() {} // CPU writes and upload notifications are already synchronous.
}
