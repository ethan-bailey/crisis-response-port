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
public static class RuntimeContracts {
    static void Check(bool condition,string name){if(!condition)throw new InvalidOperationException(name);}
    static void Near(float actual,float expected,string name){if(!float.IsFinite(actual)||MathF.Abs(actual-expected)>1e-4f)throw new InvalidOperationException($"{name}: {actual} != {expected}");}
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
        return JsonSerializer.Serialize(new {status="PASS",contracts=passed,originalSourceExecution=true,fullGameParity=false});
    }
}
