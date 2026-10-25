using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Crisis.Runtime;

/// <summary>
/// Synchronous view of an original Unity source package. A package can be
/// loaded from the legacy monolithic JSON format or assembled from separately
/// verified asset documents before a scene is activated.
/// </summary>
public sealed class OriginalSourcePackage {
    readonly JsonDocument metadataDocument;
    readonly JsonElement legacyRoot;
    readonly JsonElement manifest;
    readonly Dictionary<string,JsonDocument> assets=new(StringComparer.Ordinal);
    readonly Dictionary<string,string> assetHashes=new(StringComparer.Ordinal);
    long deliveredBytes;

    OriginalSourcePackage(JsonDocument metadataDocument,JsonElement manifest) {
        this.metadataDocument=metadataDocument;
        this.manifest=manifest;
    }

    OriginalSourcePackage(JsonElement legacyRoot) {
        this.legacyRoot=legacyRoot;
        manifest=default;
    }

    internal static OriginalSourcePackage FromJsonElement(JsonElement root) {
        if(root.ValueKind!=JsonValueKind.Object||!root.TryGetProperty("assets",out var assetTable)||assetTable.ValueKind!=JsonValueKind.Object)
            throw new InvalidDataException("Original source package has no assets object");
        return new OriginalSourcePackage(root);
    }

    public static OriginalSourcePackage FromJson(string json) {
        ArgumentNullException.ThrowIfNull(json);
        var document=JsonDocument.Parse(json);
        try {
            var root=document.RootElement;
            if(!root.TryGetProperty("assets",out var assetTable)||assetTable.ValueKind!=JsonValueKind.Object)
                throw new InvalidDataException("Original source package has no assets object");
            return new OriginalSourcePackage(root);
        } catch { document.Dispose(); throw; }
    }

    /// <summary>Creates a package from metadata containing assetManifest entries with bytes and sha256.</summary>
    public static OriginalSourcePackage CreateIncremental(string metadataJson) {
        ArgumentNullException.ThrowIfNull(metadataJson);
        var document=JsonDocument.Parse(metadataJson);
        try {
            var root=document.RootElement;
            if(root.ValueKind!=JsonValueKind.Object||root.TryGetProperty("assets",out _))
                throw new InvalidDataException("Incremental source metadata must be an object without an assets table");
            if(!root.TryGetProperty("schemaVersion",out var version)||!version.TryGetInt32(out var schemaVersion)||schemaVersion!=1)
                throw new InvalidDataException("Unsupported incremental source metadata schema version");
            if(!root.TryGetProperty("assetManifest",out var manifest)||manifest.ValueKind!=JsonValueKind.Object)
                throw new InvalidDataException("Incremental source metadata has no assetManifest object");
            foreach(var item in manifest.EnumerateObject()) {
                if(string.IsNullOrWhiteSpace(item.Name)||item.Value.ValueKind!=JsonValueKind.Object||
                   !item.Value.TryGetProperty("bytes",out var bytes)||!bytes.TryGetInt64(out var byteLength)||byteLength<0||
                   !item.Value.TryGetProperty("sha256",out var sha)||sha.ValueKind!=JsonValueKind.String||
                   !IsSha256(sha.GetString()))
                    throw new InvalidDataException($"Invalid incremental source manifest entry for {item.Name}");
            }
            return new OriginalSourcePackage(document,manifest);
        } catch { document.Dispose(); throw; }
    }

    public int AssetCount=>legacyRoot.ValueKind==JsonValueKind.Object?legacyRoot.GetProperty("assets").EnumerateObject().Count():assets.Count;
    public int ExpectedAssetCount=>legacyRoot.ValueKind==JsonValueKind.Object?AssetCount:manifest.EnumerateObject().Count();
    public long DeliveredBytes=>deliveredBytes;
    public IEnumerable<string> AssetPaths=>legacyRoot.ValueKind==JsonValueKind.Object?legacyRoot.GetProperty("assets").EnumerateObject().Select(item=>item.Name):manifest.EnumerateObject().Select(item=>item.Name);
    public bool IsIncremental=>metadataDocument!=null;

    public bool AddVerifiedAsset(string assetPath,string assetJson) {
        if(!IsIncremental)throw new InvalidOperationException("Assets can only be added to an incremental source package");
        ArgumentException.ThrowIfNullOrWhiteSpace(assetPath);ArgumentNullException.ThrowIfNull(assetJson);
        if(!manifest.TryGetProperty(assetPath,out var descriptor))throw new KeyNotFoundException($"Asset {assetPath} is absent from the source manifest");
        byte[] utf8=Encoding.UTF8.GetBytes(assetJson);
        long expectedBytes=descriptor.GetProperty("bytes").GetInt64();
        if(utf8.LongLength!=expectedBytes)throw new InvalidDataException($"Original source asset byte length mismatch for {assetPath}: expected {expectedBytes}, received {utf8.LongLength}");
        string expectedHash=descriptor.GetProperty("sha256").GetString();
        string actualHash=Convert.ToHexString(SHA256.HashData(utf8)).ToLowerInvariant();
        if(!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(actualHash),Encoding.ASCII.GetBytes(expectedHash)))
            throw new InvalidDataException($"Original source asset SHA-256 mismatch for {assetPath}");
        if(assets.TryGetValue(assetPath,out _)) {
            if(assetHashes[assetPath]==actualHash)return false;
            throw new InvalidDataException($"Conflicting duplicate original source asset {assetPath}");
        }
        var parsed=JsonDocument.Parse(utf8);
        if(parsed.RootElement.ValueKind!=JsonValueKind.Object||!parsed.RootElement.TryGetProperty("records",out var records)||records.ValueKind!=JsonValueKind.Array) {
            parsed.Dispose();throw new InvalidDataException($"Original source asset {assetPath} has no records array");
        }
        assets.Add(assetPath,parsed);assetHashes.Add(assetPath,actualHash);deliveredBytes=checked(deliveredBytes+utf8.LongLength);return true;
    }

    public bool ContainsAsset(string path)=>legacyRoot.ValueKind==JsonValueKind.Object?legacyRoot.GetProperty("assets").TryGetProperty(path,out _):assets.ContainsKey(path);

    public JsonElement GetAsset(string path) {
        if(legacyRoot.ValueKind==JsonValueKind.Object)return legacyRoot.GetProperty("assets").GetProperty(path);
        if(!assets.TryGetValue(path,out var document))throw new KeyNotFoundException($"Original source asset {path} has not been delivered; {assets.Count} of {ExpectedAssetCount} manifest assets are present");
        return document.RootElement;
    }

    public bool TryGetAsset(string path,out JsonElement asset) {
        if(legacyRoot.ValueKind==JsonValueKind.Object)return legacyRoot.GetProperty("assets").TryGetProperty(path,out asset);
        if(assets.TryGetValue(path,out var document)){asset=document.RootElement;return true;}
        asset=default;return false;
    }

    public bool TryGetProperty(string name,out JsonElement value) {
        if(legacyRoot.ValueKind==JsonValueKind.Object)return legacyRoot.TryGetProperty(name,out value);
        return metadataDocument.RootElement.TryGetProperty(name,out value);
    }

    public JsonElement GetProperty(string name) {
        if(name=="assets"&&legacyRoot.ValueKind!=JsonValueKind.Object)
            throw new InvalidOperationException("Incremental source packages expose assets through GetAsset/TryGetAsset");
        return legacyRoot.ValueKind==JsonValueKind.Object?legacyRoot.GetProperty(name):metadataDocument.RootElement.GetProperty(name);
    }

    public void RequireAssets(IEnumerable<string> paths) {
        var missing=paths.Where(path=>!ContainsAsset(path)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        if(missing.Length>0)throw new InvalidOperationException($"Original source package is incomplete: {missing.Length} required assets are missing; first missing asset: {missing[0]}");
    }

    static bool IsSha256(string value)=>value?.Length==64&&value.All(c=>c is >= '0' and <= '9' or >= 'a' and <= 'f');
}
