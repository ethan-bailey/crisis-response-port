using System.Reflection;
using System.Text.Json;
using Crisis.Runtime;
using Crisis.Runtime.Character;
using Crisis.Runtime.Weapons;
using UnityEngine;

// Diagnostic observers only. They are never installed in WeaponHost or live
// NPCs. Every original presentation request is recorded for order assertions.
sealed class HitEffectsObserver : IOriginalCharacterHitEffects {
 public readonly List<string> Calls=new();
 public bool Ready=true;
 public float InjuryTime;
 public void ValidateBindings(){if(!Ready)throw new NotSupportedException("Diagnostic unbound hit context");}
 public float FirstInjuryTimestamp {set{InjuryTime=value;Calls.Add("mission");}}
 public bool AllRoomsChecked=>false;
 public bool HasOccupants=>false;
 public bool AllOccupantsRenderedSafe=>false;
 public bool ShowInteractionMarkers=>true;
 public bool VoicePlaying=>false;
 public void InvokeBulletTimeOnHit(bool player)=>Calls.Add("bulletTime");
 public void ShowStabilizeMarker(bool show)=>Calls.Add("stabilize");
 public void Hurt(float delay)=>Calls.Add("hurt");
 public void DropWeapons(bool keep)=>Calls.Add("dropWeapons");
}
sealed class MarksmanAttackObserver : Component,IOriginalMarksmanAttack {
 public int Calls;public CharacterBody LastVictim;
 public void ValidateBindings(){}
 public void OnSuccessfulAttack(CharacterBody victim){Calls++;LastVictim=victim;}
}
public static class ProjectileInjuryContracts {
 static readonly MethodInfo handle=typeof(Projectile).GetMethod("HandleLimbHit",BindingFlags.Instance|BindingFlags.NonPublic);
 static void Check(bool b,string s){if(!b)throw new Exception(s);}
 static BallisticResult Hit(Projectile p,Limb l,Vector3? hitPoint=null){
  try{return (BallisticResult)handle.Invoke(p,new object[]{new RaycastHit{collider=l.GetComponent<BoxCollider>(),point=hitPoint??(l.transform.position+new Vector3(.1f,.1f,-.01f)),normal=new Vector3(-1f,0f,0f)}});}
  catch(TargetInvocationException e){throw e.InnerException;}
 }
 public static string Run(string weaponJson=null,string materialJson=null,string skinJson=null){
  var passed=new List<string>();
  var world=new RuntimeWorld();
  // An empty second world advances the shared source clock without running
  // unsupported scene lifecycle on the deliberately bounded test actors.
  var clock=new RuntimeWorld(false,false);
  void Advance(){clock.Tick(.02f);world.MakeCurrent();}
  Advance();
  PerformanceManager.Instance=new PerformanceManager{MaxEnvironmentHitEffects=0};
  MissionHelper.Instance.EffectsContainer=new GameObject("original effects root",world).transform;
  CharacterBody Actor(string name,out HitEffectsObserver observer){
   var go=new GameObject(name,world);var body=go.AddComponent<CharacterBody>();
   go.AddComponent<Animator>();go.AddComponent<Rigidbody>();
   body.rightArm=new GameObject("right solver",world).AddComponent<ArmIKSolver>();
   body.leftArm=new GameObject("left solver",world).AddComponent<ArmIKSolver>();
   body.InitializeOriginalPhysiology(Time.time,4500f,0f,0f);
   observer=new HitEffectsObserver();
   body.InitializeOriginalCombat(go.transform,100f,true,observer);
   return body;
  }
  var owner=Actor("original shooter",out var ownerEffects);
  var target=Actor("original target",out var effects);
  var second=Actor("second original target",out var secondEffects);
  // Load the actual recovered M4 ammunition, including original expansion and
  // cavity curves, instead of filling AmmunitionBase defaults for this test.
  if(weaponJson==null){
   using var fs=File.OpenRead("game/assets/weapon-runtime.json.gz");
   using var gz=new System.IO.Compression.GZipStream(fs,System.IO.Compression.CompressionMode.Decompress);
   using var reader=new StreamReader(gz);weaponJson=reader.ReadToEnd();
  }
  using var weaponData=JsonDocument.Parse(weaponJson);
  var config=weaponData.RootElement;
  var originalAmmo=AmmunitionLoader.Load(config.GetProperty("ammoPackage").GetRawText(),config.GetProperty("ammoPath").GetString(),world);
  var weapon=new GameObject("source M4",world).AddComponent<WeaponBase>();weapon.Owner=owner;
  var ammo=new GameObject("source ammunition",world).AddComponent<WeaponAmmunition>();
  foreach(var f in typeof(AmmunitionBase).GetFields())f.SetValue(ammo,f.GetValue(originalAmmo));
  weapon.ammunition=ammo;weapon.rifledBarrel=true;weapon.rifleTwistRate=7f;
  using var data=JsonDocument.Parse(materialJson??File.ReadAllText("game/assets/original-limb-ballistic-materials.json"));
  var material=data.RootElement.GetProperty("materials").GetProperty("d16aa32f896a9634581f616d9a27cfc4").GetProperty("fields");
  Limb Torso(CharacterBody actor){
   var go=new GameObject("original torso",world);go.transform.SetParent(actor.transform,false);go.layer=15;
   var limb=go.AddComponent<Limb>();limb.Body=actor;limb.id=LimbId.Torso;actor.Torso=limb;actor.limbs.Add(limb);
   limb.LimbPhysiology=new OriginalLimbPhysiology(actor.SourcePhysiology);
   go.AddComponent<Rigidbody>().isKinematic=true;
   var box=go.AddComponent<BoxCollider>();box.size=new Vector3(.5f,.5f,.5f);
   var prop=go.AddComponent<BallisticMaterialProperties>();
   prop.surfaceDensity=material.GetProperty("surfaceDensity").GetSingle();prop.surfaceThickness=material.GetProperty("surfaceThickness").GetSingle();
   prop.density=material.GetProperty("density").GetSingle();prop.maximumDeflection=material.GetProperty("maximumDeflection").GetSingle();
   var range=material.GetProperty("densityRandomness");prop.densityRandomness=new(range.GetProperty("x").GetSingle(),range.GetProperty("y").GetSingle());
   limb.properties=prop;limb.depth=.55f;limb.Awake();limb.Start();return limb;
  }
  var torso=Torso(target);var otherTorso=Torso(second);var selfTorso=Torso(owner);
  Projectile Shot(int swarm){
   var p=new GameObject("source M4 projectile",world).AddComponent<Projectile>();
   p.meshTransform=new GameObject("source projectile mesh",world).transform;p.meshTransform.SetParent(p.transform,false);
   p.transform.rotation=Quaternion.LookRotation(new Vector3(1f,0f,.2f));
   p.ForceMultiplier=.015f;p.SwarmId=swarm;p.InitializeXY(weapon,false);return p;
  }
  var projectile=Shot(17);
  // All mandatory context is checked before tissue, energy, position or stats.
  effects.Ready=false;var initial=projectile.Energy;var position=projectile.transform.position;
  try{Hit(projectile,torso);throw new Exception("Unbound hit context was accepted");}catch(NotSupportedException){}
  Check(torso.LimbPhysiology.Wounds.Count==0&&projectile.Energy==initial&&(projectile.transform.position-position).sqrMagnitude==0f&&owner.ShotsHit==0,"Preflight mutated injury/energy/bookkeeping");
  effects.Ready=true;
  Hit(projectile,selfTorso);
  Check(selfTorso.LimbPhysiology.Wounds.Count==0&&owner.ShotsHit==0&&projectile.FirstHitCharacter==null,"Self hit changed injury or success credit");
  passed.Add("Missing mandatory dependencies and self-hits do not mutate tissue, projectile or combat state");
  var order=new List<string>();
  target.EventHit.AddListener(()=>{if(target.LastHitLimb==torso){Check(projectile.FirstHitCharacter==target,"First-hit update must precede victim hit");order.Add("victim");}});
  projectile.onInteraction.AddListener(p=>{Check(target.HitCount.Contains(17),"Projectile event preceded victim HitCount");order.Add("interaction");});
  projectile.onLimbInteraction.AddListener((p,l)=>{Check(l==torso,"Wrong limb event");order.Add("limb");});
  // Original missing-renderer route has zero available internal distance. It
  // tests callback order only, never claims original visible skin penetration.
  Hit(projectile,torso);
  Check(order.SequenceEqual(new[]{"victim","interaction","limb"}),"Original dispatcher order changed: "+string.Join(',',order));
  Check(owner.ShotsHit==1&&owner.LastAttackTarget==target&&target.LastHitBy==owner&&target.LastHitLimb==torso&&target.LastHitTimestamp==Time.time,"Original attack/victim state was not recorded");
  Check(effects.Calls.Take(2).SequenceEqual(new[]{"mission","bulletTime"}),"Original OnWasHit mission/bullet-time order changed");
  passed.Add("Original RecordAttackInfo, LastHitLimb, OnWasHit and both events execute in source order");
  // Source FirstHitCharacter is assigned on EVERY victim, despite its name.
  projectile.onInteraction=new global::ProjectileEvent();projectile.onLimbInteraction=new ProjectileLimbEvent();
  Hit(projectile,otherTorso);
  Check(owner.ShotsHit==1&&projectile.FirstHitCharacter==second&&second.LastHitBy==owner,"Penetrating second victim duplicated shooter credit or failed victim notification");
  var sameSwarm=Shot(17);Hit(sameSwarm,otherTorso);Check(owner.ShotsHit==1,"Same swarm duplicated original ShotsHit");
  var nextSwarm=Shot(18);Hit(nextSwarm,otherTorso);Check(owner.ShotsHit==2,"New swarm did not increment original ShotsHit");
  passed.Add("Per-projectile first-contact credit and original per-swarm shot accounting");
  // Actual source scorecard formula: compliant first hit .1, rapid next hit
  // .1 * (.2 / 2); stray halves the latter. Facing/range are original logic.
  var penaltyTarget=Actor("compliant target",out var penaltyEffects);penaltyTarget.IsCompliant=true;
  Check(!penaltyTarget.IsCompliant,"Source compliance must retain its half-second reaction margin");
  for(int i=0;i<30;i++)Advance();
  Check(penaltyTarget.IsCompliant,"Source compliance did not settle after the reaction margin");
  var penalty=Shot(19);penalty.transform.position=new Vector3(-2f,0f,0f);
  penaltyTarget.OnWasAttacked(penalty);
  penalty.IsStray=true;penaltyTarget.OnWasAttacked(penalty);
  Check(Math.Abs(penaltyTarget.ScoreCard.Compliant-.105f)<1e-6f&&penaltyTarget.ScoreCard.CompliantCount==2&&penaltyTarget.ScoreCard.UncheckedCount==2,"Source score penalties/visibility/burst modifier mismatch");
  passed.Add("Original compliance timing, scorecard visibility, penalties, burst and stray modifiers");
  // Verify the query skips owner colliders and still returns nearest eligible
  // limb, respecting masks. This queries real adapter geometry, not a hit stub.
  selfTorso.transform.position=new Vector3(1f,0f,0f);torso.transform.position=new Vector3(2f,0f,0f);otherTorso.transform.position=new Vector3(3f,0f,0f);
  var query=typeof(Projectile).GetMethod("FlightRaycast",BindingFlags.Instance|BindingFlags.NonPublic);
  object[] queryArgs={new Ray(Vector3.zero,Vector3.right),default(RaycastHit),10f,1<<15};
  Check((bool)query.Invoke(projectile,queryArgs)&&((RaycastHit)queryArgs[1]).collider.GetComponent<Limb>()==torso,"Owner filtering lost nearest distinct target");
  queryArgs[3]=1<<14;Check(!(bool)query.Invoke(projectile,queryArgs),"Flight query ignored layer mask");
  passed.Add("Nearest real non-owner collision and layer-mask exclusion");
  // Original Limb.AccumulateExitingProjectiles has a precise 16.7ms window,
  // and the first exit after a gap resets accumulation. Exercise its exact
  // state transition without claiming unavailable GPU exit-wound rendering.
  var exiting=Shot(23);exiting.DisplacedMaterialVolume=30f;
  var accumulator=typeof(Limb).GetMethod("AccumulateExitingProjectiles",BindingFlags.Instance|BindingFlags.NonPublic);
  var exitVolume=typeof(Limb).GetField("accumulatedExitVolume",BindingFlags.Instance|BindingFlags.NonPublic);
  Check(accumulator!=null&&exitVolume!=null,"Original exit accumulation source was not imported");
  accumulator.Invoke(torso,new object[]{exiting});
  Check((float)exitVolume.GetValue(torso)==0f,"First exit after gap must clear accumulated volume");
  accumulator.Invoke(torso,new object[]{exiting});
  Check((float)exitVolume.GetValue(torso)==30f,"Second exit inside 16.7ms did not accumulate source displaced volume");
  exiting.DisplacedMaterialVolume=3000f;
  accumulator.Invoke(torso,new object[]{exiting});
  Check((float)exitVolume.GetValue(torso)==1000f,"Original consecutive exit accumulation exceeded 1000 source cap");
  Advance();
  accumulator.Invoke(torso,new object[]{exiting});
  Check((float)exitVolume.GetValue(torso)==0f,"Exit accumulation did not reset after source time window");
  passed.Add("Original consecutive exit accumulation, 16.7ms reset and 1000 source cap");
  // Marksman path exercises authored depth and real recovered Torso PNG alpha.
  // Actor placement remains a controlled fixture, not a live mission target.
  var penetrating=Shot(20);var marksman=new GameObject("diagnostic marksman notification observer",world).AddComponent<MarksmanAttackObserver>();penetrating.MarksmanOwner=marksman;
  var injuryTarget=Actor("source wound target",out var injuryEffects);var injuryTorso=Torso(injuryTarget);
  if(skinJson==null){
   using var sf=File.OpenRead("game/assets/operator/original-source-limb-damage-mask.json.gz");
   using var sg=new System.IO.Compression.GZipStream(sf,System.IO.Compression.CompressionMode.Decompress);
   using var sr=new StreamReader(sg);skinJson=sr.ReadToEnd();
  }
  using var masks=JsonDocument.Parse(skinJson);
  var skin=masks.RootElement.GetProperty("skins").EnumerateArray().Single(s=>
   s.GetProperty("sourceRenderers").EnumerateArray().Any(r=>r.GetProperty("transformFileID").GetString()=="4617365563186364"));
  // Exact recovered Operator Torso renderer ID and original PNG SHA256.
  Check(skin.GetProperty("sha256").GetString()=="b361dfbd06cc0d25ef057ca7b30d8367abb47f3f4acd78bbdf5c93ab1d590a5f","Wrong original Torso skin asset");
  int sw=skin.GetProperty("width").GetInt32(),sh=skin.GetProperty("height").GetInt32();
  var green=Convert.FromBase64String(skin.GetProperty("originalStaticDamageGreenBottomUpBase64").GetString());
  var solid=Enumerable.Range(0,green.Length).Where(i=>green[i]==0)
   .OrderBy(i=>Math.Abs(i%sw-sw/2)+Math.Abs(i/sw-sh/2)).First();
  var sourceHit=new Vector3((solid%sw+.5f)/sw-.5f,(solid/sw+.5f)/sh-.5f,0f);
  injuryTorso.SourceSkinTrace=new OriginalLimbSkinTrace(new GameObject("recovered Torso skin fixture plane",world).transform,sw,sh,green);
  var forces=new List<ForceMode>();injuryTorso.Rigidbody.PhysicsForceSink=(force,point,mode)=>forces.Add(mode);
  int unconscious=0,death=0,ragdoll=0;injuryTarget.EventUnconscious.AddListener(()=>unconscious++);injuryTarget.EventDeath.AddListener(()=>death++);injuryTarget.EventRagdoll.AddListener(()=>ragdoll++);
  float beforeEnergy=penetrating.Energy;Hit(penetrating,injuryTorso,sourceHit);
  Check(penetrating.Energy<beforeEnergy&&injuryTorso.LimbPhysiology.WoundCavityVolume>0f&&injuryTorso.LimbPhysiology.Wounds.Count==1&&marksman.Calls==1,"Original M4 energy-to-source-wound chain failed");
  var pstate=injuryTarget.SourcePhysiology;float blood=pstate.BloodVolume,oxygen=pstate.BloodOxygen;
  float now=Time.time;
  for(int i=0;i<15000&&pstate.Alive;i++){Advance();now+=.02f;injuryTarget.StepOriginalPhysiology(.02f,now,0f,3.2f,heartRandom:0f);}
  Check(pstate.BloodVolume<blood&&pstate.BloodOxygen<oxygen&&!pstate.Conscious&&!pstate.Alive,"Source sustained wound did not cause blood/oxygen loss, unconsciousness and eventual death");
  Check(unconscious==1&&death==1&&ragdoll==1&&!injuryTorso.Rigidbody.isKinematic&&forces.Contains(ForceMode.Impulse),"Source vital transitions duplicated/lost events or injury impulse");
  passed.Add("Actual source M4 ammunition -> tissue cavity/wound -> continuing hemorrhage -> hypoxia/unconsciousness/death and rigidbody impulse");
  var crossing=Actor("same-frame circulation threshold",out var crossingEffects);
  var crossingLimb=Torso(crossing);
  var crossingForces=new List<ForceMode>();
  crossingLimb.Rigidbody.PhysicsForceSink=(f,p,m)=>crossingForces.Add(m);
  crossing.SourcePhysiology.RemoveBlood(2249f);
  crossingLimb.LimbPhysiology.AddTrauma(100f);
  crossing.StepOriginalPhysiology(.02f,Time.time,0f,3.2f,heartRandom:0f);
  Check(crossing.SourcePhysiology.BloodOxygen<.5f&&!crossing.SourcePhysiology.Conscious,
   "Hemorrhage must precede original oxygen/consciousness evaluation in the same frame");
  Check(Mathf.Clamp(-1f,0f,-2f)==0f&&Mathf.Clamp(1f,0f,-2f)==-2f,"Unity inverted-bound clamp semantics differ");
  passed.Add("Unity clamp and source same-frame hemorrhage/oxygen/state update ordering");
  return JsonSerializer.Serialize(new{status="PASS",contracts=passed,liveNpcGameplay=false,sourceSkinFixture=true,originalTorsoSkin=true});
 }
}
