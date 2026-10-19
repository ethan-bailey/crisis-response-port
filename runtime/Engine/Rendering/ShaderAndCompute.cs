namespace UnityEngine;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class ExecuteInEditModeAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequireComponentAttribute : Attribute {
    public Type RequiredComponent { get; }
    public RequireComponentAttribute(Type requiredComponent) => RequiredComponent=requiredComponent??throw new ArgumentNullException(nameof(requiredComponent));
    public RequireComponentAttribute(Type requiredComponent,Type secondRequiredComponent) : this(requiredComponent) { }
    public RequireComponentAttribute(Type requiredComponent,Type secondRequiredComponent,Type thirdRequiredComponent) : this(requiredComponent,secondRequiredComponent) { }
}

public enum FilterMode { Point, Bilinear, Trilinear }
public enum MeshTopology { Triangles,Quads,Lines,LineStrip,Points }
public enum ComputeBufferType { Default, Raw, Append, Counter, Constant, Structured, DrawIndirect, IndirectArguments }
public enum HideFlags { None=0, HideInHierarchy=1, HideInInspector=2, DontSaveInEditor=4, NotEditable=8, DontSaveInBuild=16, DontUnloadUnusedAsset=32, DontSave=52, HideAndDontSave=61 }

// Unity shader identity is resolved from imported assets by a host. A missing
// shader stays unsupported; the adapter never pretends a GPU shader ran.
public class Shader : Object {
    public static Func<string,Shader> Resolver { get; set; }
    public bool isSupported { get; set; }
    public static Shader Find(string name) {
        ArgumentException.ThrowIfNullOrEmpty(name);
        return Resolver?.Invoke(name);
    }
}

public interface IComputeBufferDriver {
    void SetData(ComputeBuffer buffer,Array values);
    void GetData(ComputeBuffer buffer,Array values);
    void SetCounterValue(ComputeBuffer buffer,uint value);
    void Release(ComputeBuffer buffer);
}

// Buffer storage preserves source data and exposes explicit driver handoff for
// compute work. It does not emulate dispatch or indirect GPU execution.
public sealed class ComputeBuffer : IDisposable {
    Array data;
    bool released;
    public static IComputeBufferDriver Driver { get; set; }
    public static void CopyCount(ComputeBuffer source,ComputeBuffer destination,int dstOffsetBytes)=>throw new NotSupportedException("GPU append counter readback requires a native compute driver");
    public int count { get; }
    public int stride { get; }
    public ComputeBufferType type { get; }
    public ComputeBuffer(int count,int stride,ComputeBufferType type=ComputeBufferType.Default) {
        if(count<=0)throw new ArgumentOutOfRangeException(nameof(count));
        if(stride<=0)throw new ArgumentOutOfRangeException(nameof(stride));
        this.count=count;this.stride=stride;this.type=type;
    }
    public bool IsValid()=>!released;
    public void SetData(Array values) {
        EnsureActive();ArgumentNullException.ThrowIfNull(values);
        if(values.Length>count)throw new ArgumentException("Compute buffer data exceeds its declared capacity",nameof(values));
        Driver?.SetData(this,values);data=(Array)values.Clone();
    }
    public void GetData(Array values) {
        EnsureActive();ArgumentNullException.ThrowIfNull(values);
        if(values.Length>count)throw new ArgumentException("Compute buffer destination exceeds its declared capacity",nameof(values));
        if(Driver!=null){Driver.GetData(this,values);return;}
        if(data==null)throw new NotSupportedException("Compute buffer has no uploaded data or native driver");
        if(values.GetType().GetElementType()!=data.GetType().GetElementType())throw new ArgumentException("Compute buffer element type mismatch",nameof(values));
        Array.Copy(data,values,Math.Min(data.Length,values.Length));
    }
    public void SetCounterValue(uint value){EnsureActive();if(Driver==null)throw new NotSupportedException("Append/counter buffers require a native compute driver");Driver.SetCounterValue(this,value);}
    public void Release(){if(released)return;Driver?.Release(this);data=null;released=true;}
    public void Dispose()=>Release();
    void EnsureActive(){if(released)throw new ObjectDisposedException(nameof(ComputeBuffer));}
}
