using System.Collections;
using System.Reflection;
using N = System.Numerics;

namespace UnityEngine;

public class Object {
    static int nextID;
    internal bool destroyed;
    internal RuntimeWorld world;
    readonly int instanceID=Interlocked.Increment(ref nextID);
    public virtual string name { get; set; }
    public int GetInstanceID() => instanceID;
    public static implicit operator bool(Object o) => !ReferenceEquals(o,null)&&!o.destroyed;
    public static bool operator ==(Object a,Object b) {
        bool an=ReferenceEquals(a,null)||a.destroyed, bn=ReferenceEquals(b,null)||b.destroyed;
        return an||bn ? an==bn : ReferenceEquals(a,b);
    }
    public static bool operator !=(Object a,Object b) => !(a==b);
    public override bool Equals(object other) => other is Object obj ? this==obj : other is null && destroyed;
    public override int GetHashCode() => instanceID;
    public static void Destroy(Object obj) { if(obj) obj.world.QueueDestroy(obj); }
    public static T[] FindObjectsOfType<T>() where T:Object => RuntimeWorld.Current.Objects.OfType<T>().Where(o=>o&&! (o is Component c && !c.gameObject.activeInHierarchy)).ToArray();
    public static T Instantiate<T>(T obj) where T:Object => throw new NotSupportedException("Prefab instantiation is not implemented; original object will not be reused as a clone.");
}
public class GameObject:Object {
    internal readonly List<Component> components=[];
    bool active=true;
    public int layer {get;set;}
    public bool isStatic {get;set;}
    public string tag {get;set;}="Untagged";
    public Transform transform {get;internal set;}
    public bool activeSelf => active;
    public bool activeInHierarchy => this&&active&&(transform.parent==null||transform.parent.gameObject.activeInHierarchy);
    public GameObject(string name="GameObject",RuntimeWorld owner=null,bool rect=false) {
        world=owner??RuntimeWorld.Current;this.name=name;world.Register(this);
        transform=rect ? new RectTransform() : new Transform(); Attach(transform);
    }
    internal void Attach(Component c) { c.world=world;c.gameObject=this;components.Add(c);world.Register(c); }
    public T AddComponent<T>() where T:Component,new() => (T)AddComponent(typeof(T));
    public Component AddComponent(Type type) {
        if(!typeof(Component).IsAssignableFrom(type)||type.IsAbstract) throw new ArgumentException("Not a concrete component");
        if(typeof(Transform).IsAssignableFrom(type)) throw new InvalidOperationException("A GameObject already has its Transform");
        var c=(Component)Activator.CreateInstance(type);Attach(c);
        if(world.Initialized)world.Activate(c);
        return c;
    }
    public T GetComponent<T>() where T:class => components.OfType<T>().FirstOrDefault(c=>c is not Object o||o);
    public Component GetComponent(Type type) => components.FirstOrDefault(c=>c&&type.IsInstanceOfType(c));
    public T[] GetComponents<T>() where T:class => components.OfType<T>().Where(c=>c is not Object o||o).ToArray();
    public T[] GetComponentsInChildren<T>(bool includeInactive=false) where T:class {
        var result=new List<T>();
        void Visit(Transform t){if(includeInactive||t.gameObject.activeInHierarchy) result.AddRange(t.gameObject.GetComponents<T>());foreach(var child in t.children)Visit(child);}
        Visit(transform);return result.ToArray();
    }
    public void SetActive(bool value){if(active==value)return;active=value;world.RefreshActivation();}
}
public class Component:Object {
    public GameObject gameObject {get;internal set;}
    public Transform transform => gameObject.transform;
    public override string name {get=>gameObject?.name;set {if(gameObject!=null)gameObject.name=value;}}
    public T GetComponent<T>() where T:class => gameObject.GetComponent<T>();
    public T[] GetComponents<T>() where T:class => gameObject.GetComponents<T>();
    public T[] GetComponentsInChildren<T>(bool includeInactive=false) where T:class => gameObject.GetComponentsInChildren<T>(includeInactive);
    public T GetComponentInParent<T>() where T:class {
        for(var t=transform;t!=null;t=t.parent){var c=t.GetComponent<T>();if(c!=null)return c;}return null;
    }
}
public class Behaviour:Component {
    bool value=true;
    public bool enabled {get=>value;set{this.value=value;world?.RefreshActivation();}}
    public bool isActiveAndEnabled => this&&value&&gameObject.activeInHierarchy;
}
public class MonoBehaviour:Behaviour {
    internal bool awoken,started,enableNotified;
    public CoroutineHandle StartCoroutine(IEnumerator routine) => world.StartRoutine(this,routine);
    public void StopAllCoroutines() => world.StopRoutines(this);
}
public class Transform:Component,IEnumerable<Transform> {
    internal readonly List<Transform> children=[];
    Transform parentValue;
    public Vector3 localPosition {get;set;}=Vector3.zero;
    public Vector3 localScale {get;set;}=Vector3.one;
    public Quaternion localRotation {get;set;}=Quaternion.identity;
    public Transform parent {get=>parentValue;set=>SetParent(value);}
    public int childCount => children.Count;
    internal N.Matrix4x4 LocalMatrix => N.Matrix4x4.CreateScale(localScale.Native)*N.Matrix4x4.CreateFromQuaternion(localRotation.Native)*N.Matrix4x4.CreateTranslation(localPosition.Native);
    internal N.Matrix4x4 WorldMatrix => parentValue==null ? LocalMatrix : LocalMatrix*parentValue.WorldMatrix;
    internal N.Matrix4x4 InverseMatrix {get {if(!N.Matrix4x4.Invert(WorldMatrix,out var inv)) throw new InvalidOperationException("Singular transform");return inv;}}
    public Vector3 position {get=>TransformPoint(Vector3.zero);set=>localPosition=parentValue==null?value:parentValue.InverseTransformPoint(value);}
    public Quaternion rotation {get=>parentValue==null?localRotation:parentValue.rotation*localRotation;set=>localRotation=parentValue==null?value:Quaternion.Inverse(parentValue.rotation)*value;}
    public Vector3 forward => rotation*Vector3.forward;
    public Transform GetChild(int index)=>children[index];
    public Vector3 TransformPoint(Vector3 point)=>Vector3.From(N.Vector3.Transform(point.Native,WorldMatrix));
    public Vector3 InverseTransformPoint(Vector3 point)=>Vector3.From(N.Vector3.Transform(point.Native,InverseMatrix));
    public Vector3 TransformVector(Vector3 v)=>Vector3.From(N.Vector3.TransformNormal(v.Native,WorldMatrix));
    public Vector3 InverseTransformVector(Vector3 v)=>Vector3.From(N.Vector3.TransformNormal(v.Native,InverseMatrix));
    public Vector3 TransformDirection(Vector3 v)=>rotation*v;
    public Vector3 InverseTransformDirection(Vector3 v)=>Quaternion.Inverse(rotation)*v;
    internal Vector3 TransformNormal(Vector3 normal)=>Vector3.From(N.Vector3.TransformNormal(normal.Native,N.Matrix4x4.Transpose(InverseMatrix))).normalized;
    public void SetParent(Transform value,bool worldPositionStays=true){
        if(value==parentValue)return;
        for(var t=value;t!=null;t=t.parent)if(ReferenceEquals(t,this))throw new InvalidOperationException("Transform cycle");
        if(value!=null&&value.world!=world)throw new InvalidOperationException("Cross-world parenting");
        var previous=WorldMatrix;
        parentValue?.children.Remove(this);parentValue=value;parentValue?.children.Add(this);
        if(worldPositionStays){var local=value==null?previous:previous*value.InverseMatrix;
            if(!N.Matrix4x4.Decompose(local,out var scale,out var rot,out var pos))throw new NotSupportedException("Cannot preserve sheared transform when reparenting");
            localPosition=Vector3.From(pos);localScale=Vector3.From(scale);localRotation=Quaternion.From(rot);}
        world.RefreshActivation();
    }
    public IEnumerator<Transform> GetEnumerator()=>children.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
}
public class RectTransform:Transform { }
public class Light:Behaviour { public float intensity {get;set;}=1; }

public sealed class RuntimeWorld {
    [ThreadStatic] static RuntimeWorld current;
    public static RuntimeWorld Current => current??=new RuntimeWorld();
    public readonly List<Object> Objects=[];
    readonly HashSet<Object> destroyQueue=[];
    readonly List<CoroutineHandle> routines=[];
    readonly Dictionary<(Type,string),MethodInfo> messages=[];
    public bool Initialized {get;private set;}
    public RuntimeWorld(){current=this;Time.Reset();Input.Reset();}
    public void MakeCurrent()=>current=this;
    internal void Register(Object obj)=>Objects.Add(obj);
    internal void QueueDestroy(Object obj)=>destroyQueue.Add(obj);
    public void Invoke(MonoBehaviour target,string message){
        if(!messages.TryGetValue((target.GetType(),message),out var method)){
            for(var type=target.GetType();type!=null&&method==null;type=type.BaseType)
                method=type.GetMethod(message,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null);
            messages[(target.GetType(),message)]=method;
        }
        if(method==null)return;
        try{method.Invoke(target,null);}catch(TargetInvocationException e){throw new InvalidOperationException($"{target.GetType().Name}.{message} on {target.name}",e.InnerException);}
    }
    internal void Activate(Component c){if(c is not MonoBehaviour m||!m.gameObject.activeInHierarchy)return;
        if(!m.awoken){m.awoken=true;Invoke(m,"Awake");}
        if(m.isActiveAndEnabled&&!m.enableNotified){m.enableNotified=true;Invoke(m,"OnEnable");}
    }
    public void Initialize(){Initialized=true;foreach(var m in Objects.OfType<MonoBehaviour>().ToArray())Activate(m);}
    internal void RefreshActivation(){if(!Initialized)return;foreach(var m in Objects.OfType<MonoBehaviour>().ToArray()){
        if(m.gameObject.activeInHierarchy)Activate(m);
        if(!m.isActiveAndEnabled&&m.enableNotified){m.enableNotified=false;Invoke(m,"OnDisable");}
    }}
    public void Tick(float realDelta,Action beforeUpdate=null){
        MakeCurrent();
        if(!Initialized)Initialize();
        Input.BeginFrame();Time.BeginFrame(realDelta);
        foreach(var m in Objects.OfType<MonoBehaviour>().Where(m=>m.isActiveAndEnabled&&!m.started).ToArray()){m.started=true;Invoke(m,"Start");}
        while(Time.BeginFixedStep())foreach(var m in Active())Invoke(m,"FixedUpdate");
        Time.BeginUpdate();beforeUpdate?.Invoke();
        foreach(var m in Active())Invoke(m,"Update");
        foreach(var routine in routines.ToArray())routine.Advance();
        routines.RemoveAll(r=>r.finished);
        foreach(var m in Active())Invoke(m,"LateUpdate");
        foreach(var obj in destroyQueue.ToArray())DestroyNow(obj);destroyQueue.Clear();
        Input.EndFrame();
    }
    MonoBehaviour[] Active()=>Objects.OfType<MonoBehaviour>().Where(m=>m.isActiveAndEnabled).ToArray();
    void DestroyNow(Object obj){if(!obj)return;
        if(obj is GameObject go){foreach(var child in go.transform.children.ToArray())DestroyNow(child.gameObject);foreach(var c in go.components.ToArray())DestroyNow(c);go.transform.parent?.children.Remove(go.transform);}
        if(obj is MonoBehaviour m){if(m.enableNotified)Invoke(m,"OnDisable");if(m.awoken)Invoke(m,"OnDestroy");StopRoutines(m);}
        obj.destroyed=true;Objects.Remove(obj);
        if(obj is Component component)component.gameObject.components.Remove(component);
    }
    internal CoroutineHandle StartRoutine(MonoBehaviour owner,IEnumerator routine){var handle=new CoroutineHandle(owner,routine);routines.Add(handle);handle.Advance(true);return handle;}
    internal void StopRoutines(MonoBehaviour owner){foreach(var r in routines.Where(r=>r.owner==owner))r.finished=true;}
}
