using System.Text.Json;
using Crisis.Runtime;

namespace UnityEngine;

public static class Resources {
    static OriginalSourcePackage package;
    static readonly Dictionary<string,string> paths=new(StringComparer.OrdinalIgnoreCase);
    static readonly Dictionary<string,Object> cache=new(StringComparer.OrdinalIgnoreCase);
    static RuntimeWorld assetWorld;
    public static void Configure(string executionPackage,RuntimeWorld world=null){
        Configure(OriginalSourcePackage.FromJson(executionPackage),world);
    }
    public static void Configure(OriginalSourcePackage sourcePackage,RuntimeWorld world=null){
        ArgumentNullException.ThrowIfNull(sourcePackage);paths.Clear();cache.Clear();package=sourcePackage;
        foreach(var path in sourcePackage.AssetPaths){
            const string prefix="Assets/Resources/";if(!path.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))continue;
            var key=Path.ChangeExtension(path[prefix.Length..],null).Replace('\\','/');
            if(!paths.TryAdd(key,path))throw new InvalidOperationException($"Ambiguous resource path {key}");
        }
        assetWorld=world??new RuntimeWorld(makeCurrent:false,resetClock:false);
    }
    public static Object Load(string path){
        if(package==null)throw new InvalidOperationException("Original resource package is not configured");
        path=path.Replace('\\','/');if(cache.TryGetValue(path,out var cached)&&cached)return cached;
        if(!paths.TryGetValue(path,out var assetPath))return null;
        if(assetPath.StartsWith("Assets/Resources/characters/",StringComparison.OrdinalIgnoreCase)&&assetPath.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase)){
            var loaded=SceneLoader.LoadPrefab(package,assetPath,assetWorld,requireComplete:true);
            cache[path]=loaded.prefabRoot;return loaded.prefabRoot;
        }
        if(assetPath.StartsWith("Assets/Resources/items/",StringComparison.OrdinalIgnoreCase)){
            var loaded=SceneLoader.LoadPrefab(package,assetPath,assetWorld,requireComplete:true);
            cache[path]=loaded.prefabRoot;return loaded.prefabRoot;
        }
        if(assetPath.StartsWith("Assets/Resources/weapons/ammunition/",StringComparison.OrdinalIgnoreCase)){
            // Ammunition is an authored MonoBehaviour prefab. Load its real
            // component graph through the same source-script and serialized
            // reference path as the other original prefabs. The former
            // field-only loader attached the transport/global AmmunitionBase,
            // so FullSource.AmmunitionBase.GetAmmunition returned null and
            // original WeaponBase.InitializeAmmunition left the weapon empty.
            var loaded=SceneLoader.LoadPrefab(package,assetPath,assetWorld,requireComplete:true);
            loaded.prefabRoot.assetObject=true;
            cache[path]=loaded.prefabRoot;return loaded.prefabRoot;
        }
        if(assetPath.StartsWith("Assets/Resources/weapons/",StringComparison.OrdinalIgnoreCase)){
            var loaded=SceneLoader.LoadPrefab(package,assetPath,assetWorld,requireComplete:true);
            loaded.prefabRoot.assetObject=true;
            cache[path]=loaded.prefabRoot;return loaded.prefabRoot;
        }
        if(assetPath.StartsWith("Assets/Resources/",StringComparison.OrdinalIgnoreCase)&&assetPath.EndsWith(".prefab",StringComparison.OrdinalIgnoreCase)){
            var loaded=SceneLoader.LoadPrefab(package,assetPath,assetWorld,requireComplete:true);
            loaded.prefabRoot.assetObject=true;
            cache[path]=loaded.prefabRoot;return loaded.prefabRoot;
        }
        throw new NotSupportedException($"Resource dependency closure for {assetPath} is not executable yet");
    }
    public static Object Load(string path,Type type){var asset=Load(path);return asset!=null&&type.IsInstanceOfType(asset)?asset:null;}
    public static T Load<T>(string path) where T:Object=>Load(path) as T;
}
