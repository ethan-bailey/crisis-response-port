using System.Text.Json;
using UnityEngine;
using Object=UnityEngine.Object;
namespace UnityEngine.Events {
 public delegate void UnityAction();public delegate void UnityAction<T>(T value);public delegate void UnityAction<T,U>(T arg0,U arg1);
 public class UnityEvent {event UnityAction callbacks;public void AddListener(UnityAction f)=>callbacks+=f;public void Invoke()=>callbacks?.Invoke();}
 public class UnityEventBool {event UnityAction<bool> callbacks;public void AddListener(UnityAction<bool> f)=>callbacks+=f;public void Invoke(bool value)=>callbacks?.Invoke(value);}
}
namespace Crisis.Runtime.Weapons {
// Explicit browser transport views. Original gameplay methods are in Original/Weapons.
public sealed class WeaponEffects {
 public readonly List<object> events=[];
 public void Emit(string type,object value)=>events.Add(new{type,value,time=Time.time});
}
public sealed class PropagatingAudioSample {
 readonly WeaponEffects effects;readonly string name;
 public PropagatingAudioSample(WeaponEffects e,string n){effects=e;name=n;}
 public void Play(float position,object instigator)=>effects.Emit("audio",name);
}
public class Casing:Component {
 public bool Spent{get;private set;} public bool Live=>!Spent;public float Offset;
 public void Detonate(){Spent=true;if(transform.childCount>0)Object.Destroy(transform.GetChild(0).gameObject);}
}
public class HammerUnsupported:Component {public void Release()=>throw new NotSupportedException();public void SetLevel(float level)=>throw new NotSupportedException();}
public class WeaponAmmunition:global::AmmunitionBase {public Casing casing;public global::Projectile projectile;}
public class WeaponComponentMagazine:Component {
 public int capacity;public float malfunctionRate,weight;public WeaponEffects Effects;public Transform ikTarget;
 public void Drop(){gameObject.SetActive(false);Effects.Emit("magazineRemoved",null);}
}
// Transport binding inherits the unchanged compiled original Hand.SetMode.
public class Hand:global::Hand {
 public Mode mode=>CurrentMode;
 public readonly Dictionary<string,GameObject> SourceVisuals=[];
}
public partial class Limb:BallisticMaterial {public CharacterBody Body;}
public class WeaponArmConfiguration {public Hand.Mode rightHandMode,leftHandMode;public float rightHandAngle,leftHandAngle;}
public class Blackboard {public float weaponSkill;}
public partial class CharacterBody:Component {
 public enum CharacterAction{None,SwitchMagazine,WorkBolt}
 public Blackboard Blackboard=new(){weaponSkill=1};public Equipment Equipment;public WeaponBase ActiveWeapon;
 public ArmIKSolver leftArm,rightArm;public Limb Torso,Pelvis;
 public Transform chestRigIKTarget,pistolPivot,pistolIKTarget;
 public bool IsPlayer=true,IsManipulatingEquipment;public float Direction=1,LastFiredTimestamp;public bool IsUsingPrecision;public float RotationY=>Direction==1?0:180;public int ShotsFired;
 public float Position=>transform.position.x;public bool FacingRight=>Direction==1f;public object AudioInstigator=>null;
 public CharacterAction PendingAction;bool triggerPulled;
 // Source arm state: default Operator rigidbodies are authored kinematic.
 // CharacterBody/physiology ragdoll transitions must supply future changes.
 public bool Kinematic=true;
 public GrenadeReference.Type PendingGrenadeType;
 public Transform stunGrenadeAnchor,fragGrenadeAnchor;
 public bool InsertSingleRound()=>throw new NotSupportedException("Only original detachable-magazine M4 reload is active");
}
public class AmmunitionStack {public int count;public string caliber;}
public enum TaskStatus{Running,Success,Failure}
public abstract class WeaponTask:Component {
 public virtual void OnAwake(){}public virtual void OnStart(){}public virtual void OnEnd(){}public abstract TaskStatus OnUpdate();
}
public sealed class SettingsView {public GeneralView General=new();}
public sealed class GeneralView {public bool reloadHelp=true;}
public sealed class ContextView {public InputHandler InputHandler;public SettingsView Settings=new();}
public static class Global {public static ContextView Context=new();}
public partial class WeaponBase:Component {
 public enum FireMode{Safe,Semi,Burst,Auto}public enum ChamberState{Live,Spent}public enum BoltReleaseAnimation{NoAnimation,ControlPosition,BoltHandle}
 public CyclingBolt cyclingBolt;public WeaponBarrel barrel;public CharacterBody Owner;public WeaponEffects Effects;
 public WeaponAmmunition ammunition;public WeaponComponentMagazine ComponentMagazine;public WeaponArmConfiguration armConfiguration;
 public string calibre;public bool fireFromMagazine,useInternalMagazine,manualCycling,IsHeld=true,IsRagdoll,IsPrimary=true;
 public float roundsPerMinute,MalfunctionsMultiplier=1,recoilKickModifier,recoilClimbModifier,weight;
 public float staticWeight,recoilToWeightFactor=200,recoilSuppressorModifier=1,recoilPeriod,kickValue,climbValue;
 public float boltManipulationTransitionTime;public bool leftHandBoltManipulation,leftHandBoltClose;public BoltReleaseAnimation boltReleaseAnimation;public Vector3 boltReleasePosition,magazineInsertTrack;
 public Transform rightHandAnchor,leftHandAnchor,magazineTransform,boltHandleTransform,Muzzle;
 public List<FireMode> fireModes=[];public int fireModeIndex,burstCount,consecutiveRounds,burstRoundsRemaining;
 public bool HammerReady=true;public HammerUnsupported hammer=null;public ChamberState ChamberStatus;public Magazine Magazine{get;private set;}
 public AnimationCurve recoilCycle,recoilClimbCycle;readonly List<Recoil> recoilList=[];
 public float TotalWeight=>staticWeight+MagazineWeight();public float RecoilKick=>kickValue;public float RecoilClimb=>climbValue;
 public FireMode ActiveFireMode=>fireModes[fireModeIndex];public WeaponBrace type=WeaponBrace.Shoulder;
 public UnityEngine.Events.UnityEvent onNewMagazine=new();
 public PropagatingAudioSample audioFiringPin,audioBoltExtract,audioBoltReturn,audioInsertMagazine,audioExtractMagazine;
 public void Step(){UpdateTriggerMechanism();CalculateRecoil();}
 public void InitializeRecoil()=>recoilPeriod=Mathf.Max(recoilCycle.keys[^1].time,recoilClimbCycle.keys[^1].time);
 public float boreDiameter,CombinedInaccuracy,velocityModifier=1,PrecisionBegin,PrecisionDistance,projectileForceMultiplier=1,direction=1,rifleTwistRate;public bool rifledBarrel;
 public Transform projectilesContainer;public Action<global::Projectile> onSpawnProjectileCallback;
 public ProjectileEvent onSpawnProjectile;
 public MissionView mission=new();
 public void EmitMuzzleAudio(bool supersonic){Effects.Emit("audio","audioFire");if(supersonic)Effects.Emit("audio","audioSupersonic");}
 public void ReturnMagazine()=>throw new NotSupportedException("Internal magazines not active");
}
public sealed class ProjectileEvent {public Action<global::Projectile> callback;public void Invoke(global::Projectile p)=>callback(p);}
public sealed class MissionView {public float LastWeaponFireTimestamp;}
public sealed class WeaponHost {
 JsonElement config;readonly List<global::Projectile> projectiles=[];public RuntimeWorld queryWorld;public int ballisticMask;
 readonly RuntimeWorld world;readonly WeaponBase weapon;readonly CharacterBody body;readonly ChangeMagazine reload;readonly WorkBolt workBolt;WeaponTask task;
 readonly OriginalArmPhysicsHost armPhysics;
 readonly Dictionary<string,Transform> weaponTransforms=[];readonly Dictionary<string,Transform> operatorTransforms=[];readonly Dictionary<string,Transform> magazineTransforms=[];
 public readonly WeaponEffects effects=new();
 public float Kick=>weapon.RecoilKick;public float Climb=>weapon.RecoilClimb;public bool Manipulating=>body.IsManipulatingEquipment;
 public CharacterBody Character=>body;
 readonly List<OriginalOccupantPrefab> sourceOccupants=[];
 public int SourceOccupantCount=>sourceOccupants.Count;
 // Called exclusively by the exact source OccupantSpawner placement/selection
 // bridge. Do not create fabricated test targets or activate unfinished AI.
 public object SpawnSourceOccupant(string payload){
  if(!config.TryGetProperty("armPhysics",out var physics)||
     !config.TryGetProperty("sourceLimbComponents",out var limbs))
   throw new NotSupportedException("Recovered source Occupant prefab physics unavailable");
  using var doc=JsonDocument.Parse(payload);
  var p=doc.RootElement;
  if(p.GetProperty("bodyPrefab").GetString()!="Occupant")
   throw new NotSupportedException("Non-Occupant prefab requires exact source importer");
  var location=p.GetProperty("position");
  var pos=V(location);
  if(!float.IsFinite(pos.x)||!float.IsFinite(pos.y)||!float.IsFinite(pos.z))
   throw new InvalidDataException("Original NPC spawn coordinates invalid");
  world.MakeCurrent();
  var occupant=new OriginalOccupantPrefab(physics,limbs,world);
  occupant.Character.transform.position=pos;
  if(config.TryGetProperty("sourceLimbBallistics",out var ballistics)&&Ballistics!=null)
   occupant.BindSourceBallistics(ballistics,Ballistics,world);
  sourceOccupants.Add(occupant);
  return new {index=sourceOccupants.Count-1,position=new[]{pos.x,pos.y,pos.z},
   limbs=occupant.Character.limbs.Count,
   sourceActorCount=occupant.PhysicsHost.SourceActorCount,
   originalProjectileContactsActive=occupant.PhysicsHost.SourceLimbProjectileContactsActive};
 }

 public static Vector3 V(JsonElement a)=>new(a[0].GetSingle(),a[1].GetSingle(),a[2].GetSingle());
 static Vector3 Vec(JsonElement a)=>new(a.GetProperty("x").GetSingle(),a.GetProperty("y").GetSingle(),a.GetProperty("z").GetSingle());
 public static AnimationCurve Curve(JsonElement c){float F(JsonElement n)=>n.ValueKind==JsonValueKind.String?(n.GetString() switch {"Infinity"=>float.PositiveInfinity,"-Infinity"=>float.NegativeInfinity,var text=>float.Parse(text,System.Globalization.CultureInfo.InvariantCulture)}):n.GetSingle();return new(c.GetProperty("m_Curve").EnumerateArray().Select(k=>new Keyframe(k.GetProperty("time").GetSingle(),k.GetProperty("value").GetSingle(),F(k.GetProperty("inSlope")),F(k.GetProperty("outSlope")))).ToArray()){serializedPreInfinity=2,serializedPostInfinity=2,clampOutside=true};}
 Transform Make(string name)=>new GameObject(name,world).transform;
 void Hierarchy(JsonElement records,Dictionary<string,Transform> map){foreach(var p in records.EnumerateObject()){var t=Make(p.Name);map[p.Name]=t;}
 foreach(var p in records.EnumerateObject()){var f=p.Value;var t=map[p.Name];var pid=f.GetProperty("m_Father").GetProperty("fileID").GetString();if(map.TryGetValue(pid,out var parent))t.SetParent(parent,false);t.localPosition=Vec(f.GetProperty("m_LocalPosition"));t.localScale=Vec(f.GetProperty("m_LocalScale"));var q=f.GetProperty("m_LocalRotation");t.localRotation=new(q.GetProperty("x").GetSingle(),q.GetProperty("y").GetSingle(),q.GetProperty("z").GetSingle(),q.GetProperty("w").GetSingle());}}
 public BallisticRuntimeHost Ballistics {get;private set;}
 public void InitializeBallistics(string json,SceneLoadResult scene){
  Ballistics=new(json,scene,effects,p=>{p.FlightMask=ballisticMask;if(!projectiles.Contains(p))projectiles.Add(p);});
  ballisticMask=Masks.Instance.BallisticTargets;
  if(config.TryGetProperty("sourceLimbBallistics",out var source))
    armPhysics.BindSourceBallistics(source,Ballistics,world);
 }
 public WeaponHost(string json,InputHandler input,RuntimeWorld sourceWorld=null){
  world=sourceWorld??new RuntimeWorld(true,false);world.MakeCurrent();using var d=JsonDocument.Parse(json);var c=d.RootElement;config=c.Clone();var w=c.GetProperty("weapon");Global.Context.InputHandler=input;
  Hierarchy(c.GetProperty("weaponTransforms"),weaponTransforms);Hierarchy(c.GetProperty("operatorTransforms"),operatorTransforms);
  Transform WT(string key)=>weaponTransforms[w.GetProperty(key).GetProperty("fileID").GetString()];
  var wg=weaponTransforms.Values.First(t=>t.parent==null).gameObject;weapon=wg.AddComponent<WeaponBase>();weapon.Effects=effects;
  var bg=operatorTransforms.Values.First(t=>t.parent==null).gameObject;body=bg.AddComponent<CharacterBody>();body.ActiveWeapon=weapon;weapon.Owner=body;body.Equipment=bg.AddComponent<Equipment>();
  foreach(var f in new[]{"roundsPerMinute","recoilKickModifier","recoilClimbModifier","weight","boltManipulationTransitionTime"})typeof(WeaponBase).GetField(f).SetValue(weapon,w.GetProperty(f).GetSingle());
  weapon.staticWeight=weapon.weight;weapon.recoilCycle=Curve(w.GetProperty("recoilCycle"));weapon.recoilClimbCycle=Curve(w.GetProperty("recoilClimbCycle"));weapon.InitializeRecoil();
  weapon.fireModes=c.GetProperty("fireModes").EnumerateArray().Select(x=>(WeaponBase.FireMode)x.GetInt32()).ToList();weapon.burstCount=w.GetProperty("burstCount").GetInt32();weapon.calibre=w.GetProperty("calibre").GetProperty("guid").GetString();
  weapon.leftHandBoltManipulation=w.GetProperty("leftHandBoltManipulation").GetInt32()!=0;weapon.leftHandBoltClose=w.GetProperty("leftHandBoltClose").GetInt32()!=0;weapon.boltReleaseAnimation=(WeaponBase.BoltReleaseAnimation)w.GetProperty("boltReleaseAnimation").GetInt32();weapon.boltReleasePosition=Vec(w.GetProperty("boltReleasePosition"));weapon.magazineInsertTrack=Vec(w.GetProperty("magazineInsertTrack"));
  weapon.rightHandAnchor=WT("rightHandAnchor");weapon.leftHandAnchor=WT("leftHandAnchor");weapon.magazineTransform=WT("magazineTransform");weapon.boltHandleTransform=WT("boltHandleTransform");weapon.Muzzle=WT("muzzleTransform");
  var ammo= AmmunitionLoader.Load(c.GetProperty("ammoPackage").GetRawText(),c.GetProperty("ammoPath").GetString(),world);
  weapon.ammunition=Make("Active source ammunition").gameObject.AddComponent<WeaponAmmunition>();foreach(var field in typeof(global::AmmunitionBase).GetFields())field.SetValue(weapon.ammunition,field.GetValue(ammo));
  weapon.ammunition.casing=Make("Source chamber round").gameObject.AddComponent<Casing>();weapon.ammunition.casing.gameObject.assetObject=true;
  if(c.GetProperty("ammunition").GetProperty("explosionMode").GetInt32()!=0)throw new NotSupportedException("Explosive projectiles are not ported");
  weapon.projectilesContainer=Make("Original projectiles container");weapon.ammunition.projectile=Make("Original projectile transport prefab").gameObject.AddComponent<global::Projectile>();weapon.ammunition.projectile.meshTransform=Make("Original projectile mesh pose");weapon.ammunition.projectile.meshTransform.SetParent(weapon.ammunition.projectile.transform,false);weapon.ammunition.projectile.gameObject.assetObject=true;
  weapon.onSpawnProjectile=new(){callback=p=>{p.FlightMask=ballisticMask;projectiles.Add(p);effects.Emit("shot",new{id=p.Id,position=new[]{p.transform.position.x,p.transform.position.y,p.transform.position.z},direction=new[]{p.transform.forward.x,p.transform.forward.y,p.transform.forward.z},velocity=p.Velocity,energy=p.Energy,muzzle=new[]{weapon.Muzzle.position.x,weapon.Muzzle.position.y,weapon.Muzzle.position.z},rotation=new[]{weapon.Muzzle.rotation.x,weapon.Muzzle.rotation.y,weapon.Muzzle.rotation.z,weapon.Muzzle.rotation.w}});}};
  weapon.boreDiameter=w.GetProperty("boreDiameter").GetSingle();weapon.CombinedInaccuracy=w.GetProperty("inaccuracy").GetSingle()+weapon.ammunition.inaccuracy;weapon.rifledBarrel=w.GetProperty("rifledBarrel").GetInt32()!=0;weapon.rifleTwistRate=w.GetProperty("rifleTwistRate").GetSingle();
  var m=c.GetProperty("magazine");Hierarchy(c.GetProperty("magazineTransforms"),magazineTransforms);weapon.ComponentMagazine=magazineTransforms.Values.First(t=>t.parent==null).gameObject.AddComponent<WeaponComponentMagazine>();weapon.ComponentMagazine.capacity=m.GetProperty("capacity").GetInt32();weapon.ComponentMagazine.weight=m.GetProperty("weight").GetSingle();weapon.ComponentMagazine.malfunctionRate=m.GetProperty("malfunctionRate").GetSingle();weapon.ComponentMagazine.Effects=effects;weapon.ComponentMagazine.ikTarget=magazineTransforms[m.GetProperty("ikTarget").GetProperty("fileID").GetString()];
  Transform ForGO(JsonElement records,string go)=>weaponTransforms[records.EnumerateObject().First(x=>x.Value.GetProperty("m_GameObject").GetProperty("fileID").GetString()==go).Name];
  var b=c.GetProperty("bolt");weapon.cyclingBolt=ForGO(c.GetProperty("weaponTransforms"),b.GetProperty("m_GameObject").GetProperty("fileID").GetString()).gameObject.AddComponent<CyclingBolt>();
  var cb=weapon.cyclingBolt;cb.chamber=weaponTransforms[b.GetProperty("chamber").GetProperty("fileID").GetString()];cb.chargingHandle=weaponTransforms[b.GetProperty("chargingHandle").GetProperty("fileID").GetString()];cb.cycle=Curve(b.GetProperty("cycle"));
  foreach(var key in new[]{"travelRange","ejectPosition","feedPosition"})typeof(CyclingBolt).GetField(key).SetValue(cb,b.GetProperty(key).GetSingle());
  cb.lockOpen=b.GetProperty("lockOpen").GetInt32()!=0;cb.manualLockOpen=b.GetProperty("manualLockOpen").GetInt32()!=0;
  var br=c.GetProperty("barrel");weapon.barrel=ForGO(c.GetProperty("weaponTransforms"),br.GetProperty("m_GameObject").GetProperty("fileID").GetString()).gameObject.AddComponent<WeaponBarrel>();var barrel=weapon.barrel;
  barrel.chamber=cb.chamber;barrel.magazine=weapon.magazineTransform;barrel.animateChamber=br.GetProperty("animateChamber").GetInt32()!=0;barrel.feedOffset=Curve(br.GetProperty("feedOffset"));barrel.feedAngle=Curve(br.GetProperty("feedAngle"));
  weapon.audioFiringPin=new(effects,"audioFiringPin");weapon.audioBoltExtract=new(effects,"audioBoltExtract");weapon.audioBoltReturn=new(effects,"audioBoltReturn");
  weapon.audioInsertMagazine=new(effects,"audioInsertMagazine");weapon.audioExtractMagazine=new(effects,"audioExtractMagazine");
  // Scoped test loadout, not a claim about the original saved player's equipment.
  body.Equipment.primaryWeapon=weapon;body.Equipment.primaryMagazineCount=1;body.Equipment.Initialize();
  weapon.InsertMagazine(new Magazine(weapon));body.Equipment.AddMagazineForWeapon(weapon.Magazine,weapon);
  world.Invoke(barrel,"Awake");world.Invoke(cb,"Awake");cb.Initialize(weapon,weapon.OnBoltOpened,weapon.OnBoltClosed,w.GetProperty("boltFeedMalfunctionRate").GetSingle());
  Transform OT(string key)=>operatorTransforms[c.GetProperty("bodyTransforms").GetProperty(key).GetString()];
  body.chestRigIKTarget=OT("chestRigIKTarget");body.pistolPivot=OT("pistolPivot");body.pistolIKTarget=OT("pistolIKTarget");body.Torso=OT("Torso").gameObject.AddComponent<Limb>();body.Pelvis=OT("Pelvis").gameObject.AddComponent<Limb>();
  var ac=c.GetProperty("armConfiguration");weapon.armConfiguration=new(){rightHandMode=(Hand.Mode)ac.GetProperty("rightHandMode").GetInt32(),leftHandMode=(Hand.Mode)ac.GetProperty("leftHandMode").GetInt32(),rightHandAngle=ac.GetProperty("rightHandAngle").GetSingle(),leftHandAngle=ac.GetProperty("leftHandAngle").GetSingle()};
  // Source WeaponBase.Initialize configures grips before the IK solvers target the weapon.
  Vector3 GripOffset(string key){var offset=ac.GetProperty(key);return new(offset.GetProperty("x").GetSingle(),offset.GetProperty("y").GetSingle(),0);}
  weapon.rightHandAnchor.localPosition=GripOffset("rightHandOffset");
  weapon.leftHandAnchor.localPosition=GripOffset("leftHandOffset");
  ArmIKSolver Arm(bool right){
   var rig=c.GetProperty("armRigBindings").GetProperty(right?"right":"left");
   Transform Node(string key)=>operatorTransforms[rig.GetProperty(key).GetString()];
   Limb Bone(string key){var node=Node(key);var limb=node.GetComponent<Limb>()??node.gameObject.AddComponent<Limb>();limb.Body=body;return limb;}
   var a=Node("solverTransform").gameObject.AddComponent<ArmIKSolver>();
   var settings=rig.GetProperty("settings");a.interpolation=Curve(settings.GetProperty("interpolation"));
   a.flip=settings.GetProperty("flip").GetInt32()!=0;a.smoothing=settings.GetProperty("smoothing").GetInt32()!=0;a.smoothFactor=settings.GetProperty("smoothFactor").GetSingle();
   a.upperArm=Bone("upperTransform");a.lowerArm=Bone("lowerTransform");Bone("handTransform");
   a.hand=Node("handTransform").gameObject.AddComponent<Hand>();
   a.hand.grabPoint=operatorTransforms[rig.GetProperty("handComponent").GetProperty("grabPoint").GetProperty("fileID").GetString()];
   foreach(var variant in rig.GetProperty("variants").EnumerateObject()){
    var value=variant.Value;var go=operatorTransforms[value.GetProperty("transform").GetString()].gameObject;
    go.SetActive(value.GetProperty("active").GetBoolean());a.hand.SourceVisuals[value.GetProperty("gameObject").GetString()]=go;
    switch(variant.Name){case "fist":a.hand.fist=go;break;case "trigger":a.hand.trigger=go;break;case "forwardAssist":a.hand.forwardAssist=go;break;}
   }
   a.InitializeSourcePose();a.SetOnWeapon(weapon,.1f,right);return a;
  }
  body.rightArm=Arm(true);body.leftArm=Arm(false);
  var limbSource=c.TryGetProperty("sourceLimbComponents",out var limbConfiguration)?limbConfiguration:default;
  armPhysics=new OriginalArmPhysicsHost(c.GetProperty("armPhysics"),operatorTransforms,body,world,limbSource);
  if(c.TryGetProperty("sourceSkinMasks",out var actualOriginalSkinMasks))
   armPhysics.BindOriginalSkinCoverage(actualOriginalSkinMasks);
  reload=bg.AddComponent<ChangeMagazine>();reload.isPlayer=true;reload.OnAwake();workBolt=bg.AddComponent<WorkBolt>();workBolt.OnAwake();effects.events.Clear();
 }
 public void BindOriginalLimbPhysiology(){
  armPhysics.BindOriginalPhysiology();
  foreach(var limb in body.limbs.Where(l=>l.SourceSkinTrace!=null)){
   limb.SourceDrawDamage=(source,position,radius,depth,irregularity)=>{
    // This is the original LimbRenderer.DrawDamage request, carrying the
    // actual authored limb id and exact Unity world-space contact point.
    // Browser presentation must render it against source skin material.
    effects.Emit("sourceLimbDamage",new {
     limb=(int)source.id,point=new[]{position.x,position.y,position.z},
     radiusMillimeters=radius,depth,irregularity
    });
   };
  }
 }
 public object Step(string json){world.MakeCurrent();using var d=JsonDocument.Parse(json);var f=d.RootElement;
  body.Direction=f.GetProperty("direction").GetSingle();body.transform.position=V(f.GetProperty("position"));body.transform.rotation=Quaternion.AngleAxis(body.RotationY,Vector3.up);
  if(f.TryGetProperty("velocity",out var sourceVelocity)&&body.GetComponent<Rigidbody>() is Rigidbody rootRigidbody)
   rootRigidbody.velocity=V(sourceVelocity);
  foreach(var pair in f.GetProperty("operatorTransforms").EnumerateObject())if(operatorTransforms.TryGetValue(pair.Name,out var target))SetWorld(target,pair.Value);
  SetWorld(weapon.transform,f.GetProperty("weaponPose"));
  if(body.Kinematic){body.rightArm.Step();body.leftArm.Step();}
  armPhysics.Step(f,world);
  // Original CharacterBody.QueryTrigger clears one-frame requests; manipulation blocks new pulls.
  if(body.Kinematic&&f.GetProperty("fire").GetBoolean()&&!body.IsManipulatingEquipment)body.PullTrigger();
  if(body.Kinematic&&task==null&&f.GetProperty("reload").GetBoolean()){body.PendingAction=CharacterBody.CharacterAction.SwitchMagazine;task=reload;task.OnStart();}
  if(task!=null&&task.OnUpdate()!=TaskStatus.Running){task.OnEnd();task=null;}
  if(task==null&&body.PendingAction==CharacterBody.CharacterAction.WorkBolt){task=workBolt;task.OnStart();}
  weapon.Step();world.Invoke(weapon.cyclingBolt,"Update");body.QueryTrigger();
  queryWorld?.MakeCurrent();Ballistics?.Step();
  foreach(var p in projectiles.ToArray()){if(!p.gameObject.activeInHierarchy)continue;p.StepFlight();effects.Emit("flight",new{id=p.Id,position=new[]{p.transform.position.x,p.transform.position.y,p.transform.position.z},finished=p.Finished});if(p.Finished)projectiles.Remove(p);}
  world.FlushDestroyed();var result=Snapshot();effects.events.Clear();return result;
 }
 static void SetWorld(Transform t,JsonElement p){t.position=V(p.GetProperty("p"));var q=p.GetProperty("q");t.rotation=new(q[0].GetSingle(),q[1].GetSingle(),q[2].GetSingle(),q[3].GetSingle());}
 public object Snapshot(){var mp=weapon.ComponentMagazine.transform.position;var mq=weapon.ComponentMagazine.transform.rotation;object Pose(Transform t)=>new{p=new[]{t.position.x,t.position.y,t.position.z},q=new[]{t.rotation.x,t.rotation.y,t.rotation.z,t.rotation.w}};
  object Arm(ArmIKSolver a)=>new{target=new[]{a.ikTarget.position.x,a.ikTarget.position.y,a.ikTarget.position.z},a.OnTarget,a.OnWeapon,a.HasSolution,smoothing=a.smoothing||a.Transitioning,handMode=(int)a.hand.CurrentMode,angle=a.hand.transform.localEulerAngles.z,
   upper=a.upperArm.transform.eulerAngles.z,lower=a.lowerArm.transform.eulerAngles.z,
   bones=new{upper=Pose(a.upperArm.transform),lower=Pose(a.lowerArm.transform),hand=Pose(a.hand.transform)},
   handObjects=a.hand.SourceVisuals.Select(pair=>new{id=pair.Key,active=pair.Value.activeSelf}).ToArray()};// Presentation timing must use the same scaled Unity Time.time as the recovered weapon/effect events.
  return new{sourceTime=Time.time,sourcePhysiology=body.SourcePhysiology==null?null:new{body.SourcePhysiology.Alive,body.SourcePhysiology.Conscious,body.SourcePhysiology.BloodVolume,body.SourcePhysiology.BloodOxygen,body.SourcePhysiology.HeartRate,body.SourcePhysiology.RespirationRate,body.SourcePhysiology.BrainTrauma,
   body.SourcePhysiology.Shock,body.SourcePhysiology.LungsOxygen,
   WoundVolume=body.limbs.Sum(l=>l.LimbPhysiology?.WoundCavityVolume??0f),
   Hemorrhaging=body.limbs.Sum(l=>l.LimbPhysiology?.InternalHemorrhaging??0f),
   limbCount=body.limbs.Count,
   originalSkinRenderers=body.limbs.Count(l=>l.SourceSkinTrace!=null),
   originalOrganGroups=body.limbs.Count(l=>l.SourceVitals!=null),
   originalOrgans=body.limbs.Sum(l=>l.SourceVitals?.Functions.Count??0),
   body.SourcePhysiology.Paralyzed,body.Kinematic,body.CrippledRatio},ballistics=Ballistics?.Snapshot(),magazinePose=new{p=new[]{mp.x,mp.y,mp.z},q=new[]{mq.x,mq.y,mq.z,mq.w},visible=weapon.ComponentMagazine.gameObject.activeSelf},shots=body.ShotsFired,
   // The only currently executable original weapon host is the recovered M4A1.
   // Send source-owned magazine capacity and chamber state to the browser.
   // Future weapon hosts must supply their own actual equipped weapon identity;
   // a merely loaded Glock visual is NOT a playable equipped Glock.
   physicalArms=armPhysics.Snapshot(),
    sourceLimbBallistics=new{materials=armPhysics.SourceLimbMaterialCount,
       colliders=armPhysics.SourceLimbColliderCount,armors=armPhysics.SourceLimbArmorCount,
       projectileContactsActive=armPhysics.SourceLimbProjectileContactsActive},
    weaponId="M4A1",magazineCapacity=weapon.Magazine?.capacity??weapon.ComponentMagazine.capacity,
   magazine=weapon.Magazine?.roundCount??0,reserve=body.Equipment.PrimaryMagazines.Where(m=>!ReferenceEquals(m,weapon.Magazine)).Sum(m=>m.roundCount),chamberLive=weapon.cyclingBolt.Live,bolt=weapon.cyclingBolt.Status.ToString(),reloading=task!=null,manipulating=body.IsManipulatingEquipment,kick=Kick,climb=Climb,right=Arm(body.rightArm),left=Arm(body.leftArm),events=effects.events.ToArray()};}
}
}

