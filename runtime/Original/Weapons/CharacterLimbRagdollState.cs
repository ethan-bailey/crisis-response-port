using UnityEngine;
using Crisis.Runtime.Character;
using UnityEngine.Events;
namespace Crisis.Runtime.Weapons;

// Developer-authored LimbId order, original Limb.SetKinematic/SetFloating and
// CharacterBody physics transition logic. Operations go to real PhysX sinks
// on Rigidbody, not to an independent ragdoll approximation.
public enum LimbId {
 Pelvis,Abdomen,Torso,Neck,Head,RightUpperArm,RightLowerArm,RightHand,
 LeftUpperArm,LeftLowerArm,LeftHand,RightUpperLeg,RightLowerLeg,
 RightFoot,RightToes,LeftUpperLeg,LeftLowerLeg,LeftFoot,LeftToes,NumberOfLimbs
}
public partial class Limb {
 public LimbId id;
 public Transform kinematicParent;
 public OriginalLimbPhysiology LimbPhysiology;
 public OriginalLimbMuscle SourceMuscle;
 public OriginalVitalFunctions SourceVitals;
 public OriginalLimbSkinTrace SourceSkinTrace;
 public Rigidbody Rigidbody => GetComponent<Rigidbody>();
 Vector3 potentialForce,potentialPosition;
 float potentialTime,rigidBodyDefaultDrag;
 public void SetKinematic(bool kinematic,Vector3 worldSpaceVelocity=default(Vector3)){
  if(id!=LimbId.Pelvis||!Body.PinnedBodies){
   Rigidbody.isKinematic=kinematic;
   if(!kinematic){
    transform.SetParent(Body.transform,true);
    Rigidbody.interpolation=RigidbodyInterpolation.Interpolate;
    SetFloating(Body.FloatingBodies);
    Rigidbody.AddForce(worldSpaceVelocity,ForceMode.VelocityChange);
    Rigidbody.angularDrag=10f;
    Rigidbody.AddForceAtPosition(potentialForce,potentialPosition,ForceMode.Impulse);
    potentialForce=Vector3.zero;
   }
  }
 }
 // Called AFTER original Projectile/BallisticMaterial determines displaced
 // tissue, impulse and shock. Never guess these from a screen tap or raw hit.
 public void ApplyResolvedSourceImpact(float cavityVolume,float impactShock,
      float boneVolume,float normalizedLocalY,Vector3 impulse,Vector3 worldPosition,
      OriginalWound wound=null){
  if(LimbPhysiology==null)throw new InvalidOperationException("Original limb has no Physiology component: "+id);
  if(cavityVolume<0f||impactShock<0f||boneVolume<0f||
    !float.IsFinite(cavityVolume)||!float.IsFinite(impactShock)||!float.IsFinite(boneVolume))
    throw new ArgumentOutOfRangeException(nameof(cavityVolume));
  AddTrauma(cavityVolume,impulse,worldPosition);
  LimbPhysiology.AddShock(impactShock);
  if(id==LimbId.Head||id==LimbId.Neck){
   float value=impactShock*UnityEngine.Random.Range(.0005f,.002f)
       * (id==LimbId.Head?1f:.25f);
   Body.SourcePhysiology?.AddConcussion(value);
  }
  if(boneVolume>0f){
   Body.SourcePhysiology?.AddShock(impactShock);
   SourceMuscle?.AddJointTrauma(boneVolume,normalizedLocalY);
   if((!Body.NoPlayerLimp||!Body.IsPlayer)&&
      (id==LimbId.RightUpperLeg||id==LimbId.RightLowerLeg||
       id==LimbId.RightFoot||id==LimbId.LeftUpperLeg||
       id==LimbId.LeftLowerLeg||id==LimbId.LeftFoot))Body.CrippledRatio=1f;
  }
  if(wound!=null)LimbPhysiology.CreateWound(wound);
 }
 public void SetFloating(bool enable){
  if(Rigidbody.useGravity != !enable){
   Rigidbody.useGravity=!enable;
   if(enable)rigidBodyDefaultDrag=Rigidbody.drag;
   Rigidbody.drag=(!enable)?rigidBodyDefaultDrag:0f;
  }
 }
 // Source Limb.AddTrauma overload, the PhysX impulse itself is separately
 // controlled by source ApplyForceAtPosition and connected Rigidbody.
 public void AddTrauma(float volume,Vector3 impulse,Vector3 impulsePosition){
  if(LimbPhysiology==null)throw new InvalidOperationException("Original LimbPhysiology required");
  LimbPhysiology.AddTrauma(volume);
  ApplyForceAtPosition(impulse,impulsePosition);
  Body.EventHit.Invoke();
 }
 // Recovered Limb.ApplyForceAtPosition source energy redistribution.
 public void ApplyForceAtPosition(Vector3 force,Vector3 position){
  if(force.sqrMagnitude!=0f){
   float value=UnityEngine.Random.value;
   float num=Mathf.Abs(force.x)+Mathf.Abs(force.y);
   force.x+=Mathf.Abs(force.z)*(Mathf.Abs(force.x)/num)*Mathf.Sign(force.x)*value;
   force.y+=Mathf.Abs(force.z)*(Mathf.Abs(force.y)/num)*Mathf.Sign(force.y)*value;
   force.z=0f;
   if(Rigidbody.isKinematic){
    potentialForce*=0.1f;
    potentialForce+=force;
    potentialPosition=position;
    potentialTime=Time.time;
   }else{
    Rigidbody.AddForceAtPosition(force,position,ForceMode.Impulse);
   }
  }
 }
}
public partial class CharacterBody {
 public readonly List<Limb> limbs=new();
 public bool PinnedBodies;
 public bool FloatingBodies;
 public bool NoPlayerLimp;
 public float CrippledRatio;
 public UnityEvent EventHit=new();
 public UnityEvent EventRagdoll=new();
 public UnityEvent EventUnconscious=new();
 public UnityEvent EventDeath=new();
 public bool RagdollEntered=>!Kinematic;
 public void OnOriginalOrganUnconscious(){
  // Physiology.SetUnconscious calls the actual source CharacterBody transition.
  if(SourcePhysiology?.Conscious==true)return;
  if(Kinematic){
   EnterSourceRagdoll();
   EventUnconscious.Invoke();
  }
 }
 public void OnOriginalJointBroken(Limb limb){
  if(limb==null||!limbs.Contains(limb))throw new InvalidOperationException("Original joint not owned by character");
  // Original Physiology.OnJointBroken loses consciousness if conscious/kinematic.
  if(Kinematic&&SourcePhysiology?.Conscious==true){
   SourcePhysiology.SetUnconscious(Time.time);
   EnterSourceRagdoll();
   EventUnconscious.Invoke();
  }
 }
 public void EnterSourceRagdoll(){
  if(!Kinematic)return;
  // Original CharacterBody.SetRagdoll: relax the real authored ArmsControl,
  // release ALL source limb actors, and stop clip-driven bone transforms.
  rightArm.enabled=false;
  leftArm.enabled=false;
  GetComponent<ArmsControl>()?.Relax();
  SetKinematic(false);
  SourceMuscleControl?.Relax();
  EventRagdoll.Invoke();
 }
 public void SetKinematic(bool state){
  if(Kinematic==state)return;
  Kinematic=state;
  GetComponent<Animator>().enabled=state;
  var velocity=GetComponent<Rigidbody>()?.velocity??Vector3.zero;
  foreach(Limb limb in limbs)limb.SetKinematic(state,velocity);
 }
 public void SetFloating(bool enable){
  if(Kinematic)return;
  FloatingBodies=enable;
  foreach(Limb limb in limbs)limb.SetFloating(enable);
 }
 // Original CharacterBody.SetRagdoll includes additional source events
 // (MuscleControl, disarm, collider, etc.) which require real dependencies.
 // Do not call SetKinematic(false) equivalent to full SetRagdoll.
}
