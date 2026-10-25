using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Crisis.Runtime;
using UnityEngine;

public static class OriginalSourcePackageContracts {
    public static string Run() {
        const string assetPath="Assets/Resources/Global context.prefab";
        const string sourceAsset="{\"records\":[{\"classID\":1,\"fileID\":\"1\",\"className\":\"GameObject\",\"fields\":{\"m_Name\":\"Verified original root\",\"m_Layer\":0,\"m_TagString\":\"Untagged\",\"m_StaticEditorFlags\":0,\"m_IsActive\":1}},{\"classID\":4,\"fileID\":\"4\",\"className\":\"Transform\",\"fields\":{\"m_GameObject\":{\"fileID\":1},\"m_LocalPosition\":{\"x\":2,\"y\":3,\"z\":4},\"m_LocalScale\":{\"x\":1,\"y\":1,\"z\":1},\"m_LocalRotation\":{\"x\":0,\"y\":0,\"z\":0,\"w\":1},\"m_Father\":{\"fileID\":0},\"m_Children\":[]}}],\"sourceSha256\":\"original-asset-source-hash\"}";
        var bytes=Encoding.UTF8.GetBytes(sourceAsset);
        var hash=Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        string metadata=JsonSerializer.Serialize(new {
            schemaVersion=1,
            guidPaths=new Dictionary<string,string>{{"source-guid",assetPath}},
            assetManifest=new Dictionary<string,object>{[assetPath]=new{bytes=bytes.Length,sha256=hash}}
        });
        var package=OriginalSourcePackage.CreateIncremental(metadata);
        if(package.ExpectedAssetCount!=1||package.AssetCount!=0||!package.IsIncremental)
            throw new InvalidOperationException("Incremental source package did not initialize empty against its manifest");
        if(!package.AddVerifiedAsset(assetPath,sourceAsset)||package.AssetCount!=1)
            throw new InvalidOperationException("Verified source asset was not added");
        if(package.AddVerifiedAsset(assetPath,sourceAsset)||package.AssetCount!=1)
            throw new InvalidOperationException("Identical source asset retry was not idempotent");
        var record=package.GetAsset(assetPath).GetProperty("records")[0];
        if(record.GetProperty("fileID").GetString()!="1"||record.GetProperty("fields").GetProperty("m_Name").GetString()!="Verified original root")
            throw new InvalidOperationException("Incremental source asset fields or fileID changed");
        if(!package.TryGetProperty("guidPaths",out var paths)||paths.GetProperty("source-guid").GetString()!=assetPath)
            throw new InvalidOperationException("Package metadata is unavailable to the source resolver");
        var world=new RuntimeWorld();
        var loaded=SceneLoader.Load(package,assetPath,world);
        var root=world.Objects.OfType<GameObject>().Single(go=>go.name=="Verified original root");
        if(!ReferenceEquals(loaded.world,world)||root.transform.localPosition.x!=2||root.transform.localPosition.y!=3||root.transform.localPosition.z!=4)
            throw new InvalidOperationException("SceneLoader did not synchronously consume the verified incremental source asset in its target world");
        try { package.AddVerifiedAsset(assetPath,sourceAsset.Replace("Verified original root","Changed root",StringComparison.Ordinal)); throw new InvalidOperationException("Conflicting source payload unexpectedly passed verification"); }
        catch(InvalidDataException) { }
        try { package.GetAsset("Assets/missing.prefab"); throw new InvalidOperationException("Missing source asset unexpectedly resolved"); }
        catch(KeyNotFoundException) { }
        try { package.AddVerifiedAsset("Assets/missing.prefab",sourceAsset); throw new InvalidOperationException("Unmanifested source asset unexpectedly passed verification"); }
        catch(KeyNotFoundException) { }
        using var legacyDocument=JsonDocument.Parse(sourceAsset);
        var legacy=OriginalSourcePackage.FromJson(JsonSerializer.Serialize(new{assets=new Dictionary<string,JsonElement>{{assetPath,legacyDocument.RootElement}}}));
        if(legacy.AssetCount!=1||legacy.GetAsset(assetPath).GetProperty("records").GetArrayLength()!=2)
            throw new InvalidOperationException("Legacy monolithic package compatibility failed");
        return JsonSerializer.Serialize(new{status="PASS",verifiedAssetBytes=bytes.Length,verifiedAssetSha256=hash,metadataLookup=true,duplicateRetry="idempotent",sceneLoad="same RuntimeWorld",badHash="rejected",missingAsset="fail-closed",legacyPackage="compatible"});
    }
}
