using UnityEngine;
namespace Crisis.Runtime.Presentation;
// Typed views of currently executable, uninjured / unfired scene state.
// They do not simulate physiology, equipment, recoil or damage.
public sealed class LimbPoseSnapshot {public Transform transform;}
public sealed class ArmTargetSnapshot {public Transform ikTarget;}
public sealed class ArmConfigurationSnapshot {public float torsoAngle,headAngle;}
public sealed class WeaponPoseSnapshot {
 public WeaponBrace type;public float Angle,RecoilKick,weight,alignmentModifier=1;
 public bool IsBlocked;public ArmConfigurationSnapshot armConfiguration;
 public float TotalWeight=>weight;
 public float AlignmentSpeed=>type==WeaponBrace.Shoulder?Mathf.Lerp(12,4,Mathf.Clamp(TotalWeight*alignmentModifier,2.5f,10)/10):Mathf.Lerp(12,7.5f,Mathf.Clamp(TotalWeight*alignmentModifier,.6f,2)/2);
}
public sealed class PosePhysiologySnapshot {
 // Physiology.Initialize sets BloodVolume==StartingBloodVolume. No injury input
 // can be supplied while the actual physiology/damage runtime is unavailable.
 public float Respiration,RespirationRateFactor;public bool invulnerable;
 public float BloodVolumeNormalized=>1;
}
public sealed partial class WeaponAimerSlice {
 public BodyPoseSlice Body;public bool useSights,isAimingLow,IsUsingSights;
 public void Step()=>UpdateHeadState();
}
public sealed partial class BodyPoseSlice {
 public enum HeadLookMode {Weapon,AimPoint,RightHand,LeftHand}
 public bool IsMoving,IsSprinting,IsKneeling,IsUsingPrecision,FreeAimActive,StunResponseStateActive,IsLoweringWeapon;
 public PosePhysiologySnapshot Physiology=new();
 public float torsoHitAngleAddition,LastHitTimestamp,LastHitEnergy,LastDirectionAttackedFrom,LookAngle,VisionOverload;
 public float HeadBaseAngle=-13,HeadCurrentAngle,TorsoCurrentAngle,RotationY;
 public Vector3 AimPoint;public HeadLookMode HeadMode=HeadLookMode.Weapon;
 public LimbPoseSnapshot Eye,Head,Neck,Torso,Abdomen;
 public ArmTargetSnapshot rightArm,leftArm;
 public WeaponAimerSlice WeaponAimer;public WeaponPoseSnapshot ActiveWeapon;
 public BodyPoseSlice(RuntimeWorld world){
  LimbPoseSnapshot node(string n)=>new(){transform=new GameObject(n,world).transform};
  Eye=node("Eye");Head=node("Head");Neck=node("Neck");Torso=node("Torso");Abdomen=node("Abdomen");
  rightArm=new(){ikTarget=new GameObject("Right arm target snapshot",world).transform};leftArm=new(){ikTarget=new GameObject("Left arm target snapshot",world).transform};
  WeaponAimer=new(){Body=this};
 }
 public void Step()=>UpdateUpperBodyTransforms(ActiveWeapon);
 public object Snapshot()=>new{head=HeadCurrentAngle,torso=TorsoCurrentAngle,sights=WeaponAimer.IsUsingSights,weaponAngle=ActiveWeapon.Angle,blocked=ActiveWeapon.IsBlocked,scope="original uninjured/unfired upper-body math; physiology/equipment reactions excluded"};
}
