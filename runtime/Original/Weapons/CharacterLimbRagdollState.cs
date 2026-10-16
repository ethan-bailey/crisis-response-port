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
 public UnityEvent EventHit=new();
 public UnityEvent EventRagdoll=new();
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
