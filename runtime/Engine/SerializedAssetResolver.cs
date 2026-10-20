using Crisis.Runtime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using UnityEngine.Audio;

namespace UnityEngine;

/// <summary>Resolves serialized Unity GUID/fileID pairs against the preserved execution package.</summary>
internal static class SerializedAssetResolver {
    static readonly ConditionalWeakTable<RuntimeWorld,Dictionary<string,Object>> cacheByWorld=new();
    internal static Object Resolve(JsonElement package,string guid,string fileID,RuntimeWorld world,Type expectedType) {
        ArgumentNullException.ThrowIfNull(world);ArgumentNullException.ThrowIfNull(expectedType);
        var cache=cacheByWorld.GetValue(world,_=>new(StringComparer.Ordinal));
        string key=guid+":"+fileID;
        if(cache.TryGetValue(key,out var cached))return expectedType.IsInstanceOfType(cached)?cached:
            throw new InvalidCastException($"Serialized reference {key} is {cached.GetType().Name}, expected {expectedType.Name}");
        if(package.TryGetProperty("builtinObjects",out var builtins)&&builtins.TryGetProperty(guid,out var builtinGuid)&&builtinGuid.TryGetProperty(fileID,out var builtinType)) {
            var builtinCreated=CreateBuiltin(guid,fileID,builtinType.GetString(),world);
            if(!expectedType.IsInstanceOfType(builtinCreated))throw new InvalidCastException($"Built-in reference {key} is {builtinCreated.GetType().Name}, expected {expectedType.Name}");
            builtinCreated.OriginalAssetGuid=guid;builtinCreated.OriginalAssetFileID=fileID;cache[key]=builtinCreated;return builtinCreated;
        }
        if(!package.TryGetProperty("guidPaths",out var paths)||!paths.TryGetProperty(guid,out var pathValue))
            throw new KeyNotFoundException($"Package has no asset for GUID {guid} ({fileID})");
        string path=pathValue.GetString();
        if(!package.GetProperty("assets").TryGetProperty(path,out var asset))throw new KeyNotFoundException($"Package GUID {guid} points to missing asset {path}");
        var assetRecords=asset.GetProperty("records");
        if(assetRecords.GetArrayLength()==0&&expectedType==typeof(AudioClip)&&asset.TryGetProperty("dataBase64",out var encodedAudio)){
            var bytes=Convert.FromBase64String(encodedAudio.GetString());float duration=WavDuration(bytes);var clip=new AudioClip(path,duration){SourceData=bytes,OriginalAssetGuid=guid,OriginalAssetFileID=fileID,OriginalAssetPath=path};world.Register(clip);cache[key]=clip;return clip;
        }
        var record=assetRecords.EnumerateArray().FirstOrDefault(r=>r.GetProperty("fileID").GetString()==fileID);
        if(record.ValueKind==JsonValueKind.Undefined)throw new KeyNotFoundException($"Package asset {path} has no serialized fileID {fileID}");
        var fields=record.GetProperty("fields");int classID=record.GetProperty("classID").GetInt32();
        if(classID==114&&path.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase)){
            var nested=SceneLoader.LoadPrefab(package.GetRawText(),path,world,requireComplete:true);
            foreach(var gameObject in nested.objects.Values.OfType<GameObject>())gameObject.assetObject=true;
            if(!nested.objects.TryGetValue(fileID,out var component))throw new KeyNotFoundException($"Nested prefab {path} has no object {fileID}");
            if(!expectedType.IsInstanceOfType(component))throw new InvalidCastException($"Nested prefab component {path}:{fileID} is {component.GetType().Name}, expected {expectedType.Name}");
            cache[key]=component;return component;
        }
        var created=CreateAsset(classID,fields,world);
        if(!expectedType.IsInstanceOfType(created))throw new InvalidCastException($"Package object {path}:{fileID} is {created.GetType().Name}, expected {expectedType.Name}");
        created.name=fields.TryGetProperty("m_Name",out var name)?name.GetString():path;
        created.OriginalAssetGuid=guid;created.OriginalAssetFileID=fileID;created.OriginalAssetPath=path;
        foreach(var property in fields.EnumerateObject())created.OriginalSerializedFields[property.Name]=property.Value.Clone();
        world.Register(created);cache[key]=created;return created;
    }
    static float WavDuration(byte[] bytes){
        if(bytes.Length<44||System.Text.Encoding.ASCII.GetString(bytes,0,4)!="RIFF"||System.Text.Encoding.ASCII.GetString(bytes,8,4)!="WAVE")throw new InvalidDataException("Packaged AudioClip is not a supported RIFF/WAVE asset");
        uint byteRate=System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(28,4));if(byteRate==0)throw new InvalidDataException("Packaged WAVE AudioClip has a zero byte rate");
        for(int i=12;i+8<=bytes.Length;){string chunk=System.Text.Encoding.ASCII.GetString(bytes,i,4);uint size=System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(i+4,4));if(chunk=="data")return (float)size/byteRate;i=checked(i+8+(int)size+(int)(size&1));}
        throw new InvalidDataException("Packaged WAVE AudioClip has no data chunk");
    }
    static Object CreateBuiltin(string guid,string fileID,string kind,RuntimeWorld world) => kind switch {
        "Mesh"=>new Mesh {name=$"builtin:{guid}:{fileID}"},
        "Font"=>new Font {name=$"builtin:{guid}:{fileID}"},
        "Shader"=>new Shader {name=$"builtin:{guid}:{fileID}"},
        _=>throw new NotSupportedException($"Built-in Unity object {guid}:{fileID} of type {kind} is not supported")
    };
    static Object CreateAsset(int classID,JsonElement fields,RuntimeWorld world) => classID switch {
        21=>new Material(),
        43=>new Mesh(),
        48=>new Shader(),
        91=>new RuntimeAnimatorController(),
        134=>new PhysicMaterial(),
        243=>new AudioMixerGroup(),
        _=>throw new NotSupportedException($"Unity package asset classID {classID} ({fields.GetProperty("m_Name").GetString()}) has no runtime mapping")
    };
}

/// <summary>Identity and original serialized payload retained for an imported asset object.</summary>
public static class SerializedAssetIdentity {
    public static string Guid(Object value)=>value?.OriginalAssetGuid;
    public static string FileID(Object value)=>value?.OriginalAssetFileID;
    public static string Path(Object value)=>value?.OriginalAssetPath;
}

public sealed class PhysicMaterial:Object {
    public float dynamicFriction {get;set;}
    public float staticFriction {get;set;}
    public float bounciness {get;set;}
    public int frictionCombine {get;set;}
    public int bounceCombine {get;set;}
}
public class RuntimeAnimatorController:Object { }
