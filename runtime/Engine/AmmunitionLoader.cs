using System.Reflection;
using System.Text.Json;
using UnityEngine;

namespace Crisis.Runtime;

public static class AmmunitionLoader {
    public static AmmunitionBase Load(string packageText,string assetPath,RuntimeWorld world){
        using var document=JsonDocument.Parse(packageText);var package=document.RootElement;
        var records=package.GetProperty("assets").GetProperty(assetPath).GetProperty("records").EnumerateArray();
        JsonElement fields=default;
        foreach(var record in records){if(record.GetProperty("classID").GetInt32()!=114)continue;
            var f=record.GetProperty("fields");var guid=f.GetProperty("m_Script").GetProperty("guid").GetString();
            if(package.GetProperty("guidPaths").TryGetProperty(guid,out var p)&&p.GetString().EndsWith("/AmmunitionBase.cs")){fields=f.Clone();break;}}
        if(fields.ValueKind==JsonValueKind.Undefined)throw new InvalidOperationException("Original ammunition component not found");
        var ammo=new GameObject(assetPath,world).AddComponent<AmmunitionBase>();
        foreach(var field in typeof(AmmunitionBase).GetFields(BindingFlags.Instance|BindingFlags.Public)){
            if(!fields.TryGetProperty(field.Name,out var value))throw new InvalidOperationException($"Original ammunition field missing: {field.Name}");
            if(field.FieldType==typeof(float))field.SetValue(ammo,value.GetSingle());
            else if(field.FieldType==typeof(int))field.SetValue(ammo,value.GetInt32());
            else if(field.FieldType==typeof(bool))field.SetValue(ammo,value.ValueKind==JsonValueKind.True||(value.ValueKind==JsonValueKind.Number&&value.GetInt32()!=0));
            else if(field.FieldType==typeof(string))field.SetValue(ammo,value.ValueKind==JsonValueKind.Null?null:value.GetString());
            else if(field.FieldType.IsEnum)field.SetValue(ammo,Enum.ToObject(field.FieldType,value.GetInt32()));
            else if(field.FieldType==typeof(AnimationCurve)){
                var keys=value.GetProperty("m_Curve").EnumerateArray().Select(k=>{
                    if(k.TryGetProperty("weightedMode",out var mode)&&mode.GetInt32()!=0)throw new NotSupportedException("Weighted original animation curve");
                    return new Keyframe(k.GetProperty("time").GetSingle(),k.GetProperty("value").GetSingle(),k.GetProperty("inSlope").GetSingle(),k.GetProperty("outSlope").GetSingle());
                }).ToArray();
                field.SetValue(ammo,new AnimationCurve(keys){serializedPreInfinity=value.GetProperty("m_PreInfinity").GetInt32(),serializedPostInfinity=value.GetProperty("m_PostInfinity").GetInt32()});
            }else throw new NotSupportedException(field.FieldType.Name);
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
}
