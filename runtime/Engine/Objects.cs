using System.Collections;
using System.Reflection;
using N = System.Numerics;

namespace UnityEngine;

public class Object {
    static int nextID;
    internal bool destroyed;
    internal RuntimeWorld world;
    public HideFlags hideFlags {get;set;}
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
    public static void DestroyImmediate(Object obj,bool allowDestroyingAssets=false) { if(obj)obj.world.DestroyNow(obj); }
    public static void DontDestroyOnLoad(Object obj) { if(!obj)throw new ArgumentNullException(nameof(obj)); }
    public static T[] FindObjectsOfType<T>() where T:Object => RuntimeWorld.Current.Objects.OfType<T>().Where(o=>o&&! (o is Component c && !c.gameObject.activeInHierarchy)).ToArray();
    public static T[] FindObjectsOfType<T>(bool includeInactive) where T:Object=>RuntimeWorld.Current.Objects.OfType<T>().Where(o=>o&&(includeInactive||o is not Component c||c.gameObject.activeInHierarchy)).ToArray();
    public static Object[] FindObjectsOfType(Type type,bool includeInactive=false){ArgumentNullException.ThrowIfNull(type);return RuntimeWorld.Current.Objects.Where(o=>o&&type.IsInstanceOfType(o)&&(includeInactive||o is not Component c||c.gameObject.activeInHierarchy)).ToArray();}
    public static T Instantiate<T>(T obj) where T:Object => PrefabCloner.Clone(obj);
    public static T Instantiate<T>(T obj,Transform parent) where T:Object {var clone=PrefabCloner.Clone(obj,parent,false);return clone;}
    public static T Instantiate<T>(T obj,Transform parent,bool worldPositionStays) where T:Object => PrefabCloner.Clone(obj,parent,worldPositionStays);
    public static T Instantiate<T>(T obj,Vector3 position,Quaternion rotation) where T:Object => PrefabCloner.Clone(obj,null,true,position,rotation);
}
public class GameObject:Object {
    internal readonly List<Component> components=[];
    bool active=true;
    internal bool assetObject;
    public int layer {get;set;}
    public bool isStatic {get;set;}
    public string tag {get;set;}="Untagged";
    public Transform transform {get;internal set;}
    public GameObject gameObject=>this;
    public bool activeSelf => active;
    public bool activeInHierarchy => this&&!assetObject&&active&&(transform.parent==null||transform.parent.gameObject.activeInHierarchy);
    public GameObject(string name="GameObject",RuntimeWorld owner=null,bool rect=false) {
        world=owner??RuntimeWorld.Current;this.name=name;world.Register(this);
        transform=rect ? new RectTransform() : new Transform(); Attach(transform);
    }
    public GameObject(string name,params Type[] componentTypes):this(name,(RuntimeWorld)null,false){
        ArgumentNullException.ThrowIfNull(componentTypes);
        foreach(var type in componentTypes)AddComponent(type);
    }
    public static GameObject Find(string name){
        if(string.IsNullOrEmpty(name))return null;
        string path=name.Trim('/');
        return RuntimeWorld.Current.Objects.OfType<GameObject>()
            .Where(go=>go&&go.activeInHierarchy)
            .FirstOrDefault(go=>go.name==path||TransformPath(go.transform)==path);
        static string TransformPath(Transform transform){
            var names=new Stack<string>();for(var current=transform;current!=null;current=current.parent)names.Push(current.name);
            return string.Join("/",names);
        }
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
    public virtual bool enabled {get=>value;set{this.value=value;world?.RefreshActivation();}}
    public bool isActiveAndEnabled => this&&value&&gameObject.activeInHierarchy;
}
public class MonoBehaviour:Behaviour {
    internal bool awoken,started,enableNotified;
    public CoroutineHandle StartCoroutine(IEnumerator routine) => world.StartRoutine(this,routine);
    public Coroutine StartCoroutine(string name){
        var method=GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,Type.EmptyTypes,null)??throw new MissingMethodException(GetType().FullName,name);
        if(method.Invoke(this,null) is not IEnumerator routine)throw new InvalidOperationException("Coroutine method must return IEnumerator");
        var handle=world.StartRoutine(this,routine);handle.methodName=name;return handle;
    }
    public void StopCoroutine(IEnumerator routine)=>world.StopRoutine(this,routine);
    public void StopCoroutine(string name)=>world.StopRoutine(this,name);
    public void StopCoroutine(Coroutine routine){if(routine is CoroutineHandle handle&&handle.owner==this)handle.finished=true;}
    public static void print(object message)=>Debug.Log(message);
    public void StopAllCoroutines() => world.StopRoutines(this);
    public void Invoke(string methodName,float time){
        ArgumentException.ThrowIfNullOrEmpty(methodName);
        IEnumerator Delayed(){yield return new WaitForSeconds(MathF.Max(0,time));
            var method=GetType().GetMethod(methodName,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,Type.EmptyTypes,null)
                ??throw new MissingMethodException(GetType().FullName,methodName);
            method.Invoke(this,null);
        }
        StartCoroutine(Delayed());
    }
}
public class Transform:Component,IEnumerable<Transform> {
    internal readonly List<Transform> children=[];
    Transform parentValue;
    public Vector3 localPosition {get;set;}=Vector3.zero;
    public Vector3 localScale {get;set;}=Vector3.one;
    public Quaternion localRotation {get;set;}=Quaternion.identity;
    public Transform parent {get=>parentValue;set=>SetParent(value);}
    public int childCount => children.Count;
    public Transform Find(string path){
        if(string.IsNullOrEmpty(path))return null;
        Transform current=this;
        foreach(var segment in path.Split('/')){
            if(segment.Length==0)continue;
            current=current.children.FirstOrDefault(child=>child.name==segment);
            if(current==null)return null;
        }
        return current==this?null:current;
    }
    public void SetSiblingIndex(int index){
        if(parentValue==null)return;
        if(index<0)throw new ArgumentOutOfRangeException(nameof(index));
        var siblings=parentValue.children;siblings.Remove(this);siblings.Insert(Math.Min(index,siblings.Count),this);
    }
    public int GetSiblingIndex()=>parentValue==null?0:parentValue.children.IndexOf(this);
    public void SetAsLastSibling()=>SetSiblingIndex(parentValue==null?0:parentValue.children.Count-1);
    public void LookAt(Vector3 worldPosition,Vector3 worldUp){rotation=Quaternion.LookRotation(worldPosition-position,worldUp);}
    public void LookAt(Vector3 worldPosition)=>LookAt(worldPosition,Vector3.up);
    internal N.Matrix4x4 LocalMatrix => N.Matrix4x4.CreateScale(localScale.Native)*N.Matrix4x4.CreateFromQuaternion(localRotation.Native)*N.Matrix4x4.CreateTranslation(localPosition.Native);
    internal N.Matrix4x4 WorldMatrix => parentValue==null ? LocalMatrix : LocalMatrix*parentValue.WorldMatrix;
    internal N.Matrix4x4 InverseMatrix {get {if(!N.Matrix4x4.Invert(WorldMatrix,out var inv)) throw new InvalidOperationException("Singular transform");return inv;}}
    public Vector3 position {get=>TransformPoint(Vector3.zero);set=>localPosition=parentValue==null?value:parentValue.InverseTransformPoint(value);}
    public Quaternion rotation {get=>parentValue==null?localRotation:parentValue.rotation*localRotation;set=>localRotation=parentValue==null?value:Quaternion.Inverse(parentValue.rotation)*value;}
    public Vector3 eulerAngles {get=>Euler(rotation);set=>rotation=Quaternion.AngleAxis(value.y,Vector3.up)*Quaternion.AngleAxis(value.x,Vector3.right)*Quaternion.AngleAxis(value.z,Vector3.forward);}
    static Vector3 Euler(Quaternion q){var f=q*Vector3.forward;var r=q*Vector3.right;var u=q*Vector3.up;return new(MathF.Asin(Math.Clamp(-f.y,-1,1))*180/MathF.PI,MathF.Atan2(f.x,f.z)*180/MathF.PI,MathF.Atan2(r.y,u.y)*180/MathF.PI);}
    public Vector3 localEulerAngles {get=>Euler(localRotation);set=>localRotation=Quaternion.AngleAxis(value.y,Vector3.up)*Quaternion.AngleAxis(value.x,Vector3.right)*Quaternion.AngleAxis(value.z,Vector3.forward);}
    public void Rotate(Vector3 axis,float angle)=>localRotation=localRotation*Quaternion.AngleAxis(angle,axis);
    public void Rotate(float x,float y,float z)=>Rotate(new Vector3(x,y,z),Space.Self);
    public void Rotate(Vector3 eulers,Space relativeTo=Space.Self){var delta=Quaternion.Euler(eulers);localRotation=relativeTo==Space.Self?localRotation*delta:delta*rotation;}
    public Vector3 up=>rotation*Vector3.up;
    public Vector3 right=>rotation*Vector3.right;
    public Vector3 forward => rotation*Vector3.forward;
    public Transform GetChild(int index)=>children[index];
    public Vector3 TransformPoint(Vector3 point)=>Vector3.From(N.Vector3.Transform(point.Native,WorldMatrix));
    public Vector3 InverseTransformPoint(Vector3 point)=>Vector3.From(N.Vector3.Transform(point.Native,InverseMatrix));
    public Vector3 TransformVector(Vector3 v)=>Vector3.From(N.Vector3.TransformNormal(v.Native,WorldMatrix));
    public Vector3 InverseTransformVector(Vector3 v)=>Vector3.From(N.Vector3.TransformNormal(v.Native,InverseMatrix));
    public Vector3 TransformDirection(float x,float y,float z)=>TransformDirection(new Vector3(x,y,z));
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
public class RectTransform:Transform {
    public enum Axis { Horizontal,Vertical }
    public Vector2 sizeDelta;
    public Vector2 anchoredPosition;
    float anchoredZ;
    public Vector3 anchoredPosition3D {get=>new(anchoredPosition.x,anchoredPosition.y,anchoredZ);set{anchoredPosition=new(value.x,value.y);anchoredZ=value.z;}}
    public Rect rect=>new(-sizeDelta.x*.5f,-sizeDelta.y*.5f,sizeDelta.x,sizeDelta.y);
    public void SetSizeWithCurrentAnchors(Axis axis,float size){
        if(!float.IsFinite(size))throw new ArgumentOutOfRangeException(nameof(size));
        if(axis==Axis.Horizontal)sizeDelta.x=size;
        else if(axis==Axis.Vertical)sizeDelta.y=size;
        else throw new ArgumentOutOfRangeException(nameof(axis));
    }
    public void GetWorldCorners(Vector3[] corners) {
        ArgumentNullException.ThrowIfNull(corners);
        if(corners.Length<4)throw new ArgumentException("Four corner slots are required",nameof(corners));
        var r=rect;
        corners[0]=TransformPoint(new Vector3(r.xMin,r.yMin,0));
        corners[1]=TransformPoint(new Vector3(r.xMin,r.yMax,0));
        corners[2]=TransformPoint(new Vector3(r.xMax,r.yMax,0));
        corners[3]=TransformPoint(new Vector3(r.xMax,r.yMin,0));
    }
}
public interface ILightDriver {
    void SetEnabled(Light light,bool enabled);
    void SetShadows(Light light,LightShadows shadows);
}
public class Light:Behaviour {
    LightShadows shadowType;
    ILightDriver driver;
    public float intensity {get;set;}=1;
 public Color color {get;set;}=Color.white;
    public ILightDriver Driver {
        get=>driver;
        set {driver=value;if(driver!=null){driver.SetEnabled(this,enabled);driver.SetShadows(this,shadowType);}}
    }
    public override bool enabled {
        get=>base.enabled;
        set {base.enabled=value;driver?.SetEnabled(this,value);}
    }
    public LightShadows shadows {
        get=>shadowType;
        set {if(!Enum.IsDefined(value))throw new ArgumentOutOfRangeException(nameof(value));shadowType=value;driver?.SetShadows(this,value);}
    }
}

public sealed class RuntimeWorld {
    [ThreadStatic] static RuntimeWorld current;
    public static RuntimeWorld Current => current??=new RuntimeWorld();
    public readonly List<Object> Objects=[];
    public Func<SpringJoint,ISpringJointDriver> SpringJointFactory;
    public Func<Vector3,float,int,QueryTriggerInteraction,Collider[]> OverlapSphereQuery;
    readonly HashSet<Object> destroyQueue=[];
    readonly List<CoroutineHandle> routines=[];
    readonly Dictionary<(Type,string),MethodInfo> messages=[];
    readonly Dictionary<(Type,string),MethodInfo> collisionMessages=[];
    int activationDeferrals;
    public bool Initialized {get;private set;}
    public RuntimeWorld(bool makeCurrent=true,bool resetClock=true){if(makeCurrent)current=this;if(resetClock){Time.Reset();Input.Reset();}}
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
    // Called with contacts from the native physics bridge, never inferred from
    // rendering overlap. Unity delivers collisions to disabled behaviours too.
    public void DispatchCollision(GameObject receiver,string message,Collision collision){
        if(message is not ("OnCollisionEnter" or "OnCollisionStay" or "OnCollisionExit"))throw new ArgumentException(nameof(message));
        if(receiver.world!=this||collision.collider.world!=this)throw new InvalidOperationException("Cross-world collision");
        if(!receiver.activeInHierarchy)return;
        foreach(var target in receiver.GetComponents<MonoBehaviour>()){
            if(!collisionMessages.TryGetValue((target.GetType(),message),out var method)){
                for(var type=target.GetType();type!=null&&method==null;type=type.BaseType)
                    method=type.GetMethod(message,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly,null,[typeof(Collision)],null);
                collisionMessages[(target.GetType(),message)]=method;
            }
            if(method==null)continue;
            try{method.Invoke(target,[collision]);}catch(TargetInvocationException e){throw new InvalidOperationException($"{target.GetType().Name}.{message} on {target.name}",e.InnerException);}
        }
    }
    internal void Activate(Component c){if(activationDeferrals>0||c is not MonoBehaviour m||!m.gameObject.activeInHierarchy)return;
        if(!m.awoken){m.awoken=true;Invoke(m,"Awake");}
        if(m.isActiveAndEnabled&&!m.enableNotified){m.enableNotified=true;Invoke(m,"OnEnable");}
    }
    public void Initialize(){Initialized=true;foreach(var m in Objects.OfType<MonoBehaviour>().ToArray())Activate(m);}
    internal void RefreshActivation(){if(!Initialized||activationDeferrals>0)return;foreach(var m in Objects.OfType<MonoBehaviour>().ToArray()){
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
        FlushDestroyed();
        Input.EndFrame();
    }
    internal void FlushDestroyed(){foreach(var obj in destroyQueue.ToArray())DestroyNow(obj);destroyQueue.Clear();}
    MonoBehaviour[] Active()=>Objects.OfType<MonoBehaviour>().Where(m=>m.isActiveAndEnabled).ToArray();
    internal void DestroyNow(Object obj){if(!obj)return;
        if(obj is GameObject go){foreach(var child in go.transform.children.ToArray())DestroyNow(child.gameObject);foreach(var c in go.components.ToArray())DestroyNow(c);go.transform.parent?.children.Remove(go.transform);}
        if(obj is MonoBehaviour m){if(m.enableNotified)Invoke(m,"OnDisable");if(m.awoken)Invoke(m,"OnDestroy");StopRoutines(m);}
        obj.destroyed=true;Objects.Remove(obj);
        if(obj is Component component)component.gameObject.components.Remove(component);
    }
    internal CoroutineHandle StartRoutine(MonoBehaviour owner,IEnumerator routine){var handle=new CoroutineHandle(owner,routine);routines.Add(handle);handle.Advance(true);return handle;}
    internal void StopRoutine(MonoBehaviour owner,IEnumerator routine){foreach(var r in routines.Where(r=>r.owner==owner&&ReferenceEquals(r.routineIdentity,routine)))r.finished=true;}
    internal void StopRoutine(MonoBehaviour owner,string name){foreach(var r in routines.Where(r=>r.owner==owner&&r.methodName==name))r.finished=true;}
    internal void StopRoutines(MonoBehaviour owner){foreach(var r in routines.Where(r=>r.owner==owner))r.finished=true;}
    public IDisposable DeferActivation(){activationDeferrals++;return new ActivationScope(this);}
    sealed class ActivationScope(RuntimeWorld world):IDisposable {
        RuntimeWorld value=world;
        public void Dispose(){if(value==null)return;var w=value;value=null;if(--w.activationDeferrals==0)w.RefreshActivation();}
    }
}

