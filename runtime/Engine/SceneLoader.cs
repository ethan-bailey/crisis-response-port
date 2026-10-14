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
    public object Report()=>new {scene,serializedRecords,gameObjects,transforms,boxColliders,originalComponents,unsupportedComponents,unresolvedReferences};
}
public static class SceneLoader {
    static readonly Dictionary<string,Type> scripts=new() {
        [nameof(DecayingObject)]=typeof(DecayingObject),[nameof(ObjectRemover)]=typeof(ObjectRemover),
        [nameof(AudioMaterial)]=typeof(AudioMaterial),[nameof(BuildingController)]=typeof(BuildingController)
    };
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
    public static SceneLoadResult Load(string json,string scene) {
        using var document=JsonDocument.Parse(json);
        var package=document.RootElement;
        var records=package.GetProperty("assets").GetProperty(scene).GetProperty("records").EnumerateArray().Select(r=>r.Clone()).ToArray();
        var result=new SceneLoadResult{scene=scene,serializedRecords=records.Length,world=new RuntimeWorld()};
        var transformsByGO=records.Where(r=>r.GetProperty("classID").GetInt32() is 4 or 224).ToDictionary(r=>Ref(r.GetProperty("fields").GetProperty("m_GameObject")));
        foreach(var record in records.Where(r=>r.GetProperty("classID").GetInt32()==1)) {
            var id=record.GetProperty("fileID").GetString();var f=record.GetProperty("fields");
            if(!transformsByGO.TryGetValue(id,out var tr))throw new NotSupportedException($"GameObject {id} requires prefab/stripped Transform resolution before execution");
            var go=new GameObject(f.GetProperty("m_Name").GetString(),result.world,tr.GetProperty("classID").GetInt32()==224);
            go.layer=f.GetProperty("m_Layer").GetInt32();go.tag=f.GetProperty("m_TagString").GetString();
            go.isStatic=f.TryGetProperty("m_StaticEditorFlags",out var flags)&&flags.GetUInt32()!=0;
            go.SetActive(Flag(f,"m_IsActive"));result.objects[id]=go;
            result.objects[tr.GetProperty("fileID").GetString()]=go.transform;result.gameObjects++;result.transforms++;
        }
        foreach(var record in transformsByGO.Values) {
            var t=(Transform)result.objects[record.GetProperty("fileID").GetString()];var f=record.GetProperty("fields");
            t.localPosition=Vector(f.GetProperty("m_LocalPosition"));t.localScale=Vector(f.GetProperty("m_LocalScale"));t.localRotation=Rotation(f.GetProperty("m_LocalRotation"));
            var parent=Ref(f.GetProperty("m_Father"));
            if(parent!="0") {
                if(!result.objects.TryGetValue(parent,out var p)||p is not Transform parentTransform)throw new InvalidOperationException($"Unresolved Transform parent {parent}");
                t.SetParent(parentTransform,false);
            }
        }
        // Use serialized child order (GetChild(0) is gameplay-significant).
        foreach(var record in transformsByGO.Values) {
            var t=(Transform)result.objects[record.GetProperty("fileID").GetString()];var f=record.GetProperty("fields");
            if(f.TryGetProperty("m_Children",out var children)&&children.ValueKind==JsonValueKind.Array){
                var ordered=children.EnumerateArray().Select(c=>Ref(c)).Select(id=>result.objects.TryGetValue(id,out var obj)?obj as Transform:null).ToArray();
                if(ordered.Any(c=>c==null)||ordered.Any(c=>c.parent!=t)||ordered.Length!=t.childCount)throw new InvalidOperationException($"Inconsistent children on Transform {record.GetProperty("fileID")}");
                t.children.Clear();t.children.AddRange(ordered);
            }
        }
        var scriptFields=new List<(Component component,JsonElement fields,string id)>();
        foreach(var record in records) {
            int cls=record.GetProperty("classID").GetInt32();if(cls is 1 or 4 or 224)continue;
            var fields=record.GetProperty("fields");var id=record.GetProperty("fileID").GetString();string label=record.GetProperty("className").GetString();
            if(!fields.TryGetProperty("m_GameObject",out var goRef))continue;
            if(!result.objects.TryGetValue(Ref(goRef),out var goObj)||goObj is not GameObject go)throw new InvalidOperationException($"Component {id} has unresolved GameObject");
            Component component=null;
            if(cls==65){var box=go.AddComponent<BoxCollider>();box.center=Vector(fields.GetProperty("m_Center"));box.size=Vector(fields.GetProperty("m_Size"));box.isTrigger=Flag(fields,"m_IsTrigger",false);component=box;result.boxColliders++;}
            else if(cls==108){var light=go.AddComponent<Light>();light.intensity=Number(fields,"m_Intensity",1);component=light;}
            else if(cls==114){
                string guid=fields.GetProperty("m_Script").GetProperty("guid").GetString();
                label=package.GetProperty("guidPaths").TryGetProperty(guid,out var path)?Path.GetFileNameWithoutExtension(path.GetString()):$"Script:{guid}";
                if(scripts.TryGetValue(label,out var type)){component=go.AddComponent(type);scriptFields.Add((component,fields,id));result.originalComponents++;}
            }
            if(component!=null){result.objects[id]=component;if(component is Behaviour b)b.enabled=Flag(fields,"m_Enabled");}
            else result.unsupportedComponents[label]=result.unsupportedComponents.GetValueOrDefault(label)+1;
        }
        foreach(var (component,fields,id) in scriptFields)foreach(var property in fields.EnumerateObject()){
            if(property.Name.StartsWith("m_"))continue;
            var field=component.GetType().GetField(property.Name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
            if(field==null){result.unresolvedReferences.Add($"Unsupported serialized field {component.GetType().Name}.{property.Name}");continue;}
            object value;
            if(field.FieldType==typeof(float))value=property.Value.GetSingle();
            else if(field.FieldType==typeof(int))value=property.Value.GetInt32();
            else if(field.FieldType==typeof(bool))value=property.Value.ValueKind==JsonValueKind.True||(property.Value.ValueKind==JsonValueKind.Number&&property.Value.GetInt32()!=0);
            else {result.unresolvedReferences.Add($"Field conversion not implemented {id}:{property.Name}");continue;}
            field.SetValue(component,value);
        }
        var settings=package.GetProperty("assets").GetProperty("ProjectSettings/TimeManager.asset").GetProperty("records")[0].GetProperty("fields");
        Time.fixedDeltaTime=Number(settings,"Fixed Timestep",.02f);Time.maximumDeltaTime=Number(settings,"Maximum Allowed Timestep",.33333334f);Time.timeScale=Number(settings,"m_TimeScale",1);
        // Scene graph is loaded without calling gameplay. Unsupported components
        // remain explicit; activation must be chosen by the host for a stated scope.
        return result;
    }
}
