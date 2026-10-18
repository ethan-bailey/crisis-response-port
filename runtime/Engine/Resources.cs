using System.Text.Json;
using Crisis.Runtime;

namespace UnityEngine;

public static class Resources {
    static string packageText;
    static readonly Dictionary<string,string> paths=new(StringComparer.OrdinalIgnoreCase);
    static readonly Dictionary<string,Object> cache=new(StringComparer.OrdinalIgnoreCase);
    static RuntimeWorld assetWorld;
    public static void Configure(string executionPackage){
        using var document=JsonDocument.Parse(executionPackage);paths.Clear();cache.Clear();packageText=executionPackage;
        foreach(var property in document.RootElement.GetProperty("assets").EnumerateObject()){
            const string prefix="Assets/Resources/";var path=property.Name;if(!path.StartsWith(prefix,StringComparison.Ordinal))continue;
            var key=Path.ChangeExtension(path[prefix.Length..],null).Replace('\\','/');
            if(!paths.TryAdd(key,path))throw new InvalidOperationException($"Ambiguous resource path {key}");
        }
        assetWorld=new RuntimeWorld(makeCurrent:false,resetClock:false);
    }
    public static Object Load(string path){
        if(packageText==null)throw new InvalidOperationException("Original resource package is not configured");
        path=path.Replace('\\','/');if(cache.TryGetValue(path,out var cached)&&cached)return cached;
        if(!paths.TryGetValue(path,out var assetPath))return null;
        if(!assetPath.StartsWith("Assets/Resources/weapons/ammunition/",StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException($"Resource dependency closure for {assetPath} is not executable yet");
        var ammo=AmmunitionLoader.Load(packageText,assetPath,assetWorld);ammo.gameObject.assetObject=true;
        cache[path]=ammo.gameObject;return ammo.gameObject;
    }
    public static Object Load(string path,Type type){var asset=Load(path);return asset!=null&&type.IsInstanceOfType(asset)?asset:null;}
    public static T Load<T>(string path) where T:Object=>Load(path) as T;
}

