using System.Text.Json;
using UnityEngine;
using Crisis.Runtime.Weapons;

namespace Crisis.Runtime.Character;

// Original VitalFunctions.HitTest and LimbPhysiology.InflictVitalTrauma source
// rules. Organ curves and probabilities are read from the genuine serialized
// Operator/Occupant prefab, never inferred from limb name or collider size.
// This is called after source ballistics has resolved a tissue interaction;
// no approximate ray-to-wound fallback is provided.
public sealed class OriginalVitalFunctions {
 public enum OrganType {None=0,Bone=1,BloodVessels=2,Lungs=3,Heart=4,Spine=5,Brain=6}
 public sealed class Vital {
  public string Name {get;}
  public OrganType Type {get;}
  public float Probability {get;}
  public AnimationCurve Factor {get;}
  public Vital(string name,OrganType type,float probability,AnimationCurve factor){
   Name=name;Type=type;Probability=probability;Factor=factor;
  }
 }
 readonly Limb limb;
 readonly OriginalPhysiologyConditions physiology;
 readonly float capsuleHeight,capsuleOffset;
 readonly List<Vital> vitals=new();
 public IReadOnlyList<Vital> Functions=>vitals;
 public OriginalVitalFunctions(Limb sourceLimb,OriginalPhysiologyConditions sourcePhysiology,
     JsonElement serializedVitalFunctions,float serializedCapsuleHeight,float serializedCapsuleCenterY){
  limb=sourceLimb??throw new ArgumentNullException(nameof(sourceLimb));
  physiology=sourcePhysiology??throw new ArgumentNullException(nameof(sourcePhysiology));
  if(limb.LimbPhysiology==null)
   throw new InvalidDataException("Original VitalFunctions requires a source LimbPhysiology");
  if(!float.IsFinite(serializedCapsuleHeight)||serializedCapsuleHeight<=0||
      !float.IsFinite(serializedCapsuleCenterY))
   throw new InvalidDataException("Original VitalFunctions CapsuleCollider is missing");
  capsuleHeight=serializedCapsuleHeight;
  capsuleOffset=capsuleHeight*.5f-serializedCapsuleCenterY;
  foreach(var value in serializedVitalFunctions.GetProperty("vitalFunctions").EnumerateArray()){
   int kind=value.GetProperty("type").GetInt32();
   if(!Enum.IsDefined(typeof(OrganType),kind))
    throw new InvalidDataException("Unrecognized original LimbVitalFunction type");
   var prob=value.GetProperty("probability").GetSingle();
   if(!float.IsFinite(prob)||prob<0f||prob>1f)
    throw new InvalidDataException("Invalid original organ hit probability");
   var curve=WeaponHost.Curve(value.GetProperty("factor"));
   vitals.Add(new Vital(value.GetProperty("name").GetString(),(OrganType)kind,prob,curve));
  }
 }
 // Exactly source VitalFunctions.HitTest, with ballistic volume supplied after
 // genuine projectile friction/interior calculations, not guessed from hits.
 public void HitTest(float displacedMaterialVolume,Vector3 entry,Vector3 exit,
                     float shock,Action<string> onHit=null){
  if(!float.IsFinite(displacedMaterialVolume)||displacedMaterialVolume<0f||
     !float.IsFinite(shock)||shock<0f)
   throw new ArgumentOutOfRangeException(nameof(displacedMaterialVolume));
  float num=Mathf.Abs(limb.transform.InverseTransformPoint(entry).y+capsuleOffset)/capsuleHeight;
  float num2=Mathf.Abs(limb.transform.InverseTransformPoint(exit).y+capsuleOffset)/capsuleHeight;
  float num3=(num+num2)*.5f;
  foreach(var vital in vitals){
   float value=UnityEngine.Random.value;
   if(!(value>vital.Probability)){
    float num4=vital.Factor.Evaluate(num3);
    if(num4>0f){
     InflictVitalTrauma(vital,displacedMaterialVolume*num4,shock*num4,num3);
     onHit?.Invoke(vital.Name);
    }
   }
  }
 }
 // Original LimbPhysiology.InflictVitalTrauma switch. Lack of an authored
 // BloodVessels effect is deliberate: that case is empty in source.
 void InflictVitalTrauma(Vital vital,float volume,float shock,float normalizedLocalY){
  switch(vital.Type){
   case OrganType.Brain:
    physiology.AddBrainTrauma(volume);
    break;
   case OrganType.Lungs:
    physiology.AddLungsTrauma(volume,shock);
    break;
   case OrganType.Heart:
    physiology.AddHeartTrauma(volume,shock,
      limb.Body.Torso?.LimbPhysiology ??
      throw new InvalidOperationException("Original heart hit requires source Torso physiology"),
      () => limb.Body.OnOriginalOrganUnconscious(),
      duration => limb.Body.SourceMuscleControl?.ActivateCramp(duration));
    break;
   case OrganType.Spine:
    physiology.SetParalyzed(duration => limb.Body.SourceMuscleControl?.ActivateCramp(duration));
    limb.Body.EnterSourceRagdoll();
    break;
   case OrganType.Bone:
    physiology.AddShock(shock);
    limb.SourceMuscle?.AddJointTrauma(volume,normalizedLocalY);
    if((!limb.Body.NoPlayerLimp||!limb.Body.IsPlayer) &&
       (limb.id==LimbId.RightUpperLeg||limb.id==LimbId.RightLowerLeg||
        limb.id==LimbId.RightFoot||limb.id==LimbId.LeftUpperLeg||
        limb.id==LimbId.LeftLowerLeg||limb.id==LimbId.LeftFoot))
     limb.Body.CrippledRatio=1f;
    break;
   case OrganType.BloodVessels:
   case OrganType.None:
    break;
  }
 }
}
