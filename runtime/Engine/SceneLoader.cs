using System.Collections;
using System.Reflection;
using System.Text.Json;
using UObject=UnityEngine.Object;
using UnityEngine;

namespace Crisis.Runtime;

public sealed class SceneLoadResult {
    public string scene {get;set;}
    public int serializedRecords {get;set;}
    public int gameObjects {get;set;}
    public int transforms {get;set;}
    public int boxColliders {get;set;}
    public int originalComponents {get;set;}
    public Dictionary<string,int> unsupportedComponents {get;set;}=[];
    public List<string> unresolvedReferences {get;set;}=[];
    public Dictionary<string,UObject> objects {get;}=[];
    public RuntimeWorld world {get;set;}
    public GameObject prefabRoot {get;set;}
    public object Report()=>new {scene,serializedRecords,gameObjects,transforms,boxColliders,originalComponents,unsupportedComponents,unresolvedReferences};
}
public static class SceneLoader {
    static string Ref(JsonElement value)=>value.GetProperty("fileID").ToString();
    static bool Flag(JsonElement fields,string name,bool fallback=true)=>fields.TryGetProperty(name,out var v) ? v.ValueKind==JsonValueKind.True||(v.ValueKind==JsonValueKind.False?false:v.ValueKind==JsonValueKind.String?int.Parse(v.GetString()!,System.Globalization.CultureInfo.InvariantCulture)!=0:v.GetInt32()!=0) : fallback;
    static float Number(JsonElement fields,string name,float fallback=0)=>fields.TryGetProperty(name,out var v)?float.Parse(v.ValueKind==JsonValueKind.String?v.GetString()!:v.GetRawText(),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture):fallback;
    static Vector3 Vector(JsonElement value)=>new(Number(value,"x"),Number(value,"y"),Number(value,"z"));
    static Quaternion Rotation(JsonElement value)=>new(Number(value,"x"),Number(value,"y"),Number(value,"z"),Number(value,"w",1));
    public static object VerifyImportedRay(SceneLoadResult result){
        result.world.MakeCurrent();
        var box=result.world.Objects.OfType<BoxCollider>().First(b=>b.isActiveAndEnabled&&MathF.Abs(b.size.x)>1e-5f&&MathF.Abs(b.size.y)>1e-5f&&MathF.Abs(b.size.z)>1e-5f);
        var origin=box.transform.TransformPoint(box.center-new Vector3(0,0,MathF.Abs(box.size.z)*.5f+1));
        var direction=box.transform.TransformVector(Vector3.forward).normalized;
        var ray=new Ray(origin,direction);
        if(!box.Raycast(ray,out var direct,10000))throw new InvalidOperationException("Imported original collider does not answer its entering ray");
        if(!Physics.Raycast(ray,out var nearest,10000,1<<box.gameObject.layer,QueryTriggerInteraction.Collide))throw new InvalidOperationException("Imported original scene query failed");
        if(nearest.distance>direct.distance+1e-3f)throw new InvalidOperationException("Nearest imported hit exceeded known collider");
        return new {sourceColliderFileID=result.objects.First(p=>ReferenceEquals(p.Value,box)).Key,
            sourceObject=box.name,directDistance=direct.distance,nearestObject=nearest.collider.name,nearestDistance=nearest.distance};
    }
    public static SceneLoadResult Load(string json,string scene,bool resetClock=true)=>
        LoadRecords(json,scene,new RuntimeWorld(resetClock:resetClock),false,false);
    public static SceneLoadResult LoadPrefab(string json,string prefabPath,RuntimeWorld world,bool requireComplete=true)=>
        LoadRecords(json,prefabPath,world,true,requireComplete);

    static SceneLoadResult LoadRecords(string json,string assetPath,RuntimeWorld world,bool prefab,bool requireComplete){
        ArgumentNullException.ThrowIfNull(world);
        using var document=JsonDocument.Parse(json);
        var package=document.RootElement;
        var asset=package.GetProperty("assets").GetProperty(assetPath);
        var records=asset.GetProperty("records").EnumerateArray().Select(r=>r.Clone()).ToArray();
        var result=new SceneLoadResult{scene=assetPath,serializedRecords=records.Length,world=world};
        var existingObjects=new HashSet<UObject>(world.Objects,ReferenceEqualityComparer.Instance);
        using var deferred=world.DeferActivation();
        try {
            var transformsByGO=records.Where(r=>r.GetProperty("classID").GetInt32() is 4 or 224)
                .ToDictionary(r=>Ref(r.GetProperty("fields").GetProperty("m_GameObject")));
            foreach(var record in records.Where(r=>r.GetProperty("classID").GetInt32()==1)){
                var id=record.GetProperty("fileID").GetString();var f=record.GetProperty("fields");
                if(!transformsByGO.TryGetValue(id,out var tr))throw new NotSupportedException($"GameObject {id} requires prefab/stripped Transform resolution before execution");
                var go=new GameObject(f.GetProperty("m_Name").GetString(),world,tr.GetProperty("classID").GetInt32()==224);
                go.layer=f.GetProperty("m_Layer").GetInt32();go.tag=f.GetProperty("m_TagString").GetString();
                go.isStatic=f.TryGetProperty("m_StaticEditorFlags",out var flags)&&flags.GetUInt32()!=0;
                go.SetActive(Flag(f,"m_IsActive"));result.objects[id]=go;
                result.objects[tr.GetProperty("fileID").GetString()]=go.transform;result.gameObjects++;result.transforms++;
            }
            foreach(var record in transformsByGO.Values){
                var t=(Transform)result.objects[record.GetProperty("fileID").GetString()];var f=record.GetProperty("fields");
                t.localPosition=Vector(f.GetProperty("m_LocalPosition"));t.localScale=Vector(f.GetProperty("m_LocalScale"));t.localRotation=Rotation(f.GetProperty("m_LocalRotation"));
                var parent=Ref(f.GetProperty("m_Father"));
                if(parent!="0"){
                    if(!result.objects.TryGetValue(parent,out var p)||p is not Transform parentTransform)throw new InvalidOperationException($"Unresolved Transform parent {parent}");
                    t.SetParent(parentTransform,false);
                }
            }
            // Unity's serialized child order is gameplay-significant.
            foreach(var record in transformsByGO.Values){
                var t=(Transform)result.objects[record.GetProperty("fileID").GetString()];var f=record.GetProperty("fields");
                if(f.TryGetProperty("m_Children",out var children)&&children.ValueKind==JsonValueKind.Array){
                    var ordered=children.EnumerateArray().Select(Ref).Select(id=>result.objects.TryGetValue(id,out var obj)?obj as Transform:null).ToArray();
                    if(ordered.Any(c=>c==null)||ordered.Any(c=>c.parent!=t)||ordered.Length!=t.childCount)throw new InvalidOperationException($"Inconsistent children on Transform {record.GetProperty("fileID")}");
                    t.children.Clear();t.children.AddRange(ordered);
                }
            }
            var scriptFields=new List<(Component component,JsonElement fields,string id)>();
            var builtinFields=new List<(Component component,JsonElement fields,string id,int classID)>();
            foreach(var record in records){
                int cls=record.GetProperty("classID").GetInt32();if(cls is 1 or 4 or 224 or 1001)continue;
                var fields=record.GetProperty("fields");var id=record.GetProperty("fileID").GetString();string label=record.GetProperty("className").GetString();
                if(!fields.TryGetProperty("m_GameObject",out var goRef))continue;
                if(!result.objects.TryGetValue(Ref(goRef),out var goObj)||goObj is not GameObject go)throw new InvalidOperationException($"Component {id} has unresolved GameObject");
                Component component=null;
                switch(cls){
                    case 23 when prefab:component=go.AddComponent<MeshRenderer>();break;
                    case 33 when prefab:component=go.AddComponent<MeshFilter>();break;
                    case 54 when prefab:component=go.AddComponent<Rigidbody>();break;
                    case 59 when prefab:component=go.AddComponent<HingeJoint>();break;
                    case 61 when prefab:component=go.AddComponent<BoxCollider2D>();break;
                    case 65:{var box=go.AddComponent<BoxCollider>();box.center=Vector(fields.GetProperty("m_Center"));box.size=Vector(fields.GetProperty("m_Size"));box.isTrigger=Flag(fields,"m_IsTrigger",false);component=box;result.boxColliders++;break;}
                    case 82 when prefab:component=go.AddComponent<AudioSource>();break;
                    case 95 when prefab:component=go.AddComponent<Animator>();break;
                    case 108:{var light=go.AddComponent<Light>();light.intensity=Number(fields,"m_Intensity",1);component=light;break;}
                    case 136 when prefab:component=go.AddComponent<CapsuleCollider>();break;
                    case 114:{
                        var script=fields.GetProperty("m_Script");
                        string guid=script.GetProperty("guid").GetString();
                        string fileID=script.GetProperty("fileID").ToString();
                        label=ResolveScriptName(package,guid,fileID);
                        var type=ResolveScriptType(package,guid,fileID);
                        if(type!=null){component=go.AddComponent(type);scriptFields.Add((component,fields,id));result.originalComponents++;}
                        break;
                    }
                }
                if(component!=null){
                    result.objects[id]=component;
                    if(component is Behaviour b)b.enabled=Flag(fields,"m_Enabled");
                    if(prefab&&cls!=114)builtinFields.Add((component,fields,id,cls));
                } else result.unsupportedComponents[label]=result.unsupportedComponents.GetValueOrDefault(label)+1;
            }
            foreach(var (component,fields,id,classID) in builtinFields){
                try{AssignBuiltinFields(component,fields,result,id,classID);}
                catch(Exception ex){throw new InvalidOperationException($"Prefab {assetPath}: built-in {component.GetType().Name} fileID {id} classID {classID}, fields [{string.Join(",",fields.EnumerateObject().Select(p=>p.Name))}]",ex);}
            }
            foreach(var (component,fields,id) in scriptFields){
                try{AssignSerializedFields(component,fields,result,id);}
                catch(Exception ex){throw new InvalidOperationException($"Prefab {assetPath}: script {component.GetType().Name} fileID {id}, fields [{string.Join(",",fields.EnumerateObject().Select(p=>p.Name))}]",ex);}
            }
            var roots=records.Where(r=>r.GetProperty("classID").GetInt32() is 4 or 224)
                .Select(r=>(Transform)result.objects[r.GetProperty("fileID").GetString()])
                .Where(t=>t.parent==null).ToArray();
            if(prefab&&roots.Length!=1)throw new InvalidOperationException($"Expected one root Transform in prefab {assetPath}, found {roots.Length}");
            if(prefab){
                result.prefabRoot=roots[0].gameObject;
                foreach(var go in world.Objects.OfType<GameObject>().Where(g=>ReferenceEquals(g.world,world)&&g!=null&&IsWithin(g.transform,roots[0])))go.assetObject=true;
            }
            if(!prefab&&package.GetProperty("assets").TryGetProperty("ProjectSettings/TimeManager.asset",out var timeAsset)){
                var settings=timeAsset.GetProperty("records")[0].GetProperty("fields");
                Time.fixedDeltaTime=Number(settings,"Fixed Timestep",.02f);Time.maximumDeltaTime=Number(settings,"Maximum Allowed Timestep",.33333334f);Time.timeScale=Number(settings,"m_TimeScale",1);
            }
            if(requireComplete&&(result.unsupportedComponents.Count>0||result.unresolvedReferences.Count>0))
                throw new NotSupportedException($"Incomplete original prefab {assetPath}: unsupported [{string.Join(", ",result.unsupportedComponents.Keys)}], unresolved [{string.Join(", ",result.unresolvedReferences.Take(8))}]");
            return result;
        }catch{
            foreach(var root in world.Objects.OfType<GameObject>().Where(g=>g&&!existingObjects.Contains(g)&&ReferenceEquals(g.world,world)&&g.transform.parent==null).ToArray())world.DestroyNow(root);
            throw;
        }
    }
    static bool IsWithin(Transform value,Transform root){for(var t=value;t!=null;t=t.parent)if(ReferenceEquals(t,root))return true;return false;}
    static string ResolveScriptName(JsonElement package,string guid,string fileID){
        if(package.TryGetProperty("scriptTypes",out var bindings)&&bindings.TryGetProperty(guid,out var types)&&types.TryGetProperty(fileID,out var mapped))return mapped.GetString();
        if(package.GetProperty("guidPaths").TryGetProperty(guid,out var source))return Path.GetFileNameWithoutExtension(source.GetString());
        return $"Script:{guid}:{fileID}";
    }
    static Type ResolveScriptType(JsonElement package,string guid,string fileID){
        string name=ResolveScriptName(package,guid,fileID);
        if(name.StartsWith("Script:",StringComparison.Ordinal))return null;
        var candidates=AppDomain.CurrentDomain.GetAssemblies().SelectMany(SafeTypes)
            .Where(t=>!t.IsAbstract&&typeof(Component).IsAssignableFrom(t)&&t.Name==name).ToArray();
        var fullSource=candidates.Where(t=>t.Namespace=="Crisis.Runtime.FullSource").ToArray();
        if(fullSource.Length==1)return fullSource[0];
        if(candidates.Length==1)return candidates[0];
        if(candidates.Length>1)throw new AmbiguousMatchException($"Original script {name} resolves to multiple runtime component types");
        return null;
    }
    static IEnumerable<Type> SafeTypes(Assembly assembly){try{return assembly.GetTypes();}catch(ReflectionTypeLoadException e){return e.Types.Where(t=>t!=null);}}
    static IEnumerable<FieldInfo> SerializedFields(Type type){
        for(var t=type;t!=null&&t!=typeof(UObject)&&t!=typeof(Component)&&t!=typeof(Behaviour)&&t!=typeof(MonoBehaviour);t=t.BaseType)
            foreach(var field in t.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly))
                if(!field.IsStatic&&!field.IsInitOnly&&field.GetCustomAttribute<NonSerializedAttribute>()==null&&(field.IsPublic||field.GetCustomAttribute<SerializeField>()!=null))yield return field;
    }
    static void AssignBuiltinFields(Component component,JsonElement fields,SceneLoadResult result,string id,int classID){
        var consumed=new HashSet<string>(StringComparer.Ordinal){"serializedVersion","m_ObjectHideFlags","m_PrefabParentObject","m_PrefabInternal","m_GameObject","m_Enabled"};
        void SetField(string source,string target,Type type){
            if(!fields.TryGetProperty(source,out var value))return;
            consumed.Add(source);
            var member=(MemberInfo)component.GetType().GetField(target,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)
                ??component.GetType().GetProperty(target,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if(member is FieldInfo f)f.SetValue(component,ReadValue(value,type,result));
            else if(member is PropertyInfo p&&p.CanWrite)p.SetValue(component,ReadValue(value,type,result));
            else result.unresolvedReferences.Add($"Unsupported built-in member {component.GetType().Name}.{target}");
        }
        void External(string source){
            if(!fields.TryGetProperty(source,out var value))return;
            consumed.Add(source);
            component.OriginalSerializedFields[source]=value.Clone();
            void Add(JsonElement reference){
                if(reference.ValueKind!=JsonValueKind.Object||!reference.TryGetProperty("fileID",out var file)||file.ToString()=="0")return;
                string guid=reference.TryGetProperty("guid",out var g)?g.GetString():"local";
                result.unresolvedReferences.Add($"Unresolved external asset {component.GetType().Name}.{source} {guid}:{file}");
            }
            if(value.ValueKind==JsonValueKind.Object)Add(value);
            else if(value.ValueKind==JsonValueKind.Array)foreach(var reference in value.EnumerateArray())Add(reference);
        }
        switch(classID){
            case 23:External("m_Materials");External("m_ProbeAnchor");External("m_LightProbeVolumeOverride");External("m_LightmapParameters");break;
            case 33:External("m_Mesh");break;
            case 54:
                SetField("m_Mass","mass",typeof(float));SetField("m_Drag","drag",typeof(float));SetField("m_AngularDrag","angularDrag",typeof(float));
                SetField("m_UseGravity","useGravity",typeof(bool));SetField("m_IsKinematic","isKinematic",typeof(bool));SetField("m_Interpolate","interpolation",typeof(RigidbodyInterpolation));
                SetField("m_Constraints","constraints",typeof(RigidbodyConstraints));SetField("m_CollisionDetection","collisionDetectionMode",typeof(CollisionDetectionMode));break;
            case 59:
                SetField("m_ConnectedBody","connectedBody",typeof(Rigidbody));SetField("m_Anchor","anchor",typeof(Vector3));SetField("m_Axis","axis",typeof(Vector3));SetField("m_ConnectedAnchor","connectedAnchor",typeof(Vector3));
                SetField("m_AutoConfigureConnectedAnchor","autoConfigureConnectedAnchor",typeof(bool));SetField("m_BreakForce","breakForce",typeof(float));SetField("m_BreakTorque","breakTorque",typeof(float));
                SetField("m_EnableCollision","enableCollision",typeof(bool));SetField("m_EnablePreprocessing","enablePreprocessing",typeof(bool));
                SetField("m_UseSpring","useSpring",typeof(bool));SetField("m_Spring","spring",typeof(JointSpring));SetField("m_UseLimits","useLimits",typeof(bool));SetField("m_Limits","limits",typeof(JointLimits));
                SetField("m_UseMotor","useMotor",typeof(bool));SetField("m_Motor","motor",typeof(JointMotor));
                SetField("m_MassScale","massScale",typeof(float));SetField("m_ConnectedMassScale","connectedMassScale",typeof(float));break;
            case 61:
                SetField("m_Density","density",typeof(float));SetField("m_Offset","offset",typeof(Vector2));SetField("m_Size","size",typeof(Vector2));SetField("m_IsTrigger","isTrigger",typeof(bool));SetField("m_UsedByEffector","usedByEffector",typeof(bool));SetField("m_UsedByComposite","usedByComposite",typeof(bool));SetField("m_AutoTiling","autoTiling",typeof(bool));SetField("m_EdgeRadius","edgeRadius",typeof(float));External("m_Material");break;
            case 82:
                SetField("m_audioClip","clip",typeof(AudioClip));SetField("m_Volume","volume",typeof(float));SetField("m_Pitch","pitch",typeof(float));
                SetField("Loop","loop",typeof(bool));SetField("Mute","mute",typeof(bool));SetField("Priority","priority",typeof(int));External("OutputAudioMixerGroup");break;
            case 95:
                SetField("m_CullingMode","cullingMode",typeof(AnimatorCullingMode));SetField("m_UpdateMode","updateMode",typeof(AnimatorUpdateMode));
                SetField("m_ApplyRootMotion","applyRootMotion",typeof(bool));SetField("m_LinearVelocityBlending","linearVelocityBlending",typeof(bool));
                SetField("m_WarningMessage","warningMessage",typeof(string));SetField("m_HasTransformHierarchy","hasTransformHierarchy",typeof(bool));
                SetField("m_AllowConstantClipSamplingOptimization","allowConstantClipSamplingOptimization",typeof(bool));
                External("m_Avatar");External("m_Controller");break;
            case 108:SetField("m_Intensity","intensity",typeof(float));break;
            case 65:SetField("m_Center","center",typeof(Vector3));SetField("m_Size","size",typeof(Vector3));SetField("m_IsTrigger","isTrigger",typeof(bool));External("m_Material");break;
            case 136:
                SetField("m_Center","center",typeof(Vector3));SetField("m_Radius","radius",typeof(float));SetField("m_Height","height",typeof(float));SetField("m_Direction","direction",typeof(int));
                SetField("m_IsTrigger","isTrigger",typeof(bool));External("m_Material");break;
        }
        foreach(var property in fields.EnumerateObject())
            if(!consumed.Contains(property.Name)&&!property.Name.StartsWith("m_Prefab",StringComparison.Ordinal))
            {
                component.OriginalSerializedFields[property.Name]=property.Value.Clone();
                result.unresolvedReferences.Add($"Unsupported built-in field {component.GetType().Name}.{property.Name}");
            }
    }
    static void AssignSerializedFields(Component component,JsonElement fields,SceneLoadResult result,string id){
        foreach(var property in fields.EnumerateObject()){
            if(property.Name.StartsWith("m_",StringComparison.Ordinal))continue;
            component.OriginalSerializedFields[property.Name]=property.Value.Clone();
            var field=SerializedFields(component.GetType()).FirstOrDefault(f=>f.Name==property.Name);
            if(field==null){result.unresolvedReferences.Add($"Unsupported serialized field {component.GetType().Name}.{property.Name}");continue;}
            try{field.SetValue(component,ReadValue(property.Value,field.FieldType,result));}
            catch(Exception error){throw new NotSupportedException($"Cannot load original field {id}:{property.Name} ({field.FieldType.FullName})",error);}
        }
    }
    static object ReadValue(JsonElement value,Type type,SceneLoadResult result){
        if(value.ValueKind==JsonValueKind.Null)return null;
        var nullable=Nullable.GetUnderlyingType(type);if(nullable!=null)return ReadValue(value,nullable,result);
        if(typeof(UObject).IsAssignableFrom(type)){
            if(value.ValueKind!=JsonValueKind.Object||!value.TryGetProperty("fileID",out var fileId))throw new NotSupportedException($"External {type.Name} asset reference is not mapped");
            var key=fileId.ToString();if(key=="0")return null;
            if(result.objects.TryGetValue(key,out var found)&&type.IsInstanceOfType(found))return found;
            string guid=value.TryGetProperty("guid",out var externalGuid)?externalGuid.GetString():"local";
            result.unresolvedReferences.Add($"Unresolved {type.Name} reference {guid}:{key}");return null;
        }
        if(type.IsEnum)return value.ValueKind==JsonValueKind.String?Enum.Parse(type,value.GetString(),true):Enum.ToObject(type,value.GetInt32());
        if(type==typeof(string))return value.GetString();
        if(type==typeof(bool))return value.ValueKind==JsonValueKind.True||(value.ValueKind==JsonValueKind.Number&&value.GetInt32()!=0);
        if(type==typeof(float))return float.Parse(value.ValueKind==JsonValueKind.String?value.GetString()!:value.GetRawText(),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture);
        if(type==typeof(double))return value.GetDouble();
        if(type==typeof(int))return value.ValueKind==JsonValueKind.String?int.Parse(value.GetString()!,System.Globalization.CultureInfo.InvariantCulture):value.GetInt32();
        if(type==typeof(long))return value.GetInt64();
        if(type==typeof(uint))return value.GetUInt32();
        if(type==typeof(byte))return value.ValueKind==JsonValueKind.String?byte.Parse(value.GetString()!,System.Globalization.CultureInfo.InvariantCulture):value.GetByte();
        if(type==typeof(byte[])&&value.ValueKind==JsonValueKind.String)return Convert.FromHexString(value.GetString()!);if(type.IsArray){if(value.ValueKind!=JsonValueKind.Array)throw new NotSupportedException($"Cannot load serialized array {type.FullName} from {value.GetRawText()}");var items=value.EnumerateArray().Select(v=>ReadValue(v,type.GetElementType(),result)).ToArray();var array=Array.CreateInstance(type.GetElementType(),items.Length);for(int i=0;i<items.Length;i++)array.SetValue(items[i],i);return array;}
        if(type.IsGenericType&&typeof(IList).IsAssignableFrom(type)){if(type.GetGenericArguments()[0]==typeof(byte)&&value.ValueKind==JsonValueKind.String){var raw=Convert.FromHexString(value.GetString()!);var decoded=(IList)Activator.CreateInstance(type)!;foreach(var b in raw)decoded.Add(b);return decoded;}if(type.GetGenericArguments()[0]==typeof(int)&&value.ValueKind==JsonValueKind.String){var raw=Convert.FromHexString(value.GetString()!);if(raw.Length%4!=0)throw new FormatException($"Invalid serialized int list length: {raw.Length}");var decoded=(IList)Activator.CreateInstance(type)!;for(var offset=0;offset<raw.Length;offset+=4)decoded.Add(System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(raw.AsSpan(offset,4)));return decoded;}if(value.ValueKind!=JsonValueKind.Array)throw new NotSupportedException($"Cannot load serialized list {type.FullName} from {value.GetRawText()}");
            var list=(IList)Activator.CreateInstance(type);var element=type.GetGenericArguments()[0];foreach(var item in value.EnumerateArray())list.Add(ReadValue(item,element,result));return list;
        }
        if(value.ValueKind!=JsonValueKind.Object)throw new NotSupportedException($"Expected serialized object for {type.FullName}");
        object instance=type==typeof(AnimationCurve)?System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(type):Activator.CreateInstance(type,true)!;
        foreach(var property in value.EnumerateObject()){
            var member=type.GetField(property.Name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic) as MemberInfo
                ??type.GetProperty(property.Name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if(member is FieldInfo f&&!f.IsInitOnly)try{f.SetValue(instance,ReadValue(property.Value,f.FieldType,result));}catch(Exception ex){throw new InvalidOperationException($"Failed serialized member {type.FullName}.{f.Name} ({f.FieldType.FullName}) from {property.Value.GetRawText()}",ex);}
            else if(member is PropertyInfo p&&p.CanWrite)try{p.SetValue(instance,ReadValue(property.Value,p.PropertyType,result));}catch(Exception ex){throw new InvalidOperationException($"Failed serialized member {type.FullName}.{p.Name} ({p.PropertyType.FullName}) from {property.Value.GetRawText()}",ex);}
        }
        return instance;
}
