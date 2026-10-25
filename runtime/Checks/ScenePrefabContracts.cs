using System.Text.Json;
using UnityEngine;
using UnityEngine.UI;

namespace Crisis.Runtime;

public sealed class ScenePrefabProbe : MonoBehaviour {
    public Transform child;
    public BoxCollider colliderReference;
    public HingeJoint joint;
    public AnimationCurve interpolation;
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
            new {fileID="3",classID=114,className="MonoBehaviour",fields=new {m_GameObject=Ref("1"),m_Enabled=true,m_Script=new{fileID="11500000",guid,type=3},child=Ref("5"),colliderReference=Ref("6"),joint=Ref("8"),interpolation=new{serializedVersion=2,m_Curve=new[]{new{serializedVersion=2,time=0f,value=0f,inSlope=0f,outSlope=0f,tangentMode=0},new{serializedVersion=2,time=1f,value=1f,inSlope=0f,outSlope=0f,tangentMode=0}},m_PreInfinity=2,m_PostInfinity=2,m_RotationOrder=4},linkedTransforms=new[]{Ref("5")},ready=true}},
            new {fileID="7",classID=54,className="Rigidbody",fields=new {m_GameObject=Ref("1"),m_Enabled=true,m_Mass=2f,m_Drag=.1f,m_AngularDrag=.2f,m_UseGravity=true,m_IsKinematic=false,m_Interpolate=1,m_Constraints=0,m_CollisionDetection=0}},
            new {fileID="4",classID=1,className="GameObject",fields=new {m_Name="Child",m_Layer=0,m_TagString="Untagged",m_StaticEditorFlags=0,m_IsActive=true}},
            new {fileID="5",classID=4,className="Transform",fields=new {m_GameObject=Ref("4"),m_LocalPosition=new{x=0f,y=1f,z=0f},m_LocalScale=new{x=1f,y=1f,z=1f},m_LocalRotation=new{x=0f,y=0f,z=0f,w=1f},m_Father=Ref("2"),m_Children=Array.Empty<object>()}},
            new {fileID="6",classID=65,className="BoxCollider",fields=new {m_GameObject=Ref("4"),m_Enabled=true,m_Center=new{x=0f,y=0f,z=0f},m_Size=new{x=1f,y=1f,z=1f},m_IsTrigger=false}},
            new {fileID="9",classID=212,className="SpriteRenderer",fields=new {m_GameObject=Ref("4"),m_Enabled=true,m_MaskInteraction=2,m_WasSpriteAssigned=1}},
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
        Check(template.interpolation?.length==2&&template.interpolation.serializedPreInfinity==2&&template.interpolation.serializedPostInfinity==2&&template.interpolation.preWrapMode==WrapMode.ClampForever&&template.interpolation.postWrapMode==WrapMode.ClampForever&&template.interpolation.Evaluate(2f)==1f,"Unity 2017 constant infinity metadata must clamp an authored transition curve instead of looping it");
        var spriteRenderer=templateChild.GetComponent<SpriteRenderer>();
        Check(spriteRenderer!=null&&spriteRenderer.maskInteraction==2&&spriteRenderer.wasSpriteAssigned,"Original SpriteRenderer mask interaction and sprite-assignment state are retained");
        Check(!template.awokeWithBoundReferences&&!prefab.activeInHierarchy,"Prefab asset loading defers gameplay callbacks and remains outside the active scene");
        var world=new RuntimeWorld();world.MakeCurrent();
        var templateBody=prefab.GetComponent<Rigidbody>();
        templateBody.PhysicsForceSink=(_,_,_)=>{};
        var first=UnityEngine.Object.Instantiate(prefab);var second=UnityEngine.Object.Instantiate(prefab);world.Tick(0);
        var a=first.GetComponent<ScenePrefabProbe>();var b=second.GetComponent<ScenePrefabProbe>();
        Check(a.awokeWithBoundReferences&&b.awokeWithBoundReferences,"Cloned actors enter Awake only after serialized references bind");
        Check(!ReferenceEquals(a.child,b.child)&&!ReferenceEquals(a.colliderReference,b.colliderReference),"Cloned actors own independent child and collider components");
        Check(ReferenceEquals(a.child,a.colliderReference.transform)&&ReferenceEquals(b.child,b.colliderReference.transform)&&ReferenceEquals(a.joint.connectedBody,first.GetComponent<Rigidbody>())&&ReferenceEquals(b.joint.connectedBody,second.GetComponent<Rigidbody>()),"Each actor's component links resolve within its own clone");
        Check(a.joint.useSpring&&b.joint.useSpring&&a.joint.spring.spring==12f&&b.joint.spring.spring==12f,"Cloned actors retain the serialized original joint state");
        Check(!ReferenceEquals(a.interpolation,b.interpolation)&&a.interpolation.Evaluate(2f)==1f&&b.interpolation.Evaluate(2f)==1f,"Cloned actors retain independent clamped transition curves");
        Check(first.GetComponent<Rigidbody>().PhysicsForceSink==null&&second.GetComponent<Rigidbody>().PhysicsForceSink==null,"Runtime physics delegates are not copied from a prefab asset into independent instances");
        Check(first.transform.childCount==1&&second.transform.childCount==1,"Prefab hierarchy survives independent actor clones");
        const string uiPath="Assets/GameObject/UiContract.prefab";
        const string unityUiGuid="f5f67c52d1564df4a8936ccd202a3bd8";
        object UiScript(string fileID)=>new {fileID,guid=unityUiGuid,type=3};
        object Vec2(float x,float y)=>new{x,y};
        var uiRecords=new object[]{
            new {fileID="10",classID=1,className="GameObject",fields=new {m_Name="Canvas",m_Layer=5,m_TagString="Untagged",m_StaticEditorFlags=0,m_IsActive=true}},
            new {fileID="11",classID=224,className="RectTransform",fields=new {m_GameObject=Ref("10"),m_LocalPosition=new{x=0f,y=0f,z=0f},m_LocalScale=new{x=1f,y=1f,z=1f},m_LocalRotation=new{x=0f,y=0f,z=0f,w=1f},m_Father=Ref("0"),m_Children=new[]{Ref("17")},m_AnchorMin=Vec2(0,0),m_AnchorMax=Vec2(1,1),m_AnchoredPosition=Vec2(0,0),m_SizeDelta=Vec2(1280,720),m_Pivot=Vec2(.5f,.5f)}},
            new {fileID="12",classID=223,className="Canvas",fields=new {m_GameObject=Ref("10"),m_Enabled=true,m_RenderMode=0,m_Camera=Ref("0"),m_PlaneDistance=100f,m_PixelPerfect=0,m_ReceivesEvents=1,m_OverrideSorting=0,m_OverridePixelPerfect=0,m_SortingBucketNormalizedSize=0f,m_AdditionalShaderChannelsFlag=25,m_SortingLayerID=0,m_SortingOrder=0,m_TargetDisplay=0}},
            new {fileID="13",classID=114,className="MonoBehaviour",fields=new {m_GameObject=Ref("10"),m_Enabled=true,m_Script=UiScript("1980459831"),m_UiScaleMode=1,m_ReferencePixelsPerUnit=100f,m_ScaleFactor=1f,m_ReferenceResolution=Vec2(1280,720),m_ScreenMatchMode=0,m_MatchWidthOrHeight=.25f,m_PhysicalUnit=3,m_FallbackScreenDPI=96f,m_DefaultSpriteDPI=96f,m_DynamicPixelsPerUnit=1f}},
            new {fileID="14",classID=114,className="MonoBehaviour",fields=new {m_GameObject=Ref("10"),m_Enabled=true,m_Script=UiScript("1301386320"),m_IgnoreReversedGraphics=1,m_BlockingObjects=2,m_BlockingMask=new{serializedVersion=2,m_Bits=unchecked((int)0xffff00ff)}}},
            new {fileID="16",classID=1,className="GameObject",fields=new {m_Name="Panel",m_Layer=5,m_TagString="Untagged",m_StaticEditorFlags=0,m_IsActive=true}},
            new {fileID="17",classID=224,className="RectTransform",fields=new {m_GameObject=Ref("16"),m_LocalPosition=new{x=0f,y=0f,z=0f},m_LocalScale=new{x=1f,y=1f,z=1f},m_LocalRotation=new{x=0f,y=0f,z=0f,w=1f},m_Father=Ref("11"),m_Children=Array.Empty<object>(),m_AnchorMin=Vec2(0,0),m_AnchorMax=Vec2(1,1),m_AnchoredPosition=Vec2(0,0),m_SizeDelta=Vec2(-20,-40),m_Pivot=Vec2(.5f,.5f)}}
        };
        var uiPackage=JsonSerializer.Serialize(new{guidPaths=new Dictionary<string,string>(),assets=new Dictionary<string,object>{{uiPath,new{records=uiRecords}}}});
        var uiWorld=new RuntimeWorld();uiWorld.MakeCurrent();
        var uiRoot=SceneLoader.LoadPrefab(uiPackage,uiPath,uiWorld).prefabRoot;
        var canvas=uiRoot.GetComponent<Canvas>();var scaler=uiRoot.GetComponent<CanvasScaler>();var raycaster=uiRoot.GetComponent<GraphicRaycaster>();
        var panel=(RectTransform)uiRoot.transform.GetChild(0);
        Check(canvas.renderMode==RenderMode.ScreenSpaceOverlay&&canvas.receivesEvents&&canvas.planeDistance==100&&canvas.additionalShaderChannelsFlag==25,"Original Canvas render and event settings are imported");
        Check(scaler.uiScaleMode==1&&scaler.referenceResolution.x==1280&&scaler.referenceResolution.y==720&&scaler.matchWidthOrHeight==.25f&&scaler.dynamicPixelsPerUnit==1,"Original CanvasScaler authored values are imported");
        Check(raycaster.blockingObjects==2&&raycaster.blockingMask.value==unchecked((int)0xffff00ff)&&raycaster.ignoreReversedGraphics,"Original GraphicRaycaster filtering values are imported");
        Check(panel.anchorMax.x==1&&panel.anchorMin.y==0&&panel.pivot.x==.5f&&panel.rect.width==1260&&panel.rect.height==680,"RectTransform anchors, pivot, and sizeDelta determine its stretched rectangle");
        var uiClone=UnityEngine.Object.Instantiate(uiRoot);var clonedPanel=(RectTransform)uiClone.transform.GetChild(0);
        Check(clonedPanel.anchorMax.x==1&&clonedPanel.pivot.x==.5f&&clonedPanel.rect.width==1260&&uiClone.GetComponent<CanvasScaler>().referenceResolution.x==1280&&uiClone.GetComponent<GraphicRaycaster>().blockingMask.value==unchecked((int)0xffff00ff),"Instantiated authored UI keeps its layout and scaler/raycaster state");
        return "PASS: source prefab record references, deferred Awake, and two independent actor clones";
    }
}
