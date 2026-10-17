using UnityEngine;
using Crisis.Runtime.Character;
namespace Crisis.Runtime.Weapons;

// Physiology lives on the existing game-owned CharacterBody, not a standalone
// test fixture. WorkIntensity formula is from recovered Physiology.SetWorkLevel.
// Additional source Spasm/Cramps/weapon-hold terms require MuscleControl to
// be imported; no false injury playback until those are attached.
public partial class CharacterBody {
 public OriginalPhysiologyConditions SourcePhysiology {get;private set;}
 public OriginalMuscleControl SourceMuscleControl {get;private set;}
 public void InitializeOriginalPhysiology(float now,float? startingVolume=null,
      float? heartbeatOffset=null,float? breathingOffset=null){
  SourcePhysiology=new OriginalPhysiologyConditions();
  SourcePhysiology.InitializeBloodVolume(startingVolume??UnityEngine.Random.Range(4000f,5000f));
  SourcePhysiology.InitializeVitalClock(now,heartbeatOffset??UnityEngine.Random.value,
      breathingOffset??UnityEngine.Random.value);
  SourceMuscleControl=new OriginalMuscleControl(this);
  SourcePhysiology.MuscleControl=SourceMuscleControl;
 }
 public object StepOriginalPhysiology(float dt,float now,float velocity,float sprintSpeed,
    bool respirationBlocked=false,float holdWeaponWorkIntensity=0f,
    bool spasmsActive=false,bool crampsActive=false,float heartRandom=0f){
  var p=SourcePhysiology??throw new InvalidOperationException("Original CharacterBody physiology not initialized");
  if(sprintSpeed<=0||!float.IsFinite(sprintSpeed))throw new ArgumentOutOfRangeException(nameof(sprintSpeed));
  float num=.2f*(p.RespirationRate/p.respirationRateMax);
  float num2=.1f*(p.HeartRate/p.heartRateMax);
  float num3=Mathf.Abs(velocity)/sprintSpeed;
  // Source Physiology.SetWorkLevel reads actual MuscleControl activity.
  float num4=((spasmsActive||SourceMuscleControl?.SpasmsActive==true)?.5f:0f)
    +((crampsActive||SourceMuscleControl?.CrampsActive==true)?.75f:0f);
  float work=Mathf.Clamp01(num2+num+holdWeaponWorkIntensity+num3+num4);
  bool wasAlive=p.Alive,wasConscious=p.Conscious;
  p.UpdateVitalDynamics(dt,now,work,respirationBlocked,heartRandom,deferPostCirculation:true);
  // The original Physiology.Update orders circulation/heart, then UpdateLimbs,
  // SetHemorrhaging, SetBloodOxygen, and SetCharacterState. The source limb
  // instances now belong to this actual gameplay character.
  if(!p.Exsanguinated)foreach(var limb in limbs)
    limb.LimbPhysiology?.UpdateHemorrhaging(dt);
  float hemorrhaging=0f,woundVolume=0f;
  foreach(var limb in limbs){
   if(limb.LimbPhysiology!=null){
    hemorrhaging+=limb.LimbPhysiology.InternalHemorrhaging;
    woundVolume+=limb.LimbPhysiology.WoundCavityVolume;
   }
   limb.SourceMuscle?.Update(now);
  }
  p.CompletePostCirculation(dt,now);
  if(Kinematic&&(p.Shock>=1000f||p.ShouldLoseFooting()))
   EnterSourceRagdoll();
  if(wasConscious&&!p.Conscious){
   EnterSourceRagdoll();
   EventUnconscious.Invoke();
  }
  if(!wasConscious&&p.Conscious)
   SourceMuscleControl?.ActivateMusclesOnRegainedConsciousness();
  // Original SetCharacterState drives hypoxic spasms after ragdoll; do not
  // generate imitation animation, signal the recovered joint muscle system.
  if(!Kinematic&&p.Alive&&SourceMuscleControl!=null&&!SourceMuscleControl.SpasmsActive
    &&p.BloodOxygen<=.75f&&p.BrainTraumaNormalized<.25f){
   if(UnityEngine.Random.value<.5f)SourceMuscleControl.StartLowIntensityMuscleSpasms();
   else SourceMuscleControl.StartHighIntensityMuscleSpasms();
  }
  if(wasAlive&&!p.Alive){
   SourceMuscleControl?.OnDeath();
   EventDeath.Invoke();
  }
  SourceMuscleControl?.Update();
  return new {p.Alive,p.Conscious,p.BloodVolume,p.BloodOxygen,p.LungsOxygen,
       p.HeartRate,p.RespirationRate,p.HeartBeatCycle,p.Respiration,
       p.BloodPressure,p.Shock,p.BrainTrauma,p.WorkIntensity,
       Hemorrhaging=hemorrhaging,WoundVolume=woundVolume,Kinematic,CrippledRatio,
       LimbCount=limbs.Count};
 }
}

