using System.Collections;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Task = BehaviorDesigner.Runtime.Tasks.Task;
using Status = BehaviorDesigner.Runtime.Tasks.TaskStatus;

int assertions=0;
void Check(bool condition,string label){if(!condition)throw new Exception(label);assertions++;}
void Near(float actual,float expected,string label)=>Check(MathF.Abs(actual-expected)<1e-4f,$"{label}: {actual} != {expected}");
void Throws<T>(System.Action action,string label) where T:Exception {try{action();}catch(T){assertions++;return;}throw new Exception(label);}

// Capsule geometry: cylinder, endcap, axial/parallel rays, sphere degeneration,
// world scale/rotation, inside-origin exclusion and existing layer filtering.
{
 var w=new RuntimeWorld();var go=new GameObject("source capsule",w);var c=go.AddComponent<CapsuleCollider>();
 Check(c.Raycast(new Ray(new(-2,0,0),Vector3.right),out var hit,10),"capsule cylinder hit");Near(hit.distance,1.5f,"cylinder distance");Near(hit.normal.x,-1,"cylinder normal");
 Check(c.Raycast(new Ray(new(0,3,0),-Vector3.up),out hit,10),"axial endcap hit");Near(hit.distance,2,"endcap distance");
 Check(!c.Raycast(new Ray(Vector3.zero,Vector3.right),out _,10),"inside origin rejected");
 Check(!c.Raycast(new Ray(new(-2,1.1f,0),Vector3.right),out _,10),"endcap miss");
 c.height=.25f;Check(c.Raycast(new Ray(new(-2,0,0),Vector3.right),out hit,10),"short capsule sphere");Near(hit.distance,1.5f,"sphere distance");
 c.height=2;go.transform.localScale=new(2,3,4);go.transform.rotation=Quaternion.AngleAxis(90,Vector3.forward);
 var(a,b,r)=c.Geometry();Near(r,2,"nonuniform transverse scale");Near((b-a).magnitude,2,"scaled segment");
 Check(c.Raycast(new Ray(new(5,0,0),-Vector3.right),out hit,10),"rotated capsule hit");Near(hit.distance,2,"rotated hit distance");
 c.direction=0;go.transform.localScale=Vector3.one;go.transform.rotation=Quaternion.identity;
 Check(c.Raycast(new Ray(new(-3,0,0),Vector3.right),out hit,10),"X axis hit");Near(hit.distance,2,"X axis distance");
 go.layer=7;Check(Physics.RaycastAll(new Ray(new(-3,0,0),Vector3.right),10,1<<7).Length==1,"capsule in Physics query");
 Check(Physics.OverlapSphere(new(1.1f,0,0),.2f,1<<7).Contains(c),"capsule sphere overlap");
 Check(!Physics.OverlapSphere(new(2,0,0),.2f,1<<7).Contains(c),"capsule sphere separation");
 c.enabled=false;Check(Physics.RaycastAll(new Ray(new(-3,0,0),Vector3.right),10,1<<7).Length==0,"disabled capsule excluded");
}
// Native contact payload delivery, including disabled MonoBehaviours and
// attached rigidbody identity for compound colliders.
{
 var w=new RuntimeWorld();var receiver=new GameObject("receiver",w);var probe=receiver.AddComponent<ContactProbe>();probe.enabled=false;
 var other=new GameObject("other body",w);var body=other.AddComponent<Rigidbody>();var child=new GameObject("other shape",w);child.transform.parent=other.transform;var c=child.AddComponent<BoxCollider>();
 var collision=new Collision(c,new(2,0,0),new(3,0,0),new ContactPoint{point=new(1,0,0),normal=Vector3.up,otherCollider=c});
 w.DispatchCollision(receiver,"OnCollisionEnter",collision);
 Check(probe.Last==collision,"collision reaches disabled source component");Check(collision.gameObject==other&&collision.rigidbody==body,"compound collider body identity");
 var copy=collision.contacts;copy[0]=default;Near(collision.GetContact(0).point.x,1,"contact payload immutable");
 receiver.SetActive(false);probe.Last=null;w.DispatchCollision(receiver,"OnCollisionEnter",collision);Check(probe.Last==null,"inactive collision receiver excluded");
}
// SpringJoint never simulates an invented force: configuration and teardown
// must reach its native driver, with source anchors preserved.
{
 var w=new RuntimeWorld();var go=new GameObject("source hand",w);go.AddComponent<Rigidbody>();var joint=go.AddComponent<SpringJoint>();
 Throws<NotSupportedException>(()=>joint.Synchronize(),"unbound spring must fail");
 var driver=new JointDriver();joint.Driver=driver;var torso=new GameObject("source torso",w);joint.connectedBody=torso.AddComponent<Rigidbody>();
 go.transform.position=new(3,0,0);torso.transform.position=new(1,0,0);joint.anchor=new(0,.1f,0);joint.spring=1500;joint.Synchronize();
 Near(driver.Last.ConnectedAnchor.x,2,"automatic connected anchor");Near(driver.Last.Spring,1500,"source spring strength");
 joint.autoConfigureConnectedAnchor=false;joint.connectedAnchor=new(-.1f,-.025f,.1f);joint.Synchronize();Near(driver.Last.ConnectedAnchor.z,.1f,"explicit restraint anchor");
 w.Initialize();UnityEngine.Object.Destroy(go);w.FlushDestroyed();Check(driver.Disposed,"native joint disposed");
}
// Original limb limit updates keep the PhysX observer contract.
{
 var w=new RuntimeWorld();var hinge=new GameObject("source elbow",w).AddComponent<HingeJoint>();JointLimits observed=default;bool enabled=false;
 hinge.LimitsChanged=(limits,useLimits)=>{observed=limits;enabled=useLimits;};hinge.limits=new(){min=-100,max=0};hinge.useLimits=true;Near(observed.min,-100,"source hinge minimum");Check(enabled,"source hinge limits enabled");
}
// Original parameter names survive the adapter IDs; evaluation needs a real
// graph driver and follows the source speed setting.
{
 var w=new RuntimeWorld();var a=new GameObject("source Animator",w).AddComponent<Animator>();int moving=Animator.StringToHash("Moving");
 Check(moving==Animator.StringToHash("Moving"),"parameter identity stable");a.SetBool(moving,true);Check(a.GetBool("Moving"),"source bool parameter");
 Throws<InvalidOperationException>(()=>a.SetFloat(moving,1),"typed parameters");Throws<NotSupportedException>(()=>a.Update(.1f),"graph driver required");
 var driver=new AnimatorDriver();a.Driver=driver;Check(driver.Last.Name=="Moving","late Animator binding replays source state");a.SetFloat("MoveSpeed",.25f);a.speed=1.35f;a.Update(.1f);Check(driver.Last.Name=="MoveSpeed","original parameter name forwarded");Near(driver.Speed,1.35f,"source speed forwarded");
}
// Executed original equipment copy and migration rules, not a browser view.
{
 var e=new SerializableEquipment();e.primary.prefabName="M4";e.primary.componentSlots.Add(new(){name="source slot",prefabNames=["original attachment"]});e.items.itemPrefabNames.Add("Plasticuffs");
 var copy=e.Copy();copy.primary.componentSlots[0].prefabNames.Add("other");copy.items.itemPrefabNames.Add("Gauze");
 Check(e.primary.componentSlots[0].prefabNames.Count==1&&e.items.itemPrefabNames.Count==1,"original equipment deep copy");Check(e.ContainsPlasticuffs()&&!e.ContainsGauze()&&copy.ContainsGauze(),"original equipment item predicates");
 var v1=new SerializableEquipment.SerializableEquipment_v1();v1.primary.prefabName="source primary";var migrated=new SerializableEquipment(v1);Check(migrated.primary.prefabName=="source primary"&&!migrated.administerFirstAid,"original equipment migration");
}
// Original mission delay ordering, scaled and unscaled clocks, and mode hooks.
{
 var w=new RuntimeWorld();var scheduler=new ActionScheduler();var events=new List<string>();
 scheduler.Schedule(()=>events.Add("scaled"),1);scheduler.Schedule(()=>events.Add("real"),.5f,ActionScheduler.DelayType.UnscaledTime);
 scheduler.Update(0,.6f);Check(events.SequenceEqual(["real"]),"unscaled mission delay");scheduler.Update(1,.6f);scheduler.Update(2,2);Check(events.SequenceEqual(["real","scaled"]),"source scheduler once only");
 var mode=new GameObject("mode",w).AddComponent<ModeProbe>();mode.Hide();mode.Show();Check(mode.Events.SequenceEqual(["hide","show"]),"original mission mode hooks");
}
// Original surface sample selection and explicit decoded-audio driver boundary.
{
 var w=new RuntimeWorld();var go=new GameObject("surface",w);var properties=go.AddComponent<AudioSurfaceProperties>();properties.footstepSamples=[];properties.grenadeBounceSamples=[];var surface=go.AddComponent<AudioSurface>();surface.surfaceProperties=properties;
 Check(surface.RandomFootstep()==null,"original empty footstep list");var clip=new AudioClip("original.wav",.25f);properties.footstepSamples.Add(clip);Check(surface.RandomFootstep()==clip,"original surface sample returned");
 var audio=go.AddComponent<AudioSource>();Throws<NotSupportedException>(()=>audio.Stop(),"audio driver mandatory");var driver=new AudioDriver();audio.Driver=driver;Throws<InvalidOperationException>(()=>audio.PlayOneShot(clip),"undecoded sample rejected");
 clip.ReportLoadState(AudioDataLoadState.Loaded);audio.priority=60;audio.PlayOneShot(clip,.5f);Check(audio.isPlaying&&driver.Clip==clip&&driver.Priority==60,"decoded original sample delivered");audio.Stop();Check(!audio.isPlaying,"voice stop delivered");
}
// Curves serialized by the actual plugin retain their tangent data and wraps.
{
 var curve=new AnimationCurve();curve.AddKey(new Keyframe(0,0,1,1));curve.AddKey(new Keyframe(1,1,1,1));curve.preWrapMode=WrapMode.Loop;curve.postWrapMode=WrapMode.PingPong;
 Near(curve.Evaluate(-.25f),.75f,"source curve loop");Near(curve.Evaluate(1.25f),.75f,"source curve ping-pong");Check(curve.AddKey(new Keyframe(1,8,0,0))==-1,"duplicate serialized curve key rejected");
}
// Actual recovered scheduler + original Sequence/Wait/Selector and variables.
// Probe leaves only observe lifecycle; this is not a replacement NPC behavior.
{
 var w=new RuntimeWorld();var manager=new GameObject("original BehaviorManager",w).AddComponent<BehaviorManager>();manager.UpdateInterval=UpdateIntervalType.Manual;
 var tree=new GameObject("tree A",w).AddComponent<BehaviorTree>();tree.StartWhenEnabled=false;
 var leaf=new LifecycleTask{NodeData=new()};var sequence=new Sequence{NodeData=new()};sequence.AddChild(leaf,0);sequence.AddChild(new Wait{NodeData=new(),waitTime=.05f},1);tree.GetBehaviorSource().RootTask=sequence;tree.GetBehaviorSource().HasSerialized=true;
 tree.SetVariable("skill",new SharedInt{Value=1});
 var second=new GameObject("tree B",w).AddComponent<BehaviorTree>();second.StartWhenEnabled=false;var fallback=new LifecycleTask{NodeData=new()};var selector=new Selector{NodeData=new()};selector.AddChild(new AlwaysFail{NodeData=new()},0);selector.AddChild(fallback,1);second.GetBehaviorSource().RootTask=selector;second.GetBehaviorSource().HasSerialized=true;second.SetVariable("skill",new SharedInt{Value=2});
 w.Initialize();manager.UpdateInterval=UpdateIntervalType.Manual;tree.EnableBehavior();second.EnableBehavior();
 manager.Tick(tree);manager.Tick(second);
 Check(leaf.Events.Take(3).SequenceEqual(["awake","start","update"]),"original scheduler leaf lifecycle");Check(fallback.Events.Contains("start"),"original Selector fallback");
 tree.DisableBehavior(true);Check(leaf.Events.Contains("pause"),"original scheduler pause");w.Tick(.1f);tree.EnableBehavior();Check(leaf.Events.Contains("resume"),"original scheduler resume");
 manager.Tick(tree);manager.Tick(second);w.Tick(.1f);manager.Tick(tree);
 Check(leaf.Events.Count(e=>e=="awake")==1&&leaf.Events.Count(e=>e=="start")==1&&leaf.Events.Contains("end"),"original scheduler completion once");
 Check(((SharedInt)tree.GetVariable("skill")).Value==1&&((SharedInt)second.GetVariable("skill")).Value==2,"independent original blackboards");
 Check(tree.ExecutionStatus==Status.Success&&second.ExecutionStatus==Status.Success,"original Sequence Wait and Selector complete");
}
// Original scene APIs forward to a real world-bound transition driver.
{
 var first=new RuntimeWorld();first.MakeCurrent();
 Throws<NotSupportedException>(()=>UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"unbound scene transitions rejected");
 var sceneDriver=new SceneTransitionProbe();
 UnityEngine.SceneManagement.SceneManager.Bind(first,sceneDriver);
 Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex==2,"original active scene index forwarded");
 UnityEngine.SceneManagement.SceneManager.LoadScene(5);
 Check(sceneDriver.LastIndex==5,"original scene load forwarded to host");
 var secondWorld=new RuntimeWorld();secondWorld.MakeCurrent();
 Throws<NotSupportedException>(()=>UnityEngine.SceneManagement.SceneManager.LoadScene(2),"scene transition driver isolated per world");
 first.MakeCurrent();
 Check(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Original scene","source scene name preserved");
}
Console.WriteLine(Crisis.Runtime.RuntimeContracts.Run());
Console.WriteLine($"PASS: {assertions} foundation assertions; original full CharacterBody build is a separate required gate.");

sealed class ContactProbe:MonoBehaviour {public Collision Last;void OnCollisionEnter(Collision c)=>Last=c;}
sealed class JointDriver:ISpringJointDriver {public SpringJointSettings Last;public bool Disposed;public void Apply(SpringJointSettings s)=>Last=s;public void Dispose()=>Disposed=true;}
sealed class AnimatorDriver:IAnimatorDriver {public AnimatorParameter Last;public float Speed;public void SetParameter(AnimatorParameter p)=>Last=p;public void Evaluate(float dt,float speed)=>Speed=speed;}
sealed class AudioDriver:IAudioSourceDriver {public AudioClip Clip;public int Priority;public bool IsPlaying{get;private set;}public void PlayOneShot(AudioClip c,float v,int p){Clip=c;Priority=p;IsPlaying=true;}public void Stop()=>IsPlaying=false;}
sealed class ModeProbe:MissionMode {public List<string> Events=[];public override void OnShow()=>Events.Add("show");public override void OnHide()=>Events.Add("hide");}
sealed class LifecycleTask:BehaviorDesigner.Runtime.Tasks.Action {
 public readonly List<string> Events=[];int ticks;
 public override void OnAwake()=>Events.Add("awake");public override void OnStart()=>Events.Add("start");
 public override Status OnUpdate(){Events.Add("update");return ++ticks<2?Status.Running:Status.Success;}
 public override void OnEnd()=>Events.Add("end");public override void OnPause(bool paused)=>Events.Add(paused?"pause":"resume");
}
sealed class AlwaysFail:BehaviorDesigner.Runtime.Tasks.Action {public override Status OnUpdate()=>Status.Failure;}

sealed class SceneTransitionProbe:UnityEngine.SceneManagement.ISceneManagerDriver {
 public int LastIndex=-1;
 public UnityEngine.SceneManagement.Scene GetActiveScene()=>new(2,"Original scene");
 public void LoadScene(int buildIndex)=>LastIndex=buildIndex;
}
