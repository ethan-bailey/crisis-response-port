using System.Text.Json;
using UnityEngine;

namespace Crisis.Runtime;

public sealed class ScenePrefabProbe : MonoBehaviour {
    public Transform child;
    public BoxCollider colliderReference;
    public HingeJoint joint;
    public List<Transform> linkedTransforms=[];
    public bool ready;
    [NonSerialized] public bool awokeWithBoundReferences;
    void Awake()=>awokeWithBoundReferences=ready&&child!=null&&colliderReference!=null&&joint?.connectedBody!=null&&ReferenceEquals(child,colliderReference.transform)&&linkedTransforms.Count==1&&ReferenceEquals(linkedTransforms[0],child);
}

public static class ScenePrefabContracts {
    static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
    public static string Run(){
        const string path="Assets/Resources/characters/Probe.prefab";
        const string guid="aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        object Ref(string id)=>new {fileID=id};
        var records=new object[]{
            new {fileID="1",classID=1,className="GameObject",fields=new {m_Name="Probe",m_Layer=0,m_TagString="Untagged",m_StaticEditorFlags=0,m_IsActive=true}},
            new {fileID="2",classID=4,className="Transform",fields=new {m_GameObject=Ref("1"),m_LocalPosition=new{x=0f,y=0f,z=0f},m_LocalScale=new{x=1f,y=1f,z=1f},m_LocalRotation=new{x=0f,y=0f,z=0f,w=1f},m_Father=Ref("0"),m_Children=new[]{Ref("5")}}},
            new {fileID="3",classID=114,className="MonoBehaviour",fields=new {m_GameObject=Ref("1"),m_Enabled=true,m_Script=new{fileID="11500000",guid,type=3},child=Ref("5"),colliderReference=Ref("6"),joint=Ref("8"),linkedTransforms=new[]{Ref("5")},ready=true}},
            new {fileID="7",classID=54,className="Rigidbody",fields=new {m_GameObject=Ref("1"),m_Enabled=true,m_Mass=2f,m_Drag=.1f,m_AngularDrag=.2f,m_UseGravity=true,m_IsKinematic=false,m_Interpolate=1,m_Constraints=0,m_CollisionDetection=0}},
            new {fileID="4",classID=1,className="GameObject",fields=new {m_Name="Child",m_Layer=0,m_TagString="Untagged",m_StaticEditorFlags=0,m_IsActive=true}},
            new {fileID="5",classID=4,className="Transform",fields=new {m_GameObject=Ref("4"),m_LocalPosition=new{x=0f,y=1f,z=0f},m_LocalScale=new{x=1f,y=1f,z=1f},m_LocalRotation=new{x=0f,y=0f,z=0f,w=1f},m_Father=Ref("2"),m_Children=Array.Empty<object>()}},
            new {fileID="6",classID=65,className="BoxCollider",fields=new {m_GameObject=Ref("4"),m_Enabled=true,m_Center=new{x=0f,y=0f,z=0f},m_Size=new{x=1f,y=1f,z=1f},m_IsTrigger=false}},
            new {fileID="8",classID=59,className="HingeJoint",fields=new {m_GameObject=Ref("4"),m_Enabled=true,m_ConnectedBody=Ref("7"),m_Anchor=new{x=0f,y=0f,z=0f},m_Axis=new{x=1f,y=0f,z=0f},m_ConnectedAnchor=new{x=0f,y=0f,z=0f},m_AutoConfigureConnectedAnchor=false,m_UseSpring=true,m_Spring=new{spring=12f,damper=3f,targetPosition=4f},m_UseLimits=false,m_Limits=new{min=-10f,max=10f,bounciness=0f,bounceMinVelocity=0f,contactDistance=0f},m_UseMotor=false,m_Motor=new{targetVelocity=0f,force=0f,freeSpin=false},m_BreakForce=100f,m_BreakTorque=100f,m_EnableCollision=false,m_EnablePreprocessing=true,m_MassScale=1f,m_ConnectedMassScale=1f}}
        };
        var package=JsonSerializer.Serialize(new{
            guidPaths=new Dictionary<string,string>{{guid,"Assets/Scripts/ScenePrefabProbe.cs"}},
            assets=new Dictionary<string,object>{{path,new{records}}}
        });
        Resources.Configure(package);
        var prefab=Resources.Load("Characters/Probe") as GameObject;
        Check(prefab!=null,"Original character prefab is available through Resources.Load");
        var template=prefab.GetComponent<ScenePrefabProbe>();
        var templateChild=prefab.transform.GetChild(0);
        Check(template!=null&&template.child!=null,"Original prefab records load without unresolved component fields");
        Check(ReferenceEquals(template.child,templateChild)&&ReferenceEquals(template.colliderReference,templateChild.GetComponent<BoxCollider>())&&ReferenceEquals(template.linkedTransforms[0],templateChild)&&ReferenceEquals(template.joint.connectedBody,prefab.GetComponent<Rigidbody>()),"Serialized references bind to the exact prefab graph objects");
        Check(prefab.GetComponent<Rigidbody>().mass==2f&&template.joint.useSpring&&template.joint.spring.spring==12f&&template.joint.axis.x==1f,"Serialized rigidbody and hinge state retains original values");
        Check(!template.awokeWithBoundReferences&&!prefab.activeInHierarchy,"Prefab asset loading defers gameplay callbacks and remains outside the active scene");
        var world=new RuntimeWorld();world.MakeCurrent();
        var first=UnityEngine.Object.Instantiate(prefab);var second=UnityEngine.Object.Instantiate(prefab);world.Tick(0);
        var a=first.GetComponent<ScenePrefabProbe>();var b=second.GetComponent<ScenePrefabProbe>();
        Check(a.awokeWithBoundReferences&&b.awokeWithBoundReferences,"Cloned actors enter Awake only after serialized references bind");
        Check(!ReferenceEquals(a.child,b.child)&&!ReferenceEquals(a.colliderReference,b.colliderReference),"Cloned actors own independent child and collider components");
        Check(ReferenceEquals(a.child,a.colliderReference.transform)&&ReferenceEquals(b.child,b.colliderReference.transform)&&ReferenceEquals(a.joint.connectedBody,first.GetComponent<Rigidbody>())&&ReferenceEquals(b.joint.connectedBody,second.GetComponent<Rigidbody>()),"Each actor's component links resolve within its own clone");
        Check(a.joint.useSpring&&b.joint.useSpring&&a.joint.spring.spring==12f&&b.joint.spring.spring==12f,"Cloned actors retain the serialized original joint state");
        Check(first.transform.childCount==1&&second.transform.childCount==1,"Prefab hierarchy survives independent actor clones");
        return "PASS: source prefab record references, deferred Awake, and two independent actor clones";
    }
}
