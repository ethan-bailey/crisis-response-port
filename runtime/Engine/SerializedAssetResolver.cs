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
        if(assetRecords.GetArrayLength()==0&&expectedType==typeof(Texture2D)&&asset.TryGetProperty("dataBase64",out var encodedTexture))
            return ResolvePngTexture(package,asset,guid,fileID,path,Convert.FromBase64String(encodedTexture.GetString()),world,cache);
        if(assetRecords.GetArrayLength()==0&&expectedType==typeof(AudioClip)&&asset.TryGetProperty("dataBase64",out var encodedAudio)){
            var bytes=Convert.FromBase64String(encodedAudio.GetString());float duration=WavDuration(bytes);var clip=new AudioClip(path,duration){SourceData=bytes,OriginalAssetGuid=guid,OriginalAssetFileID=fileID,OriginalAssetPath=path};world.Register(clip);cache[key]=clip;return clip;
        }
        var record=assetRecords.EnumerateArray().FirstOrDefault(r=>r.GetProperty("fileID").GetString()==fileID);
        if(record.ValueKind==JsonValueKind.Undefined)throw new KeyNotFoundException($"Package asset {path} has no serialized fileID {fileID}");
        var fields=record.GetProperty("fields");int classID=record.GetProperty("classID").GetInt32();
        if(classID==114&&path.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase)){
            var nested=SceneLoader.LoadPrefab(package,path,world,requireComplete:true);
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
    static Texture2D ResolvePngTexture(JsonElement package,JsonElement asset,string guid,string fileID,string path,byte[] bytes,RuntimeWorld world,Dictionary<string,Object> cache) {
        if(!path.EndsWith(".png",StringComparison.OrdinalIgnoreCase))throw new NotSupportedException($"Binary Texture2D format is unsupported: {path}");
        string sourceHash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes)).ToLowerInvariant();
        if(!asset.TryGetProperty("sha256",out var sourceHashValue)||sourceHashValue.GetString()!=sourceHash)
            throw new InvalidDataException($"Packaged texture source hash does not match {path}");
        if(!package.TryGetProperty("guidMetadata",out var metadata)||!metadata.TryGetProperty(guid,out var metaEntry)||
           !metaEntry.TryGetProperty("importerSourceText",out var importerValue)||
           !metaEntry.TryGetProperty("importerSourceBase64",out var importerBytesValue))
            throw new InvalidDataException($"Original TextureImporter metadata is missing for {path}");
        string importer=importerValue.GetString()??throw new InvalidDataException($"Original TextureImporter metadata is empty for {path}");
        byte[] importerBytes=Convert.FromBase64String(importerBytesValue.GetString());
        if(!System.Text.Encoding.UTF8.GetString(importerBytes).Equals(importer,StringComparison.Ordinal))
            throw new InvalidDataException($"Original TextureImporter text differs from its preserved bytes for {path}");
        string metaHash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(importerBytes)).ToLowerInvariant();
        if(!metaEntry.TryGetProperty("metaSha256",out var metaHashValue)||metaHashValue.GetString()!=metaHash)
            throw new InvalidDataException($"Original TextureImporter metadata hash does not match {path}");
        if(!System.Text.RegularExpressions.Regex.IsMatch(importer,$@"(?m)^guid:\s*{System.Text.RegularExpressions.Regex.Escape(guid)}\s*$")||
           !System.Text.RegularExpressions.Regex.IsMatch(importer,@"(?m)^\s*fileIDToRecycleName:\s*\{\}\s*$")||
           !System.Text.RegularExpressions.Regex.IsMatch(importer,@"(?m)^\s*isReadable:\s*1\s*$")||
           !System.Text.RegularExpressions.Regex.IsMatch(importer,@"(?m)^\s*textureFormat:\s*4\s*$")||
           !System.Text.RegularExpressions.Regex.IsMatch(importer,@"(?m)^\s*textureType:\s*0\s*$")||
           !System.Text.RegularExpressions.Regex.IsMatch(importer,@"(?m)^\s*maxTextureSize:\s*2048\s*$"))
            throw new NotSupportedException($"Original TextureImporter identity or settings are unknown or unsupported for {path}");
        if(!package.TryGetProperty("objectReferences",out var references))
            throw new InvalidDataException("Package has no serialized object reference provenance for binary texture");
        var referencedIDs=references.EnumerateArray()
            .Where(r=>r.TryGetProperty("guid",out var g)&&g.GetString()==guid)
            .Select(r=>r.TryGetProperty("fileID",out var id)&&id.ValueKind!=JsonValueKind.Null?(id.ValueKind==JsonValueKind.String?id.GetString():id.ToString()):null)
            .Where(id=>id!=null).Distinct(StringComparer.Ordinal).ToArray();
        if(referencedIDs.Length!=1||referencedIDs[0]!=fileID)
            throw new InvalidDataException($"Texture {path} original fileID is ambiguous or differs from serialized references; requested {fileID}, observed [{string.Join(",",referencedIDs)}]");
        var texture=new Texture2D(1,1,new byte[4],world);
        texture.LoadImage(bytes);
        texture.name=path;texture.OriginalAssetGuid=guid;texture.OriginalAssetFileID=fileID;texture.OriginalAssetPath=path;
        texture.ImporterSourceText=importer;cache[guid+":"+fileID]=texture;return texture;
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
