namespace UnityEngine;

public enum TextureFormat { RGBA32=4 }

// CPU texture data is bottom-left first. Rendering/upload observers remain
// separate; no GPU completion is claimed by changing these pixels.
internal static class PngTextureDecoder {
    static readonly byte[] Signature={137,80,78,71,13,10,26,10};
    static uint ReadU32(byte[] bytes,int offset)=>System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(offset,4));
    static uint Crc32(ReadOnlySpan<byte> bytes) {
        uint crc=0xffffffff;
        foreach(byte value in bytes){crc^=value;for(int bit=0;bit<8;bit++)crc=(crc&1)!=0?(crc>>1)^0xedb88320:crc>>1;}
        return crc^0xffffffff;
    }
    static byte Paeth(byte a,byte b,byte c) {
        int p=a+b-c,pa=Math.Abs(p-a),pb=Math.Abs(p-b),pc=Math.Abs(p-c);
        return pa<=pb&&pa<=pc?a:pb<=pc?b:c;
    }
    internal static (int width,int height,byte[] rgba) Decode(byte[] png) {
        ArgumentNullException.ThrowIfNull(png);
        if(png.Length<8||!png.AsSpan(0,8).SequenceEqual(Signature))throw new InvalidDataException("Texture asset is not a PNG");
        int offset=8,width=0,height=0,channels=0;bool header=false,end=false,seenData=false;using var compressed=new System.IO.MemoryStream();
        while(offset<=png.Length-12) {
            uint length=ReadU32(png,offset);offset+=4;
            if(length>int.MaxValue||length>(uint)(png.Length-offset-8))throw new InvalidDataException("PNG chunk exceeds source bounds");
            int count=(int)length;var type=System.Text.Encoding.ASCII.GetString(png,offset,4);int typeOffset=offset;offset+=4;int dataOffset=offset;
            uint actualCrc=Crc32(png.AsSpan(typeOffset,checked(4+count)));
            if(ReadU32(png,checked(dataOffset+count))!=actualCrc)throw new InvalidDataException($"PNG {type} chunk CRC mismatch");
            if(type=="IHDR") {
                if(header||offset!=16||count!=13)throw new InvalidDataException("PNG must have one leading 13-byte IHDR");
                width=checked((int)ReadU32(png,dataOffset));height=checked((int)ReadU32(png,dataOffset+4));
                if(width<=0||height<=0||width>2048||height>2048)throw new NotSupportedException($"PNG dimensions {width}x{height} exceed the preserved importer limit");
                channels=png[dataOffset+9] switch {2=>3,6=>4,_=>0};
                if(png[dataOffset+8]!=8||channels==0||png[dataOffset+10]!=0||png[dataOffset+11]!=0||png[dataOffset+12]!=0)
                    throw new NotSupportedException("Only non-interlaced 8-bit RGB or RGBA PNG textures are supported");
                header=true;
            } else if(type=="IDAT") {
                if(!header||end)throw new InvalidDataException("PNG IDAT appears outside the image-data section");
                seenData=true;compressed.Write(png,dataOffset,count);
            } else if(type=="IEND") {
                if(count!=0||!header||!seenData)throw new InvalidDataException("PNG IEND is invalid");
                end=true;offset=checked(dataOffset+count+4);break;
            } else if((type[0]&0x20)==0&&type!="PLTE")throw new NotSupportedException($"Unsupported critical PNG chunk {type}");
            offset=checked(dataOffset+count+4);
        }
        if(!end||offset!=png.Length)throw new InvalidDataException("PNG is missing IEND or has trailing bytes");
        int sourceRowBytes=checked(width*channels),expected=checked(height*(sourceRowBytes+1));compressed.Position=0;
        using var inflater=new System.IO.Compression.ZLibStream(compressed,System.IO.Compression.CompressionMode.Decompress,true);
        using var decoded=new System.IO.MemoryStream(expected);inflater.CopyTo(decoded);
        if(decoded.Length!=expected)throw new InvalidDataException($"PNG pixel stream has {decoded.Length} bytes; expected {expected}");
        byte[] filtered=decoded.ToArray(),rgba=new byte[checked(width*height*4)],previous=new byte[sourceRowBytes],current=new byte[sourceRowBytes];int at=0;
        for(int y=0;y<height;y++) {
            byte filter=filtered[at++];
            if(filter>4)throw new InvalidDataException($"Unsupported PNG row filter {filter}");
            for(int i=0;i<sourceRowBytes;i++) {
                byte left=i>=channels?current[i-channels]:(byte)0,up=previous[i],upperLeft=i>=channels?previous[i-channels]:(byte)0;
                int predictor=filter switch {0=>0,1=>left,2=>up,3=>(left+up)/2,4=>Paeth(left,up,upperLeft),_=>throw new InvalidDataException()};
                current[i]=unchecked((byte)(filtered[at++]+predictor));
            }
            // PNG stores top row first; Unity CPU textures address from bottom-left.
            int targetRow=(height-y-1)*width*4;
            for(int x=0;x<width;x++){
                int source=x*channels,target=targetRow+x*4;
                rgba[target]=current[source];rgba[target+1]=current[source+1];rgba[target+2]=current[source+2];rgba[target+3]=channels==4?current[source+3]:(byte)255;
            }
            (previous,current)=(current,previous);
        }
        return(width,height,rgba);
    }
}

public sealed class Texture2D:Texture {
    public int width {get;private set;}
    public int height {get;private set;}
    byte[] rgba;
    public byte[] OriginalImageBytes {get;private set;}
    public string ImporterSourceText {get;internal set;}
    public string ImporterRuntimeDisposition {get;internal set;}
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
    public bool LoadImage(byte[] imageData,bool markNonReadable=false) {
        var decoded=PngTextureDecoder.Decode(imageData);width=decoded.width;height=decoded.height;rgba=decoded.rgba;
        OriginalImageBytes=(byte[])imageData.Clone();return true;
    }
    static byte[] Allocate(int w,int h,TextureFormat format,bool mipChain) {
        if(format!=TextureFormat.RGBA32||mipChain)throw new NotSupportedException("This CPU texture adapter supports RGBA32 level-zero textures without mip chains");
        return new byte[ByteCount(w,h)];
    }
    public Texture2D(int w,int h,TextureFormat format,bool mipChain):this(w,h,Allocate(w,h,format,mipChain)) { }
    public Texture2D CloneTexture(RuntimeWorld owner)=>new(width,height,rgba,owner){SourceID=SourceID,Changed=Changed,wrapMode=wrapMode,OriginalImageBytes=OriginalImageBytes is null?null:(byte[])OriginalImageBytes.Clone(),ImporterSourceText=ImporterSourceText,ImporterRuntimeDisposition=ImporterRuntimeDisposition};
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
