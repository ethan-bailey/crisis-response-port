namespace UnityEngine;

public readonly record struct SpringJointSettings(Rigidbody Body,Rigidbody ConnectedBody,Vector3 Anchor,Vector3 ConnectedAnchor,float Spring,float Damper,float MinDistance,float MaxDistance,float Tolerance,float BreakForce,float BreakTorque,bool EnableCollision,bool EnablePreprocessing,float MassScale,float ConnectedMassScale);
public interface ISpringJointDriver:IDisposable {
    void Apply(SpringJointSettings settings);
}
// No hand-written spring integration: native solver ownership is mandatory.
// CharacterBody configures these after AddComponent, before the fixed step.
public sealed class SpringJoint:MonoBehaviour {
    public Rigidbody connectedBody {get;set;}
    public Vector3 anchor {get;set;}
    public Vector3 connectedAnchor {get;set;}
    public bool autoConfigureConnectedAnchor {get;set;}=true;
    public float spring {get;set;}
    public float damper {get;set;}
    public float minDistance {get;set;}
    public float maxDistance {get;set;}
    public float tolerance {get;set;}=.025f;
    public float breakForce {get;set;}=float.PositiveInfinity;
    public float breakTorque {get;set;}=float.PositiveInfinity;
    public bool enableCollision {get;set;}
    public bool enablePreprocessing {get;set;}=true;
    public float massScale {get;set;}=1;
    public float connectedMassScale {get;set;}=1;
    ISpringJointDriver driver;
    public ISpringJointDriver Driver {get=>driver;set{if(ReferenceEquals(driver,value))return;driver?.Dispose();driver=value;previous=null;}}
    SpringJointSettings? previous;
    bool configuredAnchor;
    Vector3 automaticAnchor;
    Rigidbody previousConnectedBody;
    Vector3 previousAnchor;
    public void Synchronize(){
        var body=GetComponent<Rigidbody>()??throw new InvalidOperationException("SpringJoint requires a Rigidbody");
        if(connectedBody!=null&&connectedBody.world!=world)throw new InvalidOperationException("Cross-world joint");
        Driver??=world.SpringJointFactory?.Invoke(this);
        if(Driver==null)throw new NotSupportedException("SpringJoint native PhysX driver is not bound");
        if(autoConfigureConnectedAnchor&&(!configuredAnchor||previousConnectedBody!=connectedBody||!previousAnchor.Equals(anchor))){
            var point=transform.TransformPoint(anchor);automaticAnchor=connectedBody!=null?connectedBody.transform.InverseTransformPoint(point):point;
            configuredAnchor=true;previousConnectedBody=connectedBody;previousAnchor=anchor;
        }
        if(!autoConfigureConnectedAnchor)configuredAnchor=false;
        var settings=new SpringJointSettings(body,connectedBody,anchor,autoConfigureConnectedAnchor?automaticAnchor:connectedAnchor,spring,damper,minDistance,maxDistance,tolerance,breakForce,breakTorque,enableCollision,enablePreprocessing,massScale,connectedMassScale);
        if(previous!=settings){Driver.Apply(settings);previous=settings;}
    }
    void FixedUpdate()=>Synchronize();
    void OnDestroy(){Driver=null;}
}
