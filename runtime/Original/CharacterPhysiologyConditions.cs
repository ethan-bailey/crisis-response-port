using UnityEngine;
namespace Crisis.Runtime.Character;

// Executable physiology condition slice from original Physiology.cs lines
// 367-385. These are original thresholds, not invented health behavior.
// Full Physiology.Update, trauma propagation and limb injury dependencies
// must be integrated before claiming complete physiology.
public sealed partial class OriginalPhysiologyConditions {
 public float StartingBloodVolume {get;private set;}
 public float BloodVolume {get;private set;}
 public float LungsOxygen {get;private set;}=.75f;
 float bloodDropOxygen;
 // Exact source Awake() blood state, with original Random.Range sample supplied
 // by source initialization rather than introducing a new random generator.
 public void InitializeBloodVolume(float startingVolume) {
  if(startingVolume<4000f||startingVolume>5000f||!float.IsFinite(startingVolume))
   throw new ArgumentOutOfRangeException(nameof(startingVolume));
  StartingBloodVolume=BloodVolume=startingVolume;
  bloodDropOxygen=1f/StartingBloodVolume;
  BloodOxygen=1f;
  LungsOxygen=.75f;
 }
 public float BloodVolumeNormalized=>BloodVolume/StartingBloodVolume;
 public bool Exsanguinated=>BloodVolume<=0f;
 public float BloodPressure {get;private set;}
 public float DiastolicPressure {get;private set;}
 public void SetBloodPressure(float heartRate,float heartRateMax,float heartEffectiveness,float heartBeatCycle){
  float num=heartRate/heartRateMax*heartEffectiveness*0.3f;
  float num2=BloodVolumeNormalized*0.3f;
  DiastolicPressure=num+num2;
  BloodPressure=DiastolicPressure+heartBeatCycle*0.4f;
 }
 // Unchanged developer-authored Physiology.RemoveBlood() lines 501-505.
 public void RemoveBlood(float milliliters) {
  if(StartingBloodVolume==0f)throw new InvalidOperationException("Initialize physiology from original blood volume first");
  BloodVolume -= milliliters;
  BloodOxygen -= bloodDropOxygen * milliliters;
 }
 // Unchanged developer-authored Physiology.RemoveLungsOxygen lines 507-510.
 public void RemoveLungsOxygen(float normalizedPercent) {
  LungsOxygen=Mathf.Clamp(LungsOxygen-LungsOxygen*normalizedPercent,0f,1f);
 }
 public bool invulnerable;
 public float ShockNormalized=>Shock/1000f;
 public float Shock {get;set;}
 public float Concussion {get;set;}
 public float BloodOxygen {get;set;}=1f;
 public float BrainTrauma {get;set;}
 public float ShockResistance {get;set;}
 public float MaxConsciousBrainTrauma {get;}=3f; // Original field default.
 public bool ShouldLoseFooting() {
  return Shock>=2500f;
 }
 public bool ShouldLoseConsciousness() {
  bool flag=Concussion>=0.775f;
  bool flag2=BloodOxygen<=0.5f;
  bool flag3=BrainTrauma>=MaxConsciousBrainTrauma;
  return flag||flag2||flag3;
 }
 public bool ShouldRegainConsciousness() {
  bool flag=Concussion>=0.4f;
  bool flag2=BloodOxygen<=0.6f;
  bool flag3=BrainTrauma>=MaxConsciousBrainTrauma;
  return !flag&&!flag2&&!flag3;
 }
 public void AddShock(float shock) {
  Shock+=shock*(1f-ShockResistance);
 }
}

