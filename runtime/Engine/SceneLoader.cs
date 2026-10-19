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
    static bool Flag(JsonElement fields,string name,bool fallback=true)=>fields.TryGetProperty(name,out var v) ? v.ValueKind==JsonValueKind.True||(v.ValueKind==JsonValueKind.Number&&v.GetInt32()!=0) : fallback;
    static float Number(JsonElement fields,string name,float fallback=0)=>fields.TryGetProperty(name,out var v)?v.GetSingle():fallback;
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
            foreach(var record in records){
                int cls=record.GetProperty("classID").GetInt32();if(cls is 1 or 4 or 224)continue;
                var fields=record.GetProperty("fields");var id=record.GetProperty("fileID").GetString();string label=record.GetProperty("className").GetString();
                if(!fields.TryGetProperty("m_GameObject",out var goRef))continue;
                if(!result.objects.TryGetValue(Ref(goRef),out var goObj)||goObj is not GameObject go)throw new InvalidOperationException($"Component {id} has unresolved GameObject");
                Component component=null;
                if(cls==65){var box=go.AddComponent<BoxCollider>();box.center=Vector(fields.GetProperty("m_Center"));box.size=Vector(fields.GetProperty("m_Size"));box.isTrigger=Flag(fields,"m_IsTrigger",false);component=box;result.boxColliders++;}
                else if(cls==54){component=go.AddComponent<Rigidbody>();}
                else if(cls==136){component=go.AddComponent<CapsuleCollider>();}
                else if(cls==108){component=go.AddComponent<Light>();}
                else if(cls==114){
                    string guid=fields.GetProperty("m_Script").GetProperty("guid").GetString();
                    label=package.GetProperty("guidPaths").TryGetProperty(guid,out var path)?Path.GetFileNameWithoutExtension(path.GetString()):$"Script:{guid}";
                    var type=ResolveScriptType(package,guid);
                    if(type!=null){component=go.AddComponent(type);scriptFields.Add((component,fields,id));result.originalComponents++;}
                }
                if(component!=null){
                    result.objects[id]=component;
                    if(component is Behaviour b)b.enabled=Flag(fields,"m_Enabled");
                    if(cls is 54 or 108 or 136)AssignSerializedFields(component,fields,result,id);
                } else result.unsupportedComponents[label]=result.unsupportedComponents.GetValueOrDefault(label)+1;
            }
            foreach(var (component,fields,id) in scriptFields)AssignSerializedFields(component,fields,result,id);
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
            var root=world.Objects.OfType<GameObject>().FirstOrDefault(g=>g&&ReferenceEquals(g.world,world)&&g.transform.parent==null&&g.name==assetPath);
            if(root!=null)world.DestroyNow(root);
            throw;
        }
    }
    static bool IsWithin(Transform value,Transform root){for(var t=value;t!=null;t=t.parent)if(ReferenceEquals(t,root))return true;return false;}
    static Type ResolveScriptType(JsonElement package,string guid){
        if(!package.GetProperty("guidPaths").TryGetProperty(guid,out var source))return null;
        string name=Path.GetFileNameWithoutExtension(source.GetString());
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
    static void AssignSerializedFields(Component component,JsonElement fields,SceneLoadResult result,string id){
        foreach(var property in fields.EnumerateObject()){
            if(property.Name.StartsWith("m_",StringComparison.Ordinal))continue;
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
            result.unresolvedReferences.Add($"Unresolved {type.Name} reference {key}");return null;
        }
        if(type.IsEnum)return value.ValueKind==JsonValueKind.String?Enum.Parse(type,value.GetString(),true):Enum.ToObject(type,value.GetInt32());
        if(type==typeof(string))return value.GetString();
        if(type==typeof(bool))return value.ValueKind==JsonValueKind.True||(value.ValueKind==JsonValueKind.Number&&value.GetInt32()!=0);
        if(type==typeof(float))return value.GetSingle();
        if(type==typeof(double))return value.GetDouble();
        if(type==typeof(int))return value.GetInt32();
        if(type==typeof(long))return value.GetInt64();
        if(type==typeof(uint))return value.GetUInt32();
        if(type==typeof(byte))return value.GetByte();
        if(type.IsArray){var items=value.EnumerateArray().Select(v=>ReadValue(v,type.GetElementType(),result)).ToArray();var array=Array.CreateInstance(type.GetElementType(),items.Length);for(int i=0;i<items.Length;i++)array.SetValue(items[i],i);return array;}
        if(type.IsGenericType&&typeof(IList).IsAssignableFrom(type)){
            var list=(IList)Activator.CreateInstance(type);var element=type.GetGenericArguments()[0];foreach(var item in value.EnumerateArray())list.Add(ReadValue(item,element,result));return list;
        }
        if(value.ValueKind!=JsonValueKind.Object)throw new NotSupportedException($"Expected serialized object for {type.FullName}");
        object instance=Activator.CreateInstance(type,true);
        foreach(var property in value.EnumerateObject()){
            var member=type.GetField(property.Name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic) as MemberInfo
                ??type.GetProperty(property.Name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if(member is FieldInfo f&&!f.IsInitOnly)f.SetValue(instance,ReadValue(property.Value,f.FieldType,result));
            else if(member is PropertyInfo p&&p.CanWrite)p.SetValue(instance,ReadValue(property.Value,p.PropertyType,result));
        }
        return instance;
    }
}
