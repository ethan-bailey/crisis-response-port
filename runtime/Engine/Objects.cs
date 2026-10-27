using System.Collections;
using System.Reflection;
using System.Text.Json;
using N = System.Numerics;

namespace UnityEngine;

// Engine lifecycle hooks must remain usable by EngineChecks, which excludes
// the full audio transport. AudioSource supplies this narrow adapter.
public interface IAudioSourceLifecycle {
    bool IsLifecyclePlaying {get;}
    void StopLifecyclePlayback();
}

public class Object {
    static int nextID;
    internal bool destroyed;
    internal RuntimeWorld world;
    public HideFlags hideFlags {get;set;}
    readonly int instanceID=Interlocked.Increment(ref nextID);
    public virtual string name { get; set; }
    public Dictionary<string,JsonElement> OriginalSerializedFields {get;}=new(StringComparer.Ordinal);
    public byte[] OriginalSerializedBytes {get;internal set;}
    public string OriginalSerializedSha256 {get;internal set;}
    public string OriginalAssetGuid {get;internal set;}
    public string OriginalAssetFileID {get;internal set;}
    public string OriginalAssetPath {get;internal set;}
    public string OriginalAssetSha256 {get;internal set;}
    public int GetInstanceID() => instanceID;
    public static implicit operator bool(Object o) => !ReferenceEquals(o,null)&&!o.destroyed;
    public static bool operator ==(Object a,Object b) {
        bool an=ReferenceEquals(a,null)||a.destroyed, bn=ReferenceEquals(b,null)||b.destroyed;
        return an||bn ? an==bn : ReferenceEquals(a,b);
    }
    public static bool operator !=(Object a,Object b) => !(a==b);
    public override bool Equals(object other) => other is Object obj ? this==obj : other is null && destroyed;
    public override int GetHashCode() => instanceID;
    public static void Destroy(Object obj) { if(obj) obj.world.QueueDestroy(obj); }
    public static void DestroyImmediate(Object obj,bool allowDestroyingAssets=false) { if(obj)obj.world.DestroyNow(obj); }
    public static void DontDestroyOnLoad(Object obj) { if(!obj)throw new ArgumentNullException(nameof(obj)); }
    public static T[] FindObjectsOfType<T>() wh