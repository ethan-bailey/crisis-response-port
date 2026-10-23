using Crisis.Runtime;
using System.Runtime.CompilerServices;
using System.Text.Json;
using UnityEngine.Audio;

namespace UnityEngine;

/// <summary>Resolves serialized Unity GUID/fileID pairs against the preserved execution package.</summary>
internal static class SerializedAssetResolver {
    static readonly ConditionalWeakTable<RuntimeWorld,Dictionary<string,Object>> cacheByWorld=new();
    internal static Object Resolve(JsonElement package,string guid,string fileID,RuntimeWorld world,Type expectedType,bool requireComplete=true) {
        ArgumentNullException.ThrowIfNull(world);ArgumentNullException.ThrowIfNull(expectedType);
        var cache=cacheByWorld.GetValue(world,_=>new(StringComparer.Ordinal));
        string key=guid+":"+fileID;
        if(cache.TryGetValue(key,out var cached))return expectedType.IsInstanceOfType(cached)?cached:
            throw new InvalidCastException($"Serialized reference {key} is {cached.GetType().Name}, expected {expectedType.Name}");
        if(package.TryGetProperty("builtinObjects",out var builtins)&&builtins.TryGetProperty(guid,out var builtinGuid)&&builtinGuid.TryGetProperty(fileID,out var builtinType)) {
            var builtinCreated=CreateBuiltin(package,guid,fileID,builtinType.GetString(),world,cache);
            if(!expectedType.IsInstanceOfType(builtinCreated))throw new InvalidCastException($"Built-in reference {key} is {builtinCreated.GetType().Name}, expected {expectedType.Name}");
            builtinCreated.OriginalAssetGuid=guid;builtinCreated.OriginalAssetFileID=fileID;cache[key]=builtinCreated;return builtinCreated;
        }
        if(guid=="0000000000000000f000000000000000")
            throw new NotSupportedException($"Unity built-in object {guid}:{fileID} has no source-faithful runtime mapping for expected {expectedType.Name}; refusing a synthetic substitute");
        if(!package.TryGetProperty("guidPaths",out var paths)||!paths.TryGetProperty(guid,out var pathValue))
            throw new KeyNotFoundException($"Package has no asset for GUID {guid} ({fileID})");
        string path=pathValue.GetString();
        if(!package.GetProperty("assets").TryGetProperty(path,out var asset))throw new KeyNotFoundException($"Package GUID {guid} points to missing asset {path}");
        var assetRecords=asset.GetProperty("records");
        if(assetRecords.GetArrayLength()==0&&expectedType==typeof(Texture2D)&&asset.TryGetProperty("dataBase64",out var encodedTexture))
            return ResolvePngTexture(package,asset,guid,fileID,path,Convert.FromBase64String(encodedTexture.GetString()),world,cache);
        if(assetRecords.GetArrayLength()==0&&expectedType==typeof(AudioClip)&&asset.TryGetProperty("dataBase64",out var encodedAudio)){
            var bytes=Convert.FromBase64String(encodedAudio.GetString());float duration=AudioDuration(path,bytes);var clip=new AudioClip(path,duration){SourceData=bytes,OriginalAssetGuid=guid,OriginalAssetFileID=fileID,OriginalAssetPath=path};world.Register(clip);cache[key]=clip;return clip;
        }
        var record=assetRecords.EnumerateArray().FirstOrDefault(r=>r.GetProperty("fileID").GetString()==fileID);
        if(record.ValueKind==JsonValueKind.Undefined)throw new KeyNotFoundException($"Package asset {path} has no serialized fileID {fileID}");
        var fields=record.GetProperty("fields");int classID=record.GetProperty("classID").GetInt32();
        if((typeof(Component).IsAssignableFrom(expectedType)||expectedType==typeof(GameObject))&&path.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase)){
            var nested=SceneLoader.LoadPrefab(package,path,world,requireComplete);
            foreach(var gameObject in nested.objects.Values.OfType<GameObject>())gameObject.assetObject=true;
            if(!nested.objects.TryGetValue(fileID,out var component))throw new KeyNotFoundException($"Nested prefab {path} has no object {fileID}");
            if(!expectedType.IsInstanceOfType(component))throw new InvalidCastException($"Nested prefab component {path}:{fileID} is {component.GetType().Name}, expected {expectedType.Name}");
            component.OriginalAssetGuid=guid;component.OriginalAssetFileID=fileID;component.OriginalAssetPath=path;
            cache[key]=component;return component;
        }
        var created=CreateAsset(classID,fields,world);
        if(!expectedType.IsInstanceOfType(created))throw new InvalidCastException($"Package object {path}:{fileID} is {created.GetType().Name}, expected {expectedType.Name}");
        created.name=fields.TryGetProperty("m_Name",out var name)?name.GetString():path;
        created.OriginalAssetGuid=guid;created.OriginalAssetFileID=fileID;created.OriginalAssetPath=path;
        foreach(var property in fields.EnumerateObject())created.OriginalSerializedFields[property.Name]=property.Value.Clone();
        if(created is Audio.AudioMixerGroup mixerGroup)mixerGroup.audioMixer=ResolveSiblingMixer(assetRecords,guid,path,world,cache);
        if(created is Audio.AudioMixerSnapshot mixerSnapshot)mixerSnapshot.audioMixer=ResolveSiblingMixer(assetRecords,guid,path,world,cache);
        world.Register(created);cache[key]=created;return created;
    }
    static Audio.AudioMixer ResolveSiblingMixer(JsonElement assetRecords,string guid,string path,RuntimeWorld world,Dictionary<string,Object> cache){
        const string mixerID="24100000";string key=guid+":"+mixerID;
        if(cache.TryGetValue(key,out var cached))return cached as Audio.AudioMixer??throw new InvalidCastException($"Audio mixer asset {path}:{mixerID} is not an AudioMixer");
        var record=assetRecords.EnumerateArray().FirstOrDefault(item=>item.GetProperty("fileID").GetString()==mixerID);
        if(record.ValueKind==JsonValueKind.Undefined)throw new KeyNotFoundException($"Audio mixer asset {path} has no serialized mixer object {mixerID}");
        var fields=record.GetProperty("fields");var mixer=new Audio.AudioMixer{name=fields.GetProperty("m_Name").GetString(),OriginalAssetGuid=guid,OriginalAssetFileID=mixerID,OriginalAssetPath=path};
        foreach(var property in fields.EnumerateObject())mixer.OriginalSerializedFields[property.Name]=property.Value.Clone();
        world.Register(mixer);cache[key]=mixer;return mixer;
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
        bool HasImporterField(string field,string value)=>System.Text.RegularExpressions.Regex.IsMatch(importer,$@"(?m)^  {System.Text.RegularExpressions.Regex.Escape(field)}:\s*{System.Text.RegularExpressions.Regex.Escape(value)}\s*$");
        if(!System.Text.RegularExpressions.Regex.IsMatch(importer,$@"(?m)^guid:\s*{System.Text.RegularExpressions.Regex.Escape(guid)}\s*$")||
           !HasImporterField("fileIDToRecycleName","{}"))
            throw new NotSupportedException($"Original TextureImporter identity is unknown for {path}");
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
        texture.ImporterSourceText=importer;texture.ImporterRuntimeDisposition="Decoded verified original PNG source to CPU RGBA32; original importer YAML and hashes are retained verbatim; importer platform settings, compression, and GPU output are not emulated.";cache[guid+":"+fileID]=texture;return texture;
    }
    static float AudioDuration(string path,byte[] bytes){
        string extension=System.IO.Path.GetExtension(path);
        if(extension.Equals(".wav",StringComparison.OrdinalIgnoreCase))return WavDuration(bytes);
        if(extension.Equals(".ogg",StringComparison.OrdinalIgnoreCase))return OggVorbisDuration(bytes);
        // Tracker modules are preserved byte-for-byte, but this runtime has no
        // tracker decoder from which to report a duration. Keep the duration
        // explicitly unknown (AudioClip accepts zero) instead of rejecting a
        // valid original .mod dependency or inventing a playback length.
        if(extension.Equals(".mod",StringComparison.OrdinalIgnoreCase)&&bytes.Length>=1084)return 0f;
        throw new NotSupportedException("Packaged AudioClip '"+path+"' uses unsupported source extension '"+extension+"'");
    }
    static float OggVorbisDuration(byte[] bytes){
        int offset=0;
        uint streamSerial=0,expectedSequence=0,sampleRate=0;
        bool firstPage=true,endOfStream=false,packetContinues=false,identificationComplete=false;
        ulong finalGranule=ulong.MaxValue;
        var identification=new System.Collections.Generic.List<byte>();
        while(offset<bytes.Length){
            if(endOfStream||bytes.Length-offset<27)throw new InvalidDataException("Packaged Ogg AudioClip has trailing data or a truncated page header");
            if(System.Text.Encoding.ASCII.GetString(bytes,offset,4)!="OggS"||bytes[offset+4]!=0)throw new InvalidDataException("Packaged AudioClip has an invalid Ogg page header");
            byte flags=bytes[offset+5];
            if((flags&~7)!=0)throw new InvalidDataException("Packaged Ogg AudioClip uses reserved page flags");
            bool continued=(flags&1)!=0,beginning=(flags&2)!=0,ending=(flags&4)!=0;
            if((firstPage&&!beginning)||(!firstPage&&beginning)||continued!=packetContinues)throw new InvalidDataException("Packaged Ogg AudioClip has invalid BOS or packet continuation flags");
            ulong granule=System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(offset+6,8));
            uint serial=System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset+14,4));
            uint sequence=System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(offset+18,4));
            if(sequence!=expectedSequence)throw new InvalidDataException("Packaged Ogg AudioClip has a discontinuous page sequence");
            if(firstPage){streamSerial=serial;firstPage=false;}
            else if(serial!=streamSerial)throw new InvalidDataException("Packaged Ogg AudioClip contains multiple logical streams");
            expectedSequence=checked(sequence+1);
            int count=bytes[offset+26],table=offset+27;
            if(bytes.Length-table<count)throw new InvalidDataException("Packaged Ogg AudioClip has a truncated lacing table");
            int body=table+count,length=0;
            for(int i=0;i<count;i++)length=checked(length+bytes[table+i]);
            if(length>bytes.Length-body)throw new InvalidDataException("Packaged Ogg AudioClip has a truncated page body");
            int cursor=body;bool continues=packetContinues;
            for(int i=0;i<count;i++){
                int segment=bytes[table+i];
                if(!identificationComplete){
                    for(int j=0;j<segment;j++)identification.Add(bytes[cursor+j]);
                    if(segment<255){
                        identificationComplete=true;
                        if(identification.Count!=30)throw new InvalidDataException("Packaged Ogg AudioClip has an invalid Vorbis identification header length");
                        byte[] h=identification.ToArray();
                        if(h[0]!=1||System.Text.Encoding.ASCII.GetString(h,1,6)!="vorbis"||System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(h.AsSpan(7,4))!=0)throw new InvalidDataException("Packaged Ogg AudioClip uses an unsupported Vorbis identification header");
                        if(h[11]==0)throw new InvalidDataException("Packaged Vorbis AudioClip has zero channels");
                        sampleRate=System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(h.AsSpan(12,4));
                        if(sampleRate==0||(h[28]&15)<6||(h[28]>>4)<(h[28]&15)||(h[28]>>4)>13||(h[29]&1)==0)throw new InvalidDataException("Packaged Vorbis AudioClip has invalid identification metadata");
                    }
                }
                cursor+=segment;continues=segment==255;
            }
            packetContinues=continues;
            if(granule!=ulong.MaxValue)finalGranule=granule;
            offset=body+length;
            if(ending){
                if(offset!=bytes.Length||packetContinues)throw new InvalidDataException("Packaged Ogg AudioClip has an invalid EOS page");
                endOfStream=true;
            }
        }
        if(firstPage||!endOfStream||!identificationComplete||sampleRate==0)throw new InvalidDataException("Packaged Ogg AudioClip is incomplete");
        if(finalGranule==ulong.MaxValue||finalGranule==0)throw new InvalidDataException("Packaged Ogg AudioClip has no valid final granule position");
        double duration=(double)finalGranule/sampleRate;
        if(double.IsInfinity(duration)||duration<=0||duration>float.MaxValue)throw new InvalidDataException("Packaged Ogg AudioClip has an invalid duration");
        return (float)duration;
    }
    static float WavDuration(byte[] bytes){
        if(bytes.Length<44||System.Text.Encoding.ASCII.GetString(bytes,0,4)!="RIFF"||System.Text.Encoding.ASCII.GetString(bytes,8,4)!="WAVE")throw new InvalidDataException("Packaged AudioClip is not a supported RIFF/WAVE asset");
        uint byteRate=System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(28,4));if(byteRate==0)throw new InvalidDataException("Packaged WAVE AudioClip has a zero byte rate");
        for(int i=12;i+8<=bytes.Length;){string chunk=System.Text.Encoding.ASCII.GetString(bytes,i,4);uint size=System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(i+4,4));if(chunk=="data")return (float)size/byteRate;i=checked(i+8+(int)size+(int)(size&1));}
        throw new InvalidDataException("Packaged WAVE AudioClip has no data chunk");
    }
    static Object CreateBuiltin(JsonElement package,string guid,string fileID,string kind,RuntimeWorld world,Dictionary<string,Object> cache) {
        if(package.TryGetProperty("builtinResourceRecords",out var resourcePack)&&
           resourcePack.TryGetProperty("records",out var records)&&records.TryGetProperty(fileID,out var record))
            return CreateRecoveredBuiltin(package,guid,fileID,kind,record,world,cache);
        if(package.TryGetProperty("builtinPlayerResourceRecords",out var playerPack)&&
           playerPack.TryGetProperty("records",out var playerRecords)&&playerRecords.TryGetProperty(fileID,out var playerRecord))
            return CreateRecoveredBuiltin(package,guid,fileID,kind,playerRecord,world,cache);
        if(kind=="Material"||(kind=="Shader"&&fileID=="46"))
            throw new NotSupportedException($"Unity built-in {kind} {guid}:{fileID} has no recovered source record in the execution package");
        return kind switch {
            "Mesh"=>new Mesh {name=$"builtin:{guid}:{fileID}"},
            "Font"=>new Font {name=$"builtin:{guid}:{fileID}"},
            "Shader"=>new Shader {name=$"builtin:{guid}:{fileID}"},
            _=>throw new NotSupportedException($"Built-in Unity object {guid}:{fileID} of type {kind} is not supported")
        };
    }
    static Object CreateRecoveredBuiltin(JsonElement package,string guid,string fileID,string expectedKind,JsonElement record,RuntimeWorld world,Dictionary<string,Object> cache) {
        string recordKind=record.GetProperty("type").GetString();
        if(recordKind!=expectedKind||record.GetProperty("fileID").GetString()!=fileID)
            throw new InvalidDataException($"Recovered built-in record {guid}:{fileID} has type/ID {recordKind}:{record.GetProperty("fileID").GetString()}, expected {expectedKind}:{fileID}");
        byte[] raw=Convert.FromBase64String(record.GetProperty("rawBase64").GetString());
        string rawHash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(raw)).ToLowerInvariant();
        if(raw.Length!=record.GetProperty("rawBytes").GetInt32()||rawHash!=record.GetProperty("rawSha256").GetString())
            throw new InvalidDataException($"Recovered built-in {expectedKind} {fileID} raw record failed its length or SHA-256 check");
        var fields=record.GetProperty("fields");
        Object created;
        if(expectedKind=="Shader") {
            string shaderName=fields.GetProperty("m_ParsedForm").GetProperty("m_Name").GetString();
            created=new Shader {name=shaderName,isSupported=false};
        } else if(expectedKind=="Material") {
            if(fileID!="10303"&&fileID!="10754")throw new NotSupportedException($"Source-backed built-in Material {fileID} is not in the runtime record set");
            var material=new Material {
                name=fields.GetProperty("m_Name").GetString(),
                hideFlags=(HideFlags)fields.GetProperty("m_ObjectHideFlags").GetInt32(),
                renderQueue=fields.GetProperty("m_CustomRenderQueue").GetInt32(),
                enableInstancing=fields.GetProperty("m_EnableInstancingVariants").GetBoolean(),
                doubleSidedGI=fields.GetProperty("m_DoubleSidedGI").GetBoolean(),
                lightmapFlags=fields.GetProperty("m_LightmapFlags").GetInt32()
            };
            ApplyRecoveredMaterialProperties(material,fields.GetProperty("m_SavedProperties"));
            var shaderPointer=fields.GetProperty("m_Shader");
            if(shaderPointer.GetProperty("m_FileID").GetInt32()!=0)
                throw new InvalidDataException($"Recovered built-in Material {fileID} has an unexpected external shader fileID");
            string shaderID=shaderPointer.GetProperty("m_PathID").GetInt64().ToString(System.Globalization.CultureInfo.InvariantCulture);
            material.shader=(Shader)CreateBuiltin(package,guid,shaderID,"Shader",world,cache);
            created=material;
        } else if(expectedKind=="Mesh") {
            if(fileID!="10206"&&fileID!="10210")
                throw new NotSupportedException($"Source-backed built-in Mesh {fileID} is not in the recovered player resource record set");
            if(fileID=="10206"&&fields.GetProperty("m_Name").GetString()!="Cylinder")
                throw new InvalidDataException("Recovered Windows x86 Mono Mesh 10206 is not the source Cylinder");
            var geometry=record.GetProperty("runtimeGeometry");
            var mesh=new Mesh {name=fields.GetProperty("m_Name").GetString()};
            mesh.vertices=ReadVector3Array(geometry.GetProperty("vertices"));
            mesh.triangles=geometry.GetProperty("triangles").EnumerateArray().Select(index=>index.GetInt32()).ToArray();
            mesh.uv=ReadVector2Array(geometry.GetProperty("uv"));
            var sourceNormals=ReadVector3Array(geometry.GetProperty("normals"));
            if(sourceNormals.Length>0)mesh.SetImportedNormals(sourceNormals);
            if(mesh.vertexCount==0||mesh.triangles.Length==0||mesh.triangles.Length%3!=0||
               (mesh.uv.Length!=0&&mesh.uv.Length!=mesh.vertexCount)||
               (sourceNormals.Length!=0&&sourceNormals.Length!=mesh.vertexCount))
                throw new InvalidDataException($"Recovered Windows x86 Mono Mesh {fileID} geometry has inconsistent source channels");
            if(fileID=="10206"&&(mesh.vertexCount!=88||mesh.triangles.Length!=240||mesh.uv.Length!=88||sourceNormals.Length!=88))
                throw new InvalidDataException("Recovered Windows x86 Mono Cylinder geometry has unexpected vertex, index, UV, or normal counts");
            created=mesh;
        } else throw new NotSupportedException($"Recovered built-in type {expectedKind} is not supported");
        created.OriginalAssetGuid=guid;created.OriginalAssetFileID=fileID;
        created.OriginalAssetPath=record.GetProperty("sourceFile").GetString();
        created.OriginalSerializedBytes=raw;created.OriginalSerializedSha256=rawHash;
        foreach(var property in fields.EnumerateObject())created.OriginalSerializedFields[property.Name]=property.Value.Clone();
        world.Register(created);cache[guid+":"+fileID]=created;return created;
    }
    static Vector3[] ReadVector3Array(JsonElement values)=>values.EnumerateArray().Select(value=>new Vector3(value[0].GetSingle(),value[1].GetSingle(),value[2].GetSingle())).ToArray();
    static Vector2[] ReadVector2Array(JsonElement values)=>values.EnumerateArray().Select(value=>new Vector2(value[0].GetSingle(),value[1].GetSingle())).ToArray();
    static void ApplyRecoveredMaterialProperties(Material material,JsonElement saved) {
        if(saved.TryGetProperty("m_Colors",out var colors))foreach(var pair in colors.EnumerateArray()) {
            string name=pair[0].GetString();var value=pair[1];
            material.SetColor(name,new Color(value.GetProperty("r").GetSingle(),value.GetProperty("g").GetSingle(),value.GetProperty("b").GetSingle(),value.GetProperty("a").GetSingle()));
        }
        if(saved.TryGetProperty("m_Floats",out var floats))foreach(var pair in floats.EnumerateArray())
            material.SetFloat(pair[0].GetString(),pair[1].GetSingle());
    }
    static Object CreateAsset(int classID,JsonElement fields,RuntimeWorld world) => classID switch {
        21=>new Material(),
        43=>new Mesh(),
        48=>new Shader(),
        91=>new RuntimeAnimatorController(),
        134=>new PhysicMaterial(),
        213=>new Sprite(),
        241=>new Audio.AudioMixer(),
   243=>new AudioMixerGroup(),
        245=>new Audio.AudioMixerSnapshot(),
        _=>throw new NotSupportedException($"Unity package asset classID {classID} ({fields.GetProperty("m_Name").GetString()}) has no runtime mapping")
    };
}

/// <summary>Identity and original serialized payload retained for an imported asset object.</summary>
public static class SerializedAssetIdentity {
    public static string Guid(Object value)=>value?.OriginalAssetGuid;
    public static string FileID(Object value)=>value?.OriginalAssetFileID;
    public static string Path(Object value)=>value?.OriginalAssetPath;
}
