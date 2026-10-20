namespace UnityEngine;

public enum AnimatorControllerParameterType {Float=1,Int=3,Bool=4,Trigger=9}
public readonly record struct AnimatorParameter(string Name,AnimatorControllerParameterType Type,object Value);
public interface IAnimatorDriver {
    void SetParameter(AnimatorParameter parameter);
    void Evaluate(float deltaTime,float speed);
}
// Parameters use interned host IDs, never pretend to reproduce Unity's opaque
// native hash. The driver receives the original parameter name from the asset.
public enum AnimatorCullingMode { AlwaysAnimate=0, CullUpdateTransforms=1, CullCompletely=2 }
public enum AnimatorUpdateMode { Normal=0, AnimatePhysics=1, UnscaledTime=2 }
public sealed class Animator:Behaviour {
    public Object avatar {get;set;}
    public RuntimeAnimatorController runtimeController {get;set;}
    static readonly Dictionary<string,int> ids=new(StringComparer.Ordinal);
    static readonly Dictionary<int,string> names=[];
    readonly Dictionary<int,AnimatorParameter> parameters=[];
    IAnimatorDriver driver;
    public IAnimatorDriver Driver {
        get=>driver;
        set {if(value!=null)foreach(var parameter in parameters.Values)value.SetParameter(parameter);driver=value;}
    }
    public float speed {get;set;}=1;
    public AnimatorCullingMode cullingMode {get;set;}=AnimatorCullingMode.AlwaysAnimate;
    public AnimatorUpdateMode updateMode {get;set;}=AnimatorUpdateMode.Normal;
    public bool applyRootMotion {get;set;}
    public bool linearVelocityBlending {get;set;}
    public string warningMessage {get;set;}
    public bool hasTransformHierarchy {get;set;}
    public bool allowConstantClipSamplingOptimization {get;set;}=true;
    public static int StringToHash(string name){
        ArgumentNullException.ThrowIfNull(name);
        lock(ids){if(ids.TryGetValue(name,out int id))return id;id=ids.Count+1;ids.Add(name,id);names.Add(id,name);return id;}
    }
    void Set(int id,AnimatorControllerParameterType type,object value){
        string name;lock(ids){if(!names.TryGetValue(id,out name))throw new NotSupportedException("Native Animator hashes must be bound through source parameter names");}
        if(parameters.TryGetValue(id,out var existing)&&existing.Type!=type)throw new InvalidOperationException($"Animator parameter {name} changed type");
        var parameter=new AnimatorParameter(name,type,value);Driver?.SetParameter(parameter);parameters[id]=parameter;
    }
    T Get<T>(int id,AnimatorControllerParameterType type,T defaultValue){if(!parameters.TryGetValue(id,out var p))return defaultValue;if(p.Type!=type)throw new InvalidOperationException($"Animator parameter {p.Name} type mismatch");return (T)p.Value;}
    public void SetBool(int id,bool value)=>Set(id,AnimatorControllerParameterType.Bool,value);
    public void SetFloat(int id,float value)=>Set(id,AnimatorControllerParameterType.Float,value);
    public void SetInteger(int id,int value)=>Set(id,AnimatorControllerParameterType.Int,value);
    public void SetTrigger(int id)=>Set(id,AnimatorControllerParameterType.Trigger,true);
    public void ResetTrigger(int id)=>Set(id,AnimatorControllerParameterType.Trigger,false);
    public bool GetBool(int id)=>Get(id,AnimatorControllerParameterType.Bool,false);
    public float GetFloat(int id)=>Get(id,AnimatorControllerParameterType.Float,0f);
    public int GetInteger(int id)=>Get(id,AnimatorControllerParameterType.Int,0);
    public void SetBool(string name,bool value)=>SetBool(StringToHash(name),value);
    public void SetFloat(string name,float value)=>SetFloat(StringToHash(name),value);
    public void SetInteger(string name,int value)=>SetInteger(StringToHash(name),value);
    public void SetTrigger(string name)=>SetTrigger(StringToHash(name));
    public void ResetTrigger(string name)=>ResetTrigger(StringToHash(name));
    public bool GetBool(string name)=>GetBool(StringToHash(name));
    public float GetFloat(string name)=>GetFloat(StringToHash(name));
    public int GetInteger(string name)=>GetInteger(StringToHash(name));
    public void Update(float deltaTime){if(!float.IsFinite(deltaTime)||deltaTime<0||!float.IsFinite(speed))throw new ArgumentOutOfRangeException(nameof(deltaTime));if(!isActiveAndEnabled)return;if(Driver==null)throw new NotSupportedException("Original AnimatorController graph driver is not bound");Driver.Evaluate(deltaTime,speed);}
}
