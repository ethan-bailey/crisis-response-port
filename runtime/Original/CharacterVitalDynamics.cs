using UnityEngine;
namespace Crisis.Runtime.Character;

// Original Physiology.cs heart, breathing, shock and oxygen equations, with
// Unity Time and obstructed-mouth Physics.Raycast supplied by the host.
// This executes real physiological state, but original nervous-system/MuscleControl
// reactions and audio/cough effects require their separate source dependencies.
public sealed partial class OriginalPhysiologyConditions {
 public bool Alive{get;private set;}=true;
 public bool Conscious{get;private set;}=true;
 public bool RespirationDisabled{get;set;}
 public bool CirculationDisabled{get;set;}
 public bool RespirationBlocked{get;private set;}
 public float heartCapacity=70f,respirationToLungsFactor=.35f,heartOxygenTake=.02f,
              heartOxygenDeliver=1f,workOxygenFactor=.067f,
              respirationRateAdaptionRate=.1f,respirationDepthAdaptionRate=1f,fitness=1f,
              heartRateMax=190f,respirationRateMax=160f;
 public float HeartRate{get;private set;}=100f;
 public float RespirationRate{get;private set;}=15f;
 public float RespirationDepth{get;private set;}=.5f;
 public float Respiration{get;private set;}
 public float HeartBeatCycle{get;private set;}
 public float LungsCapacity{get;private set;}=1f;
 public float LungsEffectiveness{get;private set;}=1f;
 public float ShockRecovery{get;set;}=1f;
 public float WorkIntensity{get;private set;}
 public float BrainTissue{get;private set;}=1000f;
 public float BrainTraumaNormalized=>BrainTrauma/125f;
 public float LungsOxygenNormalized=>Mathf.Clamp(LungsOxygen/(LungsCapacity*LungsEffectiveness),0f,1f);
 public float RespirationRateFactor=>RespirationRate/respirationRateMax;
 public float BloodPressureAverage=>DiastolicPressure+BloodVolumeNormalized*.2f;
 float heartbeatTimestamp,respirationTimestamp,lastBrainOxygenTimestamp;
 float startingHeartCapacity=70f,lungsLeakage;
 bool skipHeartBeat,isInhaling;
 public bool NeedsUnconscious=>Conscious&&ShouldLoseConsciousness();
 public bool NeedsRagdoll=>Shock>=1000f||ShouldLoseFooting();
 public float HeartEffectiveness=>heartCapacity/startingHeartCapacity;
 public float HeartCapacity=>(startingHeartCapacity+fitness*.3f)*HeartEffectiveness;
 public void InitializeVitalClock(float now,float heartRandom,float breathingRandom){
  if(StartingBloodVolume<=0)throw new InvalidOperationException("Initialize original blood volume first");
  if(heartRandom<0||heartRandom>1||breathingRandom<0||breathingRandom>1)
   throw new ArgumentOutOfRangeException(nameof(heartRandom));
  startingHeartCapacity=heartCapacity;HeartRate=100f;RespirationRate=15f;RespirationDepth=.5f;
  LungsCapacity=1f;LungsEffectiveness=1f;
  heartbeatTimestamp=now+heartRandom;respirationTimestamp=now+breathingRandom;
  lastBrainOxygenTimestamp=now;Alive=true;Conscious=true;
 }
 public void AddLungsTrauma(float volume,float shock){
  RemoveLungsOxygen(shock*.0005f);
  lungsLeakage+=volume*.001f;
  LungsEffectiveness=Mathf.Clamp(LungsEffectiveness-volume*.001f,0f,1f);
 }
 public void SetUnconscious(float time){
  if(Conscious){Conscious=false;UnconsciousTimestamp=time;}
 }
 public float UnconsciousTimestamp{get;private set;}
 public void UpdateVitalDynamics(float dt,float now,float workIntensity,bool respirationBlocked,float heartRandom){
  if(StartingBloodVolume<=0)throw new InvalidOperationException("Missing original physiology initialization");
  if(!float.IsFinite(dt)||dt<=0||!float.IsFinite(now))throw new ArgumentOutOfRangeException(nameof(dt));
  if(heartRandom<0||heartRandom>1)throw new ArgumentOutOfRangeException(nameof(heartRandom));
  WorkIntensity=Mathf.Clamp01(workIntensity);
  RespirationBlocked=respirationBlocked;
  if(Alive){
   // Physiology.SetHeartRate source equations.
   if(CirculationDisabled)HeartRate=0f;
   else if(BloodOxygen<.85f||DiastolicPressure<.3f){
    float num=(1f-BloodOxygen)*100f*.0167f;
    HeartRate=Mathf.Clamp(HeartRate+num,100f,heartRateMax);
   }else{
    float num2=1f/WorkIntensity*Mathf.Clamp(fitness,.1f,1f)*.0083f;
    HeartRate=Mathf.Clamp(HeartRate-num2,100f,heartRateMax);
   }
   // Physiology.SetRespirationRate.
   if(RespirationDisabled)RespirationRate=0f;
   else{
    float num=Mathf.Min(BloodVolumeNormalized/.25f,1f);
    float num2=HeartCapacity*BloodPressureAverage*heartOxygenTake*HeartRate;
    float b=Mathf.Clamp(num2/(respirationToLungsFactor*RespirationDepth*60f),20f,respirationRateMax)*num;
    RespirationRate=Mathf.Lerp(RespirationRate,b,respirationRateAdaptionRate*dt);
   }
   float b2=.5f+(1f-Mathf.Max(LungsOxygenNormalized-.25f,0f)/.75f)*.5f;
   RespirationDepth=Mathf.Min(Mathf.Lerp(RespirationDepth,b2,respirationDepthAdaptionRate*dt),BloodVolumeNormalized);
   // Physiology.Respirate; the mouth ray hit is supplied by actual physics.
   if(!RespirationDisabled&&RespirationRate!=0f){
    float period=60f/RespirationRate;
    float num=RespirationDepth*(respirationBlocked?.1f:1f);
    bool inhaling=now-respirationTimestamp<period*.5f;
    float num2=period*.5f;
    if(inhaling){
     isInhaling=true;
     LungsOxygen=Mathf.Clamp(LungsOxygen+dt*respirationToLungsFactor*num,0f,
       LungsCapacity*LungsEffectiveness*BloodVolumeNormalized);
     Respiration=(now-respirationTimestamp)/(period*.5f)*num;
    }else{
     isInhaling=false;
     Respiration=(1f-(now-respirationTimestamp-num2)/num2)*num;
    }
    if(now-respirationTimestamp>=period*.5f+num2)respirationTimestamp=now;
    if(lungsLeakage>0f)LungsOxygen=Mathf.Max(0f,LungsOxygen-lungsLeakage*.001f*dt);
   }else{isInhaling=false;}
   // Physiology.SetBrainOxygen: source hypoxia damage branch without
   // unimplemented nerves/audio side effects.
   if(BloodOxygen>0f)lastBrainOxygenTimestamp=now;
   else if(now-lastBrainOxygenTimestamp>=10f){
    BrainTrauma+=dt*7.5f;
    BrainTissue=Mathf.Max(0f,BrainTissue-dt*7.5f*.5f);
   }
   Shock=Mathf.Lerp(Shock,0f,dt*.25f*ShockRecovery);
   if(Shock<.01f)Shock=0f;
   Concussion=Mathf.Clamp01(Concussion-dt*.025f);
  }
  // Physiology.UpdateHeart source circulation / oxygen transfer.
  if(CirculationDisabled){HeartBeatCycle=Mathf.Max(0f,HeartBeatCycle-dt);}
  else{
   float num=now-heartbeatTimestamp;
   float period=HeartRate!=0?60f/HeartRate:float.PositiveInfinity;
   float num2=num/(period*2f);
   if(!skipHeartBeat||num>=period){
    if(skipHeartBeat)skipHeartBeat=false;
    if(num2<=1f)HeartBeatCycle=.5f+Mathf.Sin(4f*num2*MathF.PI-MathF.PI/2f)*.5f;
    else if(num2<=2f)HeartBeatCycle=.5f+Mathf.Sin(1.3f*num2*MathF.PI+MathF.PI/5f)*.5f;
    else HeartBeatCycle=Mathf.Max(0f,HeartBeatCycle-dt);
    if(num>=period){
     if(heartRandom>HeartEffectiveness*Mathf.Pow(BloodOxygen,2f))skipHeartBeat=true;
     else{
      float num3=Mathf.Min(LungsOxygen,HeartCapacity*BloodPressure);
      LungsOxygen=Mathf.Max(LungsOxygen-num3*heartOxygenTake,0f);
      BloodOxygen=Mathf.Min(BloodOxygen+num3*heartOxygenDeliver,1f);
      BloodOxygen=Mathf.Min(BloodOxygen,BloodVolumeNormalized);
      heartbeatTimestamp=now;
     }
    }
   }
  }
  SetBloodPressure(HeartRate,heartRateMax,HeartEffectiveness,HeartBeatCycle);
  // Physiology.SetBloodOxygen: consume oxygen at original work rate.
  BloodOxygen=Mathf.Clamp(BloodOxygen-WorkIntensity*workOxygenFactor*dt,0f,1f);
  if(Conscious&&ShouldLoseConsciousness())SetUnconscious(now);
  else if(!Conscious&&ShouldRegainConsciousness())Conscious=true;
 }
}
