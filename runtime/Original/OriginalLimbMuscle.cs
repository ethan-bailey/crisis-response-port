using UnityEngine;
namespace Crisis.Runtime.Character;

// Source-backed LimbMuscle.cs joint torque/spring, cramp/spasm, fracture
// calculations. Bound to real imported HingeJoint and PhysX spring sink.
// MuscleControl nerve scheduler and visual deformations remain separate.
public sealed class OriginalLimbMuscle {
 readonly HingeJoint joint;
 readonly Rigidbody rigidBody;
 readonly OriginalPhysiologyConditions physiology;
 readonly Action onJointBroken;
 JointSpring spring;
 bool jointLocked,fadeUp;
 float startBoneIntegrity,extend,contract,impulseTimestamp,impulseDuration,impulseIntensity,jointIntegrity;
 public float contractAngle,extendAngle,strength=100f,boneIntegrity=500f;
 public float BoneIntegrityNormalized=>boneIntegrity/startBoneIntegrity;
 public float JointIntegrityNormalized=>jointIntegrity/startBoneIntegrity;
 public OriginalLimbMuscle(HingeJoint joint,Rigidbody rigidBody,OriginalPhysiologyConditions physiology,Action onJointBroken){
  this.joint=joint??throw new ArgumentNullException(nameof(joint));
  this.rigidBody=rigidBody??throw new ArgumentNullException(nameof(rigidBody));
  this.physiology=physiology??throw new ArgumentNullException(nameof(physiology));
  this.onJointBroken=onJointBroken??throw new ArgumentNullException(nameof(onJointBroken));
  startBoneIntegrity=jointIntegrity=boneIntegrity;
  spring=joint.spring;
 }
 public void Update(float now){
  if(physiology.BrainTissue/1000f<.5f||joint==null||rigidBody.isKinematic)return;
  float num=Mathf.Clamp01((now-impulseTimestamp)/impulseDuration);
  float num2=(!fadeUp)?(1f-num):Mathf.Abs(Mathf.Sin(num*MathF.PI));
  float num3=(extend+contract)*impulseIntensity*num2;
  if(num3<=float.Epsilon){joint.useSpring=false;return;}
  float num4=extend-contract;
  spring.spring=Mathf.Min(num3*strength,strength*physiology.BloodOxygen);
  if(Mathf.Abs(num4)>float.Epsilon)
   spring.targetPosition=(!(num4>0f))?contractAngle:extendAngle;
  joint.spring=spring;joint.useSpring=true;
 }
 public void SetSignal(float extend,float contract,float duration,float intensity=1f,bool fadeUp=false,float now=0f){
  this.extend=Mathf.Clamp01(extend);this.contract=Mathf.Clamp01(contract);
  impulseTimestamp=now;impulseDuration=duration;impulseIntensity=intensity;this.fadeUp=fadeUp;
 }
 void SetTargetPosition(float position,float strength){
  if(!(joint==null)){
   spring.spring=strength;spring.targetPosition=position;joint.spring=spring;
   if(!rigidBody.isKinematic)joint.useSpring=true;
  }
 }
 public void SetAngle(float angle,float force){SetTargetPosition(angle,force);}
 public void LockCurrentAngle(float strengthScale=1f,float duration=1f){
  if(!(joint==null)&&!jointLocked){SetTargetPosition(joint.angle,strength*strengthScale);jointLocked=true;}
 }
 public void Extend(float force){SetTargetPosition(extendAngle,force);}
 public void Contract(float force){SetTargetPosition(contractAngle,force);}
 public void StartSpasm(float extend,float contract,float duration,float intensity,bool fadeUp=false,float now=0f){
  if(!(joint==null)&&!rigidBody.isKinematic){SetSignal(extend,contract,duration,intensity,fadeUp,now);joint.useSpring=true;}
 }
 public void Relax(){if(!(joint==null)){jointLocked=false;joint.useSpring=false;}}
 public void UpdateStrength(float strengthScale){
  if(!(joint==null)){spring.spring=strength*strengthScale;joint.spring=spring;
   if(!rigidBody.isKinematic)joint.useSpring=true;}
 }
 public void AddJointTrauma(float volume,float normalizedLocalY){
  float num=boneIntegrity;boneIntegrity-=volume;
  float num2=Mathf.Abs(normalizedLocalY-.5f)*2f;
  float num3=volume*num2;
  jointIntegrity-=num3;
  if(num>0f&&boneIntegrity<=0f)onJointBroken();
 }
}
