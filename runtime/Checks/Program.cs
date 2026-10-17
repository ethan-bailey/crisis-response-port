using Crisis.Runtime;
using System.Text.Json;

if(args.Contains("--projectile-injury-chain")){Console.WriteLine(ProjectileInjuryContracts.Run());return;}
if(args.Contains("--source-muscle-control")){Console.WriteLine(OriginalMuscleControlContracts.Run());return;}
if(args.Contains("--original-limb-skin")){
 var file="game/assets/operator/original-source-limb-damage-mask.json.gz";
 using var fs=File.OpenRead(file);
 using var gz=new System.IO.Compression.GZipStream(fs,System.IO.Compression.CompressionMode.Decompress);
 using var document=JsonDocument.Parse(gz);
 var masks=document.RootElement;
 if(masks.GetProperty("format").GetString()!="crisis-original-limb-initial-damage-green-v1"||
    masks.GetProperty("originalRendererCount").GetInt32()!=17||
    masks.GetProperty("originalTextureCount").GetInt32()!=11)
  throw new Exception("Original source LimbTextureData mask inventory changed");
 int sampled=0;
 foreach(var skin in masks.GetProperty("skins").EnumerateArray()){
  int width=skin.GetProperty("width").GetInt32(),
      height=skin.GetProperty("height").GetInt32();
  var green=Convert.FromBase64String(skin.GetProperty("originalStaticDamageGreenBottomUpBase64").GetString());
  if(green.Length!=width*height)throw new Exception("Original skin coverage pixel length mismatch");
  var world=new UnityEngine.RuntimeWorld();
  var visual=new UnityEngine.GameObject("Source LimbRenderer",world).transform;
  var tracer=new Crisis.Runtime.Character.OriginalLimbSkinTrace(visual,width,height,green);
  int filled=Array.FindIndex(green,b=>b==0);
  if(filled<0)throw new Exception("Original skin contains no solid texels");
  int x=filled%width,y=filled/width;
  var p=new UnityEngine.Vector3((x+.5f)/width-.5f,(y+.5f)/height-.5f,0f);
  UnityEngine.Vector3 entry=default,exit=default;
  var status=tracer.FindMarksmanEntryAndExit(p,UnityEngine.Vector3.right,ref entry,ref exit);
  if(status!=Crisis.Runtime.Character.OriginalLimbSkinTrace.TraceResult.EntryExit||
     Math.Abs(tracer.InitialDamageGreen(x,y))>1e-5f||
     (exit-entry-UnityEngine.Vector3.right*.2f).magnitude>1e-5f)
   throw new Exception("Original LimbRenderer pixel/marksman source trace mismatch");
  int transparent=Array.FindIndex(green,b=>b==255);
  if(transparent>=0){
   int tx=transparent%width,ty=transparent/width;
   var q=new UnityEngine.Vector3((tx+.5f)/width-.5f,(ty+.5f)/height-.5f,0f);
   status=tracer.FindMarksmanEntryAndExit(q,UnityEngine.Vector3.right,ref entry,ref exit);
   if(status!=Crisis.Runtime.Character.OriginalLimbSkinTrace.TraceResult.Failure)
    throw new Exception("Original alpha-derived transparent pixel falsely registers as solid");
  }
  sampled++;
 }
 if(sampled!=11)throw new Exception("Not all recovered source skin masks tested");
 Console.WriteLine("{\"originalLimbSkinTrace\":\"pass\",\"sourceSkinTextures\":11,\"sourceRenderers\":17,\"nativePixelTrace\":true}");
 return;
}
if(args.Contains("--source-tissue-penetration")){
 var world=new UnityEngine.RuntimeWorld();
 world.MakeCurrent();
 PerformanceManager.Instance=new PerformanceManager{MaxEnvironmentHitEffects=0};
 MissionHelper.Instance.EffectsContainer=new UnityEngine.GameObject("source impact effects",world).transform;
 var owner=new UnityEngine.GameObject("source impact shooter",world)
    .AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
 var source=new UnityEngine.GameObject("source M4-like ballistic fixture",world)
    .AddComponent<Crisis.Runtime.Weapons.WeaponBase>();
 source.Owner=owner;
 source.rifledBarrel=true;source.rifleTwistRate=7f;
 var ammo=new UnityEngine.GameObject("source default ammo fixture",world)
    .AddComponent<Crisis.Runtime.Weapons.WeaponAmmunition>();
 // Source AmmunitionBase default velocity/mass/calibre, and the same genuine
 // class energy/friction laws, with an explicit constant test expansion curve.
 ammo.expansionFunction=new UnityEngine.AnimationCurve(
     new UnityEngine.Keyframe(0f,1f,0f,0f),
     new UnityEngine.Keyframe(1f,1f,0f,0f)){clampOutside=true};
 source.ammunition=ammo;
 var target=new UnityEngine.GameObject("source Torso target",world)
    .AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
 var limb=target.gameObject.AddComponent<Crisis.Runtime.Weapons.Limb>();
 limb.Body=target;limb.id=Crisis.Runtime.Weapons.LimbId.Torso;
 limb.depth=.55f; // extracted source Operator Torso
 var physiology=new Crisis.Runtime.Character.OriginalPhysiologyConditions();
 physiology.InitializeBloodVolume(4500f);
 physiology.InitializeVitalClock(0f,0f,0f);
 physiology.SetBloodPressure(100f,190f,1f,.5f);
 limb.LimbPhysiology=new Crisis.Runtime.Character.OriginalLimbPhysiology(physiology);
 target.gameObject.AddComponent<UnityEngine.Rigidbody>().isKinematic=true;
 limb.SourceSkinTrace=new Crisis.Runtime.Character.OriginalLimbSkinTrace(
   new UnityEngine.GameObject("full skin fixture",world).transform,32,32,
   Enumerable.Repeat((byte)0,1024).ToArray());
 // The actual original torso flesh material, not test-generated density.
 using var data=JsonDocument.Parse(File.ReadAllText("game/assets/original-limb-ballistic-materials.json"));
 var material=data.RootElement.GetProperty("materials")
   .GetProperty("d16aa32f896a9634581f616d9a27cfc4").GetProperty("fields");
 var properties=target.gameObject.AddComponent<BallisticMaterialProperties>();
 properties.surfaceDensity=material.GetProperty("surfaceDensity").GetSingle();
 properties.surfaceThickness=material.GetProperty("surfaceThickness").GetSingle();
 properties.density=material.GetProperty("density").GetSingle();
 properties.maximumDeflection=material.GetProperty("maximumDeflection").GetSingle();
 var range=material.GetProperty("densityRandomness");
 properties.densityRandomness=new UnityEngine.Vector2(
     range.GetProperty("x").GetSingle(),range.GetProperty("y").GetSingle());
 limb.properties=properties;
 limb.Awake();limb.Start();
 var projectile=new UnityEngine.GameObject("source physical projectile",world)
    .AddComponent<Projectile>();
 projectile.meshTransform=new UnityEngine.GameObject("source projectile mesh",world).transform;
 projectile.meshTransform.SetParent(projectile.transform,false);
 // Marksman-specific source skin route avoids inventing a simulated exit
 // thickness while testing the real recovered Projectile+Limb source code.
 projectile.MarksmanOwner=source;
 projectile.transform.position=new UnityEngine.Vector3(.10f,.10f,-.01f);
 projectile.transform.rotation=UnityEngine.Quaternion.LookRotation(
    new UnityEngine.Vector3(1f,0f,.2f));
 projectile.ForceMultiplier=.015f; // recovered original Projectile.ForceScale
 projectile.InitializeXY(source,false);
 var initialEnergy=projectile.Energy;
 var hitEvent=0;target.EventHit.AddListener(()=>hitEvent++);
 Projectile outcome=limb.HandleProjectile(projectile,new UnityEngine.Vector3(-1f,0f,0f));
 if(outcome!=projectile||!(initialEnergy>projectile.Energy)||
   !(projectile.InteractionEnergy>0f)||!(projectile.DisplacedMaterialVolume>0f)||
   !(limb.LimbPhysiology.WoundCavityVolume>0f)||
   limb.LimbPhysiology.Wounds.Count!=1||hitEvent==0)
  throw new Exception("Original authored torso material interior did not propagate projectile energy through trauma and wounds");
 var bloodBefore=physiology.BloodVolume;
 limb.LimbPhysiology.UpdateHemorrhaging(.02f);
 if(!(physiology.BloodVolume<bloodBefore))
  throw new Exception("Original projectile interior wound did not drive source hemorrhage/blood loss");
 Console.WriteLine("{\"OriginalLimbTissuePenetration\":\"pass\",\"authorFleshDensity\":true,\"sourceCavity\":true}");
 return;
}
if(args.Contains("--source-limb-interior")){
  var world=new UnityEngine.RuntimeWorld();
  world.MakeCurrent();
  var go=new UnityEngine.GameObject("Original target torso",world);
  var body=go.AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
  var limb=go.AddComponent<Crisis.Runtime.Weapons.Limb>();
  var p=new Crisis.Runtime.Character.OriginalPhysiologyConditions();
  p.InitializeBloodVolume(4500f);p.InitializeVitalClock(0f,0f,0f);
  p.SetBloodPressure(100f,190f,1f,0.5f);
  limb.Body=body;limb.id=Crisis.Runtime.Weapons.LimbId.Torso;
  limb.LimbPhysiology=new Crisis.Runtime.Character.OriginalLimbPhysiology(p);
  var skin=new UnityEngine.GameObject("Source skin",world).transform;
  var coverage=Enumerable.Repeat((byte)0,32*32).ToArray();
  limb.SourceSkinTrace=new Crisis.Runtime.Character.OriginalLimbSkinTrace(skin,32,32,coverage);
  if(limb is not BallisticMaterial)
    throw new Exception("Original Limb is no longer a BallisticMaterial");
  if(typeof(Crisis.Runtime.Weapons.Limb).GetMethod("HandleInterior",
     System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|
     System.Reflection.BindingFlags.DeclaredOnly)==null)
    throw new Exception("Original Limb.HandleInterior is not executable in source runtime");
  if(typeof(Crisis.Runtime.Weapons.Limb).GetMethod("FindInternalDistanceAndExitPosition",
     System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|
     System.Reflection.BindingFlags.DeclaredOnly)==null)
    throw new Exception("Original Limb.FindInternalDistanceAndExitPosition is not compiled");
  // Execute the developer's marksman-specific entry/exit selector against
  // real source skin mask bounds; original limb depth is used, never
  // a made-up capsule thickness.
  limb.depth=.55f; // recovered original Torso.prefab depth
  var sourceProjectile=new UnityEngine.GameObject("Original marksman projectile",world)
      .AddComponent<Projectile>();
  sourceProjectile.MarksmanOwner=go.AddComponent<UnityEngine.Component>();
  sourceProjectile.transform.position=new UnityEngine.Vector3(.17f,.13f,-.12f);
  var originalFind=typeof(Crisis.Runtime.Weapons.Limb).GetMethod(
    "FindInternalDistanceAndExitPosition",
    System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|
    System.Reflection.BindingFlags.DeclaredOnly);
  originalFind.Invoke(limb,[sourceProjectile]);
  var distanceField=typeof(BallisticMaterial).GetField("availableInternalDistance",
    System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
  var foundField=typeof(BallisticMaterial).GetField("foundPotentialExit",
    System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
  if(Math.Abs((float)distanceField.GetValue(limb)-limb.depth)>1e-5f||
    !(bool)foundField.GetValue(limb))
    throw new Exception("Source Limb pixel-trace marksman path ignored authored depth and exit");
  limb.LimbPhysiology.AddTrauma(24f);
  var entry=new UnityEngine.Vector3(.17f,.13f,0f);
  var direction=new UnityEngine.Vector3(1f,.12f,.01f);
  var wound=limb.LimbPhysiology.CreateWoundFromSourceProjectile(limb,5f,24f,.008f,entry,direction);
  if(Math.Abs(wound.Diameter-5f)>1e-6f||
     Math.Abs(wound.EmissionRadiusMeters-.008f)>1e-6f||
     (wound.LocalPosition-entry).sqrMagnitude>1e-8f||
     Math.Abs(wound.LocalPosition.z)>1e-6f||
     Math.Abs(wound.LocalDirection.x-1f)>1e-6f||
     wound.LocalDirection.z==0f||
     !(wound.available>=0f&&wound.available<1f))
    throw new Exception("Original LimbPhysiology.CreateWound source space/randomization mismatch");
  if(limb.LimbPhysiology.Wounds.Count!=1)
    throw new Exception("Projectile wound did not enter source limb bleed graph");
  var blood=p.BloodVolume;
  limb.LimbPhysiology.UpdateHemorrhaging(.02f);
  if(!(p.BloodVolume<blood)||!(limb.LimbPhysiology.InternalBloodVolume>0f))
    throw new Exception("Resolved original projectile wound did not circulate blood");
  var woundCount=limb.LimbPhysiology.Wounds.Count;
  try {
   limb.LimbPhysiology.CreateWoundFromSourceProjectile(limb,5f,1f,.008f,entry,direction,
     useWoundStreams:true);
   throw new Exception("Unimplemented original BloodStreamGenerator cannot be silently invented");
  }catch(NotSupportedException){}
  if(limb.LimbPhysiology.Wounds.Count!=woundCount)
    throw new Exception("Unsupported original stream mode mutated source wounds");
  Console.WriteLine("{\"SourceLimbProjectileInterior\":\"pass\",\"sourceWoundAndHemorrhage\":true}");
  return;
}
if(args.Contains("--character-systems")){
 var physiology=new Crisis.Runtime.Character.OriginalPhysiologyConditions();
 physiology.InitializeBloodVolume(4500f);
 physiology.InitializeVitalClock(0f,0f,0f);
 physiology.SetBloodPressure(100f,190f,1f,0f);
 var limbPhysiology=new Crisis.Runtime.Character.OriginalLimbPhysiology(physiology);
 limbPhysiology.AddTrauma(40f);
 limbPhysiology.CreateWound(new Crisis.Runtime.Character.OriginalWound(
  5f,UnityEngine.Vector3.zero,UnityEngine.Vector3.forward,.01f,.25f));
 var before=physiology.BloodVolume;
 limbPhysiology.UpdateHemorrhaging(.02f);
 if(limbPhysiology.WoundCavityVolume!=40f ||
     !(limbPhysiology.InternalHemorrhaging>0f) ||
     !(physiology.BloodVolume<before) ||
     !(limbPhysiology.InternalBloodVolume>=0f))
  throw new Exception("Original injury internal hemorrhage/circulation not advancing");
 limbPhysiology.AddShock(200f);
 if(Math.Abs(physiology.Shock-200f)>1e-5f)throw new Exception("Original limb shock propagation mismatch");
 limbPhysiology.StopHemorrhage();
 if(limbPhysiology.WoundCavityVolume!=0f||limbPhysiology.InternalBloodVolume!=0f)
  throw new Exception("Original limb StopHemorrhage mismatch");
 physiology.AddLungsTrauma(20f,300f);
 if(physiology.LungsEffectiveness>=1f||physiology.LungsOxygen>=.75f)
  throw new Exception("Original lungs trauma oxygen leakage mismatch");
 physiology.UpdateVitalDynamics(.02f,1f,.3f,false,0f);
 if(physiology.HeartRate<=0f||physiology.BloodPressure<=0f||physiology.RespirationRate<=0f)
  throw new Exception("Original heart/breathing pressures did not advance");
 var hypoxia=new Crisis.Runtime.Character.OriginalPhysiologyConditions();
 hypoxia.InitializeBloodVolume(4500f);
 hypoxia.InitializeVitalClock(0f,0f,0f);
 hypoxia.RemoveBlood(4500f);
 hypoxia.CirculationDisabled=true;
 hypoxia.UpdateVitalDynamics(.02f,1f,.2f,true,0f);
 if(hypoxia.Conscious)throw new Exception("Source hypoxia should cause unconsciousness at 0 blood oxygen");
 hypoxia.UpdateVitalDynamics(.02f,31.2f,.2f,true,0f);
 if(hypoxia.Alive||!hypoxia.RespirationDisabled)
  throw new Exception("Original 30-second blood-oxygen brain-death state not applied");
 var concussion=new Crisis.Runtime.Character.OriginalPhysiologyConditions();
 concussion.AddConcussion(1.3f);
 if(concussion.BrainTrauma<2.9f||!concussion.ShouldLoseConsciousness())
  throw new Exception("Original concussion-to-brain-trauma propagation failed");
 // Real recovered Head/LimbVitalFunction prefab curves drive the native
 // original brain injury and unconsciousness thresholds, not test-only rules.
 using(var sourceVitals=JsonDocument.Parse(File.ReadAllText("game/assets/original-character-limb-components.json")))
 using(var physicsPrefab=JsonDocument.Parse(File.ReadAllText("game/assets/original-full-character-physics.json"))){
  var sourceHead=sourceVitals.RootElement.GetProperty("characters").EnumerateArray()
    .Single(c=>c.GetProperty("name").GetString()=="Operator")
    .GetProperty("limbs").EnumerateArray()
    .Single(l=>l.GetProperty("name").GetString()=="Head");
  var sourceBrain=sourceHead.GetProperty("sourceFields").GetProperty("VitalFunctions")
    .GetProperty("vitalFunctions").EnumerateArray()
    .Single(v=>v.GetProperty("name").GetString()=="Brain");
  var originalHeadGo=sourceHead.GetProperty("gameObject").GetString();
  var sourceCapsule=physicsPrefab.RootElement.GetProperty("prefabs").EnumerateArray()
    .Single(c=>c.GetProperty("name").GetString()=="Operator")
    .GetProperty("records").EnumerateArray()
    .Single(c=>c.GetProperty("classID").GetInt32()==136&&
       c.GetProperty("fields").GetProperty("m_GameObject").GetProperty("fileID").GetString()==originalHeadGo)
    .GetProperty("fields");
  var anatomyWorld=new UnityEngine.RuntimeWorld();
  var anatomyBody=new UnityEngine.GameObject("Original vital body",anatomyWorld)
    .AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
  anatomyBody.InitializeOriginalPhysiology(0f,4500f,0f,0f);
  var headLimb=new UnityEngine.GameObject("Original source Head",anatomyWorld)
    .AddComponent<Crisis.Runtime.Weapons.Limb>();
  headLimb.id=Crisis.Runtime.Weapons.LimbId.Head;headLimb.Body=anatomyBody;
  headLimb.LimbPhysiology=new Crisis.Runtime.Character.OriginalLimbPhysiology(anatomyBody.SourcePhysiology);
  var vitalOnly=JsonDocument.Parse(JsonSerializer.Serialize(new{vitalFunctions=new[]{sourceBrain.Clone()}}));
  var vital=new Crisis.Runtime.Character.OriginalVitalFunctions(
    headLimb,anatomyBody.SourcePhysiology,vitalOnly.RootElement,
    sourceCapsule.GetProperty("m_Height").GetSingle(),
    sourceCapsule.GetProperty("m_Center").GetProperty("y").GetSingle());
  var sourceCenter=sourceCapsule.GetProperty("m_Center").GetProperty("y").GetSingle();
  var sourceHeight=sourceCapsule.GetProperty("m_Height").GetSingle();
  float normalizedSourceY=.7f*sourceHeight-(sourceHeight*.5f-sourceCenter);
  var hit=new UnityEngine.Vector3(0f,normalizedSourceY,0f);
  var announced=new List<string>();
  vital.HitTest(12f,hit,hit,300f,name=>announced.Add(name));
  float sourceBrainFactor=vital.Functions[0].Factor.Evaluate(.7f);
  if(announced.Count!=1||announced[0]!="Brain"||
    MathF.Abs(anatomyBody.SourcePhysiology.BrainTrauma-12f*sourceBrainFactor)>1e-4f||
    !anatomyBody.SourcePhysiology.ShouldLoseConsciousness())
   throw new Exception($"Original brain vital failure: names=[{string.Join(",",announced)}], trauma={anatomyBody.SourcePhysiology.BrainTrauma}, position={normalizedSourceY}, curve={vital.Functions[0].Factor.Evaluate(.7f)}, lost={anatomyBody.SourcePhysiology.ShouldLoseConsciousness()}");
  var p=anatomyBody.SourcePhysiology;
  var torsoHeart=new Crisis.Runtime.Character.OriginalLimbPhysiology(p);
  var pressure=torsoHeart.bloodPressureModifier;
  p.AddHeartTrauma(7f,0f,torsoHeart);
  if(Math.Abs(p.HeartEffectiveness-.9f)>1e-4f||
     Math.Abs(torsoHeart.bloodPressureModifier-(pressure+.1f))>1e-4f)
    throw new Exception("Original heart trauma and torso pressure modification source mismatch");
  p.SetParalyzed();
  if(!p.Paralyzed)throw new Exception("Original spine paralysis state not retained");
 }
 var world=new UnityEngine.RuntimeWorld();
 var host=new UnityEngine.GameObject("Source ragdoll operator",world);
 var body=host.AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
 host.AddComponent<UnityEngine.Animator>();
 host.AddComponent<UnityEngine.Rigidbody>().velocity=new UnityEngine.Vector3(1,0,0);
 var bone=new UnityEngine.GameObject("Source upper arm",world);
 var rb=bone.AddComponent<UnityEngine.Rigidbody>();
 rb.drag=.7f;
 var events=new List<(string kind,object value)>();
 rb.KinematicChanged=state=>events.Add(("kinematic",state));
 rb.GravityChanged=state=>events.Add(("gravity",state));
 rb.DragChanged=value=>events.Add(("drag",value));
 rb.PhysicsForceSink=(force,point,mode)=>events.Add(("force",mode));
 var limb=bone.AddComponent<Crisis.Runtime.Weapons.Limb>();
 limb.id=Crisis.Runtime.Weapons.LimbId.RightUpperArm;
 limb.Body=body;body.limbs.Add(limb);
 var injury=new Crisis.Runtime.Character.OriginalLimbPhysiology(physiology);
 limb.LimbPhysiology=injury;
 var hits=0;body.EventHit.AddListener(()=>hits++);
 limb.AddTrauma(6f,UnityEngine.Vector3.zero,UnityEngine.Vector3.zero);
 if(hits!=1||injury.WoundCavityVolume!=6f)
  throw new Exception("Original limb injury event propagation mismatch");
 body.SetKinematic(false);
 if(rb.isKinematic||!events.Any(e=>e.kind=="force"&&Equals(e.value,UnityEngine.ForceMode.VelocityChange)))
  throw new Exception("Original ragdoll kinematic release or source velocity transfer failed");
 body.SetFloating(true);
 if(rb.useGravity||rb.drag!=0f)throw new Exception("Source floating limb force flags failed");
 body.SetFloating(false);
 if(!rb.useGravity||Math.Abs(rb.drag-.7f)>1e-5f)
  throw new Exception("Source gravity/drag restoration mismatch");
 body.SetKinematic(true);
 if(!rb.isKinematic)throw new Exception("Original character re-kinematic limb transition failed");
 var hinge=bone.AddComponent<UnityEngine.HingeJoint>();
 hinge.angle=33f;
 var breaks=0;
 var muscle=new Crisis.Runtime.Character.OriginalLimbMuscle(hinge,rb,physiology,()=>breaks++);
 muscle.extendAngle=65f;
 body.SetKinematic(false);
 muscle.SetSignal(1f,0f,1f,1f,false,0f);
 muscle.Update(.25f);
 if(!hinge.useSpring||hinge.spring.spring<=0||Math.Abs(hinge.spring.targetPosition-65f)>1e-5f)
  throw new Exception("Recovered LimbMuscle.Update source spring force and target mismatch");
 muscle.LockCurrentAngle(.5f);
 if(Math.Abs(hinge.spring.targetPosition-33f)>1e-5f)
  throw new Exception("Recovered joint-angle locking incorrect");
 muscle.AddJointTrauma(550f,1f);
 if(breaks!=1||muscle.BoneIntegrityNormalized>=0f||muscle.JointIntegrityNormalized>=0f)
  throw new Exception("Original LimbMuscle fracture propagated no joint-break callback");
 muscle.Relax();
 if(hinge.useSpring)throw new Exception("Recovered LimbMuscle.Relax did not release source hinge");
 // Live CharacterBody owns and advances its original physiology, not a test-only kernel.
 body.InitializeOriginalPhysiology(0f,4500f,0f,0f);
 var initialVitals=body.SourcePhysiology.BloodOxygen;
 var vitalSnapshot=body.StepOriginalPhysiology(.02f,.5f,1.4f,3.2f,heartRandom:0f);
 if(body.SourcePhysiology.WorkIntensity<=0f||
    body.SourcePhysiology.BloodOxygen>initialVitals||
    body.SourcePhysiology.BloodVolume!=4500f||
    body.SourcePhysiology.RespirationRate<=0f)
  throw new Exception("Live CharacterBody original physiological work, blood and breathing did not advance");
 var target=new UnityEngine.GameObject("Original arm sequence target",world).transform;
 var binding=new SourceArmTestBindings(target);
 var arm=new Crisis.Runtime.Character.OriginalIKArmControllerPlayback(binding);
 var seq=new IKArmSequenceRuntime();
 seq.right.Add(new IKArmAnimationFrame{action=IKArmAction.Wait,time=.1f,callback=IKControllerInvoke.LowerWeapon});
 seq.right.Add(new IKArmAnimationFrame{action=IKArmAction.Transition,time=.2f,transform=IKArmTarget.MagazinePocket,
                                       callback=IKControllerInvoke.HolsterWeapon});
 seq.left.Add(new IKArmAnimationFrame{action=IKArmAction.WaitForSiblingFrame,waitForSiblingFrame=0,
                                      callback=IKControllerInvoke.ActivatePendingWeapon});
 arm.Play(seq,0f);
 arm.Update(.05f);
 if(binding.Calls.Count!=0)throw new Exception("Original sequencer callback fired before timed wait");
 arm.Update(.11f);arm.Update(.12f);arm.Update(.13f);arm.Update(.14f);
 if(binding.Calls.Count==0||binding.Calls[0]!=IKControllerInvoke.LowerWeapon||
     binding.Transitions!=1)
  throw new Exception("Original IK sequencer timed callback/target transition mismatch");
 arm.Update(.15f);arm.Update(.16f);
 if(!binding.Calls.Contains(IKControllerInvoke.ActivatePendingWeapon))
  throw new Exception("Original IK sibling-frame synchronization failed");
 Console.WriteLine("{\"OriginalCharacterSystems\":\"pass\",\"RagdollPhysics\":true,\"SourceIKFrames\":true,\"Injury\":true}");
 return;
}

if (args.Contains("--character-facing")) {
{
    var w = new UnityEngine.RuntimeWorld();
    var a = new UnityEngine.GameObject("source facing A", w).AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
    var b = new UnityEngine.GameObject("source facing B", w).AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
    a.Direction=1f;
    b.Direction=-1f;
    a.transform.position=new UnityEngine.Vector3(0f,0f,0f);
    b.transform.position=new UnityEngine.Vector3(3f,0f,0f);
    a.Torso=new UnityEngine.GameObject("source torso A",w).AddComponent<Crisis.Runtime.Weapons.Limb>();
    a.Torso.transform.position=new UnityEngine.Vector3(1f,0f,0f);
    if (!a.Facing(3f) || !a.Facing(b) || !a.BodyFacingMe(b) || !a.FacingDirection(1f))
        throw new Exception("Recovered CharacterBody facing methods differ from source");
    if (a.Facing(-3f) || a.FacingDirection(-1f) || a.Facing((Crisis.Runtime.Weapons.CharacterBody)null))
        throw new Exception("Recovered CharacterBody facing rejection failed");
    if (Math.Abs(a.KinematicDistanceToPosition(4f)-3f)>1e-6f)
        throw new Exception("Recovered CharacterBody kinematic distance failed");
    Console.WriteLine("{\"CharacterBodyFacingSource\":\"pass\"}");
}

return;
}
if (args.Contains("--character-dependencies")) {
    var world=new UnityEngine.RuntimeWorld();
    var obj=new UnityEngine.GameObject("character deps",world);
    var animator=obj.AddComponent<UnityEngine.Animator>();
    animator.SetFloat("KneelRatio",.35f);
    animator.SetBool("Moving",true);
    if(Math.Abs(animator.GetFloat("KneelRatio")-.35f)>1e-6f||!animator.GetBool("Moving"))
       throw new Exception("Original animator parameter storage failed");
    var col=obj.AddComponent<UnityEngine.CapsuleCollider>();
    col.direction=0;col.radius=.5f;col.height=2f;
    world.Initialize();
    if(!col.Raycast(new UnityEngine.Ray(new UnityEngine.Vector3(-3,0,0),UnityEngine.Vector3.right),out var h,10f)
        || Math.Abs(h.distance-2f)>1e-4f)
       throw new Exception("Original axis-X capsule raycast failed");
    if(col.Raycast(new UnityEngine.Ray(new UnityEngine.Vector3(0,0,0),UnityEngine.Vector3.right),out _,10f))
       throw new Exception("Capsule raycast should ignore ray origin inside collider");
    var blood=new Crisis.Runtime.Character.OriginalPhysiologyConditions();
    blood.InitializeBloodVolume(4500f);
    blood.RemoveBlood(450f);
    if(Math.Abs(blood.BloodVolume-4050f)>1e-5f ||
       Math.Abs(blood.BloodOxygen-.9f)>1e-5f ||
       Math.Abs(blood.BloodVolumeNormalized-.9f)>1e-5f)
       throw new Exception("Original Physiology.RemoveBlood source regression");
    blood.RemoveLungsOxygen(.2f);
    if(Math.Abs(blood.LungsOxygen-.6f)>1e-5f)
       throw new Exception("Original Physiology.RemoveLungsOxygen source regression");
    var physiology=new Crisis.Runtime.Character.OriginalPhysiologyConditions();
    if(physiology.ShouldLoseFooting()||physiology.ShouldLoseConsciousness()||!physiology.ShouldRegainConsciousness())
       throw new Exception("Original healthy physiology thresholds failed");
    physiology.ShockResistance=.5f;
    physiology.AddShock(5000f);
    if(!physiology.ShouldLoseFooting()||Math.Abs(physiology.Shock-2500f)>1e-6f)
       throw new Exception("Original shock resistance or footing threshold failed");
    physiology.Concussion=.775f;
    if(!physiology.ShouldLoseConsciousness()||physiology.ShouldRegainConsciousness())
       throw new Exception("Original concussion unconsciousness threshold failed");
    physiology.Concussion=.0f;physiology.BloodOxygen=.5f;
    if(!physiology.ShouldLoseConsciousness())throw new Exception("Original blood oxygen threshold failed");
    physiology.BloodOxygen=.9f;physiology.BrainTrauma=3f;
    if(!physiology.ShouldLoseConsciousness())throw new Exception("Original brain trauma threshold failed");
    var body=obj.AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
    body.Direction=1f;
    var limb=obj.AddComponent<Crisis.Runtime.Weapons.Limb>();
    limb.Body=body;
    limb.visual=new UnityEngine.GameObject("limb visual",world).transform;
    limb.visual.localPosition=new UnityEngine.Vector3(0,0,.4f);
    limb.zLeft=-.25f;
    var attach=new UnityEngine.GameObject("attachment",world).AddComponent<Crisis.Runtime.Weapons.LimbAttachment>();
    attach.zLeft=-.15f;attach.zRight=.2f;
    limb.attachments.Add(attach);
    limb.CaptureSourceVisualDepth();
    body.Direction=-1f;
    limb.Flip();
    if(Math.Abs(limb.visual.localPosition.z+.25f)>1e-6f || Math.Abs(attach.transform.localPosition.z+.15f)>1e-6f)
       throw new Exception("Recovered original Limb.Flip left-depth source behavior failed");
    body.Direction=1f;
    limb.Flip();
    if(Math.Abs(limb.visual.localPosition.z-.4f)>1e-6f || Math.Abs(attach.transform.localPosition.z-.2f)>1e-6f)
       throw new Exception("Recovered original Limb.Flip right-depth source behavior failed");
    Console.WriteLine("{\"CharacterBodyDependencyPrimitives\":\"pass\"}");
    return;
}
if(args.Contains("--character-animation")) {
 var world=new UnityEngine.RuntimeWorld();
 var go=new UnityEngine.GameObject("Original CharacterBody animation",world);
 var body=go.AddComponent<Crisis.Runtime.Weapons.CharacterBody>();
 var animator=go.AddComponent<UnityEngine.Animator>();
 var capsule=go.AddComponent<UnityEngine.CapsuleCollider>();
 body.InitializeSourceAnimation(animator,capsule);
 for(int i=0;i<10;i++)body.ExecuteSourceUpdateAnimation(true,true,false,2.55f,1.5f,4.2f,.6f,true,.02f,-.725f);
 if(body.KneelRatio<=.5f || Math.Abs(capsule.height-1.25f)>1e-5f
   || Math.Abs(capsule.center.x+.15f)>1e-5f || Math.Abs(capsule.center.y+.725f)>1e-5f)
   throw new Exception("Original CharacterBody kneeling collider interpolation failed");
 if(!animator.GetBool("Moving") || animator.GetBool("Backwards")
   || Math.Abs(animator.GetFloat("MoveSpeed")-.19444445f)>1e-4f
   || animator.GetFloat("CrippledRatio")!=0f)
   throw new Exception("Original CharacterBody animation parameters failed");
 body.ExecuteSourceUpdateAnimation(false,true,true,0f,1.5f,4.2f,.6f,false,.02f,-.725f);
 if(!animator.GetBool("Backwards") || Math.Abs(animator.GetFloat("CrippledRatio")-.6f)>1e-5f
    || body.KneelRatio>=1f)
   throw new Exception("Original CharacterBody backwards and civilian cripple state failed");
 Console.WriteLine("{\"CharacterBodyUpdateAnimationSource\":\"pass\"}");
 return;
}
Console.WriteLine(RuntimeContracts.Run());
Console.WriteLine(ArmSequenceContracts.Run("game/assets/original-arm-sequences.json"));
if(File.Exists("game/assets/presentation.json.gz"))Console.WriteLine(PresentationContracts.Run("game/assets/presentation.json.gz"));
if(File.Exists("game/assets/weapon-runtime.json.gz"))Console.WriteLine(WeaponContracts.Run("game/assets/weapon-runtime.json.gz"));
if(File.Exists("game/assets/ballistics.json.gz"))Console.WriteLine(BallisticContracts.Run());
if(args.Length>0){
    var package=File.ReadAllText(args[0]);
    foreach(var path in new[]{"Assets/Scenes/Marksman range.unity","Assets/Scenes/Mission 01.unity"}){
        var loaded=SceneLoader.Load(package,path);
        if(loaded.unresolvedReferences.Count>0)throw new Exception("Unresolved supported fields");
        Console.WriteLine(JsonSerializer.Serialize(loaded.Report()));
        Console.WriteLine(JsonSerializer.Serialize(SceneLoader.VerifyImportedRay(loaded)));
    }
    foreach(var path in new[]{"Assets/Resources/weapons/ammunition/5.56-JHP.prefab","Assets/Resources/weapons/ammunition/308-Win-BSP.prefab"})
        Console.WriteLine(JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalPrefab(package,path)));
    Console.WriteLine(JsonSerializer.Serialize(AmmunitionLoader.VerifyOriginalResource(package,"5.56-JHP")));
}

{
    var body = new Crisis.Runtime.Weapons.CharacterBody { Direction=1f };
    // Original CharacterBody.Move sets acceleration and reset behavior;
    // our browser boundary returns the source ForceMode.Acceleration value.
    body.Move(1f);
    var first=JsonSerializer.SerializeToElement(body.ExecuteSourceMovement(0f,.02f,1.5f,4.2f,1f,true,false,true,true,false));
    if(Math.Abs(first.GetProperty("forceX").GetSingle()-63f)>0.001f || !first.GetProperty("reset").GetBoolean())
        throw new Exception("Original full-pull sprint force regression");
    body.Move(1f);
    var partial=JsonSerializer.SerializeToElement(body.ExecuteSourceMovement(0f,.02f,1.5f,4.2f,.5f,true,false,true,true,false));
    if(Math.Abs(partial.GetProperty("forceX").GetSingle()-31.5f)>0.001f)
        throw new Exception("Owner-approved proportional source sprint force regression");
    body.Move(-1f);
    var opposite=JsonSerializer.SerializeToElement(body.ExecuteSourceMovement(0f,.02f,1.5f,4.2f,1f,true,false,true,true,false));
    if(Math.Abs(opposite.GetProperty("forceX").GetSingle()+22.5f)>0.001f)
        throw new Exception("Original backwards sprint restriction regression");
    body.Move(1f);
    var overSpeed=JsonSerializer.SerializeToElement(body.ExecuteSourceMovement(5f,.02f,1.5f,4.2f,1f,false,false,true,true,false));
    if(overSpeed.GetProperty("forceX").GetSingle()!=0f || overSpeed.GetProperty("reset").GetBoolean())
        throw new Exception("Original overspeed no-force behavior regression");
    Console.WriteLine("{\"CharacterBodySourceMovement\":\"pass\",\"AnalogEnhancement\":true}");
}

sealed class SourceArmTestBindings : Crisis.Runtime.Character.IOriginalIKArmBindings {
 readonly UnityEngine.Transform target;
 public readonly List<IKControllerInvoke> Calls=new();
 public int Transitions;
 public SourceArmTestBindings(UnityEngine.Transform target){this.target=target;}
 public bool OnTarget(IKArmType type)=>true;
 public float WeaponAimAlignment()=>0f;
 public UnityEngine.Transform TargetTransform(IKArmTarget value,IKArmType type,out UnityEngine.Vector3 offset){
  offset=UnityEngine.Vector3.zero;return target;
 }
 public void TargetTransition(IKArmType type,UnityEngine.Transform t,UnityEngine.Vector3 offset,float seconds) {
  if(t!=target||seconds<=0)throw new Exception("Source IK target transform invalid");Transitions++;
 }
 public void StartWait(IKArmType type){}
 public void Invoke(IKControllerInvoke method)=>Calls.Add(method);
}
