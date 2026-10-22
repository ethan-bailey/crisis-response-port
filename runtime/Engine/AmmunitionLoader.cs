using System.Reflection;
using System.Text.Json;
using UnityEngine;

namespace Crisis.Runtime;

public static class AmmunitionLoader {
    public static UnityEngine.Object ResolveOriginalCalibreReference(JsonElement package,string guid,string fileID,RuntimeWorld world) => SerializedAssetResolver.Resolve(package,guid,fileID,world,typeof(Crisis.Runtime.FullSource.Calibre));
    public static AmmunitionBase Load(string packageText,string assetPath,RuntimeWorld world){
        using var document=JsonDocument.Parse(packageText);var package=document.RootElement;
        var records=package.GetProperty("assets").GetProperty(assetPath).GetProperty("records").EnumerateArray();
        JsonElement fields=default;
        foreach(var record in records){if(record.GetProperty("classID").GetInt32()!=114)continue;
            var f=record.GetProperty("fields");var guid=f.GetProperty("m_Script").GetProperty("guid").GetString();
            if(package.GetProperty("guidPaths").TryGetProperty(guid,out var p)&&p.GetString().EndsWith("/AmmunitionBase.cs")){fields=f.Clone();break;}}
        if(fields.ValueKind==JsonValueKind.Undefined)throw new InvalidOperationException("Original ammunition component not found");
        var originalRecords=package.GetProperty("assets").GetProperty(assetPath).GetProperty("records").EnumerateArray().ToArray();
        var rootRecord=originalRecords.Single(r=>r.GetProperty("classID").GetInt32()==1).GetProperty("fields");
        var go=new GameObject(rootRecord.GetProperty("m_Name").GetString(),world){layer=rootRecord.GetProperty("m_Layer").GetInt32(),tag=rootRecord.GetProperty("m_TagString").GetString()};
        go.SetActive(rootRecord.GetProperty("m_IsActive").GetInt32()!=0);
        var transform=originalRecords.Single(r=>r.GetProperty("classID").GetInt32()==4).GetProperty("fields");
        Vector3 Vector(JsonElement v)=>new(v.GetProperty("x").GetSingle(),v.GetProperty("y").GetSingle(),v.GetProperty("z").GetSingle());
        go.transform.localPosition=Vector(transform.GetProperty("m_LocalPosition"));go.transform.localScale=Vector(transform.GetProperty("m_LocalScale"));
        var q=transform.GetProperty("m_LocalRotation");go.transform.localRotation=new(q.GetProperty("x").GetSingle(),q.GetProperty("y").GetSingle(),q.GetProperty("z").GetSingle(),q.GetProperty("w").GetSingle());
        var ammo=go.AddComponent<AmmunitionBase>();
        foreach(var field in typeof(AmmunitionBase).GetFields(BindingFlags.Instance|BindingFlags.Public)){
            if(!fields.TryGetProperty(field.Name,out var value))throw new InvalidOperationException($"Original ammunition field missing: {field.Name}");
            if(field.FieldType==typeof(float))field.SetValue(ammo,value.GetSingle());
            else if(field.FieldType==typeof(int))field.SetValue(ammo,value.GetInt32());
            else if(field.FieldType==typeof(bool))field.SetValue(ammo,value.ValueKind==JsonValueKind.True||(value.ValueKind==JsonValueKind.Number&&value.GetInt32()!=0));
            else if(field.FieldType==typeof(string))field.SetValue(ammo,value.ValueKind==JsonValueKind.Null?null:value.GetString());
            else if(field.FieldType.IsEnum)field.SetValue(ammo,Enum.ToObject(field.FieldType,value.GetInt32()));
            else if(field.FieldType==typeof(List<AmmunitionBase.FragmentationSpecification>)){field.SetValue(ammo,JsonSerializer.Deserialize<List<AmmunitionBase.FragmentationSpecification>>(value.GetRawText(),new JsonSerializerOptions{IncludeFields=true}));}
            else if(field.FieldType==typeof(Utilities.Availability)){
                var availability=JsonSerializer.Deserialize<Utilities.Availability>(value.GetRawText(),new JsonSerializerOptions{IncludeFields=true});
                if(availability==null)throw new InvalidDataException($"Original ammunition field {field.Name} deserialized to null");
                field.SetValue(ammo,availability);
            }
            else if(field.FieldType==typeof(AnimationCurve)){
                var keys=value.GetProperty("m_Curve").EnumerateArray().Select(k=>{
                    if(k.TryGetProperty("weightedMode",out var mode)&&mode.GetInt32()!=0)throw new NotSupportedException("Weighted original animation curve");
                    return new Keyframe(k.GetProperty("time").GetSingle(),k.GetProperty("value").GetSingle(),k.GetProperty("inSlope").GetSingle(),k.GetProperty("outSlope").GetSingle());
                }).ToArray();
                // Explicit endpoint-clamp adapter, matching the existing weapon curve boundary.
                // Native serialized-infinity mapping still requires a Unity comparison.
                field.SetValue(ammo,new AnimationCurve(keys){clampOutside=value.GetProperty("m_PreInfinity").GetInt32()==2&&value.GetProperty("m_PostInfinity").GetInt32()==2,serializedPreInfinity=value.GetProperty("m_PreInfinity").GetInt32(),serializedPostInfinity=value.GetProperty("m_PostInfinity").GetInt32()});
            }else if(typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)&&value.ValueKind==JsonValueKind.Object&&value.TryGetProperty("fileID",out var objectFileID)){
                string objectId=objectFileID.ValueKind==JsonValueKind.String?objectFileID.GetString()!:objectFileID.ToString();
                if(objectId=="0")field.SetValue(ammo,null);
                else{
                    if(!value.TryGetProperty("guid",out var objectGuid))throw new InvalidDataException($"Original ammunition Unity object field {field.Name} has fileID {objectId} but no GUID");
                    var resolved=SerializedAssetResolver.Resolve(package,objectGuid.GetString()!,objectId,world,field.FieldType);
                    field.SetValue(ammo,resolved);
                }
            }else{
                string serializedValue=value.ValueKind==JsonValueKind.Object&&value.TryGetProperty("guid",out var unsupportedGuid)
                    ? $"{unsupportedGuid.GetString()}:{(value.TryGetProperty("fileID",out var unsupportedFileID)?unsupportedFileID.ToString():"?")}"
                    : value.GetRawText();
                throw new NotSupportedException($"Original ammunition field '{field.Name}' ({field.FieldType.FullName}) is unsupported; serialized value: {serializedValue}");
            }
        }
        return ammo;
    }
    public static object VerifyOriginalPrefab(string packageText,string assetPath){
        var world=new RuntimeWorld();var ammo=Load(packageText,assetPath,world);
        var p=new GameObject("Original projectile math gate",world).AddComponent<Projectile>();
        // Configuration fixture only: original firing/initialization remains
        // unselected. Calculations below execute verbatim recovered methods.
        p.Mass=ammo.mass;p.BaseVelocity=ammo.velocity;p.StartPosition=p.transform.position;
        typeof(Projectile).GetProperty("Ammunition").SetValue(p,ammo);
        typeof(Projectile).GetProperty("StartEnergy").SetValue(p,ammo.MuzzleEnergy);
        typeof(Projectile).GetField("diameter",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(p,ammo.diameter);
        p.meshTransform=new GameObject("Original projectile mesh pose",world).transform;
        p.RecalculateVelocity();float before=p.Energy;
        float loss=before*.025f;p.ConvertEnergyToFriction(loss);
        if(MathF.Abs(p.Energy-(before-loss))>.02f)throw new InvalidOperationException("Original energy conversion");
        float penetration=p.PotentialPenetrationDistance(1000);float reverse=p.EnergyToPenetrate(penetration,1000);
        if(!float.IsFinite(reverse)||MathF.Abs(reverse-p.Energy)>.02f)throw new InvalidOperationException("Original penetration inverse");
        var curve=ammo.expansionFunction.keys;
        if(curve.Length<1)throw new InvalidOperationException("Original expansion curve is empty");
        var sample=ammo.expansionFunction.Evaluate(curve[0].time);
        if(sample!=curve[0].value)throw new InvalidOperationException("Original curve key");
        return new{assetPath,ammo.mass,ammo.velocity,ammo.diameter,energyBefore=before,energyAfter=p.Energy,
            penetrationDistance=penetration,curveKeys=curve.Length,sourceMethodsUnchanged=true,fullProjectileLifecycle=false};
    }
    public static object VerifyOriginalResource(string packageText,string name){
        var world=new RuntimeWorld();world.Tick(.05f);float originalClock=Time.time;
        Resources.Configure(packageText);
        var fallback=new GameObject("Fallback only",world).AddComponent<AmmunitionBase>();
        var ammo=AmmunitionBase.GetAmmunition(name,fallback);
        if(ReferenceEquals(ammo,fallback))throw new InvalidOperationException("Original resource lookup returned fallback for an existing original prefab");
        var again=AmmunitionBase.GetAmmunition(name,fallback);
        if(!ReferenceEquals(ammo,again))throw new InvalidOperationException("Original Resources.Load cache identity");
        if(Time.time!=originalClock)throw new InvalidOperationException("Asset loading reset active game time");
        if(ammo.gameObject.activeInHierarchy)throw new InvalidOperationException("Resource prefab became an active scene instance");
        var instance=UnityEngine.Object.Instantiate(ammo.gameObject);var clone=instance.GetComponent<AmmunitionBase>();
        if(clone.mass!=ammo.mass||ReferenceEquals(clone,ammo)||ReferenceEquals(clone.expansionFunction,ammo.expansionFunction))throw new InvalidOperationException("Original-configured ammunition clone fields");
        clone.mass+=1;if(clone.mass==ammo.mass)throw new InvalidOperationException("Clone modified source resource");
        if(!ReferenceEquals(AmmunitionBase.GetAmmunition("missing-source-resource",fallback),fallback))throw new InvalidOperationException("Original fallback branch");
        return new{name,sourceName=ammo.name,sourceMass=ammo.mass,instanceName=instance.name,independentInstance=true,
            cachedResource=true,clockUnchanged=true,originalGetAmmunitionExecuted=true,scope="selected original ammunition fields/calculations; remaining equipment references are excluded"};
    }
}
