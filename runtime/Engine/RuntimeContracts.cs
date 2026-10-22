using System.Collections;
using System.Text.Json;
using UnityEngine;
using UObject=UnityEngine.Object;

namespace Crisis.Runtime;

// Verification components are isolated from original gameplay and scene loading.
public sealed class LifecycleProbe:MonoBehaviour {
    public readonly List<string> trace=[];
    void Awake()=>trace.Add("Awake");void OnEnable()=>trace.Add("OnEnable");void Start()=>trace.Add("Start");
    void Update()=>trace.Add("Update");void FixedUpdate()=>trace.Add("FixedUpdate");void LateUpdate()=>trace.Add("LateUpdate");
    void OnDisable()=>trace.Add("OnDisable");void OnDestroy()=>trace.Add("OnDestroy");
}
public sealed class CloneProbe:MonoBehaviour {
    public Transform child;
    public BoxCollider colliderReference;
    public List<Transform> linkedTransforms=[];
    public bool valid;
    [NonSerialized] public bool awakeSawReferences;
    void Awake(){awakeSawReferences=valid&&child!=null&&colliderReference!=null&&child==colliderReference.transform&&linkedTransforms[0]==child;}
}
public static class RuntimeContracts {
    static void Check(bool condition,string name){if(!condition)throw new InvalidOperationException(name);}
    static void Near(float actual,float expected,string name){if(!float.IsFinite(actual)||MathF.Abs(actual-expected)>1e-4f)throw new InvalidOperationException($"{name}: {actual} != {expected}");}
    public static string RunBuiltinResource(JsonElement package){
        const string guid="0000000000000000f000000000000000";
        var world=new RuntimeWorld();
        var material=(Material)SerializedAssetResolver.Resolve(package,guid,"10303",world,typeof(Material));
        var shader=(Shader)SerializedAssetResolver.Resolve(package,guid,"46",world,typeof(Shader));
        Check(material.name=="Default-Material","Recovered built-in Material name");
        Check(SerializedAssetIdentity.Guid(material)==guid&&SerializedAssetIdentity.FileID(material)=="10303","Recovered built-in Material identity");
        Check(material.OriginalAssetPath=="Editor/Data/Resources/unity_builtin_extra","Recovered built-in Material source path");
        Check(material.OriginalSerializedBytes?.Length==1076&&material.OriginalSerializedSha256=="1df2f22f709850941a6efe240cebcc3d2a8a892bd4fc10c51c735c5659d33c1e","Recovered built-in Material bytes and hash");
        Check(material.shader==shader&&shader.name=="Standard","Material's original Shader 46 pointer");
        Check(SerializedAssetIdentity.FileID(shader)=="46"&&shader.OriginalSerializedBytes?.Length==73778&&shader.OriginalSerializedSha256=="67598553f5b1987b841059e991434dbadfe15a1b72955d19b52d1edd647e07f5","Recovered built-in Shader identity and bytes");
        Check(!shader.isSupported,"Serialized shader is not reported as GPU-executable by the CPU runtime");
        Near(material.color.r,1,"Default material red");Near(material.color.g,1,"Default material green");Near(material.color.b,1,"Default material blue");Near(material.color.a,1,"Default material alpha");
        Near(material.GetFloat("_Metallic"),0,"Default material metallic");Near(material.GetFloat("_Glossiness"),.5f,"Default material glossiness");
        Near(material.GetFloat("_SrcBlend"),1,"Default material source blend");Near(material.GetFloat("_DstBlend"),0,"Default material destination blend");
        Check(material.renderQueue==-1&&!material.enableInstancing&&!material.doubleSidedGI&&material.lightmapFlags==5,"Default material render and lighting flags");
        Check(world.Objects.Contains(material)&&world.Objects.Contains(shader),"Recovered built-ins are registered in their RuntimeWorld");
        return JsonSerializer.Serialize(new{status="PASS",materialFileID="10303",materialName=material.name,shaderFileID="46",shaderName=shader.name,materialSha256=material.OriginalSerializedSha256,shaderSha256=shader.OriginalSerializedSha256,shaderExecutable=false,editorRecordSource=material.OriginalAssetPath});
    }
    public static string Run(){
        var passed=new List<string>();
        var world=new RuntimeWorld();var parent=new GameObject("parent",world);var child=new GameObject("child",world);
        parent.transform.localPosition=new(10,0,0);parent.transform.localScale=new(2,3,4);parent.transform.localRotation=Quaternion.AngleAxis(90,Vector3.forward);
        child.transform.SetParent(parent.transform,false);child.transform.localPosition=new(1,0,0);
        Near(child.transform.position.x,10,"Scaled parent X");Near(child.transform.position.y,2,"Scaled parent Y");
        var p=child.transform.TransformPoint(new(2,3,4));var roundtrip=child.transform.InverseTransformPoint(p);
        Near(roundtrip.x,2,"Inverse X");Near(roundtrip.y,3,"Inverse Y");Near(roundtrip.z,4,"Inverse Z");
        child.transform.SetParent(null,true);Near(child.transform.position.y,2,"Reparent preserve position");
        passed.Add("Hierarchy, scale, rotation, inverse and reparent transforms");

        world=new RuntimeWorld();var go=new GameObject("lifecycle",world);var probe=go.AddComponent<LifecycleProbe>();
        world.Tick(.02f);Check(probe.trace.SequenceEqual(new[]{"Awake","OnEnable","Start","FixedUpdate","Update","LateUpdate"}),"Lifecycle order");
        probe.enabled=false;Check(probe.trace.Last()=="OnDisable","Disable callback");probe.enabled=true;Check(probe.trace.Last()=="OnEnable","Enable callback");
        UObject.Destroy(go);Check(go!=null,"Destroy is deferred");world.Tick(0);Check(go==null,"Destroyed Unity object null semantics");
        Check(probe.trace.Last()=="OnDestroy","OnDestroy callback");passed.Add("Lifecycle, active state and deferred destruction");

        world=new RuntimeWorld();go=new GameObject("Original DecayingObject",world);var decay=go.AddComponent<DecayingObject>();decay.lifeTime=.05f;
        world.Tick(.01f);world.Tick(.02f);Check(go!=null,"Original decay remains before deadline");world.Tick(.04f);Check(go==null,"Original DecayingObject destroyed by real Time and lifecycle");
        passed.Add("Complete original DecayingObject.cs execution");
        world=new RuntimeWorld();go=new GameObject("Original ObjectRemover",world);var remover=go.AddComponent<ObjectRemover>();remover.maxObjects=1;
        var first=new GameObject("first",world);first.transform.SetParent(go.transform,false);var second=new GameObject("second",world);second.transform.SetParent(go.transform,false);
        world.Tick(.01f);Check(first==null&&second!=null&&go.transform.childCount==1,"Original ObjectRemover removes GetChild(0)");passed.Add("Complete original ObjectRemover.cs execution");

        world=new RuntimeWorld();var input=new OriginalInputHost(world);input.SetAction("fire",true);input.Tick(.01f);
        Check(input.mapping.fire.Down&&input.mapping.fire.Pressed&&!input.mapping.fire.DownLong,"Original input press edge");
        input.Tick(.1f);Check(!input.mapping.fire.Down&&!input.mapping.fire.PressedLong,"Original input held short");
        input.SetAction("fire",false);input.Tick(.01f);Check(input.mapping.fire.Up&&input.mapping.fire.UpShort&&!input.mapping.fire.UpLong,"Original short release");
        input.SetAction("fire",true);input.Tick(.01f);input.Tick(.21f);Check(input.mapping.fire.DownLong&&input.mapping.fire.PressedLong,"Original long edge");
        input.Tick(.01f);Check(!input.mapping.fire.DownLong&&input.mapping.fire.PressedLong,"Original long edge once");
        input.SetAction("fire",false);input.Tick(.01f);Check(input.mapping.fire.UpLong&&!input.mapping.fire.UpShort,"Original long release");
        input.SetAction("fire",true);input.Tick(.01f);input.mapping.fire.DownHandled=true;input.SetAction("fire",false);input.Tick(.01f);
        // UpdateKeys resets DownHandled after computing the release flags. Keep
        // the recovered behavior even though Up and UpShort can then differ.
        Check(input.mapping.fire.Up&&!input.mapping.fire.UpShort&&!input.mapping.fire.UpLong,"Original handled release ordering");
        input.handler.SuspendInput(true,true);input.SetAction("left",true);input.SetAction("console",true);input.Tick(.01f);
        Check(!input.mapping.left.Pressed&&input.mapping.console.Pressed,"Original suspension and universal key");
        input.handler.SuspendInput(false,true);Input.SetAxis("ScrollWheel",1);input.Tick(.01f);
        Check(input.mapping.doorPush.Pressed&&!input.mapping.doorPull.Pressed,"Original signed axis mapping");
        input.handler.EnableCategory(InputKey.Category.Movement,false);input.Tick(.01f);Check(!input.mapping.left.Enabled,"Original category disable");
        passed.Add("Original InputKey and selected InputHandler methods: edges, long/short, handling, suspension, axis, categories");

        world=new RuntimeWorld();go=new GameObject("ray box",world);go.layer=8;go.transform.localPosition=new(0,0,5);var box=go.AddComponent<BoxCollider>();box.size=new(2,2,2);
        Check(Physics.Raycast(new Ray(Vector3.zero,Vector3.forward),out var hit,10,1<<8),"Real box hit");Near(hit.distance,4,"Box distance");Near(hit.normal.z,-1,"Box normal");
        Check(!Physics.Raycast(new Ray(Vector3.zero,Vector3.forward),out _,10,1<<9),"Layer mask");box.isTrigger=true;
        Check(!Physics.Raycast(new Ray(Vector3.zero,Vector3.forward),out _,10,1<<8,QueryTriggerInteraction.Ignore),"Trigger filter");
        Check(!box.Raycast(new Ray(new(0,0,5),Vector3.forward),out _,10),"Inside origin excluded");
        box.isTrigger=false;go.transform.localScale=new(2,1,1);go.transform.localRotation=Quaternion.AngleAxis(90,Vector3.up);
        Check(box.Raycast(new Ray(Vector3.zero,Vector3.forward),out hit,10),"Oriented scaled collider");Near(hit.distance,3,"Scaled rotated box distance");
        passed.Add("Actual box ray queries: nearest hit, layers, triggers, inside-origin behavior, orientation and scale");

        world=new RuntimeWorld();input=new OriginalInputHost(world);Time.timeScale=0;input.SetAction("fire",true);input.Tick(.01f);input.Tick(.3f);
        Near(Time.time,0,"Paused scaled time");Check(input.mapping.fire.PressedLong,"Original input long press uses real time while paused");
        passed.Add("Scaled/unscaled time and original input during pause");
        world=new RuntimeWorld();go=new GameObject("coroutine",world);probe=go.AddComponent<LifecycleProbe>();bool called=false;
        IEnumerator Routine(){yield return new WaitForSeconds(.04f);called=true;}
        probe.StartCoroutine(Routine());world.Tick(.02f);Check(!called,"Coroutine waits");world.Tick(.02f);Check(called,"Coroutine resumed");
        passed.Add("Coroutine scaled wait and resume");
        var curve=new AnimationCurve(new Keyframe(0,0,1,1),new Keyframe(1,1,1,1));
        Near(curve.Evaluate(.25f),.25f,"Hermite curve retains tangents");
        curve=new AnimationCurve(new Keyframe(0,0,0,0),new Keyframe(1,1,0,0));
        Near(curve.Evaluate(.25f),.15625f,"Hermite curve is not linear interpolation");
        passed.Add("Unweighted original curve key/tangent interpolation within key domain");
        world=new RuntimeWorld();world.Tick(.01f);
        var template=new GameObject("Original-configured template",world);template.assetObject=true;
        var templateChild=new GameObject("child",world);templateChild.transform.SetParent(template.transform,false);templateChild.transform.localPosition=new(2,3,4);
        box=templateChild.AddComponent<BoxCollider>();box.size=new(2,4,6);
        var sourceProbe=template.AddComponent<CloneProbe>();sourceProbe.child=templateChild.transform;sourceProbe.colliderReference=box;sourceProbe.linkedTransforms.Add(templateChild.transform);sourceProbe.valid=true;
        var copyGO=UObject.Instantiate(template);var copyProbe=copyGO.GetComponent<CloneProbe>();
        Check(!ReferenceEquals(copyGO,template)&&copyGO.name==template.name+"(Clone)","Distinct clone identity and name");
        Check(copyProbe.awakeSawReferences,"Clone references and fields resolve before original lifecycle");
        Check(copyProbe.child!=sourceProbe.child&&copyProbe.child==copyGO.transform.GetChild(0),"Internal Transform reference remap");
        Check(copyProbe.colliderReference!=box&&copyProbe.colliderReference.size.y==4,"Internal component remap and collider data");
        copyProbe.linkedTransforms.Clear();Check(sourceProbe.linkedTransforms.Count==1,"Serialized list is independent");
        var parentGO=new GameObject("instance parent",world);parentGO.transform.localPosition=new(10,0,0);
        var componentCopy=UObject.Instantiate(box,parentGO.transform,false);
        Check(componentCopy.gameObject!=box.gameObject&&componentCopy.transform.parent==parentGO.transform,"Component Instantiate clones owning subtree with parent");
        Near(componentCopy.transform.localPosition.x,2,"Parented clone local pose");
        passed.Add("Prefab clone identity, hierarchy, serialized fields, list/reference remapping and deferred activation");
        return JsonSerializer.Serialize(new {status="PASS",contracts=passed,originalSourceExecution=true,fullGameParity=false});
    }
}
