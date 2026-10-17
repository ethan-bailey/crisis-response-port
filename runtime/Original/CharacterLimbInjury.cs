using UnityEngine;
using Crisis.Runtime.Weapons;
namespace Crisis.Runtime.Character;

// Executable source-accurate injury arithmetic from recovered Wound.cs and
// LimbPhysiology.cs. Rendering hooks use the source WoundEmission event boundary;
// BloodStreamGenerator and actual visual spawning are separately tracked.
public sealed class OriginalWound {
 public readonly float Diameter,EmissionRadiusMeters;
 public readonly Vector3 LocalPosition,LocalDirection;
 public readonly bool UseParticles;
 public readonly float Multiplier;
 public float available;
 public OriginalWound(float diameterMillimeters,Vector3 localPosition,Vector3 localDirection,
      float emissionRadiusMeters,float initialAvailable,bool useParticles=false,float multiplier=1f){
  if(diameterMillimeters<=0||!float.IsFinite(diameterMillimeters))throw new ArgumentOutOfRangeException(nameof(diameterMillimeters));
  Diameter=diameterMillimeters;LocalPosition=localPosition;LocalDirection=localDirection;
  EmissionRadiusMeters=emissionRadiusMeters;available=initialAvailable;
  UseParticles=useParticles;Multiplier=multiplier;
 }
 // Source Wound.Update(float pressure), with Time.deltaTime and particle emission
 // supplied explicitly to permit deterministic WASM/PhysX stepping.
 public float Update(float pressure,float deltaTime,float woundCavityVolume,
       Action<int,Vector3,Vector3,float,float> woundEmission=null) {
  float num=pressure*Diameter+available;
  float num2=num*deltaTime;
  // Original per-wound velocity law; visuals remain on the caller's renderer.
  float num3=pressure*(10f/Diameter)*Multiplier;
  if(woundCavityVolume>0f) {
   if(UseParticles){
    if(woundEmission==null)throw new InvalidOperationException("Original WoundEmission renderer must be bound for particle wounds");
    int num4=Mathf.FloorToInt(num2);
    woundEmission(num4,LocalPosition,LocalDirection,num3,EmissionRadiusMeters);
    if(num4==0)num2=0f;
   }
  }
  available=num-num2;
  return num2;
 }
}
public sealed class OriginalLimbPhysiology {
 readonly OriginalPhysiologyConditions physiology;
 readonly List<OriginalWound> wounds=new();
 public OriginalLimbPhysiology(OriginalPhysiologyConditions physiology) =>
  this.physiology=physiology??throw new ArgumentNullException(nameof(physiology));
 public float bloodPressureModifier=1f,shockModifier=1f,maxHemorrhaging=1f,maxWoundCavityVolume=100f;
 public IReadOnlyList<OriginalWound> Wounds=>wounds;
 public float InternalHemorrhaging{get;private set;}
 public float WoundCavityVolume{get;private set;}
 public float DamagedTissueVolume{get;private set;}
 public float InternalBloodVolume{get;private set;}
 public bool Wounded=>WoundCavityVolume>0f;
 public float LocalPressure=>physiology.BloodPressure*bloodPressureModifier;
 public float HemorrhagingNormalized=>Mathf.Clamp01(WoundCavityVolume/maxWoundCavityVolume);
 public float Hemorrhaging=>InternalHemorrhaging*HemorrhagingNormalized*(physiology.Exsanguinated?0f:1f);
 public float HemorrhagingScore=>WoundCavityVolume*bloodPressureModifier;
 public void CreateWound(OriginalWound wound){wounds.Add(wound??throw new ArgumentNullException(nameof(wound)));}
 // Original LimbPhysiology.CreateWound: retain authored limb-space wound
 // position, randomized direction Z and source Wound.available state.
 // The original GPU particle stream and dynamic decal dependencies remain
 // separate and explicitly reject unsupported enabled settings.
 public OriginalWound CreateWoundFromSourceProjectile(
    Limb limb,float diameterMillimeters,float volume,float emissionRadiusMeters,
    Vector3 worldPoint,Vector3 worldDirection,
    bool useBloodParticles=false,bool useWoundStreams=false,float woundPowerMultiplier=1f){
  if(limb==null)throw new ArgumentNullException(nameof(limb));
  if(!ReferenceEquals(limb.LimbPhysiology,this))
   throw new InvalidOperationException("Original source projectile hit a mismatched limb physiology");
  if(useWoundStreams)
   throw new NotSupportedException("Original BloodStreamGenerator rendering is not bound");
  if(useBloodParticles)
   throw new NotSupportedException("Original Limb.WoundEmission particle renderer is not bound");
  if(diameterMillimeters<=0||!float.IsFinite(diameterMillimeters)||
    !float.IsFinite(volume)||volume<0f||!float.IsFinite(woundPowerMultiplier)||
    woundPowerMultiplier<=0f)
   throw new ArgumentOutOfRangeException(nameof(diameterMillimeters));
  Vector3 vector=limb.transform.InverseTransformPoint(worldPoint);
  Vector3 localDirection=limb.transform.InverseTransformDirection(worldDirection);
  if(Mathf.Abs(localDirection.z)<0.5f){
   localDirection.z=UnityEngine.Random.value*
       (!(UnityEngine.Random.value>0.5f)?-1f:1f);
  }
  var wound=new OriginalWound(diameterMillimeters,
    new Vector3(vector.x,vector.y,0f),localDirection,
    emissionRadiusMeters,UnityEngine.Random.Range(0f,1f),
    useParticles:useBloodParticles,multiplier:woundPowerMultiplier);
  CreateWound(wound);
  return wound;
 }
 // Original LimbPhysiology.AddTrauma method, no substitute injury thresholds.
 public void AddTrauma(float volume) {
  float num=Mathf.Min(volume,Mathf.Max(maxWoundCavityVolume-WoundCavityVolume,0f));
  WoundCavityVolume+=num;
  DamagedTissueVolume+=num;
 }
 public void AddShock(float shock) {physiology.AddShock(shock*shockModifier);}
 // Original LimbPhysiology.StopHemorrhage.
 public void StopHemorrhage(){
  float woundCavityVolume=(InternalBloodVolume=0f);
  WoundCavityVolume=woundCavityVolume;
 }
 // Original UpdateHemorrhaging: bleeding into internal cavity, then wound flow.
 public void UpdateHemorrhaging(float deltaTime,Action<int,Vector3,Vector3,float,float> emit=null){
  if(InternalBloodVolume<WoundCavityVolume){
   InternalHemorrhaging=WoundCavityVolume*LocalPressure*deltaTime;
   InternalBloodVolume+=InternalHemorrhaging;
   physiology.RemoveBlood(InternalHemorrhaging);
  } else {
   InternalHemorrhaging=0f;
  }
  float num=(wounds.Count!=0)?(2f/(float)wounds.Count):0f;
  float num2=LocalPressure*num;
  float num3=0f;
  foreach(OriginalWound wound in wounds)
    num3+=wound.Update(num2,deltaTime,WoundCavityVolume,emit);
  InternalBloodVolume-=num3;
 }
}
